using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESMERALDAClasses;

namespace ESMERALDA
{
    public partial class MetadataControl : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public bool ReadOnly
        {
            set
            {
                if (value)
                {
                    controls.Visible = false;
                    listTags.Enabled = false;
                    listValues.Enabled = false;
                }
                else
                {
                    controls.Visible = true;
                    listTags.Enabled = true;
                    listValues.Enabled = true;
                }
            }
        }

        public void PopulateMetadata(EsmeraldaEntity inEntity)
        {
            listTags.Items.Clear();
            listValues.Items.Clear();

            foreach (string s in inEntity.Metadata.Keys)
            {
                ListItem li = new ListItem();
                li.Value = s;
                li.Text = s;
                listTags.Items.Add(li);

                li = new ListItem();
                List<string> vals = inEntity.GetMetadataValueArray(s);
                if (vals.Count == 0)
                {
                    li.Value = vals[0];
                    li.Text = vals[0];
                }
                else
                {
                    string final_val = vals[0];
                    for (int i = 1; i < vals.Count; i++)
                        final_val = final_val + "\\" + vals[i];
                    li.Value = final_val;
                    li.Text = final_val;
                }
                listValues.Items.Add(li);
            }            
        }

        public void GetSelectedItems(EsmeraldaEntity inEntity)
        {
            char[] delim = { '\\' };
            string tag = string.Empty;
            string[] val = null;
            inEntity.Metadata.Clear();
            for (int i = 0; i < listTags.Items.Count; i++)
            {
                tag = listTags.Items[i].Value;
                val = listValues.Items[i].Value.Split(delim);
                inEntity.SetMetadataValueArray(tag, val);
            }
        }
    }
}