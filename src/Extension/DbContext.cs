using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Volte.Data.Dapper
{

    public class DbContext : IDisposable {
        const string ZFILE_NAME = "DbContext";

        private readonly StringBuilder _Fields    = new StringBuilder();
        private readonly EntityCompiler _Compiler = new EntityCompiler();

        private string _ConnectionString = "";
        private string paramPrefix      = "@";
        private string providerName     = "System.Data.SqlClient";
        private DBType _dbType          = DBType.SqlServer;
        private bool _Transaction       = false;
        private bool _Writeable         = true;

        private IDbConnection dbConnection;
        private DbProviderFactory dbFactory;
        private IDbTransaction _DbTransaction;

        public IDbConnection DbConnection   { get { return dbConnection;      }  }
        public IDbTransaction DbTransaction { get { return _DbTransaction;    }  }
        public bool Transaction             { get { return _Transaction;      }  }
        public string ParamPrefix           { get { return paramPrefix;       }  }
        public string ProviderName          { get { return providerName;      }  }
        public string dbAdapter             { get { return _ConnectionString; }  }
        public DBType DbType                { get { return _dbType;           }  }

        public bool IsOpen
        {
            get {
                if (dbConnection == null) {
                    return false;
                }

                if (dbConnection.State == ConnectionState.Open) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        public bool Writeable { get { return _Writeable; } set { _Writeable  = value; }  }

        public DbContext(string DbName)
        {
            if (!string.IsNullOrEmpty(DbName)) {
                Setting _Setting = Settings.GetValue(DbName);
                _dbType          = _Setting.DbType;
                paramPrefix      = _Setting.ParamPrefix;
                providerName     = _Setting.ProviderName;

                _ConnectionString = _Setting.ConnectionString;
                dbFactory         = DbProviderFactories.GetFactory(providerName);
                dbConnection      = dbFactory.CreateConnection();

                this.Open();
            }

        }

        public DbContext(string DbName, string connStr)
        {
            providerName = "System.Data.SqlClient";

            Setting _Setting  = Settings.GetValue(DbName, providerName, connStr);
            _dbType           = _Setting.DbType;
            paramPrefix       = _Setting.ParamPrefix;
            providerName      = _Setting.ProviderName;
            _ConnectionString = connStr;
            dbFactory         = DbProviderFactories.GetFactory(providerName);
            dbConnection      = dbFactory.CreateConnection();

            this.Open();
        }

        public DbContext(string DbName, string providerName, string connStr)
        {
            Setting _Setting  = Settings.GetValue(DbName, providerName, connStr);
            _dbType           = _Setting.DbType;
            paramPrefix       = _Setting.ParamPrefix;
            providerName      = _Setting.ProviderName;
            _ConnectionString = connStr;

            if (providerName == "MySqlClient") {

                dbConnection = dbFactory.CreateConnection();
            } else {
                dbFactory         = DbProviderFactories.GetFactory(providerName);
                dbConnection      = dbFactory.CreateConnection();
            }

            this.Open();
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
                _Column.SetValue("TableName"  , _TableName);
                _Column.SetValue("ColumnName" , _ColumnName);
                _JSONObject.Add(_Column);

            }

            return _JSONObject;
        }

        public void Open()
        {

            try {

                dbConnection.ConnectionString = _ConnectionString;

                if (dbConnection.State == ConnectionState.Closed) {
                    dbConnection.Open();
                }

            } catch (Exception exception1) {
                string _cnnString = _ConnectionString;
                ZZLogger.Error(ZFILE_NAME, _ConnectionString);
                ZZLogger.Error(ZFILE_NAME, exception1);

                if (dbConnection.State != ConnectionState.Closed) {
                    dbConnection.Close();
                }

                dbConnection.ConnectionString = _cnnString + ";Pooling=false;";
                dbConnection.Open();
            }
        }


        public void BeginTransaction()
        {

            if (_Writeable == false) {
                throw new DapperException("Connection Is Readonly", ExceptionTypes.NotDataIsReadonly);
            }

            if (_Transaction) {
                ZZLogger.Debug(ZFILE_NAME, "In Transaction Mode");
                return;
            }

            _DbTransaction = dbConnection.BeginTransaction();

            _Transaction = true;
        }

        public void Commit()
        {
            if (_Writeable == false) {
                throw new DapperException("Connection Is Readonly", ExceptionTypes.NotDataIsReadonly);
            }

            if (!_Transaction) {
                ZZLogger.Debug(ZFILE_NAME, "not in trasactin mode");
                return;
            }

            _DbTransaction.Commit();
            _DbTransaction = null;
        }

        public void RollBack()
        {
            if (!_Transaction) {
                return;
            }

            _DbTransaction.Rollback();
            _DbTransaction = null;
        }

        public void Close()
        {
            if (dbConnection.State == ConnectionState.Open) {
                dbConnection.Close();
            }
        }

        public IDataReader DataReader(string strSql)
        {
            return DataReader(strSql , CommandBehavior.Default);
        }

        public IDataReader DataReader(string strSql, CommandBehavior behavior)
        {
            IDbCommand cmd  = dbConnection.CreateCommand();
            cmd.CommandText = strSql;

            if (this.Transaction) {
                cmd.Transaction = _DbTransaction;
            }

            return cmd.ExecuteReader(behavior);
        }

        public dynamic Query(string strSql)
        {
            return this.Query(strSql, 0);
        }

        public dynamic Query(string strSql, int top)
        {

            IDbCommand cmd          = dbConnection.CreateCommand();
            cmd.CommandText         = strSql;

            if (this.Transaction) {
                cmd.Transaction = _DbTransaction;
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

        public dynamic GetEntity(string strSql)
        {

            IDbCommand cmd          = dbConnection.CreateCommand();
            cmd.CommandText         = strSql;

            if (this.Transaction) {
                cmd.Transaction = _DbTransaction;
            }

            IDataReader _DataReader = cmd.ExecuteReader();

            _Fields.Length = 0;

            for (int i = 0; i < _DataReader.FieldCount; i++) {
                _Fields.Append("_");
                _Fields.Append(_DataReader.GetName(i));
            }

            string className = DapperUtil.ComputeHash(_Fields.ToString() + "_");
            string[] ar      = new string[0];//; { fileName };

            return _Compiler.GetEntity(ar, _DataReader, className);
        }

        public int Execute(string sql)
        {
            ZZLogger.Debug(ZFILE_NAME, sql);
            return dbConnection.Execute(sql, null, this.DbTransaction);
        }

        public void Dispose()
        {
            if (dbConnection != null) {
                try {
                    dbConnection.Dispose();
                } catch { }
            }
        }
    }

    public enum DBType {
        SqlServer,
        SqlServerCE,
        MySql,
        PostgreSQL,
        Oracle,
        SQLite
    }
}
