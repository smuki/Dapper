using System;

namespace Volte.Data.Dapper
{
    public class DynamicPropertyModel
    {
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
