using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Container
{
    /// <summary>
    /// 对象仓库管理接口
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public interface IObjectContainer<T>
    {
        /// <summary>
        /// 获取对象仓库
        /// </summary>
        /// <returns>对象仓库</returns>
        Dictionary<string, T> GetStore();

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">对象</param>
        void AddObject(string key, T obj);

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="key">键</param>
        void DropObject(string key);

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        T GetObject(string key);

        /// <summary>
        /// 判断对象是否存在
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        bool HasObject(string key);
    }
}
