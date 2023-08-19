using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Volte.Data.Dapper
{
    public class QueryOrder
    {
        public virtual string Field
        {
            get;
            set;
        }

        public virtual string Asc
        {
            get;
            set;
        }
    }
}
