using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    internal class CommanderRetrieve : CommanderStream {
        const string ZFILE_NAME = "CommanderRetrieve";

        private void getSQLClause()
        {
            AttributeMapping map2;
            int num1;
            this.AddSqlClause("SELECT ");
            bool flag1 = true;
            ClassMapping map1 = this.ThisClassMapping;
            string text1 = " AS " + this.Streaming.QuotationMarksStart;

            num1 = map1.AttributeMappings.Count;

            for (int num2 = 0; num2 < num1; num2++) {
                map2 = map1.AttributeMappings[num2];
                string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, map2.ColumnName, this.Streaming.QuotationMarksEnd } ;
                this.AddSqlClause((flag1 ? "" : ",") + string.Concat(textArray1));
                this.AddSqlClause(text1 + map2.Name + this.Streaming.QuotationMarksEnd);
                flag1 = false;
            }

            this.AddSqlClause(" FROM " + this.Streaming.QuotationMarksStart + this.ThisClassMapping.TableName + this.Streaming.QuotationMarksEnd);
            flag1 = true;

            _selecFromClause = this.SqlString;
            this.AddSqlClause(" WHERE 1=1 ");

            for (int num3 = 0; num3 < this.ThisClassMapping.GetKeySize(); num3++) {
                map2 = this.ThisClassMapping.KeyAttributeMapping(num3);

                if (map2.PrimaryKey) {
                    string[] textArray1 = new string[7] { this.Streaming.QuotationMarksStart, this.ThisClassMapping.TableName, this.Streaming.QuotationMarksEnd, ".", this.Streaming.QuotationMarksStart, map2.ColumnName, this.Streaming.QuotationMarksEnd } ;
                    this.AddSqlClause(" AND " + string.Concat(textArray1) + "=" + this.Streaming.GetStringParameter(map2.Name, num3));
                }
            }
        }

        // Methods
        public CommanderRetrieve(Streaming _streaming, ClassMapping _classMapping) : base(_streaming, _classMapping)
        {
            _stringForInherit = null;
            this.getSQLClause();
        }

        public override IDbCommand BuildForObject(Streaming _streaming, EntityObject obj)
        {
            IDbCommand command1 = _streaming.GetCommand();
            command1.CommandText = this.SqlString;
            int num1 = this.ThisClassMapping.GetKeySize();

            for (int num2 = 0; num2 < num1; num2++) {
                AttributeMapping map1 = this.ThisClassMapping.KeyAttributeMapping(num2);

                if (map1.PrimaryKey) {
                    IDataParameter parameter1 = command1.CreateParameter();
                    parameter1.ParameterName = _streaming.ParameterPrefix + map1.Name;
                    parameter1.DbType = map1.Type;
                    parameter1.Value = obj.GetAttributeValue(map1.Name);
                    command1.Parameters.Add(parameter1);
                }
            }

            return command1;
        }

        // Properties
        public string SelectClause
        {
            get {
                return _selecFromClause;
            }
        }
        public string StringForInherit
        {
            get {
                return _stringForInherit;
            }
        }

        // Fields
        private string _selecFromClause;
        private string _stringForInherit;
    }
}

