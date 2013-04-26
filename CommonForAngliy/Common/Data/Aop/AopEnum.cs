using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Aop
{
    /// <summary>
    /// 框架内部数据库操作方法枚举
    /// </summary>
    public enum AopEnum
    {
        /// <summary>
        /// 查询多条记录方法
        /// </summary>
        Select,
        /// <summary>
        /// 插入方法
        /// </summary>
        Insert,
        /// <summary>
        /// 更新方法
        /// </summary>
        Update,
        /// <summary>
        /// 删除方法
        /// </summary>
        Delete,
        /// <summary>
        /// 查询一条记录方法
        /// </summary>
        Fill,
        /// <summary>
        /// 取记录总数
        /// </summary>
        GetCount,
        /// <summary>
        /// MProc查询返回MDataTable方法
        /// </summary>
        ExeMDataTable,
        /// <summary>
        /// MProc执行返回受影响行数方法
        /// </summary>
        ExeNonQuery,
        /// <summary>
        /// MProc执行返回首行首列方法
        /// </summary>
        ExeScalar
    }
}
