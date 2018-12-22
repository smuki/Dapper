using System;
using System.Text;
using System.Data;

namespace Volte.Data.Dapper
{
    internal abstract class CommanderStream {
        const string ZFILE_NAME = "SQLClause";
        // Methods
        public CommanderStream(Streaming _rdb, ClassMapping cm)
        {
            _sql.Length    = 0;
            _partForObject = "";
            _clsMap        = cm;
            _streaming     = _rdb;
        }

        public void Clear()
        {
            _sql.Length = 0;
        }

        public void AddSqlClause(string s)
        {
            _sql.Append(s);
        }

        public abstract IDbCommand BuildForObject(Streaming _streaming, EntityObject obj);

        // Properties
        public string SqlString
        {
            get {
                return _sql.ToString();
            }
        }
        public ClassMapping ThisClassMapping
        {
            get {
                return _clsMap;
            }
        }

        public string partForObject
        {
            get {
                return _partForObject;
            } set {
                _partForObject = value;
            }
        }
        public Streaming    Streaming
        {
            get {
                return _streaming;
            } set {
                _streaming    = value;
            }
        }

        // Fields
        private readonly StringBuilder _sql = new StringBuilder();
        private ClassMapping _clsMap;
        private Streaming _streaming;
        private string _partForObject;
    }
}

