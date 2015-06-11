using QuickFind;
using System;
using System.Collections.Generic;
using System.Text;
using VSAddInLib;

namespace minitags
{
    public class MinitagsConfig
    {
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// QuickFindDialog settings for the BrowseMinitags command.
        /// </summary>
        public QuickFindDialogSettings browseMinitagsQuickFindDialogSettings = null;

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// if set, print stuff to output window as it goes (otherwise, entirely
        /// silent, even if error)
        /// </summary>
        public bool printToOutputWindow = true;

        //////////////////////////////////////////////////////////////////////////

        public static MinitagsConfig Load()
        {
            MinitagsConfig config;
            AddInConfig.LoadConfig("minitags", out config);

            return config;
        }

        //////////////////////////////////////////////////////////////////////////

        public void Save()
        {
            AddInConfig.SaveConfig("minitags", this);
        }

        //////////////////////////////////////////////////////////////////////////
    }
}
