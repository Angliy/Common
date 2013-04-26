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

            ////创建自定义配置节
            System.Configuration.ConfigurationManager.GetSection("TestConfigHandler");
            //Console.WriteLine(TestConfig.UserID + " " + TestConfig.PassWord);
          

            ////反射抽象工厂获取实例
            //IFetchData fetchData = FetchFactory.Get(TestConfig.FactoryObject);
            //fetchData.GetData();


            ////初始化全局缓存
            //ICache cache = (ICache)Activator.CreateInstance(Type.GetType(TestConfig.CacheType),
            //            new CacheSettings()
            //            {
            //                PrefixForCacheKeys = TestConfig.CachePrefix,
            //                DefaultTimeToLive = int.Parse(TestConfig.CacheDefaultTimeToLive),
            //            });
            //Cacher.Init(cache);
            //Cacher.Insert("key","value");
            //Console.WriteLine(Common.Caching.Cacher.Get("key").ToString());


            //ORM

            TestData.Init("rdc_option", "rdc_option_seq", TestConfig.OrmConnection);

            TestData1.Init("rdc_option_1", "rdc_option_seq", TestConfig.OrmConnection);

            //TestData data=new TestData();


            //TestData1 data1 = new TestData1() { Name="测试2,",Query_Id="test",Value="test"};


            //data1.Insert();

            //Console.WriteLine(data.Name+"   "+data1.Name);



            TestData data = new TestData();

            //data.Fill(39);
            int uu=0;

            Common.Data.Table.MDataTable tab = data.Select(1, 20, "", out uu);

            List<TestData> list = tab.ToList<TestData>();

            foreach(TestData t in list)
            {
                Console.WriteLine(t.Name);
            }
             

            Console.Read();




        }
    }
}
