using HotelHub.API.Entity;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Data
{
	public class ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
	: DbContext(options)
	{
		public DbSet<Country> Countries { get; set; }
		public DbSet<Hotel> Hotels { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(ApplicationDbContext).Assembly	
			);
		}
	}
}
