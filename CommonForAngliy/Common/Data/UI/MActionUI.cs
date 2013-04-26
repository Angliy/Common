using System;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using Win = System.Windows.Forms;
using Common.Data.Table;
using System.Collections.Generic;
using Common.Data.SQL;
using System.Data;
using System.ComponentModel;
namespace Common.Data
{
    internal class MActionUI:IDisposable
    {
        private List<string> autoPrefixList;//调用插入和更新,自动获取控件名的前缀
        public MDataRow _Row;
        public MActionUI(ref MDataRow row)
        {
            _Row = row;
        }

        #region UI操作分路
        public void Set(object ct, object value, bool isControlEnabled)
        {
            if (ct is Control)
            {
                SetTo(ct as Control, value, isControlEnabled);
            }
            else
            {
                SetTo(ct as Win.Control, value, isControlEnabled);
            }
        }
        public void Get(object ct, object value)
        {
            if (ct is Control)
            {
                GetFrom(ct as Control, value);
            }
            else
            {
                GetFrom(ct as Win.Control, value);
            }
        }
        #endregion

        #region WebUI操作
        public void SetTo(Control ct, object value, bool isControlEnabled)
        {
            string propName = ct.ID.Substring(3);
            if (value == null)
            {
                value = _Row[propName].Value;
            }
            switch (ct.GetType().Name)
            {
                case "TextBox":
                    ((TextBox)ct).Text = Convert.ToString(value);
                    ((TextBox)ct).Enabled = isControlEnabled;
                    break;
                case "Literal":
                    ((Literal)ct).Text = Convert.ToString(value);
                    break;
                case "Label":
                    ((Label)ct).Text = Convert.ToString(value);
                    break;
                case "HiddenField":
                    ((HiddenField)ct).Value = Convert.ToString(value);
                    break;
                case "DropDownList":
                    ((DropDownList)ct).SelectedValue = Convert.ToString(value);
                    ((DropDownList)ct).Enabled = isControlEnabled;
                    break;
                case "CheckBox":
                    bool tempValue;
                    if (Convert.ToString(value) == "1")
                    {
                        tempValue = true;
                    }
                    else
                    {
                        bool.TryParse(Convert.ToString(value), out tempValue);
                    }
                    ((CheckBox)ct).Checked = tempValue;
                    ((CheckBox)ct).Enabled = isControlEnabled;
                    break;
            }

        }
        public void GetFrom(Control ct, object value)
        {
            string propName = ct.ID.Substring(3);
            if (value == null)
            {
                switch (ct.GetType().Name)
                {
                    case "TextBox":
                        value = ((TextBox)ct).Text.Trim();
                        break;
                    case "Literal":
                        value = ((Literal)ct).Text;
                        break;
                    case "Label":
                        value = ((Label)ct).Text;
                        break;
                    case "HiddenField":
                        value = ((HiddenField)ct).Value;
                        break;
                    case "DropDownList":
                        value = ((DropDownList)ct).SelectedValue;
                        break;
                    case "CheckBox":
                        value = ((CheckBox)ct).Checked;
                        break;
                }
            }
            _Row[propName].Value = value;
        }
        #endregion

