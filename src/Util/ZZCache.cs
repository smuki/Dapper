using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Threading;

using Volte.Data.Json;

namespace Volte.Data.Dapper
{
    public class ZZCache {
        const string ZFILE_NAME = "ZZCache";

        private readonly StringBuilder sb = new StringBuilder();
        private static ZZCache _Cache;
        private static Queue<JSONTable> _que              = new Queue<JSONTable>();
        private static LRU<string, string> _cache         = new LRU<string, string>(5000);
        private static LRU<string, JSONTable> _cache_Cell = new LRU<string, JSONTable>(5000);
        private static LRU<string, JSONArray> _JSONObject = new LRU<string, JSONArray>(10);
        private static object _PENDING                    = new object();
        private static string _Cache_Location             = "";
        private bool _Running                             = false;
        private Thread _worker;

        public static ZZCache getInstance()
        {
            if (_Cache == null) {
                _que   = new Queue<JSONTable>();
                _Cache = new ZZCache();
                _Cache.Running();
                Initialize();
            }

            return _Cache;
        }

        internal void Running()
        {
            _worker = new Thread(new ThreadStart(_Do_Worker));
            _worker.IsBackground = true;
            _worker.Start();

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

            ZZCache.getInstance().Running();

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
                    return "";
                }
            }
        }

        public static void WriteJSONArray(string fileName, JSONArray _Values)
        {
            _JSONObject.SetValue(fileName, _Values);
        }

        public static JSONArray ReadJSONArray(string fileName)
        {
            JSONArray _Values = _JSONObject.GetValue(fileName);

            if (_Values == null) {
                _Values = new JSONArray();
            }

            return _Values;
        }

        public static void WriteJSONTable(string fileName, JSONTable _JSONTable)
        {
            Initialize();
            _cache_Cell.SetValue(fileName, _JSONTable);

            _que.Enqueue(_JSONTable);

            ZZLogger.Debug(ZFILE_NAME , _JSONTable.ToString());

        }

        internal void _Do_Worker()
        {
            if (_Running) {
                return;
            }

            _Running  = true;

            while (true) {

                try {
                    ZZLogger.Debug(ZFILE_NAME , _Cache_Location);
                    if (_que.Count > 0) {
                        lock (_PENDING) {
                            Initialize();
                            JSONTable _JSONTable= _que.Dequeue();

                            try {
                                string _fileName = _Cache_Location + _JSONTable.Variable.GetValue("lnk_DataUrl")+ "T";

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
                    }
                } catch (Exception e) {

                }

                Thread.Sleep(10000);
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

                        if (_JSONTable.RecordCount<2000){
                            _cache_Cell.SetValue(fileName, _JSONTable);
                        }
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
