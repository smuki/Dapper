using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace Volte.Data.Dapper
{

    [Serializable]
    internal class Row {
        const string ZFILE_NAME = "Row";
        // Methods
        public Row()
        {
            _cells = new List<Cell>();
        }

        public Row(int i)
        {
            _cells = new List<Cell> (i);
        }

        internal void Read(Lexer _Lexer)
        {
            if (_Lexer.Current == '{') {
                _Lexer.NextToken();

                string name = _Lexer.ParseName();

                int c = 0;

                if (name == "d") {

                    _Lexer.SkipWhiteSpace();

                    if (_Lexer.Current == '[') {
                        _Lexer.NextToken();

                        for (;;) {
                            c++;

                            if (_Lexer.Current == '{') {

                                JSONObjects _JSONObjects = new JSONObjects();
                                _JSONObjects.Read(_Lexer);
                                this.JSONTable.Add(new Cell(_JSONObjects));

                            } else {

                                this.JSONTable.Add(new Cell(_Lexer.ParseValue()));

                            }

                            if (_Lexer.Current == ',') {
                                _Lexer.NextToken();
                            } else {
                                break;
                            }
                        }
                    }

                    _Lexer.NextToken();
                }

                if (_Lexer.Current == ',') {
                    _Lexer.NextToken();
                }

                name = _Lexer.ParseName();

                if (name == "Reference") {

                    _NameValue = new JSONObject();
                    _NameValue.Read(_Lexer);

                    _Lexer.SkipWhiteSpace();
                }

                _Lexer.NextToken();
            } else {

            }
        }

        internal void Write(StringBuilder writer)
        {
            writer.AppendLine("{");

            if (_cells != null) {
                writer.Append("\"d\":");
                writer.Append("[");
                bool first = true;

                foreach (Cell _Cell in _cells) {
                    if (!first) {
                        writer.Append(",");
                    } else {
                        first = false;
                    }

                    _Cell.Write(writer);
                }

                writer.Append("]");
                writer.AppendLine(",");
                writer.AppendLine("");
            }
            writer.Append("\"Reference\":");
            _NameValue.Write(writer);

            writer.AppendLine("");
            writer.AppendLine("}");
        }

        public List<Cell> JSONTable
        {
            get {
                if (_cells == null) {
                    _cells = new List<Cell>();
                }

                return _cells;
            } set {
                _cells = value;
            }
        }

        public DateTime getDateTime(int i)
        {
            object _obj = this[i];

            if (_obj is DateTime) {
                return (DateTime) _obj;
            } else if (_obj.ToString() == "") {
                return DapperUtil.DateTime_MinValue;
            } else if (DapperUtil.IsNumeric(_obj) && _obj.ToString().Length == 8) {
                return DateTime.ParseExact(_obj.ToString(), "yyyyMMdd", null);
            } else if (DapperUtil.IsNumeric(_obj)) {
                return DateTime.ParseExact(_obj.ToString(), "yyyyMMddhhmmss", null);
            } else {
                return Convert.ToDateTime(_obj);
            }
        }

        public Cell this[int i]
        {
            get {
                if (i >= _cells.Count) {
                    _size = _cells.Count;

                    for (int x = _size; x <= i; x++) {
                        _size++;
                        _cells.Add(new Cell());
                    }
                }

                return _cells[i];
            } set {
                if (i >= _cells.Count) {
                    _size = _cells.Count;

                    for (int x = _size; x <= i; x++) {
                        _size++;

                        if (x >= i) {
                            _cells.Add(value);
                        } else {
                            _cells.Add(new Cell());
                        }
                    }
                } else {
                    _cells[i] = value;
                }
            }
        }

        public int    Index         { get { return _Index;     } set { _Index     = value; }  }
        public JSONObject Reference { get { return _NameValue; } set { _NameValue = value;  }  }

        // Fields
        private int _Index;
        private int _size;

        private List<Cell> _cells;
        private JSONObject _NameValue;
    }
}
