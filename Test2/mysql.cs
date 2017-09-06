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

            var connStr = @"Database='ekb400';Data Source=192.168.2.224;User ID=root;Password=root;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";
            var providerName = @"MySql.Data.MySqlClient";

            providerName = "System.Data.SqlClient";
            providerName = @"MySql";

            Settings.Instance().GetValue("MySqlUnitTest" , providerName , connStr);

            DbContext _Trans  = new DbContext("MySqlUnitTest", providerName, connStr);

            Exp(_Trans,"zZU5Rjr+Jk1k00");

        }

        public static void Exp(DbContext _Trans , string sCode)
        {

            QueryRows RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From flow_flow where corporationid='"+sCode+"' order by corporationId,id";
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("flows");
            while (!RsSysRef.EOF) {
                _Trans.Execute("update flow_flow set $updatetime=date_add($updatetime, interval 1 second) where id='"+RsSysRef.GetValue("id")+"'");

                Console.Write(".");
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();

            RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT id From organization_staff  where corporationid='"+sCode+"' order by corporationId,id";
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("staff");
            while (!RsSysRef.EOF) {
                _Trans.Execute("update organization_staff set $updatetime=date_add($updatetime, interval 1 second) where id='"+RsSysRef.GetValue("id")+"'");

                Console.Write(".");
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();

            RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT id From organization_department where corporationid='"+sCode+"'  order by corporationId,id";
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("department");
            while (!RsSysRef.EOF) {
                _Trans.Execute("update organization_department set $updatetime=date_add($updatetime, interval 1 second) where id='"+RsSysRef.GetValue("id")+"'");

                Console.Write(".");
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();

            RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From flow_feeType where corporationid='"+sCode+"'  order by corporationId,id";
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("flow_feeType");
            while (!RsSysRef.EOF) {
                _Trans.Execute("update flow_feeType set $updatetime=date_add($updatetime, interval 1 second) where id='"+RsSysRef.GetValue("id")+"'");

                Console.Write(".");
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();

            RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From basedata_dimensionItem where corporationid='"+sCode+"'  order by corporationId,id";
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("basedata_dimensionItem");
            while (!RsSysRef.EOF) {
                _Trans.Execute("update basedata_dimensionItem set $updatetime=date_add($updatetime, interval 1 second) where id='"+RsSysRef.GetValue("id")+"'");

                Console.Write(".");
                RsSysRef.MoveNext();
            }
            RsSysRef.Close();


        }

    }
}
