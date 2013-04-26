using System;
using System.Configuration;

namespace Common.Data
{
    public class AppConfig
    {
        private static string _CDataLeft = null;
        public static string CDataLeft
        {
            get
            {
                if(_CDataLeft==null)
                {
                    _CDataLeft= GetApp("CDataLeft");
                    if(_CDataLeft==null)
                    {
                        _CDataLeft = "<![CDATA[MMS::";
                    }
                }
                return _CDataLeft;
            }
        }
        private static string _CDataRight = null;
        public static string CDataRight
        {
            get
            {
                if (_CDataRight == null)
                {
                    _CDataRight = GetApp("CDataRight");
                    if (_CDataRight == null)
                    {
                        _CDataRight = "::MMS]]>";
                    }
                }
                return _CDataRight;
            }
        }

        public static string GetApp(string key)
        {
            return Convert.ToString(ConfigurationManager.AppSettings[key]);
        }
        public static string GetConn(string key)
        {
            ConnectionStringSettings logConn = ConfigurationManager.ConnectionStrings[key];
            if (logConn != null)
            {
                return logConn.ConnectionString;
            }
            return "";
        }
        /// <summary>
        /// Aop程序集[程序集名称,类名]
        /// </summary>
        public static string Aop
        {
            get
            {
                return GetApp("Aop");
            }
        }

        public static bool IsWriteLog
        {
            get
            {
                bool _IsWriteLog;
                bool.TryParse(GetApp("IsWriteLog"), out _IsWriteLog);
                return _IsWriteLog;
            }
        }
        public static string LogConn
        {
            get
            {
               return GetConn("LogConn");
            }
        }
      
        /// <summary>
        /// Oracle 序列ID
        /// </summary>
        public static string AutoID
        {
            get
            {
                return GetApp("AutoID");
            }
        }
        public static string DtdUri
        {
            get
            {
                string dtdUri = GetApp("DtdUri");
                if (dtdUri != null && dtdUri.IndexOf("http://") == -1)//相对路径
                {
                    dtdUri = AppDomain.CurrentDomain.BaseDirectory + dtdUri.TrimStart('/');
                }
                return dtdUri;
            }
        }
        /// <summary>
        /// 多国语言时网站主域名[不带www]
        /// </summary>
        public static string Domain
        {
            get
            {
                return  GetApp("Domain");
            }
        }
        /// <summary>
        /// 缓存时间[分钟]
        /// </summary>
        public static int CacheTime
        {
            get
            {
                int cacheTime = 0;
                int.TryParse(GetApp("CacheTime"), out cacheTime);
                return cacheTime;
            }
        }
        public static bool UseFileLoadXml
        {
            get
            {
                bool _UseFileLoadXml;
                bool.TryParse(GetApp("UseFileLoadXml"), out _UseFileLoadXml);
                return _UseFileLoadXml;
            }
        }
        /// <summary>
        /// 写日志出现异常时，记录到文本文件路径
        /// </summary>
        public static string LogPath
        {
            get
            {
                return GetApp("LogPath");
            }
        }
        /// <summary>
        /// 系统默认语言Key
        /// </summary>
        public static string SysLangKey
        {
            get
            {
                return GetApp("SysLangKey");
            }
        }
    }
}
