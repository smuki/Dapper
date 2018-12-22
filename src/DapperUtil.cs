using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Data;

using Volte.Data.Json;
using Volte.Utils;

namespace Volte.Data.Dapper
{

    public class DapperUtil {
        const string ZFILE_NAME = "DapperUtil";

        private static readonly object _PENDING                = new object();
        private static Dictionary<string, Assembly> oAssembly  = new Dictionary<string, Assembly>();
        private static Dictionary<string, DateTime> oZDateTime = new Dictionary<string, DateTime>();
        private static string cDbName = "_";
        private static Dictionary<string, ClassMapping> _ClassMappingCache = new Dictionary<string, ClassMapping>();
        private static Dictionary<string, AttributeMapping> _AttributeMappings = new Dictionary<string, AttributeMapping>();

        public DapperUtil()
        {

        }

        public static System.Reflection.Assembly ReadAssembly(string _AssemblyFile)
        {
            lock (_PENDING) {
                System.Reflection.Assembly _Assembly = null;

                if (File.Exists(_AssemblyFile)) {
                    DateTime _DateTime = System.IO.File.GetLastWriteTime(_AssemblyFile);
                    bool     _Lastest  = true;

                    if (oZDateTime.ContainsKey(_AssemblyFile)) {
                        if (!_DateTime.Equals(oZDateTime[_AssemblyFile])) {
                            _Lastest = false;
                        }
                    }

                    if (_Lastest && oAssembly.ContainsKey(_AssemblyFile)) {
                        return oAssembly[_AssemblyFile];
                    }

                    byte[] fileContent = new byte[0];
                    using(FileStream dll = File.OpenRead(_AssemblyFile)) {
                        fileContent = new byte[dll.Length];
                        ZZLogger.Debug(ZFILE_NAME, dll.Length);
                        dll.Read(fileContent, 0, (int)dll.Length);
                    }

                    string _PdbFileName = _AssemblyFile.Replace(".dll", ".pdb");

                    if (System.IO.File.Exists(_PdbFileName)) {

                        byte[] PdbContent = new byte[0];
                        using(FileStream pdb = File.OpenRead(_PdbFileName)) {
                            PdbContent = new byte[pdb.Length];
                            ZZLogger.Debug(ZFILE_NAME, pdb.Length);
                            pdb.Read(PdbContent, 0, (int)pdb.Length);

                        }

                        _Assembly = System.Reflection.Assembly.Load(fileContent, PdbContent);

                    } else {
                        _Assembly = System.Reflection.Assembly.Load(fileContent);
                    }

                    oAssembly[_AssemblyFile]  = _Assembly;
                    oZDateTime[_AssemblyFile] = _DateTime;

                }

                return _Assembly;
            }
        }

        public static bool IfHasTable(DbContext trans, string tableName)
        {
            QueryRows _sysobjects = new QueryRows(trans);
            _sysobjects.CommandText = "select * from sysobjects where name = '" + tableName + "'";
            _sysobjects.Open();

            return !_sysobjects.EOF;
        }

        public static bool IsNullOrEmpty(decimal cValue)
        {
            return cValue == 0;
        }

        public static bool IsNullOrEmpty(Nullable<DateTime> cValue)
        {
            return !cValue.HasValue;
        }

        public static bool IsNullOrEmpty(DateTime cValue)
        {
            return cValue == Util.DateTime_MinValue;
        }

        public static bool IsNullOrEmpty(string cValue)
        {
            return string.IsNullOrEmpty(cValue);
        }

        public static bool IsNullOrEmpty(bool cValue)
        {
            return false;
        }

        public static bool IsBoolean(string oValue)
        {
            if (string.IsNullOrEmpty(oValue)) {
                oValue = "";
            }

            oValue = oValue.ToUpper();

            if (oValue.Equals("CHECKED")) {
                return true;;
            }

            if (oValue.Equals("Y")) {
                return true;;
            }

            if (oValue.Equals("N")) {
                return true;;
            }

            if (oValue.Equals("YES")) {
                return true;;
            }

            if (oValue.Equals("NO")) {
                return true;;
            }

            if (oValue.Equals("TRUE")) {
                return true;;
            }

            if (oValue.Equals("FALSE")) {
                return true;;
            }

            return false;;
        }

