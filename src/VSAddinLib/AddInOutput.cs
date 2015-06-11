using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Text;

namespace VSAddInLib
{
    /// <summary>
    /// AddInOutput handles output to a VS output window pane with a particular
    /// name, creating that pane if it doesn't exist already.
    /// </summary>
    public class AddInOutput
    {
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create new AddInOutput that, if printed to, will print to an output
        /// window pane with the given name.
        /// </summary>
        /// <param name="dte2">dte2</param>
        /// <param name="paneName">name of the pane to print to</param>
        public AddInOutput(DTE2 dte2, string paneName)
        {
            _paneName = paneName;
            _pane = null;
            _dte2 = dte2;
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Print to the pane.
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">format args</param>
        public void Print(string format, params object[] args)
        {
            if (_pane == null)
                _pane = CreatePane(_paneName);

            _pane.OutputString(string.Format(format, args));
        }

        //////////////////////////////////////////////////////////////////////////

        private OutputWindowPane CreatePane(string name)
        {
            foreach (OutputWindowPane pane in _dte2.ToolWindows.OutputWindow.OutputWindowPanes)
            {
                if (pane.Name == name)
                    return pane;
            }

            return _dte2.ToolWindows.OutputWindow.OutputWindowPanes.Add(name);
        }

        //////////////////////////////////////////////////////////////////////////

        private readonly string _paneName;

        //////////////////////////////////////////////////////////////////////////

        private OutputWindowPane _pane;

        //////////////////////////////////////////////////////////////////////////

        private readonly DTE2 _dte2;

        //////////////////////////////////////////////////////////////////////////
    }
}
