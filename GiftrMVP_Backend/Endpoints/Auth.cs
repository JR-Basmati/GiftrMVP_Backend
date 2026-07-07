using dotenv.net;
using Microsoft.EntityFrameworkCore;
using GiftrMVP_Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

static class Tokens
{
    public static string GenerateJwt(Profile user, IConfiguration config)
    {

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),//Sub == "subject"
            new("email", user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT_SECRET"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: config["JWT_ISSUER"],
            audience: config["JWT_AUDIENCE"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

//JSON Bodies coming from the Client
record UserRegisterDetails(string FirstName, string LastName, string Email, string Password);
record UserLoginDetails(string Email, string Password);

namespace GiftrMVP_Backend.Endpoints
{
    public static class Auth
    {
       public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/register", async (UserRegisterDetails body, UserManager<Profile> users) =>
            {
                var user = new Profile
                {
                    FirstName = body.FirstName,
                    LastName = body.LastName,
                    Email = body.Email,
                    UserName = body.Email //MS Identity requires a username, just use email for now..
                };

                var result = await users.CreateAsync(user, body.Password);
                return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
            });

            app.MapPost("/login", async (UserLoginDetails body, UserManager<Profile> userManager, SignInManager<Profile> signInManager, IConfiguration config) =>
            {
                var user = await userManager.FindByEmailAsync(body.Email);
                if (user is null) return Results.Unauthorized();

                var check = await signInManager.CheckPasswordSignInAsync(user, body.Password, lockoutOnFailure: true);
                if (!check.Succeeded) return Results.Unauthorized();

                return Results.Ok(
                    new { token = Tokens.GenerateJwt(user, config)}
                    );
            });
            app.MapGet("/me", async (ClaimsPrincipal user, GiftrDbContext db) =>
            {

                var userId = int.Parse(user.FindFirstValue("sub")!);

                var userProfile = await db.Profiles.FirstOrDefaultAsync<Profile>(p => p.Id == userId);

                if (userProfile is null) return Results.Unauthorized();

                return Results.Ok(new { id = userProfile.Id, firstName = userProfile.FirstName, lastName = userProfile.LastName, email = userProfile.Email });

            }).RequireAuthorization();
        }
    }
}
