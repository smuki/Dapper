using System;
using System.Xml;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volte.Data.Dapper
{
    [Serializable]
    internal sealed class Cell {
        // Methods
        public Cell()
        {
            _index = 0;
        }

        public Cell(object text)
        {
            _index = 0;
            this.Text = text;
        }

        internal void Read(Lexer element)
        {
        }

        internal void Write(StringBuilder  writer)
        {
            writer.Append("\"");

            if (this.Text != null) {
                string ctemp = "";

                if (this.Text is DateTime) {
                    if ((DateTime) this.Text <= DapperUtil.DateTime_MinValue) {
                        ctemp = "";
                    } else {
                        ctemp = ((DateTime) this.Text).ToString("yyyyMMddhhmmss");
                    }
                } else if (this.Text is bool) {
                    if ((bool) this.Text) {
                        ctemp = "Y";
                    } else {
                        ctemp = "N";
                    }
                } else if (this.Text is JSONObjects) {

                    ((JSONObjects)this.Text).Write(writer);

                } else {
                    ctemp = Convert.ToString(this.Text);
                }

                if (string.IsNullOrEmpty(ctemp)) {
                    writer.Append("");
                } else {
                    DapperUtil.EscapeString(writer, ctemp);
                }
            } else {
                writer.Append("");
            }

            writer.Append("\"");
        }

        // Properties
        public string getValue
        {
            get {
                if (_text == null) {
                    return "";
                } else {
                    return _text.ToString();
                }
            }
        }

        // Properties
        public object Text
        {
            get {
                if (_text == null) {
                    return "";
                } else {
                    return _text;
                }
            } set {
                _text = value;
            }
        }

        // Fields

        public int Index
        {
            get {
                return _index;
            } set {
                _index = value;
            }
        }
        private object _text;
        private int   _index;
    }
}