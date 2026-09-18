namespace HotelHub.API.Services.IServices
{
	public interface IEmailService
	{
		Task SendAsync(
			string to,
			string subject,
			string body
		);
	}
}
