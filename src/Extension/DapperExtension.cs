using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

using Volte.Data.Json;

namespace Volte.Data.Dapper
{
    public static class DapperExtension {
        const string ZFILE_NAME   = "DapperExtension";
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="t"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static bool AddNew<T> (this DbContext dbs, T t, int ? commandTimeout = null) where T : class, new()
        {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;
            QueryBuilder sql = QueryBuilder<T>.Builder(dbs);

            ZZLogger.Debug(ZFILE_NAME, t);

            sql = sql.AppendParam<T> (t);

            var flag = db.Execute(sql.InsertSql, t, _dbtransaction, commandTimeout);

            return flag == 1;
        }

        /// <summary>
        ///  批量插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="lt"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static bool AddNew<T> (this DbContext dbs, IList<T> lt, int ? commandTimeout = null) where T : class, new()
        {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;
            var sql = QueryBuilder<T>.Builder(dbs);

            var flag = db.Execute(sql.InsertSql, lt, _dbtransaction, commandTimeout);

//            Console.WriteLine("AddNew "+flag);

            return flag == lt.Count;
        }

        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool Delete<T> (this DbContext dbs, QueryBuilder sql = null) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            var f = db.Execute(sql.DeleteSql, sql.Param, _dbtransaction);
//            Console.WriteLine("DELETE "+f);
            ZZLogger.Debug(ZFILE_NAME, "DELETE1");
            return f > 0;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="t">如果sql为null，则根据t的主键进行删除</param>
        /// <param name="sql">按条件删除</param>
        /// <returns></returns>
        public static bool Delete<T> (this DbContext dbs, T t, QueryBuilder sql = null) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            sql = sql.AppendParam<T> (t);

            var f = db.Execute(sql.DeleteSql, sql.Param, _dbtransaction);

//            Console.WriteLine(sql.DeleteSql);
//            Console.WriteLine("DELETE "+f);
            ZZLogger.Debug(ZFILE_NAME, "DELETE2");
            return f > 0;
        }


        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="t">如果sql为null，则根据t的主键进行修改</param>
        /// <param name="sql">按条件修改</param>
        /// <returns></returns>
        public static bool Update<T> (this DbContext dbs, T t, QueryBuilder sql = null) where T : class, new()
        {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null) {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            sql = sql.AppendParam<T> (t);
            ZZLogger.Debug(ZFILE_NAME , "UPDATE " + sql.UpdateSql);
            var f = db.Execute(sql.UpdateSql, sql.Param, _dbtransaction);

            ZZLogger.Debug(ZFILE_NAME , "UPDATE " + sql.UpdateSql);
            ZZLogger.Debug(ZFILE_NAME , "UPDATE " + f);
            ZZLogger.Debug(ZFILE_NAME , "UPDATE " + dbs.Transaction);
            return f > 0;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="t">如果sql为null，则根据t的主键进行修改</param>
        /// <param name="sql">按条件修改</param>
        /// <returns></returns>
        public static T Query<T> (this DbContext dbs, T t, QueryBuilder sql = null) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            sql = sql.AppendParam<T> (t);
            sql = sql.Top(1);
            var result = db.Query<T> (sql.QuerySql, sql.Param, _dbtransaction);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// 获取默认一条数据，没有则为NULL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static T SingleOrDefault<T> (this DbContext dbs, QueryBuilder sql) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            sql = sql.Top(1);
            var result = db.Query<T>(sql.QuerySql, sql.Param, _dbtransaction);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// 获取默认一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static T SingleOrDefault<T> (this DbContext dbs, QueryBuilder sql, int ZZRWN) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            sql = sql.Top(1, ZZRWN);
            ZZLogger.Debug(ZFILE_NAME, "xxx " + ZZRWN + "  " + sql.QuerySql);
            var result = db.Query<T> (sql.QuerySql, sql.Param, _dbtransaction);
            return result.FirstOrDefault();
        }


        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbs"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static IList<T> Query<T> (this DbContext dbs, QueryBuilder sql = null) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            var result = db.Query<T> (sql.QuerySql, sql.Param, _dbtransaction);
            return result.ToList();
        }

        ///// <summary>
        ///// 数据数量
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="dbs"></param>
        ///// <param name="sql"></param>
        ///// <returns></returns>
        public static int Count<T> (this DbContext dbs, QueryBuilder sql = null) where T : class {
            var db = dbs.DbConnection;
            var _dbtransaction = dbs.DbTransaction;

            if (sql == null)
            {
                sql = QueryBuilder<T>.Builder(dbs);
            }

            var cr = db.Query(sql.RecordCount, sql.Param, _dbtransaction).SingleOrDefault();
            return (int) cr.RecordCount;
        }
    }
}
