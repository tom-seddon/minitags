using EnvDTE;
using EnvDTE80;
using QuickFind;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using VSAddInLib;

namespace minitags
{
    //////////////////////////////////////////////////////////////////////////

    class BrowseMinitagsCommand : AddInCommand
    {
        //////////////////////////////////////////////////////////////////////////

        public BrowseMinitagsCommand(DTE2 dte2)
            :
            base(dte2, "BrowseMinitags", "Browse minitags", "Browse minitags in current file")
        {
        }

        //////////////////////////////////////////////////////////////////////////

        public override void Exec(ref object varIn, ref object varOut)
        {
            MinitagsConfig config = MinitagsConfig.Load();

            Stream stream;
            if (config.printToOutputWindow)
                stream = new AddInOutputWindowStream(_dte2, "minitags");
            else
                stream = Stream.Null;

            List<object> minitags;
            using (StreamWriter writer = new StreamWriter(stream))
            {
                minitags = Minitags.GetMinitagsFromDocument(_dte2,
                    _dte2.ActiveDocument, writer);
            }

            if (minitags == null)
            {
                _dte2.StatusBar.Text = "minitags: nothing for this type of file.";
            }
            else if (minitags.Count == 0)
            {
                _dte2.StatusBar.Text = "minitags: no tags in this file.";
            }
            else
            {
                QuickFindDialog qfd = new QuickFindDialog(minitags, new MinitagItemHandler(),
                    config.browseMinitagsQuickFindDialogSettings);

                DialogResult dr = qfd.ShowDialog();

                // TODO maybe better if QuickFindDialog stores settings by
                // reference??
                config.browseMinitagsQuickFindDialogSettings = qfd.Settings;
                config.Save();

                if (dr == DialogResult.OK && qfd.SelectedItem != null)
                {
                    Minitag minitag = qfd.SelectedItem as Minitag;

                    Visit(minitag);
                }
            }

            config.Save();
        }

        //////////////////////////////////////////////////////////////////////////

        public override vsCommandStatus GetCommandStatus()
        {
            vsCommandStatus status = vsCommandStatus.vsCommandStatusSupported;

            //if (_dte2.ActiveDocument != null)
            status |= vsCommandStatus.vsCommandStatusEnabled;

            return status;
        }

        //////////////////////////////////////////////////////////////////////////

        private void Visit(Minitag minitag)
        {
            TextDocument td = _dte2.ActiveDocument.Object("TextDocument") as TextDocument;
            if (td == null)
                return;//erm...

            td.Selection.MoveToPoint(minitag.Range.StartPoint, false);
            td.Selection.MoveToPoint(minitag.Range.EndPoint, true);

            td.Selection.ActivePoint.TryToShow(vsPaneShowHow.vsPaneShowCentered,
                td.Selection.ActivePoint);
        }

        //////////////////////////////////////////////////////////////////////////
    }
}
