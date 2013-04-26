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
    /// ���б����ݲ�����
    /// </summary>
    public class MAction : IDisposable
    {


        #region ˽�б���
        /// <summary>
        /// ���ݲ�������
        /// </summary>
        private DbBase _DataSqlHelper;
        /// <summary>
        /// sqlƴ����
        /// </summary>
        private SQLString _SQLString;
        private MActionUI _UI;
        /// <summary>
        /// �����
        /// </summary>
        private Aop.IAop _Aop = new Aop.Aop();
        /// <summary>
        /// ������
        /// </summary>
        private MDataRow _Row;
        /// <summary>
        /// ����
        /// </summary>
        private string _TableName;
        /// <summary>
        /// �����ַ���
        /// </summary>
        private string _ConnectionString;
        /// <summary>
        /// ���������Ϣ
        /// </summary>
        private string debugInfo = string.Empty;
        /// <summary>
        /// �Ƿ��Զ�������
        /// </summary>
        private bool _IsFillComplete;
        /// <summary>
        /// �Ƿ�ִ�в�������(������Update����)
        /// </summary>
        private bool _IsInsertCommand;
        /// <summary>
        /// ������
        /// </summary>
        private string _SequenceName;

        #endregion

        #region ����
        /// <summary>
        /// Fill��֮�󷵻ص�������
        /// </summary>
        public MDataRow Data
        {
            get
            {
                return _Row;
            }
        }


        /// <summary>
        /// ��ǰ�ı���
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
        /// ���ݿ������ַ���
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
        /// ������Ϣ���,����sql��估�������
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
        /// ���ݿ�����
        /// </summary>
        public DalType DalType
        {
            get
            {
                return _DataSqlHelper.dalType;
            }
        }


        /// <summary>
        /// ����ֵ,ͨ�����ڼ�¼����
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
        /// ������
        /// </summary>
        public string SequenceName
        {
            get { return _SequenceName; }
            set { _SequenceName=value;}
        }


       
        #endregion

        #region ���캯��

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="tableNamesEnum">����/��ͼ����</param>
        /// <example><code>
        ///     MAction action=new MAction(TableNames.Users);
        /// ��  MAction action=new MAction("Users");
        /// ��  MAction action=new MAction("(select m.*,u.UserName from Users u left join Message m on u.ID=m.UserID) v");
        /// ��  MAction action=new MAction(ViewNames.Users);//����ͼ
        /// ������ݿⷽʽ��
        /// MAction action=new MAction(U_DataBaseNameEnum.Users);
        /// ˵�����Զ���ȡ���ݿ�����[U_��EnumΪǰ��׺],ȡ�������ݿ�������ΪDataBaseNameConn
        /// U_Ϊ�� V_Ϊ��ͼ P_Ϊ�洢����
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
        /// ���캯��
        /// </summary>
        /// <param name="tableNamesEnum">����/��ͼ����</param>
        /// <param name="conn">web.config�µ�connectionStrings��name����������,�������������ַ���</param>
        /// <example><code>
        ///     MAction action=new MAction(TableNames.Users,"Conn");
        /// ��  MAction action=new MAction(TableNames.Users,"server=.;database=CYQ;uid=sa;pwd=123456");
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

        #region ��ʼ��
        private void InitSqlHelper(string conn)
        {
            if (_DataSqlHelper == null)
            {
                _DataSqlHelper = DalAction.GetHelper(conn);
                _ConnectionString = _DataSqlHelper.Con.ConnectionString;
            }
        }
        /// <summary>
        /// ���л�,��A��ʱ�������Ҫ����B,����Ҫ����newһ��MAaction,��ֱ�ӻ��ñ������л�
        /// </summary>
        /// <param name="tableName">Ҫ�л��ı���</param>
        /// <example><code>
        ///     MAction action = new MAction(TableNames.Users);
        ///     if (action.Fill("UserName='·������'"))
        ///     {
        ///         int id = action.Get&lt;int&gt;(Users.ID);
        ///         if (action.ResetTable(TableNames.Message))
        ///         {
        ///             //����Message��
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
                throw new Exception("���ݿ��ֶμ���ʧ��!�������ݿ����Ӽ�����(" + TableName + ")�Ƿ����!");
            }
            
            if(_SequenceName!=null)
                _SQLString = new SQLString(ref _Row, _SequenceName, ref _DataSqlHelper);
            else
                _SQLString = new SQLString(ref _Row, ref _DataSqlHelper);

            _UI = new MActionUI(ref _Row);

            Aop.IAop myAop = _Aop.GetFromConfig();//��ͼ�������ļ������Զ���Aop
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
            if (_SQLString != null)//�л����Զ������ʱ��ΪNull
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
            else//��Cache����ʧ��
            {

                try
                {
                    MDataColumn mdcs = null;
                    if (_TableName.IndexOf('(') > -1 && _TableName.IndexOf(')') > -1)//�Զ���table
                    {
                        _TableName = SQLString.FormatDal(_TableName, _DataSqlHelper.dalType, false);
                        _DataSqlHelper.attachInfo = "view";//ʹ��access��ʽ������
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


        #region ���ݿ��������

        private bool InsertOrUpdate(string SqlCommandText)
        {
            bool returnResult = false;
            if (_SQLString._IsCanDo)
            {
                if (_IsInsertCommand) //����
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
                else //����
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
        ///  ��������
        /// </summary>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.Set(Users.Username,"·������");
        /// action.Insert();
        /// action.Close();
        /// </code></example>
        public bool Insert()
        {
            return Insert(false);
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="AutoSetValue">�Ƿ��Զ���ȡֵ[�Զ��ӿؼ���ȡֵ,��Ҫ�ȵ���SetAutoPrefix�������ÿؼ�ǰ׺]</param>
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
        ///  ��������
        /// </summary>
        /// <param name="where">where����,��ֱ�Ӵ�id��ֵ��:[88],������where������:[id=88 and name='·������']</param>
        /// <param name="AutoSetValue">�Ƿ��Զ���ȡֵ[�Զ��ӿؼ���ȡֵ,��Ҫ�ȵ���SetAutoPrefix�������ÿؼ�ǰ׺]</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.SetAutoPrefix("txt","ddl");
        /// action.Update("name='·������'",true);
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
        ///  ��������[Ĭ��ID������ֵ]
        /// </summary>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.Set(Users.Username,"·������");
        /// action.Set(Users.ID,1);
        /// action.Update();
        /// action.Close();
        /// </code></example>
        public bool Update()
        {
            return Update(_Row[0].Value, false);
        }
        /// <summary>
        ///  ��������
        /// </summary>
        /// <param name="where">where����,��ֱ�Ӵ�id��ֵ��:[88],������where������:[id=88 and name='·������']</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// action.Set(Users.Username,"·������");
        /// action.Update("id=1");
        /// action.Close();
        /// </code></example>
        public bool Update(object where)
        {
            return Update(where, false);
        }
        /// <summary>
        ///  ɾ������[Ĭ��ID������ֵ]
        /// </summary>
        public bool Delete()
        {
            return Delete(_Row[0].Value);
        }
        /// <summary>
        ///  ɾ������
        /// </summary>
        /// <param name="where">where����,��ֱ�Ӵ�id��ֵ��:[88],������where������:[id=88 and name='·������']</param>
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
        /// ѡ����������
        /// </summary>
        /// <returns></returns>
        public MDataTable Select(params object[] aopInfo)
        {
            int count;
            return Select(0, 0, "", out count, aopInfo);
        }
        /// <summary>
        /// ���ֲ����ܵ�ѡ��[��������ѯ,ѡ������ʱֻ���PageIndex/PageSize����Ϊ0]
        /// </summary>
        /// <param name="PageIndex">�ڼ�ҳ</param>
        /// <param name="PageSize">ÿҳ����[Ϊ0ʱĬ��ѡ������]</param>
        /// <param name="Where"> ��ѯ����[�ɸ��� order by ���]</param>
        /// <param name="RowCount">���صļ�¼����</param>
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
            MTable = sdReader;//nullҲҪ����һ����null��MDataTable
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
        /// �������[������ѡ��]
        /// </summary>
        /// <param name="where">where����,��ֱ�Ӵ�id��ֵ��:[88],������where������:[id=88 and name='·������']</param>
        /// <example><code>
        /// MAction action=new MAction(TableNames.Users);
        /// if(action.Fill("name='·������'")) //����action.Fill(888) ���� action.Fill(id=888)
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
        /// ���ؼ�¼����
        /// </summary>
        /// <param name="where">where����,��ֱ�Ӵ�id��ֵ��:[88],������where������:[id=88 and name='·������']</param>
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

        #region ��������

        /// <summary>
        /// �ͷ���Դ,��ͬ�ڵ���Disponse
        /// </summary>
        public void Close()
        {
            Dispose();
            if (_Aop != null)
            {
                _Aop = null;
            }
        }
        #region UI����

        /// <summary>
        /// ��ֵ���õ��ؼ���
        /// </summary>
        /// <param name="ct">�ؼ�,Ŀǰ֧������Ϊ[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <param name="value">�Զ���ֵ,����ֵ���ڣ��򲻴ӿؼ��л�ȡֵ</param>
        /// <param name="isControlEnabled">�ؼ��Ƿ����</param>
        /// <example><code>
        /// ʾ��1��action.SetTo(txtUserName,"·������",true);//��ͬ�ڣ�txtUserName.Text="·������";txtUserName.IsEnabled=true;
        /// ʾ��2��action.SetTo(txtUserName,null,false);//ͬ�ڣ�txtUserName.Text=action.Get&lt;string&gt;(Users.UserName);txtUserName.IsEnabled=false;
        /// </code></example>
        public void SetTo(object control, object value, bool isControlEnabled)
        {
            _UI.Set(control, value, isControlEnabled);
        }
        /// <summary>
        /// ��ֵ���õ��ؼ���
        /// </summary>
        /// <param name="ct">�ؼ�,Ŀǰ֧������Ϊ[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <param name="value">�Զ���ֵ,����ֵ���ڣ��򲻴ӿؼ��л�ȡֵ</param>
        /// <example><code>
        /// ʾ��1��action.SetTo(txtUserName,"·������",true);//��ͬ�ڣ�txtUserName.Text="·������";
        /// ʾ��2��action.SetTo(txtUserName,null);//ͬ�ڣ�txtUserName.Text=action.Get&lt;string&gt;(Users.UserName);
        /// </code></example>
        public void SetTo(object control, object value)
        {
            _UI.Set(control, value, true);
        }
        /// <summary>
        /// ��ֵ���õ��ؼ���
        /// </summary>
        /// <param name="ct">�ؼ�,Ŀǰ֧������Ϊ[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <example><code>
        /// ʾ����action.SetTo(txtUserName);//ͬ�ڣ�txtUserName.Text=action.Get&lt;string&gt;(Users.UserName);
        /// </code></example>
        public void SetTo(object control)
        {
            _UI.Set(control, null, true);
        }
        /// <summary>
        /// ���ؼ���ֵ���õ�ʵ����[Ĭ�ϴӿؼ����Զ���ȡֵ]
        /// </summary>
        /// <param name="ct">�ؼ�,Ŀǰ֧������Ϊ[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <param name="value">�Զ���ֵ,����ֵ���ڣ��򲻴ӿؼ��л�ȡֵ</param>
        /// <example><code>
        /// ʾ��1��action.GetFrom(txtUserName,"·������");//���Զ���ֵ��·�����족��������UserName
        /// ʾ��2��action.GetFrom(txtUserName,null);//��txtUserName.Text��ֵ��������UserName
        /// </code></example>
        public void GetFrom(object control, object value)
        {
            _UI.Get(control, value);
        }
        /// <summary>
        /// ���ؼ���ֵ���õ�ʵ����[Ĭ�ϴӿؼ����Զ���ȡֵ]
        /// </summary>
        /// <param name="ct">�ؼ�,Ŀǰ֧������Ϊ[TextBox/Literal/Label/HiddenField/DropDownList/CheckBox]</param>
        /// <example><code>
        /// ʾ����action.GetFrom(txtUserName);//��txtUserName.Text��ֵ��������UserName
        /// </code></example>
        public void GetFrom(object control)
        {
            _UI.Get(control, null);
        }
        /// <summary>
        /// [2.0֮�����ӵĹ���]�󶨿ؼ����ÿؼ���Ҫ�̳��ԣ�ListControl����DropDown/CheckBoxList/RadioButtonList�ȡ�
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
        /// ȡֵ
        /// </summary>
        public T Get<T>(object key)
        {
            return _Row.Get<T>(key);
        }
        /// <summary>
        /// ����ֵ,����:[action.Set(TableName.ID,10);]
        /// </summary>
        /// <param name="key">�ֶ�����,����ö����:[TableName.ID]</param>
        /// <param name="value">Ҫ���ø��ֶε�ֵ</param>
        /// <example><code>
        /// setʾ����action.Set(Users.UserName,"·������");
        /// getʾ����int id=action.Get&lt;int&gt;(Users.ID);
        /// </code></example>
        public void Set(object key, object value)
        {
            _Row[key].Value = value;
        }
        /// <summary>
        /// �Զ�����ǰ׺,�ɴ����ǰ׺[����1��]
        /// </summary>
        /// <param name="autoPrefix">��һ��ǰ׺[����]</param>
        /// <param name="otherPrefix">����N��ǰ׺[��ѡ]</param>
        public void SetAutoPrefix(string autoPrefix, params string[] otherPrefix)
        {
            _UI.SetAutoPrefix(autoPrefix, otherPrefix);
        }

        #region �������

        public void SetTransLevel(IsolationLevel level)
        {
            _DataSqlHelper.SetLevel(level);
        }
        public void ReOpenTransation()
        {
            _DataSqlHelper.openTrans = true;
        }
        /// <summary>
        /// �ύ��������[Ĭ�Ͽ�������,ֱ������Close/Disponseʱ���ύ����]
        /// �����Ҫ��ǰ��������,�ɵ��ô˷���
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

        #region Aop����


        /// <summary>
        /// ȡ��Aop����Aop����ģ��ʹ��MActionʱ�������
        /// </summary>
        public void SetNoAop()
        {
            _Aop = new Aop.Aop();
        }
        /// <summary>
        /// ��������ע���Aop��һ������²���Ҫ�õ���
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


        #region IDisposable ��Ա
        /// <summary>
        /// �ͷ���Դ
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

        #region ICloneable ��Ա
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
