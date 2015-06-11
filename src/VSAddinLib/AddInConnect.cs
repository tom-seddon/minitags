using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//////////////////////////////////////////////////////////////////////////

namespace VSAddInLib
{
    /// <summary>
    /// Base class for add in connect. Simply supply pointer to static
    /// AddInCommandRegistry (base class takes ownership) on construction.
    /// </summary>
    [ComVisible(true)]
    public class AddInConnect : IDTExtensibility2, IDTCommandTarget
    {
        //////////////////////////////////////////////////////////////////////////

        public AddInConnect(AddInCommandRegistry commands)
        {
            _commands = commands;
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Implements the OnConnection method of the IDTExtensibility2
        /// interface. Receives notification that the Add-in is being
        /// loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode,
            object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            _commands.OnConnection(_applicationObject);

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;
                string toolsMenuName;

                try
                {
                    // If you would like to move the command to a different
                    // menu, change the word "Tools" to the English version of
                    // the menu. This code will take the culture, append on the
                    // name of the menu then add the command to that menu. You
                    // can find a list of all the top-level menus in the file
                    // CommandBar.resx.
                    ResourceManager resourceManager =
                        new ResourceManager(_commands.Prefix + "CommandBar",
                        Assembly.GetExecutingAssembly());

                    CultureInfo cultureInfo =
                        new System.Globalization.CultureInfo(_applicationObject.LocaleID);

                    string resourceName =
                        String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");

                    toolsMenuName = resourceManager.GetString(resourceName);
                }
                catch
                {
                    // We tried to find a localized version of the word Tools,
                    // but one was not found. Default to the en-US word, which
                    // may work for the current culture.
                    toolsMenuName = "Tools";
                }

                // Place the command on the tools menu. Find the MenuBar command
                // bar, which is the top-level command bar holding all the main
                // menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar =
                    ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

                // Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                _commands.AddNamedCommands2(_applicationObject, _addInInstance,
                    ref contextGUIDS, toolsPopup);
            }

            // TODO sort this bit out.
            _commands.OnConnection2(_applicationObject);
        }

        //////////////////////////////////////////////////////////////////////////

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

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Implements the OnAddInsUpdate method of the IDTExtensibility2
        /// interface. Receives notification when the collection of Add-ins has
        /// changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Implements the OnStartupComplete method of the IDTExtensibility2
        /// interface. Receives notification that the host application has
        /// completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Implements the OnBeginShutdown method of the IDTExtensibility2
        /// interface. Receives notification that the host application is being
        /// unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        //////////////////////////////////////////////////////////////////////////

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
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (_commands.TryQueryStatus(commandName, ref status))
                    return;
            }
        }

        //////////////////////////////////////////////////////////////////////////

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
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (_commands.TryExecCommand(commandName, ref varIn, ref varOut, ref handled))
                {
                    return;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private DTE2 _applicationObject;

        //////////////////////////////////////////////////////////////////////////

        private AddIn _addInInstance;

        //////////////////////////////////////////////////////////////////////////

        private readonly AddInCommandRegistry _commands;

        //////////////////////////////////////////////////////////////////////////
    }

    //////////////////////////////////////////////////////////////////////////
}