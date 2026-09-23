using HotelHub.API.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace HotelHub.API.Data
{
	/// <summary>
	/// Provides methods for seeding default roles and the initial administrator account.
	/// </summary>
	public static class IdentitySeeder
	{
		/// <summary>
		/// Creates the default application roles if they do not already exist.
		/// </summary>
		public static async Task SeedRoleAsync(
			RoleManager<IdentityRole<Guid>> roleManager)
		{
			string[] roles =
			[
				"Admin",
				"User"
			];

			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(
						new IdentityRole<Guid>(role)
					);
				}
			}
		}

		/// <summary>
		/// Creates the initial administrator account and assigns the Admin role.
		/// </summary>
		public static async Task SeedAdminAsync(
			UserManager<ApplicationUser> userManager,
			IConfiguration configuration
		)
		{
			var email = configuration["AdminUser:Email"]
						?? throw new InvalidOperationException(
						"Admin email must be configured.");

			var password = configuration["AdminUser:Password"]
							?? throw new InvalidOperationException(
							"Admin password must be configured.");

			var firstName = configuration["AdminUser:FirstName"]
							?? throw new InvalidOperationException(
							"First name must be configured.");

			var lastName = configuration["AdminUser:LastName"]
							?? throw new InvalidOperationException(
							"Last name must be configured.");

			const string adminRole = "Admin";

			var admin = await userManager.FindByEmailAsync(email);

			if (admin is null)
			{
				admin = new ApplicationUser
				{
					FirstName = firstName,
					LastName = lastName,
					Email = email,
					UserName = email,
					EmailConfirmed = true
				};

				var createResult = await userManager.CreateAsync(
					admin,
					password
				);

				if (!createResult.Succeeded)
				{
					throw new InvalidOperationException(
						string.Join(
							", ",
							createResult.Errors.Select(e => e.Description)
						)
					);
				}
			}

			if (!await userManager.IsInRoleAsync(admin, adminRole))
			{
				var roleResult = await userManager
					.AddToRoleAsync(admin, adminRole);

				if (!roleResult.Succeeded)
				{
					throw new InvalidOperationException(
						string.Join(
							", ",
							roleResult.Errors.Select(e => e.Description)
						)
					);
				}
			}
		}
	}
}