using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Aop
{
    /// <summary>
    /// Aop接口，需要实现时继承
    /// </summary>
    public interface IAop
    {
        /// <summary>
        /// 方法调用之前被调用
        /// </summary>
        /// <param name="action">方法名称</param>
        /// <param name="objName">表名/存储过程名/视图名/sql语句</param>
        /// <param name="aopInfo">附带分支参数</param>
        void Begin(AopEnum action,string objName, params object[] aopInfo);
        /// <summary>
        /// 方法调用之后被调用
        /// </summary>
        /// <param name="action">方法名称</param>
        /// <param name="success">调用是否成功</param>
        /// <param name="id">一般调用后的id[或其它值]</param>
        /// <param name="aopInfo">附带分支参数</param>
        void End(AopEnum action, bool success, object id, params object[] aopInfo);
        /// <summary>
        /// 数据库操作产生异常时,引发此方法
        /// </summary>
        /// <param name="msg"></param>
        void OnError(string msg);
        /// <summary>
        /// 内部获取配置Aop，外部使用返回null即可。
        /// </summary>
        /// <returns></returns>
        IAop GetFromConfig();
    }
}
