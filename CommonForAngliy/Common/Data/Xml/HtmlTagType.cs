

namespace Common.Data.Xml
{
    public enum HtmlTag
    {
        div = 1,
        a = 2,
        li = 3,
        img = 4,
        br = 5,
        ul = 6,
        h1 = 7,
        span = 8,
        p = 9
    }
    public enum SetType
    {
        InnerXml,
        InnerText,
        Value,
        Href,
        Src,
        Class,
        A,
        Img,
        Input,
        Select,
        Checked,
        Disabled,
        ID,
        Name,
        Custom,
    }
    public class ValueReplace
    {
        public const string Source = "[#source]";
        public const string New = "[#new]";
        public const string LangSplit = "[#langsplit]";
    }
    
}
