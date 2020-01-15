using DemoAngularApp.Auth;
using DemoAngularApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DemoAngularApp.Controllers
{
    [JwtAuthentication]
    public class HistoryController : ApiController
    {
        string connectionstring = ConfigurationManager.AppSettings["ConnectionString"].ToString();

        [HttpPost]
        public HttpResponseMessage GetPurchaseHistory([FromBody]UserAddClass userClass)
        {
            HttpResponseMessage msg = null;
            List<HistoryInfo> info = null;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "purchasehistoryInfo";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@UserId", userClass.UserId);
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = cmd;
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);
                    if(dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
                    {
                        DataTable dt = dataSet.Tables[0];
                        if(dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            info = new List<HistoryInfo>();
                            foreach(DataRow dr in dt.Rows)
                            {
                                HistoryInfo history = new HistoryInfo();
                                string purchaseInfo = dr["purchaseInfo"].ToString();
                                history.purchaseInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BookDetails>>(purchaseInfo);
                                history.OrderId = Convert.ToInt32(dr["orderId"]);
                                history.Status = Convert.ToInt32(dr["status"]);
                                info.Add(history);
                            }
                        }
                    }
                    msg = Request.CreateResponse(HttpStatusCode.OK, info);
                }
            }
            catch(Exception ex)
            {
                msg = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
            return msg;
        }
    }
}
