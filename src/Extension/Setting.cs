using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Volte.Data.Dapper
{
    public class Setting {
        const string ZFILE_NAME          = "Setting";
        private string _providerName     = "System.Data.SqlClient";
        private string _typeName         = "";
        private string _ClassMapPath     = "";
        private string _DbName           = "";
        private bool   _Include          = true;
        private string _ConnectionString = "";
        private string paramPrefix       = "@";
        private string unicodePrefix     = "";
        private DBType _dbType           = DBType.SqlServer;

        public string ConnectionString { get { return _ConnectionString; } set { _ConnectionString = value; }  }
        public string DbName           { get { return _DbName;           } set { _DbName           = value; }  }
        public bool   Include          { get { return _Include;          } set { _Include          = value; }  }
        public string ProviderName     { get { return _providerName;     } set { _providerName     = value; }  }
        public string ClassMapPath     { get { return _ClassMapPath;     } set { _ClassMapPath     = value; }  }
        public string TypeName         { get { return _typeName;         } set { _typeName         = value; }  }
        public string ParamPrefix      { get { return paramPrefix;       } set { paramPrefix       = value; }  }
        public string UnicodePrefix    { get { return unicodePrefix;     } set { unicodePrefix     = value; }  }
        public DBType DbType           { get { return _dbType;           } set { _dbType           = value; }  }

    }
}
