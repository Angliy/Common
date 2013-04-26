using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Common.Data.Table;
namespace Common.Data.Xml
{
    //public class RssDemo
    //{
    //    public string GetRss()
    //    {
    //        Rss2 rss = new Rss2();
    //        rss.channel.Title = "秋色园";
    //        rss.channel.Link = "http://www.cyqdata.com";
    //        rss.channel.Description = "秋色园-QBlog-Power by Blog.CYQ";
    //        for (int i = 0; i < 10; i++)
    //        {
    //            RssItem item = new RssItem();
    //            item.Title = string.Format("第{0}项", i);
    //            item.Link = "http://www.cyqdata.com";
    //            item.Description = "很长很长的内容";
    //            rss.channel.Items.Add(item);
    //        }
    //        return rss.OutXml;
    //    }
    //}
    //public class Rss2
    //{
    //    XmlDocument rssDoc;
    //    public RssChannel channel;
    //    public Rss2()
    //    {
    //        rssDoc = new XmlDocument();
    //        rssDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel></channel></rss>");
    //        channel = new RssChannel();
    //    }
    //    private void BuildRss()
    //    {
    //        XmlNode cNode = rssDoc.DocumentElement.ChildNodes[0];//取得channel元素
    //        ForeachCreateChild(cNode, channel);//Channel处理
    //        if (channel.RssImage != null)
    //        {
    //            ForeachCreateChild(Create("image", null, cNode), channel.RssImage);//Channel-Image处理
    //        }
    //        if (channel.Items.Count > 0)
    //        {
    //            foreach (RssItem item in channel.Items)
    //            {
    //                ForeachCreateChild(Create("item", null, cNode), item);//Channel-Items处理
    //            }
    //        }
    //    }
    //    private void ForeachCreateChild(XmlNode parent, object obj)
    //    {
    //        object propValue = null;
    //        PropertyInfo[] pis = obj.GetType().GetProperties();
    //        for (int i = 0; i < pis.Length; i++)
    //        {
    //            if (pis[i].Name == "Items" || pis[i].Name == "Image")
    //            {
    //                continue;
    //            }
    //            propValue = pis[i].GetValue(obj, null);
    //            if (propValue == null || propValue == DBNull.Value)
    //            {
    //                continue;
    //            }
    //            if (pis[i].Name == "Description")
    //            {
    //                propValue = "<![CDATA[" + propValue.ToString() + "]]>";
    //            }
    //            Create(pis[i].Name.Substring(0, 1).ToLower() + pis[i].Name.Substring(1), propValue.ToString(), parent);
    //        }
    //    }
    //    private XmlNode Create(string name, string value,XmlNode parent)
    //    {
    //        XmlElement xNode = rssDoc.CreateElement(name);
    //        if (string.IsNullOrEmpty(value))
    //        {
    //            xNode.InnerXml = value;
    //        }
    //        parent.AppendChild(xNode as XmlNode);
    //        return xNode as XmlNode;
    //    }
    //    public string OutXml
    //    {
    //        get
    //        {
    //            BuildRss();
    //            return rssDoc.OuterXml;
    //        }
    //    }
    //}
    public class Rss
    {
        internal class RssItemMap
        {
            internal string RssItemName;
            internal object[] TableColumnNames;
            internal string FormatText;
        }
        MDataTable _MTable = null;
        List<RssItemMap> mapList = new List<RssItemMap>();//与MDataTable映射
        public RssChannel channel;
        RssImage img;
        XmlHelper rssDoc;
        public Rss()
        {
            rssDoc = new XmlHelper(false);
            rssDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel></channel></rss>");
            channel = new RssChannel();
        }
        public void Set(string title, string link, string description)
        {
            channel.Title = title;
            channel.Link = link;
            channel.Description = description;
        }
        public void SetImg(string url, string title, string link)
        {
            if (img == null)
            {
                img = new RssImage();
                img.Url = url;
                img.Title = title;
                img.Link = link;
            }
        }
        public void AddItem(string title, string link, string author, string pubDate, string description)
        {
            RssItem item = new RssItem();
            item.Title = title;
            item.Link = link;
            item.Author = author;
            item.PubDate = pubDate;
            item.Description = description;
            channel.Items.Add(item);
        }

