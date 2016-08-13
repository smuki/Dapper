using System;

namespace Volte.Data.Dapper
{
    public class DapperException : Exception {
        // Methods
        public DapperException(Exception e) : base("Entity Exception")
        {
            _ErrorType = ExceptionTypes.Unknown;
            _ErrorSource = e;
        }

        public DapperException(string message) : base(message)
        {
            _ErrorType = ExceptionTypes.Unknown;
        }

        public DapperException(string message, ExceptionTypes errorType) : base(message)
        {
            _ErrorType = errorType;
        }


        // Properties
        public Exception ErrorSource
        {
            get {
                return _ErrorSource;
            }
        }

        public ExceptionTypes ErrorType
        {
            get {
                return _ErrorType;
            }
        }


        // Fields
        private Exception _ErrorSource;
        private ExceptionTypes _ErrorType;
    }
}
