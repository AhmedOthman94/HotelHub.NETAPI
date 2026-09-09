using HotelHub.API.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelHub.API.Data.Configurations
{
	public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
	{
		public void Configure(EntityTypeBuilder<Hotel> builder)
		{
			builder.HasKey(h => h.Id);

			builder.Property(h => h.Name)
				.IsRequired()
				.HasMaxLength(150);

			builder.Property(h => h.Rating)
				.HasPrecision(2, 1);

			builder.ComplexProperty(h => h.Address, address => 
			{
				address.Property(a => a.Street)
					.IsRequired()
					.HasMaxLength(200);

				address.Property(a => a.City)
					.IsRequired()
					.HasMaxLength(100);

				address.Property(a => a.State)
					.HasMaxLength(100);

				address.Property(a => a.PostalCode)
					.HasMaxLength(20);

				address.Property(a => a.Latitude)
					.HasColumnName("Address_Latitude")
					.HasPrecision(9, 6);

				address.Property(a => a.Longitude)
					.HasColumnName("Address_Longitude")
					.HasPrecision(9, 6);
			});

			builder.HasOne(h => h.Country)
				.WithMany(c => c.Hotels)
				.HasForeignKey(h => h.CountryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.ToTable("Hotels", table =>
			{
				table.HasCheckConstraint(
					"CK_Hotels_Rating",
					"[Rating] >= 0 AND [Rating] <= 5");

				table.HasCheckConstraint(
					"CK_Hotels_Latitude",
					"[Address_Latitude] >= -90 AND [Address_Latitude] <= 90");

				table.HasCheckConstraint(
					"CK_Hotels_Longitude",
					"[Address_Longitude] >= -180 AND [Address_Longitude] <= 180");
			});
		}
	}
}
