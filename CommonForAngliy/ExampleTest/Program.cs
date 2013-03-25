using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Example;
using Common.Caching;
 


namespace ExampleTest
{
    class Program
    {
        static void Main(string[] args)
        {

            //创建自定义配置节
            System.Configuration.ConfigurationManager.GetSection("TestConfigHandler");
            Console.WriteLine(TestConfig.UserID + " " + TestConfig.PassWord);
          

            //反射抽象工厂获取实例
            IFetchData fetchData = FetchFactory.Get(TestConfig.FactoryObject);
            fetchData.GetData();


            //初始化全局缓存
            ICache cache = (ICache)Activator.CreateInstance(Type.GetType(TestConfig.CacheType),
                        new CacheSettings()
                        {
                            PrefixForCacheKeys = TestConfig.CachePrefix,
                            DefaultTimeToLive = int.Parse(TestConfig.CacheDefaultTimeToLive),
                        });
            Cacher.Init(cache);
            Cacher.Insert("key","value");
            Console.WriteLine(Common.Caching.Cacher.Get("key").ToString());





            Console.Read();




        }
    }
}
