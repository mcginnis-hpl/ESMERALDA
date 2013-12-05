using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using ESMERALDAClasses;

namespace ESMERALDA
{
    public partial class PersonChooser : System.Web.UI.UserControl
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
                    dynamicChooser.Visible = false;
                    staticChooser.Visible = true;
                }
                else
                {
                    dynamicChooser.Visible = true;
                    staticChooser.Visible = false;
                }
            }
        }

        public void PopulateChooser(SqlConnection conn, EsmeraldaEntity inEntity)
        {
            if (listAvailableUsers.Items.Count <= 0)
            {
                List<Person> folks = inEntity.GetEligibleUsers(conn);
                foreach (Person p in folks)
                {
                    string label = p.GetMetadataValue("firstname") + " " + p.GetMetadataValue("lastname") + " (" + p.GetMetadataValue("cntorg") + ")";
                    string val = p.ID.ToString();
                    listAvailableUsers.Items.Add(new ListItem(label, val));
                }
            }
            listSelectedUsers.Items.Clear();
            while (staticChooser.Rows.Count > 1)
            {
                staticChooser.Rows.RemoveAt(staticChooser.Rows.Count - 1);
            }
            userValues.Value = string.Empty;
            relationshipValues.Value = string.Empty;

            if (inEntity.Relationships.Count > 0)
            {                
                foreach (PersonRelationship pr in inEntity.Relationships)
                {
                    bool found = false;
                    foreach (ListItem li in listAvailableUsers.Items)
                    {
                        if (li.Value == pr.person.ID.ToString())
                        {
                            found = true;
                            TableRow tr = new TableRow();
                            TableCell td = new TableCell();
                            td.Text = li.Text;
                            tr.Cells.Add(td);
                            td = new TableCell();
                            td.Text = pr.relationship;
                            tr.Cells.Add(td);
                            staticChooser.Rows.Add(tr);

                            ListItem li2 = new ListItem(li.Text + ": " + pr.relationship, li.Value);                            
                            listSelectedUsers.Items.Add(li2);
                            if (string.IsNullOrEmpty(userValues.Value))
                            {
                                userValues.Value = li.Value;
                                relationshipValues.Value = pr.relationship;
                            }
                            else
                            {
                                userValues.Value = userValues.Value + "|" + li.Value;
                                relationshipValues.Value = relationshipValues.Value + "|" + pr.relationship;
                            }
                            break;
                        }
                    }
                    if (!found)
                    {
                        string label = pr.GetMetadataValue("firstname") + " " + pr.GetMetadataValue("lastname") + " (" + pr.GetMetadataValue("cntorg") + ")";
                        string val = pr.ID.ToString();

                        TableRow tr = new TableRow();
                        TableCell td = new TableCell();
                        td.Text = label;
                        tr.Cells.Add(td);
                        td = new TableCell();
                        td.Text = pr.relationship;
                        tr.Cells.Add(td);
                        staticChooser.Rows.Add(tr);

                        ListItem li2 = new ListItem(label + ": " + pr.relationship, val);
                        listSelectedUsers.Items.Add(li2);
                        if (string.IsNullOrEmpty(userValues.Value))
                        {
                            userValues.Value = li2.Value;
                            relationshipValues.Value = pr.relationship;
                        }
                        else
                        {
                            userValues.Value = userValues.Value + "|" + li2.Value;
                            relationshipValues.Value = relationshipValues.Value + "|" + pr.relationship;
                        }
                    }
                }
            }
        }
        
        public void GetSelectedItems(List<Guid> outids, List<string> outrelationships)
        {
            string[] ids = userValues.Value.Split("|".ToCharArray());
            string[] rels = relationshipValues.Value.Split("|".ToCharArray());

            for (int i = 0; i < ids.Length; i++)
            {
                if (!string.IsNullOrEmpty(ids[i]))
                {
                    outids.Add(new Guid(ids[i]));
                    outrelationships.Add(rels[i]);
                }
            }
        }
    }
}