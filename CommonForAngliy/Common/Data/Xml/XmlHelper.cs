using System;
using System.Xml;
using Common.Data.Table;

namespace Common.Data.Xml
{
    public class XmlHelper : XmlBase
    {
        public XmlHelper()
            : base()
        {

        }
        public XmlHelper(bool forHtml)
            : base()
        {
            if (forHtml)
            {
                base.LoadNameSpace(htmlNameSpace);
            }
        }
        public XmlHelper(string nameSpaceUrl)
            : base()
        {
            base.LoadNameSpace(nameSpaceUrl);
        }
        #region 操作数据
        MDataRow _Row;
        MDataTable _Table;



        #region 加载表格循环方式
        public void LoadData(MDataTable table)
        {
            _Table = table;
            if (_Table.Rows.Count > 0)
            {
                _Row = _Table.Rows[0];
            }
        }
        public delegate string SetForeachEventHandler(string text, object[] values, int row);
        public event SetForeachEventHandler OnForeach;
        public void SetForeach(string id, SetType setType, params object[] formatValues)
        {
            string text = string.Empty;
            XmlNode node = GetByID(id);
            if (node == null)
            {
                return;
            }
            switch (setType)
            {
                case SetType.InnerText:
                    text = node.InnerText;
                    break;
                case SetType.InnerXml:
                    text = node.InnerXml;
                    break;
                case SetType.Value:
                case SetType.Href:
                case SetType.Src:
                case SetType.Class:
                    string key = setType.ToString().ToLower();
                    if (node.Attributes[key] != null)
                    {
                        text = node.Attributes[key].Value;
                    }
                    break;
            }
            SetForeach(node, text, formatValues);
        }
        public void SetForeach(string id, string text, params object[] formatValues)
        {
            XmlNode node = GetByID(id);
            SetForeach(node, text, formatValues);
        }
        private void SetForeach(XmlNode node, string text, params object[] formatValues)
        {
            if (node != null && _Table != null && _Table.Rows.Count > 0)
            {
                string innerXml = "";
                object[] values = new object[formatValues.Length];
                //foreach (MDataRow row in _Table.Rows)
                string newText = text;
                MDataCell cell;
                for (int k = 0; k < _Table.Rows.Count; k++)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        cell = _Table.Rows[k][formatValues[i].ToString()];
                        if (cell == null && formatValues[i].ToString().ToLower() == "row")
                        {
                            values[i] = k + 1;
                        }
                        else
                        {
                            values[i] = cell.Value;
                        }
                    }
                    if (OnForeach != null)
                    {
                        newText = OnForeach(text, values, k);
                    }
                    innerXml += string.Format(newText, values);
                }
                try
                {
                    node.InnerXml = innerXml;
                }
                catch
                {
                    node.InnerXml = SetCDATA(innerXml);
                }
            }
            if (OnForeach != null)
            {
                OnForeach = null;
            }
        }
        #endregion

        #region 加载行数据后操作方式

        private bool _IsUserLang = true;
        /// <summary>
        /// 当前请求是否用户的语言
        /// </summary>
        public bool IsUserLang
        {
            get
            {
                return _IsUserLang;
            }
            set
            {
                _IsUserLang = value;
            }
        }
        public bool IsOpenUserLang = true;//是否开始自定义语言分隔
        public void LoadData(MDataRow row)
        {
            _Row = row;
        }
        public void SetFor(string id)
        {
            SetFor(id, SetType.InnerXml);
        }
        public void SetFor(string id, SetType setType)
        {
            SetFor(id, setType, GetRowValue(id));
        }
        public void SetFor(string id, SetType setType, params string[] values)
        {
            int i = setType == SetType.Custom ? 1 : 0;
            for (; i < values.Length; i++)
            {
                if (values[i].Contains(ValueReplace.New))
                {
                    values[i] = values[i].Replace(ValueReplace.New, GetRowValue(id));
                }
            }
            XmlNode node = GetByID(id);

            Set(node, setType, values);
        }
        private string GetRowValue(string id)
        {
            string rowValue = "";
            if (_Row != null)
            {
                MDataCell cell = _Row[id.Substring(3)];
                if (cell == null)
                {
                    cell = _Row[id];
                }
                if (cell != null)
                {
                    rowValue = Convert.ToString(cell.Value);
                }
            }
            return rowValue;
        }
        #endregion
        private string SetValue(string sourceValue, string newValue, bool addCData)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return sourceValue;
            }
            newValue = newValue.Replace(ValueReplace.Source, sourceValue);
            if (IsOpenUserLang)
            {
                int split = newValue.IndexOf(ValueReplace.LangSplit);
                if (split > -1)
                {
                    newValue = _IsUserLang ? newValue.Substring(0, split) : newValue.Substring(split + ValueReplace.LangSplit.Length);
                }
            }
            if (addCData)
            {
                newValue = SetCDATA(newValue);
            }
            return newValue;
        }
        private void SetAttrValue(XmlNode node, string key, string value)
        {
            if (node.Attributes[key] == null)
            {
                XmlAttribute attr = xmlDoc.CreateAttribute(key);
                node.Attributes.Append(attr);
            }
            node.Attributes[key].Value = SetValue(node.Attributes[key].InnerXml, value, false);
        }

        public void Set(XmlNode node, SetType setType, params string[] values)
        {

            if (node != null)
            {
                switch (setType)
                {
                    case SetType.InnerText:
                        node.InnerText = SetValue(node.InnerText, values[0], false);
                        break;
                    case SetType.InnerXml:
                        node.InnerXml = SetValue(node.InnerXml, values[0], true);
                        break;
                    case SetType.Value:
                    case SetType.Href:
                    case SetType.Src:
                    case SetType.Class:
                    case SetType.Disabled:
                    case SetType.ID:
                    case SetType.Name:
                        string key = setType.ToString().ToLower();
                        SetAttrValue(node, key, values[0]);
                        break;
                    case SetType.Custom:
                        SetAttrValue(node, values[0], values[1]);
                        break;
                    case SetType.A:
                        node.InnerXml = SetValue(node.InnerXml, values[0], true);
                        if (values.Length > 1)
                        {
                            SetAttrValue(node, "href", values[1]);
                        }
                        break;
                    case SetType.Select:
                        foreach (XmlNode option in node.ChildNodes)
                        {
                            if (option.Attributes["value"] != null && option.Attributes["value"].Value == values[0])
                            {
                                SetAttrValue(option, "selected", "selected");
                                break;
                            }
                        }
                        break;
                    case SetType.Checked:
                        if (values[0] == "1" || values[0].ToLower() == "true")
                        {
                            key = setType.ToString().ToLower();
                            SetAttrValue(node, key, key);
                        }
                        break;
                }
            }
        }
        public void Set(string id, SetType setType, params string[] values)
        {
            XmlNode node = GetByID(id);
            Set(node, setType, values);
        }
        public void Set(string id, string value)
        {
            XmlNode node = GetByID(id);
            Set(node, SetType.InnerXml, value);
        }
        #endregion
        #region 查询
        public XmlNode GetByName(string name)
        {
            return Fill(GetXPath("*", "name", name), null);
        }
        public XmlNode GetByName(string name, XmlNode node)
        {
            return Fill(GetXPath("*", "name", name), node);
        }
        public XmlNode GetByID(string id)
        {
            return Fill(GetXPath("*", "id", id), null);
        }
        public XmlNode GetByID(string id, XmlNode node)
        {
            return Fill(GetXPath("*", "id", id), node);
        }
        public XmlNode Get(string tag, string attr, string value, XmlNode parent)
        {
            return Fill(GetXPath(tag, attr, value), parent);
        }



        public XmlNodeList GetList(string tag, string attr, string value)
        {
            return Select(GetXPath(tag, attr, value), null);
        }
        public XmlNodeList GetList(string tag, string attr, string value, XmlNode node)
        {
            return Select(GetXPath(tag, attr, value), node);
        }
        public XmlNodeList GetList(string tag, string attr)
        {
            return Select(GetXPath(tag, attr, null), null);
        }
        public XmlNodeList GetList(string tag, string attr, XmlNode node)
        {
            return Select(GetXPath(tag, attr, null), node);
        }
        public XmlNodeList GetList(string tag)
        {
            return Select(GetXPath(tag, null, null), null);
        }
        public XmlNodeList GetList(string tag, XmlNode node)
        {
            return Select(GetXPath(tag, null, null), node);
        }
        #endregion

        #region 创建
        public XmlNode CreateNode(string tag, string text, params string[] attrAndValue)
        {
            XmlElement xElement = Create(tag);
            try
            {
                xElement.InnerXml = text;
            }
            catch
            {
                xElement.InnerXml = SetCDATA(text);
            }
            if (attrAndValue != null && attrAndValue.Length % 2 == 0)
            {
                string attr = "", value = "";
                for (int i = 0; i < attrAndValue.Length; i++)
                {
                    attr = attrAndValue[i];
                    i++;
                    value = attrAndValue[i];
                    xElement.SetAttribute(attr, value);
                }
            }
            return xElement as XmlNode;
        }
        #endregion

        #region 附加
        public void AppendNode(XmlNode parentNode, XmlNode childNode)
        {
            if (parentNode != null && childNode != null)
            {
                parentNode.AppendChild(childNode);
            }
        }
        public void AppendNode(XmlNode parentNode, XmlNode childNode, int position)
        {
            if (parentNode != null && childNode != null)// A B
            {
                if (parentNode.ChildNodes.Count == 0 || position >= parentNode.ChildNodes.Count)
                {
                    parentNode.AppendChild(childNode);
                }
                else if (position == 0)
                {
                    InsertBefore(childNode, parentNode.ChildNodes[0]);
                }
                else
                {
                    InsertAfter(childNode, parentNode.ChildNodes[position - 1]);
                }
            }
        }

        #endregion

        #region 删除
        /// <summary>
        /// 保留节点,但清除节点所内容/属性
        /// </summary>
        /// <param name="OldNode"></param>
        public void Clear(XmlNode node)
        {
            node.RemoveAll();
        }
        public void Remove(XmlNode node)
        {
            node.ParentNode.RemoveChild(node);
        }
        public void Remove(string id)
        {
            XmlNode node = GetByID(id);
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
            }
        }
        public void RemoveChild(string id, int index)
        {
            XmlNode node = GetByID(id);
            if (node != null)
            {
                RemoveChild(node, index);
            }
        }
        public void RemoveChild(XmlNode node, int index)
        {
            if (node.ChildNodes.Count >= index)
            {
                node.RemoveChild(node.ChildNodes[index - 1]);
            }
        }
        #endregion

        #region 其它交换节点/插入节点

        /// <summary>
        /// 两个节点交换位置
        /// </summary>
        /// <param name="XNodeFirst">第一个节点</param>
        /// <param name="XNodeLast">第二个节点</param>
        public void InterChange(XmlNode xNodeFirst, XmlNode xNodeLast)
        {
            if (xNodeFirst != null && xNodeLast != null)
            {
                if (xNodeFirst.ParentNode != null && xNodeLast.ParentNode != null)
                {
                    xNodeFirst.ParentNode.ReplaceChild(xNodeLast.Clone(), xNodeFirst);
                    xNodeLast.ParentNode.ReplaceChild(xNodeFirst.Clone(), xNodeLast);
                }
                else
                {
                    xmlDoc.DocumentElement.ReplaceChild(xNodeLast.Clone(), xNodeFirst);
                    xmlDoc.DocumentElement.ReplaceChild(xNodeFirst.Clone(), xNodeLast);
                }
            }
        }
        /// <summary>
        /// 节点替换[支持两个的文档间替换]
        /// </summary>
        /// <param name="NewXNode"></param>
        /// <param name="OldXNode"></param>
        public void ReplaceNode(XmlNode newNode, XmlNode oldNode)
        {
            if (newNode != null && oldNode != null)
            {
                oldNode.RemoveAll();
                //if (!string.IsNullOrEmpty(newNode.InnerXml) && !string.IsNullOrEmpty(newNode.NamespaceURI))
                //{
                //    oldNode.InnerXml = newNode.InnerXml.Replace("xmlns=\"" + newNode.NamespaceURI+"\"", string.Empty);
                //}
                //else
                //{
                oldNode.InnerXml = newNode.InnerXml;
                //}
                XmlAttributeCollection xAttributes = newNode.Attributes;
                if (xAttributes != null && xAttributes.Count > 0)
                {
                    for (int i = 0; i < xAttributes.Count; i++)
                    {
                        ((XmlElement)oldNode).SetAttribute(xAttributes[i].Name, xAttributes[i].Value);
                    }
                }
            }
        }
        /// <summary>
        /// 节点之后插入[支持两文档之间的插入]
        /// </summary>
        /// <param name="NewNode">要被插入的新节点</param>
        /// <param name="RefNode">在此节点后插入NewNode节点</param>
        public void InsertAfter(XmlNode newNode, XmlNode refNode)
        {
            XmlNode xDocNode = CreateNode(newNode.Name, "");
            ReplaceNode(newNode, xDocNode);
            refNode.ParentNode.InsertAfter(xDocNode, refNode);
        }
        /// <summary>
        /// 节点之前插入[支持两文档之间的插入]
        /// </summary>
        /// <param name="NewNode">要被插入的新节点</param>
        /// <param name="RefNode">在此节点前插入NewNode节点</param>
        public void InsertBefore(XmlNode newNode, XmlNode refNode)
        {
            XmlNode xDocNode = CreateNode(newNode.Name, "");
            ReplaceNode(newNode, xDocNode);
            refNode.ParentNode.InsertBefore(xDocNode, refNode);
        }
        #endregion

        #region 属性判断/取值
        public bool HasAttr(string nodeID, string attrName)
        {
            return GetAttrValue(nodeID, attrName) != string.Empty;
        }
        public bool HasAttr(XmlNode node, string attrName)
        {
            return GetAttrValue(node, attrName) != string.Empty;
        }
        public string GetAttrValue(string nodeID, string attrName)
        {
            XmlNode node = GetByID(nodeID);
            return GetAttrValue(node, attrName);
        }
        public string GetAttrValue(XmlNode node, string attrName)
        {
            if (node != null && node.Attributes != null && node.Attributes[attrName] != null)
            {
                return node.Attributes[attrName].Value;
            }
            return string.Empty;
        }
        public void RemoveAttr(string nodeID, params string[] attrNames)
        {
            XmlNode node = GetByID(nodeID);
            RemoveAttr(node, attrNames);
        }
        public void RemoveAttr(XmlNode node, params string[] attrNames)
        {
            if (node != null && node.Attributes != null)
            {
                foreach (string name in attrNames)
                {
                    if (node.Attributes[name] != null)
                    {
                        node.Attributes.Remove(node.Attributes[name]);
                    }
                }

            }
        }

        #endregion
    }

}
