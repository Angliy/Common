using System;
using System.Collections.Generic;
using System.Text;
using Common.Data.Table;
using System.Reflection;
using Common.Caching;

namespace Common.Data.Orm
{
    /// <summary>
    /// ORM扩展基类
    /// </summary>
    public  abstract class OrmBase<T>: ICommon
    {
        /// <summary>
        /// 数据操作类
        /// </summary>
        static MAction action = null;
      
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="conn"></param>
        public static void Init(string tableName, string conn)
        {
            string key = tableName + "_Action";
            if (Cacher.Contains(key))
                action = Cacher.Get<MAction>(key);
            else
                action=new MAction(tableName, conn);
            action.EndTransation();
        }


        /// <summary>
        ///  带有序列初始化
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="sequenceName"></param>
        /// <param name="conn"></param>
        public static void Init(string tableName, string sequenceName, string conn)
        {
            string key = tableName + "_Action";
            if (Cacher.Contains(key))
                action = Cacher.Get<MAction>(key);
            else
            {
                action = new MAction(tableName, sequenceName, conn);
                Cacher.Insert(key, action);
            }
            action.EndTransation();
        }




        /// <summary>
        /// 将实体属性赋值给操作类
        /// </summary>
        private void SetValueToAction()
        {
            //实体对象
            Object entity = this;
            //实体类型
            Type typeInfo = this.GetType();

            PropertyInfo[] pis = typeInfo.GetProperties();
            object propValue = null;
            for (int i = 0; i < pis.Length; i++)
            {
                propValue = pis[i].GetValue(entity, null);
                //if (propValue == null || Convert.ToString(propValue)==Convert.ToString(DateTime.MinValue))
                //{
                //    continue;
                //}
                action.Data[pis[i].Name].Value = propValue;
            }
        }

        /// <summary>
        /// 操作类将值赋给实体
        /// </summary>
        private void SetValueToEntity()
        {
            //实体对象
            Object entity = this;
            //实体类型
            Type typeInfo = this.GetType();

            PropertyInfo[] pis = typeInfo.GetProperties();
            object propValue = null;
            for (int i = 0; i < pis.Length; i++)
            {
                propValue = action.Data[pis[i].Name].Value;
                //if (propValue == null)
                //{
                //    continue;
                //}
                pis[i].SetValue(entity, propValue,null);
            }
        }
       
        #region ICommon 成员

        public bool Insert()
        {
            SetValueToAction();
            bool result=action.Insert();
            SetValueToEntity();
            return result;
        }

        public bool Update()
        {
            SetValueToAction();
            return action.Update();
        }

        public bool Update(object where)
        {
            SetValueToAction();
            return action.Update(where);
        }

        public bool Delete(object where)
        {
            return action.Delete(where);
        }

        public bool Delete()
        {
            SetValueToAction();
            return action.Delete();
        }

        public bool Fill(object where)
        {
            bool result = action.Fill(where);
            if (result)
            {
                SetValueToEntity();
            }
            return result;
        }
        public MDataTable Select()
        {
            int count;
            return Select(0, 0, string.Empty, out count);
        }
        public MDataTable Select(int pageIndex, int pageSize, string where, out int count)
        {
           return  action.Select(pageIndex, pageSize, where, out count);
        }


        public int GetCount(string where)
        {
            return action.GetCount(where);
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (action != null)
            {
                action.Close();
            }
        }

        #endregion

       
    }
   

  
}
