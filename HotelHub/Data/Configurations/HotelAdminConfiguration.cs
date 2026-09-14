using HotelHub.API.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelHub.API.Data.Configurations
{
	public class HotelAdminConfiguration : IEntityTypeConfiguration<HotelAdmin>
	{
		public void Configure(EntityTypeBuilder<HotelAdmin> builder)
		{
			builder.HasKey(ha => ha.Id);

			builder.HasOne(ha => ha.Hotel)
					.WithMany(h => h.HotelAdmins)
					.HasForeignKey(ha => ha.HotelId)
					.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(ha => ha.User)
					.WithMany()
					.HasForeignKey(ha => ha.UserId)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(ha => new { ha.HotelId, ha.UserId })
					.IsUnique();
		}
	}
}
