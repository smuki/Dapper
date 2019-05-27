using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Text;
using Volte.Data.Json;

namespace Volte.Data.Dapper
{
    internal class ClassMapping {
        const string ZFILE_NAME = "ClassMapping";
        // Fields
        private Dictionary<string, AttributeMapping> _hashAttributeMaps = new Dictionary<string, AttributeMapping>();

        private List<AttributeMapping> _AttributeMappings;
        private List<AttributeMapping> _KeyAttributeMappings;
        private CommanderDelete        _delete;
        private CommanderAddNew        _insert;
        private CommanderRetrieve      _select;
        private CommanderUpdate        _update;
        private AttributeMapping       _timestampAttribute;
        private string                 _autoIdentityAttribute;

        // Methods
        public ClassMapping()
        {
            _select    = null;
            _insert    = null;
            _update    = null;
            _delete    = null;
            _TableName = null;

            _hashAttributeMaps    = new Dictionary<string, AttributeMapping>();
            _AttributeMappings    = new List<AttributeMapping>();
            _KeyAttributeMappings = new List<AttributeMapping>();
        }

        public void AddAttributeMap(AttributeMapping attribute)
        {
            _hashAttributeMaps[attribute.Name] = attribute;

            if (string.IsNullOrEmpty(_TableName) && !string.IsNullOrEmpty(attribute.TableName)) {
                _TableName = attribute.TableName;
            }

            if (attribute.AutoIdentity) {
                AutoIdentityAttribute = attribute.Name;
                AutoIdentityIndex++;
            }

            if (attribute.PrimaryKey) {
                this.PrimaryKeyIndex++;
                _KeyAttributeMappings.Add(attribute);
                _AttributeMappings.Insert(this.PrimaryKeyIndex, attribute);
            } else {
                _AttributeMappings.Add(attribute);
            }

            if (attribute.Timestamp) {
                this.TimestampAttribute = attribute;
            }
        }

        public AttributeMapping AttributeMapping(int index)
        {
            return _AttributeMappings[index];
        }

        public AttributeMapping AttributeMapping(string name)
        {
            return this.AttributeMapping(name, false);
        }

        public AttributeMapping AttributeMapping(string name, bool isSuperClassInc)
        {
            AttributeMapping map1 = _hashAttributeMaps[name];

            if (map1 == null) {
                string xException = " " + name + "";
                throw new DapperException(xException);
            }

            return map1;
        }
        public IDbCommand GetDeleteSqlFor(Streaming _Streaming, EntityObject obj)
        {
            if (_delete == null) {
                _delete = new CommanderDelete(_Streaming, this);
            }

            return _delete.BuildForObject(_Streaming, obj);
        }
        public IDbCommand GetInsertSqlFor(Streaming _Streaming, EntityObject obj)
        {
            if (_insert == null) {
                _insert = new CommanderAddNew(_Streaming, this);
            }

            return _insert.BuildForObject(_Streaming, obj);
        }

        public AttributeMapping KeyAttributeMapping(int index)
        {
            return _KeyAttributeMappings[index];
        }

        public int GetKeySize()
        {
            return _KeyAttributeMappings.Count;
        }

        public IDbCommand GetSelectSqlFor(Streaming _Streaming, EntityObject obj)
        {
            if (_select == null) {
                _select = new CommanderRetrieve(_Streaming, this);
            }

            return _select.BuildForObject(_Streaming, obj);
        }
        public int GetSize()
        {
            return _AttributeMappings.Count;
        }
        public IDbCommand UpdateSqlClause(Streaming _Streaming, EntityObject obj)
        {
            if (_update == null) {
                _update = new CommanderUpdate(_Streaming, this);
            }

            return _update.BuildForObject(_Streaming, obj);
        }

        internal void SetObject(EntityObject obj, IDataReader row)
        {
            AttributeMapping map1;
            obj.Verified = true;
            int num1     = this.AttributeMappings.Count;

            if (row.FieldCount < num1) {
                for (int i = 0; i < row.FieldCount; i++) {
                    if (_hashAttributeMaps.ContainsKey(row.GetName(i))) {
                        map1 = _hashAttributeMaps[row.GetName(i)];
                        object oValue = row[i];

                        if (!DBNull.Value.Equals(oValue)) {
                            obj.SetValue(map1.Name, oValue);
                        }
                    }
                }
            } else {

                for (int num3 = 0; num3 < num1; num3++) {
                    map1 = this.AttributeMappings[num3];

                    if (!DBNull.Value.Equals(row[num3])) {
                        obj.SetValue(map1.Name, row[num3]);
                    }
                }
            }
        }
        public string DeleteFromClause(Streaming _Streaming)
        {
            if (_delete == null) {
                _delete = new CommanderDelete(_Streaming, this);
            }

            return _delete.DeleteClause;
        }

        public string SelectFromClause(Streaming _Streaming)
        {
            if (_select == null) {
                _select = new CommanderRetrieve(_Streaming, this);
            }

            return _select.SelectClause;
        }

        public string StringForInherit(Streaming _Streaming)
        {
            if (_select == null) {
                _select = new CommanderRetrieve(_Streaming, this);
            }

            return _select.StringForInherit;
        }

        public string TableName                    { get { return _TableName;             } set { _TableName             = value; }  }
        public string AutoIdentityAttribute        { get { return _autoIdentityAttribute; } set { _autoIdentityAttribute = value; }  }
        public int AutoIdentityIndex               { get { return _AutoIdentityIndex;     } set { _AutoIdentityIndex     = value; }  }
        public int PrimaryKeyIndex                 { get { return _PrimaryKeyIndex;       } set { _PrimaryKeyIndex       = value; }  }
        public AttributeMapping TimestampAttribute { get { return _timestampAttribute;    } set { _timestampAttribute    = value; }  }

        public List<AttributeMapping> AttributeMappings    { get { return _AttributeMappings;    }  }
        public List<AttributeMapping> KeyAttributeMappings { get { return _KeyAttributeMappings; }  }

        public Type ClassType
        {
            get;
            set;
        }

        public string ClassName
        {
            get;
            set;
        }
        private int _AutoIdentityIndex = -1;
        private int _PrimaryKeyIndex   = -1;
        private string _TableName;

    }
}
