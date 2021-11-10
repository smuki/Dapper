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

        public DbContext()
        {
        }

        public DbContext(string _DbName)
        {
            if (!string.IsNullOrEmpty(_DbName)){
                _dbName        = _DbName;
                _Writeable     = false;
                _Transaction   = false;
                _IsForceCommit = false;
                _Broker        = ObjectBroker.Instance();
                _connection     = _Broker.getStreaming(_DbName).GetCopy();
                _connection.Open();
            }
        }

        public DbContext(string _DbName, string connStr)
        {

            _dbName          = _DbName;
            Setting _Setting = Settings.Instance().GetValue(_DbName, "MsSqlServer", connStr);
            _dbType          = _Setting.DbType;
            _Broker          = ObjectBroker.Instance();
            _connection       = _Broker.getStreaming(_DbName).GetCopy();
            _connection.Open();
        }

        public DbContext(string _DbName, string providerName, string connStr)
        {

            _dbName          = _DbName;
            Setting _Setting = Settings.Instance().GetValue(_DbName, providerName, connStr);
            _dbType          = _Setting.DbType;
            _Broker          = ObjectBroker.Instance();
            _connection       = _Broker.getStreaming(_DbName).GetCopy();
            _connection.Open();
        }

        public bool IsOpen
        {
            get {
                if (_connection.Connection == null) {
                    return false;
                }

                if (_connection.Connection.State == ConnectionState.Open) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        public void Close()
        {
            if (_connection== null || _connection.Connection == null) {
                return;
            }
            _connection.Close();
        }

        public void DeleteEntity(EntityObject obj)
        {
            this.BeginTransaction();

            obj.DbName = DbName;
            _Broker.DeleteObject(obj, _connection, _IsForceCommit);
        }

        public List<JSONObject> TableColumns(string cSql)
        {
            IDataReader _DataReader = this.DataReader(cSql, CommandBehavior.KeyInfo);
            DataTable myDataTable = _DataReader.GetSchemaTable();
            _DataReader.Close();

            List<JSONObject> _JSONObject = new List<JSONObject>();

            foreach (DataRow myDataRow in myDataTable.Rows) {
                string _TableName = "";
                string _ColumnName = "";
                string _DataType = "";
                string _DataTypeName = "";
                string _ColumnSize = "10";

                foreach (DataColumn myDataColumn in myDataTable.Columns) {

                    //ZZLogger.Debug(ZFILE_NAME , myDataColumn.ToString()+"=="+myDataRow[myDataColumn].ToString());
                    if (myDataColumn.ToString() == "BaseColumnName") {
                        _ColumnName = myDataRow[myDataColumn].ToString();
                    } else if (myDataColumn.ToString() == "BaseTableName") {
                        _TableName = myDataRow[myDataColumn].ToString();
                    } else if (myDataColumn.ToString() == "DataTypeName") {
                       _DataTypeName = myDataRow[myDataColumn].ToString();
                    } else if (myDataColumn.ToString() == "DataType") {
                       _DataType = myDataRow[myDataColumn].ToString();
                    } else if (myDataColumn.ToString() == "ColumnSize") {
                       _ColumnSize = myDataRow[myDataColumn].ToString();
                    }
                }
                if (!string.IsNullOrEmpty(_DataTypeName)){
                    _DataType = _DataTypeName;
                }

                JSONObject _Column = new JSONObject();
                _Column.SetValue("sTableName"  , _TableName);
                _Column.SetValue("sColumnName" , _ColumnName);
                _Column.SetValue("sDataType"   , _DataType);
                _Column.SetValue("nColumnSize" , _ColumnSize);
                _JSONObject.Add(_Column);
            }
            return _JSONObject;
        }

        public IDbCommand DbCommand(string strSql)
        {
            return DbCommand(strSql , CommandBehavior.Default);
        }

        public IDbCommand DbCommand(string strSql, CommandBehavior behavior)
        {
            IDbCommand cmd  = _connection.Connection.CreateCommand();
            cmd.CommandText = strSql;

            if (this.Transaction) {
                cmd.Transaction = _connection.Transaction;
            }

            return cmd;
        }

        public IDataReader DataReader(string sCommandText)
        {
            return DataReader(sCommandText, CommandBehavior.Default);
        }

        public IDataReader DataReader(string sCommandText, CommandBehavior behavior)
        {
            IDbCommand cmd  = _connection.Connection.CreateCommand();
            cmd.CommandTimeout = CommandTimeout;
            cmd.CommandText = sCommandText;

            if (this.Transaction) {
                cmd.Transaction = _connection.Transaction;
            }

            return cmd.ExecuteReader(behavior);
        }

        public DataTable RetrieveDataTable(CriteriaRetrieve retrieve)
        {
            retrieve.DbName = DbName;
            return _Broker.DoRetrieveDataTable(retrieve, _connection);
        }

        public void Dispose()
        {
            if (_connection != null) {
                if (_connection.Connection != null) {
                    try {
                        _connection.Connection.Close();
                        _connection.Connection.Dispose();
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

            IDbCommand cmd  = _connection.Connection.CreateCommand();
            cmd.CommandText = strSql;

            if (this.Transaction) {
                cmd.Transaction = _connection.Transaction;
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

            IDataReader _DataReader = _Broker.DoRetrieveDataReader(retrieve, _connection);

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
            IDataReader _DataReader = _Broker.DoRetrieveDataReader(retrieve, _connection);

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

            IDataReader _DataReader = _Broker.getDataReader(this.DbName, _connection, strSql, 0);

            return  AutoCompiler<T>.ConvertToEntity(_DataReader);
        }

        public dynamic Entities(string strSql)
        {
            return this.Entities(strSql, 0);
        }

        public dynamic Entities(string strSql, int top)
        {
            IDataReader _DataReader = _Broker.getDataReader(this.DbName, _connection, strSql, top);

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
            IDataReader _DataReader = _Broker.DoRetrieveDataReader(retrieve, _connection);
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
            IDataReader _DataReader    = _Broker.DoRetrieveDataReader(retrieve, _connection);
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
            _Broker.RetrieveEntityObject(obj, _connection);
        }

        public void SaveChanges(CriteriaUpdate update)
        {
            update.DbName = DbName;
            this.BeginTransaction();
            _Broker.ProcessCriteria(update, _connection, _IsForceCommit);
        }

        public void SaveChanges(CriteriaDelete delete)
        {
            delete.DbName = DbName;
            this.BeginTransaction();
            _Broker.ProcessCriteria(delete, _connection, _IsForceCommit);
        }

        public void SaveChanges(EntityObject obj)
        {
            obj.DbName = DbName;
            this.BeginTransaction();
            _Broker.SaveChange(obj, _connection, _IsForceCommit);
        }

        public int Execute(string sCommandText)
        {
            return _connection.DoSql(sCommandText);
        }

        public int Execute(string sCommandText , JSONObject Parameters)
        {

            this.BeginTransaction();
            IDbCommand cmd     = _connection.Connection.CreateCommand();
            cmd.CommandText    = sCommandText;
            cmd.CommandTimeout = CommandTimeout;
            cmd.Transaction    = _connection.Transaction;

            JSONObject type = Parameters.GetJSONObject("_");
            foreach (string item in Parameters.Names) {
                if (item!="_"){
                    IDataParameter parameter1 = cmd.CreateParameter();
                    parameter1.ParameterName  = this.ParamPrefix + item;

                    if (type.GetValue(item).ToLower()=="bit"){

                        parameter1.DbType = System.Data.DbType.Boolean;
                        parameter1.Value  = Parameters.GetBoolean(item);

                    }else if (type.GetValue(item).ToLower()=="datetime"){

                        parameter1.DbType = System.Data.DbType.DateTime;
                        if (string.IsNullOrEmpty(Parameters.GetValue(item))) {
                            parameter1.Value = DBNull.Value;
                        }else{
                            parameter1.Value  = Parameters.GetDateTime(item);
                        }

                    }else if (type.GetValue(item).ToLower()=="decimal"){

                        parameter1.DbType = System.Data.DbType.Decimal;
                        if (string.IsNullOrEmpty(Parameters.GetValue(item))) {
                            parameter1.Value = DBNull.Value;
                        }else{
                            parameter1.Value  = Parameters.GetDecimal(item);
                        }

                    }else{
                        parameter1.Value = Parameters.GetValue(item);
                    }

                    cmd.Parameters.Add(parameter1);
                }
            }
            return Convert.ToInt32(cmd.ExecuteNonQuery());
        }

        public int Execute(string sCommandText , Dictionary<string , object> Parameters)
        {
            if (Parameters.Count>0){
                this.BeginTransaction();
                IDbCommand cmd  = _connection.Connection.CreateCommand();
                cmd.CommandText = sCommandText;
                cmd.Transaction = _connection.Transaction;

                foreach (var item in Parameters) {

                    IDataParameter parameter1 = cmd.CreateParameter();
                    parameter1.ParameterName  = this.ParamPrefix + item.Key;

                    object obj1 = item.Value;

                    if (obj1 == null) {
                        parameter1.Value = DBNull.Value;
                    } else {
                        parameter1.Value = obj1;
                    }

                    cmd.Parameters.Add(parameter1);
                }
                return Convert.ToInt32(cmd.ExecuteNonQuery());
            }else{
                return _connection.DoSql(sCommandText);
            }
        }

        public DataTable RetrieveDataTable(string strSql)
        {
            return this.RetrieveDataTable(strSql,  0);
        }

        public DataTable RetrieveDataTable(string strSql,  int m_Top)
        {
            this.BeginTransaction();

            return _Broker.DoQueryTransaction(_connection, strSql, m_Top);
        }

        public IDataReader RetrieveDataReader(string strSql)
        {
            return this.RetrieveDataReader(strSql,  0);
        }

        public IDataReader RetrieveDataReader(string strSql,  int m_Top)
        {
            return _Broker.getDataReader(this.DbName, _connection, strSql, m_Top);
        }

        public void BeginTransaction()
        {
       
            if (_Writeable == false) {
                throw new DapperException("Data Is Readonly", ExceptionTypes.NotDataIsReadonly);
            }

            if (_Transaction) {
                ZZLogger.Debug(ZFILE_NAME, "In Transaction Mode");
                return;
            }

            _connection.BeginTransaction();
            _Transaction = true;

        }

        public bool Commit()
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
                _connection.CommitTransaction();
                flag2        = true;
                _Transaction = false;
            } catch (Exception exception1) {
                _connection.RollbackTransaction();
                throw;
            } finally {
                _connection.Close();
            }

            return flag2;
        }

        public void RollBack()
        {
      
            try {
                _connection.RollbackTransaction();
                _Transaction = false;
            } finally {
                _connection.Close();
            }
        }

        // Properties
        public bool IsForceCommit  { get { return _IsForceCommit;  } set { _IsForceCommit  = value; }  }
        public bool Writeable      { get { return _Writeable;      } set { _Writeable      = value; }  }
        public int  CommandTimeout { get { return _CommandTimeout; } set { _CommandTimeout = value;      }  }
        public DBType DbType       { get { return _dbType;      }  }

        public IDbConnection DbConnection   { get { return _connection.Connection;       }  }
        public IDbTransaction DbTransaction { get { return _connection.Transaction;      }  }

        public string Vendor                { get { return _connection.Vendor;           }  }
        public bool Transaction             { get { return _Transaction;                }  }
        public string ParamPrefix           { get { return _connection.ParameterPrefix;  }  }
        public string DbName                { get { return _dbName;                     }  }
        public string dbAdapter             { get { return _connection.ConnectionString; }  }

        // Fields
        private readonly StringBuilder _Fields    = new StringBuilder();
        private readonly EntityCompiler _Compiler = new EntityCompiler();
        private string _dbName;
        private DBType _dbType       = DBType.SqlServer;
        private bool _Transaction    = false;
        private int  _CommandTimeout = 120;
        private bool _Writeable      = false;
        private bool _IsForceCommit  = true;
        private ObjectBroker _Broker;
        private Streaming _connection;
    }
}
