using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PropositoFit.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void EnviarCorreo(string destino, string asunto, string mensaje)
        {
            var email = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            var correo = new MimeMessage();
            correo.From.Add(MailboxAddress.Parse(email));
            correo.To.Add(MailboxAddress.Parse(destino));
            correo.Subject = asunto;

            correo.Body = new TextPart("html")
            {
                Text = mensaje
            };

            using var smtp = new SmtpClient();
            smtp.Connect(host, port, SecureSocketOptions.StartTls);
            smtp.Authenticate(email, password);
            smtp.Send(correo);
            smtp.Disconnect(true);
        }
    }
}