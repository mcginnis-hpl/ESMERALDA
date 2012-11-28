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
            if (!base.IsAuthenticated)
            {
                base.ShowAlert("You must be logged in to change user data.");
            }
            else if (this.txtPasswordNew.Text != this.txtPasswordConfirm.Text)
            {
                base.ShowAlert("Passwords do not match.");
            }
            else if (string.IsNullOrEmpty(this.txtPasswordNew.Text))
            {
                base.ShowAlert("Your password can not be empty.");
            }
            else if (string.IsNullOrEmpty(this.txtEmail.Text))
            {
                base.ShowAlert("Your email address can not be empty.");
            }
            else
            {
                Person inperson = (Person)GetSessionValue("WorkingPerson");
                inperson.first_name = this.txtFirstName.Text;
                inperson.middle_name = this.txtMiddleName.Text;
                inperson.last_name = this.txtLastName.Text;
                inperson.address_line1 = this.txtAddress1.Text;
                inperson.address_line2 = this.txtAddress2.Text;
                inperson.affiliation = this.txtAffiliation.Text;
                inperson.city = this.txtCity.Text;
                inperson.comment = this.txtComments.Text;
                inperson.country = this.txtCountry.Text;
                inperson.email = this.txtEmail.Text;
                inperson.fax = this.txtFax.Text;
                inperson.honorific = this.txtHonorific.Text;
                inperson.phone = this.txtPhone.Text;
                inperson.state = this.txtState.Text;
                inperson.zipcode = this.txtZIP.Text;
                if (!string.IsNullOrEmpty(this.lblUserID.Text))
                {
                    inperson.ID = new Guid(this.lblUserID.Text);
                }
                else
                {
                    inperson.ID = Guid.NewGuid();
                }
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                if (!(!(base.GetUserID(conn) != inperson.ID) || base.UserIsAdministrator))
                {
                    base.ShowAlert("You may only edit your own user information if you are not an administrator.");
                }
                else
                {
                    inperson.Save(conn);
                    SqlCommand cmd = new SqlCommand("RegisterUser", conn)
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
                    this.PopulateData(inperson, conn);
                    conn.Close();
                }
            }
        }

        protected void Login()
        {
            Person p = (Person)GetSessionValue("WorkingPerson");
            bool passwordMatch = false;
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            SqlCommand cmd = new SqlCommand("LookupUser", conn)
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
                    FormsAuthenticationTicket tkt = new FormsAuthenticationTicket(1, this.txtUsername.Text, DateTime.Now, DateTime.Now.AddMinutes(30.0), this.chkPersistCookie.Checked, "");
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
                    PopulateData(p, conn);
                }
                else
                {
                    base.ShowAlert("Login incorrect.  Please try again, or create a new user account.");
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
            if(username == string.Empty && userid == Guid.Empty)
            {
                username = Username;
            }
            if (isLogin && !base.IsAuthenticated)
            {
                this.login.Visible = true;
            }
            else
            {
                this.login.Visible = false;
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
            this.txtFirstName.Text = inperson.first_name;
            this.txtMiddleName.Text = inperson.middle_name;
            this.txtLastName.Text = inperson.last_name;
            this.txtAddress1.Text = inperson.address_line1;
            this.txtAddress2.Text = inperson.address_line2;
            this.txtAffiliation.Text = inperson.affiliation;
            this.txtCity.Text = inperson.city;
            this.txtComments.Text = inperson.comment;
            this.txtCountry.Text = inperson.country;
            this.txtEmail.Text = inperson.email;
            this.txtFax.Text = inperson.fax;
            this.txtHonorific.Text = inperson.honorific;
            this.txtPhone.Text = inperson.phone;
            this.txtState.Text = inperson.state;
            this.txtZIP.Text = inperson.zipcode;
            this.lblUserID.Text = inperson.ID.ToString();
            this.txtEmail.ReadOnly = true;
            if (base.Username == inperson.email)
            {
                SqlCommand cmd = new SqlCommand("LookupUser", conn)
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
                this.txtFirstName.ReadOnly = false;
                this.txtMiddleName.ReadOnly = false;
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
            else if(IsAuthenticated)
            {
                this.txtPasswordNew.Enabled = false;
                this.txtPasswordConfirm.Enabled = false;
                this.txtFirstName.ReadOnly = false;
                this.txtMiddleName.ReadOnly = false;
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
                this.txtMiddleName.ReadOnly = true;
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
        }
        
        protected void txtPassword_TextChanged(object sender, EventArgs e)
        {
            this.Login();
        }
    }
}