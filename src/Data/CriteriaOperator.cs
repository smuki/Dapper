using System;

namespace Volte.Data.Dapper
{
    internal enum CriteriaOperator {
        // Fields
        Equals              = 0,
        NotEquals           = 1,
        GreaterThan         = 2,
        GreaterThanOrEquals = 3,
        LessThan            = 4,
        LessThanOrEquals    = 5,
        Like                = 6,
        NotLike             = 7,
        In                  = 8,
        NotIn               = 9,
        Where               = 10,
    }
}

