using System;
using System.Data;
using System.Data.Odbc;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    internal class Odbc : Streaming {
        const string ZFILE_NAME = "Odbc";
        // Methods
        public Odbc()
        {
            base.Vendor = "Odbc";
        }

        private Odbc(string name, string connectionString)
        {
            base.Vendor = "Odbc";
            this.ConnectionString = connectionString;
            this.Connection = new OdbcConnection(connectionString);
            base.DbName = name;
        }

        public override DataTable AsDataTable(IDbCommand cmd, int top)
        {
            cmd.Connection = this.Connection;
            cmd.Transaction = this.Transaction;
            string text1 = cmd.CommandText;

            if (text1.StartsWith("SELECT")) {
                text1 = "SELECT TOP " + top.ToString() + text1.Substring(6);
            }

            cmd.CommandText = text1;
            OdbcDataAdapter adapter1 = new OdbcDataAdapter((OdbcCommand) cmd);
            DataTable table1 = new DataTable();
            adapter1.Fill(table1);
            return table1;
        }

        public override IDataReader GetDataReader(IDbCommand cmd, int top)
        {
            if (top > 0) {
                string text1 = cmd.CommandText;

                if (text1.StartsWith("SELECT")) {
                    text1 = "SELECT TOP " + top.ToString() + text1.Substring(6);
                }

                cmd.CommandText = text1;
            }

            cmd.Connection = this.Connection;
            cmd.Transaction = this.Transaction;
            return cmd.ExecuteReader();
        }

        public override ExceptionTypes ErrorHandler(Exception e, out string message)
        {
            message = "";

            if (!(e is OdbcException)) {
                message = "";
                return ExceptionTypes.Unknown;
            }

            OdbcException exception1 = (OdbcException) e;
            int num1 = 0;
            num1 = 0;

            while (num1 < exception1.Errors.Count) {
                if (exception1.Errors[num1].NativeError != 0xe25) {
                    break;
                }

                num1++;
            }

            int num3 = exception1.Errors[num1].NativeError;

            if (num3 <= 0x220) {
                if (num3 == 0) {
                    return ExceptionTypes.DataTypeNotMatch;
                }

                if (num3 == 0x203) {
                    message = "参考：" + exception1.Message;
                    return ExceptionTypes.NotAllowDataNull;
                }

                if (num3 == 0x220) {
                    message = "参考：" + exception1.Message;
                    return ExceptionTypes.AutoValueOn;
                }
            } else if (num3 <= 0xa43) {
                if (num3 == 0x223) {
                    message = "参考：" + exception1.Message;
                    return ExceptionTypes.RestrictError;
                }

                if (num3 == 0xa43) {
                    message = "数据重复！";
                    return ExceptionTypes.NotUnique;
                }
            } else {
                if (num3 == 0x1fd8) {
                    return ExceptionTypes.DataTooLong;
                }

                if (num3 == 0x1ff2) {
                    message = "参考：" + exception1.Message;
                    return ExceptionTypes.RequireAttribute;
                }
            }

            message = "数据库操作异常:";

            for (int num2 = 0; num2 < exception1.Errors.Count; num2++) {
                object obj1 = message;
                object[] objArray1 = new object[10] { obj1, "Index #", num2, "\nMessage: ", exception1.Message, "\nNative: ", exception1.Errors[num2].NativeError.ToString(), "\nSource: ", exception1.Errors[num2].Source, "\n" };
                message = string.Concat(objArray1);
            }

            return ExceptionTypes.DatabaseUnknwnError;
        }

        public override IDataAdapter GetAdapter(IDbCommand cmd)
        {
            OdbcDataAdapter adapter1 = new OdbcDataAdapter();
            adapter1.SelectCommand = (OdbcCommand) cmd;
            cmd.Connection = this.Connection;
            return adapter1;
        }

        public override Streaming GetCopy()
        {
            return new Odbc(base.DbName, this.ConnectionString);
        }

        public override DataRow GetDataRow(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;
            OdbcDataAdapter adapter1 = new OdbcDataAdapter();
            adapter1.SelectCommand = (OdbcCommand) cmd;
            DataTable table1 = new DataTable();
            adapter1.Fill(table1);

            if (table1.Rows.Count > 0) {
                return table1.Rows[0];
            }

            return null;
        }

        public override string GetStringParameter(string name, int i)
        {
            return "?";
        }

        public override void Initialize(string connectionString)
        {
            this.ConnectionString = connectionString.Replace("Provider=SQLOLEDB.1;", "");

            try {
                OdbcConnection connection1 = new OdbcConnection(this.ConnectionString);
                this.Connection = connection1;
                this.Connection.Open();
            } catch (OdbcException exception1) {
                if (exception1.Errors[0].NativeError == 0x11) {
                    throw new DapperException("数据库不存在！参考：" + exception1.Message, ExceptionTypes.DatabaseConnectionError);
                }

                throw new DapperException("连接数据库失败！参考：" + exception1.Message, ExceptionTypes.DatabaseError);
            } finally {
                this.Connection.Close();
            }
        }

        public override int InsertRecord(IDbCommand cmd, out object identity)
        {
            int num1 = 0;
            cmd.Transaction = this.Transaction;
            cmd.Connection = this.Connection;
            num1 = cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT @@IDENTITY ";
            identity = cmd.ExecuteScalar();
            return num1;
        }

        public override SqlValueTypes SqlValueType(DbType type)
        {
            if (type == DbType.Boolean) {
                return SqlValueTypes.BoolToInterger;
            }

            return SqlValueTypes.PrototypeString;
        }


        // Fields
        private const string GET_IDENTITY = "SELECT @@IDENTITY ";
    }
}
