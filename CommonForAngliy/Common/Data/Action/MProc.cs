using Common.Data.SQL;
using System.Data.SqlClient;
using System.Data;
using System;
using Common.Data.Table;
using Common.Data.DAL;

namespace Common.Data
{
    /// <summary>
    /// 存储过程、SQL语句操作类
    /// </summary>
    /// <example><code>
    /// 使用示例：
    /// 实例化：  MProc proc = new MProc(ProcNames.GetList);
    /// 添加参数：proc.Set(GetList.ID, 10);
    /// 获取列表：MDataTable table = proc.ExeMDataTable();
    /// 关闭链接：proc.Close();
    /// 绑定控件：GridView1.DataSource = table;GridView1.DataBind();
    /// </code></example>
    public class MProc : IDisposable
    {
        DbBase helper;
        private Aop.IAop _Aop = new Aop.Aop();//切入点
        private string procName=string.Empty;
        private bool isProc = true;
        private string debugInfo = string.Empty;
        public string DebugInfo
        {
            get
            {
                if (helper != null)
                {
                    return helper.attachInfo;
                }
                return debugInfo;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="procNamesEnum">存储过程名称,可通过枚举传入</param>
        /// <example><code>
        ///     MProc action=new MProc(ProcNames.SelectAll);
        /// 或  MProc action=new MProc("SelectAll");
        /// 或多数据库方式：
        /// MAction action=new MAction(P_DataBaseNameEnum.SelectAll);
        /// 说明：自动截取数据库链接[P_及Enum为前后缀],取到的数据库配置项为DataBaseNameConn
        /// U_为表 V_为视图 P_为存储过程
        /// </code></example>
        public MProc(object procNamesEnum)
        {
            Init(procNamesEnum, string.Empty);
        }
        /// <summary>
        /// 构造函数2
        /// </summary>
        /// <param name="procName">存储过程名称,可通过枚举传入</param>
        /// <param name="conn">web.config下的connectionStrings的name配置项名称,或完整的链接字符串</param>
        /// <example><code>
        ///     MProc action=new MProc(ProcNames.SelectAll,"CYQ");
        /// 或  MProc action=new MProc(ProcNames.SelectAll,"server=.;database=CYQ;uid=sa;pwd=123456");
        /// </code></example>
        public MProc(object procNamesEnum, string conn)
        {
            Init(procNamesEnum, conn);
        }
        public void Init(object procNamesEnum, string conn)
        {
            if (procNamesEnum is Enum)
            {
                string enumName = procNamesEnum.GetType().Name;
                if (enumName != "ProcNames")
                {
                    conn = enumName.Substring(2).Replace("Enum", "Conn");
                }
            }
            procName = procNamesEnum.ToString().Trim();
            isProc=!(procName.Contains(" ") && procName.Length > 16);//包含空格，当成sql语句
        
            helper = DalAction.GetHelper(conn);
            Aop.IAop myAop = _Aop.GetFromConfig();//试图从配置文件加载自定义Aop
            if (myAop != null)
            {
                SetAop(myAop);
            }
        }
        /// <summary>
        ///  表切存储过程,在操作完A存储过程后，如果需要操作B存储过程,不需要重新new一个MProc,可直接换用本函数切换
        /// 用法参考MAction的ResetTable
        /// </summary>
        /// <param name="procNamesEnum"></param>
        public void ResetProc(object procNamesEnum)
        {
            helper.ClearParameters();
            procName = procNamesEnum.ToString().Trim();
            isProc = !(procName.Contains(" ") && procName.Length > 16);//包含空格，当成sql语句
        }
        /// <summary>
        /// 返回MDataTable
        /// </summary>
        /// <returns></returns>
        public MDataTable ExeMDataTable(params object[] aopInfo)
        {
            _Aop.Begin(Common.Data.Aop.AopEnum.ExeMDataTable, procName, aopInfo);
            MDataTable mTable = helper.ExeDataReader(procName, isProc);
            _Aop.End(Common.Data.Aop.AopEnum.ExeMDataTable, mTable != null, null, aopInfo);
            return mTable;
        }
      
        /// <summary>
        /// 返回受影响的行数[用于更新或删除]
        /// </summary>
        /// <returns></returns>
        public int ExeNonQuery(params object[] aopInfo)
        {
            _Aop.Begin(Common.Data.Aop.AopEnum.ExeNonQuery, procName, aopInfo);
            int row = helper.ExeNonQuery(procName, isProc);
            _Aop.End(Common.Data.Aop.AopEnum.ExeNonQuery, row > 0, row, aopInfo);
            return row;
        }
        /// <summary>
        /// 返回首行首列的单个值
        /// </summary>
        public T ExeScalar<T>(params object[] aopInfo)
        {
            _Aop.Begin(Common.Data.Aop.AopEnum.ExeScalar, procName, aopInfo);
            object value = helper.ExeScalar(procName, isProc);
            bool result = value != null && value !=DBNull.Value;
            _Aop.End(Common.Data.Aop.AopEnum.ExeScalar, result, result, aopInfo);
            if (!result)
            {
                return default(T);
            }
            switch (typeof(T).Name)
            {
                case "Int32":
                    value = Convert.ToInt32(value);
                    break;
                case "String":
                    value = value.ToString();
                    break;
            }
            return (T)value;
        }
        //public string ExeXmlScalar()
        //{
        //    return helper.ExeXmlScalar(procName, true);
        //}
        /// <summary>
        /// 释放资源,等同于调用Dispose
        /// </summary>
        public void Close()
        {
            Dispose();
        }
        /// <summary>
        /// 设置存储过程参数
        /// </summary>
        /// <param name="paraName">参数名称如["ID"或Users.ID]</param>
        /// <param name="value">参数值如"11"</param>
        public void Set(object paraName, object value)
        {
            helper.AddParameters(Convert.ToString(paraName), value);
        }
        public void Set(object paraName, object value,DbType dbType)
        {
            helper.AddParameters(Convert.ToString(paraName), value, dbType,-1, ParameterDirection.Input);
        }
        public void SetCustom(object paraName, ParaType paraType)
        {
            helper.AddCustomePara(Convert.ToString(paraName), paraType);
        }
        /// <summary>
        ///设置存储过程参数
        /// </summary>
        /// <param name="paraName">参数名称如["ID"或Users.ID]</param>
        /// <param name="value">参数值如"11"</param>
        /// <param name="sqlDbType">值的sql类型</param>
        //public void Set(object paraName, object value,SqlDbType sqlDbType)
        //{
        //    string name = Convert.ToString(paraName);
        //    helper.AddParameters(name.Substring(0, 1) == "@" ? name : "@" + name, value, sqlDbType);
        //}
        /// <summary>
        /// 清除存储过程参数
        /// </summary>
        public void Clear()
        {
            helper.ClearParameters();
        }
        /// <summary>
        /// 存储过程的返回值
        /// </summary>
        public int ReturnValue
        {
            get
            {
               return helper.ReturnValue;
            }
        }

        public void SetNoAop()
        {
            _Aop = new Aop.Aop();
        }
        public void SetAop(Aop.IAop aop)
        {
            _Aop = aop;
            helper.OnExceptionEvent += new DbBase.OnException(helper_OnExceptionEvent);
        }

        void helper_OnExceptionEvent(string msg)
        {
            _Aop.OnError(msg);
        }
        #region 事务操作
        public void SetTransLevel(IsolationLevel level)
        {
            helper.SetLevel(level);
        }
        public void ReOpenTransation()
        {
            helper.openTrans = true;
        }
        /// <summary>
        /// 提交结束事务[默认开启事务,直到调用Close/Disponse时才提交事务]
        /// 如果需要提前结束事务,可调用此方法
        /// </summary>
        public void EndTransation()
        {
            if (helper != null && helper.openTrans)
            {
                helper.EndTransaction();
            }
        }
        public void RollBack()
        {
            if (helper != null && helper.openTrans)
            {
                helper.RollBack();
            }
        }
        #endregion
        #region IDisposable 成员

        public void Dispose()
        {
            if (helper != null)
            {
                debugInfo = helper.attachInfo;
                helper.Dispose();
                helper = null;
            }
        }

        #endregion
    }
    
}
