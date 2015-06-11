using QuickFind;
using System;
using System.Collections.Generic;
using System.Text;

namespace minitags
{
    class MinitagItemHandler : QuickFindItemHandler
    {
        //////////////////////////////////////////////////////////////////////////

        public override string[] ColumnNames
        {
            get
            {
                return _columnNames;
            }
        }

        //////////////////////////////////////////////////////////////////////////

        public override string GetColumnContents(object o, int columnIdx)
        {
            Minitag minitag=o as Minitag;

            switch (columnIdx)
            {
            case 0:
                return minitag.Expansion;

            case 1:
                return minitag.Type.TypeName;

            case 2:
                return minitag.Range.StartPoint.Line.ToString();
            }

            return null;
        }

        //////////////////////////////////////////////////////////////////////////

        public override int SearchColumn
        {
            get
            {
                return 0;//name
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private static readonly string[] _columnNames = new string[] {
            "Name",
            "Type",
            "Line",
        };

        //////////////////////////////////////////////////////////////////////////
    }
}
