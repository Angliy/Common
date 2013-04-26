using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Table
{
    /// <summary>
    /// 头列表集合
    /// </summary>
    public class MDataColumn : List<CellStruct>
    {
        private MDataTable _Table;
        public MDataColumn()
            : base()
        {
        }
        internal MDataColumn(MDataTable table)
        {
            _Table = table;
        }
        public MDataColumn Clone()
        {
            MDataColumn mcs = new MDataColumn();

            for (int i = 0; i < base.Count; i++)
            {
                CellStruct mcb = base[i];
                mcs.Add(mcb);
            }
            return mcs;
        }
        public bool Contains(string columnName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if(this[i].ColumnName==columnName)
                {
                    return true;
                }
            }
            return false;
        }
        public void Add(string columnName)
        {
            Add(columnName, System.Data.SqlDbType.NVarChar);
        }
        public void Add(string columnName, System.Data.SqlDbType SqlType)
        {
            CellStruct mdcStruct = new CellStruct(columnName, SqlType, false, true, 0, System.Data.ParameterDirection.Input);
            this.Add(mdcStruct);
        }
        public void Remove(string columnName)
        {
            int index = -1;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].ColumnName == columnName)
                {
                    index = i;
                    break;
                }
            }
            if (index >-1)
            {
                RemoveAt(index);
            }
        }
        public new void RemoveAt(int index)
        {
            if (_Table != null)
            {
                foreach (MDataRow row in _Table.Rows)
                {
                    row.RemoveAt(index);
                }
            }
            base.RemoveAt(index);
        }
    }
}
