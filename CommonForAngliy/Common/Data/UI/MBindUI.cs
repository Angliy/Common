using System;
using System.Web.UI.WebControls;
using System.Text;
using System.Web.UI;
using Win = System.Windows.Forms;
namespace Common.Data
{
    internal class MBindUI
    {
        public static void Bind(object ct,object source)
        {
            if (ct is GridView)
            {
                ((GridView)ct).DataSource = source;
                ((GridView)ct).DataBind();
            }
            else if (ct is Repeater)
            {
                ((Repeater)ct).DataSource = source;
                ((Repeater)ct).DataBind();
            }
            else if (ct is DataList)
            {
                ((DataList)ct).DataSource = source;
                ((DataList)ct).DataBind();
            }
            else if (ct is DataGrid)
            {
                ((DataGrid)ct).DataSource = source;
                ((DataGrid)ct).DataBind();
            }
            else if (ct is Win.DataGrid)
            {
                ((DataGrid)ct).DataSource = source;
            }
            else if (ct is Win.DataGridView)
            {
                ((System.Windows.Forms.DataGridView)ct).DataSource = source;
            }
        }
        public static void BindList(object ct, Common.Data.Table.MDataTable source)
        {
            if (ct is ListControl)
            {
                BindList(ct as ListControl, source);
            }
            else
            {
                BindList(ct as Win.ListControl, source);
            }
        }
        private static void BindList(Win.ListControl listControl, Common.Data.Table.MDataTable source)
        {
            listControl.DataSource = source;
            listControl.DisplayMember = source.Columns[0].ColumnName;
            listControl.ValueMember = source.Columns[1].ColumnName;
        }
        private static void BindList(ListControl listControl, Common.Data.Table.MDataTable source)
        {
            listControl.DataSource = source;
            listControl.DataTextField = source.Columns[0].ColumnName;
            listControl.DataValueField = source.Columns[1].ColumnName;
            listControl.DataBind();
        }
        public static string GetID(object ct)
        {
            if (ct is Control)
            {
                return ((Control)ct).ID;
            }
            else if (ct is Win.Control)
            {
                return ((Win.Control)ct).Name;
            }
            return "cyq";
        }
    }
}
