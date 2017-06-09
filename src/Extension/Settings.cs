using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

using Volte.Data.Json;

namespace Volte.Data.Dapper
{
    public class Settings {
        const string ZFILE_NAME = "Settings";
        private static Settings _instance;
        private static Dictionary<string, Setting> _Settings = new Dictionary<string, Setting>();

        // Fields
        static Settings()
        {
            Settings._instance = null;
        }

        public static Settings Instance()
        {
            if (Settings._instance == null) {
                Settings._instance = new Settings();
            }

            return Settings._instance;
        }

        public void RegisterDbConnectionInfo(string name, string DbType, string connectionString)
        {
            this.RegisterDbConnectionInfo(name, DbType, connectionString, "");
        }

        public void RegisterDbConnectionInfo(string name, string DbType, string connectionString, string cClassMappingFile)
        {
            string text1 = this.ToString().Substring(0, this.ToString().LastIndexOf('.') + 1);
            ObjectBroker.Instance().RegisterDbConnectionInfo(name, text1 + DbType, connectionString, cClassMappingFile);
        }

        public string UnicodePrefix(string name)
        {
            Streaming database1 = ObjectBroker.Instance().getStreaming(name);

            if (database1 == null) {
                return "";
            }

            return database1.UnicodePrefix;
        }

        public void Clear()
        {
            ObjectBroker.Instance().Clear();
        }

        public bool ContainsKey(string DbName)
        {
            return _Settings.ContainsKey(DbName);
        }

        public Setting GetValue(string DbName)
        {
            if (_Settings.ContainsKey(DbName)) {
                return _Settings[DbName];
            }

            return new Setting();
        }

        public Setting GetValue(string DbName, string providerName, string connStr)
        {

            string text1 = this.ToString().Substring(0, this.ToString().LastIndexOf('.') + 1);

            //Console.WriteLine(text1);
            //if (_Settings.ContainsKey(DbName)) {
            //return _Settings[DbName];
            //} else {

            Setting _Setting          = new Setting();
            _Setting.ConnectionString = connStr;
            _Setting.ProviderName     = providerName;
            _Setting.TypeName         = text1 + providerName;
            _Setting.DbName           = DbName;

            // 使用类型名判断
            //
            if (providerName.StartsWith("MsSqlServer")) {
                _Setting.DbType  = DBType.SqlServer;
                _Setting.Include = true;
            } else if (providerName.StartsWith("MySql")) {
                _Setting.DbType = DBType.MySql;
            } else if (providerName.StartsWith("Vertica")) {
                _Setting.DbType = DBType.Vertica;
            } else if (providerName.StartsWith("SqlCe")) {
                _Setting.DbType = DBType.SqlServerCE;
            } else if (providerName.StartsWith("Npgsql")) {
                _Setting.DbType = DBType.PostgreSQL;
            } else if (providerName.StartsWith("Oracle")) {
                _Setting.DbType = DBType.Oracle;
            } else if (providerName.StartsWith("SQLite")) {
                _Setting.DbType = DBType.SQLite;
            } else if (providerName.StartsWith("System.Data.SqlClient.")) {
                _Setting.DbType = DBType.SqlServer;
            }
            // else try with provider name
            else if (providerName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                _Setting.DbType = DBType.MySql;
            }else if (providerName.IndexOf("Vertica", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                _Setting.DbType = DBType.Vertica;
            } else if (providerName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                _Setting.DbType = DBType.SqlServerCE;
            } else if (providerName.IndexOf("Npgsql", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                _Setting.DbType = DBType.PostgreSQL;
            } else if (providerName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                _Setting.DbType = DBType.Oracle;
            } else if (providerName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                _Setting.DbType = DBType.SQLite;
            }

            if (_Setting.DbType  == DBType.MySql && _Setting.ConnectionString != null && _Setting.ConnectionString.IndexOf("Allow User Variables=true") >= 0) {
                _Setting.ParamPrefix = "?";
            }

            if (_Setting.DbType == DBType.Oracle) {
                _Setting.ParamPrefix = ":";
            }

            //Console.WriteLine(text1);

            ZZLogger.Debug(ZFILE_NAME, text1);

            ObjectBroker.Instance().RegisterDbConnectionInfo(_Setting);

            _Settings[DbName] = _Setting;

            return _Setting;
            //}
        }

    }
}
