using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace VSAddInLib
{
    /// <summary>
    /// An AddInCommandRegistry manages the list of add-in commands.
    /// 
    /// The registry starts out containing only a list of command creator
    /// delegates. When OnConnection is called, the mapping of VS command name
    /// to EnvDTE.Command is filled in. This mapping is then used to service
    /// Exec and QueryStatus requests from the IDE.
    /// 
    /// TODO this class could be a lot simpler now that the AddInConnect class
    /// exists.
    /// </summary>
    public class AddInCommandRegistry
    {
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Delegate that when called creates a new instance of an
        /// AddInCommand-derived type.
        /// </summary>
        /// <param name="dte2">dte2</param>
        /// <returns></returns>
        public delegate AddInCommand AddInCommandCreator(DTE2 dte2);

        //////////////////////////////////////////////////////////////////////////

        public AddInCommandRegistry(string prefix) : this(prefix, null) { }

        /// <summary>
        /// Create new registry, with the common command prefix
        /// ("blahblah.Connect") and an array of delegates that will create all
        /// the types of command.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="creators"></param>
        public AddInCommandRegistry(string prefix, AddInCommandCreator[] creators)
        {
            _commands = null;

            if (creators == null)
                _creators = null;
            else
                _creators = new List<AddInCommandCreator>(creators);

            Prefix = prefix;
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates the name->EnvDTE.Command mapping. Call from OnConnection
        /// each time.
        /// </summary>
        /// <param name="dte2"></param>
        public void OnConnection(DTE2 dte2)
        {
            _commands = new Dictionary<string, AddInCommand>();

            if (_creators != null)
            {
                foreach (AddInCommandCreator creator in _creators)
                {
                    AddInCommand command = creator(dte2);

                    _commands[Prefix + "." + command.Name] = command;
                }
            }
        }

        public void AddCommand(AddInCommand command)
        {
            _commands[Prefix + "." + command.Name] = command;
        }

        //////////////////////////////////////////////////////////////////////////

        public void OnConnection2(DTE2 dte2)
        {
            foreach (KeyValuePair<string, AddInCommand> kvpCommand in _commands)
                kvpCommand.Value.Command = dte2.Commands.Item(kvpCommand.Key, 0);
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds the commands in the registry to VS and creates UI for each one.
        /// Call from OnConnection in the ext_ConnectMode.ext_cm_UISetup case.
        /// </summary>
        /// <param name="dte2"></param>
        /// <param name="addInInstance"></param>
        /// <param name="contextUIGUIDs"></param>
        /// <param name="toolsPopup"></param>
        public void AddNamedCommands2(DTE2 dte2, AddIn addInInstance,
            ref object[] contextUIGUIDs, CommandBarPopup toolsPopup)
        {
            Commands2 commands2 = dte2.Commands as Commands2;

            //             MessageBox.Show(string.Format("AICR: Adding {0} commands", _commands.Count));

            foreach (KeyValuePair<string, AddInCommand> kvpCommand in _commands)
            {
                try
                {
                    //                     MessageBox.Show(string.Format("AICR: Adding \"{0}\"\nText=\"{1}\nToolTipText={2}\n",
                    //                         kvpCommand.Key, kvpCommand.Value.Text, kvpCommand.Value.ToolTipText));

                    vsCommandStatus initialStatus = vsCommandStatus.vsCommandStatusEnabled |
                        vsCommandStatus.vsCommandStatusSupported;
                    vsCommandStyle commandStyle = vsCommandStyle.vsCommandStylePictAndText;

                    Command c = commands2.AddNamedCommand2(
                        addInInstance,
                        kvpCommand.Value.Name,
                        kvpCommand.Value.Text,
                        kvpCommand.Value.ToolTipText,
                        true,//MSOButton (??)
                        59,//smiley face
                        ref contextUIGUIDs,
                        (int)initialStatus,
                        (int)commandStyle,
                        vsCommandControlType.vsCommandControlTypeButton);

                    //                     MessageBox.Show(string.Format("AICR: Added \"{0}\"\n", c.Name));

                    if (c != null && toolsPopup != null)
                        c.AddControl(toolsPopup.CommandBar, 1);

                }
                catch (System.ArgumentException)
                {
                    // oh well...

                    //                     string x = "";
                    // 
                    //                     for (Exception e = argExc; e != null; e = e.InnerException)
                    //                         x += e.Message;
                    // 
                    //                     x += "\n\n" + new StackTrace().ToString();
                    // 
                    //                     MessageBox.Show(x);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Query status of a command. Forwarded from the Connect.QueryStatus
        /// method.
        /// </summary>
        /// <param name="commandName">name of command</param>
        /// <param name="status">var to fill in with command status</param>
        /// <returns>true if status was filled in</returns>
        public bool TryQueryStatus(string commandName, ref vsCommandStatus status)
        {
            if (_commands != null)//lazy superstition
            {
                AddInCommand command;
                if (_commands.TryGetValue(commandName, out command))
                {
                    status = command.GetCommandStatus();
                    return true;
                }
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Execute command. Forwarded from the Connect.Exec method.
        /// </summary>
        /// <param name="commandName">name of command</param>
        /// <param name="varIn">varIn from Connect.Exec</param>
        /// <param name="varOut">varOut from Connect.Exec</param>
        /// <param name="handled">handled from Connect.Exe</param>
        /// <returns>true if the ref and out parameters were filled in</returns>
        public bool TryExecCommand(string commandName, ref object varIn, ref object varOut, ref bool handled)
        {
            if (_commands != null)//lazy superstition
            {
                AddInCommand command;
                if (_commands.TryGetValue(commandName, out command))
                {
                    command.Exec(ref varIn, ref varOut);
                    handled = true;
                    return true;
                }
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////

        public IEnumerable<AddInCommand> Commands
        {
            get
            {
                return _commands.Values;
            }
        }

        //////////////////////////////////////////////////////////////////////////

        public readonly string Prefix;

        //////////////////////////////////////////////////////////////////////////

        private List<AddInCommandCreator> _creators;

        //////////////////////////////////////////////////////////////////////////

        private Dictionary<string, AddInCommand> _commands;

        //////////////////////////////////////////////////////////////////////////
    }
}
