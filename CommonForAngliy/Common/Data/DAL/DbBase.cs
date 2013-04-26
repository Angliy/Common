using System;
using System.Data.Common;
using System.Configuration;
using System.Data;
using Common.Data.SQL;

namespace Common.Data.DAL
{

    /// <summary>
    /// 数据库操作基类
    /// </summary>
    internal abstract class DbBase : IDisposable
    {
        internal string attachInfo = "";
        internal bool openTrans = true;
        public DalType dalType = DalType.Sql;
        private DbProviderFactory _fac = null;
        private DbConnection _con = null;
        public DbConnection Con
        {
            get
            {
                return _con;
            }
        }
        public DbCommand Com
        {
            get
            {
                return _com;
            }
        }
        private DbCommand _com;
        private DbTransaction tran;
        IsolationLevel level = IsolationLevel.Unspecified;
        public bool WriteLog = true;
        public DbBase(string conn, string providerName)
        {
            dalType = DalAction.GetDalType(providerName);
            _fac = DbProviderFactories.GetFactory(providerName);
            _con = _fac.CreateConnection();
            _con.ConnectionString = FormatConn(conn);
            _com = _con.CreateCommand();
            _com.Connection = _con;
        }
        private string FormatConn(string connString)
        {
            if (dalType != DalType.Access)
            {
                string conn = connString.ToLower();
                int index = conn.IndexOf("provider");
                if (index > -1)
                {
                    int end = conn.IndexOf(';', index);
                    if (end > index)
                    {
                        connString = conn.Remove(index, end - index + 1);
                    }
                }
            }
            return connString;
        }
        private int returnValue = -1;
        public int ReturnValue
        {
            get
            {
                if (returnValue == -1 && _com != null && _com.Parameters != null)
                {
                    int.TryParse(Convert.ToString(_com.Parameters[_com.Parameters.Count - 1].Value), out returnValue);
                }
                return returnValue;
            }
            set
            {
                returnValue = value;
            }
        }
        public virtual string Pre
        {
            get
            {
                return "@";
            }
        }
        #region 执行
        public DbDataReader ExeDataReader(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            DbDataReader sdr = null;
            try
            {
                OpenCon();
                sdr = _com.ExecuteReader();
                if (sdr != null && !sdr.HasRows)
                {
                    sdr.Close();
                    sdr = null;
                }

            }
            catch (DbException err)
            {
                WriteError(err.Message);
            }
            return sdr;
        }
        public int ExeNonQuery(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            int rowCount = 1;
            try
            {
                OpenCon();
                _com.ExecuteNonQuery();
            }
            catch (DbException err)
            {
                rowCount = 0;
                WriteError(err.Message);
            }
            finally
            {
                if (!openTrans)
                {
                    CloseCon();
                }
            }
            return rowCount;
        }
        public object ExeScalar(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            object returnValue = null;
            try
            {
                OpenCon();
                returnValue = _com.ExecuteScalar();
            }
            catch (DbException err)
            {
                WriteError(err.Message);
            }
            finally
            {
                if (!openTrans)
                {
                    CloseCon();
                }
            }
            return returnValue;
        }
        public DataTable ExeDataTable(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            DbDataAdapter sdr = _fac.CreateDataAdapter();
            sdr.SelectCommand = _com;
            DataTable dataTable = new DataTable();
            try
            {
                OpenCon();
                sdr.Fill(dataTable);
            }
            catch (DbException err)
            {
                WriteError(err.Message);
            }
            finally
            {
                sdr.Dispose();
                if (!openTrans)
                {
                    CloseCon();
                }
            }
            return dataTable;
        }
        #endregion
        public void AddParameters(string parameterName, object value)
        {
            AddParameters(parameterName, value, DbType.String, -1, ParameterDirection.Input);
        }
        public virtual void AddParameters(string parameterName, object value, DbType dbType, int size, ParameterDirection direction)
        {
            if (dalType == DalType.Oracle)
            {
                parameterName = parameterName.Replace(":", "").Replace("@", "");
            }
            else
            {
                parameterName = parameterName.Substring(0, 1) == "@" ? parameterName : "@" + parameterName;
            }
            DbParameter para = GetNewParameter();
            para.ParameterName = parameterName;
            para.Value = value;
            para.DbType = dbType;

            if (size > -1)
            {
                para.Size = size;
            }
            para.Direction = direction;

            Com.Parameters.Add(para);
        }
        internal virtual void AddCustomePara(string paraName, ParaType paraType) { }
        public abstract DbParameter GetNewParameter();

