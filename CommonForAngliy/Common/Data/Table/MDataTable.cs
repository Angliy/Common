using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Common.Data.SQL;
using System.Reflection;


namespace Common.Data.Table
{
    /// <summary>
    /// 表格
    /// </summary>
    public class MDataTable : IDataReader, IEnumerable,System.ComponentModel.IListSource
    {
        public static implicit operator MDataTable(System.Data.Common.DbDataReader sdr)
        {
            MDataTable mTable = new MDataTable("default");
            if (sdr != null)
            {
                CellStruct mStruct;
                MDataRow mRecord = new MDataRow();
                int columnState = 0;
                while (sdr.Read())
                {
                    for (int i = 0; i < sdr.FieldCount; i++)
                    {
                        if (columnState == 0)
                        {
                            mStruct = new CellStruct(sdr.GetName(i), DataType.GetSqlType(sdr.GetFieldType(i)), false, true, -1, ParameterDirection.InputOutput);
                            MDataCell mdc = new MDataCell(ref mStruct, sdr.GetValue(i));
                            mRecord.Add(mdc);
                            mTable.Columns.Add(mStruct);
                        }
                        if (sdr[mRecord[i]._CellStruct.ColumnName] == null || sdr[mRecord[i]._CellStruct.ColumnName].ToString() == string.Empty)
                        {
                            mRecord[i].Value = DBNull.Value;
                        }
                        else
                        {
                            mRecord[i].Value = sdr[mRecord[i]._CellStruct.ColumnName]; //sdr.GetValue(i);
                        }
                    }
                    columnState++;
                    mTable.Rows.Add(mRecord.Clone());
                }
                mRecord.Clear();
                mRecord = null;
                sdr.Close();
                sdr.Dispose();
                sdr = null;
            }
            return mTable;

        }

        #region 属性
        private List<MDataRow> _Mdr;
        public List<MDataRow> Rows
        {
            get
            {
                return _Mdr;
            }
        }
        public MDataTable()
        {
            Init("路过秋天");
        }
        public MDataTable(string tableName)
        {
            Init(tableName);
        }
        private void Init(string tableName)
        {
            _Mdr = new List<MDataRow>();
            _TableName = tableName;
            if (_Columns == null)
            {
                _Columns = new MDataColumn(this);
            }
        }
        private string _TableName = string.Empty;
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

        private MDataColumn _Columns;
        public MDataColumn Columns
        {
            get
            {

                //if (_Mdr != null && _Mdr.Count > 0)
                //{
                //    _Columns = _Mdr[0].Columns;
                //}
                return _Columns;
            }
        }
        public MDataRow NewRow()
        {
            MDataRow mdr = new MDataRow();
            mdr.TableName = _TableName;
            CellStruct mdcStruct = null;
            for (int i = 0; i < _Columns.Count; i++)
            {
                mdcStruct = _Columns[i];
                mdr.Add(new MDataCell(ref mdcStruct));
            }
            return mdr;
        }
        #endregion

