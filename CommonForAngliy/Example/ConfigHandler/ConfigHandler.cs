using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Common;

namespace Example
{

    public static class TestConfig
    {
        public static string UserID { get; set; }
        public static string PassWord { get; set; }
        public static string FactoryObject { get; set; }
        public static string CacheType { get; set; }
        public static string CachePrefix { get; set; }
        public static string CacheDefaultTimeToLive{ get; set; }

        public static string OrmConnection { get; set; }



    }


    public class TestConfigHandler : Common.ConfigHandler.ConfigHandler
    {
        public override object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            TestConfig.UserID = GetConfigValue(section, "//UserID");
            TestConfig.PassWord = GetConfigValue(section, "//PassWord");
            TestConfig.FactoryObject = GetConfigValue(section, "FactoryObject");
            TestConfig.CachePrefix = GetConfigValue(section, "//CachePrefix");
            TestConfig.CacheDefaultTimeToLive = GetConfigValue(section, "//CacheDefaultTimeToLive");
            TestConfig.CacheType = GetConfigValue(section, "//CacheType");
            TestConfig.OrmConnection = GetConfigValue(section, "//OrmConnection");
            return section;
        }
    }

   
}