        public void ClearParameters()
        {
            if (_com != null && _com.Parameters != null)
            {
                _com.Parameters.Clear();
            }
        }
        public abstract void AddReturnPara();

        private void SetCommandText(string commandText, bool isProc)
        {
            _com.CommandText = isProc ? commandText : SQLString.FormatDal(commandText, dalType,false);
            _com.CommandType = isProc ? CommandType.StoredProcedure : CommandType.Text;
            if (isProc && commandText.Contains("SelectBase") && !_com.Parameters.Contains("ReturnValue"))
            {
                AddReturnPara();
            }
            else
            {
                string checkText=commandText.ToLower();
                if (checkText.IndexOf("table") > -1 && (checkText.IndexOf("delete") > -1 || checkText.IndexOf("drop") > -1 || checkText.IndexOf("truncate") > -1))
                {
                    Log.WriteLog(commandText);
                }
            }
            attachInfo += "<br><hr>SQL:<br> " + commandText;
            foreach (DbParameter item in _com.Parameters)
            {
                attachInfo += "<br>Para: " + item.ParameterName + "->" + item.Value;
            }
            attachInfo += "<hr>";
        }
        public void SetLevel(IsolationLevel tranLevel)
        {
            level = tranLevel;
        }
        #region IDisposable 成员

        public void Dispose()
        {

            if (_con != null)
            {
                CloseCon();
                _con = null;
            }
            if (_com != null)
            {
                _com = null;
            }
        }
        private void OpenCon()
        {
            try
            {
                if (_con.State == ConnectionState.Closed)
                {
                    _con.Open();
                    if (openTrans)
                    {
                        tran = _con.BeginTransaction(level);
                        _com.Transaction = tran;
                    }
                }
            }
            catch (DbException err)
            {
                WriteError(err.Message);
            }

        }
        private void CloseCon()
        {
            try
            {
                if (_con.State == ConnectionState.Open)
                {
                    if (tran != null)
                    {
                        openTrans = false;
                        tran.Commit();
                        tran = null;
                    }
                    _con.Close();
                }
            }
            catch (DbException err)
            {
                WriteError(err.Message);
            }

        }
        public void EndTransaction()
        {
            openTrans = false;
            if (tran != null)
            {
                try
                {
                    tran.Commit();
                }
                catch (Exception err)
                {
                    WriteError(err.Message);
                }
                tran = null;
            }
        }
        public void RollBack()
        {
            if (tran != null)
            {
                tran.Rollback();
            }
        }
        internal delegate void OnException(string msg);
        internal event OnException OnExceptionEvent;
        private void WriteError(string err)
        {
            if (OnExceptionEvent != null)
            {
                OnExceptionEvent(err);
            }
            if (tran != null)
            {
                tran.Rollback();
                tran = null;
            }
            if (WriteLog)
            {
                Log.WriteLog(err + attachInfo);
            }

        }
        #endregion
    }
    /// <summary>
    /// 特殊参数类型[MProc操作Oracle时用到]
    /// </summary>
    public enum ParaType
    {
        /// <summary>
        /// 游标类型
        /// </summary>
        Cursor,
        /// <summary>
        /// 输出类型
        /// </summary>
        OutPut,
        /// <summary>
        /// 返回值类型
        /// </summary>
        ReturnValue,
    }
}