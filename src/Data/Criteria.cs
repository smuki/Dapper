using System;
using System.Data;

using Volte.Data.Dapper;
using Volte.Data.Json;

namespace Volte.Data.Dapper
{
    internal class Criteria {
        const string ZFILE_NAME = "Criteria";

        public Criteria(CriteriaOperator criteriaType, string attributeName, object attributeValue)
        {

            if (attributeValue == null) {
                throw new DapperException("?ҫ??tNull!");
            }

            _isFieldToField = false;
            _attribute2     = null;
            _type           = criteriaType;
            _attrName       = attributeName;
            _attrValue      = attributeValue;
        }

        public Criteria(CriteriaOperator criteriaType, string attributeName, string attributeName2, bool _FieldToField)
        {

            if (attributeName2 == null) {
                throw new DapperException("???t?X??ҫ?t???t??t?t");
            }

            _isFieldToField = _FieldToField;
            _attribute2     = null;
            _type           = criteriaType;
            _attrName       = attributeName;
            _attrName2      = attributeName2;
        }

        public string AsSqlClause(ClassMapping _classMapping, Streaming rdb)
        {

            if (_type == CriteriaOperator.Where) {
                return " (" + _attrValue.ToString() + ") ";
            }

            _atrribute = _classMapping.AttributeMapping(_attrName, true);

            if (_isFieldToField) {
                _attribute2 = _classMapping.AttributeMapping(_attrName2, true);
            }

            string[] textArray2 = new string[7] { rdb.QuotationMarksStart, _classMapping.TableName, rdb.QuotationMarksEnd, ".", rdb.QuotationMarksStart, _atrribute.ColumnName, rdb.QuotationMarksEnd } ;
            string text2 = string.Concat(textArray2);
            string text1 = text2;

            switch (_type) {
            case CriteriaOperator.Equals: {
                if ((_attrValue == null) && (_attribute2 == null)) {
                    return text1 = text1 + "=NULL";
                }

                text1 = text1 + "=";
                break;
            }

            case CriteriaOperator.GreaterThan: {
                text1 = text1 + ">";
                break;
            }

            case CriteriaOperator.GreaterThanOrEquals: {
                text1 = text1 + ">=";
                break;
            }

            case CriteriaOperator.NotEquals: {
                text1 = text1 + "<>";
                break;
            }

            case CriteriaOperator.LessThan: {
                text1 = text1 + "<";
                break;
            }

            case CriteriaOperator.LessThanOrEquals: {
                text1 = text1 + "<=";
                break;
            }

            case CriteriaOperator.Like: {
                text1 = text1 + " LIKE ";
                break;
            }

            case CriteriaOperator.NotLike: {
                text1 = text1 + " NOT LIKE ";
                break;
            }

            case CriteriaOperator.In: {
                text1 = text1 + " IN (";
                object[] objArray1 = (object[]) _attrValue;
                return text1 + this.StringOfInCrtieria(rdb, objArray1) + ")";
            }

            case CriteriaOperator.NotIn: {
                text1 = text1 + " NOT IN (";
                object[] objArray2 = (object[]) _attrValue;
                return text1 + this.StringOfInCrtieria(rdb, objArray2) + ")";
            }

            case CriteriaOperator.Where: {
                return " (" + _attrValue.ToString() + ") ";
            }

            default: {
                text1 = text1 + "";
                break;
            }
            }

            if (_isFieldToField) {
                string[] textArray3 = new string[7] { rdb.QuotationMarksStart, _classMapping.TableName, rdb.QuotationMarksEnd, ".", rdb.QuotationMarksStart, _attribute2.ColumnName, rdb.QuotationMarksEnd } ;
                return text1 + string.Concat(textArray3);
            }

            SqlValueTypes _SqlValueStringType = SqlValueType(rdb, _atrribute.Type);

            switch (_SqlValueStringType) {
            case SqlValueTypes.PrototypeString: {
                if (_type == CriteriaOperator.Like) {
                    return text1 + rdb.UnicodePrefix + "'" + _attrValue.ToString() + "'";
                }

                return text1 + _attrValue.ToString();
            }

            case SqlValueTypes.SimpleQuotesString: {
                return text1 + rdb.UnicodePrefix + "'" + _attrValue.ToString() + "'";
            }

            case SqlValueTypes.String: {
                return text1 + rdb.UnicodePrefix + "'" + _attrValue.ToString().Replace("'", "''") + "'";
            }

            case SqlValueTypes.BoolToString:
            case SqlValueTypes.BoolToInterger: {
                int num1 = Convert.ToInt32(_attrValue);
                return text1 + "'" + num1.ToString() + "'";
            }

            case SqlValueTypes.AccessDateString: {
                return text1 + "#" + _attrValue.ToString().Replace("'", "''") + "#";
            }

            case SqlValueTypes.OracleDate: {
                return text1 + "to_date('" + _attrValue.ToString().Replace("'", "''") + "','yyyy-mm-dd hh24:mi:ss')";
            }

            case SqlValueTypes.NotSupport: {
                string text3 = "??????X?t??" + _atrribute.Name + "?X?????X?t???t?X???X????";
                throw new DapperException(text3, ExceptionTypes.PesistentError);
            }
            }

            return text1;
        }