        public static bool IsDateTime(string sdate, string dateFormatWithSeperator)
        {

            DateTime dt;

            if (dateFormatWithSeperator != "") {
                if (DateTime.TryParseExact(sdate, dateFormatWithSeperator, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt)) {
                    return true;
                }
            }

            string[] formats = {"yyyy-MM-dd", "MM-dd-yyyy", "yyyy/MM/dd", "MM/dd/yyyy"};

            if (DateTime.TryParseExact(sdate, formats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt)) {
                return true;
            }

            return false;
        }


        public static bool IsDateTime(string sdate)
        {
            return IsDateTime(sdate, "");
        }

        public static bool IsNumeric(object str)
        {
            decimal d;
            return decimal.TryParse(str.ToString(), out d);
        }

        public static bool ToBoolean(object cValue)
        {
            bool d;
            return bool.TryParse(Convert.ToString(cValue), out d) ? d : false;
        }

        public static decimal ToDecimal(object cValue)
        {
            decimal d;
            return decimal.TryParse(Convert.ToString(cValue), out d) ? d : 0M;
        }

        public static double ToDouble(object cValue)
        {
            double d;
            return double.TryParse(Convert.ToString(cValue), out d) ? d : 0;
        }

        public static int ToInt(object oValue)
        {
            return DapperUtil.ToInt32(oValue);
        }

        public static int ToInt32(object oValue)
        {
            int d;
            return int.TryParse(oValue.ToString(), out d) ? d : 0;
        }

        public static long ToLong(object oValue)
        {
            long d;
            return long.TryParse(Convert.ToString(oValue), out d) ? d : 0;
        }

        public static int Length(object str)
        {
            return Encoding.Default.GetBytes(str.ToString()).Length;
        }

        public static DateTime ZZDateTime
        {
            get {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }
        }

        public static string AntiSQLInjection(string inputString)
        {

            if (string.IsNullOrEmpty(inputString)) {
                return "";
            } else {
                StringBuilder sb = new StringBuilder();
                char[] old = inputString.ToCharArray();

                for (int i = 0; i < old.Length; i++) {
                    char c = old[i];

                    switch (c) {
                        case '\'':
                        case '\\':
                        case ';': {
                                      break;
                                  }

                        default: {
                                     sb.Append(c);
                                     break;
                                 }
                    }
                }

                return sb.ToString();
            }
        }

        public static string Compress(string str)
        {
            return Util.Base64UrlEncodeByte(Compress(Encoding.UTF8.GetBytes(str)));
        }

        public static string Decompress(string str)
        {
            return Encoding.UTF8.GetString(Decompress(Util.Base64UrlDecodeByte(str)));
        }

        internal static string BuildForConditions(ClassMapping _classMapping, Streaming rdb, List<Condition> conditions)
        {
            string cWhereClause = "";

            foreach (Condition condition1 in conditions) {
                string cClause = GetConditionUnit(_classMapping, rdb, condition1);

                if (!string.IsNullOrEmpty(cClause)) {
                    if (string.IsNullOrEmpty(cWhereClause))                 {
                        cWhereClause = cClause;
                    } else {
                        cWhereClause = cWhereClause + condition1.BooleanOperator;
                        cWhereClause = cWhereClause + "(" + cClause + ")";
                    }
                }
            }

            return cWhereClause;
        }

        private static string GetConditionUnit(ClassMapping _classMapping, Streaming rdb, Condition condition1)
        {
            string cWhereClause = "";

            foreach (Criteria criteria1 in condition1.Parameters) {
                if (cWhereClause != "") {
                    cWhereClause = cWhereClause + condition1.BooleanOperator;
                }

                cWhereClause = cWhereClause + criteria1.AsSqlClause(_classMapping, rdb);
            }

            if (condition1.Conditions != null) {
                foreach (Condition condition2 in condition1.Conditions) {
                    string cClause = GetConditionUnit(_classMapping, rdb, condition2);

                    if (!string.IsNullOrEmpty(cClause)) {
                        if (string.IsNullOrEmpty(cWhereClause)) {
                            cWhereClause = "(" + cClause + ")";
                        } else {
                            cWhereClause = cWhereClause + condition2.BooleanOperator;
                            cWhereClause = cWhereClause + "(" + cClause + ")";
                        }
                    }
                }
            }

            return cWhereClause;
        }

