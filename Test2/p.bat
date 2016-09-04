@echo on
del *.bak
copy D:\System\igs\trunk\NUnit\Volte.Data.Dapper.*

%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc mysql.cs /w:2 /debug+ /r:Volte.Data.Dapper.dll,Volte.Data.Json.dll
