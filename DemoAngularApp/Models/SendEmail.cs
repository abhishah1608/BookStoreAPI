using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Models
{
    public class SendEmail
    {
        public string Subject { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string body { get; set; }

        public string password { get; set; }

        public string OrderId { get; set; }
    }
}