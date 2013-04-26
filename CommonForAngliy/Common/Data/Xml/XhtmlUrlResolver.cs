using System;

namespace Common.Data.Xml
{
    internal class ResolverDtd
    {
        public static void Resolver(ref System.Xml.XmlDocument xmlDoc)
        {
            xmlDoc.XmlResolver = new Xml.XhtmlUrlResolver();
        }
    }
    internal class XhtmlUrlResolver : System.Xml.XmlUrlResolver
    {
        private string dtdUri = null;

        public string DtdUri
        {
            get
            {
                if (dtdUri == null)
                {
                    dtdUri = AppConfig.DtdUri;
                }
                return dtdUri;
            }
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (relativeUri.Contains("xhtml1-transitional.dtd") && DtdUri != null)
            {
                relativeUri = DtdUri;
            }
            //relativeUri = relativeUri.Replace(".dtd", ".xml").Replace(".ent", ".xml");//转成xml，iis不需要设置mime类型
            return base.ResolveUri(baseUri, relativeUri);
        }
    }
}
