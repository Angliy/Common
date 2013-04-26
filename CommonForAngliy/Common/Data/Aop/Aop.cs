using System.Configuration;
using System;
using Common.Caching;

namespace Common.Data.Aop
{
    /// <summary>
    /// 内部预先实现空的Aop
    /// </summary>
    internal class Aop:IAop
    {
        #region IAop 成员

        public void Begin(AopEnum action, string objName, params object[] aopInfo)
        {
            
        }

        public void End(AopEnum action, bool success, object id, params object[] aopInfo)
        {
            
        }

        public void OnError(string msg)
        {
            
        }
        public IAop GetFromConfig()
        {
            string aopApp =AppConfig.Aop;
            if (aopApp!=null)
            {
                if (Cacher.Contains("Aop"))
                {
                    return Cacher.Get("Aop") as IAop;
                }
                string[] aopItem = aopApp.Split(',');
                if (aopItem.Length == 2)
                {
                    try
                    {
                        System.Reflection.Assembly ass = System.Reflection.Assembly.Load(aopItem[0]);
                        if (ass != null)
                        {
                            object instance = ass.CreateInstance(aopItem[1]);
                            if (instance != null)
                            {
                                Cacher.Insert("Aop", instance);
                                return instance as IAop;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        string errMsg = err.Message + "--标题Aop配置为[程序集名称,名称空间.类名]如:<add key=\"Aop\" value=\"Common.Data.Test,Common.Data.Test.MyAop\" />";
                        throw new Exception(errMsg);
                    }
                }
            }
            return null;
        }

        #endregion

       
    }
}
