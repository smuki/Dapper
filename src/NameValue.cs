using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;
//using System.Web;
//using System.Web.UI;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volte.Data.Dapper
{

    [Serializable]
    public class NameValue {
        const string ZFILE_NAME = "NameValue";

        // Methods
        public NameValue()
        {
            _Dictionary = new Dictionary<string, NameValuePair> (StringComparer.InvariantCultureIgnoreCase);
        }

        public NameValue(string cData)
        {
            _Dictionary = new Dictionary<string, NameValuePair> (StringComparer.InvariantCultureIgnoreCase);

            if (!string.IsNullOrEmpty(cData)) {
                Parser(cData);
            }
        }

        internal void Read(Lexer _Lexer)
        {
            _Lexer.SkipWhiteSpace();

            if (_Lexer.Current == '{' && _Lexer.NextChar != '}') {
                _Lexer.NextToken();

                for (;;) {
                    NameValuePair variable1 = new NameValuePair();

                    variable1.Read(_Lexer);

                    if (variable1.Value != null) {
                        this.SetValue(variable1.Name, variable1.Value, variable1.Type);
                    }

                    _Lexer.SkipWhiteSpace();

                    if (_Lexer.Current == ',') {
                        _Lexer.NextToken();
                    } else {
                        break;
                    }
                }

                _Lexer.NextToken();
            }
        }

        internal void Write(StringBuilder writer)
        {
            writer.AppendLine("{");

            if (_Dictionary.Count > 0) {
                int i = 0;

                foreach (string name in _Dictionary.Keys) {
                    if (i > 0) {
                        writer.Append(",");

                        if (Indented) {
                            writer.AppendLine("");
                        }
                    }

                    _Dictionary[name].Write(writer);
                    i++;
                }
            }

            writer.AppendLine("");
            writer.Append("}");
            writer.AppendLine("");
        }

        public List<string> getArrayList()
        {
            List<string> _oList = new List<string>();

            foreach (KeyValuePair<string, NameValuePair> kvp in _Dictionary) {
                _oList.Add(kvp.Key);
            }

            return _oList;
        }

        public void Parser(string cString)
        {
            if (string.IsNullOrEmpty(cString)) {
                return;
            }

            Lexer oLexer = new Lexer(cString);

            this.Read(oLexer);
        }

        public override string ToString()
        {
            return _ToString();
        }

        private string _ToString()
        {

            s.Length = 0;

            this.Write(s);

            string cString = s.ToString();

            if (!Indented) {
                cString = cString.Replace("\n", "");
            }

            return cString;
        }

        public bool ContainsKey(string name)
        {
            return _Dictionary.ContainsKey(name);
        }

        public void SetDateTime(string name, DateTime value)
        {
            this.SetValue(name, value, "datetime");
        }

        public void SetDateTime(string name, DateTime? value)
        {
            if (value.HasValue) {
                this.SetValue(name , value.Value , "datetime");
            } else {
                this.SetValue(name, "", "datetime");
            }
        }

        public void SetValue(string name, DateTime value)
        {
            this.SetValue(name, value, "datetime");
        }

        public DateTime GetDateTime(string Name)
        {
            object o = GetValue(Name);

            try {
                return Convert.ToDateTime(o);
            } catch {
                return DapperUtil.DateTime_MinValue;
            }
        }

        public bool GetBoolean(string Name)
        {
            object o = GetValue(Name);

            try {
                return Convert.ToBoolean(o);
            } catch {
                return false;
            }
        }

        public int GetInteger(string Name)
        {
            object o = GetValue(Name);
            return DapperUtil.ToInt32(o);
        }

        public decimal GetDecimal(string Name)
        {
            return DapperUtil.ToDecimal(GetValue(Name));
        }

        public void SetDouble(string name, double value)
        {
            this.SetValue(name, value, "");
        }

        public double GetDouble(string name)
        {
            return Convert.ToDouble(this.GetValue(name));
        }

        public void SetDecimal(string name, decimal value)
        {
            this.SetValue(name, value, "");
        }

        public bool IsNameValue(string name)
        {
            return this.GetType(name) == "v";
        }

        public bool IsNameValues(string name)
        {
            return this.GetType(name) == "l";
        }

        public NameValue GetNameValue(string Name)
        {
            NameValue _NameValue = new NameValue();

            if (this.GetType(Name) == "v") {
                if (_Dictionary.ContainsKey(Name)) {
                    _NameValue = (NameValue) _Dictionary[Name].Value;
                }
            }

            return _NameValue;
        }

        public NameValues GetNameValues(string Name)
        {
            NameValues _NameValues = new NameValues();

            if (this.GetType(Name) == "l") {
                if (_Dictionary.ContainsKey(Name)) {
                    _NameValues = (NameValues)_Dictionary[Name].Value;
                }
            }

            return _NameValues;
        }

        public string GetValue(string name)
        {
            if (_Dictionary.ContainsKey(name)) {
                NameValuePair variable1 = _Dictionary[name];
                return variable1.Value.ToString();
            } else {
                return "";
            }
        }

        public string GetType(string name)
        {
            if (_Dictionary.ContainsKey(name)) {
                NameValuePair variable1 = _Dictionary[name];
                return variable1.Type;
            } else {
                return "nvarchar";
            }
        }

        public void SetValue(string name, NameValues value)
        {
            NameValuePair variable1 = new NameValuePair();
            variable1.Name          = name;
            variable1.Value         = value;
            variable1.Type          = "l";
            _Dictionary[name]       = variable1;
        }

        public void SetValue(string name, NameValue value)
        {
            NameValuePair variable1 = new NameValuePair();
            variable1.Name          = name;
            variable1.Value         = value;
            variable1.Type          = "v";
            _Dictionary[name]       = variable1;
        }

        public void SetValue(string name, bool value)
        {
            this.SetBoolean(name, value);
        }

        public void SetValue(string name, int value)
        {
            this.SetInteger(name, value);
        }

        public void SetBoolean(string name, bool value)
        {
            NameValuePair variable1 = new NameValuePair();
            variable1.Name          = name;
            variable1.Value         = value;
            variable1.Type          = "boolean";
            _Dictionary[name]       = variable1;
        }

        public void SetInteger(string Name, int value)
        {
            NameValuePair variable1 = new NameValuePair();
            variable1.Name          = Name;
            variable1.Value         = value;
            variable1.Type          = "integer";
            _Dictionary[Name]       = variable1;
        }

        public void SetValue(string name, string value)
        {
            this.SetValue(name, value, "");
        }

        public void Add(object key, object value)
        {
            SetValue(key.ToString(), value, "nvarchar");
        }

        public void Remove(object key)
        {
            _Dictionary.Remove(key.ToString());
        }

        public void SetValue(string name, object value, string cType)
        {
            if (_Dictionary == null) {
                _Dictionary = new Dictionary<string, NameValuePair> (StringComparer.InvariantCultureIgnoreCase);
            }

            NameValuePair variable1 = new NameValuePair();
            variable1.Name          = name;
            variable1.Value         = value;
            variable1.Type          = cType;
            _Dictionary[name]       = variable1;
        }

        public void Clear()
        {
            _Dictionary = new Dictionary<string, NameValuePair> (StringComparer.InvariantCultureIgnoreCase);
        }

        public List<string> Names
        {
            get {
                List<string> _Names = new List<string>();

                foreach (string name in _Dictionary.Keys) {
                    _Names.Add(name);
                }

                return _Names;
            }
        }

        public object this[object name]
        {
            get {
                return this[name.ToString()];
            } set {
            }
        }

        public object this[string name]
        {
            get {
                if (_Dictionary.ContainsKey(name)) {
                    return _Dictionary[name].Value;
                } else {
                    return null;
                }
            } set {
                NameValuePair variable1 = new NameValuePair();

                variable1.Name  = name;
                variable1.Value = value;
                variable1.Type  = "nvarchar";

                _Dictionary[name] = variable1;
            }
        }

        public int Count
        {
            get {
                return _Dictionary.Count;
            }
        }

        // NameValue
        public bool Indented = true;
        private readonly StringBuilder s = new StringBuilder();

        private Dictionary<string, NameValuePair> _Dictionary = new Dictionary<string, NameValuePair> (StringComparer.InvariantCultureIgnoreCase);

    }
}
