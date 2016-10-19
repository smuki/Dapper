using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

using Volte.Data.Dapper;

namespace Volte.Data.Dapper
{

    public class QueryRows {
        const string ZFILE_NAME = "QueryRows";
        public QueryRows()
        {
        }

        public QueryRows(DbContext Trans)
        {
            _DbContext     = Trans;
            _dbConnecttion = _DbContext.DbConnection;
        }

        public QueryRows(IDbConnection dbConnection)
        {
            _dbConnecttion = dbConnection;
        }

        public void Open()
        {
            if (string.IsNullOrEmpty(_CommandText)) {
                throw new DapperException("CommandText expected");
            }

            IDbCommand cmd = this.DbConnecttion.CreateCommand();
            cmd.CommandText = _CommandText;

            if (_DbContext.Transaction) {
                cmd.Transaction = _DbContext.DbTransaction;
            }

            try {
                IDataReader _DataReader = cmd.ExecuteReader();
                _Fill(_DataReader);
            } catch (Exception e) {
                ZZLogger.Error(ZFILE_NAME, _CommandText);
                throw e;
            }

        }

        public void Open(IDataReader _DataReader)
        {
            _Fill(_DataReader);
        }

        internal void _Fill(IDataReader _DataReader)
        {
            _RecordCount = -1;
            _fieldCount = _DataReader.FieldCount;
            _Ordinal    = new Dictionary<string , int>();
            _Type       = new Dictionary<string , string>();
            _Columns    = new List<string>();

            Dictionary<string, int> _t_name = new Dictionary<string, int>();

            for (int i = 0; i < _fieldCount; i++) {
                string s = _DataReader.GetName(i).ToLower();

                if (_t_name.ContainsKey(s)) {
                    int cc = _t_name[s] + 1;
                    s = s + cc.ToString();
                    _t_name[s] = cc;
                } else {
                    _t_name[s] = 0;
                }

                _Columns.Add(s);
                _Type[s] = _DataReader.GetFieldType(i).ToString();
                _Ordinal[s] = i;
            }

            _Data          = new List<object[]>();

            while (_DataReader.Read()) {
                object[] values = new object[_fieldCount];
                _DataReader.GetValues(values);
                _Data.Add(values);

            }

            _RecordCount = _Data.Count;

            _DataReader.Close();
            _Pointer = 0;
            _FillData();
        }

        private void _FillData()
        {
            if (_Pointer >= 0 && _Pointer < _Data.Count) {
                _Row  = _Data[_Pointer];
            } else {
                _Row = null;
            }

            if (_KeepPrev) {
                if (_Pointer > 0 && _Pointer < _Data.Count) {
                    _Prev = _Data[_Pointer - 1];
                } else {
                    _Prev = null;
                }
            } else {
                _Prev = null;
            }
        }

        public void MovePrev()
        {
            _Pointer--;
        }

        public void MoveNext()
        {
            _Pointer++;
            _FillData();
        }

        public void MoveFirst()
        {
            if (_Data.Count > 0) {
                _Pointer = 0;
            } else {
                _Pointer = 1000000;
            }

            _FillData();
        }

        public void Move(int _Absolute)
        {
            _Pointer = _Absolute;
        }

        public bool HasColumn(string Name)
        {
            return GetOrdinal(Name) >= 0;
        }

        public int GetOrdinal(string Name)
        {

            if (_Ordinal.ContainsKey(Name.ToLower())) {
                return _Ordinal[Name.ToLower()];
            }

            return -1;
        }

        private bool ConvertToBoolean(object obj)
        {
            if (DBNull.Value.Equals(obj) || obj==null) {
                return false;
            }
            if (obj is bool) {
                return (bool)obj;
            } else if (obj.Equals("Y") || obj.Equals("y")) {
                return true;
            } else if (obj.ToString()=="1") {
                return true;
            } else if (obj.Equals("True") || obj.Equals("true")) {
                return true;
            } else if (obj.Equals("N") || obj.Equals("n")) {
                return false;
            } else if (obj.ToString()=="0") {
                return false;
            } else if (obj.Equals("False") || obj.Equals("false")) {
                return false;
            } else {
                return DapperUtil.ToBoolean(obj);
            }
        }

        public bool GetBoolean(int i)
        {
            return ConvertToBoolean(this[i]);

        }

        public bool GetBoolean(string Name)
        {
            return ConvertToBoolean(this[Name]);
        }

