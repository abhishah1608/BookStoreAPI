using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Models
{
    public class PaymentForm
    {
        public int loginId { get; set; }

        public int userId { get; set; }

        public string key { get; set; }

        public string TransactionId { get; set; }

        public string amount { get; set; }
        
        public string productinfo { get; set; }

        public string firstName { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public string surl { get; set; }

        public string furl { get; set; }

        public string hash { get; set; }

        public string lastName { get; set; }

        public string address1 { get; set; }

        public string address2 { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string country { get; set; }

        public string zipCode { get; set; }

        public string salt { get; set; }

        public string url { get; set; }

        public string product { get; set; }

        public string udf1 { get; set; }

        public string CartGuid { get; set; }

        public string StripeUrl { get; set; }

        public string checkoutSessionId { get; set; }

        public string language { get; set; }

        public string IsReact { get; set; }
    }

    public class paymentInfo
    {
        public string status { get; set; }

        public List<BookDetails> details { get; set; }

        public string amount { get; set; }

        public string OrderId { get; set; }
    }


}