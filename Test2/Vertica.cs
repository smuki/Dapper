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
using Vertica.Data.VerticaClient;

namespace JitEngine.TDriver
{

    public class Multiple {

        public static void Main(string[] args)
        {

            string connectString = "DATABASE=docker;HOST=192.168.2.219;USER=dbadmin;Password=";

            VerticaConnection  _conn = new VerticaConnection(connectString);

            _conn.Open();
            //Perform some operations
            VerticaCommand command = _conn.CreateCommand();
         //   for (int i=1;i<20;i++){
         //       command = _conn.CreateCommand();
         //       command.CommandText =
         //           "INSERT into test values('w"+i+"', '"+i+"', "+i+", "+i+")";
         //       Int32 rowsAdded = command.ExecuteNonQuery();
         //       Console.WriteLine( rowsAdded + " rows added!");
         //   }

            command = _conn.CreateCommand();
            command.CommandText ="select id,amount from bi_flow_flows";
            VerticaDataReader dr = command.ExecuteReader();

            Console.WriteLine("\n\n Fat Content\t  Product Description");
            Console.WriteLine("------------\t  -------------------");
            Console.WriteLine(dr.Read());
            int rows = 0;
            while (dr.Read())
            {
                Console.WriteLine("     " + dr[0] + "    \t  " + dr[1]);
                ++rows;
            }
            Console.WriteLine("------------\n  (" + rows + " rows)\n");
            dr.Close();

            _conn.Close();

            //            var connStr = @"Database='hrms_l';Data Source=192.168.0.203;User ID=root;Password=;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";
           //             var providerName = @"MySql.Data.MySqlClient";
            //
            //            providerName = "System.Data.SqlClient";
            //            providerName = @"Vertica";

            //            Settings.Instance().GetValue("VerticaUnitTest" , providerName , connectString);

            //            DbContext _Trans  = new DbContext("VerticaUnitTest");

            //            QueryRows RsSysRef   = new QueryRows(_Trans);
            //            RsSysRef.CommandText = "SELECT * From test";
            //            RsSysRef.Open();

            //            Console.WriteLine(RsSysRef.CommandText);

            //            while (!RsSysRef.EOF) {
            //                string sTableName1 = RsSysRef.GetValue("cus_id");
            //                Console.Write("----");
            //                Console.Write(sTableName1);
            //                //Console.WriteLine(RsSysRef.GetBoolean("cus_order"));

            //                Console.Write("----");
            //                Console.Write(RsSysRef.GetValue("cus_order"));
            //                RsSysRef.MoveNext();
            //            }
            //            RsSysRef.Close();
                        //_Trans.Execute("delete from test");

        }

    }
}
