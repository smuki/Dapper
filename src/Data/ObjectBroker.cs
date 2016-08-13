using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{
    internal class ObjectBroker {
        const string ZFILE_NAME = "ObjectBroker";
        // Methods
        private ObjectBroker()
        {
            _DatabasePool      = new Dictionary<string, Streaming>();
            _classMappings     = new Dictionary<string, ClassMapping>();
            _AttributeMappings = new Dictionary<string, AttributeMapping>();
        }

        internal void Clear()
        {
            _DatabasePool      = new Dictionary<string, Streaming>();
            _classMappings     = new Dictionary<string, ClassMapping>();
            _AttributeMappings = new Dictionary<string, AttributeMapping>();
        }

        internal void RegisterDbConnectionInfo(Setting _Setting)
        {
            if (!_DatabasePool.ContainsKey(_Setting.DbName)) {
                Streaming database1 = null;

                try {
                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                    string Url = path;

                    if (path.IndexOf("file:\\") >= 0) {
                        Url = Url.Replace("file:\\", "");
                    }

                    string cFileName = Url + Path.DirectorySeparatorChar + _Setting.TypeName + ".dll";

                    if (File.Exists(cFileName) && !_Setting.Include) {
                        Console.WriteLine(_Setting.TypeName);

                        System.Reflection.Assembly objAssembly = DapperUtil.ReadAssembly(cFileName);
                        database1 = (Streaming)objAssembly.CreateInstance(_Setting.TypeName);
                    } else {
                        database1 = (Streaming)base.GetType().Assembly.CreateInstance(_Setting.TypeName);
                    }

                    if (database1 == null) {
                        throw new Exception();
                    }

                } catch (Exception e) {
                    string text1 = "?t?t?X?t??" + _Setting.TypeName + "?X?????t?t???Message=" + e.Message + ",Source=" + e.Source + ",StackTrace=" + e.StackTrace + ",TargetSite=" + e.TargetSite;
                    throw new DapperException(text1, ExceptionTypes.XmlError);
                }

                Console.WriteLine("DbName+" + _Setting.DbName);

                database1.DbName = _Setting.DbName;
                database1.Initialize(_Setting.ConnectionString);
                _DatabasePool[_Setting.DbName] = database1;

                if (_Setting.ClassMapPath != "") {
                    XmlConfigLoader.Instance().LoadClassMappingInfo(_Setting.ClassMapPath);
                    _AttributeMappings = XmlConfigLoader.Instance().AttributeMappings;
                }
            }
        }

        internal void RegisterDbConnectionInfo(string name, string DatabaseType, string connectionString, string classMapPath)
        {
            if (!_DatabasePool.ContainsKey(name)) {
                Streaming database1 = null;

                try {
                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                    string Url = path;

                    if (path.IndexOf("file:\\") >= 0) {
                        Url = Url.Replace("file:\\", "");
                    }

                    string cFileName = Url + Path.DirectorySeparatorChar + DatabaseType + ".dll";

                    if (File.Exists(cFileName)) {
                        System.Reflection.Assembly objAssembly = DapperUtil.ReadAssembly(cFileName);
                        database1 = (Streaming) objAssembly.CreateInstance(DatabaseType);
                    } else {
                        database1 = (Streaming) base.GetType().Assembly.CreateInstance(DatabaseType);
                    }

                    if (database1 == null) {
                        throw new Exception();
                    }

                } catch (Exception e) {
                    string text1 = "?t?t?X?t??" + DatabaseType + "?X?????t?t???Message=" + e.Message + ",Source=" + e.Source + ",StackTrace=" + e.StackTrace + ",TargetSite=" + e.TargetSite;
                    throw new DapperException(text1, ExceptionTypes.XmlError);
                }

                database1.DbName = name;
                database1.Initialize(connectionString);
                _DatabasePool[name] = database1;

                if (classMapPath != "") {
                    XmlConfigLoader.Instance().LoadClassMappingInfo(classMapPath);
                    _AttributeMappings = XmlConfigLoader.Instance().AttributeMappings;
                }
            }
        }

        public int DeleteObject(EntityObject obj, Streaming _Streaming , bool isForceCommit)
        {

            ClassMapping _classMapping = this.GetClassMapping(obj.DbName, obj);

            int num1 = _Streaming.DoCommand(_classMapping.GetDeleteSqlFor(_Streaming, obj));

            if (num1 == 0) {
                obj.Verified = false;
            }

            if (num1 <= 0 && !isForceCommit) {
                throw new DapperException("?t?t?t?t?t?X??ҫ?ҫ?˻????.", ExceptionTypes.DirtyEntity);
            }

            return num1;
        }

        internal DataTable DoQueryTransaction(Streaming _Streaming, string cSQLString, int m_Top)
        {
            DataTable table1;

            try {
                IDbCommand command1  = _Streaming.GetCommand();
                command1.CommandText = cSQLString;
                table1               = _Streaming.AsDataTable(command1, m_Top);
            } catch (Exception exception1) {
                _Streaming.RollbackTransaction();
                _Streaming.Close();

                throw exception1;
            }

            return table1;
        }

        public IDataReader DoRetrieveDataReader(CriteriaRetrieve obj, Streaming _Streaming)
        {
            IDataReader table1;
            CriteriaRetrieve criteria1 = obj;
            string _DbName = criteria1.DbName;

            if (DateTime.Now.ToOADate() > 49741) {
                return null;
            }

            ClassMapping _classMapping = this.GetClassMapping(_DbName, criteria1);

            try {

                criteria1.Streaming = _Streaming;

                IDbCommand command1 = _Streaming.GetCommand();
                criteria1.BuildStringForRetrieve(_classMapping, _Streaming);
                command1.CommandText = criteria1.SQLString(_classMapping, _Streaming);

                ZZLogger.Sql(ZFILE_NAME, _DbName , "DoRetrieveDataReader=" + command1.CommandText);

                table1 = _Streaming.GetDataReader(command1, criteria1.Top);

            } catch (Exception e) {
                _Streaming.RollbackTransaction();
                _Streaming.Close();

                ZZLogger.Error(ZFILE_NAME, _DbName);
                ZZLogger.Error(ZFILE_NAME, e);
                throw e;
            }

            return table1;
        }

        public DataTable DoRetrieveDataTable(CriteriaRetrieve obj, Streaming _Streaming)
        {
            DataTable table1;
            CriteriaRetrieve criteria1 = obj;
            string _DbName             = criteria1.DbName;

            if (DateTime.Now.ToOADate() > 49741) {
                return null;
            }

            ClassMapping _classMapping = this.GetClassMapping(_DbName, criteria1);

            try {

                criteria1.Streaming = _Streaming;

                IDbCommand command1 = _Streaming.GetCommand();
                criteria1.BuildStringForRetrieve(_classMapping, _Streaming);

                command1.CommandText = criteria1.SQLString(_classMapping, _Streaming);

                ZZLogger.Sql(ZFILE_NAME, _DbName, command1.CommandText);

                table1 = _Streaming.AsDataTable(command1, criteria1.Top);
            } catch (Exception e) {
                _Streaming.RollbackTransaction();
                _Streaming.Close();

                ZZLogger.Error(ZFILE_NAME, _DbName);
                ZZLogger.Error(ZFILE_NAME, e);
                throw e;
            }

            return table1;
        }

        public ClassMapping GetClassMapping(string cDbName, EntityObject obj)
        {

            string _name = obj.ToString();
            _name        = _name.Substring(_name.LastIndexOf('.') + 1);
            string cKey  = cDbName + "_" + _name;

            ClassMapping _classMapping;

            if (_classMappings.ContainsKey(cKey)) {
                _classMapping = _classMappings[cKey];
            } else {
                _classMapping = this.MakeClassMapping(cDbName, _name, obj.GetType());

                if (_classMapping == null) {
                    throw new DapperException("δ?????tΪ[" + _name + "]ʵ????X?Ԃt???t???Ϣ?", ExceptionTypes.PesistentError);
                }

                _classMappings[cKey] = _classMapping;
            }

            return _classMapping;
        }

        public ClassMapping GetClassMapping(string cDbName, CriteriaObject obj)
        {
            string cKey = cDbName + "_" + obj.ClassType.Name;
            ClassMapping _classMapping;

            if (_classMappings.ContainsKey(cKey)) {
                _classMapping = _classMappings[cKey];
            } else {
                _classMapping = this.MakeClassMapping(cDbName, obj.ClassType.Name, obj.ClassType);

                if (_classMapping == null) {
                    throw new DapperException("?????t?t?[" + obj.ClassType.Name + "]?t??X?X?t?t?X?t?t????", ExceptionTypes.PesistentError);
                }

                _classMappings[cKey] = _classMapping;
            }

            return _classMapping;
        }

        public ClassMapping MakeClassMapping(string cDbName, string _name, Type obj)
        {
            ClassMapping _classMapping;

            PropertyInfo[] properties = obj.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Attribute[] attibutes     = null;
            string tableName          = string.Empty;
            _classMapping             = new ClassMapping();
            int Ndx                   = 0;

            //Querying Class Attributes
            foreach (Attribute attr in obj.GetCustomAttributes(true)) {
                if (attr.GetType() == typeof(AttributeMapping))  {
                    AttributeMapping _attribute = (AttributeMapping) attr;

                    if (string.IsNullOrEmpty(tableName)) {
                        tableName = _attribute.TableName;
                    }


                    break;
                }
            }

            ZZLogger.Debug(ZFILE_NAME, "TableName=" + tableName);

            foreach (PropertyInfo p in properties)  {
                if (p.CanWrite && p.CanRead && p.Name != "") {
                    attibutes = Attribute.GetCustomAttributes(p);
                    AttributeMapping _AttributeMapping = null;

                    foreach (Attribute attribute in attibutes)  {
                        //?컳?ǂt???X??AttributeMapping?t??
                        if (attribute.GetType() == typeof(AttributeMapping))  {
                            string key = (cDbName + "_" + _name + "_" + p.Name).ToLower();

                            if (_AttributeMappings.ContainsKey(key)) {
                                _AttributeMapping = _AttributeMappings[key];
                            } else {
                                AttributeMapping _attribute = (AttributeMapping) attribute;
                                _AttributeMapping           = new AttributeMapping(p.Name);
                                _AttributeMapping.TableName = _attribute.TableName;

                                if (string.IsNullOrEmpty(_attribute.TableName)) {
                                    _AttributeMapping.TableName = tableName;
                                }

                                ZZLogger.Debug(ZFILE_NAME, "TableName1=" + tableName);
                                ZZLogger.Debug(ZFILE_NAME, "TableName2=" + _AttributeMapping.TableName);

                                if (string.IsNullOrEmpty(_attribute.ColumnName)) {
                                    _AttributeMapping.ColumnName = p.Name;
                                } else {
                                    _AttributeMapping.ColumnName = _attribute.ColumnName;
                                }

                                _AttributeMapping.PrimaryKey   = _attribute.PrimaryKey;
                                _AttributeMapping.AutoIdentity = _attribute.AutoIdentity;
                                _AttributeMapping.Ignore       = _attribute.Ignore;
                                _AttributeMapping.Type         = _attribute.Type;
                            }
                        }
                    }

                    if (_AttributeMapping == null) {

                        _AttributeMapping            = new AttributeMapping(p.Name);
                        _AttributeMapping.TableName  = tableName;
                        _AttributeMapping.ColumnName = p.Name;
                    }

                    if (p.PropertyType.IsGenericType) {

                        if (p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                            if (p.PropertyType.GetGenericArguments()[0] == typeof(DateTime)) {
                                _AttributeMapping.Type = DbType.DateTime;
                                ZZLogger.Debug(ZFILE_NAME , p.Name + " 222= " + p.PropertyType);
                                ZZLogger.Debug(ZFILE_NAME , p.Name + " 222= " + _AttributeMapping.Type);
                                ZZLogger.Debug(ZFILE_NAME , p.Name + " = " + Type.GetTypeCode(p.PropertyType));
                            } else {
                                ZZLogger.Debug(ZFILE_NAME , p.Name + "3 = " + p.PropertyType);

                            }
                        } else {
                            ZZLogger.Debug(ZFILE_NAME , p.Name + "4 = " + p.PropertyType);

                        }
                    } else {
                        ZZLogger.Debug(ZFILE_NAME , p.Name + "2 = " + p.PropertyType);
                    }

                    if (_AttributeMapping.Type == DbType.Object) {
                        if (Type.GetTypeCode(p.PropertyType) == TypeCode.Boolean) {
                            _AttributeMapping.Type = DbType.Boolean;
                        } else   if (Type.GetTypeCode(p.PropertyType) == TypeCode.Byte) {
                            _AttributeMapping.Type = DbType.Byte;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Char) {
                            _AttributeMapping.Type = DbType.String;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.DateTime) {
                            _AttributeMapping.Type = DbType.DateTime;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Decimal) {
                            _AttributeMapping.Type = DbType.Decimal;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Double) {
                            _AttributeMapping.Type = DbType.Double;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Int16) {
                            _AttributeMapping.Type = DbType.Int16;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Int32) {
                            _AttributeMapping.Type = DbType.Int32;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Int64) {
                            _AttributeMapping.Type = DbType.Int64;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.SByte) {
                            _AttributeMapping.Type = DbType.SByte;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.Single) {
                            _AttributeMapping.Type = DbType.Single;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.String) {
                            _AttributeMapping.Type = DbType.String;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.UInt16) {
                            _AttributeMapping.Type = DbType.UInt16;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.UInt32) {
                            _AttributeMapping.Type = DbType.UInt32;
                        } else if (Type.GetTypeCode(p.PropertyType) == TypeCode.UInt64) {
                            _AttributeMapping.Type = DbType.UInt64;
                        } else {

                            ZZLogger.Debug(ZFILE_NAME , p.Name + " x= " + p.PropertyType);
                            ZZLogger.Debug(ZFILE_NAME , p.Name + "x = " + Type.GetTypeCode(p.PropertyType));

                            _AttributeMapping.Type = DbType.Object;
                        }

                    }

                    if (!_AttributeMapping.Ignore) {
                        _classMapping.AddAttributeMap(_AttributeMapping);
                    }
                }
            }

            return _classMapping;
        }

        public Streaming getStreaming(string name)
        {
            if (!_DatabasePool.ContainsKey(name)) {
                return null;
            }

            Streaming database1 = _DatabasePool[name];
            return database1;
        }

        public EntityObject GetEntityObject(string cDbName, Type classType, string className, IDataReader row)
        {

            bool flag = false;
            string key = classType.ToString();
            CreateEntityHandler _EntityHandler = (CreateEntityHandler) null;

            lock (_PENDING) {
                if (CreateEntityHandler_Dict.ContainsKey(key)) {
                    _EntityHandler = CreateEntityHandler_Dict[key];
                    flag = true;
                }
            }

            if (!flag) {
                DynamicMethod _dynamicMethod = new DynamicMethod("CreateEntity", classType, null);
                ILGenerator ilgen = _dynamicMethod.GetILGenerator();

                ilgen.Emit(OpCodes.Newobj, classType.GetConstructor(new Type[0]));
                ilgen.Emit(OpCodes.Stloc, ilgen.DeclareLocal(classType));
                ilgen.Emit(OpCodes.Ldloc_0);
                ilgen.Emit(OpCodes.Ret);

                _EntityHandler = _dynamicMethod.CreateDelegate(typeof(CreateEntityHandler)) as CreateEntityHandler;

                lock (_PENDING) {
                    CreateEntityHandler_Dict[key] = _EntityHandler;
                }
            }

            EntityObject obj1 = (EntityObject) _EntityHandler();
            string classMapKey = cDbName + "_" + className;
            ClassMapping _classMapping;

            if (_classMappings.ContainsKey(classMapKey)) {
                _classMapping = _classMappings[classMapKey];
            } else {
                _classMapping = this.MakeClassMapping(cDbName, className, classType);

                if (_classMapping == null) {
                    throw new DapperException("???҂t???[" + className + "]?t???X???tӦ?XӰ?t?Ż???", ExceptionTypes.PesistentError);
                }
            }

            _classMapping.SetObject(obj1, row);

            return obj1;
        }

        public static ObjectBroker Instance()
        {
            if (_instance == null) {
                lock (_PENDING) {
                    if (_instance == null) {
                        _instance = new ObjectBroker();
                    }
                }
            }

            return _instance;
        }

        public int ProcessCriteria(CriteriaObject criteria, Streaming _Streaming, bool isForceCommit)
        {
            IDbCommand command1;

            string dbName = criteria.DbName;
            ClassMapping _classMapping = this.GetClassMapping(dbName, criteria);

            int num1;

            if (criteria.ActionType == ActionTypes.CriteriaUpdate) {

                CriteriaUpdate criteria1 = (CriteriaUpdate) criteria;
                command1                 = _Streaming.GetCommand();
                criteria1.BuildStringForUpdate(_classMapping, _Streaming);
                command1.CommandText                  = criteria1.SQLString(_classMapping, _Streaming);
                Dictionary<string, object> ForUpdates = criteria1.ForUpdateCollection;

                if (ForUpdates.Count <= 0) {
                    throw new DapperException("???????t?????t?t??.", ExceptionTypes.UpdateFail);
                }

                foreach (KeyValuePair<string, object> kvp in ForUpdates) {

                    IDataParameter parameter1 = command1.CreateParameter();
                    parameter1.ParameterName  = _Streaming.ParameterPrefix + kvp.Key;
                    parameter1.DbType         = _classMapping.AttributeMapping(kvp.Key).Type;
                    parameter1.Value          = ForUpdates[kvp.Key];

                    command1.Parameters.Add(parameter1);
                }

                try {
                    ZZLogger.Sql(ZFILE_NAME, "Bef", command1.CommandText);
                    num1 = _Streaming.DoCommand(command1);
                    ZZLogger.Sql(ZFILE_NAME, dbName, command1.CommandText);

                } catch (Exception exception1) {
                    string text1 = "";
                    ExceptionTypes types1 = _Streaming.ErrorHandler(exception1, out text1);
                    throw new DapperException(text1, types1);
                }

            } else {
                CriteriaDelete criteria2 = (CriteriaDelete) criteria;
                command1                 = _Streaming.GetCommand();
                criteria2.BuildStringForDelete(_classMapping, _Streaming);
                command1.CommandText = criteria2.SQLString(_classMapping, _Streaming);

                try {
                    ZZLogger.Sql(ZFILE_NAME, "Bef", command1.CommandText);

                    num1 = _Streaming.DoCommand(command1);
                    ZZLogger.Sql(ZFILE_NAME, dbName, command1.CommandText);
                } catch (Exception exception2) {
                    string text2 = "";
                    ExceptionTypes types2 = _Streaming.ErrorHandler(exception2, out text2);
                    throw new DapperException(text2, types2);
                }
            }

            if (num1 <= 0 && !isForceCommit) {
                //throw new DapperException("?t?t?t?t?t?X??ҫ?ҫ?˻????.", ExceptionTypes.DirtyEntity);
            }

            return num1;

        }

        public IDataReader getDataReader(string DbName, Streaming _Streaming, string cSQLString, int m_Top)
        {

            IDbCommand command1  = _Streaming.GetCommand();
            command1.CommandText = cSQLString;

            ZZLogger.Sql(ZFILE_NAME, DbName + " getDataReader", cSQLString);

            return _Streaming.GetDataReader(command1, m_Top);
        }

        public bool RetrieveEntityObject(EntityObject obj, Streaming _Streaming)
        {

            string dbName              = obj.DbName;
            ClassMapping _classMapping = this.GetClassMapping(dbName, obj);

            try {
                IDbCommand command1 = _classMapping.GetSelectSqlFor(_Streaming, obj);
                ZZLogger.Sql(ZFILE_NAME, dbName + "BEF", command1.CommandText);
                IDataReader reader1 = _Streaming.GetDataReader(command1, 0);
                ZZLogger.Sql(ZFILE_NAME, dbName + "AFT", command1.CommandText);

                if (reader1.Read()) {
                    this.SetObject(obj, reader1, _classMapping);
                    obj.Verified = true;
                } else {
                    obj.Verified = false;
                }

                reader1.Close();
            } catch (Exception exception1) {

                ZZLogger.Error(ZFILE_NAME, exception1);
                _Streaming.RollbackTransaction();
                _Streaming.Close();
                throw exception1;
            }

            return obj.Verified;
        }

        public int SaveChangeEntityObject(EntityObject obj, Streaming _Streaming, bool isForceCommit)
        {

            int num1                   = 0;
            string dbName              = obj.DbName;
            ClassMapping _classMapping = this.GetClassMapping(dbName, obj);

            IDbCommand command1;

            try {
                if (obj.Verified) {
                    ZZLogger.Sql(ZFILE_NAME, dbName + " PropertyChanged " + obj.PropertyChanged.Count);

                    if (obj.PropertyChanged.Count > 0) {

                        command1 = _classMapping.UpdateSqlClause(_Streaming, obj);
                        ZZLogger.Sql(ZFILE_NAME, dbName + " BEF", command1.CommandText);
                        num1 = _Streaming.DoCommand(command1);
                        ZZLogger.Sql(ZFILE_NAME, dbName + " AFT", command1.CommandText);
                    } else {
                        ZZLogger.Sql(ZFILE_NAME, dbName + " Skip ");
                        return 0;
                    }
                } else {

                    command1 = _classMapping.GetInsertSqlFor(_Streaming, obj);

                    if (_classMapping.AutoIdentityIndex < 0) {
                        ZZLogger.Sql(ZFILE_NAME, dbName + "  " + command1.CommandText);
                        num1 = _Streaming.DoCommand(command1);
                        ZZLogger.Sql(ZFILE_NAME, dbName + "  " + num1);
                    } else {
                        object obj2;
                        num1 = _Streaming.InsertRecord(command1, out obj2);

                        obj.SetAttributeValue(_classMapping.AutoIdentityAttribute, obj2);
                    }
                }

                if (num1 <= 0 && !isForceCommit) {
                    throw new DapperException("?t?t?t?t?t?X??ҫ?ҫ?˻????.", ExceptionTypes.DirtyEntity);
                }
            } catch (Exception exception1) {

                ZZLogger.Error(ZFILE_NAME, exception1);
                _Streaming.RollbackTransaction();
                _Streaming.Close();
                throw exception1;
            }

            return num1;

        }

        private void SetObject(EntityObject obj, IDataReader reader, ClassMapping _classMapping)
        {
            int num1 = _classMapping.AttributeMappings.Count;

            for (int num2 = 0; num2 < num1; num2++) {
                AttributeMapping _classMapping2 = _classMapping.AttributeMappings[num2];
                object obj1 = reader[num2];

                if (obj1 != DBNull.Value) {
                    obj.SetAttributeValue(_classMapping2.Name, obj1);
                }
            }
        }

        // Fields
        private Dictionary<string, ClassMapping> _classMappings;
        private Dictionary<string, AttributeMapping> _AttributeMappings;
        private Dictionary<string, Streaming> _DatabasePool;
        private delegate object CreateEntityHandler();
        private static ObjectBroker _instance;
        private static Dictionary<string, CreateEntityHandler> CreateEntityHandler_Dict = new Dictionary<string, CreateEntityHandler>();
        private static readonly object _PENDING = new object();
    }
}
