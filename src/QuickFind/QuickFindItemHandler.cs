using System;
using System.Collections.Generic;
using System.Text;

namespace QuickFind
{
    /// <summary>
    /// Base class for handler objects that quick find dialog uses to extract
    /// attributes of user object.
    /// 
    /// </summary>
    /// 

    //  TODO this should all be done with generics (think that should be
    //  possible...) -- this would also save creating a new List<object> just
    //  for the QuickFind stuff when the caller already has a List<T>
    //  appropriate for their situation.

    public abstract class QuickFindItemHandler
    {
        /// <summary>
        /// Retrieve name of columns in display. Number of columns is fixed.
        /// </summary>
        public abstract string[] ColumnNames
        {
            get;
        }

        /// <summary>
        /// Retrieve contents of given column
        /// </summary>
        /// <param name="o"></param>
        /// <param name="columnIdx"></param>
        /// <returns></returns>
        public abstract string GetColumnContents(object o, int columnIdx);

        /// <summary>
        /// Retrieve index of column being searched.
        /// </summary>
        public abstract int SearchColumn
        {
            get;
        }
    }
}
