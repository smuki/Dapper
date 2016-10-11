using System;
//using System.Web;
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

namespace JitEngine.TDriver
{

    public class Multiple {

        public static void Main(string[] args)
        {

            var connStr = @"Database='hrms_l';Data Source=192.168.0.203;User ID=root;Password=;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";
            var providerName = @"MySql.Data.MySqlClient";

            providerName = "System.Data.SqlClient";
            providerName = @"MySql";

            Settings.Instance().GetValue("MySqlUnitTest" , providerName , connStr);

            DbContext _Trans  = new DbContext("MySqlUnitTest", providerName, connStr);

            QueryRows RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From syscaption";
            RsSysRef.Open();

            Console.WriteLine(RsSysRef.CommandText);

            while (!RsSysRef.EOF) {
                string sTableName1 = RsSysRef.GetValue("sCaptionCode");
                Console.WriteLine("----");
                Console.WriteLine(sTableName1);
                Console.WriteLine(RsSysRef.GetValue("sCaption"));
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();

        }

    }
}