        private SqlValueTypes SqlValueType(Streaming rdb, DbType cType)
        {
            SqlValueTypes _SqlValueStringType = SqlValueTypes.PrototypeString;

            switch (cType) {
            case DbType.Boolean: {
                _SqlValueStringType = rdb.SqlValueType(DbType.Boolean);
                break;
            }

            case DbType.String: {
                _SqlValueStringType = SqlValueTypes.String;
                break;
            }

            case DbType.DateTime: {

                if (rdb.Vendor == "MsAccess") {
                    _SqlValueStringType = SqlValueTypes.AccessDateString;
                } else {
                    if (rdb.Vendor == "Oracle") {
                        _SqlValueStringType = SqlValueTypes.OracleDate;
                    } else {
                        _SqlValueStringType = SqlValueTypes.SimpleQuotesString;
                    }
                }

                break;
            }

            case DbType.Object: {
                _SqlValueStringType = SqlValueTypes.NotSupport;
                break;
            }
            }

            return _SqlValueStringType;
        }

        private string StringOfInCrtieria(Streaming rdb, object[] list)
        {
            string text1 = "";
            int num1 = 0;

            SqlValueTypes _SqlValueStringType = SqlValueType(rdb, _atrribute.Type);

            switch (_SqlValueStringType) {
            case SqlValueTypes.PrototypeString: {
                while (num1 < list.Length) {
                    if (num1 > 0) {
                        text1 = text1 + ",";
                    }

                    text1 = text1 + list[num1].ToString();
                    num1++;
                }

                return text1;
            }

            case SqlValueTypes.SimpleQuotesString: {
                while (num1 < list.Length) {
                    if (num1 > 0) {
                        text1 = text1 + ",";
                    }

                    text1 = text1 + "'" + list[num1].ToString() + "'";
                    num1++;
                }

                return text1;
            }

            case SqlValueTypes.String: {
                while (num1 < list.Length) {
                    if (num1 == 0) {
                        text1 = text1 + ",'";
                    }

                    text1 = text1 + "'" + list[num1].ToString().Replace("'", "''") + "'";
                    num1++;
                }

                return text1;
            }

            case SqlValueTypes.BoolToString:
            case SqlValueTypes.BoolToInterger:
            case SqlValueTypes.AccessDateString: {
                return text1;
            }

            case SqlValueTypes.NotSupport: {
                throw new DapperException("??????X?t??" + _atrribute.Name + "?X?????X?t???t?X???X????", ExceptionTypes.PesistentError);
            }

            default: {
                return text1;
            }
            }
        }

        // Properties
        public AttributeMapping Attribute { set { _atrribute = value; }  }
        public string AttributeName       { get { return _attrName;   }  }
        public object AttributeValue      { get { return _attrValue;  }  }
        public CriteriaOperator Type      { get { return _type;       }  }

        // Fields
        private AttributeMapping _atrribute;
        private AttributeMapping _attribute2;
        private bool   _isFieldToField;
        private string _attrName;
        private string _attrName2;
        private object _attrValue;
        private CriteriaOperator _type;
    }
}
