using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using Common.Data.DAL;

namespace Common.Data.DAL
{
    internal class Log
    {
        public static void WriteLog(string message)
        {
            if (AppConfig.IsWriteLog)
            {
                InsertLogToData(message);
            }
            else
            {
                //throw new Exception("Error on DataOperator:" +message);
            }
        }
        private static void InsertLogToData(string message)
        {
            string conn = AppConfig.LogConn;
            if (string.IsNullOrEmpty(conn))
            {
                WriteLogText(message);
                return ;
            }
            string pageUrl = System.Web.HttpContext.Current.Request.Url.ToString();
            DbBase helper = DalAction.GetHelper(conn);
            helper.WriteLog = false;
            try
            {
                helper.AddParameters("@PageUrl", pageUrl);
                helper.AddParameters("@ErrorMessage", message);
                if (helper.dalType == DalType.Oracle)
                {
                    helper.ExeNonQuery(string.Format("insert into ErrorLogs(ID,PageUrl,ErrorMessage) values({0}.nextval,@PageUrl,@ErrorMessage))",SQL.SQLString.AutoID), false);
                }
                else
                {
                    helper.ExeNonQuery("insert into ErrorLogs(PageUrl,ErrorMessage) values(@PageUrl,@ErrorMessage)", false);
                }
                
            }
            catch
            {
                WriteLogText(message);
            }
            finally
            {
                helper.Dispose();
            }
        }
        private static void WriteLogText(string message)
        {
            if (!string.IsNullOrEmpty(AppConfig.LogPath))
            {
                
                try
                {
                    string folder = AppDomain.CurrentDomain.BaseDirectory + AppConfig.LogPath;
                    if (!System.IO.Directory.Exists(folder))
                    {
                        System.IO.Directory.CreateDirectory(folder);
                    }
                    string todayKey = DateTime.Today.ToString("yyyyMMdd") + ".txt";
                    string filePath = folder + todayKey;
                    string pageUrl = System.Web.HttpContext.Current.Request.Url.ToString();
                    System.IO.File.AppendAllText(filePath, "\r\n------------------------\r\nlog:" + pageUrl + "\r\n" + message);
                }
                catch
                {
                    throw new Exception("Error on WriteLog :" + message);
                }
            }
        }
    }
}
