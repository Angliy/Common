using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.OracleClient;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace Common.Helper
{
    public static class SqlHelper
    {
        public static string ConnectionString = "";

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="ConnectionString">数据库连接</param>
        /// <param name="strSql">执行的SQL语句</param>
        /// <returns></returns>
        public static void ExecuteSql(string strSql)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            OracleCommand dbCommand = new OracleCommand();
            dbCommand.Connection = conn;
            dbCommand.CommandText = strSql;
            dbCommand.CommandType = CommandType.Text;
            dbCommand.ExecuteNonQuery();

            conn.Close();
        }

        /// <summary>
        /// 执行select count查询，返回行数
        /// </summary>
        /// <param name="ConnectionString">数据库连接</param>
        /// <param name="strSql">执行的SQL语句</param>
        /// <returns></returns>
        public static int SelectCountSql(string strSql)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            OracleCommand dbCommand = new OracleCommand();
            dbCommand.Connection = conn;
            dbCommand.CommandText = strSql;
            dbCommand.CommandType = CommandType.Text;
            object obj = dbCommand.ExecuteScalar();
            conn.Close();
            int count = 0;
            int.TryParse(obj.ToString(), out count);
            return count;
        }

        /// <summary>
        /// 执行查询，返回第一行第一列的值
        /// </summary>
        /// <param name="ConnectionString">数据库连接</param>
        /// <param name="strSql">执行的SQL语句</param>
        /// <returns></returns>
        public static object SelectSql(string strSql)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            OracleCommand dbCommand = new OracleCommand();
            dbCommand.Connection = conn;
            dbCommand.CommandText = strSql;
            dbCommand.CommandType = CommandType.Text;
            object obj = dbCommand.ExecuteScalar();
            conn.Close();
            return obj;
        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static bool ExecuteProduce(string name, IList<DbParameter> paramList)
        {

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            OracleCommand dbCommand = new OracleCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = name;

            try
            {
                foreach (DbParameter param in paramList)
                {
                    dbCommand.Parameters.Add(param);
                }
                dbCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                conn.Close();
                return false;
            }
            finally 
            {
                conn.Close();
            }
           
        }


        /// <summary>
        /// 创建输入参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DbParameter BuildInParam(string name, OracleType dbType, object val)
        {
            DbParameter param = new OracleParameter(name, dbType);
            param.Value = val;
            return param;
        }

        /// <summary>
        /// 创建输出参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static DbParameter BuildOutParam(string name, OracleType dbType)
        {
            DbParameter param = new OracleParameter(name, dbType);
            param.Direction = ParameterDirection.Output;
            return param;
        }



    }
}