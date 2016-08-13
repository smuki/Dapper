using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.IO;

using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    internal class XmlConfigLoader {
        const string ZFILE_NAME = "XmlConfigLoader";

        // Methods
        private XmlConfigLoader()
        {
            _AttributeMappings = new Dictionary<string, AttributeMapping>();
        }

        private void AttributeMapping(XmlReader reader, string database, string clsMapping_Name, string cTable_Name, int colIndex)
        {
            string text1 = reader.GetAttribute("name");
            string text2 = reader.GetAttribute("column");
            string text3 = reader.GetAttribute("key");
            string text5 = reader.GetAttribute("type");
            string text6 = reader.GetAttribute("increment");
            string text7 = reader.GetAttribute("timestamp");
            string text8 = reader.GetAttribute("auto");

            AttributeMapping map1 = null;

            if (text1 != null) {
                map1 = new AttributeMapping(text1);
                map1.TableName = cTable_Name;

                if (text2 != null) {
                    map1.ColumnName = text2;

                    if (text3 != null) {
                        map1.PrimaryKey = true;
                    }

                    if (text5 == null) {
                        string text9 = "?ҫ?X?t?t" + map1.ColumnName + "?X?????X?t?";
                        throw new DapperException(text9, ExceptionTypes.XmlError);
                    }

                    switch (text5.ToLower()) {
                    case "boolean": {
                        map1.Type = DbType.Boolean;
                        break;
                    }

                    case "bigint": {
                        map1.Type = DbType.Int64;
                        break;
                    }

                    case "binary": {
                        map1.Type = DbType.Binary;
                        break;
                    }

                    case "currency": {
                        map1.Type = DbType.Currency;
                        break;
                    }

                    case "date": {
                        map1.Type = DbType.DateTime;
                        break;
                    }

                    case "dbdate": {
                        map1.Type = DbType.Date;
                        break;
                    }

                    case "decimal": {
                        map1.Type = DbType.Decimal;
                        break;
                    }

                    case "double": {
                        map1.Type = DbType.Double;
                        break;
                    }

                    case "guid": {
                        map1.Type = DbType.Guid;
                        break;
                    }

                    case "object": {
                        map1.Type = DbType.Object;
                        break;
                    }

                    case "single": {
                        map1.Type = DbType.Single;
                        break;
                    }

                    case "smallint": {
                        map1.Type = DbType.Int16;
                        break;
                    }

                    case "tinyint": {
                        map1.Type = DbType.Byte;
                        break;
                    }

                    case "integer": {
                        map1.Type = DbType.Int32;
                        break;
                    }

                    case "varchar":
                    case "string": {
                        map1.Type = DbType.String;
                        break;
                    }

                    default: {
                        throw new DapperException("Ŀ?t???֧???Ļ??ݞX???", ExceptionTypes.XmlError);
                    }
                    }

                    if ((text6 != null) && (text6.ToLower() == "true")) {
                        map1.CanWrite = false;
                        map1.AutoIdentity = true;
                    }

                    if ((text7 != null) && (text7.ToLower() == "true")) {
                        map1.CanWrite = false;
                        map1.Timestamp  = true;
                    }

                    if (text8 != null && (text8.ToLower() == "true")) {
                        map1.CanWrite = false;
                    }
                }
            }

            if (map1 != null) {
                lock (_PENDING) {
                    _AttributeMappings[(database + "_" + clsMapping_Name + "_" + text1).ToLower()] = map1;
                }
            }

            return;
        }

        private void GetClassMapping(XmlNodeReader node)
        {
            string text6;
            string _classmapping_name = node.GetAttribute("name");
            string text2     = node.GetAttribute("table");
            string _database = node.GetAttribute("database");

            int num1 = 0;
            ClassMapping map1 = null;

            if (((_classmapping_name != null) && (_database != null)) && (text2 != null)) {

                map1 = new ClassMapping();
                map1.TableName = text2;

                int num2 = node.Depth;

                while (node.Read() && (node.Depth > num2)) {
                    if ((node.NodeType == XmlNodeType.Element) && (node.Name == "attribute")) {
                        this.AttributeMapping(node, _database, _classmapping_name, text2, num1);
                        num1++;
                    }
                }
            } else {
                text6 = "ClassMapping ȱ?tclassName,DbName,tableName ҫЩ?tҪ?t???";
                throw new DapperException(text6, ExceptionTypes.XmlError);
            }

            return;
        }

        public static XmlConfigLoader Instance()
        {
            if (_xmlConfigLoader == null) {
                _xmlConfigLoader = new XmlConfigLoader();
            }

            return _xmlConfigLoader;
        }

        internal void LoadClassMappingInfo(string xmlFile)
        {
            XmlDocument document1 = new XmlDocument();
            string text1 = xmlFile;

            try {

                document1.Load(text1);

                XmlNodeReader reader1 = new XmlNodeReader(document1);

                while (reader1.Read()) {
                    string text3;

                    if (reader1.NodeType != XmlNodeType.Element) {
                        continue;
                    }

                    if ((text3 = reader1.Name.ToLower()) == null) {
                        continue;
                    }

                    text3 = string.IsInterned(text3);

                    if (text3 != "class") {
                        continue;
                    }

                    this.GetClassMapping(reader1);
                    continue;
                }

                reader1.Close();
            } catch (DapperException exception1) {
                throw exception1;
            } catch (Exception exception2) {
                string text2 = "ҫ?t?X???t???" + text1 + "?t?X??ҫ,ҫ?Xҫ??X???ҫ?t?t???t!";
                text2 = text2 + exception2.Message;

                ZZLogger.Error(ZFILE_NAME, exception2);

                throw new DapperException(text2, ExceptionTypes.XmlError);
            }
        }

        public void Clear()
        {
            _AttributeMappings = new Dictionary<string, AttributeMapping>();
        }

        // Properties

        public Dictionary<string, AttributeMapping> AttributeMappings
        {
            get {
                return _AttributeMappings;
            }
        }

        // Fields
        private Dictionary<string, AttributeMapping> _AttributeMappings;
        private static readonly object _PENDING = new object();
        private static XmlConfigLoader _xmlConfigLoader;
    }
}
