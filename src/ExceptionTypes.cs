using System;

namespace Volte.Data.Dapper
{
    public enum ExceptionTypes {
        // Fields
        FormatException         = 0,
        NotFound                = 1,
        XmlError                = 2,
        Unknown                 = 3,
        DatabaseError           = 4,
        DatabaseConnectionError = 5,
        DatabaseUnknwnError     = 6,
        DataTooLong             = 8,
        NotUnique               = 7,
        NotAllowStringEmpty     = 9,
        NotAllowDataNull        = 10,
        DataTypeNotMatch        = 11,
        AutoValueOn             = 12,
        UpdateFail              = 13,
        RestrictError           = 14,
        RequireAttribute        = 15,
        PesistentError          = 16,
        DirtyEntity             = 17,
        NotDataIsReadonly       = 18
    }
}
