using System;
using System.Collections;
using System.Collections.Generic;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    public class Condition {
        const string ZFILE_NAME = "Condition";

        // Methods
        internal Condition()
        {
            _Parameters      = new List<Criteria>();
            _Conditions      = new List<Condition>();
            _BooleanOperator = " AND ";
        }

        internal Condition(string cBooleanOperator)
        {
            _Parameters      = new List<Criteria>();
            _Conditions      = new List<Condition>();
            _BooleanOperator = cBooleanOperator;
        }

        //======   Equals   ===
        public void Equals(string _Name, object _Value)
        {
            AddCondition(_Name, CriteriaOperator.Equals, _Value);
        }

        public void EqualsField(string _Name, string _Name2)
        {
            AddCondition(_Name, CriteriaOperator.Equals, _Name2, true);
        }

        //======   NotEquals   ===
        public void NotEquals(string _Name, object _Value)
        {
            AddCondition(_Name, CriteriaOperator.NotEquals, _Value);
        }

        public void NotEqualsField(string _Name, string _Name2)
        {
            AddCondition(_Name, CriteriaOperator.NotEquals, _Name2, true);
        }

        //====  GreaterThan ===
        public void GreaterThan(string _Name, object _Value)
        {
            AddCondition(_Name, CriteriaOperator.GreaterThan, _Value);
        }

        public void GreaterThanField(string _Name, string _Name2)
        {
            AddCondition(_Name, CriteriaOperator.GreaterThanOrEquals, _Name2, true);
        }

        //====  GreaterThanOrEquals ===
        public void GreaterThanOrEquals(string _Name, object _Value)
        {
            AddCondition(_Name, CriteriaOperator.GreaterThanOrEquals, _Value);
        }

        public void GreaterThanOrEqualsField(string _Name, string _Name2)
        {
            AddCondition(_Name, CriteriaOperator.GreaterThanOrEquals, _Name2, true);
        }

        //==== In
        public void In(string _Name, object[] list)
        {
            if ((list == null) || (list.Length == 0)) {
                throw new DapperException("??t?Xlist??t???0?");
            }

            AddCondition(_Name, CriteriaOperator.In, list);
        }

        public void In(string _Name, string XString)
        {
            this.In(_Name, XString.Split((new char[3] { ';', ',', '|' })));
        }

        //===== LessThan
        public void LessThan(string _Name, object _Value)
        {
            AddCondition(_Name, CriteriaOperator.LessThan, _Value);
        }

        public void LessThanField(string _Name, string _Name2)
        {
            AddCondition(_Name, CriteriaOperator.LessThan, _Name2, true);
        }

        //==== LessThanOrEquals
        public void LessThanOrEquals(string _Name, object _Value)
        {
            AddCondition(_Name, CriteriaOperator.LessThanOrEquals, _Value);
        }

        public void LessThanOrEqualsField(string _Name, string _Name2)
        {
            AddCondition(_Name, CriteriaOperator.LessThanOrEquals, _Name2, true);
        }

        //==== Like
        public void Like(string _Name, string _Value)
        {
            _Value = "%" + _Value + "%";

            AddCondition(_Name, CriteriaOperator.Like, _Value);
        }

        public void Expression(string _Name, string cValue)
        {
            if (_Name.IndexOf('.') > 0) {
                string[] s = _Name.Split(new char[] { '.' });
                _Name = s[1];
            }

            if (cValue == null) {
                cValue = "";
            }

            if (cValue == "") {
            } else {
                string cExp = cValue.Substring(0, 1);

                if (cExp == ">" || cExp == "<" || cExp == "=") {
                    if (cValue.Substring(0, 2) == ">=") {
                        cValue = cValue.Replace(">=", "");
                        AddCondition(_Name, CriteriaOperator.GreaterThanOrEquals, cValue);
                    } else if (cValue.Substring(0, 2) == "<=") {
                        cValue = cValue.Replace("<=", "");
                        AddCondition(_Name, CriteriaOperator.LessThanOrEquals, cValue);
                    } else if (cValue.Substring(0, 2) == "<>") {
                        cValue = cValue.Replace("<>", "");
                        AddCondition(_Name, CriteriaOperator.NotEquals, cValue);
                    } else if (cValue.Substring(0, 1) == ">") {
                        cValue = cValue.Replace(">", "");
                        AddCondition(_Name, CriteriaOperator.GreaterThan, cValue);
                    } else if (cValue.Substring(0, 1) == "<") {
                        cValue = cValue.Replace("<", "");
                        AddCondition(_Name, CriteriaOperator.LessThan, cValue);
                    } else {
                        cValue = cValue.Replace("=", "");
                        AddCondition(_Name, CriteriaOperator.Equals, cValue);
                    }
                } else if (cValue.IndexOf('*') >= 0) {
                    AddCondition(_Name, CriteriaOperator.Like, cValue.Replace("*", "%"));
                } else {
                    AddCondition(_Name, CriteriaOperator.Like, cValue + "%");
                }
            }
        }

        public void Prefix(string _Name, string _Value)
        {
            _Value = _Value + "%";
            AddCondition(_Name, CriteriaOperator.Like, _Value);
        }

        public void Suffix(string _Name, string _Value)
        {
            _Value = "%" + _Value;
            AddCondition(_Name, CriteriaOperator.Like, _Value);
        }

        public void NotIn(string _Name, object[] list)
        {
            if ((list == null) || (list.Length == 0)) {
                throw new DapperException("???t??list???????0?");
            }

            AddCondition(_Name, CriteriaOperator.NotIn, list);
        }

        public void NotLike(string _Name, string _Value)
        {
            _Value = "%" + _Value + "%";
            AddCondition(_Name, CriteriaOperator.NotLike, _Value);
        }

        public void NotPrefix(string _Name, string _Value)
        {
            _Value = _Value + "%";

            AddCondition(_Name, CriteriaOperator.NotLike, _Value);
        }

        public void NotSuffix(string _Name, string _Value)
        {
            _Value = "%" + _Value;
            AddCondition(_Name, CriteriaOperator.NotLike, _Value);
        }

        public void Where(string _Value)
        {
            Criteria criteria1 = new Criteria(CriteriaOperator.Where , "" , _Value);
            _Parameters.Add(criteria1);
        }

        private void AddCondition(string _Name, CriteriaOperator _Operator, object _Value)
        {
            Criteria criteria1 = new Criteria(_Operator, _Name, _Value);
            _Parameters.Add(criteria1);
        }

        private void AddCondition(string cName, CriteriaOperator _Operator, string cValue, bool field)
        {
            Criteria criteria1 = new Criteria(_Operator, cName, cValue, true);
            _Parameters.Add(criteria1);
        }

        public void Clear()
        {
            _Parameters.Clear();
        }

        // Properties
        public Condition NewCondition()
        {
            return this.NewCondition(" OR ");
        }

        public Condition NewCondition(string BooleanOperator)
        {
            Condition group1 = new Condition(BooleanOperator);
            _Conditions.Add(group1);
            return group1;
        }

        internal string BooleanOperator     { get { return _BooleanOperator; }  }
        internal List<Criteria> Parameters  { get { return _Parameters;      }  }
        internal List<Condition> Conditions { get { return _Conditions;      }  }

        // Fields
        private string _BooleanOperator;
        private List<Criteria> _Parameters  = new List<Criteria>();
        private List<Condition> _Conditions = new List<Condition>();
    }
}

