using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
namespace Common.Data.DAL
{
    internal class SQLHelper : DbBase
    {
        public SQLHelper(string conn,string providerName)
            : base(conn,providerName)
        { }
        public override void AddReturnPara()
        {
            AddParameters("ReturnValue", null, DbType.Int32, 32, ParameterDirection.ReturnValue);
        }
        internal override void AddCustomePara(string paraName, ParaType paraType)
        {
            switch (paraType)
            {
                case ParaType.OutPut:
                case ParaType.ReturnValue:
                    SqlParameter para = new SqlParameter();
                    para.ParameterName = paraName;
                    para.SqlDbType = SqlDbType.Int;
                    if (paraType == ParaType.OutPut)
                    {
                        para.Direction = ParameterDirection.Output;
                    }
                    else
                    {
                        para.Direction = ParameterDirection.ReturnValue;
                    }
                    Com.Parameters.Add(para);
                    break;
            }
        }
        public  void AddParameters(string parameterName, object value, DbType dbType, int size)
        {
            AddParameters(parameterName, value, dbType, size, ParameterDirection.Input);
        }
        public override System.Data.Common.DbParameter GetNewParameter()
        {
            return new SqlParameter();
        }
    }
}
