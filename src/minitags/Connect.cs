using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using VSAddInLib;

//////////////////////////////////////////////////////////////////////////
//
// WEIRD VS PROBLEMS
//
// - Name and Text must match
//
// http://www.bokebb.com/dev/english/1965/posts/196564011.shtml
//
// command's name and text must match when using AddNamedCommand2! WTF.
//
// - Can't use full name for adding command
//
// If you accidentally use the full name (e.g.,
// "minitags.Connect.BrowseMinitags") for the command name passed in to
// AddCommands2, it works fine with the VS SDK installed, but doesn't work at
// all without it!
//
//////////////////////////////////////////////////////////////////////////

namespace minitags
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        //////////////////////////////////////////////////////////////////////////

        // the Connect class is created multiple times, it seems, once for
        // adding commands in then once again for querying the status, then
        // probably more times for other stuff. so having the command registry
        // as an instance field and creating it in the constructor is no good,
        // because unless the OnConnection has run and called AddNamedCommands2
        // on that particular specific instance the registry's table of
        // AddInCommands is empty (no DTE2 object to create them with...) and so
        // the status of each can't be determined.
        //
        // So presumably you're supposed to switch on command name in the
        // relevant functions and have the logic there, or forward it manually
        // to somewhere else, then duplicate all the names in every function.
        // *slack-jawed incomprehension* but I suppose for quick & dirty addins
        // this is if not the ticket then certainly enough to gain you entry.
        //
        // This isn't a particularly great way of solving this conundrum
        // (fucking globals) but it does at least work.
        //
        // TODO junk all the wizard auto shit and sort it out proper like.
        private static readonly AddInCommandRegistry _commands = new AddInCommandRegistry(
            "minitags.Connect",
            new AddInCommandRegistry.AddInCommandCreator[] {
                delegate(DTE2 dte2)
                {
                    return new BrowseMinitagsCommand(dte2);
                },
            });

        /// <summary>
        /// Implements the constructor for the Add-in object. Place your
        /// initialization code within this method.</summary>
        public Connect()
        {
            //             Debug.WriteLine("minitags.Connect.Connect");
            //             MessageBox.Show("minitags.Connect.Connect");
        }

        /// <summary>
        /// Implements the OnConnection method of the IDTExtensibility2
        /// interface. Receives notification that the Add-in is being
        /// loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            //             Debug.WriteLine(string.Format("minitags.Connect.OnConnection: connectMode={0}", connectMode));
            //             MessageBox.Show(string.Format("minitags.Connect.OnConnection: connectMode={0}", connectMode));

            _commands.OnConnection(_applicationObject);

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;
                string toolsMenuName;

                try
                {
                    // If you would like to move the command to a different
                    // menu, change the word "Tools" to the 
                    //  English version of the menu. This code will take the
                    //  culture, append on the name of the menu then add the
                    //  command to that menu. You can find a list of all the
                    //  top-level menus in the file CommandBar.resx.
                    ResourceManager resourceManager = new ResourceManager("minitags.CommandBar", Assembly.GetExecutingAssembly());
                    CultureInfo cultureInfo = new System.Globalization.CultureInfo(_applicationObject.LocaleID);
                    string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
                    toolsMenuName = resourceManager.GetString(resourceName);
                }
                catch
                {
                    //We tried to find a localized version of the word Tools, but one was not found.
                    //  Default to the en-US word, which may work for the current culture.
                    toolsMenuName = "Tools";
                }

                //Place the command on the tools menu. Find the MenuBar command
                //bar, which is the top-level command bar holding all the main
                //menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

                //Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                _commands.AddNamedCommands2(_applicationObject, _addInInstance, ref contextGUIDS, toolsPopup);
            }
        }

        /// <summary>
        /// Implements the OnDisconnection method of the IDTExtensibility2
        /// interface. Receives notification that the Add-in is being
        /// unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>
        /// Implements the OnAddInsUpdate method of the IDTExtensibility2
        /// interface. Receives notification when the collection of Add-ins has
        /// changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>
        /// Implements the OnStartupComplete method of the IDTExtensibility2
        /// interface. Receives notification that the host application has
        /// completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>
        /// Implements the OnBeginShutdown method of the IDTExtensibility2
        /// interface. Receives notification that the host application is being
        /// unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>
        /// Implements the QueryStatus method of the IDTCommandTarget interface.
        /// This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText,
            ref vsCommandStatus status, ref object commandText)
        {
//            MessageBox.Show(string.Format("minitags.Connect.QueryStatus: commandName={0} neededText={1}", commandName, neededText));
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (_commands.TryQueryStatus(commandName, ref status))
                    return;
            }
        }

        /// <summary>
        /// Implements the Exec method of the IDTCommandTarget interface. This
        /// is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
//            MessageBox.Show(string.Format("minitags.Connect.Exec: commandName={0} executeOption={1}", commandName, executeOption));

            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (_commands.TryExecCommand(commandName, ref varIn, ref varOut, ref handled))
                {
                    return;
                }
            }
        }
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
    }
}