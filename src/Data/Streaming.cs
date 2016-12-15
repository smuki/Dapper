using System;
using System.IO;
using System.Data;

using Volte.Data.Json;
namespace Volte.Data.Dapper
{

    public abstract class Streaming {
        const string ZFILE_NAME = "Streaming";
        // Methods
        public Streaming()
        {
            _QuotationMarksStart = "\"";
            _QuotationMarksEnd   = "\"";
            _vendor              = "MsSqlServer";
            _connection          = null;
            _transaction         = null;
            _IsInTransaction     = false;
            _cnnString           = "";
        }

        public Streaming(string name)
        {
            _QuotationMarksStart = "\"";
            _QuotationMarksEnd   = "\"";
            _vendor              = "MsSqlServer";
            _connection          = null;
            _transaction         = null;
            _IsInTransaction     = false;
            _cnnString           = "";
            _DbName              = name;
        }

        public abstract DataTable AsDataTable(IDbCommand cmd, int top);
        public abstract IDataReader GetDataReader(IDbCommand cmd, int top);

        public void BeginTransaction()
        {
            if ((_connection == null) || (_connection.State == ConnectionState.Closed)) {
                throw new DapperException("数据库未打开或未初始化！");
            }

            _transaction = _connection.BeginTransaction();
            _IsInTransaction = true;

        }

        public void Close()
        {
            if (_connection.State == ConnectionState.Open) {
                _connection.Close();
            }

            _IsInTransaction = false;
        }

        public void CommitTransaction()
        {
            if (_transaction == null) {
                throw new DapperException("无可用事务！");
            }

            _transaction.Commit();
            _IsInTransaction = false;
        }

        public int DoCommand(IDbCommand cmd)
        {
            if (_IsInTransaction) {
                cmd.Transaction = _transaction;
            }

            cmd.Connection = _connection;
            return cmd.ExecuteNonQuery();
        }

        public int DoSql(string sqlstring)
        {
            IDbCommand command1 = this.GetCommand();

            if (_IsInTransaction) {
                command1.Transaction = _transaction;
            }

            command1.Connection  = _connection;
            command1.CommandText = sqlstring;

            return command1.ExecuteNonQuery();
        }

        public abstract ExceptionTypes ErrorHandler(Exception e, out string message);
        public abstract IDataAdapter GetAdapter(IDbCommand cmd);

        public IDbCommand GetCommand()
        {
            return _connection.CreateCommand();
        }

        public abstract Streaming GetCopy();
        public abstract DataRow GetDataRow(IDbCommand cmd);

        public string GetQuotationColumn(string columnName)
        {
            return this.QuotationMarksStart + columnName + this.QuotationMarksEnd;
        }

        public abstract string GetStringParameter(string name, int i);

        public virtual string TopWhere(string sqlString, int top)
        {
            return sqlString;
        }

        public abstract void Initialize(string connectionString);

        public abstract int InsertRecord(IDbCommand cmd, out object identity);

        public void Open()
        {
            try {
                if (_connection.State == ConnectionState.Closed) {
                    _connection.Open();
                } else {
                    //ZZDebug.Write(ZFILE_NAME,"Keep Open!...");
                }

            } catch (Exception exception1) {
                ZZLogger.Error(ZFILE_NAME, exception1);
                string _OcnnString = _cnnString;

                if (_connection.State != ConnectionState.Closed) {
                    _connection.Close();
                }

                _connection.ConnectionString = _OcnnString + ";Pooling=false;";
                _connection.Open();
            }

        }

        public void RollbackTransaction()
        {
            if (_transaction == null) {
                //throw new DapperException("无可用事务！");
            } else {
                if (_IsInTransaction) {
                    _transaction.Rollback();
                }
            }

            _IsInTransaction = false;
            _transaction = null;
        }

        public abstract SqlValueTypes SqlValueType(DbType type);

        // Properties
        public bool IsInTransaction
        {
            get {
                return _IsInTransaction;
            }
        }

        public string ConnectionString    { get { return _cnnString;           } set { _cnnString           = value; }  }
        public string DbName              { get { return _DbName;              } set { _DbName              = value; }  }
        public string QuotationMarksEnd   { get { return _QuotationMarksEnd;   } set { _QuotationMarksEnd   = value; }  }
        public string QuotationMarksStart { get { return _QuotationMarksStart; } set { _QuotationMarksStart = value; }  }
        public string Vendor              { get { return _vendor;              } set { _vendor              = value; }  }
        public IDbConnection Connection   { get { return _connection;          } set { _connection          = value; }  }
        public IDbTransaction Transaction { get { return _transaction;         } set { _transaction         = value; }  }
        public string ParameterPrefix     { get { return _ParameterPrefix;     } set { _ParameterPrefix     = value; }  }
        public string UnicodePrefix       { get { return _UnicodePrefix;       } set { _UnicodePrefix       = value; }  }

        // Fields
        private string _cnnString;
        private bool _IsInTransaction;
        private string _DbName;
        private string _ParameterPrefix = "@";
        private string _UnicodePrefix   = "";
        private string _QuotationMarksEnd;
        private string _QuotationMarksStart;
        private string _vendor;
        private IDbConnection _connection;
        private IDbTransaction _transaction;
    }
}