        public static void CompressFile(string fileName, string toFileName)
        {

            FileStream _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            long length = _fileStream.Length;
            byte[] temp = new byte[length];

            _fileStream.Read(temp, 0, (int)length);
            _fileStream.Close();

            ZZLogger.Debug(ZFILE_NAME, length);

            System.IO.File.WriteAllBytes(toFileName, Compress(temp));

        }

        public static void DecompressFile(string fileName, string toFileName)
        {

            FileStream _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            long length = _fileStream.Length;
            byte[] temp = new byte[length];

            _fileStream.Read(temp, 0, (int)length);
            _fileStream.Close();

            ZZLogger.Debug(ZFILE_NAME, length);

            System.IO.File.WriteAllBytes(toFileName, Decompress(temp));

        }

        public static byte[] Compress(byte[] bytes)
        {
            using(MemoryStream ms = new MemoryStream()) {
                GZipStream Compress = new GZipStream(ms, CompressionMode.Compress);
                Compress.Write(bytes, 0, bytes.Length);
                Compress.Close();
                return ms.ToArray();

            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using(MemoryStream tempMs = new MemoryStream()) {
                using(MemoryStream ms = new MemoryStream(bytes)) {
                    GZipStream Decompress = new GZipStream(ms, CompressionMode.Decompress);
                    Decompress.CopyTo(tempMs);
                    Decompress.Close();
                    return tempMs.ToArray();
                }
            }

        }

        private object CopyObject(object obj) {
            if (obj == null) {
                return null;
            }
            Object targetDeepCopyObj;
            Type targetType = obj.GetType();
            if (targetType.IsValueType == true) {
                targetDeepCopyObj = obj;
            } else {
                targetDeepCopyObj = System.Activator.CreateInstance(targetType);
                System.Reflection.MemberInfo[] memberCollection = obj.GetType().GetMembers();
                foreach (System.Reflection.MemberInfo member in memberCollection) {
                    if (member.MemberType == System.Reflection.MemberTypes.Field) {
                        System.Reflection.FieldInfo field = (System.Reflection.FieldInfo)member;
                        Object fieldValue = field.GetValue(obj);
                        if (fieldValue is ICloneable) {
                            field.SetValue(targetDeepCopyObj, (fieldValue as ICloneable).Clone());
                        } else {
                            field.SetValue(targetDeepCopyObj, CopyObject(fieldValue));
                        }
                    } else if (member.MemberType == System.Reflection.MemberTypes.Property) {
                        System.Reflection.PropertyInfo myProperty = (System.Reflection.PropertyInfo)member;
                        MethodInfo info = myProperty.GetSetMethod(false);
                        if (info != null) {
                            try {
                                object propertyValue = myProperty.GetValue(obj, null);
                                if (propertyValue is ICloneable) {
                                    myProperty.SetValue(targetDeepCopyObj, (propertyValue as ICloneable).Clone(), null);
                                } else {
                                    myProperty.SetValue(targetDeepCopyObj, CopyObject(propertyValue), null);
                                }
                            } catch (System.Exception ex) {
                            }
                        }
                    }
                }
            }
            return targetDeepCopyObj;
        }
        public static string Base64Encode(string Message)
        {
            char[] Base64Code = new char[] {'A' , 'B' , 'C' , 'D' , 'E' , 'F' , 'G' , 'H' , 'I' , 'J' , 'K' , 'L' , 'M' , 'N' , 'O' , 'P' , 'Q' , 'R' , 'S' , 'T' , 'U' , 'V' , 'W' , 'X' , 'Y' , 'Z' , 'a' , 'b' , 'c' , 'd' , 'e' , 'f' , 'g' , 'h' , 'i' , 'j' , 'k' , 'l' , 'm' , 'n' , 'o' , 'p' , 'q' , 'r' , 's' , 't' , 'u' , 'v' , 'w' , 'x' , 'y' , 'z' , '0' , '1' , '2' , '3' , '4' , '5' , '6' , '7' , '8' , '9' , '+' , '/' , '=' };

            byte empty = (byte)0;
            System.Collections.ArrayList byteMessage = new System.Collections.ArrayList(System.Text.Encoding.Default.GetBytes(Message));
            System.Text.StringBuilder outmessage;
            int messageLen = byteMessage.Count;
            int page = messageLen / 3;
            int use = 0;

            if ((use = messageLen % 3) > 0) {
                for (int i = 0; i < 3 - use; i++)
                    byteMessage.Add(empty);

                page++;
            }

            outmessage = new System.Text.StringBuilder(page * 4);

            for (int i = 0; i < page; i++) {
                byte[] instr = new byte[3];
                instr[0] = (byte)byteMessage[i * 3];
                instr[1] = (byte)byteMessage[i * 3 + 1];
                instr[2] = (byte)byteMessage[i * 3 + 2];
                int[] outstr = new int[4];
                outstr[0] = instr[0] >> 2;
                outstr[1] = ((instr[0] & 0x03) << 4) ^ (instr[1] >> 4);

                if (!instr[1].Equals(empty)) {
                    outstr[2] = ((instr[1] & 0x0f) << 2) ^ (instr[2] >> 6);
                } else {
                    outstr[2] = 64;
                }

                if (!instr[2].Equals(empty)) {
                    outstr[3] = instr[2] & 0x3f;
                } else {
                    outstr[3] = 64;
                }

                outmessage.Append(Base64Code[outstr[0]]);
                outmessage.Append(Base64Code[outstr[1]]);
                outmessage.Append(Base64Code[outstr[2]]);
                outmessage.Append(Base64Code[outstr[3]]);
            }

            return outmessage.ToString();
        }

        public static string Base64Decode(string Message)
        {
            if ((Message.Length % 4) != 0) {
                throw new ArgumentException("Invalid BASE64", "Message");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Message, "^[A-Z0-9/+=]*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
                throw new ArgumentException("Invalid BASE64", "Message");
            }

            string Base64Code = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            int page = Message.Length / 4;
            System.Collections.ArrayList outMessage = new System.Collections.ArrayList(page * 3);
            char[] message = Message.ToCharArray();

            for (int i = 0; i < page; i++) {
                byte[] instr = new byte[4];
                instr[0] = (byte)Base64Code.IndexOf(message[i * 4]);
                instr[1] = (byte)Base64Code.IndexOf(message[i * 4 + 1]);
                instr[2] = (byte)Base64Code.IndexOf(message[i * 4 + 2]);
                instr[3] = (byte)Base64Code.IndexOf(message[i * 4 + 3]);
                byte[] outstr = new byte[3];
                outstr[0] = (byte)((instr[0] << 2) ^ ((instr[1] & 0x30) >> 4));

                if (instr[2] != 64) {
                    outstr[1] = (byte)((instr[1] << 4) ^ ((instr[2] & 0x3c) >> 2));
                } else {
                    outstr[2] = 0;
                }

                if (instr[3] != 64) {
                    outstr[2] = (byte)((instr[2] << 6) ^ instr[3]);
                } else {
                    outstr[2] = 0;
                }

                outMessage.Add(outstr[0]);

                if (outstr[1] != 0)
                    outMessage.Add(outstr[1]);

                if (outstr[2] != 0)
                    outMessage.Add(outstr[2]);
            }

            byte[] outbyte = (byte[])outMessage.ToArray(Type.GetType("System.Byte"));
            return System.Text.Encoding.Default.GetString(outbyte);
        }

        public static string EscapeString(string text)
        {
            StringBuilder ss = new StringBuilder();
            Util.EscapeString(ss, text);
            return ss.ToString();
        }

        public static string WhereIn(string s)
        {
            string _r = "";
            s = s + ",X1_X2_X3";
            string[] aSegment = s.Split(',');

            foreach (string Segment in aSegment) {
                if (!string.IsNullOrEmpty(Segment)){
                    if (_r != "") {
                        _r = _r + ",";
                    }

                    _r = _r + "'" + AntiSQLInjection(Segment) + "'";
                }
            }

            return _r;
        }

        public static string ComputeHash(string Content)
        {
            string computedContent = string.Format("blob {0}\0{1}", Content.Length, Content);
            var hashBytes = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(computedContent));
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++) {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        internal static string getClassName(Type classType)
        {
            string text1 = classType.ToString();
            return text1.Substring(text1.LastIndexOf('.') + 1);
        }

        public static void Clear()
        {
            lock (_PENDING) {
                oAssembly  = new Dictionary<string, Assembly>();
                oZDateTime = new Dictionary<string, DateTime>();
            }
        }

        private static ClassMapping ContainsKey(string key)
        {
            ClassMapping value;
            _ClassMappingCache.TryGetValue(key, out value);
            return value;
        }

        private static void SetValue(string key, ClassMapping _des)
        {
            lock (_PENDING) {

                if (_des != null) {
                    _ClassMappingCache[key] = _des;
                }
            }
        }


        private static void Add(string key, ClassMapping des)
        {
            lock (_PENDING) {
                if ((!_ClassMappingCache.ContainsKey(key)) && des != null) {
                    _ClassMappingCache.Add(key, des);
                }
            }
        }

        private static ClassMapping getClassMappingCache(string key)
        {
            ClassMapping value;
            _ClassMappingCache.TryGetValue(key, out value);

            if (value != null) {
                return value;
            }

            throw new Exception("缓存中没存在此数据");
        }

        private static ClassMapping UpdateClassMappingCache<T>()
        {
            var type = typeof(T);
            var cacheValue = ContainsKey(type.FullName);

            if (cacheValue == null) {
                var model = new ClassMapping();
                model.ClassType = type;
                model.ClassName = type.Name;
                string tableName          = type.Name;
                Attribute[] attibutes     = null;

                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                //Querying Class Attributes
                foreach (Attribute attr in type.GetCustomAttributes(true)) {
                    if (attr.GetType() == typeof(AttributeMapping))  {
                        AttributeMapping _attribute = (AttributeMapping) attr;
                        tableName = _attribute.TableName;
                        break;
                    }
                }

                foreach (PropertyInfo p in properties)  {


                    if (p.CanWrite && p.CanRead) {
                        attibutes = Attribute.GetCustomAttributes(p);
                        AttributeMapping _AttributeMapping = null;

                        foreach (Attribute attribute in attibutes)  {
                            //检怀是倀设瀀了AttributeMapping倀性
                            if (attribute.GetType() == typeof(AttributeMapping))  {
                                string key = (cDbName + "_" + type.FullName + "_" + p.Name).ToLower();

                                if (_AttributeMappings.ContainsKey(key)) {
                                    _AttributeMapping = _AttributeMappings[key];
                                } else {
                                    AttributeMapping _attribute = (AttributeMapping) attribute;
                                    _AttributeMapping           = new AttributeMapping(p.Name);

                                    if (string.IsNullOrEmpty(_attribute.TableName) && !string.IsNullOrEmpty(tableName)) {
                                        _AttributeMapping.TableName = tableName;
                                    } else {
                                        _AttributeMapping.TableName = _attribute.TableName;
                                    }

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

                        if (_AttributeMapping.Ignore) {
                            continue;
                        }

                        _AttributeMapping.Nullable = Nullable.GetUnderlyingType(p.PropertyType) != null;

                        ZZLogger.Debug(ZFILE_NAME , "X1" + p.PropertyType.Name);
                        ZZLogger.Debug(ZFILE_NAME , "X2" + tableName);
                        ZZLogger.Debug(ZFILE_NAME , "X3" + _AttributeMapping.TableName);

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
                                if (_AttributeMapping.Nullable) {
                                    ZZLogger.Debug(ZFILE_NAME, p.PropertyType.Name);

                                    if (p.PropertyType.Name.Contains("DateTime")) {
                                        _AttributeMapping.Type = DbType.DateTime;
                                    } else {
                                        _AttributeMapping.Type = DbType.Object;
                                    }
                                } else {
                                    _AttributeMapping.Type = DbType.Object;
                                }
                            }

                            ZZLogger.Debug(ZFILE_NAME, Type.GetTypeCode(p.PropertyType));
                            ZZLogger.Debug(ZFILE_NAME, p.Name);
                        }

                        model.AddAttributeMap(_AttributeMapping);

                    }
                }



                Add(type.FullName, model);
                cacheValue = model;
            }

            return cacheValue;
        }

        internal static ClassMapping getClassMapping<T>()
        {
            return UpdateClassMappingCache<T>();
        }

        internal static IList<AttributeMapping> GetExecColumns(ClassMapping des, bool add = true)
        {
            var columns = new List<AttributeMapping>();

            if (des != null && des.AttributeMappings != null) {
                foreach (var item in des.AttributeMappings) {
                    if (item.Ignore || item.AutoIdentity) {
                        continue;
                    }

                    if ((!add) &&  item.PrimaryKey) {
                        continue;
                    }

                    if (!item.CanWrite) {
                        continue;
                    }

                    columns.Add(item);

                }
            }

            return columns;
        }

        internal static IList<AttributeMapping> GetPrimary(ClassMapping des)
        {
            return des.KeyAttributeMappings;
        }
    }
}
