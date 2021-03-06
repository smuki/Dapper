using System;
using System.Data;
using System.Data.SqlClient;

namespace Volte.Data.Dapper
{
    public class MsSqlServer : Streaming {
        const string ZFILE_NAME = "MsSqlServer";
        // Methods
        public MsSqlServer()
        {
            base.Vendor = "MsSqlServer";
            this.QuotationMarksStart = "[";
            this.QuotationMarksEnd = "]";
            this.UnicodePrefix = "N";

        }

        private MsSqlServer(string name, string connectionString)
        {

            base.Vendor = "MsSqlServer";
            this.QuotationMarksStart = "[";
            this.QuotationMarksEnd = "]";
            this.UnicodePrefix = "N";
            this.ConnectionString = connectionString;
            this.Connection = new SqlConnection(connectionString);
            base.DbName = name;
        }

        public override DataTable AsDataTable(IDbCommand cmd, int top)
        {
            cmd.Connection = this.Connection;
            cmd.Transaction = this.Transaction;
            string text1 = cmd.CommandText;

            if (top > 0) {
                if (text1.StartsWith("SELECT")) {
                    text1 = "SELECT TOP " + top.ToString() + text1.Substring(6);
                }
            }

            cmd.CommandText = text1;
            SqlDataAdapter adapter1 = new SqlDataAdapter((SqlCommand) cmd);
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

            if (!(e is SqlException)) {
                message = "";
                return ExceptionTypes.Unknown;
            }

            SqlException exception1 = (SqlException) e;
            int num1 = 0;
            num1 = 0;

            while (num1 < exception1.Errors.Count) {
                if (exception1.Errors[num1].Number != 0xe25) {
                    break;
                }

                num1++;
            }

            int num3 = exception1.Errors[num1].Number;

            if (num3 <= 0x220) {
                if (num3 == 0) {
                    return ExceptionTypes.DataTypeNotMatch;
                }

                if (num3 == 0x203) {
                    message = "Reference:" + exception1.Message;
                    return ExceptionTypes.NotAllowDataNull;
                }

                if (num3 == 0x220) {
                    message = "Reference:" + exception1.Message;
                    return ExceptionTypes.AutoValueOn;
                }
            } else if (num3 <= 0xa43) {
                if (num3 == 0x223) {
                    message = "Reference:" + exception1.Message;
                    return ExceptionTypes.RestrictError;
                }

                if (num3 == 0xa43) {
                    message = "Reference!";
                    return ExceptionTypes.NotUnique;
                }
            } else {
                if (num3 == 0x1fd8) {
                    return ExceptionTypes.DataTooLong;
                }

                if (num3 == 0x1ff2) {
                    message = "Reference:" + exception1.Message;
                    return ExceptionTypes.RequireAttribute;
                }
            }

            message = "Database Exception:";

            for (int num2 = 0; num2 < exception1.Errors.Count; num2++) {
                object obj1 = message;
                object[] objArray1 = new object[10] { obj1, "Index #", num2, "\nMessage: ", exception1.Message, "\nNative: ", exception1.Errors[num2].Number.ToString(), "\nSource: ", exception1.Errors[num2].Source, "\n" } ;
                message = string.Concat(objArray1);
            }

            return ExceptionTypes.DatabaseUnknwnError;
        }

        public override IDataAdapter GetAdapter(IDbCommand cmd)
        {
            SqlDataAdapter adapter1 = new SqlDataAdapter();
            adapter1.SelectCommand = (SqlCommand) cmd;
            cmd.Connection = this.Connection;
            return adapter1;
        }

        public override Streaming GetCopy()
        {
            return new MsSqlServer(base.DbName, this.ConnectionString);
        }

        public override DataRow GetDataRow(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;
            SqlDataAdapter adapter1 = new SqlDataAdapter();
            adapter1.SelectCommand = (SqlCommand) cmd;
            DataTable table1 = new DataTable();
            adapter1.Fill(table1);

            if (table1.Rows.Count > 0) {
                return table1.Rows[0];
            }

            return null;
        }

        public override string GetStringParameter(string name, int i)
        {
            return "@" + name;
        }

        public override void Initialize(string connectionString)
        {
            this.ConnectionString = connectionString.Replace("Provider=SQLOLEDB.1;", "");

            try {
                SqlConnection connection1 = new SqlConnection(this.ConnectionString);
                this.Connection = connection1;
                this.Connection.Open();
            } catch (SqlException exception1) {
                try {
                    if (this.Connection.State != ConnectionState.Closed) {
                        this.Connection.Close();
                    }

                    SqlConnection connection1 = new SqlConnection(this.ConnectionString + ";Pooling=false");

                    this.Connection = connection1;
                    this.Connection.Open();
                } catch (SqlException exception2) {
                    if (exception2.Number == 0x11) {
                        throw new DapperException("Database Not Found Reference:" + exception1.Message + "." + exception2.Message, ExceptionTypes.DatabaseConnectionError);
                    }

                    throw new DapperException("Connect Database Fail Reference:" + exception1.Message + "." + exception2.Message, ExceptionTypes.DatabaseError);
                }
            } finally {
                if (this.Connection != null) {
                    this.Connection.Close();
                }
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

