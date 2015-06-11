using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using VSAddInLib;

namespace minitags
{
    static class Minitags
    {
        //////////////////////////////////////////////////////////////////////////

        static readonly string _marker = "minitags::::";

        //////////////////////////////////////////////////////////////////////////

        const int _numLinesToSearch = 10;

        //////////////////////////////////////////////////////////////////////////

        private static readonly string _iniFileName = "minitags.ini";

        //////////////////////////////////////////////////////////////////////////

        [DllImport("shlwapi.dll")]
        private static extern bool PathMatchSpec(string pszFileParam, string pszSpec);

        //////////////////////////////////////////////////////////////////////////

        private static FileInfo TryGetFileInfo(string file)
        {
            try
            {
                return new FileInfo(file);
            }
            catch (ArgumentException)
            {
                // the name is probably bad.
            }
            catch (UnauthorizedAccessException)
            {
                // file isn't accessible
            }
            catch (PathTooLongException)
            {
                // path is too long
            }
            catch (NotSupportedException)
            {
                // name has a : in it or something
            }

            return null;
        }

        //////////////////////////////////////////////////////////////////////////

        private static string TryGetFolderName(string fullName)
        {
            if (fullName != "")
            {
                try
                {
                    return Path.GetDirectoryName(fullName);
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        //////////////////////////////////////////////////////////////////////////

        private static void GetSectionsFromIniFile(string folderName,
            List<IniFile.Section> allSections, Dictionary<string, bool> lcFileNamesSeen,
            StreamWriter output)
        {
            if (folderName == null)
                return;

            FileInfo fileInfo = TryGetFileInfo(Path.Combine(folderName, _iniFileName));
            if (fileInfo == null)
                return;

            string lcFileName = fileInfo.FullName.ToLower();
            if (lcFileNamesSeen.ContainsKey(lcFileName))
                return;

            lcFileNamesSeen[lcFileName] = true;

            try
            {
                allSections.AddRange(IniFile.LoadIniFile(fileInfo.FullName));
            }
            catch (FileNotFoundException)
            {
                // just ignore these!
            }
            catch (IOException e)
            {
                output.WriteLine("{0}: {1}\n", fileInfo.FullName, e.Message);
            }
            catch (IniFile.Exception e)
            {
                output.WriteLine("{0}\n", e.Message);
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private static bool IsMarkupLine(TextDocument td, EditPoint lineStart)
        {
            EditPoint lineEnd = td.CreateEditPoint(lineStart);
            lineEnd.EndOfLine();

            string lineText = lineStart.GetText(lineEnd);

            if (lineText.IndexOf(_marker) >= 0)
                return true;
            else
                return false;
        }

        //////////////////////////////////////////////////////////////////////////

        // tries to find markup in a document. starts at 'startPoint', moving
        // down by 'lineDelta' (perhaps -ve) lines each time. if any markup
        // found in the first '_numLinesToSearch' lines, adds it to
        // 'markupLines' (*in order searched*) and sets 'firstLine' to first
        // line encountered (*in order searched*) with markup on it.
        private static bool TryGetMarkupFromDocument(TextPoint startPoint,
            int lineDelta, out List<string> markupLines, out int firstLine)
        {
            markupLines = new List<string>();
            firstLine = 0;//flibble

            bool anyMarkupFound = false;

            EditPoint lineStart = startPoint.CreateEditPoint();
            int numLinesSearched = 0;

            for (; ; )//silly mucky loop :(
            {
                EditPoint lineEnd = lineStart.CreateEditPoint();
                lineEnd.EndOfLine();

                string lineText = lineStart.GetText(lineEnd);

                int markerIdx = lineText.IndexOf(_marker);
                if (markerIdx >= 0)
                {
                    if (!anyMarkupFound)
                        firstLine = lineStart.Line;

                    anyMarkupFound = true;

                    markupLines.Add(lineText.Substring(markerIdx + _marker.Length).Trim());
                }
                else
                {
                    if (anyMarkupFound)
                    {
                        // stop; first non-markup line
                        break;
                    }
                }

                lineStart.LineDown(lineDelta);
                if (lineStart.AtStartOfDocument || lineStart.AtEndOfDocument)
                {
                    // stop; document boundary reached
                    break;
                }

                ++numLinesSearched;
                if (numLinesSearched >= _numLinesToSearch)
                {
                    // stop; have searched N lines now
                    break;
                }
            }

            return anyMarkupFound;
        }

        //////////////////////////////////////////////////////////////////////////

        // tries to find markup in a document. tries from the top first, then
        // from the bottom. if any is found, returns true, fills in markupLines
        // with the markup found (in same order as in document), and sets
        // firstLine to the line number containing markupLines[0].
        private static bool GetMarkupFromDocument(TextDocument td,
            out List<string> markupLines, out int firstLine)
        {
            TextPoint docStartPoint = td.StartPoint;//MNT
            if (TryGetMarkupFromDocument(docStartPoint, 1, out markupLines, out firstLine))
                return true;

            TextPoint docEndPoint = td.EndPoint;//MNT
            if (TryGetMarkupFromDocument(docEndPoint, -1, out markupLines, out firstLine))
            {
                firstLine -= markupLines.Count - 1;//meh
                markupLines.Reverse();//meh
                return true;
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////

        private static void GetSectionsFromDocument(Document doc,
            List<IniFile.Section> allSections, StreamWriter output)
        {
            TextDocument td = doc.Object("TextDocument") as TextDocument;
            if (td == null)
                return;

            List<string> markupLines;
            int firstMarkupLine;
            if (GetMarkupFromDocument(td, out markupLines, out firstMarkupLine))
            {
                IniFile.Section section = new IniFile.Section(null);

                for (int i = 0; i < markupLines.Count; ++i)
                {
                    if (!section.TryAddRawEntry(markupLines[i]))
                    {
                        output.WriteLine("{0}({1}): syntax error.",
                            doc.FullName, firstMarkupLine + i);
                    }
                }

                if (section.Entries.Count > 0)
                    allSections.Add(section);
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private static List<IniFile.Section> GetMinitagIniSections(DTE2 dte2,
            Document document, StreamWriter output)
        {
            Dictionary<string, bool> lcFileNamesSeen = new Dictionary<string, bool>();
            List<IniFile.Section> allSections = new List<IniFile.Section>();

            // grab from document
            GetSectionsFromDocument(document, allSections, output);

            // look in My Documents
            GetSectionsFromIniFile(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                allSections, lcFileNamesSeen, output);

            // look in document folder
            GetSectionsFromIniFile(TryGetFolderName(document.FullName), allSections,
                lcFileNamesSeen, output);

            if (document.ProjectItem != null)
            {
                if (document.ProjectItem.ContainingProject != null)
                {
                    // look in project folder
                    GetSectionsFromIniFile(
                        TryGetFolderName(document.ProjectItem.ContainingProject.FullName),
                        allSections, lcFileNamesSeen, output);
                }
            }

            if (dte2.Solution != null)
            {
                // look in solution folder
                GetSectionsFromIniFile(TryGetFolderName(dte2.Solution.FullName),
                    allSections, lcFileNamesSeen, output);
            }

            return allSections;
        }

        //////////////////////////////////////////////////////////////////////////

        private static bool Relevant(IniFile.Section section, Document document)
        {
            if (section.Name == null)
            {
                // this section came from this document, so it must be relevant.
                return true;
            }

            string[] specs = section.Name.Split(',');

            foreach (string spec in specs)
            {
                if (PathMatchSpec(document.FullName, spec))
                    return true;
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////

        private static bool TrySetMinitagTypeAttr(MinitagType type, string attrName,
            string value)
        {
            switch (attrName.ToLower())
            {
            case "regexp":
                type.Regexp = value;
                return true;

            case "name":
                type.Name = value;
                return true;

            case "text":
                type.Text = value;
                return true;

            case "greedy":
                type.Greedy = value != "0";
                return true;
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////

        private static List<MinitagType> GetRelevantMinitagTypesFromIniSections(
            List<IniFile.Section> sections, Document document, StreamWriter output)
        {
            List<MinitagType> types = new List<MinitagType>();
            Dictionary<string, MinitagType> typesByName =
                new Dictionary<string, MinitagType>();

            foreach (IniFile.Section section in sections)
            {
                if (!Relevant(section, document))
                    continue;

                bool final = false;

                List<MinitagType> localTypes = new List<MinitagType>();
                Dictionary<string, MinitagType> localTypesByName =
                    new Dictionary<string, MinitagType>();

                foreach (KeyValuePair<string, string> entry in section.Entries)
                {
                    if (entry.Key.ToLower() == "final")
                    {
                        if (entry.Value != "0")
                            final = true;
                    }
                    else
                    {
                        int dotIdx = entry.Key.IndexOf('.');
                        if (dotIdx >= 0)
                        {
                            string typeName = entry.Key.Substring(0, dotIdx);

                            if (typesByName.ContainsKey(typeName))
                            {
                                // already defined in an earlier file
                            }
                            else
                            {
                                string attrName = entry.Key.Substring(dotIdx + 1);

                                MinitagType type;
                                if (!localTypesByName.TryGetValue(typeName, out type))
                                {
                                    type = new MinitagType(typeName);

                                    localTypes.Add(type);
                                    localTypesByName[type.TypeName] = type;
                                }

                                TrySetMinitagTypeAttr(type, attrName, entry.Value);
                            }
                        }
                    }
                }

                foreach (MinitagType localType in localTypes)
                {
                    types.Add(localType);
                    typesByName[localType.TypeName] = localType;
                }

                if (final)
                    break;
            }

            return types;
        }

        //////////////////////////////////////////////////////////////////////////

        private static void GetMinitagExpansion(string text, TextRanges groups,
            out string expansion, out TextRange range)
        {
            if (text == null)
            {
                if (groups.Count >= 2)
                    range = groups.Item(2);// \1
                else
                    range = groups.Item(1);// \0

                expansion = range.StartPoint.GetText(range.EndPoint);
            }
            else
            {
                StringBuilder expansionBuilder = new StringBuilder();

                range = null;
                int startIdx = 0;
                while (startIdx < text.Length)
                {
                    int endIdx = text.IndexOf('\\', startIdx);
                    if (endIdx < 0)
                        endIdx = text.Length;

                    expansionBuilder.Append(text.Substring(startIdx, endIdx - startIdx));

                    if (endIdx < text.Length)
                    {
                        bool expanded = false;

                        if (endIdx + 1 < text.Length)
                        {
                            char c = text[endIdx + 1];
                            if (c >= '1' && c <= '9')
                            {
                                // \2 is actually \1... thanks, 1-based
                                // indexing.
                                TextRange groupRange = groups.Item(1 + (c - '0'));
                                string groupText = groupRange.StartPoint.GetText(
                                    groupRange.EndPoint);

                                expansionBuilder.Append(groupText);

                                endIdx += 2;//skip \ and number

                                expanded = true;

                                // if there's no range been seen yet, set it to
                                // this one. then if only one range is used (as
                                // is likely), it will highlight that part of
                                // the expansion that is present in the original
                                // document.
                                //
                                // otherwise, set it to \1, not really much else
                                // to be done...
                                if (range == null)
                                    range = groupRange;
                                else
                                    range = groups.Item(1);
                            }
                        }

                        if (!expanded)
                        {
                            expansionBuilder.Append('\\');

                            ++endIdx;
                        }

                        startIdx = endIdx;
                    }
                }

                expansion = expansionBuilder.ToString();
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private static List<object> GetMinitagsFromDocument(Document document,
            List<MinitagType> allTypes, StreamWriter output)
        {
            TextDocument td = document.Object("TextDocument") as TextDocument;
            if (td == null)
                return null;

            List<object> minitags = new List<object>();

            EditPoint docEndPoint = td.CreateEditPoint(null);
            docEndPoint.EndOfDocument();

            bool[] lineUsed = new bool[docEndPoint.Line];

            foreach (MinitagType type in allTypes)
            {
                EditPoint startPoint = td.CreateEditPoint(null);

                TextRanges groups = null;
                EditPoint endPoint = null;
                int flags = (int)(vsFindOptions.vsFindOptionsRegularExpression);

                while (startPoint.FindPattern(type.Regexp, flags, ref endPoint,
                    ref groups))
                {
                    bool addTag = true;
                    for (int matchLine = startPoint.Line; matchLine <= endPoint.Line;
                        ++matchLine)
                    {
                        if (lineUsed[matchLine - 1])
                        {
                            addTag = false;
                            break;
                        }
                    }

                    if (addTag)
                    {
                        string expansion;
                        TextRange range;
                        GetMinitagExpansion(type.Text, groups, out expansion, out range);

                        Minitag minitag = new Minitag(type, expansion, range);
                        minitags.Add(minitag);

                        if (type.Greedy)
                        {
                            for (int matchLine = startPoint.Line; matchLine <= endPoint.Line;
                                ++matchLine)
                            {
                                lineUsed[matchLine - 1] = true;
                            }
                        }
                    }

                    startPoint = endPoint;
                }
            }

            return minitags;
        }

        //////////////////////////////////////////////////////////////////////////

        public static List<object> GetMinitagsFromDocument(DTE2 dte2, Document document,
            StreamWriter output)
        {
            List<IniFile.Section> allSections = GetMinitagIniSections(dte2, document,
                output);

            List<MinitagType> allTypes = GetRelevantMinitagTypesFromIniSections(allSections,
                document, output);

            if (allTypes.Count == 0)
                return null;

            List<object> minitags = GetMinitagsFromDocument(dte2.ActiveDocument, allTypes,
                output);

            return minitags;
        }

        //////////////////////////////////////////////////////////////////////////
    }
}
