using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.ComponentModel;

using Common.Caching;
using Common.Data.SQL;
using Common.Data.Aop;

using Common.Data.Table;
using Common.Data.DAL;

namespace Common.Data
{
    /// <summary>
    /// 所有表数据操作类
    /// </summary>
    public class MAction : IDisposable
    {


        #region 私有变量
        /// <summary>
        /// 数据操作基类
        /// </summary>
        private DbBase _DataSqlHelper;
        /// <summary>
        /// sql拼接类
        /// </summary>
        private SQLString _SQLString;
        private MActionUI _UI;
        /// <summary>
        /// 切入点
        /// </summary>
        private Aop.IAop _Aop = new Aop.Aop();
        /// <summary>
        /// 行数据
        /// </summary>
        private MDataRow _Row;
        /// <summary>
        /// 表名
        /// </summary>
        private string _TableName;
        /// <summary>
        /// 连接字符串
        /// </summary>
        private string _ConnectionString;
        /// <summary>
        /// 调试输出信息
        /// </summary>
        private string debugInfo = string.Empty;
        /// <summary>
        /// 是否自动填充完成
        /// </summary>
        private bool _IsFillComplete;
        /// <summary>
        /// 是否执行插入命令(区分于Update命令)
        /// </summary>
        private bool _IsInsertCommand;
        /// <summary>
        /// 序列名
        /// </summary>
        private string _SequenceName;

        #endregion

        #region 属性
        /// <summary>
        /// Fill完之后返回的行数据
        /// </summary>
        public MDataRow Data
        {
            get
            {
                return _Row;
            }
        }


