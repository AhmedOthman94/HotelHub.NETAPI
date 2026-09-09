using HotelHub.API.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelHub.API.Data.Configurations
{
	public class CountryConfiguration : IEntityTypeConfiguration<Country>
	{	
		public void Configure(EntityTypeBuilder<Country> builder)
		{
			builder.HasKey(c => c.Id);

			builder.Property(c => c.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(c => c.CountryCode)
				.IsRequired()
				.HasMaxLength(3);

			builder.HasIndex(c => c.CountryCode)
				.IsUnique();

			builder.ToTable("Countries", table =>
			{
				table.HasCheckConstraint(
					"CK_Countries_CountryCode_Length",
					"LEN([CountryCode]) = 3");
			});
		}
	}
}
