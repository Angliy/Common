using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Common.Data.Table;
using System.Data;


namespace Common.Data
{
    internal class JsonHelper
    {
        /// <summary>
        /// 是否成功   
        /// </summary>
        public bool Success
        {
            get
            {
                return count > 0;
            }
        }
        private string errorMsg = "";
        /// <summary>
        /// 错误提示信息   
        /// </summary>
        public string ErrorMsg
        {
            get
            {
                return errorMsg;
            }
            set
            {
                errorMsg = value;
            }
        }
        private int count = 0;
        /// <summary>
        /// 总记 
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }
        private List<string> arrData = new List<string>();

        #region 对象与对象之间分割符
        public void addItemOk()
        {
            arrData.Add("<br>");
        }
        #endregion

        #region 在数组里添加key,value
        public void addItem(string name, string value)
        {
            arrData.Add("\"" + name + "\":" + "\"" + value + "\"");
        }
        #endregion

        #region 返回组装好的json字符串
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"count\":\"" + count + "\",");
            sb.Append("\"error\":\"" + errorMsg + "\",");
            sb.Append("\"success\":\"" + (Success ? "true" : "") + "\",");
            sb.Append("\"data\":[");

            int index = 0;
            sb.Append("{");
            if (arrData.Count <= 0)
            {
                sb.Append("}]");
            }
            else
            {
                foreach (string val in arrData)
                {
                    index++;

                    if (val != "<br>")
                    {
                        sb.Append(val + ",");
                    }
                    else
                    {
                        sb = sb.Replace(",", "", sb.Length - 1, 1);
                        sb.Append("},");
                        if (index < arrData.Count)
                        {
                            sb.Append("{");
                        }
                    }

                }
                sb = sb.Replace(",", "", sb.Length - 1, 1);
                sb.Append("]");
            }

            sb.Append("}");
            return sb.ToString();

        }
        #endregion

        #region 为DataTable增加处理
        public void Fill(MDataTable table)
        {
            if (table == null)
            {
                ErrorMsg = "查询对象为Null";
                return;
            }
            Count = table.Rows.Count;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    addItem(table.Columns[j].ColumnName, Convert.ToString(table.Rows[i][j].Value));
                }
                addItemOk();
            }
        }
        public MDataTable Load(string json)
        {
            MDataTable table = new MDataTable("loadFromJson");
            if (!string.IsNullOrEmpty(json) && json.Length > 30 && json.StartsWith("{") && json.IndexOf(',') > -1 && json.EndsWith("}"))
            {
                try
                {
                    int start=json.IndexOf(":[{") + 2;
                    string data = json.Substring(start, json.LastIndexOf("]}") - start);
                    data = data.Replace("\\}", "#100#").Replace("\\,", "#101#").Replace("\\:,", "#102#");
                    bool isOK=false;
                    if (!string.IsNullOrEmpty(data))
                    {
                        string[] items = data.Replace("{",string.Empty).Split('}');//分隔每一行
                        string item = string.Empty, key = string.Empty, value = string.Empty;
                        for (int i = 0; i < items.Length; i++)//循环每一行数据
                        {
                            item = items[i].Replace("#100#", "\\}").Trim(',');
                            if (string.IsNullOrEmpty(item))
                            {
                                continue;
                            }
                            string[] keyValues = item.Split(',');

                            string keyValue = string.Empty;
                            if (i == 0)
                            {
                                for (int j = 0; j < keyValues.Length; j++)
                                {
                                    keyValue = keyValues[j].Replace("#101#", "\\,");
                                    key = keyValue.Split(':')[0].Trim('\'', '\"');
                                    table.Columns.Add(key, SqlDbType.NVarChar);
                                }
                                isOK=true;
                               
                            }
                            if (isOK)
                            {
                                MDataRow row = table.NewRow();
                                for (int k = 0; k < keyValues.Length; k++)
                                {
                                    keyValue = keyValues[k].Replace("#101#", "\\,");
                                    if (keyValue.IndexOf(':') > -1)
                                    {
                                        value = keyValue.Substring(keyValue.IndexOf(':')+1).Replace("#102#", "\\:").Trim('\'', '\"');
                                        row[k].Value = value;
                                    }
                                }
                                table.Rows.Add(row);
                            }

                        }
                    }
                }
                catch
                {
                    return table;
                }
            }
            return table;
        }
        #endregion
    }
}
