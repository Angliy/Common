using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Container;

namespace Common.Factory
{
    /// <summary>
    /// 对象工厂抽象类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FactoryBase<T>
    {
        /// <summary>
        /// 对象仓库
        /// </summary>
        public static IObjectContainer<T> Sotre = new CommonObjectContainer<T>();

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns>对象全名（程序集;完整类名）</returns>
        protected static T Create(string fullName, params object[] paras)
        {
            string Dll = "";
            string ClassName;
            string[] Type = fullName.Split(',');
            if (Type[0] != "")
            {
                //绝对路径
                //Dll = Type[0].Replace(".dll", "");

                //发布后的相对路径
                Dll = AppDomain.CurrentDomain.BaseDirectory + Type[0].Replace(".dll", "");
            }
            ClassName = Type[1];//命名空间+类名
            T Obj;
            if (String.IsNullOrEmpty(Dll))
            {
                Type supType = System.Type.GetType(ClassName);
                Obj = (T)Activator.CreateInstance(supType, paras);
            }
            else
            {
                System.Reflection.Assembly Ass = System.Reflection.Assembly.LoadFile(Dll + ".dll");
                Obj = (T)Ass.CreateInstance(ClassName, true, System.Reflection.BindingFlags.Default, null, paras, null, null);
            }
            Sotre.AddObject(ClassName, Obj);
            return Obj;
        }



        /// <summary>
        /// 获取类型
        /// </summary>
        /// <returns>类全名（程序集;完整类名）</returns>
        public static Type GetAssType(string fullName, params object[] paras)
        {
            string Dll = "";
            string ClassName;
            string[] Type = fullName.Split(',');
            if (Type[0] != "")
            {
                //绝对路径
                //Dll = Type[0].Replace(".dll", "");

                //发布后的相对路径
                Dll = AppDomain.CurrentDomain.BaseDirectory + Type[0].Replace(".dll", "");
            }
            ClassName = Type[1];//命名空间+类名
            Type supType;
            if (String.IsNullOrEmpty(Dll))
            {
                supType = System.Type.GetType(ClassName);
               
            }
            else
            {
                System.Reflection.Assembly Ass = System.Reflection.Assembly.LoadFile(Dll + ".dll");
                supType = Ass.GetType(ClassName);
               
            }
            return supType;
        }



        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="name">对象名</param>
        /// <returns>对象全名（程序集;完整类名）</returns>
        public static T Get(string fullName, params object[] paras)
        {
            string typename = string.Empty;
            if (!string.IsNullOrEmpty(fullName))
            {
                string[] typeinfo = fullName.Split(',');
                typename = typeinfo.Length == 2 ? typeinfo[1] : typeinfo[0];
                if (!Sotre.HasObject(typename))
                {
                    Create(fullName, paras);
                }
            }
            return Sotre.GetObject(typename);
        }
    }
}
