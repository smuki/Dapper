using System;
using System.Data;
using System.IO;

using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    internal class CommanderAddNew : CommanderStream {
        const string ZFILE_NAME = "CommanderAddNew";
        // Methods
        public CommanderAddNew(Streaming _streaming, ClassMapping cm) : base(_streaming, cm)
        {
            this.getSQLClause();
        }

        public override IDbCommand BuildForObject(Streaming _streaming, EntityObject obj)
        {
            AttributeMapping map1;
            IDbCommand command1 = _streaming.GetCommand();
            command1.CommandText = this.SqlString;

            int num1 = this.ThisClassMapping.GetSize();

            for (int num2 = 0; num2 < num1; num2++) {
                map1 = this.ThisClassMapping.AttributeMapping(num2);

                if (map1.CanWrite) {
                    IDataParameter parameter1 = command1.CreateParameter();
                    parameter1.ParameterName  = _streaming.ParameterPrefix + map1.Name;
                    parameter1.DbType         = map1.Type;

                    object obj1 = obj.GetAttributeValue(map1.Name);

                    if (obj1 == null) {
                        parameter1.Value = DBNull.Value;
                    } else if (map1.Type == DbType.DateTime && (DateTime) obj1 <= DapperUtil.DateTime_MinValue) {
                        parameter1.Value = DBNull.Value;
                    } else {
                        parameter1.Value = obj1;
                    }

                    command1.Parameters.Add(parameter1);
                }
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                map1 = this.ThisClassMapping.TimestampAttribute;

                IDataParameter parameter2 = command1.CreateParameter();
                parameter2.ParameterName  = _streaming.ParameterPrefix + map1.Name;
                parameter2.DbType         = map1.Type;
                parameter2.Value          = DateTime.Now.Ticks;

                command1.Parameters.Add(parameter2);
            }

            return command1;
        }

        private void getSQLClause()
        {
            this.AddSqlClause("INSERT INTO ");
            this.AddSqlClause(this.Streaming.QuotationMarksStart + this.ThisClassMapping.TableName + this.Streaming.QuotationMarksEnd);
            this.AddSqlClause(" ");
            string text1 = "";
            bool flag1 = true;
            this.AddSqlClause("(");
            int num1 = this.ThisClassMapping.GetSize();

            for (int num2 = 0; num2 < num1; num2++) {
                AttributeMapping map1 = this.ThisClassMapping.AttributeMapping(num2);

                if (map1.CanWrite) {
                    if (flag1) {
                        this.AddSqlClause(this.Streaming.GetQuotationColumn(map1.ColumnName));
                        text1 = this.Streaming.GetStringParameter(map1.Name, num2);
                    } else {
                        this.AddSqlClause(",");
                        this.AddSqlClause(this.Streaming.GetQuotationColumn(map1.ColumnName));
                        text1 = text1 + "," + this.Streaming.GetStringParameter(map1.Name, num2);
                    }

                    flag1 = false;
                }
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                if (!flag1) {
                    this.AddSqlClause(",");
                    text1 = text1 + ",";
                }

                this.AddSqlClause(this.Streaming.GetQuotationColumn(this.ThisClassMapping.TimestampAttribute.ColumnName));
                text1 = text1 + this.Streaming.GetStringParameter(this.ThisClassMapping.TimestampAttribute.Name, num1);
            }

            this.AddSqlClause(")");
            this.AddSqlClause(" VALUES (");
            this.AddSqlClause(text1);
            this.AddSqlClause(")");
        }
    }
}

