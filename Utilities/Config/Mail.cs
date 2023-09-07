using System.Net;
using System.Net.Mail;

namespace CRM_mvc.Utilities.Config
{
    public static class Mail
    {
        public static string Host = "sandbox.smtp.mailtrap.io";
        public static int Port = 2525;
        // username 
        public static string Username = "4d9a0a90f4cd94";
        // password
        public static string Password = "1bdcb492d444c5";

        public static bool EnableSsl = true;

        public static SmtpClient setClint()
        {
            return new SmtpClient(Host, Port)
            {
                Credentials = new NetworkCredential(Username, Password),
                EnableSsl = EnableSsl
            };
        }
    }
}