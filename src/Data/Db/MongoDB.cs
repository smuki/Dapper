using System;
using System.Data;
using Vertica.Data.VerticaClient;

//using MongoDB.Bson;
using MongoDB.Driver;

namespace Volte.Data.Dapper
{
    internal class MongoDB : Streaming {
        const string ZFILE_NAME = "MongoDB";

        private const string GET_IDENTITY = "SELECT @@IDENTITY ";

        public MongoDB()
        {
            this.Vendor = "MongoDB";
            this.QuotationMarksStart = "`";
            this.QuotationMarksEnd = "`";
            this.ParameterPrefix = "?";
        }

        private MongoDB(string name, string connectionString)
        {
            this.Vendor = "MongoDB";
            this.ConnectionString = connectionString;
            this.Connection = new MongoDBConnection(connectionString);
            this.QuotationMarksStart = "`";
            this.QuotationMarksEnd = "`";
            this.ParameterPrefix = "?";
            this.DbName = name;
        }

        public override string TopWhere(string sqlString, int top)
        {
            int num1 = sqlString.IndexOf("Top");

            if (num1 != -1) {
                sqlString = sqlString.Substring(0, num1) + sqlString.Substring(num1 + 6);
                sqlString = sqlString + " limit " + top.ToString();
            } else {
                sqlString = sqlString + " limit " + top.ToString();
            }

            return sqlString;
        }

        public override DataTable AsDataTable(IDbCommand cmd, int top)
        {
            cmd.Connection  = this.Connection;
            cmd.Transaction = this.Transaction;

            if (top > 0) {
                cmd.CommandText = this.TopWhere(cmd.CommandText, top);
            }

            MongoDBDataAdapter adapter = new MongoDBDataAdapter((MongoDBCommand) cmd);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public override IDataReader GetDataReader(IDbCommand cmd, int top)
        {
            if (top > 0) {
                cmd.CommandText = this.TopWhere(cmd.CommandText, top);
            }

            ZZLogger.Debug(ZFILE_NAME, " getDataReader", cmd.CommandText);

            cmd.Connection  = this.Connection;
            cmd.Transaction = this.Transaction;
            return cmd.ExecuteReader();
        }

        public override ExceptionTypes ErrorHandler(Exception e, out string message)
        {
            message = "";

            if (e is VerticaException) {
                VerticaException exception = (VerticaException) e;
                string str = message;
                message = str + "Message: " + exception.Message + "\nNumber: " + "\nSource: " + exception.Source + "\n";
                return ExceptionTypes.DatabaseUnknwnError;
            }

            return ExceptionTypes.Unknown;
        }

        public override IDataAdapter GetAdapter(IDbCommand cmd)
        {
            MongoDBDataAdapter adapter = new MongoDBDataAdapter();
            adapter.SelectCommand    = (MongoDBCommand) cmd;
            cmd.Connection           = this.Connection;
            return adapter;
        }

        public override Streaming GetCopy()
        {
            return new MongoDB(base.DbName, base.ConnectionString);
        }

        public override DataRow GetDataRow(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;
            MongoDBDataAdapter adapter = new MongoDBDataAdapter();
            adapter.SelectCommand = (MongoDBCommand) cmd;
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count > 0) {
                return table.Rows[0];
            }

            return null;
        }

        public override string GetStringParameter(string name, int i)
        {
            return "?" + name;
        }

        public override void Initialize(string connectionString)
        {
            base.ConnectionString = connectionString;

            try {
                MongoDBConnection connection = new MongoDBConnection(base.ConnectionString);
                base.Connection = connection;
                base.Connection.Open();
            } catch (VerticaException exception) {
                throw new DapperException("连接数据库失败！参考：" + exception.Message + base.ConnectionString, ExceptionTypes.DatabaseError);
            } catch (Exception exception2) {
                throw exception2;
            } finally {
                if (base.Connection!=null){
                    base.Connection.Close();
                }
            }
        }

        public override int InsertRecord(IDbCommand cmd, out object identity)
        {
            int num = 0;

            cmd.Transaction = this.Transaction;
            cmd.Connection  = this.Connection;
            num = cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT @@IDENTITY ";
            identity = cmd.ExecuteScalar();
            return num;
        }

        public override SqlValueTypes SqlValueType(DbType type)
        {
            if (type == DbType.Boolean) {
                return SqlValueTypes.PrototypeString;
            }

            return SqlValueTypes.PrototypeString;
        }
    }
}
