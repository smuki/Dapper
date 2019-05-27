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

        static int nSequency = 100100;
        static string  sCorporation  = "";

        public static void Main(string[] args)
        {

            var connStr = @"Database='designer';Data Source=192.168.2.6;User ID=root;Password=;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";
            var providerName = @"MySql.Data.MySqlClient";
            connStr = @"Server=127.0.0.1;Database=designer;Uid=sa;Pwd=Mm123456;";

            providerName = "System.Data.SqlClient";
            providerName = @"MsSqlServer";

            Settings.Instance().GetValue("MySqlUnitTest" , providerName , connStr);

            DbContext _Trans  = new DbContext("MySqlUnitTest", providerName, connStr);


            for (int i=100;i<2000;i++){
                //  _Trans.Execute("update Doc_copy set corporationId='cc"+i+"'");
               // _Trans.Execute("insert into doc select * from Doc_copy");
                Console.Write(".");
            }

            //for (int i=1000;i<2000;i++){

                sCorporation="ding27b1a375a0aea94535c2f4657eb6378f";
                Exp(_Trans , 0 , "" , "");
            //}
        }

        public static JSONArray Exp(DbContext _Trans , int nLevel , string sDept , string sCode)
        {

            nLevel++;
            QueryRows RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From analysis where sCategory='YKB-Dept' AND  sParent='"+sCode+"'";
            RsSysRef.Open();

            //Console.WriteLine(RsSysRef.CommandText);
            Console.Write(".");

            JSONArray _JSONArray = new JSONArray();

            string sDescription = "";
            string sAnalysis    = "";

            if (RsSysRef.EOF) {

                sDescription = sCode;
                sAnalysis    = sCode;

                JSONObject _JSONObject = new JSONObject();
                _JSONObject.SetValue("sCode"        , sCode);
                _JSONObject.SetValue("sDescription" , sCode);
                _JSONObject.SetValue("sDept"        , sDept);
                _JSONArray.Add(_JSONObject);

                nSequency=nSequency+10;
                _Trans.Execute("INSERT INTO [Dept] ([sDept], [sDescription], [sCode], [sKey], [sOrganization], [sCorporation], [nLevel], [nSequency]) VALUES (N'"+sDept+"', N'"+sDescription+"', N'"+sAnalysis+"', N'"+Volte.Utils.IdGenerator.NewBase36()+"', N'', N'', '"+nLevel+"', '"+nSequency+"')");

            }
            while (!RsSysRef.EOF) {

                sDescription = RsSysRef.GetValue("sDescription01");
                sAnalysis    = RsSysRef.GetValue("sAnalysis");

                JSONObject _JSONObject = new JSONObject();
                _JSONObject.SetValue("sCode"        , sAnalysis);
                _JSONObject.SetValue("sDescription" , sDescription);
                _JSONObject.SetValue("sDept"        , sDept);
                _JSONArray.Add(_JSONObject);

                nSequency = nSequency+10;

                _Trans.Execute("INSERT INTO [Dept] ([sDept], [sDescription], [sCode], [sKey], [sOrganization], [sCorporation], [nLevel], [nSequency]) VALUES (N'"+sDept+"', N'"+sDescription+"', N'"+sAnalysis+"', N'"+Volte.Utils.IdGenerator.NewBase36()+"', N'', N'', '"+nLevel+"', '"+nSequency+"')");

                JSONArray _a = Multiple.Exp(_Trans , nLevel , sCode , sAnalysis);

                foreach (JSONObject _JSONObject2 in _a.JSONObjects) {

                    sAnalysis    = _JSONObject2.GetValue("sCode");
                    sDescription = _JSONObject2.GetValue("sDescription");

                    nSequency=nSequency+10;
                    _Trans.Execute("INSERT INTO [Dept] ([sDept], [sDescription], [sCode], [sKey], [sOrganization], [sCorporation], [nLevel], [nSequency]) VALUES (N'"+sDept+"', N'"+sDescription+"', N'"+sAnalysis+"', N'"+Volte.Utils.IdGenerator.NewBase36()+"', N'', N'"+sCorporation+"', '"+nLevel+"', '"+nSequency+"')");

                    _JSONArray.Add(_JSONObject2);
                }

                RsSysRef.MoveNext();
            }
            RsSysRef.Close();

            return _JSONArray;
        }

    }
}
