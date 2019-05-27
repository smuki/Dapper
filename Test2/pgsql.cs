using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;

using Volte.Data.Dapper;
using Volte.Data.Json;

namespace JitEngine.TDriver
{

    public class Multiple {

        public static void Main(string[] args)
        {

            var connStr = @"Database='ekbbi';Port=4000;Data Source=127.0.0.1;User ID=root;Password=D4uEQV/vPD@z3AyhtU;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";

            connStr = @"Database='ekbbi420';Port=8066;Data Source=127.0.0.1;User ID=ekbdbbi002;Password=D4uEQV/vPD@z3AyhtU;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";

            var providerName = @"MySql.Data.MySqlClient";

            providerName = "System.Data.SqlClient";
            providerName = @"MySql";

						connStr = "Server=192.168.2.86;Port=26257;User Id=root;Password=;Database=testdb;";  
            providerName = @"Npgsql";

            Settings.Instance().GetValue("MySqlUnitTest" , providerName , connStr);
						string sSql="";
            Exp(new DbContext("MySqlUnitTest", providerName, connStr),sSql);
            
        }

        public static void Exp(DbContext _Trans , string sSql)
        {
            QueryRows RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From t2";
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("flows");
            while (!RsSysRef.EOF) {

                Console.WriteLine(RsSysRef.GetValue(0)+" - "+RsSysRef.GetValue(1)+" - "+RsSysRef.GetValue(2));
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();
        }
    }
}
