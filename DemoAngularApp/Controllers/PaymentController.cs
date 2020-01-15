using DemoAngularApp.Auth;
using DemoAngularApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;

namespace DemoAngularApp.Controllers
{
    
    public class PaymentController : ApiController
    {
        string connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        [HttpPost]
        [JwtAuthentication]
        public HttpResponseMessage Generateform([FromBody]PaymentForm paymentForm)
        {
            HttpResponseMessage msg = null;
            paymentForm.key = ConfigurationManager.AppSettings["merchantkey"].ToString();
            paymentForm.salt = ConfigurationManager.AppSettings["merchantsalt"].ToString();
            string transId = System.Guid.NewGuid().ToString();
            paymentForm.TransactionId = GetString(transId);
            paymentForm.surl = ConfigurationManager.AppSettings["surl"].ToString();
            paymentForm.furl = ConfigurationManager.AppSettings["furl"].ToString();
            paymentForm.url = ConfigurationManager.AppSettings["url"].ToString();
            paymentForm.amount = "1.00";
            Generatesha512(paymentForm);
            AddPaymentInfoInDb(paymentForm);
            //HttpContext.Current.Session["ürlRedirect"] = paymentForm.udf1;
            msg = Request.CreateResponse(HttpStatusCode.OK, paymentForm);
            return msg;
        }

        private void AddPaymentInfoInDb(PaymentForm payment)
        {
            string paymentDetail = string.Empty;                   // The string variable that will hold the serialized data

            XmlSerializer serializer = new XmlSerializer(payment.GetType());
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, payment);
                paymentDetail = sw.ToString();
            }

            try
            {

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "AddPayment";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@paymentDetail", paymentDetail);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message.ToString();
            }
        }

        private string GetString(string transId)
        {
            if (transId != null && transId.Contains("-"))
            {
                string str = "";
                string[] arr = transId.Split('-');
                foreach (string s in arr)
                {
                    str += s;
                }
                transId = str.Length >= 32 ? str.Substring(0, 30) : str;
                int len = transId.Length;
            }
            return transId;
        }

        /// <summary>
        ///method used to set hash in paymentForm hash property. 
        /// </summary>
        /// <param name="paymentForm">aymentForm paymentForm</param>
        //key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5||||||salt;
        private void Generatesha512(PaymentForm paymentForm)
        {
            string input = paymentForm.key + "|" + paymentForm.TransactionId + "|" + paymentForm.amount + "|" + paymentForm.product + "|" + paymentForm.firstName + "|" + paymentForm.email + "|" + paymentForm.udf1 + "||||||||||" + paymentForm.salt;
            paymentForm.hash = SHA512(input).ToLower();
        }

        /// <summary>
        /// Method used to generate Sha 512 based on input string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>returns string as Sha512 based on input.</returns>
        private string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        [HttpPost]
        public HttpResponseMessage PostResponseFromPayUMoney()
        {
            HttpResponseMessage msg = null;

            string TransactionId = HttpContext.Current.Request.Params["txnid"];

            string status = HttpContext.Current.Request.Params["status"];

            string payumoneyId = HttpContext.Current.Request.Params["payuMoneyId"];

            string redirectUrl = HttpContext.Current.Request.Params["udf1"];

            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = "";
            }
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "updatePayment";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@transactionId", TransactionId);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@payumoneyId", payumoneyId);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            redirectUrl = redirectUrl + "app/paymentStatus";
            redirectUrl = redirectUrl + "/" + payumoneyId;
            HttpContext.Current.Response.Redirect(redirectUrl);
            return msg;
        }

        [HttpGet]
        [JwtAuthentication]
        public HttpResponseMessage GetPaymentDetail([FromUri]int payumoneyId)
        {
            HttpResponseMessage msg = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "GetPaymentDetail";
                cmd.Parameters.AddWithValue("@payumoneyId", payumoneyId);
                cmd.Connection = con;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if(dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    paymentInfo info = new paymentInfo();
                    foreach(DataRow dr in dt.Rows)
                    {
                        if (dr["status"] != null)
                        {
                            info.status = dr["status"].ToString();
                        }
                        string str = dr["purchaseInfo"].ToString();
                        info.details = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BookDetails>>(str);
                        info.amount = dr["amount"].ToString();
                        info.OrderId = dr["OrderId"].ToString();
                        msg = Request.CreateResponse(HttpStatusCode.OK, info); 
                    }
                }
            }
            return msg;
        }

    }
}