        #region 方法
        internal void LoadRow(MDataRow row)
        {
            if (this.Columns.Count == 0 && row != null && row.Count > 0)
            {
                foreach (MDataCell cell in row)
                {
                    this.Columns.Add(cell._CellStruct);
                }
                _TableName = row.TableName;
                if (row[0].Value != null && row[0].Value != DBNull.Value)
                {
                    _Mdr.Add(row);
                }
            }
        }
        public DataTable ToDataTable()
        {
            DataTable dt = new DataTable(_TableName);
            if (Columns != null && Columns.Count > 0)
            {
                foreach (CellStruct item in Columns)
                {
                    dt.Columns.Add(item.ColumnName);
                }
                foreach (MDataRow row in Rows)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < Columns.Count; i++)
                    {
                        dr[i] = row[i].Value;
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        public static MDataTable LoadFromJson(string json)
        {
            JsonHelper helper = new JsonHelper();
            return helper.Load(json);
        }
        public string ToJson()
        {
            JsonHelper helper = new JsonHelper();
            helper.Fill(this);
            return helper.ToString();
        }
        public void Bind(object control)
        {
            MBindUI.Bind(control, this);
        }
        public List<T> ToList<T>()
        {
            List<T> list = new List<T>();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (MDataRow row in Rows)
                {
                    T obj = (T)Activator.CreateInstance(typeof(T));
                    PropertyInfo[] pis =obj.GetType().GetProperties();
                    object propValue = null;
                    for (int i = 0; i < pis.Length; i++)
                    {
                        propValue = row[pis[i].Name].Value;
                        if (propValue == null || propValue==DBNull.Value)
                        {
                            continue;
                        }
                        pis[i].SetValue(obj, propValue, null);
                    }
                    list.Add(obj);
                }
            }
            return list;
        }
        ///// <summary>
        ///// 将更改的数据操作更新到数据库
        ///// </summary>
        //public void AcceptChanges()
        //{
        //    MAction action=new MAction(_TableName);
        //    foreach (MDataRow row in Rows)
        //    {
        //        action.SetRow(row);
        //        action.Update();
        //    }
        //    action.Close();
        //}
        #endregion

        private int _Ptr = 0;
        #region IDataReader 成员

        public void Close()
        {
            _Mdr.Clear();
        }

        public int Depth
        {
            get
            {
                if (_Mdr != null)
                {
                    return _Mdr.Count;
                }
                return 0;
            }
        }

        public DataTable GetSchemaTable()
        {
            return null;
        }

        public bool IsClosed
        {
            get
            {
                return true;
            }
        }

        public bool NextResult()
        {
            if (_Ptr < _Mdr.Count - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Read()
        {
            if (_Ptr < _Mdr.Count)
            {
                _Ptr++;
                return true;
            }
            else
            {
		        _Ptr = 0;
                return false;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return -1;
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            _Mdr.Clear();
            _Mdr = null;
        }

        #endregion

        #region IDataRecord 成员

        public int FieldCount
        {
            get
            {
                if (this.Columns != null)
                {
                    return this.Columns.Count;
                }
                return 0;
            }
        }

        public bool GetBoolean(int i)
        {
            return (bool)_Mdr[_Ptr][i].Value;
        }

        public byte GetByte(int i)
        {
            return (byte)_Mdr[_Ptr][i].Value;
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public char GetChar(int i)
        {
            return (char)_Mdr[_Ptr][i].Value;
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            return "";
            //return _Mdr[_Ptr][i]._CellValue.Value.GetType().Name;
            //return DataType.GetDbType(_Mdr[_Ptr][i]._CellStruct.SqlType.ToString()).ToString();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)_Mdr[_Ptr][i].Value;
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)_Mdr[_Ptr][i].Value;
        }

        public double GetDouble(int i)
        {
            return (double)_Mdr[_Ptr][i].Value;
        }

        public Type GetFieldType(int i)
        {
            return _Columns[i].ValueType;
            //return _Mdr[_Ptr][i]._CellStruct.ValueType;
        }

        public float GetFloat(int i)
        {
            return (float)_Mdr[_Ptr][i].Value;
        }

        public Guid GetGuid(int i)
        {
            return (Guid)_Mdr[_Ptr][i].Value;
        }

        public short GetInt16(int i)
        {
            return (short)_Mdr[_Ptr][i].Value;
        }

        public int GetInt32(int i)
        {
            return (int)_Mdr[_Ptr][i].Value;
        }

        public long GetInt64(int i)
        {
            return (long)_Mdr[_Ptr][i].Value;
        }

        public string GetName(int i)
        {
            return _Columns[i].ColumnName;
            //return _Mdr[_Ptr][i]._CellStruct.ColumnName;
        }

        public int GetOrdinal(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetString(int i)
        {
            return Convert.ToString(_Mdr[_Ptr][i].Value);
        }

        public object GetValue(int i)
        {
           return _Mdr[_Ptr][i].Value;
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
               values[i] = _Mdr[_Ptr - 1][i].Value;
            }
            return values.Length;
        }

        public bool IsDBNull(int i)
        {
            return _Mdr[_Ptr][i]._CellValue.IsNull;
        }

        public object this[string name]
        {
            get
            {
                return null;
            }
        }

        public object this[int i]
        {
            get
            {
                return _Mdr[i];
            }
        }

        #endregion

        #region IEnumerable 成员

        public IEnumerator GetEnumerator()
        {
            return new System.Data.Common.DbEnumerator(this);
        }

        #endregion

        #region IListSource 成员

        public bool ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        public IList GetList()
        {
            return Rows;
        }

        #endregion
    }
    
}
