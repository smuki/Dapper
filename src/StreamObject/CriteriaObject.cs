using System;

namespace Volte.Data.Dapper
{
    public class CriteriaObject {
        const string ZFILE_NAME = "CriteriaObject";
        // Methods
        public CriteriaObject()
        {
            _classType = null;
            _dbName    = null;
        }

        // Properties
        internal ActionTypes ActionType { get { return _ActionType;   } set { _ActionType   = value; }  }
        public string DbName            { get { return _dbName;       } set { _dbName       = value; }  }
        public string SELECTClause      { get { return _SELECTClause; } set { _SELECTClause = value; }  }
        public Type ClassType           { get { return _classType;    } set { _classType    = value; }  }

        // Fields
        private string      _dbName;
        private string      _SELECTClause;
        private Type        _classType;
        private ActionTypes _ActionType;
    }
}

