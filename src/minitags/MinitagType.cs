using System;
using System.Collections.Generic;
using System.Text;

namespace minitags
{
    //////////////////////////////////////////////////////////////////////////

    class MinitagType
    {
        //////////////////////////////////////////////////////////////////////////

        public readonly string TypeName;

        //////////////////////////////////////////////////////////////////////////

        public string Regexp;

        //////////////////////////////////////////////////////////////////////////

        public string Name;

        //////////////////////////////////////////////////////////////////////////

        public string Text;

        //////////////////////////////////////////////////////////////////////////

        public bool Greedy;

        //////////////////////////////////////////////////////////////////////////

        public MinitagType(string typeName)
        {
            TypeName = typeName;
            Regexp = null;
            Name = null;
            Text = null;
            Greedy = true;
        }

        //////////////////////////////////////////////////////////////////////////
    }
}
