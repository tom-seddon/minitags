using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Text;

namespace VSAddInLib
{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Base class for add-in commands. Use in conjunction with the
    /// AddInCommandRegistry. It's not much use without.
    /// </summary>
    public abstract class AddInCommand
    {
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Actual name of the command, sans "blahblah.Connect" prefix.
        /// </summary>
        public readonly string Name;

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Text for the menu.
        /// </summary>
        public readonly string Text;

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Text for the tool tip.
        /// </summary>
        public readonly string ToolTipText;

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The dte2 for any adding-in needs.
        /// </summary>
        protected readonly DTE2 _dte2;

        //////////////////////////////////////////////////////////////////////////

        // TODO property
        public Command Command;

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create AddInCommand.
        /// </summary>
        /// <param name="dte2">dte2</param>
        /// <param name="name">name of command</param>
        /// <param name="text">text for UI</param>
        /// <param name="toolTipText">tool tip text</param>
        public AddInCommand(DTE2 dte2, string name, string text, string toolTipText)
        {
            this.Name = name;
            this.Text = text;
            this.ToolTipText = toolTipText;
            _dte2 = dte2;
            Command = null;
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the command is to be executed.
        /// </summary>
        /// <param name="varIn">passed in from Connect.Exec</param>
        /// <param name="varOut">passed in from Connect.Exec</param>
        public abstract void Exec(ref object varIn, ref object varOut);

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the command's status is queried.
        /// </summary>
        /// <returns>appropriate vsCommandStatus</returns>
        public virtual vsCommandStatus GetCommandStatus()
        {
            return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
        }

        //////////////////////////////////////////////////////////////////////////
    }
}
