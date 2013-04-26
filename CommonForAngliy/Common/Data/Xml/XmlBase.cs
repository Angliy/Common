using System;
using System.Xml;

namespace Common.Data.Xml
{
    public abstract class XmlBase : IDisposable
    {
        public XmlDocument xmlDoc;//xml����
        protected XmlNamespaceManager xnm;//�����ռ����
       
        protected string htmlNameSpace = "http://www.w3.org/1999/xhtml";
        internal string PreXml = "preXml";
        public string xmlFilePath = string.Empty;
        public string xmlCacheKey = string.Empty;
        private bool _ReadOnly;
        /// <summary>
        /// �Ƿ�ֻ����ֻ��ʱ����[��ȡ]����Clone
        /// </summary>
        public bool ReadOnly
        {
            get { return _ReadOnly; }
            set { _ReadOnly = value; }
        }
        /// <summary>
        /// �ĵ��Ƿ�ȡ�Ի���
        /// </summary>
        public bool DocIsCache
        {
            get
            {
                return _ReadOnly;
            }
        }
        /// <summary>
        /// Cache�����仯
        /// </summary>
        //public bool CacheIsChanged
        //{
        //    get
        //    {
        //        return theCache.GetHasChanged(xmlCacheKey);
        //    }
        //    set
        //    {
        //        theCache.SetChange(xmlCacheKey, value);
        //    }
        //}
        public string OutXml
        {
            get
            {
                if (xmlDoc != null)
                {
                    string xml = xmlDoc.InnerXml.Replace("xmlns=\"\"", string.Empty);
                    if (xnm != null)
                    {
                        xml = xml.Replace("xmlns=\"" + xnm.LookupNamespace(PreXml) + "\"", string.Empty);
                    }
                    return ClearCDATA(xml);
                }
                return string.Empty;
            }
        }
        public XmlBase()
        {
            xmlDoc = new XmlDocument();
           
        }
        protected void LoadNameSpace(string nameSpace)
        {
            xnm = new XmlNamespaceManager(xmlDoc.NameTable);
            xnm.AddNamespace(PreXml, nameSpace);
        }
        /// <summary>
        /// �Ӿ���·���л���ļ�����ΪKeyֵ
        /// </summary>
        private string GenerateKey(string absFilePath)
        {
            xmlFilePath = absFilePath;
            absFilePath = absFilePath.Replace(":", "").Replace("/", "").Replace("\\", "").Replace(".", "");
            return absFilePath;
        }

        #region ����xml
        public void LoadXml(string xml)
        {
            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (XmlException err)
            {
                throw new XmlException(err.Message);
            }
        }
        /// <summary>
        /// ����XML
        /// </summary>
        public bool Load(string absFilePath)
        {
            bool loadState = false;
            xmlCacheKey = GenerateKey(absFilePath);//��·���л���ļ�����Ϊkey
            //loadState = LoadFromCache(xmlCacheKey);//��Cache����Xml
            //if (!loadState)//Cache����Xmlʧ��
            //{
                loadState = LoadFromFile(absFilePath);//���ļ�����Xml
            //}
            return loadState;
        }

        //public bool LoadFromCache(string key)
        //{

