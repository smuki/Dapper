﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using System.Data.Common;

using Volte.Data.JsonObject;

namespace Volte.Data.Dapper
{
    public abstract class QueryBuilder {
        const string ZFILE_NAME       = "QueryBuilder";
        private static object objLock = new object();
        protected int _TopNum;
        protected List<QueryOrder> _Orders;
        protected StringBuilder _Sql;
        protected IList<DynamicPropertyModel> _Param;
        protected string ParamPrefix = "@";
        protected string _selectMode = "";
        protected string _FromClause = "";
        protected string _RestrictClause = "";
        protected string _CountClause = "";
        protected int _PageIndex;
        protected List<AttributeMapping> _Fields = new List<AttributeMapping>();
        protected int _PageSize;
        protected int _OFFSET = -1;
        protected bool _Distinct = false;
        protected DBType _DbType;

        protected static Dictionary<string, Type> DynamicParamModelCache = new Dictionary<string, Type>();
        protected Dictionary<string, object> ParamValues = new Dictionary<string, object>();
        protected Dictionary<string, int> _ParamIndex    = new Dictionary<string, int>();
        internal ClassMapping _ClassMapping;

        public virtual int OFFSET { get { return _OFFSET; } }
        public virtual int TopNum { get { return _TopNum; } }

        public virtual List<AttributeMapping> Fields
        {
            get {
                return _Fields;
            } set {
                _Fields = value;
            }
        }

        public virtual string SelectMode { get { return _selectMode; } set { _selectMode = value; } }
        public virtual string FromClause { get { return _FromClause; } set { _FromClause = value; } }

        public virtual IList<QueryOrder> Orders
        {
            get {
                return _Orders;
            }
        }

