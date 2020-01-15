using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Models
{
    public class UserDetail
    {
        public string username { get; set; }

        public string email { get; set; }

        public string password { get; set; }

        public string browser { get; set; }

        public int Signup { get; set; }
    }
}