using System;
using System.Data;
using Vertica.Data.VerticaClient;

using Volte.Data.Json;

namespace Volte.Data.Dapper
{
    internal class Vertica : Streaming {
        const string ZFILE_NAME = "Vertica";

        private const string GET_IDENTITY = "SELECT @@IDENTITY ";

        public Vertica()
        {
            this.Vendor = "Vertica";
            this.QuotationMarksStart = "`";
            this.QuotationMarksEnd = "`";
            this.ParameterPrefix = "?";
        }

        private Vertica(string name, string connectionString)
        {
            this.Vendor = "Vertica";
            this.ConnectionString = connectionString;
            this.Connection = new VerticaConnection(connectionString);
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

            VerticaDataAdapter adapter = new VerticaDataAdapter((VerticaCommand) cmd);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public override IDataReader GetDataReader(IDbCommand cmd, int top)
        {
            if (top > 0) {
                cmd.CommandText = this.TopWhere(cmd.CommandText, top);
            }

            //ZZLogger.Sql(ZFILE_NAME, " getDataReader", cmd.CommandText);

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
            VerticaDataAdapter adapter = new VerticaDataAdapter();
            adapter.SelectCommand    = (VerticaCommand) cmd;
            cmd.Connection           = this.Connection;
            return adapter;
        }

        public override Streaming GetCopy()
        {
            return new Vertica(base.DbName, base.ConnectionString);
        }

        public override DataRow GetDataRow(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;
            VerticaDataAdapter adapter = new VerticaDataAdapter();
            adapter.SelectCommand = (VerticaCommand) cmd;
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
                VerticaConnection connection = new VerticaConnection(base.ConnectionString);
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
