using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    public class CriteriaRetrieve: CriteriaObject {
        const string ZFILE_NAME = "CriteriaRetrieve";
        // Methods
        public CriteriaRetrieve(Type classType)
        {
            _Conditions     = new List<Condition>();
            _OrderByList    = new List<OrderEntry>();
            _Top            = 0;
            _selectClause   = "";
            _selectString   = "SELECT ";
            _Streaming      = null;
            _selectfields   = new Dictionary<string, string>();
            this.ClassType  = classType;
            this.ActionType = ActionTypes.CriteriaRetrieve;
        }

        public void AddSelect(string attributeName)
        {
            _selectfields[attributeName] = attributeName;
        }

        public void AddSelect(string attributeName, string asName)
        {
            _selectfields[attributeName] = asName;
        }

        internal void BuildStringForRetrieve(ClassMapping _classMapping, Streaming rdb)
        {

            string _Where = DapperUtil.BuildForConditions(_classMapping, rdb, _Conditions);

            if (string.IsNullOrEmpty(_Where)) {
                string s = _classMapping.StringForInherit(rdb);

                if (!string.IsNullOrEmpty(s)) {
                    _Where = s;
                }
            }

            if (!string.IsNullOrEmpty(_Where)) {
                _Where = " WHERE " + _Where;
            }

            this.SELECTClause = this.GetSelectClause(_classMapping, rdb) + _Where;
            string _OrderBy   = this.GetOrderSql(_OrderByList, _classMapping, rdb);
            this.SELECTClause = this.SELECTClause + _OrderBy;

        }

        private string GetOrderSql(List<OrderEntry> _OrderByList, ClassMapping cm, Streaming rdb)
        {
            string text1 = "";

            for (int num1 = 0; num1 < _OrderByList.Count; num1++) {
                OrderEntry entry1 = _OrderByList[num1];
                AttributeMapping map1 = cm.AttributeMapping(entry1.AttributeName);

                if (text1 != "") {
                    text1 = text1 + ",";
                }

                string[] textArray1 = new string[7] { rdb.QuotationMarksStart, cm.TableName, rdb.QuotationMarksEnd, ".", rdb.QuotationMarksStart, map1.ColumnName, rdb.QuotationMarksEnd } ;

                if (entry1.IsAscend) {
                    text1 = text1 + string.Concat(textArray1) + " ASC";
                } else {
                    text1 = text1 + string.Concat(textArray1) + " DESC";
                }
            }

            if (text1 != "") {
                text1 = " ORDER BY " + text1;
            }

            return text1;
        }

        public void Clear()
        {
            _Conditions.Clear();
        }

        public Condition NewCondition()
        {
            Condition condition1 = new Condition();
            _Conditions.Add(condition1);
            return condition1;
        }

        private string GetSelectClause(ClassMapping _classMapping, Streaming rdb)
        {
            string text1 = "";

            if (_selectfields.Count > 0 && _selectClause == "") {
                string _QuotationMarksStart = " AS " + rdb.QuotationMarksStart;
                string _QuotationMarksEnd = rdb.QuotationMarksEnd;

                foreach (string text3 in _selectfields.Keys) {

                    AttributeMapping map1 = _classMapping.AttributeMapping(text3, true);
                    string text2 = _selectClause;
                    string[] textArray2 = new string[7] { rdb.QuotationMarksStart, _classMapping.TableName, rdb.QuotationMarksEnd, ".", rdb.QuotationMarksStart, map1.ColumnName, rdb.QuotationMarksEnd } ;

                    string[] textArray1 = new string[6] { text2, ",", string.Concat(textArray2) , _QuotationMarksStart, _selectfields[text3], _QuotationMarksEnd } ;
                    _selectClause = string.Concat(textArray1);
                }
            }

            if (_selectClause != "") {
                text1 = _selectString + _selectClause.Remove(0, 1);
                return text1 + " FROM " + _classMapping.TableName;
            }

            return _classMapping.SelectFromClause(rdb);
        }

        public void OrderBy(string attributeName)
        {
            this.OrderBy(attributeName, true);
        }

        public void OrderBy(string attributeName, bool isAsc)
        {
            OrderEntry  entry1 = new OrderEntry(attributeName, isAsc);

            if (_OrderByList == null) {
                _OrderByList = new List<OrderEntry>();
            }

            _OrderByList.Add(entry1);
        }


        internal string SQLString(ClassMapping _classMapping, Streaming rdb)
        {
            if (string.IsNullOrEmpty(this.SELECTClause)) {
                this.BuildStringForRetrieve(_classMapping, rdb);
            }

            return this.SELECTClause;
        }

        // Properties
        internal Streaming Streaming { get { return _Streaming; } set { _Streaming = value; }  }
        public int Top               { get { return _Top;       } set { _Top       = value; }  }

        // Fields
        private Dictionary<string, string> _selectfields;
        private List<Condition> _Conditions;
        private List<OrderEntry> _OrderByList;
        private Streaming _Streaming;
        private int    _Top;
        private string _selectClause;
        private string _selectString;
    }
}
