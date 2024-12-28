using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DemoAngularApp.Models;
using Stripe.Checkout;

namespace DemoAngularApp.Utility
{
    public static class Utility
    {
        public static List<SessionLineItemOptions> GenerateCheckoutList(List<BookDetails> booklist, out double amount)
        {
            List<Stripe.Checkout.SessionLineItemOptions> list = new List<Stripe.Checkout.SessionLineItemOptions>();
            amount = 0;
            foreach (BookDetails book in booklist)
            {
                SessionLineItemOptions option = new Stripe.Checkout.SessionLineItemOptions();
                if (book.Quantity > 0)
                {
                    option.PriceData = new SessionLineItemPriceDataOptions
                    {
                        //UnitAmount = (long)book.BookPrice,
                        UnitAmount = (long)(book.BookPrice * 100),
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = book.BookName
                        }
                    };
                    option.Quantity = (long)book.Quantity;
                    amount += book.BookPrice * book.Quantity;
                    list.Add(option);
                }
            }

            return list;
        }
    }
}