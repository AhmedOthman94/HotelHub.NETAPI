using HotelHub.API.Entity;
using HotelHub.API.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Data
{
	/// <summary>
	/// Represents the application's Entity Framework Core database context.
	/// </summary>
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: IdentityDbContext<ApplicationUser,
							IdentityRole<Guid>,
							Guid>
		(options)
	{
		public DbSet<Country> Countries { get; set; }
		public DbSet<Hotel> Hotels { get; set; }
		public DbSet<HotelAdmin> HotelAdmins { get; set; }
		public DbSet<Booking> Bookings { get; set; }

		public DbSet<Room> Rooms { get; set; }

		/// <summary>
		/// Configures entity mappings and applies all registered EF Core configurations.
		/// </summary>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(ApplicationDbContext).Assembly
			);
		}
	}
}