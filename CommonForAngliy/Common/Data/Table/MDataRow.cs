using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.ComponentModel;

namespace Common.Data.Table
{
    /// <summary>
    /// 一行记录
    /// </summary>
    public class MDataRow : List<MDataCell>, IDataRecord, System.ComponentModel.ICustomTypeDescriptor
    {

        public MDataRow()
            : base()
        {
        }
        private string _TableName;
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
        /// 输入枚举型数据
        /// </summary>
        public MDataCell this[object filed]
        {
            get
            {
                if (filed is Enum || filed is int)
                {
                    return base[(int)filed];
                }
                return this[filed.ToString()];
            }
        }
        public MDataCell this[string Key]
        {
            get
            {
                MDataCell dataCell = null;
                for (int i = 0; i < base.Count; i++)
                {
                    if (base[i]._CellStruct.ColumnName.ToLower() == Key.ToLower())
                    {
                        dataCell = base[i];
                        break;
                    }
                }
                return dataCell;
            }
        }
        /// <summary>
        /// 取值
        /// </summary>
        public T Get<T>(object key)
        {
            object value = this[key].Value;
            if (value == null || value == DBNull.Value)
            {
                return default(T);
            }

            switch (typeof(T).Name)
            {
                case "Int32":
                    value = Convert.ToInt32(value);
                    break;
                //case "Single":
                //case "Double":
                //case "Decimal":
                //case "DateTime":
                //    break;
                //default:
                case "String":
                    value = value.ToString();
                    break;

            }
            return (T)value;
        }
        public MDataTable ToTable()
        {
            MDataTable mTable = new MDataTable(_TableName);
            mTable.LoadRow(this);
            return mTable;
        }
        #region ICloneable 成员

        public MDataRow Clone()
        {
            MDataRow row = new MDataRow();

            for (int i = 0; i < base.Count; i++)
            {
                CellStruct mcb = base[i]._CellStruct;
                MDataCell mdc = new MDataCell(ref mcb);
                mdc.Value = base[i].Value;
                mdc._CellValue.IsChange = false;
                row.Add(mdc);
            }

            row.TableName = this.TableName;
            return row;
        }

        #endregion

        #region IDataRecord 成员

        int IDataRecord.FieldCount
        {
            get
            {
                return base.Count;
            }
        }

        bool IDataRecord.GetBoolean(int i)
        {
            return (bool)this[i].Value;
        }

        byte IDataRecord.GetByte(int i)
        {
            return (byte)this[i].Value;
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        char IDataRecord.GetChar(int i)
        {
            return (char)this[i].Value;
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return (long)this[i].Value;
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            return "";
            //return this[i]._CellValue.ValueType.Name;
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            return (DateTime)this[i].Value;
        }

        decimal IDataRecord.GetDecimal(int i)
        {
            return (decimal)this[i].Value;
        }

        double IDataRecord.GetDouble(int i)
        {
            return (double)this[i].Value;
        }

        Type IDataRecord.GetFieldType(int i)
        {
            return this[i]._CellStruct.ValueType;
        }

        float IDataRecord.GetFloat(int i)
        {
            return (float)this[i].Value;
        }

        Guid IDataRecord.GetGuid(int i)
        {
            return (Guid)this[i].Value;
        }

        short IDataRecord.GetInt16(int i)
        {
            return (short)this[i].Value;
        }

        int IDataRecord.GetInt32(int i)
        {
            return (int)this[i].Value;
        }

        long IDataRecord.GetInt64(int i)
        {
            return (long)this[i].Value;
        }

        string IDataRecord.GetName(int i)
        {
            return (string)this[i].Value;
        }

        int IDataRecord.GetOrdinal(string name)
        {
            return (int)this[name].Value;
        }

        string IDataRecord.GetString(int i)
        {
            return (string)this[i].Value;
        }

        object IDataRecord.GetValue(int i)
        {
            return this[i].Value;
        }

        int IDataRecord.GetValues(object[] values)
        {
            return 0;
        }

        bool IDataRecord.IsDBNull(int i)
        {
            return this[i].Value == DBNull.Value;
        }

        object IDataRecord.this[string name]
        {

            get
            {
                return this[name].Value;
            }
        }

        object IDataRecord.this[int i]
        {
            get
            {
                return this[i].Value;
            }
        }

        #endregion



        #region ICustomTypeDescriptor 成员

        public System.ComponentModel.AttributeCollection GetAttributes()
        {
            return null;
        }

        public string GetClassName()
        {
            return "MDataRow";
        }

        public string GetComponentName()
        {
            return "by  路过秋天";
        }

        public TypeConverter GetConverter()
        {
            throw new Exception("The method or operation is not implemented..-- by 路过秋天");
        }

        public EventDescriptor GetDefaultEvent()
        {
            throw new Exception("The method or operation is not implemented..-- by 路过秋天");
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            throw new Exception("The method or operation is not implemented..-- by 路过秋天");
        }

        public object GetEditor(Type editorBaseType)
        {
            throw new Exception("The method or operation is not implemented..-- by 路过秋天");
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new Exception("The method or operation is not implemented..-- by 路过秋天");
        }

        public EventDescriptorCollection GetEvents()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        int index = 0;
        PropertyDescriptorCollection properties;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (index == 1)
            {
                return properties;
            }
            index++;
            properties = new PropertyDescriptorCollection(null);

            foreach (MDataCell mdc in this)
            {
                properties.Add(new MDataProperty(mdc, null));
            }
            return properties;

        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);

        }

        public object GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