        public virtual string WhereClauseSql
        {
            get {
                var sb = new StringBuilder();
                var arr = _Sql.ToString().Split(' ').Where(m => !string.IsNullOrEmpty(m)).ToList();

                if (_Param != null && _Param.Count > 0) {
                    foreach (AttributeMapping p in DapperUtil.GetPrimary(_ClassMapping)) {
                        if (sb.Length == 0) {
                            sb.Append(string.Format(" {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                        } else {
                            sb.Append(string.Format(" AND {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                        }
                    }
                }

                for (int i = 0; i < arr.Count; i++) {
                    if (i == 0 && (arr[i] == "AND" || arr[i] == "OR")) {
                        continue;
                    }

                    if (i > 0 && arr[i - 1] == "(" && (arr[i] == "AND" || arr[i] == "OR")) {
                        continue;
                    }

                    sb.Append(" ");
                    sb.Append(arr[i]);
                }

                if (sb.Length > 0) {
                    sb.Insert(0, " WHERE ");
                }

                return sb.ToString();
            }
        }

        public virtual string WhereSql
        {
            get {
                var sb = new StringBuilder();
                var arr = _Sql.ToString().Split(' ').Where(m => !string.IsNullOrEmpty(m)).ToList();

                if (arr.Count > 0) {
                    sb.Append("WHERE");
                }

                for (int i = 0; i < arr.Count; i++) {
                    if (i == 0 && (arr[i] == "AND" || arr[i] == "OR")) {
                        continue;
                    }

                    if (i > 0 && arr[i - 1] == "(" && (arr[i] == "AND" || arr[i] == "OR")) {
                        continue;
                    }

                    sb.Append(" ");
                    sb.Append(arr[i]);
                }

                return sb.ToString();
            }
        }

        public virtual string OrderSql
        {
            get {
                var sb = new StringBuilder();

                int i = 0;

                foreach (QueryOrder item in this.Orders) {
                    ZZLogger.Debug(ZFILE_NAME, "X" + item.Field);

                    if (i == 0) {
                        sb.Append(" ");
                        sb.Append("ORDER BY");
                        sb.Append(" ");
                    } else {
                        sb.Append(",");
                    }

                    sb.Append(" ");
                    sb.Append(item.Field);
                    sb.Append(" ");
                    sb.Append(item.Asc);

                    i++;
                }

                return sb.ToString();
            }
        }

        public object Param
        {
            get {
                if (_Param != null && _Param.Count > 0) {
                    var paramKeys = this.ParamValues.Keys.ToList();
                    var listCacheKeys = new List<string>();
                    listCacheKeys.Add(_ClassMapping.TableName);
                    listCacheKeys.AddRange(paramKeys);

                    var cacheKey = string.Empty;

                    foreach (var key in DynamicParamModelCache.Keys.Where(m => m.StartsWith(_ClassMapping.TableName))) {
                        if (listCacheKeys.All(m => key.Split('_').Contains(m))) {
                            cacheKey = key;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(cacheKey)) {
                        cacheKey = string.Join("_", listCacheKeys);
                    }

                    Type modelType;

                    lock (objLock) {
                        DynamicParamModelCache.TryGetValue(cacheKey, out modelType);

                        if (modelType == null) {
                            var tyName = "CustomDynamicParamClass";
                            modelType = DynamicBuilder.DynamicCreateType(tyName, _Param);
                            DynamicParamModelCache.Add(cacheKey, modelType);
                        }
                    }

                    var model = Activator.CreateInstance(modelType);

                    foreach (var item in this.ParamValues) {
                        modelType.GetProperty(item.Key).SetValue(model, item.Value, null);
                    }

                    return model;
                } else {
                    return null;
                }
            }
        }

        public virtual string InsertSql
        {
            get {
                StringBuilder sql = new StringBuilder();
                sql.Append(string.Format("INSERT INTO {0}(", _ClassMapping.TableName));
                var colums = DapperUtil.GetExecColumns(_ClassMapping);

                for (int i = 0; i < colums.Count; i++) {
                    if (i == 0) sql.Append(colums[i].ColumnName);
                    else sql.Append(string.Format(",{0}", colums[i].ColumnName));
                }

                sql.Append(")");
                sql.Append(" VALUES(");

                for (int i = 0; i < colums.Count; i++) {
                    if (i == 0) sql.Append(string.Format("{0}{1}", ParamPrefix, colums[i].ColumnName));
                    else sql.Append(string.Format(",{0}{1}", ParamPrefix, colums[i].ColumnName));
                }

                sql.Append(") ");
                //Console.WriteLine(sql.ToString());
                return sql.ToString();
            }
        }

        public virtual string DeleteSql
        {
            get {
                StringBuilder sql = new StringBuilder();

                sql.Append(string.Format("DELETE FROM {0} ", _ClassMapping.TableName));

                //ZZLogger.Debug(ZFILE_NAME, "[" + this.WhereSql + "]");

                if (string.IsNullOrEmpty(this.WhereSql)) {
                    //ZZLogger.Debug(ZFILE_NAME, "XX2" + sql.ToString());
                    int i = 0;

                    foreach (AttributeMapping p in DapperUtil.GetPrimary(_ClassMapping)) {
                        if (i == 0) {
                            sql.Append(string.Format(" WHERE {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                        } else {
                            sql.Append(string.Format(" AND {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                        }

                        i++;
                        //ZZLogger.Debug(ZFILE_NAME, "XX" + i + sql.ToString());
                    }
                } else {
                    sql.Append(string.Format(" {0}", this.WhereSql));
                }

                //ZZLogger.Debug(ZFILE_NAME, sql.ToString());
                return sql.ToString();

            }
        }

        public virtual string UpdateSql
        {
            get {
                StringBuilder sql = new StringBuilder();
                sql.Append(string.Format("UPDATE {0} SET", _ClassMapping.TableName));
                var colums = DapperUtil.GetExecColumns(_ClassMapping, false);

                for (int i = 0; i < colums.Count; i++) {
                    if (i != 0) {
                        sql.Append(",");
                    }

                    sql.Append(" ");
                    sql.Append(colums[i].ColumnName);
                    sql.Append(" ");
                    sql.Append("=");
                    sql.Append(" ");
                    sql.Append(ParamPrefix + colums[i].ColumnName);
                }

                if (string.IsNullOrEmpty(WhereSql)) {
                    int i = 0;

                    foreach (AttributeMapping p in DapperUtil.GetPrimary(_ClassMapping)) {
                        if (i == 0) {
                            sql.Append(string.Format(" WHERE {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                        } else {
                            sql.Append(string.Format(" AND {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                        }

                        i++;
                    }
                } else {
                    sql.Append(string.Format(" {0}", WhereSql));
                }

                return sql.ToString();
            }
        }

        public virtual string QuerySql
        {
            get {
                StringBuilder _Where = new StringBuilder();
                string _TableName    = _ClassMapping.TableName;
                var sqlStr           = "";

                if (string.IsNullOrEmpty(WhereSql)) {
                    if (_Param != null && _Param.Count > 0) {

                        int i = 0;

                        foreach (AttributeMapping p in DapperUtil.GetPrimary(_ClassMapping)) {
                            if (i == 0) {
                                _Where.Append(string.Format(" WHERE {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                            } else {
                                _Where.Append(string.Format(" AND {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                            }

                            i++;
                        }
                    }
                } else {
                    _Where.Append(string.Format(" {0}", WhereSql));
                }

                StringBuilder _select = new StringBuilder();

                if (_Fields.Count > 0) {
                    int i = 0;

                    foreach (AttributeMapping _att in _Fields) {
                        if (i != 0) {
                            _select.Append(",");
                        }

                        if (_att["Expression"] != "" || _att.TableName == "VARIABLE") {

                            if (_att["Expression"] != "") {
                                _select.Append(_att["Expression"]);
                            } else {
                                _select.Append("''");
                            }

                            _select.Append(" AS ");

                            if (_att.AliasName != "") {
                                _select.Append(_att.AliasName);
                            } else {
                                _select.Append(_att.ColumnName);
                            }
                        } else {
                            _select.Append(_att.TableName + "." + _att.ColumnName);

                            if (_att.AliasName != "") {
                                _select.Append(" AS ");
                                _select.Append(_att.AliasName);
                            }
                        }

                        i++;
                    }

                } else {
                    for (int i = 0; i < _ClassMapping.AttributeMappings.Count; i++) {
                        if (i != 0) {
                            _select.Append(",");
                        }

                        _select.Append(_ClassMapping.AttributeMappings[i].ColumnName);
                    }
                }

                if (string.IsNullOrEmpty(_select.ToString())) {
                    _select.Append("*");
                }

                //ZZLogger.Debug(ZFILE_NAME, _FromClause);

                if (_FromClause != "") {
                    _TableName = _FromClause;
                }

                if (_TopNum > 0) {
                    if (_DbType == DBType.SqlServer || _DbType == DBType.SqlServerCE)
                        if (_OFFSET >= 0) {
                            if (_DbType == DBType.SqlServer) {
                                var strOrderSql = this.OrderSql ;
                                sqlStr = string.Format("SELECT {0} FROM (SELECT {0},ROW_NUMBER() OVER({1}) AS ROW_NUMBER FROM {4} {3} ) T WHERE ROW_NUMBER={2} {1}", _select.ToString(), strOrderSql, _OFFSET, _Where.ToString(), _TableName);


                                sqlStr = " SELECT * FROM (SELECT " + _select.ToString() + ",ROW_NUMBER() OVER(" + strOrderSql + ") AS ROW_NUMBER  FROM " + _TableName + " " + _Where.ToString() + ") AS D  WHERE ROW_NUMBER BETWEEN " + (_OFFSET + 1) + " AND " + (_OFFSET + _TopNum);

                            } else {

                                sqlStr = string.Format("SELECT TOP {0} {1} FROM {2} {3} {4}", _TopNum, _select.ToString(), _TableName, _Where.ToString(), this.OrderSql);
                            }
                        } else {

                            sqlStr = string.Format("SELECT TOP {0} {1} FROM {2} {3} {4}", _TopNum, _select.ToString(), _TableName, _Where.ToString(), this.OrderSql);
                        }
                    else if (_DbType == DBType.Oracle) {
                        var strWhere = "";

                        if (string.IsNullOrEmpty(_Where.ToString())) {
                            strWhere = string.Format(" WHERE  ROWNUM <= {0} ", _TopNum);
                        } else {
                            strWhere = string.Format(" {0} AND ROWNUM <= {1} ", _Where.ToString(), _TopNum);
                        }

                        sqlStr = string.Format("SELECT * FROM {2} {3} {4}", _TableName, strWhere, this.OrderSql);
                    } else {
                        sqlStr = string.Format("SELECT {0} FROM {1} {2} {3} LIMIT {4}", _select.ToString(), _TableName, _Where.ToString(), this.OrderSql, _TopNum);
                    }
                } else {

                    sqlStr = string.Format("SELECT {0} FROM {1} {2} {3}",  _select.ToString(), _TableName, _Where.ToString(), this.OrderSql);
                }

                return sqlStr;
            }
        }

        public virtual string PageSql
        {
            get {
                var sqlPage = "";
                var orderStr = string.IsNullOrEmpty(this.OrderSql) ? "ORDER BY " + _ClassMapping.AttributeMappings.FirstOrDefault().ColumnName : this.OrderSql;

                if (_DbType == DBType.SqlServer || _DbType == DBType.Oracle) {
                    var tP = _DbType == DBType.Oracle ? _ClassMapping.TableName + ".*" : "*";
                    sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) rid, {1} {2}) p_paged WHERE rid>{3} AND rid<={4}", orderStr, tP, this.WhereSql, ((_PageIndex - 1) * _PageSize), ((_PageIndex - 1) * _PageSize + _PageSize));
                } else if (_DbType == DBType.SqlServerCE) {
                    sqlPage = string.Format("SELECT * FROM {0} {1} {2} OFFSET {3} ROWS FETCH NEXT {4} ROWS ONLY", _ClassMapping.TableName, this.WhereSql, orderStr, (_PageIndex - 1) * _PageSize, (_PageIndex - 1) * _PageSize + _PageSize);
                } else {
                    sqlPage = string.Format("SELECT * FROM {0} {1} {2} LIMIT {1} OFFSET {2}", _ClassMapping.TableName, this.WhereSql, orderStr, (_PageIndex - 1) * _PageSize, (_PageIndex - 1) * _PageSize + _PageSize);
                }

                return sqlPage;
            }
        }

        public virtual string RecordCount
        {
            get {

                StringBuilder _Where = new StringBuilder();
                string _TableName    = _ClassMapping.TableName;

                if (_FromClause != "") {
                    _TableName = _FromClause;
                }

                if (string.IsNullOrEmpty(WhereSql)) {
                    if (_Param != null && _Param.Count > 0) {

                        int i = 0;

                        foreach (AttributeMapping p in DapperUtil.GetPrimary(_ClassMapping)) {
                            if (i == 0) {
                                _Where.Append(string.Format(" WHERE {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                            } else {
                                _Where.Append(string.Format(" AND {0}={1}", p.ColumnName, ParamPrefix + p.Name));
                            }

                            i++;
                        }
                    }
                } else {
                    _Where.Append(string.Format(" {0}", WhereSql));
                }

                return string.Format("SELECT COUNT(*) AS RecordCount FROM {0} {1}" , _TableName , _Where.ToString());
            }
        }

        protected QueryBuilder()
        {
            _Sql = new StringBuilder();
        }

        public QueryBuilder Top(int top)
        {
            _TopNum = top;
            _OFFSET = -1;
            return this;
        }

        public QueryBuilder Top(int top, int offset = -1)
        {
            _TopNum = top;
            _OFFSET = offset;
            return this;
        }

        public QueryBuilder OrWhere(string whereClause)
        {
            return Where(whereClause, false);
        }

        public QueryBuilder OrderClause(string field)
        {
            _Orders.Add(new QueryOrder() {
                Field = field, Asc = ""
            });
            return this;
        }

        public QueryBuilder OrderBy(string field, bool Asc = true)
        {

            var _order_Asc = "ASC";

            if (!Asc) {
                _order_Asc = "DESC";
            }

            _Orders.Add(new QueryOrder() {
                Field = field, Asc = _order_Asc
            });
            return this;
        }


        public QueryBuilder Where(string whereClause, bool isAnd = true)
        {
            if (whereClause.Trim() != "") {

                var cn = isAnd ? "AND" : "OR";
                _Sql.Append(" ");
                _Sql.Append(cn);
                _Sql.Append(" ");
                _Sql.Append(whereClause);
                _Sql.Append(" ");

            }

            return this;
        }

        public QueryBuilder LeftParen(bool isAnd = true)
        {
            var cn = isAnd ? "AND" : "OR";
            _Sql.Append(" ");
            _Sql.Append(cn);
            _Sql.Append(" ");
            _Sql.Append("(");
            return this;
        }

        public QueryBuilder RightParen()
        {
            _Sql.Append(" ");
            _Sql.Append(")");
            return this;
        }

        protected string GetOpStr(Operation method)
        {
            switch (method) {
            case Operation.Contains:
            case Operation.StartsWith:
            case Operation.EndsWith:
                return "LIKE";

            case Operation.Equal:
                return "=";

            case Operation.Greater:
                return ">";

            case Operation.GreaterOrEqual:
                return ">=";

            case Operation.Less:
                return "<";

            case Operation.LessOrEqual:
                return "<=";

            case Operation.NotEqual:
                return "<>";

            }

            return "=";
        }

        public QueryBuilder OrWhereClause(string field, Operation operation, string value)
        {
            return WhereClause(field, operation, value, false);
        }

        public QueryBuilder WhereClause(string field, Operation operation, string value, bool isAnd = true)
        {
            string _value = DapperUtil.AntiSQLInjection(value);

            ZZLogger.Debug(ZFILE_NAME, _value);

            if (operation == Operation.Contains) {
                _value = string.Format("%{0}%", _value);
            } else if (operation == Operation.StartsWith) {
                _value = string.Format("{0}%", _value);
            } else if (operation == Operation.EndsWith) {
                _value = string.Format("%{0}", _value);
            }

            return _WhereClause(field, operation, "N'" + _value + "'", isAnd);
        }

        public QueryBuilder WhereClause(string field, Operation operation, DateTime value, bool isAnd = true)
        {
            return _WhereClause(field, operation, "'" + value.ToString("yyyy-MM-dd") + "'", isAnd);
        }

        public QueryBuilder WhereClause(string field, Operation operation, decimal value, bool isAnd = true)
        {
            return _WhereClause(field, operation, value, isAnd);
        }

        public QueryBuilder WhereClause(string field, Operation operation, int value, bool isAnd = true)
        {
            return _WhereClause(field, operation, value, isAnd);
        }

        private QueryBuilder _WhereClause(string field, Operation operation, object value, bool isAnd = true)
        {
            var cn = isAnd ? "AND" : "OR";
            var op = GetOpStr(operation);
            StringBuilder sb = new StringBuilder();

            _Sql.Append(" ");
            _Sql.Append(cn);
            _Sql.Append(" ");
            _Sql.Append(field);
            _Sql.Append(" ");
            _Sql.Append(op);
            _Sql.Append(" ");
            _Sql.Append(value.ToString());

            ZZLogger.Debug(ZFILE_NAME, _Sql);
            return this;
        }


        public QueryBuilder OrWhere(string field, Operation operation, object value)
        {
            return Where(field, operation, value, false);
        }

        public QueryBuilder Where(string field, Operation operation, object value, bool isAnd = true)
        {
            var cn = isAnd ? "AND" : "OR";
            var op = GetOpStr(operation);
            StringBuilder sb = new StringBuilder();
            _Sql.Append(" ");
            _Sql.Append(cn);
            _Sql.Append(" ");
            _Sql.Append(field);
            _Sql.Append(" ");
            _Sql.Append(op);
            _Sql.Append(" ");
            var model = AddParam(operation, field, value);
            _Sql.Append(this.ParamPrefix + model.Name);

            return this;
        }

        private object CreateParam(Operation method, object value)
        {
            switch (method) {
            case Operation.Contains:
                return string.Format("%{0}%", value);

            case Operation.StartsWith:
                return string.Format("{0}%", value);

            case Operation.EndsWith:
                return string.Format("%{0}", value);

            case Operation.Equal:
                return value;

            case Operation.Greater:
                return value;

            case Operation.GreaterOrEqual:
                return value;

            case Operation.Less:
                return value;

            case Operation.LessOrEqual:
                return value;

            case Operation.NotEqual:
                return value;

            }

            return value;
        }

        protected DynamicPropertyModel AddParam(Operation method, string field, object value)
        {
            if (_Param == null) {
                _Param = new List<DynamicPropertyModel>();
            }

            var model = new DynamicPropertyModel();
            model.Name = field + GetParamIndex(field);
            model.PropertyType = value.GetType();
            _Param.Add(model);

            //Console.WriteLine(model.Name+"="+value);

            switch (method) {
            case Operation.Contains:
                this.ParamValues.Add(model.Name, string.Format("%{0}%", value));
                break;

            case Operation.StartsWith:
                this.ParamValues.Add(model.Name, string.Format("{0}%", value));
                break;

            case Operation.EndsWith:
                this.ParamValues.Add(model.Name, string.Format("%{0}", value));
                break;

            case Operation.Equal:
                this.ParamValues.Add(model.Name, value);
                break;

            case Operation.Greater:
                this.ParamValues.Add(model.Name, value);
                break;

            case Operation.GreaterOrEqual:
                this.ParamValues.Add(model.Name, value);
                break;

            case Operation.Less:
                this.ParamValues.Add(model.Name, value);
                break;

            case Operation.LessOrEqual:
                this.ParamValues.Add(model.Name, value);
                break;

            case Operation.NotEqual:
                this.ParamValues.Add(model.Name, value);
                break;

            }

            return model;
        }

        private string GetParamIndex(string field)
        {
            int _ndx = 1;

            if (_ParamIndex.ContainsKey(field)) {
                _ndx = _ParamIndex[field] + 1;
                _ParamIndex[field] = _ndx;
            } else {
                _ParamIndex[field] = _ndx;
            }

            return _ndx.ToString();
        }

        internal QueryBuilder AppendParam<T> (T t) where T : class {
            if (_Param == null)
            {
                _Param = new List<DynamicPropertyModel>();
            }

            var model = DapperUtil.getClassMapping<T>();

            foreach (var item in model.AttributeMappings)
            {
                var value = model.ClassType.GetProperty(item.Name).GetValue(t, null);
                var pmodel = new DynamicPropertyModel();
                pmodel.Name = item.Name;

                //ZZLogger.Debug(ZFILE_NAME, item.Nullable);
                //ZZLogger.Debug(ZFILE_NAME, item.Type);

                if (item.Nullable && item.Type == DbType.DateTime) {
                    pmodel.PropertyType = typeof(DateTime?);
                } else {
                    //ZZLogger.Debug(ZFILE_NAME, item.Name);
                    pmodel.PropertyType = value.GetType();
                }

                this.ParamValues.Add(item.Name, value);
                _Param.Add(pmodel);
            }

            return this;
        }

        public QueryBuilder Page(int pindex, int pageSize)
        {
            _PageIndex = pindex;
            _PageSize = pageSize;
            return this;
        }
    }

    public class QueryBuilder<T> : QueryBuilder where T : class {
        private QueryBuilder() : base()
        {
            _ClassMapping = DapperUtil.getClassMapping<T>();
            _Orders = new List<QueryOrder>();

        }

        public static QueryBuilder<T> Builder(DbContext db)
        {
            var result = new QueryBuilder<T>();

            result.ParamPrefix = db.ParamPrefix;
            result._DbType = db.DbType;

            return result;
        }

//        public QueryBuilder<T> OrderBy(Expression<Func<T, object>> expr)
//        {
//            return this.OrderBy(expr, true);
//        }
//        public QueryBuilder<T> OrderBy(Expression<Func<T, object>> expr, bool Asc)
//        {
//            var field = DapperUtil.GetPropertyByExpress<T> (_ClassMapping, expr).ColumnName;
//            var _order_Asc = "ASC";
//
//            if (!Asc) {
//                _order_Asc = "DESC";
//            }
//
//            _Orders.Add(new QueryOrder() {
//                    Field = field, Asc = _order_Asc
//                    });
//
//            return this;
//        }

        public QueryBuilder<T> OrderBy(string field, bool Asc = true)
        {

            var _order_Asc = "ASC";

            if (!Asc) {
                _order_Asc = "DESC";
            }

            _Orders.Add(new QueryOrder() {
                Field = field, Asc = _order_Asc
            });
            return this;
        }

        public QueryBuilder<T> OrderClause(string field)
        {
            _Orders.Add(new QueryOrder() {
                Field = field, Asc = ""
            });
            return this;
        }
    }
}