        public decimal GetDecimal(string Name)
        {
            object _obj = this[Name];

            if (DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToDecimal(_obj);
        }

        public decimal GetDecimal(int i)
        {
            object _obj = this[i];

            if (DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToDecimal(_obj);
        }

        public double GetDouble(string Name)
        {
            object _obj = this[Name];

            if (DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToDouble(_obj);
        }

        public long GetLong(int i)
        {
            object _obj = this[i];

            if (_obj==null || DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToInt64(_obj);
        }

        public long GetLong(string Name)
        {
            object _obj = this[Name];

            if (_obj==null || DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToInt64(_obj);
        }

        public int GetInteger(int i)
        {
            object _obj = this[i];

            if (_obj==null || DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToInt32(_obj);
        }

        public int GetInteger(string Name)
        {
            object _obj = this[Name];

            if (_obj==null || DBNull.Value.Equals(_obj)) {
                return 0;
            }

            return Convert.ToInt32(_obj);
        }

        public string GetValue(int i)
        {
            object _obj = this[i];

            if (DBNull.Value.Equals(_obj)) {
                return "";
            }

            return _obj.ToString();
        }

        public string GetValue(string Name)
        {
            object _obj = this[Name];

            if (DBNull.Value.Equals(_obj)) {
                return "";
            }

            return _obj.ToString();
        }

        public DateTime GetDateTime(int i)
        {
            object _obj = this[i];

            if (DBNull.Value.Equals(_obj)) {
                return DapperUtil.DateTime_MinValue;
            }

            return Convert.ToDateTime(_obj);
        }

        public DateTime GetDateTime(string Name)
        {
            object _obj = this[Name];

            if (DBNull.Value.Equals(_obj)) {
                return DapperUtil.DateTime_MinValue;
            }

            return Convert.ToDateTime(_obj);
        }

        public DateTime? GetDateTime2(int i)
        {
            object _obj = this[i];

            if (DBNull.Value.Equals(_obj)) {
                return null;
            }

            return Convert.ToDateTime(_obj);
        }

        public DateTime? GetDateTime2(string Name)
        {
            object _obj = this[Name];

            if (DBNull.Value.Equals(_obj)) {
                return null;
            }

            return Convert.ToDateTime(_obj);
        }

        public string GetType(string Name)
        {
            return _Type[Name];
        }

        public object this[string name]
        {
            get {
                try {

                    return _Row[_Ordinal[name.ToLower()]];
                } catch (Exception ex) {
                    if (_Row == null) {
                        ZZLogger.Debug(ZFILE_NAME , "row is null");
                    }

                    ZZLogger.Debug(ZFILE_NAME , "Exception invalid column name [" + name + "]");
                    ZZLogger.Debug(ZFILE_NAME , ex);

                    throw new DapperException("Exception invalid column name [" + name + "]");
                }
            }
        }

        public object this[int i]
        {
            get {
                return _Row[i];
            }
        }

        public void Close()
        {
            _Data = null;
            _Row  = null;
            _Prev = null;
        }

        private readonly StringBuilder _Fields = new StringBuilder();
        private List<string>  _Columns;
        private DbContext _DbContext;
        private Dictionary<string, int> _Ordinal;
        private Dictionary<string, string> _Type;
        private IDbConnection _dbConnecttion;
        private bool _KeepPrev       = false;
        private int  _Pointer        = -1;
        private int  _RecordCount    = -1;
        private int  _fieldCount     = -1;
        private int  _Top            = 0;
        private string _CommandText  = "";
        private object[] _Row        = new object[1];
        private object[] _Prev       = new object[1];
        private List<object[]> _Data = new List<object[]>();

        public int Top            { get { return _Top;         } set { _Top         = value; }  }
        public bool KeepPrev      { get { return _KeepPrev;    } set { _KeepPrev    = value; }  }
        public string CommandText { get { return _CommandText; } set { _CommandText = value; }  }

        public IDbConnection DbConnecttion { get { return _dbConnecttion;              }  }
        public List<string> Columns        { get { return _Columns;                    }  }
        public bool IsFirst                { get { return _Pointer == 0;               }  }
        public bool IsLast                 { get { return _Pointer + 1 == _Data.Count; }  }
        public bool BOF                    { get { return _Pointer < 0;                }  }
        public bool EOF                    { get { return _Pointer >= _Data.Count;     }  }
        public int  Absolute_Position      { get { return _Pointer;                    }  }
        public int  FieldCount             { get { return _fieldCount;                 }  }
        public int  RecordCount            { get { return _RecordCount;                }  }
        public int  RecordsAffected        { get { return _Data.Count;                 }  }

    }
}