        private void BuildRss()
        {
            object propValue = null;

            XmlNode cNode = rssDoc.xmlDoc.DocumentElement.ChildNodes[0];
            CreateNode(cNode, channel);//Channel处理

            XmlNode iNode = null;
            if (img != null)
            {
                iNode = rssDoc.CreateNode("image", string.Empty);
                cNode.AppendChild(iNode);
                CreateNode(iNode, img);//Channel-Image处理
            }
            if (channel.Items.Count > 0)
            {
                foreach (RssItem item in channel.Items)
                {
                    iNode = rssDoc.CreateNode("item", string.Empty);
                    cNode.AppendChild(iNode);
                    CreateNode(iNode, item);//Channel-Items处理
                }
            }
            else if (_MTable != null && mapList.Count > 0)
            {
                foreach (MDataRow row in _MTable.Rows)
                {
                    iNode = rssDoc.CreateNode("item", string.Empty);
                    cNode.AppendChild(iNode);
                    foreach (RssItemMap item in mapList)
                    {
                        if (item.TableColumnNames.Length > 1)
                        {
                            object[] values = new object[item.TableColumnNames.Length];
                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = row[item.TableColumnNames[i].ToString()].Value;
                            }
                            propValue = string.Format(item.FormatText, values);
                        }
                        else
                        {
                            propValue = row[item.TableColumnNames[0].ToString()].Value;
                        }
                        if (propValue == null || propValue == DBNull.Value)
                        {
                            continue;
                        }
                        if (item.RssItemName == "Description")
                        {
                            propValue = rssDoc.SetCDATA(propValue.ToString());
                        }
                        iNode.AppendChild(rssDoc.CreateNode(item.RssItemName.Substring(0, 1).ToLower() + item.RssItemName.Substring(1), propValue.ToString()));
                    }
                }
            }
        }
        private void CreateNode(XmlNode parent, object obj)
        {
            object propValue = null;
            PropertyInfo[] pis = obj.GetType().GetProperties();
            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i].Name == "Items")
                {
                    continue;
                }
                propValue = pis[i].GetValue(obj, null);
                if (propValue == null || propValue == DBNull.Value)
                {
                    continue;
                }
                if (pis[i].Name == "Description")
                {
                    propValue = rssDoc.SetCDATA(propValue.ToString());
                }
                parent.AppendChild(rssDoc.CreateNode(pis[i].Name.Substring(0, 1).ToLower() + pis[i].Name.Substring(1), propValue.ToString()));
            }
        }
        public string OutXml
        {
            get
            {
                BuildRss();
                return rssDoc.ClearMMS(rssDoc.xmlDoc.OuterXml);
            }
        }

        #region 与MDataTable交互
        public void LoadData(MDataTable table)
        {
            _MTable = table;
        }
        public void SetMap(RssItemName itemName, string formatText, params object[] tableColumnNames)
        {
            RssItemMap map = new RssItemMap();
            map.RssItemName = itemName.ToString();
            map.FormatText = formatText;
            map.TableColumnNames = tableColumnNames;
            mapList.Add(map);
        }
        #endregion
    }
    public class RssChannel
    {
        #region 必选
        private string _Title;
        /// <summary>
        /// 定义频道的标题
        /// </summary>
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }
        private string _Link;
        /// <summary>
        /// 定义指向频道的超链接
        /// </summary>
        public string Link
        {
            get
            {
                return _Link;
            }
            set
            {
                _Link = value;
            }
        }
        private string _Description;
        /// <summary>
        /// 描述频道
        /// </summary>
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }
        #endregion

        #region 可选
        private string _Category;
        /// <summary>
        /// 为 feed 定义所属的一个或多个种类
        /// </summary>
        public string Category
        {
            get
            {
                return _Category;
            }
            set
            {
                _Category = value;
            }
        }
        private string _Cloud;
        /// <summary>
        /// 注册进程，以获得 feed 更新的立即通知
        /// </summary>
        public string Cloud
        {
            get
            {
                return _Cloud;
            }
            set
            {
                _Cloud = value;
            }
        }
        private string _Copyright;
        /// <summary>
        /// 告知版权资料
        /// </summary>
        public string Copyright
        {
            get
            {
                return _Copyright;
            }
            set
            {
                _Copyright = value;
            }
        }
        private string _Docs;
        /// <summary>
        /// 规定指向当前 RSS 文件所用格式说明的 URL
        /// </summary>
        public string Docs
        {
            get
            {
                return _Docs;
            }
            set
            {
                _Docs = value;
            }
        }
        private string _Generator;
        /// <summary>
        /// 规定指向当前 RSS 文件所用格式说明的 URL
        /// </summary>
        public string Generator
        {
            get
            {
                return _Generator;
            }
            set
            {
                _Generator = value;
            }
        }
        private string _Language = "zh-cn";
        /// <summary>
        /// 规定编写 feed 所用的语言
        /// </summary>
        public string Language
        {
            get
            {
                return _Language;
            }
            set
            {
                _Language = value;
            }
        }

        private string _LastBuildDate;
        /// <summary>
        /// 定义 feed 内容的最后修改日期
        /// </summary>
        public string LastBuildDate
        {
            get
            {
                return _LastBuildDate;
            }
            set
            {
                _LastBuildDate = value;
            }
        }
        private string _ManagingEditor;
        /// <summary>
        /// 定义 feed 内容编辑的电子邮件地址
        /// </summary>
        public string ManagingEditor
        {
            get
            {
                return _ManagingEditor;
            }
            set
            {
                _ManagingEditor = value;
            }
        }
        private string _PubDate;
        /// <summary>
        /// 为 feed 的内容定义最后发布日期
        /// </summary>
        public string PubDate
        {
            get
            {
                return _PubDate;
            }
            set
            {
                _PubDate = value;
            }
        }
        private string _Rating;
        /// <summary>
        /// feed 的 PICS 级别
        /// </summary>
        public string Rating
        {
            get
            {
                return _Rating;
            }
            set
            {
                _Rating = value;
            }
        }
        private string _SkipDays;
        /// <summary>
        /// 规定忽略 feed 更新的天
        /// </summary>
        public string SkipDays
        {
            get
            {
                return _SkipDays;
            }
            set
            {
                _SkipDays = value;
            }
        }
        private string _SkipHours;
        /// <summary>
        /// 规定忽略 feed 更新的小时
        /// </summary>
        public string SkipHours
        {
            get
            {
                return _SkipHours;
            }
            set
            {
                _SkipHours = value;
            }
        }
        private string _TextInput;
        /// <summary>
        /// 规定应当与 feed 一同显示的文本输入域
        /// </summary>
        public string TextInput
        {
            get
            {
                return _TextInput;
            }
            set
            {
                _TextInput = value;
            }
        }
        private string _Ttl;
        /// <summary>
        /// 指定从 feed 源更新此 feed 之前，feed 可被缓存的分钟数
        /// </summary>
        public string Ttl
        {
            get
            {
                return _Ttl;
            }
            set
            {
                _Ttl = value;
            }
        }
        private string _WebMaster;
        /// <summary>
        /// 定义此 feed 的 web 管理员的电子邮件地址
        /// </summary>
        public string WebMaster
        {
            get
            {
                return _WebMaster;
            }
            set
            {
                _WebMaster = value;
            }
        }
        private string _RssImage;
        /// <summary>
        /// 定义此 feed 的 图片Logo
        /// </summary>
        public string RssImage
        {
            get
            {
                return _RssImage;
            }
            set
            {
                _RssImage = value;
            }
        }
        #endregion
        private List<RssItem> _Items = new List<RssItem>();
        public List<RssItem> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                _Items = value;
            }
        }
    }
    public class RssImage
    {
        #region 必选
        private string _Url;
        /// <summary>
        /// 图片地址
        /// </summary>
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = value;
            }
        }
        private string _Title;
        /// <summary>
        /// 图片标题
        /// </summary>
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }
        private string _Link;
        /// <summary>
        /// 提供图片的站点链接
        /// </summary>
        public string Link
        {
            get
            {
                return _Link;
            }
            set
            {
                _Link = value;
            }
        }
        private string _Description;
        /// <summary>
        /// 描述频道
        /// </summary>
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }
        #endregion
    }
    public class RssItem
    {
        #region 必选
        private string _Title;
        /// <summary>
        /// 定义频道的标题
        /// </summary>
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }
        private string _Link;
        /// <summary>
        /// 定义指向频道的超链接
        /// </summary>
        public string Link
        {
            get
            {
                return _Link;
            }
            set
            {
                _Link = value;
            }
        }
        private string _Description;
        /// <summary>
        /// 描述频道
        /// </summary>
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }
        #endregion

        #region 可选
        private string _Category;
        /// <summary>
        /// 为 feed 定义所属的一个或多个种类
        /// </summary>
        public string Category
        {
            get
            {
                return _Category;
            }
            set
            {
                _Category = value;
            }
        }
        private string _Author;
        /// <summary>
        /// 规定项目作者的电子邮件地址
        /// </summary>
        public string Author
        {
            get
            {
                return _Author;
            }
            set
            {
                _Author = value;
            }
        }
        private string _Comments;
        /// <summary>
        /// 允许项目连接到有关此项目的注释（文件）
        /// </summary>
        public string Comments
        {
            get
            {
                return _Comments;
            }
            set
            {
                _Comments = value;
            }
        }
        private string _Enclosure;
        /// <summary>
        /// 允许将一个媒体文件导入一个项中
        /// </summary>
        public string Enclosure
        {
            get
            {
                return _Enclosure;
            }
            set
            {
                _Enclosure = value;
            }
        }
        private string _Guid;
        /// <summary>
        /// 为 项目定义一个唯一的标识符
        /// </summary>
        public string Guid
        {
            get
            {
                return _Guid;
            }
            set
            {
                _Guid = value;
            }
        }
        private string _PubDate;
        /// <summary>
        ///定义此项目的最后发布日期
        /// </summary>
        public string PubDate
        {
            get
            {
                return _PubDate;
            }
            set
            {
                _PubDate = value;
            }
        }
        private string _Source;
        /// <summary>
        /// 为此项目指定一个第三方来源
        /// </summary>
        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
            }
        }
        #endregion
    }
    public enum RssItemName
    {
        Title,
        Link,
        Description,
        Category,
        Author,
        Comments,
        Enclosure,
        Guid,
        PubDate,
        Source
    }
}
