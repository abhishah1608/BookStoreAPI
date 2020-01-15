
namespace DemoAngularApp.Models
{
    public class Login
    {
        public string username { get; set; }

        public string password { get; set; }

        public int loginId { get; set; }
    }

    public class UserAddClass
    {
        public int UserId { get; set; }

        public int LoginId { get; set; }

        public string emailId { get; set; }

        public string seckey { get; set; }
    }
}