using HotelHub.API.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelHub.API.Data.Configurations
{
	public class RoomConfiguration : IEntityTypeConfiguration<Room>
	{
		public void Configure(EntityTypeBuilder<Room> builder)
		{
			builder.HasKey(r => r.Id);

			builder.Property(r => r.RoomNumber)
					.IsRequired()
					.HasMaxLength(20);

			builder.Property(r => r.RoomType)
					.HasConversion<string>()
					.IsRequired();

			builder.Property(r => r.Capacity)
					.IsRequired();

			builder.Property(r => r.PricePerNight)
					.HasPrecision(18, 2)
					.IsRequired();

			builder.HasOne(r => r.Hotel)
					.WithMany(h => h.Rooms)
					.HasForeignKey(r => r.HotelId)
					.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
					.IsUnique();
		}
	}
}
