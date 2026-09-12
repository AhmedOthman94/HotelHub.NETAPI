namespace HotelHub.API.Models.Auth
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public string Token { get; set; } = string.Empty;
		public DateTime Expires {  get; set; }
		public DateTime Created { get; set; }
		public string? CreatedByIp { get; set; }
		public DateTime? Revoked { get; set; }
		public string? RevokedByIp { get; set; }
		public string? ReplacedByToken { get; set; }

		public bool IsExpired => DateTime.UtcNow >= Expires;
		public bool IsActive => Revoked == null && !IsExpired;

		//FK to ApplicationUser
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; } = null!;
	}
}
