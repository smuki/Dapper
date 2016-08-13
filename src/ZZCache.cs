using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace Volte.Data.Dapper
{
    public class ZZCache {
        const string ZFILE_NAME = "ZZCache";

        private readonly StringBuilder sb = new StringBuilder();
        private static ZZCache _Cache;
        private static Queue<JSONObjects> _que;
        private static LRU<string, string> _cache         = new LRU<string, string>(5000);
        private static LRU<string, JSONTable> _cache_Cell     = new LRU<string, JSONTable>(5000);
        private static LRU<string, JSONObjects> _JSONObject = new LRU<string, JSONObjects>(5000);
        private static object _PENDING                    = new object();
        private static string _Cache_Location             = "";

        public static ZZCache getInstance()
        {
            if (_Cache == null) {

                Initialize();

                _que   = new Queue<JSONObjects>();
                _Cache = new ZZCache();
            }

            return _Cache;
        }

        internal ZZCache()
        {

        }

        public static void Initialize()
        {

            if (_Cache_Location == "") {
                try {
                    string rootPath  = AppDomain.CurrentDomain.BaseDirectory;
                    string separator = Path.DirectorySeparatorChar.ToString();
                    rootPath         = rootPath.Replace("/", separator);
                    _Cache_Location  = rootPath + "temp" + separator + "cache" + separator;

                    ZZLogger.Debug(ZFILE_NAME , _Cache_Location);

                    if (!Directory.Exists(_Cache_Location)) {
                        Directory.CreateDirectory(_Cache_Location);
                    }
                } catch (Exception ex) {
                    ZZLogger.Debug(ZFILE_NAME, ex);
                }
            }
        }

        public static string Type(string fileName)
        {
            if (_JSONObject.GetValue(fileName) != null) {
                return "N";
            } else if (_cache_Cell.GetValue(fileName) != null) {
                return "T";
            } else {

                Initialize();

                string _fileName = _Cache_Location + fileName + "T";

                if (File.Exists(_fileName)) {
                    return "T";
                } else {
                    return "S";
                }
            }
        }

        public static void WriteJSONObjects(string fileName, JSONObjects _Values)
        {
            _JSONObject.SetValue(fileName, _Values);
        }

        public static JSONObjects ReadJSONObjects(string fileName)
        {
            JSONObjects _Values = _JSONObject.GetValue(fileName);

            if (_Values == null) {
                _Values = new JSONObjects();
            }

            return _Values;
        }

        public static void WriteJSONTable(string fileName, JSONTable _JSONTable)
        {
            _cache_Cell.SetValue(fileName, _JSONTable);

            Initialize();

            try {
                string _fileName = _Cache_Location + fileName + "T";

                ZZLogger.Debug(ZFILE_NAME , _fileName);

                using(FileStream fs = new FileStream(_fileName , FileMode.Create)) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    GZipStream Compress = new GZipStream(fs, CompressionMode.Compress);
                    formatter.Serialize(Compress, _JSONTable);
                    Compress.Close();
                }
            } catch (Exception ex) {
                ZZLogger.Debug(ZFILE_NAME, ex);
            }
        }

        public static JSONTable ReadJSONTable(string fileName)
        {
            JSONTable _JSONTable = _cache_Cell.GetValue(fileName);

            if (_JSONTable == null) {

                ZZLogger.Debug(ZFILE_NAME , "load from " + fileName);

                Initialize();

                string _fileName = _Cache_Location + fileName + "T";

                if (File.Exists(_fileName)) {
                    using(FileStream fs = new FileStream(_fileName , FileMode.Open)) {
                       GZipStream dStream = new GZipStream(fs, CompressionMode.Decompress, true);
                        BinaryFormatter formatter = new BinaryFormatter();
                        _JSONTable = (JSONTable)formatter.Deserialize(dStream);

                        _cache_Cell.SetValue(fileName, _JSONTable);
                    }
                } else {
                    _JSONTable = new JSONTable();
                }
            } else {
                ZZLogger.Debug(ZFILE_NAME , "load memory");
            }

            return _JSONTable;
        }

        public static object Read(string fileName)
        {
            Initialize();
            fileName = _Cache_Location + fileName;
            string data = "";

            if (File.Exists(fileName)) {
                using(StreamReader sr = new StreamReader(fileName)) {
                    data = sr.ReadToEnd();
                }
            }

            return data;
        }

        public static void Write(string fileName, object Data)
        {
            string id    = Path.GetFileNameWithoutExtension(fileName);
            string _path = Path.GetDirectoryName(fileName);

            if (_path == "") {
                Initialize();
                fileName = _Cache_Location + fileName;
            } else if (!Directory.Exists(_path)) {
                Directory.CreateDirectory(_path);
            }

            StreamWriter swer = new StreamWriter(fileName, false);
            swer.Write(Data);
            swer.Flush();
            swer.Close();
        }
    }
}