        /// <summary>
        /// 当前的表名
        /// </summary>
        public string TableName
        {
            get
            {
                return _TableName;
            }
            set
            {
                _TableName = value;
            }
        }
       
        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;
            }
        }


        /// <summary>
        /// 调试信息输出,包括sql语句及传输参数
        /// </summary>
        public string DebugInfo
        {
            get
            {
                if (_DataSqlHelper != null)
                {
                    return _DataSqlHelper.attachInfo;
                }
                return debugInfo;
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DalType DalType
        {
            get
            {
                return _DataSqlHelper.dalType;
            }
        }


        /// <summary>
        /// 返回值,通常用于记录总数
        /// </summary>
        public int ReturnValue
        {
            get
            {
                if (_DataSqlHelper != null)
                {
                    return _DataSqlHelper.ReturnValue;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 序列名
        /// </summary>
        public string SequenceName
        {
            get { return _SequenceName; }
            set { _SequenceName=value;}
        }


       
        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tableNamesEnum">表名/视图名称</param>
        /// <example><code>
        ///     MAction action=new MAction(TableNames.Users);
        /// 或  MAction action=new MAction("Users");
        /// 或  MAction action=new MAction("(select m.*,u.UserName from Users u left join Message m on u.ID=m.UserID) v");
        /// 或  MAction action=new MAction(ViewNames.Users);//传视图
        /// 或多数据库方式：
        /// MAction action=new MAction(U_DataBaseNameEnum.Users);
        /// 说明：自动截取数据库链接[U_及Enum为前后缀],取到的数据库配置项为DataBaseNameConn
        /// U_为表 V_为视图 P_为存储过程
        /// </code></example>
        public MAction(object tableNamesEnum)
        {
            string conn = string.Empty;
            if (tableNamesEnum is Enum)
            {
                string enumName = tableNamesEnum.GetType().Name;
                if (enumName != "TableNames" && enumName != "ViewNames")
                {
                    conn = enumName.Substring(2).Replace("Enum", "Conn");
                }
            }
            Init(tableNamesEnum.ToString(), conn);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tableNamesEnum">表名/视图名称</param>
        /// <param name="conn">web.config下的connectionStrings的name配置项名称,或完整的链接字符串</param>
        /// <example><code>
        ///     MAction action=new MAction(TableNames.Users,"Conn");
        /// 或  MAction action=new MAction(TableNames.Users,"server=.;database=CYQ;uid=sa;pwd=123456");
        /// </code></example>
        public MAction(object tableNamesEnum, string conn)
        {
            Init(tableNamesEnum.ToString(), conn);
        }



        public MAction(object tableNamesEnum, string sequenceName, string conn)
        {
            _SequenceName = sequenceName;
            Init(tableNamesEnum.ToString(), conn);
        }

        #endregion

        #region 初始化
        private void InitSqlHelper(string conn)
        {
            if (_DataSqlHelper == null)
            {
                _DataSqlHelper = DalAction.GetHelper(conn);
                _ConnectionString = _DataSqlHelper.Con.ConnectionString;
            }
        }
        /// <summary>
        /// 表切换,在A表时，如果需要操作B,不需要重新new一个MAaction,可直接换用本函数切换
        /// </summary>
        /// <param name="tableName">要切换的表名</param>
        /// <example><code>
        ///     MAction action = new MAction(TableNames.Users);
        ///     if (action.Fill("UserName='路过秋天'"))
        ///     {
        ///         int id = action.Get&lt;int&gt;(Users.ID);
        ///         if (action.ResetTable(TableNames.Message))
        ///         {
        ///             //处理Message表
        ///             action.Close();
        ///          }
        ///      }
        /// </code></example>
        public bool ResetTable(object tableName)
        {
            _TableName = tableName.ToString();
            if (!GetPropertysByTableName())
            {
                OnError();
                return false;
            }
            ResetRow();
            return true;
        }
        private void Init(string tableName, string conn)
        {
            _TableName = tableName;
            InitSqlHelper(conn);
            _Row = new MDataRow();
            
            if (!GetPropertysByTableName())
            {
                throw new Exception("数据库字段加载失败!请检查数据库链接及表名(" + TableName + ")是否存在!");
            }
            
            if(_SequenceName!=null)
                _SQLString = new SQLString(ref _Row, _SequenceName, ref _DataSqlHelper);
            else
                _SQLString = new SQLString(ref _Row, ref _DataSqlHelper);

            _UI = new MActionUI(ref _Row);

            Aop.IAop myAop = _Aop.GetFromConfig();//试图从配置文件加载自定义Aop
            if (myAop != null)
            {
                SetAop(myAop);
            }
            if (_DataSqlHelper.dalType == DalType.Access)
            {
                EndTransation();
            }
        }
        private void ResetRow()
        {
            if (_SQLString != null)//切换到自定义语句时会为Null
            {
                _SQLString.SetRow(ref _Row);
                _UI._Row = _Row;
            }
        }
        private bool GetPropertysByTableName()
        {
            if (GetPropertysFromCache())
            {
                return true;
            }
            else//从Cache加载失败
            {

                try
                {
                    MDataColumn mdcs = null;
                    if (_TableName.IndexOf('(') > -1 && _TableName.IndexOf(')') > -1)//自定义table
                    {
                        _TableName = SQLString.FormatDal(_TableName, _DataSqlHelper.dalType, false);
                        _DataSqlHelper.attachInfo = "view";//使用access方式加载列
                        mdcs = OutPutData.GetColumn(_TableName, ref _DataSqlHelper);
                    }
                    else
                    {
                        mdcs = OutPutData.GetColumn(_TableName, ref _DataSqlHelper);
                    }
                    _Row = ToDataRow(mdcs);
                    _Row.TableName = _TableName;
                    Cacher.Insert(_TableName + "_Columns", mdcs.Clone());
                    return true;

                }
                catch
                {
                    return false;
                }

            }
        }
        private bool GetPropertysFromCache()
        {
            bool returnResult = false;
          
            if (Cacher.Contains(_TableName))
            {
                try
                {
                    _Row = ToDataRow(Cacher.Get(_TableName) as MDataColumn);
                    returnResult = _Row.Count > 0;
                }
                catch
                {
                }
            }
            return returnResult;
        }
        #endregion


        #region 数据库操作方法

        private bool InsertOrUpdate(string SqlCommandText)
        {
            bool returnResult = false;
            if (_SQLString._IsCanDo)
            {
                if (_IsInsertCommand) //插入
                {
                    _IsInsertCommand = false;
                    object ID;
                    switch (_DataSqlHelper.dalType)
                    {
                        case DalType.Sql:
                            ID = _DataSqlHelper.ExeScalar(SqlCommandText, false);
                            break;
                        default:
                            ID = _DataSqlHelper.ExeNonQuery(SqlCommandText, false);
                            if (ID != null && Convert.ToInt32(ID) > 0)
                            {
                                _DataSqlHelper.ClearParameters();
                                ID = _DataSqlHelper.ExeScalar(_SQLString.GetMaxID(), false);
                            }
                            break;
                    }
                    if (ID != null)
                    {
                        returnResult = _IsFillComplete = Fill(ID);
                    }
                }
                else //更新
                {
                    returnResult = _DataSqlHelper.ExeNonQuery(SqlCommandText, false) > 0;
                }
            }
            if (returnResult)
            {
                _SQLString._IsCanDo = false;
            }
            return returnResult;
        }
        /// <summary>
        ///  插入数据
        /// </summary>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.Set(Users.Username,"路过秋天");
        /// action.Insert();
        /// action.Close();
        /// </code></example>
        public bool Insert()
        {
            return Insert(false);
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="AutoSetValue">是否自动获取值[自动从控件获取值,需要先调用SetAutoPrefix方法设置控件前缀]</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.SetAutoPrefix("txt","ddl");
        /// action.Insert(true);
        /// action.Close();
        /// </code></example>
        public bool Insert(bool AutoSetValue, params object[] aopInfo)
        {
            _Aop.Begin(Aop.AopEnum.Insert, _TableName, aopInfo);
            if (AutoSetValue)
            {
                _UI.AutoSetColumnValue(true);
            }
            _IsInsertCommand = true;
            _DataSqlHelper.ClearParameters();
            bool result = InsertOrUpdate(_SQLString.GetInsertSql());
            object id = 0;
            if (!result)
            {
                OnError();
            }
            else
            {
                id = _Row[0].Value;
            }
            _Aop.End(Aop.AopEnum.Insert, result, id, aopInfo);
            return result;
        }
        /// <summary>
        ///  更新数据
        /// </summary>
        /// <param name="where">where条件,可直接传id的值如:[88],或传完整where条件如:[id=88 and name='路过秋天']</param>
        /// <param name="AutoSetValue">是否自动获取值[自动从控件获取值,需要先调用SetAutoPrefix方法设置控件前缀]</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.SetAutoPrefix("txt","ddl");
        /// action.Update("name='路过秋天'",true);
        /// action.Close();
        /// </code></example>
        public bool Update(object where, bool AutoSetValue, params object[] aopInfo)
        {
            _Aop.Begin(Aop.AopEnum.Update, _TableName, aopInfo);
            if (AutoSetValue)
            {
                _UI.AutoSetColumnValue(false);
            }
            if (where == null && _Row[0].Value != null)
            {
                where = _Row[0].Value;
            }
            object id = where;
            _DataSqlHelper.ClearParameters();
            string command = _SQLString.GetUpdateSql(where);
            bool result = InsertOrUpdate(command);
            if (!result)
            {
                OnError();
            }
            _Aop.End(Aop.AopEnum.Update, result, id, aopInfo);
            return result;
        }
        /// <summary>
        ///  更新数据[默认ID必须有值]
        /// </summary>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.Set(Users.Username,"路过秋天");
        /// action.Set(Users.ID,1);
        /// action.Update();
        /// action.Close();
        /// </code></example>
        public bool Update()
        {
            return Update(_Row[0].Value, false);
        }
        /// <summary>
        ///  更新数据
        /// </summary>
        /// <param name="where">where条件,可直接传id的值如:[88],或传完整where条件如:[id=88 and name='路过秋天']</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.Set(Users.Username,"路过秋天");
        /// action.Update("id=1");
        /// action.Close();
        /// </code></example>
        public bool Update(object where)
        {
            return Update(where, false);
        }
        /// <summary>
        ///  删除数据[默认ID必须有值]
        /// </summary>
        public bool Delete()
        {
            return Delete(_Row[0].Value);
        }
        /// <summary>
        ///  删除数据
        /// </summary>
        /// <param name="where">where条件,可直接传id的值如:[88],或传完整where条件如:[id=88 and name='路过秋天']</param>
        public bool Delete(object where, params object[] aopInfo)
        {
            _Aop.Begin(Aop.AopEnum.Delete, _TableName, aopInfo);
            object id = where;
            _DataSqlHelper.ClearParameters();
            int row = _DataSqlHelper.ExeNonQuery(_SQLString.GetDeleteSql(where), false);
            bool result = row > 0;
            if (result)
            {
                return true;
            }
            else
            {
                OnError();
            }
            _Aop.End(Aop.AopEnum.Delete, result, id, aopInfo);
            return result;
        }

        /// <summary>
        /// 选择所有数据
        /// </summary>
        /// <returns></returns>
        public MDataTable Select(params object[] aopInfo)
        {
            int count;
            return Select(0, 0, "", out count, aopInfo);
        }
        /// <summary>
        /// 带分布功能的选择[多条件查询,选择所有时只需把PageIndex/PageSize设置为0]
        /// </summary>
        /// <param name="PageIndex">第几页</param>
        /// <param name="PageSize">每页数量[为0时默认选择所有]</param>
        /// <param name="Where"> 查询条件[可附带 order by 语句]</param>
        /// <param name="RowCount">返回的记录总数</param>
        /// <returns></returns>
        public MDataTable Select(int pageIndex, int pageSize, string where, out int rowCount, params object[] aopInfo)
        {
            _Aop.Begin(Aop.AopEnum.Select, _TableName, aopInfo);
            rowCount = 0;
            MDataTable MTable = null;
            _DataSqlHelper.ClearParameters();
            DbDataReader sdReader = null;
            if (_SQLString != null)
            {
                where = _SQLString.FormatWhere(where);
            }
            else
            {
                where = SQLString.FormatDal(where, _DataSqlHelper.dalType, true);
            }
            switch (_DataSqlHelper.dalType)
            {
                case DalType.Sql:
                    _DataSqlHelper.AddParameters("@PageIndex", pageIndex, DbType.Int32, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddParameters("@PageSize", pageSize, DbType.Int32, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddParameters("@TableName", _TableName, DbType.String, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddParameters("@Where", where, DbType.String, -1, ParameterDirection.Input);
                    sdReader = _DataSqlHelper.ExeDataReader("SelectBase", true);
                    break;
                case DalType.Access:
                    rowCount = GetCount(where);
                    if (rowCount > 0 && (pageIndex - 1) * pageSize < rowCount)
                    {
                        string sql = Pager.GetAccess(pageIndex, pageSize, where, _TableName, rowCount);
                        sdReader = _DataSqlHelper.ExeDataReader(sql, false);
                    }
                    break;
                case DalType.Oracle:
                    _DataSqlHelper.AddParameters("PageIndex", pageIndex, DbType.Int32, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddParameters("PageSize", pageSize, DbType.Int32, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddParameters("TableName", _TableName, DbType.String, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddParameters("Filters", where, DbType.String, -1, ParameterDirection.Input);
                    _DataSqlHelper.AddCustomePara("DataRows", ParaType.Cursor);
                    _DataSqlHelper.AddCustomePara("TotalRows", ParaType.OutPut);

                    sdReader = _DataSqlHelper.ExeDataReader("COMMON_PAGELIST_PROC", true);
                    break;
            }
            bool result = false;
            MTable = sdReader;//null也要返回一个非null的MDataTable
            if (sdReader != null)
            {
                sdReader.Close();
                result = true;
                switch (_DataSqlHelper.dalType)
                {
                    case DalType.Sql:
                    case DalType.Oracle:
                        rowCount = _DataSqlHelper.ReturnValue;
                        break;
                    //case DalType.Access:
                    //    rowCount = GetCount(where);
                    //    break;
                }
            }
            else
            {
                MTable.LoadRow(_Row);
            }
            _DataSqlHelper.ClearParameters();
            _Aop.End(Aop.AopEnum.Select, result, rowCount, aopInfo);
            return MTable;
        }


        /// <summary>
        /// 填充自身[即单行选择]
        /// </summary>
        /// <param name="where">where条件,可直接传id的值如:[88],或传完整where条件如:[id=88 and name='路过秋天']</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// if(action.Fill("name='路过秋天'")) //或者action.Fill(888) 或者 action.Fill(id=888)
        /// {
        ///     action.SetTo(labUserName);
        ///     action.Close();
        /// }
        /// </code></example>
        public bool Fill(object where, params object[] aopInfo)
        {
            _Aop.Begin(Aop.AopEnum.Fill, _TableName, aopInfo);
            bool result = false;
            if (_DataSqlHelper != null)
            {
                _DataSqlHelper.ClearParameters();
                MDataTable mTable = _DataSqlHelper.ExeDataReader(_SQLString.GetTopOneSql(where), false);
                result = (mTable != null && mTable.Rows.Count > 0);
                if (result)
                {
                    _Row = mTable.Rows[0];
                    _Row.TableName = TableName;
                    where = _Row[0].Value;
                    ResetRow();
                }
                else
                {
                    OnError();
                }
            }
            _Aop.End(Aop.AopEnum.Fill, result, where, aopInfo);
            return result;
        }
        /// <summary>
        /// 返回记录总数
        /// </summary>
        /// <param name="where">where条件,可直接传id的值如:[88],或传完整where条件如:[id=88 and name='路过秋天']</param>
        public int GetCount(string where, params object[] aopInfo)
        {
            _Aop.Begin(Aop.AopEnum.GetCount, _TableName, aopInfo);
            _DataSqlHelper.ClearParameters();
            object count = _DataSqlHelper.ExeScalar(_SQLString.GetCountSql(where), false);
            bool result = (count != null && Convert.ToInt32(count) > 0);
            _Aop.End(Aop.AopEnum.GetCount, result, count, aopInfo);
            return result ? Convert.ToInt32(count) : 0;
        }

        #endregion

        #region 其它方法

        /// <summary>
        /// 释放资源,等同于调用Disponse
        /// </summary>
        public void Close()
        {
            Dispose();
            if (_Aop != null)
            {
                _Aop = null;
            }
        }
        #region UI操作

        /// <summary>
        /// 将值设置到控件中
        /// </summary>
        /// <param name="ct">控件,目前支持类型为[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <param name="value">自定义值,若此值存在，则不从控件中获取值</param>
        /// <param name="isControlEnabled">控件是否可用</param>
        /// <example><code>
        /// 示例1：action.SetTo(txtUserName,"路过秋天",true);//等同于：txtUserName.Text="路过秋天";txtUserName.IsEnabled=true;
        /// 示例2：action.SetTo(txtUserName,null,false);//同于：txtUserName.Text=action.Get&lt;string&gt;(Users.UserName);txtUserName.IsEnabled=false;
        /// </code></example>
        public void SetTo(object control, object value, bool isControlEnabled)
        {
            _UI.Set(control, value, isControlEnabled);
        }
        /// <summary>
        /// 将值设置到控件中
        /// </summary>
        /// <param name="ct">控件,目前支持类型为[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <param name="value">自定义值,若此值存在，则不从控件中获取值</param>
        /// <example><code>
        /// 示例1：action.SetTo(txtUserName,"路过秋天",true);//等同于：txtUserName.Text="路过秋天";
        /// 示例2：action.SetTo(txtUserName,null);//同于：txtUserName.Text=action.Get&lt;string&gt;(Users.UserName);
        /// </code></example>
        public void SetTo(object control, object value)
        {
            _UI.Set(control, value, true);
        }
        /// <summary>
        /// 将值设置到控件中
        /// </summary>
        /// <param name="ct">控件,目前支持类型为[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <example><code>
        /// 示例：action.SetTo(txtUserName);//同于：txtUserName.Text=action.Get&lt;string&gt;(Users.UserName);
        /// </code></example>
        public void SetTo(object control)
        {
            _UI.Set(control, null, true);
        }
        /// <summary>
        /// 将控件的值设置到实体中[默认从控件中自动获取值]
        /// </summary>
        /// <param name="ct">控件,目前支持类型为[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <param name="value">自定义值,若此值存在，则不从控件中获取值</param>
        /// <example><code>
        /// 示例1：action.GetFrom(txtUserName,"路过秋天");//将自定义值“路过秋天”赋给属性UserName
        /// 示例2：action.GetFrom(txtUserName,null);//将txtUserName.Text的值赋给属性UserName
        /// </code></example>
        public void GetFrom(object control, object value)
        {
            _UI.Get(control, value);
        }
        /// <summary>
        /// 将控件的值设置到实体中[默认从控件中自动获取值]
        /// </summary>
        /// <param name="ct">控件,目前支持类型为[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <example><code>
        /// 示例：action.GetFrom(txtUserName);//将txtUserName.Text的值赋给属性UserName
        /// </code></example>
        public void GetFrom(object control)
        {
            _UI.Get(control, null);
        }
        /// <summary>
        /// [2.0之后增加的功能]绑定控件，该控件需要继承自：ListControl。如DropDown/CheckBoxList/RadioButtonList等。
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public MAction Bind(object control)
        {
            return Bind(control, string.Empty, MBindUI.GetID(control).Substring(3), "ID");
        }
        public MAction Bind(object control, string where)
        {
            return Bind(control, where, MBindUI.GetID(control).Substring(3), "ID");
        }
        public MAction Bind(object control, string where, object text, object value)
        {
            MDataTable mTable = _DataSqlHelper.ExeDataReader(_SQLString.GetBindSql(where, text, value), false);
            bool result = (mTable != null && mTable.Rows.Count > 0);
            if (result)
            {
                MBindUI.BindList(control, mTable);
            }
            return this;
        }
        #endregion

        /// <summary>
        /// 取值
        /// </summary>
        public T Get<T>(object key)
        {
            return _Row.Get<T>(key);
        }
        /// <summary>
        /// 设置值,例如:[action.Set(TableName.ID,10);]
        /// </summary>
        /// <param name="key">字段名称,可用枚举如:[TableName.ID]</param>
        /// <param name="value">要设置给字段的值</param>
        /// <example><code>
        /// set示例：action.Set(Users.UserName,"路过秋天");
        /// get示例：int id=action.Get&lt;int&gt;(Users.ID);
        /// </code></example>
        public void Set(object key, object value)
        {
            _Row[key].Value = value;
        }
        /// <summary>
        /// 自动设置前缀,可传多个前缀[至少1个]
        /// </summary>
        /// <param name="autoPrefix">第一个前缀[必须]</param>
        /// <param name="otherPrefix">后面N个前缀[可选]</param>
        public void SetAutoPrefix(string autoPrefix, params string[] otherPrefix)
        {
            _UI.SetAutoPrefix(autoPrefix, otherPrefix);
        }

        #region 事务操作

        public void SetTransLevel(IsolationLevel level)
        {
            _DataSqlHelper.SetLevel(level);
        }
        public void ReOpenTransation()
        {
            _DataSqlHelper.openTrans = true;
        }
        /// <summary>
        /// 提交结束事务[默认开启事务,直到调用Close/Disponse时才提交事务]
        /// 如果需要提前结束事务,可调用此方法
        /// </summary>
        public void EndTransation()
        {
            if (_DataSqlHelper != null && _DataSqlHelper.openTrans)
            {
                _DataSqlHelper.EndTransaction();
            }
        }
        public void RollBack()
        {
            if (_DataSqlHelper != null && _DataSqlHelper.openTrans)
            {
                _DataSqlHelper.RollBack();
            }
        }
        #endregion

        #region Aop操作


        /// <summary>
        /// 取消Aop，在Aop独立模块使用MAction时必须调用
        /// </summary>
        public void SetNoAop()
        {
            _Aop = new Aop.Aop();
        }
        /// <summary>
        /// 主动设置注入的Aop，一般情况下不需要用到。
        /// </summary>
        /// <param name="aop"></param>
        public void SetAop(Aop.IAop aop)
        {
            _Aop = aop;
            _DataSqlHelper.OnExceptionEvent += new DbBase.OnException(_DataSqlHelper_OnExceptionEvent);
        }

        void _DataSqlHelper_OnExceptionEvent(string msg)
        {
            _Aop.OnError(msg);
        }
        #endregion

        public void FormatAllInputValue(string format)
        {
            ValueFormat.IsFormat = true;
            ValueFormat.formatString = format;
        }
        public void EndFormat()
        {
            ValueFormat.Reset();
        }
        #endregion


        #region IDisposable 成员
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            EndFormat();
            if (_DataSqlHelper != null)
            {
                //if (_Row != null)
                //{
                //    _Row.Clear();
                //    _Row = null;
                //}
                _DataSqlHelper.Dispose();
                if (_DataSqlHelper != null)
                {
                    debugInfo += _DataSqlHelper.attachInfo;
                    _DataSqlHelper = null;
                }
                if (_SQLString != null)
                {
                    _SQLString = null;
                }
                if (_UI != null)
                {
                    _UI.Dispose();
                }
            }
        }
        internal void OnError()
        {
            if (_DataSqlHelper != null && _DataSqlHelper.openTrans)
            {
                Dispose();
            }
        }
        #endregion

        #region ICloneable 成员
        internal MDataRow ToDataRow(MDataColumn mdcs)
        {
            MDataRow dataRecord = new MDataRow();

            for (int i = 0; i < mdcs.Count; i++)
            {
                CellStruct mcb = mdcs[i];
                MDataCell mdc = new MDataCell(ref mcb);
                dataRecord.Add(mdc);
            }
            dataRecord.TableName = this._TableName;

            return dataRecord;
        }

        #endregion
    }


}
