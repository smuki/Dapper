using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    internal class CommanderUpdate : CommanderStream {
        const string ZFILE_NAME = "CommanderUpdate";
        private Dictionary<string, bool> _PropertyChanged = new Dictionary<string, bool>();
        // Methods
        public CommanderUpdate(Streaming _streaming, ClassMapping cm) : base(_streaming, cm)
        {
            var xxx = DBNull.Value;
            DateTime x = DapperUtil.DateTime_MinValue;

            if (x == DapperUtil.DateTime_MinValue) {
                xxx = DBNull.Value;
            }

        }

        public override IDbCommand BuildForObject(Streaming _streaming, EntityObject _entity)
        {
            AttributeMapping map1;
            IDbCommand command1 = _streaming.GetCommand();
            _PropertyChanged = _entity.PropertyChanged;

            this.getSQLClause();

            command1.CommandText = this.SqlString;

            int num1 = this.ThisClassMapping.GetSize();

            for (int num2 = 0; num2 < num1; num2++) {
                map1 = this.ThisClassMapping.AttributeMapping(num2);

                if ((!map1.PrimaryKey) && map1.CanWrite) {
                    if (_PropertyChanged.ContainsKey(map1.Name)) {
                        IDataParameter parameter1 = command1.CreateParameter();
                        parameter1.ParameterName  = _streaming.ParameterPrefix + map1.Name;
                        parameter1.DbType         = map1.Type;

                        object obj1 = _entity.GetAttributeValue(map1.Name);

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
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                map1 = this.ThisClassMapping.TimestampAttribute;

                IDataParameter parameter2 = command1.CreateParameter();
                parameter2.ParameterName  = "@Update" + map1.Name;
                parameter2.DbType         = map1.Type;
                parameter2.Value          = DateTime.Now.Ticks;

                command1.Parameters.Add(parameter2);
            }

            for (int num3 = 0; num3 < this.ThisClassMapping.GetKeySize(); num3++) {
                map1 = this.ThisClassMapping.KeyAttributeMapping(num3);

                IDataParameter parameter3 = command1.CreateParameter();
                parameter3.ParameterName  = _streaming.ParameterPrefix + map1.Name;
                parameter3.DbType         = map1.Type;
                parameter3.Value          = _entity.GetAttributeValue(map1.Name);

                command1.Parameters.Add(parameter3);
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                map1 = this.ThisClassMapping.TimestampAttribute;

                IDataParameter parameter4 = command1.CreateParameter();
                parameter4.ParameterName  = _streaming.ParameterPrefix + map1.Name;
                parameter4.DbType         = map1.Type;
                parameter4.Value          = _entity.GetAttributeValue(map1.Name);

                command1.Parameters.Add(parameter4);
            }

            return command1;
        }

        private void getSQLClause()
        {
            AttributeMapping map1;
            this.Clear();
            this.AddSqlClause("UPDATE ");
            this.AddSqlClause(this.Streaming.QuotationMarksStart + this.ThisClassMapping.TableName + this.Streaming.QuotationMarksEnd);
            this.AddSqlClause(" ");
            this.AddSqlClause("SET ");
            bool flag1 = true;

            for (int num1 = 0; num1 < this.ThisClassMapping.GetSize(); num1++) {
                map1 = this.ThisClassMapping.AttributeMapping(num1);

                if ((!map1.PrimaryKey) && map1.CanWrite) {
                    if (_PropertyChanged.ContainsKey(map1.Name)) {
                        if (flag1) {
                            flag1 = false;
                        } else {
                            this.AddSqlClause(",");
                        }

                        string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, this.ThisClassMapping.AttributeMapping(num1).ColumnName, this.Streaming.QuotationMarksEnd } ;
                        this.AddSqlClause(string.Concat(textArray1) + "=");
                        this.AddSqlClause(this.Streaming.GetStringParameter(map1.Name, num1));
                    }
                }
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                this.AddSqlClause(",");
                string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, this.ThisClassMapping.TimestampAttribute.ColumnName, this.Streaming.QuotationMarksEnd } ;
                this.AddSqlClause(string.Concat(textArray1) + "=");
                this.AddSqlClause(this.Streaming.GetStringParameter("Update" + this.ThisClassMapping.TimestampAttribute.ColumnName, this.ThisClassMapping.GetSize() + 1));
            }

            this.AddSqlClause(" WHERE 1=1 ");

            for (int num2 = 0; num2 < this.ThisClassMapping.GetKeySize(); num2++) {
                map1 = this.ThisClassMapping.KeyAttributeMapping(num2);

                if (map1.PrimaryKey) {
                    string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, map1.ColumnName, this.Streaming.QuotationMarksEnd } ;
                    this.AddSqlClause(" AND " + string.Concat(textArray1) + "=");
                    this.AddSqlClause(this.Streaming.GetStringParameter(map1.Name, (this.ThisClassMapping.GetSize() + num2) + 1));
                }
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, ThisClassMapping.TimestampAttribute.ColumnName, this.Streaming.QuotationMarksEnd } ;
                this.AddSqlClause(" AND " + string.Concat(textArray1) + "=");
                this.AddSqlClause(this.Streaming.GetStringParameter(this.ThisClassMapping.TimestampAttribute.ColumnName, (this.ThisClassMapping.GetKeySize() + this.ThisClassMapping.GetSize()) + 2));
            }

        }
    }
}

