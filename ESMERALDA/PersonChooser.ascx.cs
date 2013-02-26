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
                    available.Visible = false;
                    controls.Visible = false;
                    listSelectedUsers.Enabled = false;
                }
                else
                {
                    available.Visible = true;
                    controls.Visible = true;
                    listSelectedUsers.Enabled = true;
                }
            }
        }

        public void PopulateChooser(SqlConnection conn, EsmeraldaEntity inEntity)
        {
            if (listAvailableUsers.Items.Count <= 0)
            {
                SqlCommand query = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_ESMERALDA_GetPersonList",
                    CommandTimeout = 60,
                    Connection = conn
                };
                SqlDataReader reader = query.ExecuteReader();
                Guid personid = Guid.Empty;
                while (reader.Read())
                {
                    string label = reader["first_name"].ToString() + " " + reader["last_name"].ToString() + " (" + reader["affiliation"].ToString() + ")";
                    personid = new Guid(reader["personid"].ToString());
                    string val = personid.ToString();
                    listAvailableUsers.Items.Add(new ListItem(label, val));                    
                }
                reader.Close();
            }
            listSelectedUsers.Items.Clear();
            userValues.Value = string.Empty;
            relationshipValues.Value = string.Empty;

            if (inEntity.Relationships.Count > 0)
            {                
                foreach (PersonRelationship pr in inEntity.Relationships)
                {
                    foreach (ListItem li in listAvailableUsers.Items)
                    {
                        if (li.Value == pr.person.ID.ToString())
                        {
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