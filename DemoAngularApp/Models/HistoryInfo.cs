using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Models
{
    public class HistoryInfo
    {
        public List<BookDetails> purchaseInfo { get; set; }

        public int Status { get; set; }

        public int OrderId { get; set; }
    }
}