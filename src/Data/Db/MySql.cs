using System;
using System.Data;
using MySql.Data.MySqlClient;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    internal class MySql : Streaming {
        const string ZFILE_NAME = "MySql";

        private const string GET_IDENTITY = "SELECT @@IDENTITY ";

        public MySql()
        {
            this.Vendor = "MySql";
            this.QuotationMarksStart = "`";
            this.QuotationMarksEnd = "`";
            this.ParameterPrefix = "?";
        }

        private MySql(string name, string connectionString)
        {
            this.Vendor = "MySql";
            this.ConnectionString = connectionString;
            this.Connection = new MySqlConnection(connectionString);
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

            MySqlDataAdapter adapter = new MySqlDataAdapter((MySqlCommand) cmd);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public override IDataReader GetDataReader(IDbCommand cmd, int top)
        {
            if (top > 0) {
                cmd.CommandText = this.TopWhere(cmd.CommandText, top);
            }

            ZZLogger.Sql(ZFILE_NAME, " getDataReader", cmd.CommandText);

            cmd.Connection  = this.Connection;
            cmd.Transaction = this.Transaction;
            return cmd.ExecuteReader();
        }

        public override ExceptionTypes ErrorHandler(Exception e, out string message)
        {
            message = "";

            if (e is MySqlException) {
                MySqlException exception = (MySqlException) e;
                string str = message;
                message = str + "Message: " + exception.Message + "\nNumber: " + exception.Number.ToString() + "\nSource: " + exception.Source + "\n";
                return ExceptionTypes.DatabaseUnknwnError;
            }

            return ExceptionTypes.Unknown;
        }

        public override IDataAdapter GetAdapter(IDbCommand cmd)
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            adapter.SelectCommand    = (MySqlCommand) cmd;
            cmd.Connection           = this.Connection;
            return adapter;
        }

        public override Streaming GetCopy()
        {
            return new MySql(base.DbName, base.ConnectionString);
        }

        public override DataRow GetDataRow(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            adapter.SelectCommand = (MySqlCommand) cmd;
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
                MySqlConnection connection = new MySqlConnection(base.ConnectionString);
                base.Connection = connection;
                base.Connection.Open();
            } catch (MySqlException exception) {
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
