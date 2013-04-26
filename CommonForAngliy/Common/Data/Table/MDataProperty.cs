using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Table
{
    internal class MDataProperty : System.ComponentModel.PropertyDescriptor
    {
        private MDataCell cell = null;
        public MDataProperty(MDataCell mdc, Attribute[] attrs)
            : base(mdc._CellStruct.ColumnName, attrs)
        {
            cell = mdc;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(MDataCell);
            }
        }
        public override object GetValue(object component)
        {
            return ((MDataRow)component)[cell._CellStruct.ColumnName].Value;
           
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get { return cell._CellStruct.ValueType; }
        }

        public override void ResetValue(object component)
        {

        }

        public override void SetValue(object component, object value)
        {
            cell.Value = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }
        //public override string Description
        //{
        //    get
        //    {
        //        return cell._CellStruct.ColumnName;
        //    }
        //}

        //public override string Category
        //{
        //    get
        //    {
        //        return _customProperty.Category;
        //    }
        //}

        //public override string DisplayName
        //{
        //    get
        //    {
        //        return cell._CellStruct.ColumnName;
        //    }
        //}
       
        public override bool IsBrowsable
        {
            get
            {
                return true;
            }
        }
    }
}
