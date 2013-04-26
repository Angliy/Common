using System.Data.OracleClient;
using System.Data;
namespace Common.Data.DAL
{
    internal class OracleHelper : DbBase
    {
        public OracleHelper(string conn, string providerName)
            : base(conn,providerName)
        { }
        public override void AddReturnPara()
        {
            if (!Com.Parameters.Contains("ResultCursor"))
            {
                AddCustomePara("ResultCursor", ParaType.Cursor);
            }
            if (!Com.Parameters.Contains("ResultCount"))
            {
                AddParameters("ResultCount", null, DbType.Int32, -1, ParameterDirection.Output);//记录总数在最后一位
            }
        }
        internal override void AddCustomePara(string paraName, ParaType paraType)
        {
            OracleParameter para = new OracleParameter();
            para.ParameterName = paraName;
            switch (paraType)
            {
                case ParaType.Cursor:
                case ParaType.OutPut:
                    if (paraType == ParaType.Cursor)
                    {
                        para.OracleType = OracleType.Cursor;
                    }
                    else
                    {
                        para.OracleType = OracleType.Int32;
                    }
                    para.Direction = ParameterDirection.Output;
                    break;
                case ParaType.ReturnValue:
                    para.OracleType = OracleType.Int32;
                    para.Direction = ParameterDirection.ReturnValue;
                    break;

            }
            Com.Parameters.Add(para);
        }
        public override System.Data.Common.DbParameter GetNewParameter()
        {
            return new OracleParameter();
        }
        public override string Pre
        {
            get
            {
                return ":";
            }
        }
    }
}
