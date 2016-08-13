using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    [Serializable]
    public abstract class EntityObject: ICloneable {
        const string ZFILE_NAME = "EntityObject";

        // Methods
        public EntityObject()
        {
            _Verified = false;
            _thisType = base.GetType();
        }

        internal object GetAttributeValue(string name)
        {
            object obj1 = null;

            try {
                obj1 = _thisType.GetProperty(name).GetValue(this, null);
            } catch (Exception exception1) {
                ZZLogger.Error(ZFILE_NAME, exception1, name);
                throw new DapperException("不能找到" + this.ToString() + "对象的[" + name + "]属性!", ExceptionTypes.NotFound);
            }

            return obj1;
        }

        internal void SetAttributeValue(string name, object objValue)
        {
            try {
                var property = _thisType.GetType().GetProperty(name);

                if (property != null) {
                    Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    object safeValue = (objValue == null) ? null : Convert.ChangeType(objValue , t);

                    property.SetValue(_thisType , safeValue , null);
                }

            } catch (Exception exception1) {
                ZZLogger.Error(ZFILE_NAME, exception1, name);
                throw exception1;
            }
        }

        public object GetValue(string name)
        {
            object obj1 = null;

            try {

                obj1 = _thisType.GetProperty(name).GetValue(this, null);
            } catch (Exception exception1) {
                ZZLogger.Error(ZFILE_NAME, exception1, name);
                ZZLogger.Error(ZFILE_NAME, exception1.ToString());
                throw new DapperException(this.ToString() + " 's [" + name + "] Property Not Found!", ExceptionTypes.NotFound);
            }

            return obj1;
        }

        public DateTime GetDateTime(string name)
        {
            object _obj = GetValue(name);

            return Convert.ToDateTime(_obj);
        }

        public decimal GetDecimal(string name)
        {
            object _obj = GetValue(name);

            return Convert.ToDecimal(_obj);
        }

        public void SetValue(string name, object objValue)
        {
            try {

                Type pType = _thisType.GetProperty(name).PropertyType;

                if ((pType.IsGenericType) && (pType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    pType = pType.GetGenericArguments()[0];
                }

                _thisType.GetProperty(name).SetValue(this , Convert.ChangeType(objValue , pType) , null);
            } catch (Exception exception1) {
                ZZLogger.Error(ZFILE_NAME, exception1, name);
                throw exception1;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public object DeepClone()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            ms.Seek(0, SeekOrigin.Begin);
            return bf.Deserialize(ms);
        }

        public void PropertyChange(string propertyName)
        {
            _PropertyChanged[propertyName] = true;
        }

        // Properties
        [AttributeMapping(Ignore = true)]
        public string DbName
        {
            get {
                return _dbName;
            } set {
                _dbName = value;
            }
        }

        [AttributeMapping(Ignore = true)]
        public bool Verified
        {
            get {
                return _Verified;
            } set {
                _Verified = value;
                _PropertyChanged = new Dictionary<string, bool>();
            }
        }

        [AttributeMapping(Ignore = true)]
        public Dictionary<string, bool> PropertyChanged
        {
            get {
                return _PropertyChanged;
            }
        }

        // Fields
        private Dictionary<string, bool> _PropertyChanged = new Dictionary<string, bool>();
        private string _dbName;
        private bool   _Verified;
        private Type   _thisType;
    }
}
