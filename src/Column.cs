using System;
using System.Xml;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volte.Data.Dapper
{

    [Serializable]
    public class Column: AttributeMapping {
        const string ZFILE_NAME = "Column";
        // Methods
        public Column()
        {
            this.Index = 0;
        }

        internal void Read(Lexer _Lexer)
        {
            if (_Lexer.Current == '{') {
                _Lexer.NextToken();

                string name = _Lexer.ParseName();

                if (name == "d") {
                    _Lexer.NextToken();

                    for (;;) {

                        name = _Lexer.ParseName();
                        _Lexer.SkipWhiteSpace();
                        string cValue = _Lexer.ParseValue();

                        if (name == "DataType") {
                            this.DataType = cValue;
                        } else if (name == "Name") {
                            this.Name = cValue;
                        } else if (name == "Caption") {
                            this.Caption = cValue;
                        } else if (name == "ColumnName") {
                            this.ColumnName = cValue;
                        } else if (name == "Index") {
                            this.Index = DapperUtil.ToInt(cValue);
                        } else if (name == "Width") {
                            this.Width = DapperUtil.ToInt(cValue);
                        } else if (name == "Scale") {
                            this.Scale = DapperUtil.ToInt(cValue);
                        } else if (name == "Reference") {
                            this.Reference = cValue;
                        } else if (name == "EnableMode") {
                            this.EnableMode = cValue;
                        } else if (name == "AlignName") {
                            this.AlignName = cValue;
                        } else if (name == "NonPrintable") {
                            this.NonPrintable = DapperUtil.ToBoolean(cValue);
                        }

                        if (_Lexer.Current == ',') {
                            _Lexer.NextToken();
                        } else {
                            break;
                        }
                    }

                    _Lexer.NextToken();
                }

                _Lexer.NextToken();
            } else {

            }
        }

        internal void Write(StringBuilder writer)
        {
            writer.AppendLine("{");
            writer.AppendLine("\"d\":{");

            if (this.Name != null) {
                writer.Append("\"Name\":\"");
                DapperUtil.EscapeString(writer, this.Name);
                writer.AppendLine("\",");
            }

            if (this.Index != 0) {
                writer.AppendLine("\"Index\":" + this.Index.ToString() + ",");
            }

            if (this.Width != 0) {
                writer.AppendLine("\"Width\":" + this.Width.ToString() + ",");
            }

            writer.AppendLine("\"Scale\":" + this.Scale.ToString() + ",");

            if (this.NonPrintable) {
                writer.AppendLine("\"NonPrintable\":\"true\",");
            } else {
                writer.AppendLine("\"NonPrintable\":\"false\",");
            }

            if (this.Caption != null) {
                writer.Append("\"Caption\":\"");
                DapperUtil.EscapeString(writer, this.Caption.ToString());
                writer.AppendLine("\",");
            }

            if (this.DataType != "") {
                writer.AppendLine("\"DataType\":\"" + this.DataType + "\",");
            }

            if (this.AlignName != "") {
                writer.AppendLine("\"AlignName\":\"" + this.AlignName + "\",");
            }

            if (this.Reference != null) {
                writer.Append("\"Reference\":\"");
                DapperUtil.EscapeString(writer, this.Reference);
                writer.AppendLine("\",");
            }

            writer.AppendLine("\"EnableMode\":\"" + this.EnableMode + "\"");
            writer.AppendLine("}");
            writer.AppendLine("}");
        }
    }
}
