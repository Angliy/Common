using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Factory
{
    public interface IFactory<T>
    {
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="name">对象名</param>
        /// <returns>对象</returns>
        T Get(string name, params object[] paras);
    }
}
