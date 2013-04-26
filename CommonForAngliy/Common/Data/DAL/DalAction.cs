using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Common.Data.DAL
{
    /// <summary>
    /// 数据库类型操作类
    /// </summary>
    internal class DalAction
    {
        public const string SqlClient = "System.Data.SqlClient";
        public const string OleDb = "System.Data.OleDb";
        public const string OracleClient = "System.Data.OracleClient";
        public static DbBase GetHelper(string conn)
        {
            string providerName = "";
            if (conn.Length < 32)
            {
                conn = conn.Length < 1 ? "Conn" : conn;
                if (ConfigurationManager.ConnectionStrings[conn] != null)
                {
                    providerName = ConfigurationManager.ConnectionStrings[conn].ProviderName;
                    conn = ConfigurationManager.ConnectionStrings[conn].ConnectionString;
                }
                else
                {
                    throw new Exception(string.Format("从配置文件webconfig中找不到 {0} 的链接字符串配置项!", conn));
                }
            }
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = DalAction.GetpPovider(conn);
            }
            switch (providerName)
            {
                case SqlClient:
                    return new SQLHelper(conn, providerName);
                case OleDb:
                    conn = string.Format(conn, AppDomain.CurrentDomain.BaseDirectory);
                    return new OleHelper(conn, providerName);
                case OracleClient:
                    return new OracleHelper(conn, providerName);
            }
            return new SQLHelper(conn, providerName);
        }

        public static string GetpPovider(string conn)
        {
            conn=conn.ToLower();
            if (conn.Contains("microsoft.jet.oledb.4.0"))
            {
                return OleDb;
            }
            else if (conn.Contains("provider=msdaora") || conn.Contains("provider=oraoledb.oracle"))
            {
                return OracleClient;
            }
            else
            {
                return SqlClient;
            }
        }
        public static DalType GetDalType(string providerName)
        {
            switch (providerName)
            {
                case SqlClient:
                    return DalType.Sql;
                case OleDb:
                    return DalType.Access;
                case OracleClient:
                    return DalType.Oracle;
            }
            return DalType.Sql;
        }
    }
    /// <summary>
    /// 操作数据库程序集类型
    /// </summary>
    public enum DalType
    {
        Sql,
        Access,
        Oracle,
        //MySql,
        //SQLite,
        //FireBird,
        //PostgreSQL
    }
    public class DalValue
    {
        /// <summary>
        /// 对于Bit类型[是/否] 类型的排序
        /// </summary>
        public const string Desc = "[#DESC]";
        public const string Asc = "[#ASC]";
        /// <summary>
        /// 对于Bit类型[是/否] 的条件值
        /// </summary>
        public const string True = "[#TRUE]";
        public const string False = "[#FALSE]";
        public const string Len = "[#LEN]";//length
        public const string Substring = "[#SUBSTRING]";//substr
        public const string GetDate = "[#GETDATE]";//length
        public const string Year = "[#YEAR]";//length
        public const string Month = "[#MONTH]";//length
        public const string Day = "[#DAY]";//length
        public const string CharIndex = "[#CHARINDEX]";
        public const string DateDiff = "[#DATEDIFF]";
        public const string Case = "[#CASE]";//单条件分支
        public const string CaseWhen = "[#CASE#WHEN]";//多条件分支
    }
}
