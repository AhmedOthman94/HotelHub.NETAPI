using HotelHub.API.Entity;
using HotelHub.API.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Data
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	: IdentityDbContext<ApplicationUser,
							IdentityRole<Guid>,
							Guid>
	(options)
	{
		public DbSet<Country> Countries { get; set; }
		public DbSet<Hotel> Hotels { get; set; }
		public DbSet<HotelAdmin> HotelAdmins { get; set; }
		public DbSet<Booking> Bookings {  get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(ApplicationDbContext).Assembly	
			);
		}
	}
}
