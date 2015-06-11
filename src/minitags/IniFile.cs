using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace minitags
{
    /// <summary>
    /// ini file reader that can read from list of strings, which
    /// unfortunately GetPrivateProfileString etc. won't.
    /// 
    /// also slightly more convenient api...
    /// </summary>
    static class IniFile
    {
        public class Exception:
            System.Exception
        {
            public Exception(string msg, params object[] args)
                : base(String.Format(msg, args))
            {
            }
        }

        public class Section
        {
            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public List<KeyValuePair<string, string>> Entries
            {
                get
                {
                    return _entries;
                }
            }

            public bool TryAddRawEntry(string rawText)
            {
                int equalsIdx = rawText.IndexOf('=');
                if (equalsIdx < 0)
                    return false;

                string key = rawText.Substring(0, equalsIdx).Trim();
                string value = rawText.Substring(equalsIdx + 1);

                AddEntry(key, value);

                return true;
            }

            public void AddEntry(string key, string value)
            {
                _entries.Add(new KeyValuePair<string, string>(key, value));
            }

            public Section(string name)
            {
                _name = name;
                _entries = new List<KeyValuePair<string, string>>();
            }

            private String _name;
            private List<KeyValuePair<string, string>> _entries;
        }

        public static List<Section> LoadIniFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);

            List<Section> sections = new List<Section>();

            Section section = null;

            for (int lineIdx = 0; lineIdx < lines.Length; ++lineIdx)
            {
                string line = lines[lineIdx].TrimStart();

                // Ignore blank lines and comments
                if (line.Length == 0 || line[0] == ';')
                    continue;

                //
                if (line[0] == '[')
                {
                    // Section
                    line = line.TrimEnd();
                    if (line[line.Length - 1] != ']')
                    {
                        throw new Exception("{0}({1}): invalid section line \"{2}\"",
                            filename, 1 + lineIdx, line);
                    }

                    section = new Section(line.Substring(1, line.Length - 2));

                    sections.Add(section);
                }
                else
                {
                    // Key/value
                    if (section == null)
                    {
                        throw new Exception("{0}({1}): unexpected \"{2}\" outside section.",
                            filename, 1 + lineIdx, line);
                    }

                    if (!section.TryAddRawEntry(line))
                        throw new Exception("{0}({1}): syntax error.", filename, 1 + lineIdx);
                }
            }

            return sections;
        }
    }
}
