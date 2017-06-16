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

            var connStr = @"Database='designer';Data Source=192.168.2.6;User ID=root;Password=;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";
            var providerName = @"MySql.Data.MySqlClient";

            providerName = "System.Data.SqlClient";
            providerName = @"MySql";

            Settings.Instance().GetValue("MySqlUnitTest" , providerName , connStr);

            DbContext _Trans  = new DbContext("MySqlUnitTest", providerName, connStr);


        }

        public static JSONArray Exp(DbContext _Trans , int nLevel , string sDept , string sCode)
        {

            nLevel++;
            QueryRows RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From analysis where sCategory='YKB-Dept' AND  sParent='"+sCode+"'";
            RsSysRef.Open();

            Console.WriteLine(RsSysRef.CommandText);
            JSONArray _JSONArray = new JSONArray();

            while (!RsSysRef.EOF) {

                JSONObject _JSONObject = new JSONObject();
                _JSONObject.SetValue("sCode" , RsSysRef.GetValue("sAnalysis"));
                _JSONObject.SetValue("sDept" , sDept);
                _JSONArray.Add(_JSONObject);

                JSONArray _a = Exp(_Trans , nLevel , RsSysRef.GetValue("sAnalysis") , RsSysRef.GetValue("sAnalysis"));


                RsSysRef.MoveNext();
            }
            RsSysRef.Close();
            return _JSONArray;

        }

    }
}