        #region WinUI操作
        public void SetTo(Win.Control ct, object value, bool isControlEnabled)
        {
            string propName = ct.Name.Substring(3);
            if (value == null)
            {
                value = _Row[propName].Value;
            }
            switch (ct.GetType().Name)
            {
                case "TextBox":
                    ((Win.TextBox)ct).Text = Convert.ToString(value);
                    ((Win.TextBox)ct).Enabled = isControlEnabled;
                    break;
                case "ComboBox":
                    ((Win.ComboBox)ct).Items.Add(value);
                    break;
                case "Label":
                    ((Win.Label)ct).Text = Convert.ToString(value);
                    break;
                case "DateTimePicker":
                    DateTime dt;
                    if (DateTime.TryParse(Convert.ToString(value), out dt))
                    {
                        ((Win.DateTimePicker)ct).Value = dt;
                    }
                    break;
                case "ListBox":
                    ((Win.ListBox)ct).Items.Add(value);
                    break;
                case "CheckBox":
                    bool tempValue;
                    if (Convert.ToString(value) == "1")
                    {
                        tempValue = true;
                    }
                    else
                    {
                        bool.TryParse(Convert.ToString(value), out tempValue);
                    }
                    ((Win.CheckBox)ct).Checked = tempValue;
                    ((Win.CheckBox)ct).Enabled = isControlEnabled;
                    break;
                case "NumericUpDown":
                    decimal result = 0;
                    if (decimal.TryParse(Convert.ToString(value), out result))
                    {
                        ((Win.NumericUpDown)ct).Value = result;
                    }
                    break;
                case "RichTextBox":
                    ((Win.ListBox)ct).Text = Convert.ToString(value);
                    break;
            }

        }
        public void GetFrom(Win.Control ct, object value)
        {
            string propName = ct.Name.Substring(3);
            if (value == null)
            {
                switch (ct.GetType().Name)
                {
                    case "TextBox":
                        value = ((Win.TextBox)ct).Text.Trim();
                        break;
                    case "ComboBox":
                        value = ((Win.ComboBox)ct).Text;
                        break;
                    case "Label":
                        value = ((Win.Label)ct).Text;
                        break;
                    case "DateTimePicker":
                        value = ((Win.DateTimePicker)ct).Value;
                        break;
                    case "ListBox":
                        value = ((Win.ListBox)ct).Text;
                        break;
                    case "CheckBox":
                        value = ((Win.CheckBox)ct).Checked;
                        break;
                    case "NumericUpDown":
                        value = ((Win.NumericUpDown)ct).Value;
                        break;
                    case "RichTextBox":
                        value = ((Win.RichTextBox)ct).Text;
                        break;
                }
            }
            _Row[propName].Value = value;
        }
        #endregion

        #region Web自动取值
        /// <summary>
        /// 自动设置列的值(true为插入,false为更新)
        /// </summary>
        public void AutoSetColumnValue(bool containsID)
        {
            // Type type = null;
            int i = 0;
            if (containsID || !_Row[0]._CellValue.IsNull)
            {
                i = 1;
            }
            for (; i < _Row.Count; i++)
            {
                if (!_Row[i]._CellValue.IsChange)
                {

                    try
                    {
                        foreach (string autoPrefix in autoPrefixList)
                        {
                            string RequestValue = System.Web.HttpContext.Current.Request[autoPrefix + _Row[i]._CellStruct.ColumnName];
                            if (RequestValue != null)
                            {
                                if (RequestValue == "on")
                                {
                                    if (_Row[i]._CellStruct.SqlType == SqlDbType.Bit)
                                    {
                                        _Row[i].Value = true;
                                    }
                                    else
                                    {
                                        _Row[i].Value = 1;
                                    }
                                    break;
                                }
                                if (RequestValue.Length == 0 && DataType.GetGroupID(_Row[i]._CellStruct.SqlType) == 1)
                                {
                                    _Row[i].Value = 0;
                                    break;
                                }
                                else if (_Row[i]._CellStruct.SqlType == SqlDbType.Bit && RequestValue.Length==1)
                                {
                                    _Row[i].Value = RequestValue == "1";
                                }
                                _Row[i].Value = TypeDescriptor.GetConverter(_Row[i]._CellStruct.ValueType).ConvertFrom(RequestValue.Trim());
                                break;
                            }
                        }
                    }
                    catch
                    {
                      
                    }
                }
            }
        }
        #endregion

        #region 其它方法
        public void SetAutoPrefix(string autoPrefix, params string[] otherPrefix)
        {
            autoPrefixList = new List<string>();
            autoPrefixList.Add(autoPrefix);
            foreach (string item in otherPrefix)
            {
                autoPrefixList.Add(item);
            }
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (autoPrefixList != null)
            {
                autoPrefixList.Clear();
                autoPrefixList = null;
            }
        }

        #endregion
    }
}
