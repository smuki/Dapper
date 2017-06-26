using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Runtime.CompilerServices;
using System.Data;

using System.Reflection.Emit;
using System.Reflection;

using Volte.Data.Json;


namespace Volte.Data.Dapper
{

    public class DbContext : IDisposable {
        // Methods
        const string ZFILE_NAME = "DbContext";
        public DbContext(string _DbName)
        {
            if (!string.IsNullOrEmpty(_DbName)){
                _dbName        = _DbName;
                _Writeable     = false;
                _Transaction   = false;
                _IsForceCommit = false;
                _Broker        = ObjectBroker.Instance();
                _Streaming     = _Broker.getStreaming(_DbName).GetCopy();
                _Streaming.Open();
            }
        }

        public DbContext(string _DbName, string connStr)
        {

            _dbName          = _DbName;
            Setting _Setting = Settings.Instance().GetValue(_DbName, "MsSqlServer", connStr);
            _dbType          = _Setting.DbType;
            _Broker          = ObjectBroker.Instance();
            _Streaming       = _Broker.getStreaming(_DbName).GetCopy();
            _Streaming.Open();
        }

        public DbContext(string _DbName, string providerName, string connStr)
        {

            _dbName          = _DbName;
            Setting _Setting = Settings.Instance().GetValue(_DbName, providerName, connStr);
            _dbType          = _Setting.DbType;
            _Broker          = ObjectBroker.Instance();
            _Streaming       = _Broker.getStreaming(_DbName).GetCopy();
            _Streaming.Open();
        }

