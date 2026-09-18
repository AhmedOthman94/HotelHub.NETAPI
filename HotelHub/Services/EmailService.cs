using HotelHub.API.Services.IServices;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using HotelHub.API.Models;

namespace HotelHub.API.Services
{
	public class EmailService (
			IOptions<EmailOptions> options
	)
	: IEmailService
	{
		private readonly EmailOptions emailOptions = options.Value;
		public async Task SendAsync(string to, string subject, string body)
		{
			using var message = new MailMessage
			{
				From = new MailAddress(
						emailOptions.From,
						emailOptions.DisplayName
				),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			message.To.Add(to);

			using var client = new SmtpClient(
						emailOptions.Host,
						emailOptions.Port
			)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(
						emailOptions.UserName,
						emailOptions.Password
				)
			};

			await client.SendMailAsync(message);
		}
	}
}
