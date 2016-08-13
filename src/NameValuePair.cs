using System;
using System.Xml;
using System.IO;
using System.Text;
//using System.Web;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volte.Data.Dapper
{

    [Serializable]
    internal class NameValuePair {
        // Methods
        const string ZFILE_NAME = "NameValuePair";
        public NameValuePair()
        {
        }

        internal void Read(Lexer _Lexer)
        {

            if (_Lexer.Current == '{') {
                NameValue _VContexts = new NameValue();

                _VContexts.Read(_Lexer);
                this.Value = _VContexts;
            } else {
                _Lexer.SkipWhiteSpace();

                if (_Lexer.Current != '}') {
                    string name = _Lexer.ParseName();
                    _Lexer.SkipWhiteSpace();

                    if (_Lexer.Current == '{') {
                        NameValue _VContexts = new NameValue();
                        _VContexts.Read(_Lexer);

                        this.Name  = name;
                        this.Type  = "v";
                        this.Value = _VContexts;

                    } else if (_Lexer.Current == '[') {
                        NameValues _VContexts = new NameValues();
                        _VContexts.Read(_Lexer);

                        this.Name  = name;
                        this.Type  = "l";
                        this.Value = _VContexts;
                    } else {
                        this.Name  = name;
                        this.Value = _Lexer.ParseValue();
                    }

                    ZZLogger.Debug(ZFILE_NAME , this.Name + "&" + this.Value);
                } else {
                    this.Value = null;
                }
            }
        }

        internal void Write(StringBuilder  writer)
        {
            if (!string.IsNullOrEmpty(this.Name)) {
                writer.Append("\"" + this.Name + "\":");

                if (this.Value != null) {
                    //ZZLogger.Debug(ZFILE_NAME , "type = "+this.Type);
                    if (this.Type == "v") {
                        writer.AppendLine();
                        ((NameValue)this.Value).Write(writer);
                    } else if (this.Type == "l") {
                        writer.AppendLine();
                        ((NameValues)this.Value).Write(writer);
                    } else if (this.Type == "t") {
                        //  this.Value.Write(writer);
                    } else {
                        if (this.Type == "nvarchar") {
                            writer.Append("\"");
                            DapperUtil.EscapeString(writer, this.Value.ToString());
                            writer.Append("\"");
                        } else if (this.Type == "decimal" || this.Type == "integer") {
                            DapperUtil.EscapeString(writer, this.Value.ToString());
                        } else if (this.Type == "datetime") {
                            DapperUtil.EscapeString(writer, this.Value.ToString());
                        } else if (this.Type == "boolean") {
                            DapperUtil.EscapeString(writer, this.Value.ToString().ToLower());
                        } else {
                            writer.Append("\"");
                            DapperUtil.EscapeString(writer, this.Value.ToString());
                            writer.Append("\"");
                        }
                    }
                } else {
                    writer.Append("\"\"");
                }
            }
        }

        public string Type
        {
            get {
                return _type;
            } set {
                _type  = value;
            }
        }

        public string Name
        {
            get {
                return _name;
            } set {
                _name  = value;
            }
        }

        public object Value
        {
            get {
                return _value;
            } set {
                _value = value;
            }
        }

        private string _name  = "";
        private string _type  = "";
        private object _value = "";
    }
}
