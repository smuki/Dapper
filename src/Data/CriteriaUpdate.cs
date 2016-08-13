using System;
using System.Collections;
using System.Collections.Generic;

namespace Volte.Data.Dapper
{
    public class CriteriaUpdate : CriteriaObject {
        const string ZFILE_NAME = "CriteriaUpdate";
        // Methods
        public CriteriaUpdate(Type classType)
        {
            _Conditions     = new List<Condition>();
            _ForUpdateList  = new Dictionary<string, object>();
            this.ClassType  = classType;
            this.ActionType = ActionTypes.CriteriaUpdate;
        }

        public void AddForUpdate(string attributeName, object attributeValue)
        {
            _ForUpdateList[attributeName] = attributeValue;
        }

        internal void BuildStringForUpdate(ClassMapping _cMp, Streaming rdb)
        {

            int num1       = 0;
            string cFields = "";
            string text1   = DapperUtil.BuildForConditions(_cMp, rdb, _Conditions);
            _sqlString     = "UPDATE ";
            _sqlString     = _sqlString + _cMp.TableName;
            _sqlString     = _sqlString + " SET ";

            foreach (KeyValuePair<string, object> kvp in _ForUpdateList) {
                if (cFields != "") {
                    cFields = cFields + ",";
                }

                cFields = cFields + kvp.Key + "=";
                cFields = cFields + rdb.GetStringParameter(kvp.Key, num1);
                num1++;
            }

            _sqlString = _sqlString + cFields;

            if (text1 != null) {
                _sqlString = _sqlString + " WHERE " + text1;
            }

        }

        public Condition NewCondition()
        {
            Condition condition1 = new Condition();
            _Conditions.Add(condition1);
            return condition1;
        }

        // Properties
        internal Dictionary<string, object> ForUpdateCollection
        {
            get {
                return _ForUpdateList;
            }
        }

        internal string SQLString(ClassMapping _cMp, Streaming rdb)
        {
            if (string.IsNullOrEmpty(_sqlString)) {
                this.BuildStringForUpdate(_cMp, rdb);
            }

            return _sqlString;
        }

        // Fields
        private List<Condition> _Conditions;
        private string _sqlString;
        private Dictionary<string, object> _ForUpdateList;
    }
}
