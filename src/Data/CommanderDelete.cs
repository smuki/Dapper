using System;
using System.Data;

using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    internal class CommanderDelete : CommanderStream {
        const string ZFILE_NAME = "CommanderDelete";
        // Methods
        public CommanderDelete(Streaming _streaming, ClassMapping cm) : base(_streaming, cm)
        {
            this.getSQLClause();
        }

        public override IDbCommand BuildForObject(Streaming _streaming, EntityObject obj)
        {
            AttributeMapping map1;
            IDbCommand command1  = this.Streaming.GetCommand();
            command1.CommandText = this.SqlString + this.partForObject;

            string delete_command = command1.CommandText;

            for (int num1 = 0; num1 < this.ThisClassMapping.GetKeySize(); num1++) {
                map1 = this.ThisClassMapping.KeyAttributeMapping(num1);

                if (map1.PrimaryKey) {
                    IDataParameter parameter1 = command1.CreateParameter();
                    parameter1.DbType         = map1.Type;
                    parameter1.Value          = obj.GetAttributeValue(map1.Name);
                    parameter1.ParameterName  = this.Streaming.ParameterPrefix + map1.Name;
                    command1.Parameters.Add(parameter1);
                    delete_command = delete_command + " " + map1.Name + "=" + parameter1.Value;
                }
            }

            ZZLogger.Sql("DELETE_LOG", delete_command);

            if (this.ThisClassMapping.TimestampAttribute != null) {

                map1                      = this.ThisClassMapping.TimestampAttribute;
                IDataParameter parameter2 = command1.CreateParameter();
                parameter2.ParameterName  = this.Streaming.ParameterPrefix + map1.Name;
                parameter2.DbType         = map1.Type;
                parameter2.Value          = obj.GetAttributeValue(map1.Name);
                command1.Parameters.Add(parameter2);
            }

            return command1;
        }

        private void getSQLClause()
        {
            this.AddSqlClause("DELETE FROM ");
            this.AddSqlClause(this.Streaming.QuotationMarksStart + this.ThisClassMapping.TableName + this.Streaming.QuotationMarksEnd);
            _deleteClause = this.SqlString;
            this.AddSqlClause(" WHERE 1=1 ");
            ClassMapping map2 = this.ThisClassMapping;

            for (int num1 = 0; num1 < map2.GetKeySize(); num1++) {
                AttributeMapping map1 = map2.KeyAttributeMapping(num1);

                if (map1.PrimaryKey) {
                    string text1 = this.partForObject;
                    string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, map1.ColumnName, this.Streaming.QuotationMarksEnd } ;

                    this.partForObject =  text1 + " AND " +  string.Concat(textArray1) + "=" + this.Streaming.GetStringParameter(map1.Name, num1);
                }
            }

            if (this.ThisClassMapping.TimestampAttribute != null) {
                string[] textArray2 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, this.ThisClassMapping.TimestampAttribute.ColumnName, this.Streaming.QuotationMarksEnd } ;
                this.AddSqlClause(" AND " + string.Concat(textArray2) + "=");
                this.AddSqlClause(this.Streaming.GetStringParameter(this.ThisClassMapping.TimestampAttribute.ColumnName, this.ThisClassMapping.GetKeySize()));
            }
        }

        // Properties
        public string DeleteClause
        {
            get {
                return _deleteClause;
            }
        }

        // Fields
        private string _deleteClause;
    }
}

