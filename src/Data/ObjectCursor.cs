using System;
using System.Data;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    internal class ObjectCursor {
        const string ZFILE_NAME = "ObjectCursor";
        // Methods

        internal ObjectCursor(string cDbName, Type classType, IDataReader oDataReader)
        {
            _broker      = ObjectBroker.Instance();
            _pointer     = 0;
            _DataReader  = oDataReader;
            _DbName      = cDbName;
            _typeOfClass = classType;
            string text1 = classType.ToString();
            _className   = text1.Substring(text1.LastIndexOf('.') + 1);
        }

        public bool HasObject()
        {
            return _DataReader.Read();
        }

        public EntityObject Next()
        {
            EntityObject _obj1 = null;

            try {
                _obj1 = _broker.GetEntityObject(_DbName, _typeOfClass, _className, _DataReader);
            } catch (IndexOutOfRangeException exception2) {
                ZZLogger.Error(ZFILE_NAME, exception2);
                throw;
            } catch (Exception exception3) {
                ZZLogger.Error(ZFILE_NAME, exception3);
                throw;
            }

            _pointer++;
            return _obj1;
        }

        // Fields
        private ObjectBroker _broker;
        private string _className;
        private string _DbName;
        private int _pointer;
        private IDataReader _DataReader;
        private Type _typeOfClass;
    }
}

