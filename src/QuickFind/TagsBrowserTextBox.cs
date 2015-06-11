using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// TODO this still doesn't work properly. It seems like the list view has
// two selections, which get out of sync when the list is rebuilt.

namespace QuickFind
{
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class TagsBrowserTextBox : TextBox
    {
        [DllImport("user32.dll")]
        static extern short GetKeyState(Int32 nVirtKey);

        private static bool GetKeyState(Keys key)
        {
            return (GetKeyState((int)key) & 0x8000) != 0;
        }

        /// <summary>
        /// the Control to send certain keypresses to.
        /// </summary>
        public Control Target;

        public TagsBrowserTextBox()
        {
            InitializeComponent();

            this.Target = null;
        }

        // couldn't work out how to do this without some Win32 crap
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
            case (int)WM.WM_KEYDOWN:
            case (int)WM.WM_KEYUP:
                {
                    int key = m.WParam.ToInt32();

                    switch (key)
                    {
                    case (int)Keys.Home:
                    case (int)Keys.End:
                        if (string.IsNullOrEmpty(Text))
                            WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, (Keys)key);

                        return;

                    case (int)Keys.PageDown:
                        if (GetKeyState(Keys.ControlKey))
                            WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, Keys.End);
                        else
                            WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, Keys.PageDown);

                        return;

                    case (int)Keys.PageUp:
                        if (GetKeyState(Keys.ControlKey))
                            WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, Keys.Home);
                        else
                            WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, Keys.PageUp);

                        return;

                    case (int)Keys.Up:
                        WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, Keys.Up);

                        return;

                    case (int)Keys.Down:
                        WindowsMessages.ForwardKeyMessageWithNewKey(Target, m, Keys.Down);

                        return;
                    }
                }
                break;
            }
            base.WndProc(ref m);
        }
    }
}
