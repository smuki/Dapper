using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Volte.Data.Dapper
{
    public class DynamicPropertyModel {
        public string Name
        {
            get;
            set;
        }

        public Type PropertyType
        {
            get;
            set;
        }
    }
}
