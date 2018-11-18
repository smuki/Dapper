@echo on
del *.bak
copy ..\dist\Volte.Data.Dapper.*

%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc mysql.cs /w:2 /debug+ /r:Volte.Data.Dapper.dll,Volte.Data.Json.dll
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc mssql.cs /w:2 /debug+ /r:Volte.Data.Dapper.dll,Volte.Data.Json.dll,Volte.Utils.dll
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc pgsql.cs /w:2 /debug+ /r:Volte.Data.Dapper.dll,Volte.Data.Json.dll,Volte.Utils.dll


%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc Vertica.cs /w:2 /debug+ /r:Volte.Data.Dapper.dll,Volte.Data.Json.dll,Vertica.Data.dll