        public bool IsOpen
        {
            get {
                if (_Streaming.Connection == null) {
                    return false;
                }

                if (_Streaming.Connection.State == ConnectionState.Open) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        public void Close()
        {
            if (_Streaming== null || _Streaming.Connection == null) {
                return;
            }
            _Streaming.Close();
        }

        public void DeleteEntity(EntityObject obj)
        {
            _BeginTransaction();

            obj.DbName = DbName;
            _Broker.DeleteObject(obj, _Streaming, _IsForceCommit);
        }

        public List<JSONObject> TableColumns(string cSql)
        {
            List<JSONObject> _JSONObject = new List<JSONObject>();

            IDataReader _DataReader = this.DataReader(cSql, CommandBehavior.KeyInfo);
            DataTable myDataTable = _DataReader.GetSchemaTable();
            _DataReader.Close();

            _JSONObject = new List<JSONObject>();

            foreach (DataRow myDataRow in myDataTable.Rows) {
                string _TableName = "";
                string _ColumnName = "";

                foreach (DataColumn myDataColumn in myDataTable.Columns) {
                    if (myDataColumn.ToString() == "BaseColumnName") {
                        _ColumnName = myDataRow[myDataColumn].ToString();
                    } else if (myDataColumn.ToString() == "BaseTableName") {
                        _TableName = myDataRow[myDataColumn].ToString();
                    }
                }

                JSONObject _Column = new JSONObject();
                _Column.SetValue("sTableName"  , _TableName);
                _Column.SetValue("sColumnName" , _ColumnName);
                _JSONObject.Add(_Column);

            }

            return _JSONObject;
        }

        public IDataReader DataReader(string strSql)
        {
            return DataReader(strSql , CommandBehavior.Default);
        }

        public IDataReader DataReader(string strSql, CommandBehavior behavior)
        {
            IDbCommand cmd  = _Streaming.Connection.CreateCommand();
            cmd.CommandText = strSql;

            if (this.Transaction) {
                cmd.Transaction = _Streaming.Transaction;
            }

            return cmd.ExecuteReader(behavior);
        }

        public DataTable RetrieveDataTable(CriteriaRetrieve retrieve)
        {
            retrieve.DbName = DbName;
            return _Broker.DoRetrieveDataTable(retrieve, _Streaming);
        }

        public void Dispose()
        {
            if (_Streaming != null) {
                if (_Streaming.Connection != null) {
                    try {
                        _Streaming.Connection.Dispose();
                    } catch { }
                }
            }
        }

        public dynamic Query(string strSql)
        {
            return this.Query(strSql, 0);
        }

        public dynamic Query(string strSql, int top)
        {

            IDbCommand cmd  = _Streaming.Connection.CreateCommand();
            cmd.CommandText = strSql;

            if (this.Transaction) {
                cmd.Transaction = _Streaming.Transaction;
            }

            IDataReader _DataReader = cmd.ExecuteReader();

            _Fields.Length = 0;

            for (int i = 0; i < _DataReader.FieldCount; i++) {
                _Fields.Append("_");
                _Fields.Append(_DataReader.GetName(i));
            }

            string className = DapperUtil.ComputeHash(_Fields.ToString());
            string[] ar      = new string[0];//; { fileName };

            return _Compiler.GetEntities(ar, _DataReader, className);
        }

        public QueryRows RetrieveRows(string sql)
        {
            QueryRows _QueryRows    = new QueryRows(this);
            _QueryRows.CommandText  = sql;
            _QueryRows.Open();
            return _QueryRows;
        }

        public EntityObject RetrieveEntity(CriteriaRetrieve retrieve)
        {
            retrieve.DbName   = DbName;
            EntityObject obj1 = null;

            IDataReader _DataReader = _Broker.DoRetrieveDataReader(retrieve, _Streaming);

            ObjectCursor cursor1 = new ObjectCursor(DbName, retrieve.ClassType, _DataReader);

            if (cursor1.HasObject()) {
                obj1 = cursor1.Next();
                obj1.DbName = DbName;
            }

            _DataReader.Close();
            return obj1;
        }

        public T Query<T> (CriteriaRetrieve retrieve) where T: new()
        {
            retrieve.DbName = DbName;
            IDataReader _DataReader = _Broker.DoRetrieveDataReader(retrieve, _Streaming);

            if (!_DataReader.Read()) {

                return default (T);
            }

            _Fields.Length = 0;

            for (int i = 0; i < _DataReader.FieldCount; i++) {
                _Fields.Append("_");
                _Fields.Append(_DataReader.GetName(i));
            }

            string className = DapperUtil.ComputeHash(_Fields.ToString());
            string[] ar      = new string[0];//; { fileName };

            return _Compiler.GetEntities(ar, _DataReader, className);

        }

        public List<T> Querys<T> (string strSql) where T: new()
        {

            IDataReader _DataReader = _Broker.getDataReader(this.DbName, _Streaming, strSql, 0);

            return  AutoCompiler<T>.ConvertToEntity(_DataReader);
        }

        public dynamic Entities(string strSql)
        {
            return this.Entities(strSql, 0);
        }

        public dynamic Entities(string strSql, int top)
        {
            IDataReader _DataReader = _Broker.getDataReader(this.DbName, _Streaming, strSql, top);

            _Fields.Length = 0;

            for (int i = 0; i < _DataReader.FieldCount; i++) {
                _Fields.Append("_");
                _Fields.Append(_DataReader.GetName(i));
            }

            string className = Volte.Data.Dapper.DapperUtil.ComputeHash(_Fields.ToString());
            string[] ar = new string[0];//; { fileName };

            return _Compiler.GetEntities(ar, _DataReader, className);
        }

        public List<T> RetrieveEntitys<T> (CriteriaRetrieve retrieve) where T: new()
        {
            retrieve.DbName = DbName;
            IDataReader _DataReader = _Broker.DoRetrieveDataReader(retrieve, _Streaming);
            ClassMapping _classMapping = _Broker.GetClassMapping(DbName, retrieve);

            if (_classMapping.AttributeMappings.Count == _DataReader.FieldCount) {
                return AutoCompiler<T>.ConvertToEntity(retrieve.ClassType, _classMapping.AttributeMappings, _DataReader);
            } else {
                return AutoCompiler<T>.ConvertToEntity(_DataReader);
            }
        }

        public List<EntityObject> RetrieveEntitys(CriteriaRetrieve retrieve)
        {

            retrieve.DbName            = DbName;
            IDataReader _DataReader    = _Broker.DoRetrieveDataReader(retrieve, _Streaming);
            ClassMapping _classMapping = _Broker.GetClassMapping(DbName, retrieve);

            if (_classMapping.AttributeMappings.Count == _DataReader.FieldCount) {
                return AutoCompiler<EntityObject>.ConvertToEntity(retrieve.ClassType, _classMapping.AttributeMappings, _DataReader);
            } else {

                List<EntityObject> container1 = new List<EntityObject>();
                ObjectCursor cursor1 = new ObjectCursor(DbName, retrieve.ClassType, _DataReader);

                while (cursor1.HasObject()) {
                    EntityObject obj1 = cursor1.Next();
                    obj1.DbName       = DbName;
                    container1.Add(obj1);
                }

                _DataReader.Close();
                return container1;
            }

        }

        public void RetrieveEntity(EntityObject obj)
        {
            obj.DbName = DbName;
            _Broker.RetrieveEntityObject(obj, _Streaming);
        }

        public void SaveChanges(CriteriaUpdate update)
        {
            update.DbName = DbName;
            _BeginTransaction();
            _Broker.ProcessCriteria(update, _Streaming, _IsForceCommit);
        }

        public void SaveChanges(CriteriaDelete delete)
        {
            delete.DbName = DbName;
            _BeginTransaction();
            _Broker.ProcessCriteria(delete, _Streaming, _IsForceCommit);
        }

        public void SaveChanges(EntityObject obj)
        {
            obj.DbName = DbName;
            _BeginTransaction();
            _Broker.SaveChange(obj, _Streaming, _IsForceCommit);
        }

        public int Execute(string strSql)
        {
            return _Streaming.DoSql(strSql);
        }

        public DataTable RetrieveDataTable(string strSql)
        {
            return this.RetrieveDataTable(strSql,  0);
        }

        public DataTable RetrieveDataTable(string strSql,  int m_Top)
        {
            _BeginTransaction();

            return _Broker.DoQueryTransaction(_Streaming, strSql, m_Top);
        }

        public IDataReader RetrieveDataReader(string strSql)
        {
            return this.RetrieveDataReader(strSql,  0);
        }

        public IDataReader RetrieveDataReader(string strSql,  int m_Top)
        {
            return _Broker.getDataReader(this.DbName, _Streaming, strSql, m_Top);
        }

        public void BeginTransaction()
        {
            _BeginTransaction();
        }

        private void _BeginTransaction()
        {
            if (_Writeable == false) {
                throw new DapperException("Data Is Readonly", ExceptionTypes.NotDataIsReadonly);
            }

            if (_Transaction) {
                ZZLogger.Debug(ZFILE_NAME, "In Transaction Mode");
                return;
            }

            _Streaming.BeginTransaction();
            _Transaction = true;

        }

        public bool Commit()
        {
            return _Commit();
        }

        private bool _Commit()
        {
            bool flag2 = false;

            if (_Writeable == false) {
                throw new DapperException("Data Is Readonly", ExceptionTypes.NotDataIsReadonly);
            }

            if (!_Transaction) {
                ZZLogger.Debug(ZFILE_NAME, "not in trasactin mode");
                return flag2;
            }



            try {
                _Streaming.CommitTransaction();
                flag2        = true;
                _Transaction = false;
            } catch (Exception exception1) {
                _Streaming.RollbackTransaction();
                throw;
            } finally {
                _Streaming.Close();
            }

            return flag2;
        }

        public void RollBack()
        {
            _RollBack();
        }

        private void _RollBack()
        {
            try {
                _Streaming.RollbackTransaction();
                _Transaction = false;
            } finally {
                _Streaming.Close();
            }
        }

        // Properties
        public bool IsForceCommit { get { return _IsForceCommit; } set { _IsForceCommit = value; }  }
        public bool Writeable     { get { return _Writeable;     } set { _Writeable     = value; }  }

        public IDbConnection DbConnection   { get { return _Streaming.Connection;       }  }
        public IDbTransaction DbTransaction { get { return _Streaming.Transaction;      }  }
        public DBType DbType                { get { return _dbType;                     }  }
        public string Vendor                { get { return _Streaming.Vendor;           }  }
        public bool Transaction             { get { return _Transaction;                }  }
        public string ParamPrefix           { get { return _Streaming.ParameterPrefix;  }  }
        public string DbName                { get { return _dbName;                     }  }
        public string dbAdapter             { get { return _Streaming.ConnectionString; }  }

        // Fields
        private readonly StringBuilder _Fields    = new StringBuilder();
        private readonly EntityCompiler _Compiler = new EntityCompiler();
        private string _dbName;
        private DBType _dbType      = DBType.SqlServer;
        private bool _Transaction   = false;
        private bool _Writeable     = false;
        private bool _IsForceCommit = true;
        private ObjectBroker _Broker;
        private Streaming _Streaming;
    }
}
