using System;
using Common.Data.Xml;
using System.Xml;
using System.Web;
namespace Common.Data.Xml
{
    public class MutilLanguage : IDisposable
    {

        XmlHelper helper;
        public string DefaultValue = "no find";
        private LanguageKey lanKey=LanguageKey.None;
        public LanguageKey LanKey
        {
            get
            {
                if (lanKey == LanguageKey.None)
                {
                    string key = AppConfig.SysLangKey;
                    if (string.IsNullOrEmpty(key))
                    {
                        lanKey = LanguageKey.China;
                    }
                    else
                    {
                        try
                        {
                            lanKey = (LanguageKey)Enum.Parse(typeof(LanguageKey), key);
                        }
                        catch
                        {
                            throw new Exception(string.Format("Error:LanguageKey not contain {0}", key));
                        }
                    }
                }
                return lanKey;
            }
        }
        public MutilLanguage(string filePath, bool forHtml, bool getValueByCookie)
        {
            Init(filePath, forHtml, getValueByCookie);
        }
        public MutilLanguage(string filePath, bool forHtml)
        {
            Init(filePath,forHtml,true);
        }
        public MutilLanguage(string filePath)
        {
            Init(filePath, true,true);
        }
        private void Init(string filePath, bool forHtml, bool getValueByCookie)
        {
            helper = new XmlHelper(forHtml);
            if (!helper.Load(filePath))
            {
                throw new Exception("加载语言文件失败:" + filePath);
            }
            if (getValueByCookie)
            {
                GetFromCookie();
            }
        }
        public string Get(object lanID)
        {
            return Get(lanID,LanKey);
        }
        public string Get(object lanID,LanguageKey lanEnum)
        {
            XmlNode node = helper.GetByID(Convert.ToString(lanID));
            if (node != null)
            {
                switch (lanEnum)
                {
                    case LanguageKey.China:
                        return node.InnerXml;
                    default:
                        string key = lanEnum.ToString().ToLower().Substring(0, 3);
                        if (node.Attributes[key] != null)
                        {
                            return node.Attributes[key].Value;
                        }
                        else
                        {
                            return node.InnerXml;
                        }
                }
            }
            return DefaultValue;
        }
        private void GetFromCookie()
        {
            HttpCookie myCookie = HttpContext.Current.Request.Cookies[AppConfig.Domain + "_LanKey"];
            if (null != myCookie)
            {
                try
                {
                    lanKey = (LanguageKey)Enum.Parse(typeof(LanguageKey), myCookie.Value);
                }
                catch
                {
                    lanKey = LanguageKey.None;
                }
            }
        }
        public void SetToCookie(LanguageKey lanKey)
        {
            SetToCookie(lanKey.ToString());
        }
        public void SetToCookie(string lanKey)
        {
            HttpCookie myCookie = new HttpCookie(AppConfig.Domain + "_LanKey", lanKey);
            if (AppConfig.Domain.IndexOf(':') == -1)//端口处理
            {
                myCookie.Domain = AppConfig.Domain;
            }
            myCookie.Expires = System.DateTime.Now.AddYears(1);
            HttpContext.Current.Response.Cookies.Add(myCookie);
        }
        #region IDisposable 成员

        public void Dispose()
        {
            helper.Dispose();
        }

        #endregion
    }
    public enum LanguageKey
    {
        /// <summary>
        /// 未设置状态
        /// </summary>
        None=0,
        /// <summary>
        /// 中文
        /// </summary>
        China=1,
        /// <summary>
        /// 英文
        /// </summary>
        English,
        /// <summary>
        /// 法语
        /// </summary>
        French,

        /// <summary>
        /// 德语
        /// </summary>
        German,

        /// <summary>
        /// 韩语
        /// </summary>
        Korean,

        /// <summary>
        /// 日语
        /// </summary>
        Japanese,

        /// <summary>
        /// 印地语
        /// </summary>
        Hindi,

        /// <summary>
        ///  俄语
        /// </summary>
        Russian,

        /// <summary>
        /// 意大利语
        /// </summary>
        Italian,
        /// <summary>
        /// 自定义语言
        /// </summary>
        Custom
    }
}
