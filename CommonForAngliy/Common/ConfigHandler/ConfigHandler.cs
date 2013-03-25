using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace Common.ConfigHandler
{
    /// <summary>
    /// //自定义Config中节点 
    /// </summary>
    public abstract class ConfigHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler 
        public virtual object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            return section;
        }
        #endregion

        protected string GetConfigValue(System.Xml.XmlNode section, string xPath)
        {
            string result = "";
            XmlNode node = section.SelectSingleNode(xPath);
            result = node.InnerText;
            return result;
        }
    }
}
