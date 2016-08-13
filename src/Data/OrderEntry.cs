using System;

namespace Volte.Data.Dapper
{
    internal struct OrderEntry {
        const string ZFILE_NAME = "OrderEntry";
        public string AttributeName;
        public bool IsAscend;
        public OrderEntry(string _Name, bool isAsc)
        {
            this.AttributeName = _Name;
            this.IsAscend      = isAsc;
        }
    }
}

