using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Volte.Data.Dapper
{

    public enum DBType {
        SqlServer,
        SqlServerCE,
        MySql,
        PostgreSQL,
        Oracle,
        Vertica,
        SQLite
    }
}
