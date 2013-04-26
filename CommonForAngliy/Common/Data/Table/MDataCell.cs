using System;
using System.Data;
using System.Collections.Generic;
using Common.Data.SQL;
namespace Common.Data.Table
{
    internal class ValueFormat
    {
        public static bool IsFormat = false;
        public static string formatString = "{0}";
        public static string Format(object value)
        {
            return string.Format(formatString, value);
        }
        public static void Reset()
        {
            IsFormat = false;
            formatString = "{0}";
        }
    }
    /// <summary>
    /// ��Ԫֵ
    /// </summary>
    public class CellValue
    {
        internal bool IsNull=true;
        internal bool IsChange=false;
        internal object Value=null;
    }

    /// <summary>
    /// ��Ԫ�ṹ����
    /// </summary>
    public class CellStruct
    {
        public bool IsCanNull;
        public bool IsReadOnly;
        public string ColumnName;
        public System.Data.SqlDbType SqlType;
        public int MaxSize;
        public string Operator = "=";
        public ParameterDirection ParaDirection;
        internal Type ValueType;
        #region ���캯��
        public CellStruct(string columnName, System.Data.SqlDbType sqlType, bool isReadOnly, bool isCanNull, int maxSize, ParameterDirection paraDirection)
        {
            ColumnName = columnName;
            SqlType = sqlType;
            IsReadOnly = isReadOnly;
            IsCanNull = isCanNull;
            MaxSize = maxSize;
            ParaDirection = paraDirection;
            ValueType = DataType.GetType(sqlType);
        }
        #endregion
    }
    /// <summary>
    /// ��Ԫ��
    /// </summary>
    public class MDataCell
    {
        internal CellValue _CellValue;
        internal CellStruct _CellStruct;
      
        #region ���캯��
        internal MDataCell(ref CellStruct dataStruct)
        {
            Init(dataStruct, null);
        }

        internal MDataCell(ref CellStruct dataStruct, object value)
        {
            Init(dataStruct, value);
        }

        #endregion

        #region ��ʼ��
        private void Init(CellStruct dataStruct, object value)
        {
            _CellValue = new CellValue();
            _CellStruct = dataStruct;
            _CellValue.Value = value;
        }
        #endregion

        #region ����

       
        public object Value
        {
            get
            {

                return _CellValue.Value;
            }
            set
            {
                //if (value != null)
                //{
                    _CellValue.IsChange = true;
                    _CellValue.IsNull = false;
                    if (ValueFormat.IsFormat && DataType.GetGroupID(_CellStruct.SqlType) == 0)
                    {
                        _CellValue.Value = ValueFormat.Format(value);
                    }
                    else
                    {
                        _CellValue.Value = value;
                    }
                    //if (_CellValue.ValueType == null)
                    //{
                    //    _CellValue.ValueType = value.GetType();
                    //}
                //}
               
            }
        }
        #endregion

        
    }



 

}

