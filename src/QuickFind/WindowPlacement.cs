using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

// names disambiguated XMLwise since they are just the standard ones from
// pinvoke.net and therefore likely to clash with other library(s) in the
// same project...

namespace QuickFind
{
    //////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [XmlType(TypeName="QuickFind_WindowPlacement")]
    public class WindowPlacement
    {
        //////////////////////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [XmlType(TypeName = "QuickFind_POINTAPI")]
        public struct POINTAPI
        {
            public int x;
            public int y;
        }

        //////////////////////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [XmlType(TypeName = "QuickFind_RECT")]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        //////////////////////////////////////////////////////////////////////////

        public int length;
        public int flags;
        public int showCmd;
        public POINTAPI ptMinPosition;
        public POINTAPI ptMaxPosition;
        public RECT rcNormalPosition;

        //////////////////////////////////////////////////////////////////////////

        public WindowPlacement()
        {
            this.length = 0;
        }

        //////////////////////////////////////////////////////////////////////////

        public static WindowPlacement GetWindowPlacement(Form form)
        {
            WindowPlacement placement = new WindowPlacement();
            placement.length = Marshal.SizeOf(placement);

            if (!GetWindowPlacement(form.Handle, placement))
                return null;

            return placement;
        }

        //////////////////////////////////////////////////////////////////////////

        public void Set(Form form)
        {
            SetWindowPlacement(form.Handle, this);
        }

        //////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, WindowPlacement lpwndpl);

        //////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] WindowPlacement lpwndpl);

        //////////////////////////////////////////////////////////////////////////
    }

    //////////////////////////////////////////////////////////////////////////
}
