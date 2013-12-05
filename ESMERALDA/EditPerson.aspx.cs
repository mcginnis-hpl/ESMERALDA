using ESMERALDAClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace ESMERALDA
{
    public partial class EditPerson : ESMERALDAPage
    {
        protected void btnLogon_Click(object sender, EventArgs e)
        {
            this.Login();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Person inperson = (Person)GetSessionValue("WorkingPerson");
            try
            {

                if (!base.IsAuthenticated)
                {
                    base.ShowAlert("You must be logged in to change user data.");
                    return;
                }
                if (this.txtPasswordNew.Text != this.txtPasswordConfirm.Text)
                {
                    base.ShowAlert("Passwords do not match.");
                    return;
                }
                /*if (base.GetUserID(conn) == inperson.ID && string.IsNullOrEmpty(this.txtPasswordNew.Text))
                {
                    base.ShowAlert("Your password can not be empty.");
                    return;
                }
                if (string.IsNullOrEmpty(this.txtEmail.Text))
                {
                    base.ShowAlert("Your email address can not be empty.");
                    return;
                }*/
                inperson.SetMetadataValue("firstname", this.txtFirstName.Text);
                inperson.SetMetadataValue("lastname", this.txtLastName.Text);
                inperson.SetMetadataValue("address", this.txtAddress1.Text + (string.IsNullOrEmpty(txtAddress2.Text) ? "\n" + txtAddress2.Text : string.Empty));
                inperson.SetMetadataValue("cntorg", this.txtAffiliation.Text);
                inperson.SetMetadataValue("city", this.txtCity.Text);
                inperson.SetMetadataValue("cntinst", this.txtComments.Text);
                inperson.SetMetadataValue("country", this.txtCountry.Text);
                inperson.SetMetadataValue("cntemail", this.txtEmail.Text);
                inperson.SetMetadataValue("cntfax", this.txtFax.Text);
                inperson.SetMetadataValue("honorific", this.txtHonorific.Text);
                inperson.SetMetadataValue("cntvoice", this.txtPhone.Text);
                inperson.SetMetadataValue("state", this.txtState.Text);
                inperson.SetMetadataValue("postal", this.txtZIP.Text);
                if (!string.IsNullOrEmpty(this.lblUserID.Text))
                {
                    inperson.ID = new Guid(this.lblUserID.Text);
                }
                if (inperson.ID == Guid.Empty)
                {
                    inperson.ID = Guid.NewGuid();
                }
                if (!(!(base.GetUserID(conn) != inperson.ID) || base.UserIsAdministrator))
                {
                    base.ShowAlert("You may only edit your own user information if you are not an administrator.");
                }
                else
                {
                    inperson.Save(conn);
                    if (!string.IsNullOrEmpty(txtPasswordNew.Text))
                    {
                        SqlCommand cmd = new SqlCommand("sp_ESMERALDA_RegisterUser", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 100).Value = this.txtEmail.Text;
                        SqlParameter sqlParam = cmd.Parameters.Add("@passwordHash", SqlDbType.VarChar, 50);
                        int saltSize = 5;
                        string salt = ESMERALDAPage.CreateSalt(saltSize);
                        string passwordHash = ESMERALDAPage.CreatePasswordHash(this.txtPasswordNew.Text, salt);
                        sqlParam.Value = passwordHash;
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            base.ShowAlert("Error creating user.  User may already exist.");
                        }
                    }
                    this.PopulateData(inperson, conn);
                    conn.Close();
                }
            }
            catch(Exception ex)
            {
                ShowAlert(ex.Message + ": " + ex.StackTrace);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        protected void Login()
        {
            Person p = (Person)GetSessionValue("WorkingPerson");
            if (p == null)
            {
                p = new Person();
            }
            bool passwordMatch = false;
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            SqlCommand cmd = new SqlCommand("sp_ESMERALDA_LookupUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@userName", SqlDbType.VarChar, 100).Value = this.txtUsername.Text;
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    string dbPasswordHash = reader.GetString(0);
                    int saltSize = 5;
                    string fakeSaltString = ESMERALDAPage.CreateSalt(saltSize);
                    string salt = dbPasswordHash.Substring(dbPasswordHash.Length - fakeSaltString.Length);
                    reader.Close();
                    passwordMatch = ESMERALDAPage.CreatePasswordHash(this.txtPassword.Text, salt).Equals(dbPasswordHash);
                }
                if (passwordMatch)
                {

                    FormsAuthenticationTicket tkt = new FormsAuthenticationTicket(1, this.txtUsername.Text, DateTime.Now, DateTime.Now.AddYears(1), this.chkPersistCookie.Checked, "");
                    string cookiestr = FormsAuthentication.Encrypt(tkt);
                    HttpCookie ck = new HttpCookie(FormsAuthentication.FormsCookieName, cookiestr);
                    if (this.chkPersistCookie.Checked)
                    {
                        ck.Expires = tkt.Expiration;
                    }
                    ck.Path = FormsAuthentication.FormsCookiePath;
                    base.Response.Cookies.Add(ck);
                    p.LoadByUsername(conn, this.txtUsername.Text);
                    if (p != null)
                    {
                        this.PopulateData(p, conn);
                    }
                    base.CurrentUser = p;
                    this.login.Visible = false;
                    base.ShowAlert("Login successful!");
                    persondata.Visible = true;
                    PopulateData(p, conn);
                }
                else
                {
                    base.ShowAlert("Login incorrect.  Please try again, or create a new user account.");
                    persondata.Visible = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Execption verifying password. " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool isLogin = false;
            string username = string.Empty;
            Guid userid = Guid.Empty;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "USERNAME")
                {
                    username = base.Request.Params[i];
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "USERID")
                {
                    userid = new Guid(base.Request.Params[i]);
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "MODE")
                {
                    if (base.Request.Params[i].ToUpper() == "LOGIN")
                    {
                        isLogin = true;
                    }
                }
            }
            if (username == string.Empty && userid == Guid.Empty)
            {
                username = Username;
            }
            if (isLogin && !base.IsAuthenticated)
            {
                this.login.Visible = true;
                btnLogout.Visible = false;
            }
            else
            {
                this.login.Visible = false;
                btnLogout.Visible = true;
            }
            if (!IsAuthenticated)
            {
                persondata.Visible = false;
            }
            else
            {
                persondata.Visible = true;
            }
            if (!UserIsAdministrator)
            {
                btnNewUser.Visible = false;
            }
            else
            {
                btnNewUser.Visible = true;
            }
            if (!IsPostBack)
            {
                RemoveSessionValue("WorkingPerson");
                Person p = new Person();
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                if (!string.IsNullOrEmpty(username) || userid != Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(username))
                    {
                        p.LoadByUsername(conn, username);
                    }
                    else
                    {
                        p.Load(conn, userid);
                    }
                }
                this.PopulateData(p, conn);
                conn.Close();
                SetSessionValue("WorkingPerson", p);
            }
        }

        protected void PopulateData(Person inperson, SqlConnection conn)
        {
            this.txtFirstName.Text = inperson.GetMetadataValue("firstname");
            this.txtLastName.Text = inperson.GetMetadataValue("lastname");
            string[] tokens = inperson.GetMetadataValue("address").Split("\n".ToCharArray());
            this.txtAddress1.Text = tokens[0];
            if (tokens.Length > 1)
                this.txtAddress2.Text = tokens[1];
            else
                this.txtAddress2.Text = string.Empty;
            this.txtAffiliation.Text = inperson.GetMetadataValue("cntorg");
            this.txtCity.Text = inperson.GetMetadataValue("city");
            this.txtComments.Text = inperson.GetMetadataValue("cntinst");
            this.txtCountry.Text = inperson.GetMetadataValue("country");
            this.txtEmail.Text = inperson.GetMetadataValue("cntemail");
            this.txtFax.Text = inperson.GetMetadataValue("cntfax");
            this.txtHonorific.Text = inperson.GetMetadataValue("honorific");
            this.txtPhone.Text = inperson.GetMetadataValue("cntvoice");
            this.txtState.Text = inperson.GetMetadataValue("state");
            this.txtZIP.Text = inperson.GetMetadataValue("postal");
            this.lblUserID.Text = inperson.ID.ToString();
            if (base.Username == inperson.GetMetadataValue("cntemail"))
            {
                SqlCommand cmd = new SqlCommand("sp_ESMERALDA_LookupUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@userName", SqlDbType.VarChar, 100).Value = base.Username;
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    string dbPasswordHash = reader.GetString(0);
                    int saltSize = 5;
                    string pass = dbPasswordHash.Substring(saltSize, dbPasswordHash.Length - saltSize);
                    this.txtPasswordNew.Text = pass;
                    this.txtPasswordConfirm.Text = pass;
                    reader.Close();
                }
                catch (Exception)
                {
                }
                this.txtPasswordNew.Enabled = true;
                this.txtPasswordConfirm.Enabled = true;
                this.txtEmail.ReadOnly = false;
                this.txtFirstName.ReadOnly = false;
                this.txtLastName.ReadOnly = false;
                this.txtAddress1.ReadOnly = false;
                this.txtAddress2.ReadOnly = false;
                this.txtAffiliation.ReadOnly = false;
                this.txtCity.ReadOnly = false;
                this.txtComments.ReadOnly = false;
                this.txtCountry.ReadOnly = false;
                this.txtFax.ReadOnly = false;
                this.txtHonorific.ReadOnly = false;
                this.txtPhone.ReadOnly = false;
                this.txtState.ReadOnly = false;
                this.txtZIP.ReadOnly = false;
            }
            else if (IsAuthenticated)
            {
                this.txtPasswordNew.Enabled = true;
                this.txtPasswordConfirm.Enabled = true;
                this.txtEmail.ReadOnly = false;
                this.txtFirstName.ReadOnly = false;
                this.txtLastName.ReadOnly = false;
                this.txtAddress1.ReadOnly = false;
                this.txtAddress2.ReadOnly = false;
                this.txtAffiliation.ReadOnly = false;
                this.txtCity.ReadOnly = false;
                this.txtComments.ReadOnly = false;
                this.txtCountry.ReadOnly = false;
                this.txtFax.ReadOnly = false;
                this.txtHonorific.ReadOnly = false;
                this.txtPhone.ReadOnly = false;
                this.txtState.ReadOnly = false;
                this.txtZIP.ReadOnly = false;
            }
            else
            {
                this.txtPasswordNew.Enabled = false;
                this.txtPasswordConfirm.Enabled = false;
                this.txtEmail.ReadOnly = true;
                this.txtFirstName.ReadOnly = true;
                this.txtLastName.ReadOnly = true;
                this.txtAddress1.ReadOnly = true;
                this.txtAddress2.ReadOnly = true;
                this.txtAffiliation.ReadOnly = true;
                this.txtCity.ReadOnly = true;
                this.txtComments.ReadOnly = true;
                this.txtCountry.ReadOnly = true;
                this.txtFax.ReadOnly = true;
                this.txtHonorific.ReadOnly = true;
                this.txtPhone.ReadOnly = true;
                this.txtState.ReadOnly = true;
                this.txtZIP.ReadOnly = true;
                this.txtPasswordNew.Enabled = true;
                this.txtPasswordConfirm.Enabled = true;
            }
            if (IsAuthenticated && UserIsAdministrator)
            {
                comboUserList.Items.Clear();
                comboUserList.Items.Add(new ListItem(string.Empty, string.Empty));
                SqlCommand cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_ESMERALDA_GetPersonList"
                };
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("personid")))
                    {
                        ListItem li = new ListItem(reader["first_name"].ToString() + " " + reader["last_name"].ToString() + " (" + reader["affiliation"].ToString() + ")", reader["personid"].ToString());
                        comboUserList.Items.Add(li);
                    }
                }
                reader.Close();
            }
        }

        protected void txtPassword_TextChanged(object sender, EventArgs e)
        {
            this.Login();
        }

        protected void btnNewUser_Click(object sender, EventArgs e)
        {
            Person p = new Person();
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulateData(p, conn);
            conn.Close();
            SetSessionValue("WorkingPerson", p);
        }

        protected void comboUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Guid personid = Guid.Empty;
            Person p = new Person();
            if (!string.IsNullOrEmpty(comboUserList.SelectedValue))
            {
                if (comboUserList.SelectedIndex >= 0)
                {
                    personid = new Guid(comboUserList.SelectedValue);
                }
                if (personid != Guid.Empty)
                {                    
                    p.Load(conn, personid);                    
                    SetSessionValue("WorkingPerson", p);
                }
            }
            this.PopulateData(p, conn);
            conn.Close();
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Logout();
            this.login.Visible = true;
            persondata.Visible = false;
            btnNewUser.Visible = false;
            btnLogout.Visible = false;
            RemoveSessionValue("WorkingPerson");
            Person p = new Person();
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulateData(p, conn);
            conn.Close();
            SetSessionValue("WorkingPerson", p);
        }
    }
}