﻿using DemoAngularApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft;
using System.Configuration;
using DemoAngularApp.Auth;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace DemoAngularApp.Controllers
{
    [JwtAuthentication]
    public class BookController : ApiController
    {
        string connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();

        [HttpGet]
        public HttpResponseMessage GetBooks()
        {
            HttpResponseMessage msg = null;
            List<Book> list = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GETBookList";
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                if (table != null && table.Rows != null && table.Rows.Count > 0)
                {
                    list = new List<Book>();
                    foreach(DataRow dr in table.Rows)
                    {
                        if(dr != null)
                        {
                            Book book = new Book();
                            book.BookId = Convert.ToInt32(dr["Bookid"]);
                            book.BookName = Convert.ToString(dr["BookName"]);
                            book.Description = Convert.ToString(dr["Description"]);
                            book.stock = Convert.ToInt32(dr["Stock"]);
                            book.BookAuthor = Convert.ToString(dr["BookAuthor"]);
                            book.BookPrice = Convert.ToInt32(dr["BookPrice"]);
                            book.ratings = Convert.ToDouble(dr["ratings"]);
                            list.Add(book);
                        }
                    }
                }
            }
            msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, list);
            return msg;
        }

        [HttpPost]
        public HttpResponseMessage AddCartDetails([FromBody]List<CartDetails> cartDetails)
        {
            int retval = 0;
            HttpResponseMessage msg = new HttpResponseMessage();
            string xml = string.Empty;
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // Remove the XML declaration
                Indent = true // Optional: makes the XML more readable
            };

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                var serializer = new XmlSerializer(typeof(List<CartDetails>));
                serializer.Serialize(xmlWriter, cartDetails);
                xml = stringWriter.ToString();
            }

            // perform sp transaction to convert.
            // AddBookInCarts
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "AddBookInCarts";
                    cmd.Parameters.AddWithValue("@CartDetails", xml);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    retval = 1;
                }
                catch(Exception ex) {
                    retval = -1;
                }
            }
            msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            return msg;
        }

        [HttpGet]
        public HttpResponseMessage GetCartDetailsList([FromUri]int UserId)
        {
            HttpResponseMessage msg = null;
            List<CartDetails> list = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCartDetailsList";
                cmd.Parameters.Add("@UserId", UserId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                if (table != null && table.Rows != null && table.Rows.Count > 0)
                {
                    list = new List<CartDetails>();
                    foreach (DataRow dr in table.Rows)
                    {
                        if (dr != null)
                        {
                            CartDetails details = new CartDetails();
                            details.BookId = Convert.ToInt32(dr["Bookid"]);
                            details.userId = Convert.ToInt32(dr["userId"]);
                            details.Quantity = Convert.ToInt32(dr["Quantity"]);
                            details.IsPurchased = Convert.ToString(dr["IsPurchased"]);
                            details.BookName = Convert.ToString(dr["BookName"]);
                            details.Price = Convert.ToDouble(dr["Price"]);
                            details.BookPrice = Convert.ToDouble(dr["BookPrice"]);
                            list.Add(details);
                        }
                    }
                }
            }
            msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, list);
            return msg;
        }

        [HttpPost]
        public HttpResponseMessage AddOrder([FromBody]BookAddedInCart bookcart)
        {
            HttpResponseMessage msg = null;
            int retval = -1;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "AddCart";
                    string list = GenerateListBasedonCart(bookcart.detail);
                    string productInfo = null;
                    if(bookcart.detail != null)
                    {
                        productInfo = Newtonsoft.Json.JsonConvert.SerializeObject(bookcart.detail);
                    }
                    cmd.Parameters.AddWithValue("@list", list);
                    cmd.Parameters.AddWithValue("@userId", bookcart.UserId);
                    cmd.Parameters.AddWithValue("@statusId", bookcart.StatusId);
                    cmd.Parameters.AddWithValue("@amount", bookcart.Amount);
                    cmd.Parameters.AddWithValue("@loginId",bookcart.LoginId);
                    cmd.Parameters.AddWithValue("@productInfo", productInfo);
                    con.Open();
                    retval = cmd.ExecuteNonQuery();
                }
                catch(Exception)
                {
                    retval = -1;
                }
                finally
                {
                    if(con != null && con.State != ConnectionState.Closed)
                    {
                        con.Close();
                    }
                }
            }
            msg = Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookcart"></param>
        /// <returns></returns>
        private string GenerateListBasedonCart(List<BookDetails> bookcart)
        {
            string str = string.Empty; 
            if(bookcart != null && bookcart.Count > 0)
            {
                foreach(BookDetails cart in bookcart)
                {
                    if (str != string.Empty)
                    {
                        str += ";";
                    }
                    str += cart.BookId.ToString() + "," + cart.Quantity.ToString() + ',' + cart.total.ToString();
                }
            }
            return str;
        }

    }
}