        //    if (theCache.Contains(key))//�����д��ڶ�Ӧֵ��key�Ķ���
        //    {
        //        if (_ReadOnly)
        //        {
        //            xmlDoc = theCache.Get(key) as XmlDocument;
        //        }
        //        else
        //        {
        //            xmlDoc = GetCloneFrom(theCache.Get(key) as XmlDocument);
        //        }
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// ���ļ�����XML
        /// </summary>
        private bool LoadFromFile(string absFilePath)
        {
            if (!System.IO.File.Exists(absFilePath))
            {
                return false;
            }
            try
            {
                string html = string.Empty;
                if (xnm != null)
                {
                    if (AppConfig.UseFileLoadXml)
                    {
                        html = System.IO.File.ReadAllText(absFilePath, System.Text.Encoding.UTF8);
                        html = html.Replace("http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", AppConfig.DtdUri);
                    }
                    else
                    {
                        ResolverDtd.Resolver(ref xmlDoc);
                    }
                }
                //System.Web.HttpContext.Current.Response.Write(AppConfig.UseFileLoadXml.ToString());
                if (html != string.Empty)
                {
                    xmlDoc.LoadXml(html);//���ַ�������xml
                }
                else
                {
                    xmlDoc.Load(absFilePath);//���ļ�����xml
                }
                xmlCacheKey = GenerateKey(absFilePath);
                //if (!theCache.Contains(xmlCacheKey))
                //{
                //    SaveToCache(xmlCacheKey, ReadOnly);
                //}
                return true;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "path:" + absFilePath);
            }
        }
        #endregion
        #region ��������
        //public void SaveToCache(string key, bool readOnly)
        //{
        //    SaveToCache(key, readOnly, 0);
        //}
        //public void SaveToCache(string key, bool readOnly, int cacheTimeMinutes)
        //{
        //    if (xmlDoc != null)
        //    {
        //        if (readOnly)
        //        {
        //            theCache.Add(key, xmlDoc, xmlFilePath, cacheTimeMinutes);//���Cache����
        //        }
        //        else
        //        {
        //            theCache.Add(key, GetCloneFrom(xmlDoc), xmlFilePath, cacheTimeMinutes);//���Cache����Clone
        //        }
        //    }
        //}
        public void Save()
        {
            Save(xmlFilePath);
        }
        public void Save(string fileName)
        {
            if (xmlDoc != null)
            {
                if (AppConfig.UseFileLoadXml)
                {
                    System.IO.StreamWriter writer = System.IO.File.CreateText(fileName);
                    writer.Write(xmlDoc.InnerXml.Replace(AppConfig.DtdUri, "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"));
                    writer.Dispose();
                    return;
                    //XmlDocumentType docType = xmlDoc.CreateDocumentType("html", "-//W3C//DTD XHTML 1.0 Transitional//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", null);
                    //xmlDoc.RemoveChild(xmlDoc.ChildNodes[0]);
                    //xmlDoc.InsertBefore(docType, xmlDoc.ChildNodes[0]);
                    // xmlDoc.InnerXml=xmlDoc.InnerXml.Replace(AppConfig.DtdUri,"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd");
                }
                xmlDoc.Save(fileName);
            }
        }
        private XmlDocument GetCloneFrom(XmlDocument xDoc)
        {
            XmlDocument newDoc = new XmlDocument();
            if (xnm != null && !AppConfig.UseFileLoadXml)
            {
                ResolverDtd.Resolver(ref newDoc);
            }
            newDoc.LoadXml(xDoc.InnerXml);
            return newDoc;
        }
        protected XmlNode Fill(string xPath, XmlNode parent)
        {
            if (parent != null)
            {
                return parent.SelectSingleNode(xPath.Replace("//", "descendant::"), xnm);
            }
            return xmlDoc.SelectSingleNode(xPath, xnm);
        }
        protected XmlNodeList Select(string xPath, XmlNode parent)
        {
            if (parent != null)
            {
                return parent.SelectNodes(xPath.Replace("//", "descendant::"), xnm);
            }
            return xmlDoc.SelectNodes(xPath, xnm);
        }
        protected XmlElement Create(string tag)
        {
            if (xnm == null)
            {
                return xmlDoc.CreateElement(tag);
            }
            return xmlDoc.CreateElement(tag, xnm.LookupNamespace(PreXml));
        }
        protected string GetXPath(string tag, string attr, string value)
        {

            string xPath = "//" + (xnm != null ? PreXml + ":" : "") + tag; //+ "[@" + attr + "='" + value + "']";
            if (attr != null)
            {
                if (value != null)
                {
                    xPath += "[@" + attr + "='" + value + "']";
                }
                else
                {
                    xPath += "[@" + attr + "]";
                }
            }
            return xPath;
        }
        /// <summary>
        /// ��ָ�����ַ�����CDATA
        /// </summary>
        /// <param name="ObjectText">�����ַ�</param>
        /// <returns></returns>
        public string SetCDATA(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            text = text.Replace(AppConfig.CDataLeft, string.Empty).Replace(AppConfig.CDataRight, string.Empty);
            //text = text.Replace(((char)10).ToString(), "<BR>");
            //text = text.Replace(((char)13).ToString(), "<BR>");
            //text = text.Replace(((char)34).ToString(), "&quot;");
            //text = text.Replace(((char)39).ToString(), "&#39;");
            text = text.Replace("\\", "#!!#").Replace("\0", "#!0!#");
            return AppConfig.CDataLeft + text + AppConfig.CDataRight;
        }
        /// <summary>
        /// ���CDATA
        /// </summary>
        /// <param name="ObjectText">�����ַ�</param>
        /// <returns></returns>
        public string ClearCDATA(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            text = text.Replace("#!!#", "\\").Replace("#!0!#", "\\0");
            text = text.Replace(AppConfig.CDataLeft, string.Empty).Replace(AppConfig.CDataRight, string.Empty);
            return text;
        }
        public string ClearMMS(string text)
        {
            return text.Replace("MMS::", string.Empty).Replace("::MMS", string.Empty);
        }
        #endregion
        #region IDisposable ��Ա

        public virtual void Dispose()
        {
            if (xmlDoc != null)
            {
                if (!ReadOnly)
                {
                    xmlDoc.RemoveAll();
                }
                xmlDoc = null;
            }
        }

        #endregion

    }
}
