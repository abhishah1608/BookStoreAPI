using DemoAngularApp.Auth;
using DemoAngularApp.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace DemoAngularApp.Controllers
{
    public class LoginController : ApiController
    {
        string connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();

        /// <summary>
        /// Used to know that user is Logged in some location.
        /// </summary>
        /// <param name="login">Login that is BO of username,password and LoginId</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage IsUserloggedIn([FromBody] Login login)
        {
            HttpResponseMessage msg = null;
            int retval = -1;
            if (login != null)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                if (connection != null)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "CheckUserLoggedIn";
                    cmd.Parameters.AddWithValue("@USERNAME", login.username);
                    cmd.Parameters.AddWithValue("@PASSWORD", login.password);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    sqlDataAdapter.Fill(table);
                    if (table != null && table.Rows != null)
                    {
                        if (table.Rows.Count > 0)
                        {
                            DataRow dr = table.Rows[0];
                            retval = Convert.ToInt32(dr[0]);
                        }
                    }
                }
            }
            msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            return msg;
        }

        /// <summary>
        /// used to Add user in login User table if Sign up is 1 else if Sign up is 0 then it will insert details in login users.
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage AddUser([FromBody] UserDetail detail)
        {
            HttpResponseMessage msg = null;
            UserAddClass user = new UserAddClass();
            string token = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AddUser";
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                cmd.Parameters.AddWithValue("@USERNAME", detail.username);
                cmd.Parameters.AddWithValue("@PASSWORD", detail.password);
                cmd.Parameters.AddWithValue("@browser", HttpContext.Current.Request.Browser.Browser);
                cmd.Parameters.AddWithValue("@SIGNUP", detail.Signup);
                cmd.Parameters.AddWithValue("@email", detail.email);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                user.LoginId = 0;
                user.UserId = 0;
                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables.Count > 1)
                    {
                        DataTable dt = ds.Tables[1];
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            user.UserId = Convert.ToInt32(dr[0]);
                            user.LoginId = Convert.ToInt32(dr[1]);
                            user.emailId = Convert.ToString(dr[2]);
                            token = JwtAuthManager.GenerateJWTToken(detail.username, 45);
                            user.seckey = token;
                        }
                    }
                }
            }
            msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, user);
            return msg;
        }

        /// <summary>
        /// Service call to logout user to update logout time in logindetails table based on loginId.
        /// </summary>
        /// <param name="loginId">unique identity for login sessionId.</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage LogoutService(string loginId)
        {
            HttpResponseMessage msg = null;
            int Retval = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LOGOUT";
                SqlParameter parameter = new SqlParameter();
                cmd.Parameters.AddWithValue("@loginId", loginId);
                con.Open();
                Retval = cmd.ExecuteNonQuery();
                con.Close();
            }
            if (Retval == 1)
            {
                msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, Retval);
            }
            else
            {
                msg = Request.CreateResponse(System.Net.HttpStatusCode.NotModified, Retval);
            }
            return msg;
        }

    }
}
