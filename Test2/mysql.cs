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

            var connStr = @"Database='bi';Port=4000;Data Source=127.0.0.1;User ID=root;Password=D4uEQV/vPD@z3AyhtU;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";

            connStr = @"Database='bi';Port=3306;Data Source=192.168.2.3;User ID=root;Password=123456;CharSet=utf8;SslMode=None;Convert Zero Datetime=True;Allow Zero Datetime=True";

            var providerName = @"MySql.Data.MySqlClient";

            providerName = "System.Data.SqlClient";
            providerName = @"MySql";

            Settings.Instance().GetValue("MySqlUnitTest" , providerName , connStr);

//VJ
            /*
            string patten="ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int  i=0;i<1;i++){
                for (int  j=0;j<1;j++){
                    try{

                        string sql1=@"INSERT INTO `bi_flow_flows`(`id`, `flowId`, `active`, `corporationid`, `formType`, `flowType`, `sense`, `ownerId`, `payeeId`, `consumptionReasons`, `paymentChannel`, `specificationVersionId`, `specificationId`, `amount`, `CODE`, `submitDate`, `payDate`, `description`, `expenseMoney`, `loanMoney`, `repaymentDate`, `loanDate`, `loanDepartment`, `title`, `expenseDate`, `feeDate`, `writtenOffMoney`, `expenseDepartment`, `payMoney`, `submitterId`, `state`, `invoice`, `feeTypeId`, `project`, `u_d0001`, `u_d0002`, `u_d0003`, `u_d0004`, `u_d0005`, `u_d0006`, `u_d0007`, `u_d0008`, `u_d0009`, `u_d0010`, `u_d0011`, `u_d0012`, `u_d0013`, `u_d0014`, `u_d0015`, `u_d0016`, `u_d0017`, `u_d0018`, `u_d0019`, `u_d0020`, `u_d0021`, `u_d0022`, `u_d0023`, `u_d0024`, `u_d0025`, `u_d0026`, `u_d0027`, `u_d0028`, `u_d0029`, `u_d0030`, `u_n0001`, `u_n0002`, `u_n0003`, `u_n0004`, `u_n0005`, `u_n0006`, `u_n0007`, `u_n0008`, `u_n0009`, `u_n0010`, `u_n0011`, `u_n0012`, `u_n0013`, `u_n0014`, `u_n0015`, `u_n0016`, `u_n0017`, `u_n0018`, `u_n0019`, `u_n0020`, `u_f0001`, `u_f0002`, `u_f0003`, `u_f0004`, `u_f0005`, `u_f0006`, `u_f0007`, `u_f0008`, `u_f0009`, `u_f0010`, `u_f0011`, `u_f0012`, `u_f0013`, `u_f0014`, `u_f0015`, `u_f0016`, `u_f0017`, `u_f0018`, `u_f0019`, `u_f0020`, `u_b0001`, `u_b0002`, `u_b0003`, `u_b0004`, `u_b0005`, `u_b0006`, `u_b0007`, `u_b0008`, `u_b0009`, `u_b0010`, `u_b0011`, `u_b0012`, `u_b0013`, `u_b0014`, `u_b0015`, `u_b0016`, `u_b0017`, `u_b0018`, `u_b0019`, `u_b0020`, `formSpecificationId`, `feeTypeSpecificationId`, `paymentAccountId`, `apportionMoney`, `apportionPercent`, `requisitionMoney`, `requisitionDate`, `feeDatePeriod_end`, `feeDatePeriod_start`, `datePeriod_start`, `datePeriod_end`, `expenseLink`, `city`, `fromCity`, `toCity`, `payerId`, `updateTime`, `nature`, `department`)
                            select CONCAT('@',id), CONCAT('@',flowId), `active`, CONCAT('@',corporationid), `formType`, `flowType`, `sense`, `ownerId`, `payeeId`, `consumptionReasons`, `paymentChannel`, `specificationVersionId`, `specificationId`, `amount`, `CODE`, `submitDate`, `payDate`, `description`, `expenseMoney`, `loanMoney`, `repaymentDate`, `loanDate`, `loanDepartment`, `title`, `expenseDate`, `feeDate`, `writtenOffMoney`, `expenseDepartment`, `payMoney`, `submitterId`, `state`, `invoice`, `feeTypeId`, `project`, `u_d0001`, `u_d0002`, `u_d0003`, `u_d0004`, `u_d0005`, `u_d0006`, `u_d0007`, `u_d0008`, `u_d0009`, `u_d0010`, `u_d0011`, `u_d0012`, `u_d0013`, `u_d0014`, `u_d0015`, `u_d0016`, `u_d0017`, `u_d0018`, `u_d0019`, `u_d0020`, `u_d0021`, `u_d0022`, `u_d0023`, `u_d0024`, `u_d0025`, `u_d0026`, `u_d0027`, `u_d0028`, `u_d0029`, `u_d0030`, `u_n0001`, `u_n0002`, `u_n0003`, `u_n0004`, `u_n0005`, `u_n0006`, `u_n0007`, `u_n0008`, `u_n0009`, `u_n0010`, `u_n0011`, `u_n0012`, `u_n0013`, `u_n0014`, `u_n0015`, `u_n0016`, `u_n0017`, `u_n0018`, `u_n0019`, `u_n0020`, `u_f0001`, `u_f0002`, `u_f0003`, `u_f0004`, `u_f0005`, `u_f0006`, `u_f0007`, `u_f0008`, `u_f0009`, `u_f0010`, `u_f0011`, `u_f0012`, `u_f0013`, `u_f0014`, `u_f0015`, `u_f0016`, `u_f0017`, `u_f0018`, `u_f0019`, `u_f0020`, `u_b0001`, `u_b0002`, `u_b0003`, `u_b0004`, `u_b0005`, `u_b0006`, `u_b0007`, `u_b0008`, `u_b0009`, `u_b0010`, `u_b0011`, `u_b0012`, `u_b0013`, `u_b0014`, `u_b0015`, `u_b0016`, `u_b0017`, `u_b0018`, `u_b0019`, `u_b0020`, `formSpecificationId`, `feeTypeSpecificationId`, `paymentAccountId`, `apportionMoney`, `apportionPercent`, `requisitionMoney`, `requisitionDate`, `feeDatePeriod_end`, `feeDatePeriod_start`, `datePeriod_start`, `datePeriod_end`, `expenseLink`, `city`, `fromCity`, `toCity`, `payerId`, `updateTime`, `nature`, `department`
                            from t_bi_flow_flows limit 0,10000;";

                        string sql2=@"INSERT INTO `bi_flow_flows_name`(`id`, `flowId`, `corporationid`, `formType`, `flowType`, `ownerId`, `payeeId`, `paymentChannel`, `specificationVersionId`, `specificationId`, `loanDepartment`, `expenseDepartment`, `submitterId`, `department`, `state`, `feeTypeId`, `project`, `formSpecificationId`, `feeTypeSpecificationId`, `paymentAccountId`, `expenseLink`, `city`, `fromCity`, `toCity`, `payerId`)
                            select CONCAT('@',id), CONCAT('@',flowId), CONCAT('@',corporationid), `formType`, `flowType`, `ownerId`, `payeeId`, `paymentChannel`, `specificationVersionId`, `specificationId`, `loanDepartment`, `expenseDepartment`, `submitterId`, `department`, `state`, `feeTypeId`, `project`, `formSpecificationId`, `feeTypeSpecificationId`, `paymentAccountId`, `expenseLink`, `city`, `fromCity`, `toCity`, `payerId` from t_bi_flow_flows_name limit 0,10000;";

                        string sql3=@"INSERT INTO `bi_flow_flows_props`(`id`, `flowId`, `corporationid`, `u_c0001`, `u_c0002`, `u_c0003`, `u_c0004`, `u_c0005`, `u_c0006`, `u_c0007`, `u_c0008`, `u_c0009`, `u_c0010`, `u_c0011`, `u_c0012`, `u_c0013`, `u_c0014`, `u_c0015`, `u_c0016`, `u_c0017`, `u_c0018`, `u_c0019`, `u_c0020`, `u_c0021`, `u_c0022`, `u_c0023`, `u_c0024`, `u_c0025`, `u_c0026`, `u_c0027`, `u_c0028`, `u_c0029`, `u_c0030`, `u_c0031`, `u_c0032`, `u_c0033`, `u_c0034`, `u_c0035`, `u_c0036`, `u_c0037`, `u_c0038`, `u_c0039`, `u_c0040`, `u_c0041`, `u_c0042`, `u_c0043`, `u_c0044`, `u_c0045`, `u_c0046`, `u_c0047`, `u_c0048`, `u_c0049`, `u_c0050`, `u_c0051`, `u_c0052`, `u_c0053`, `u_c0054`, `u_c0055`, `u_c0056`, `u_c0057`, `u_c0058`, `u_c0059`, `u_c0060`, `u_c0061`, `u_c0062`, `u_c0063`, `u_c0064`, `u_c0065`, `u_c0066`, `u_c0067`, `u_c0068`, `u_c0069`, `u_c0070`, `u_c0071`, `u_c0072`, `u_c0073`, `u_c0074`, `u_c0075`, `u_c0076`, `u_c0077`, `u_c0078`, `u_c0079`, `u_c0080`, `u_c0081`, `u_c0082`, `u_c0083`, `u_c0084`, `u_c0085`, `u_c0086`, `u_c0087`, `u_c0088`, `u_c0089`, `u_c0090`, `u_c0091`, `u_c0092`, `u_c0093`, `u_c0094`, `u_c0095`, `u_c0096`, `u_c0097`, `u_c0098`, `u_c0099`, `u_c0100`, `u_c0101`, `u_c0102`, `u_c0103`, `u_c0104`, `u_c0105`, `u_c0106`, `u_c0107`, `u_c0108`, `u_c0109`, `u_c0110`, `u_c0111`, `u_c0112`, `u_c0113`, `u_c0114`, `u_c0115`, `u_c0116`, `u_c0117`, `u_c0118`, `u_c0119`, `u_c0120`)
                            select CONCAT('@',id), CONCAT('@',flowId), CONCAT('@',corporationid), `u_c0001`, `u_c0002`, `u_c0003`, `u_c0004`, `u_c0005`, `u_c0006`, `u_c0007`, `u_c0008`, `u_c0009`, `u_c0010`, `u_c0011`, `u_c0012`, `u_c0013`, `u_c0014`, `u_c0015`, `u_c0016`, `u_c0017`, `u_c0018`, `u_c0019`, `u_c0020`, `u_c0021`, `u_c0022`, `u_c0023`, `u_c0024`, `u_c0025`, `u_c0026`, `u_c0027`, `u_c0028`, `u_c0029`, `u_c0030`, `u_c0031`, `u_c0032`, `u_c0033`, `u_c0034`, `u_c0035`, `u_c0036`, `u_c0037`, `u_c0038`, `u_c0039`, `u_c0040`, `u_c0041`, `u_c0042`, `u_c0043`, `u_c0044`, `u_c0045`, `u_c0046`, `u_c0047`, `u_c0048`, `u_c0049`, `u_c0050`, `u_c0051`, `u_c0052`, `u_c0053`, `u_c0054`, `u_c0055`, `u_c0056`, `u_c0057`, `u_c0058`, `u_c0059`, `u_c0060`, `u_c0061`, `u_c0062`, `u_c0063`, `u_c0064`, `u_c0065`, `u_c0066`, `u_c0067`, `u_c0068`, `u_c0069`, `u_c0070`, `u_c0071`, `u_c0072`, `u_c0073`, `u_c0074`, `u_c0075`, `u_c0076`, `u_c0077`, `u_c0078`, `u_c0079`, `u_c0080`, `u_c0081`, `u_c0082`, `u_c0083`, `u_c0084`, `u_c0085`, `u_c0086`, `u_c0087`, `u_c0088`, `u_c0089`, `u_c0090`, `u_c0091`, `u_c0092`, `u_c0093`, `u_c0094`, `u_c0095`, `u_c0096`, `u_c0097`, `u_c0098`, `u_c0099`, `u_c0100`, `u_c0101`, `u_c0102`, `u_c0103`, `u_c0104`, `u_c0105`, `u_c0106`, `u_c0107`, `u_c0108`, `u_c0109`, `u_c0110`, `u_c0111`, `u_c0112`, `u_c0113`, `u_c0114`, `u_c0115`, `u_c0116`, `u_c0117`, `u_c0118`, `u_c0119`, `u_c0120` from t_bi_flow_flows_props;";

                        string sql4=@"INSERT INTO `bi_flow_flows_props_name`(`id`, `flowId`, `corporationid`, `u_c0001`, `u_c0002`, `u_c0003`, `u_c0004`, `u_c0005`, `u_c0006`, `u_c0007`, `u_c0008`, `u_c0009`, `u_c0010`, `u_c0011`, `u_c0012`, `u_c0013`, `u_c0014`, `u_c0015`, `u_c0016`, `u_c0017`, `u_c0018`, `u_c0019`, `u_c0020`, `u_c0021`, `u_c0022`, `u_c0023`, `u_c0024`, `u_c0025`, `u_c0026`, `u_c0027`, `u_c0028`, `u_c0029`, `u_c0030`, `u_c0031`, `u_c0032`, `u_c0033`, `u_c0034`, `u_c0035`, `u_c0036`, `u_c0037`, `u_c0038`, `u_c0039`, `u_c0040`, `u_c0041`, `u_c0042`, `u_c0043`, `u_c0044`, `u_c0045`, `u_c0046`, `u_c0047`, `u_c0048`, `u_c0049`, `u_c0050`, `u_c0051`, `u_c0052`, `u_c0053`, `u_c0054`, `u_c0055`, `u_c0056`, `u_c0057`, `u_c0058`, `u_c0059`, `u_c0060`, `u_c0061`, `u_c0062`, `u_c0063`, `u_c0064`, `u_c0065`, `u_c0066`, `u_c0067`, `u_c0068`, `u_c0069`, `u_c0070`, `u_c0071`, `u_c0072`, `u_c0073`, `u_c0074`, `u_c0075`, `u_c0076`, `u_c0077`, `u_c0078`, `u_c0079`, `u_c0080`, `u_c0081`, `u_c0082`, `u_c0083`, `u_c0084`, `u_c0085`, `u_c0086`, `u_c0087`, `u_c0088`, `u_c0089`, `u_c0090`, `u_c0091`, `u_c0092`, `u_c0093`, `u_c0094`, `u_c0095`, `u_c0096`, `u_c0097`, `u_c0098`, `u_c0099`, `u_c0100`, `u_c0101`, `u_c0102`, `u_c0103`, `u_c0104`, `u_c0105`, `u_c0106`, `u_c0107`, `u_c0108`, `u_c0109`, `u_c0110`, `u_c0111`, `u_c0112`, `u_c0113`, `u_c0114`, `u_c0115`, `u_c0116`, `u_c0117`, `u_c0118`, `u_c0119`, `u_c0120`)
                            select CONCAT('@',id), CONCAT('@',flowId), CONCAT('@',corporationid), `u_c0001`, `u_c0002`, `u_c0003`, `u_c0004`, `u_c0005`, `u_c0006`, `u_c0007`, `u_c0008`, `u_c0009`, `u_c0010`, `u_c0011`, `u_c0012`, `u_c0013`, `u_c0014`, `u_c0015`, `u_c0016`, `u_c0017`, `u_c0018`, `u_c0019`, `u_c0020`, `u_c0021`, `u_c0022`, `u_c0023`, `u_c0024`, `u_c0025`, `u_c0026`, `u_c0027`, `u_c0028`, `u_c0029`, `u_c0030`, `u_c0031`, `u_c0032`, `u_c0033`, `u_c0034`, `u_c0035`, `u_c0036`, `u_c0037`, `u_c0038`, `u_c0039`, `u_c0040`, `u_c0041`, `u_c0042`, `u_c0043`, `u_c0044`, `u_c0045`, `u_c0046`, `u_c0047`, `u_c0048`, `u_c0049`, `u_c0050`, `u_c0051`, `u_c0052`, `u_c0053`, `u_c0054`, `u_c0055`, `u_c0056`, `u_c0057`, `u_c0058`, `u_c0059`, `u_c0060`, `u_c0061`, `u_c0062`, `u_c0063`, `u_c0064`, `u_c0065`, `u_c0066`, `u_c0067`, `u_c0068`, `u_c0069`, `u_c0070`, `u_c0071`, `u_c0072`, `u_c0073`, `u_c0074`, `u_c0075`, `u_c0076`, `u_c0077`, `u_c0078`, `u_c0079`, `u_c0080`, `u_c0081`, `u_c0082`, `u_c0083`, `u_c0084`, `u_c0085`, `u_c0086`, `u_c0087`, `u_c0088`, `u_c0089`, `u_c0090`, `u_c0091`, `u_c0092`, `u_c0093`, `u_c0094`, `u_c0095`, `u_c0096`, `u_c0097`, `u_c0098`, `u_c0099`, `u_c0100`, `u_c0101`, `u_c0102`, `u_c0103`, `u_c0104`, `u_c0105`, `u_c0106`, `u_c0107`, `u_c0108`, `u_c0109`, `u_c0110`, `u_c0111`, `u_c0112`, `u_c0113`, `u_c0114`, `u_c0115`, `u_c0116`, `u_c0117`, `u_c0118`, `u_c0119`, `u_c0120` from `t_bi_flow_flows_props_name` limit 0,10000;";

string sql5=@"INSERT INTO `bi_flow_flows_logs`(`id`, `flowId`, `corporationid`, `formType`, `flowType`, `time`, `action`, `state`, `operatorId`, `comment`, `lasttime`, `submitterId`, `submitDate`, `days`, `code`, `department`) 
select CONCAT('@',id), CONCAT('@',flowId), CONCAT('@',corporationid), `formType`, `flowType`, `time`, `action`, `state`, `operatorId`, `comment`, `lasttime`, `submitterId`, `submitDate`, `days`, `code`, `department` from t_bi_flow_flows_logs;";

string sql6=@"INSERT INTO `property_define`(`sKey`, `corporationId`, `tableName`, `NAME`, `label`, `dbType`, `dbColName`, `isRef`, `isDimension`, `isMeasure`, `isInUse`, `hasDict`, `dictName`, `remark`, `size`, `sequency`, `active`, `ability`, `createTime`) 
select CONCAT('@',sKey), CONCAT('@',corporationid),`tableName`, `NAME`, `label`, `dbType`, `dbColName`, `isRef`, `isDimension`, `isMeasure`, `isInUse`, `hasDict`, `dictName`, `remark`, `size`, `sequency`, `active`, `ability`, `createTime`  from t_property_define;";


                        string s=patten.Substring(i,1)+patten.Substring(j,1);

                        sql1=sql1.Replace("@",s);
                        sql2=sql2.Replace("@",s);
                        sql3=sql3.Replace("@",s);
                        sql4=sql4.Replace("@",s);
                        sql5=sql5.Replace("@",s);
                        sql6=sql6.Replace("@",s);

                        Console.WriteLine(s);

                        //Push(new DbContext("MySqlUnitTest", providerName, connStr),sql1);

                       // Push(new DbContext("MySqlUnitTest", providerName, connStr),sql2);
                       //
                       // Push(new DbContext("MySqlUnitTest", providerName, connStr),sql3);
                       //
                       // Push(new DbContext("MySqlUnitTest", providerName, connStr),sql4);
                       // 
                       // Push(new DbContext("MySqlUnitTest", providerName, connStr),sql5);
                       // 
                       // Push(new DbContext("MySqlUnitTest", providerName, connStr),sql6);

                    }
                    catch (Exception ErrMsg)
                    {
                        Console.WriteLine(ErrMsg.ToString());

                    }
                    Thread.Sleep(1000);
                }

            }

*/
            Exp(new DbContext("MySqlUnitTest", providerName, connStr),"zZU5Rjr+Jk1k00");

        }

        public static void Push(DbContext _Trans , string sql)
        {

            try{
                _Trans.Execute(sql);
                Console.WriteLine(".");
            }
            catch (Exception ErrMsg)
            {
                Console.WriteLine(ErrMsg.ToString());
            }
            _Trans.Close();
            Thread.Sleep(2000);

        }

        public static void Exp(DbContext _Trans , string sCode)
        {

            QueryRows RsSysRef   = new QueryRows(_Trans);
            RsSysRef.CommandText = "SELECT * From define_tables where corporationid=@sCode and id=@id order by corporationId,id";
            RsSysRef.SetParameter("sCode","a");
            RsSysRef.SetParameter("id","22");
            RsSysRef.Open();

            Console.WriteLine("");
            Console.WriteLine("flows");
            Console.WriteLine("------------");
            while (!RsSysRef.EOF) {

                Console.WriteLine(RsSysRef.GetValue("tableName"));
                RsSysRef.MoveNext();
            }
                            Console.Write("--------");

            RsSysRef.Close();

           


        }

    }
}
