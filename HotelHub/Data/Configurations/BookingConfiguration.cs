using HotelHub.API.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelHub.API.Data.Configurations
{
	public class BookingConfiguration : IEntityTypeConfiguration<Booking>
	{
		public void Configure(EntityTypeBuilder<Booking> builder)
		{
			builder.HasKey(b => b.Id);

			builder.Property(b => b.TotalPrice)
					.HasPrecision(18, 2);

			builder.HasOne(b => b.Hotel)
					.WithMany(b => b.Bookings)
					.HasForeignKey(b => b.HotelId)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(b => b.User)
					.WithMany()
					.HasForeignKey(b => b.UserId)
					.OnDelete(DeleteBehavior.Restrict);

			builder.Property(b => b.Guests)
					.IsRequired();

			builder.Property(b => b.Status)
					.HasConversion<string>();
		}
	}
}
