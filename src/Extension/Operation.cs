using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Volte.Data.Dapper
{
    public enum Operation {
        Equal          = 1,
        Less           = 2,
        Greater        = 3,
        LessOrEqual    = 4,
        GreaterOrEqual = 5,
        Contains       = 6,
        StartsWith     = 7,
        EndsWith       = 8,
        NotEqual       = 9,
        Expression     = 10,
    }
}
