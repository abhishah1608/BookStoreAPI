using DemoAngularApp.Auth;
using DemoAngularApp.Models;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Xml.Serialization;

namespace DemoAngularApp.Controllers
{
    public class StripePaymentController : ApiController
    {
        string connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();

        [HttpPost]
        [JwtAuthentication]
        public HttpResponseMessage Checkout([FromBody] PaymentForm payment)
        {
            var domain = ConfigurationManager.AppSettings["domainurl"].ToString();
            payment.CartGuid = Guid.NewGuid().ToString();
            string productInfo = payment.productinfo;
            List<BookDetails> bookDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BookDetails>>(productInfo);
            double amount = 0;
            List<SessionCustomFieldOptions> list = null;

            if (payment.language != null)
            {
                list = new List<SessionCustomFieldOptions>();
                SessionCustomFieldOptions item = new SessionCustomFieldOptions();
                item.Key = payment.language;
                list.Add(item);
            }

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = ConfigurationManager.AppSettings["surl"],
                CancelUrl = ConfigurationManager.AppSettings["cancelurl"],
                LineItems = Utility.Utility.GenerateCheckoutList(bookDetails, out amount),
                Mode = "payment",
                CustomerEmail = payment.email,
                ClientReferenceId = payment.CartGuid  
            };

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            string paymentDetail = string.Empty;                   // The string variable that will hold the serialized data

            payment.StripeUrl = session.Url;
            payment.amount = amount.ToString();
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

            return Request.CreateResponse(payment);
        }

        [HttpGet]
        public HttpResponse GetResponseFromStripeAPI([FromUri] string session_id)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = sessionService.Get(session_id);
            string email = session.CustomerEmail;
            string CartGuid = session.ClientReferenceId;
            string status = "success";
            List<SessionCustomField> options = session.CustomFields;

            string language = "null";

            if(options != null && options.Count > 0)
            {
                language = options[0].Key;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "updatePayment";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CartGuid", CartGuid);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@CHECKOUT_SESSION_ID", session_id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            string redirectUrl = ConfigurationManager.AppSettings["redirecturl"];
            if(language != null)
            {
                redirectUrl +=  "/" + language + "/app/paymentStatus";
            }
            else
            {
                redirectUrl += "/app/paymentStatus";
            }
            
            redirectUrl = redirectUrl + "/" + session_id;
            HttpContext.Current.Response.Redirect(redirectUrl);
            return null;
        }

        [HttpGet]
        public HttpResponse CancelStripePayment([FromUri] string session_id)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = sessionService.Get(session_id);
            string email = session.CustomerEmail;
            string CartGuid = session.ClientReferenceId;
            string status = "Cancel";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "updatePayment";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CartGuid", CartGuid);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@CHECKOUT_SESSION_ID", session_id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            string redirectUrl = ConfigurationManager.AppSettings["redirecturl"];
            redirectUrl = redirectUrl + "/" + session_id;
            HttpContext.Current.Response.Redirect(redirectUrl);
            return null;
        }

        [HttpGet]
        [JwtAuthentication]
        public HttpResponseMessage GetPaymentDetail([FromUri] string checkoutSessionId)
        {
            HttpResponseMessage msg = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "GetPaymentDetail";
                cmd.Parameters.AddWithValue("@CheckoutSessionId", checkoutSessionId);
                cmd.Connection = con;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    paymentInfo info = new paymentInfo();
                    foreach (DataRow dr in dt.Rows)
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
