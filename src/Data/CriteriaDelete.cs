using System;
using System.Collections;
using System.Collections.Generic;


namespace Volte.Data.Dapper
{
    public class CriteriaDelete : CriteriaObject {
        const string ZFILE_NAME = "CriteriaDelete";
        // Methods
        public CriteriaDelete(Type classType)
        {
            _Conditions     = new List<Condition>();
            this.ClassType  = classType;
            this.ActionType = ActionTypes.CriteriaDelete;
        }

        internal void BuildStringForDelete(ClassMapping _classMapping, Streaming rdb)
        {
            _sqlString   = _classMapping.DeleteFromClause(rdb);
            string text1 = DapperUtil.BuildForConditions(_classMapping, rdb, _Conditions);

            if (!string.IsNullOrEmpty(text1)) {
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
        internal string SQLString(ClassMapping _cMp, Streaming rdb)
        {
            if (string.IsNullOrEmpty(_sqlString)) {
                this.BuildStringForDelete(_cMp, rdb);
            }

            return _sqlString;
        }

        // Fields
        private List<Condition> _Conditions;
        private string _sqlString;
    }
}

