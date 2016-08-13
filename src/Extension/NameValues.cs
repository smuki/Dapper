using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volte.Data.Dapper
{

    [Serializable]
        public class NameValues {
            const string ZFILE_NAME = "NameValues";

            // Methods
            public NameValues()
            {
                _Dictionary = new List<NameValue>();
            }

            public NameValues(string cData)
            {
                _Dictionary  = new List<NameValue>();
                _Dictionary2 = new List<NameValues>();

                if (!string.IsNullOrEmpty(cData)) {
                    Parser(cData);
                }
            }

            internal void Read(Lexer _Lexer)
            {
                _Lexer.SkipWhiteSpace();

                if (_Lexer.Current == '[' && _Lexer.NextChar != ']') {
                    _Lexer.NextToken();

                    for (;;) {
                        if (_Lexer.Current == '[') {
                            NameValues variable2 = new NameValues();
                            variable2.Read(_Lexer);
                            this.Add(variable2);
                        } else {
                            NameValue variable1 = new NameValue();

                            variable1.Read(_Lexer);

                            this.Add(variable1);
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
                writer.AppendLine("[");

                int i = 0;

                if (_Dictionary.Count > 0) {

                    foreach (NameValue name in _Dictionary) {
                        if (i > 0) {
                            writer.Append(",");

                            writer.AppendLine("");
                        }

                        name.Write(writer);
                        i++;
                    }
                }

                if (_Dictionary2.Count > 0) {
                    foreach (NameValues name in _Dictionary2) {
                        if (i > 0) {
                            writer.Append(",");

                            writer.AppendLine("");
                        }

                        name.Write(writer);
                        i++;
                    }
                }

                writer.AppendLine("");
                writer.Append("]");
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
                return s.ToString();
            }

            public void Add(NameValues value)
            {
                _Dictionary2.Add(value);
            }
            public void Remove(NameValues value)
            {
                _Dictionary2.Remove(value);
            }
            public void Add(NameValue value)
            {
                _Dictionary.Add(value);
            }

            public void Remove(NameValue value)
            {
                _Dictionary.Remove(value);
            }

            public void Clear()
            {
                _Dictionary  = new List<NameValue>();
                _Dictionary2 = new List<NameValues>();
            }

            public List<NameValues> ListValues
            {
                get {
                    return _Dictionary2;
                }
            }

            public List<NameValue> Values
            {
                get {
                    return _Dictionary;
                }
            }

            public int Count
            {
                get {
                    return _Dictionary.Count + _Dictionary2.Count;
                }
            }

            // NameValues
            private readonly StringBuilder s = new StringBuilder();

            private List<NameValue> _Dictionary = new List<NameValue>();
            private List<NameValues> _Dictionary2 = new List<NameValues>();

        }
}
