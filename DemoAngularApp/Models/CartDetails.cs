using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Models
{
    public class CartDetails
    {
        public int CartId { get; set; } 
        public int BookId { get; set; } 
        
        public int userId { get; set; } 
        public int Quantity { get; set; }    
        
        public string IsPurchased { get; set; }

        public int IsRemoved { get; set; }
    }
}