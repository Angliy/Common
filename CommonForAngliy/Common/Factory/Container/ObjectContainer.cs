using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Container
{
    /// <summary>
    /// 对象仓库管理
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class ObjectContainer<T> : IObjectContainer<T>
    {
        /// <summary>
        /// 对象仓库
        /// </summary>
        public Dictionary<string, T> Objects = new Dictionary<string, T>();

        /// <summary>
        /// 获取对象仓库
        /// </summary>
        /// <returns>对象仓库</returns>
        public Dictionary<string, T> GetStore()
        {
            return Objects;
        }

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        public void AddObject(string key, T obj)
        {
            Objects.Add(key, obj);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="key">键</param>
        public void DropObject(string key)
        {
            Objects.Remove(key);
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public T GetObject(string key)
        {
            return Objects[key];
        }

        /// <summary>
        /// 判断对象是否存在
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public bool HasObject(string key)
        {
            return Objects.ContainsKey(key);
        }
    }
}
