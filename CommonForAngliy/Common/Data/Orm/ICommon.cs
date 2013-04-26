using System;
using System.Collections.Generic;
using System.Text;
using Common.Data.Table;

namespace Common.Data.Orm
{
    /// <summary>
    /// 数据操作公共接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface ICommon:IDisposable
    {
        bool Insert();
        bool Update();
        bool Update(object where);
        bool Delete(object where);
     
        bool Fill(object where);
      
        //List<T> Select(int pageIndex, int pageSize, string where, out int count);
        //List<T> Select(int pageIndex, int pageSize, string where, out int count, string customTableName);
        MDataTable Select();
        MDataTable Select(int pageIndex, int pageSize, string where, out int count);

        int GetCount(string where);

    }
}
