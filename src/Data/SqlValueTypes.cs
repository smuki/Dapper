using System;

namespace Volte.Data.Dapper
{
    public enum SqlValueTypes {
        // Fields
        PrototypeString    = 0,
        SimpleQuotesString = 1,
        String             = 2,
        BoolToString       = 3,
        BoolToInterger     = 4,
        AccessDateString   = 5,
        NotSupport         = 6,
        OracleDate         = 7
    }
}
