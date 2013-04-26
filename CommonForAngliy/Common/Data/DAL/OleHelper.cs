using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
namespace Common.Data.DAL
{
    internal class OleHelper : DbBase
    {
        public OleHelper(string conn, string providerName)
            : base(conn, providerName)
        { }
        public override void AddReturnPara()
        {

        }
        public override System.Data.Common.DbParameter GetNewParameter()
        {
            return new OleDbParameter();
        }
        public override void AddParameters(string parameterName, object value, DbType dbType, int size, ParameterDirection direction)
        {
            parameterName = parameterName.Substring(0, 1) == "@" ? parameterName : "@" + parameterName;
            OleDbParameter para = new OleDbParameter();
            para.ParameterName = parameterName;
            para.Value = value;
            if (dbType == DbType.DateTime)
            {
                para.OleDbType = OleDbType.DBTimeStamp;
                para.Value = Convert.ToString(value);
            }
            else
            {
                para.DbType = dbType;
                para.Value = value;
            }
            Com.Parameters.Add(para);
        }
    }
}
