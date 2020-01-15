using DemoAngularApp.Auth;
using DemoAngularApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;

namespace DemoAngularApp.Controllers
{
    [JwtAuthentication]
    public class EmailController : ApiController
    {
        public async Task<HttpResponseMessage> SendEmail([FromBody]SendEmail sendEmail)
        {
            HttpResponseMessage msg = null;
            string str = "";
            System.Net.HttpStatusCode statuscode = HttpStatusCode.OK;
            try
            {
                var message = new MailMessage();
                sendEmail.From = ConfigurationManager.AppSettings["emailsend"];
                sendEmail.password = "lvpgekxsvqnsazsd";
                message.To.Add(new MailAddress(sendEmail.To));  // replace with valid value 
                message.From = new MailAddress(sendEmail.From);  // replace with valid value
                message.Subject = sendEmail.Subject;
                message.Body = sendEmail.body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = sendEmail.From,  // replace with valid value
                        Password = sendEmail.password  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    str = "Email Send Successfully";
                }

            }
            catch (Exception ex)
            {
               statuscode = HttpStatusCode.InternalServerError;
               str = ex.ToString();
            }
            finally
            {
                msg = Request.CreateResponse(statuscode,str);
            }
            return msg;
        }
    }
}
