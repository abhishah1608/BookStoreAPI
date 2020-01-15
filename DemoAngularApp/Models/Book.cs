using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Models
{
    public class Book
    {
        public int BookId { get; set; }

        public string BookName { get; set; }

        public string BookAuthor { get; set; }

        public int BookPrice { get; set; }

        public int stock { get; set; }

        public string Description { get; set; }

        public double ratings { get; set; }
    }

    public class BookAddedInCart
    {
        
        public List<BookDetails> detail { get; set; }

        public int Amount { get; set; }

        public int UserId { get; set; }

        public int StatusId { get; set; }

        public int LoginId { get; set; }
    }

    public class BookDetails
    {
        public int BookId { get; set; }

        public int Quantity { get; set; }

        public int total { get; set; }

        public string BookName { get; set; }
    }
     
}