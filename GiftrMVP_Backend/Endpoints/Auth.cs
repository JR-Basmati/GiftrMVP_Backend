using Microsoft.EntityFrameworkCore;
using GiftrMVP_Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            expires: DateTime.UtcNow.AddMinutes(15), //Short-lived - the refresh token handles session longevity.
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //Opaque, high-entropy refresh token. Returns the raw value (sent to the client once) and its hash (stored in the DB).
    public static (string token, string hash) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (raw, HashToken(raw));
    }

    public static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

//JSON Bodies coming from the Client
record UserRegisterDetails(string FirstName, string LastName, string Email, string Password);
record UserLoginDetails(string Email, string Password);
record RefreshRequest(string RefreshToken);

namespace GiftrMVP_Backend.Endpoints
{
    public static class Auth
    {
        //How long a refresh token stays valid. Rotation resets this window on every use, so an active user stays logged in.
        static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

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

            app.MapPost("/login", async (UserLoginDetails body, UserManager<Profile> userManager, SignInManager<Profile> signInManager, GiftrDbContext db, IConfiguration config) =>
            {
                var user = await userManager.FindByEmailAsync(body.Email);
                if (user is null) return Results.Unauthorized();

                var check = await signInManager.CheckPasswordSignInAsync(user, body.Password, lockoutOnFailure: true);
                if (!check.Succeeded) return Results.Unauthorized();

                var refreshToken = await IssueRefreshToken(user, db);

                return Results.Ok(new
                {
                    accessToken = Tokens.GenerateJwt(user, config),
                    refreshToken
                });
            });

            //Exchange a valid refresh token for a fresh access token + a rotated refresh token.
            //Anonymous on purpose: by the time you call this the access token is already expired.
            app.MapPost("/refresh", async (RefreshRequest body, GiftrDbContext db, UserManager<Profile> userManager, IConfiguration config) =>
            {
                if (string.IsNullOrWhiteSpace(body.RefreshToken)) return Results.Unauthorized();

                var hash = Tokens.HashToken(body.RefreshToken);
                var stored = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);

                if (stored is null) return Results.Unauthorized();

                //Reuse detection: an already-rotated/revoked token is being presented again.
                //Treat as possible theft and revoke every active token for the user.
                if (stored.RevokedAt is not null)
                {
                    await db.RefreshTokens
                        .Where(rt => rt.ProfileId == stored.ProfileId && rt.RevokedAt == null)
                        .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow));
                    return Results.Unauthorized();
                }

                if (DateTime.UtcNow >= stored.ExpiresAt) return Results.Unauthorized();

                var user = await userManager.FindByIdAsync(stored.ProfileId.ToString());
                if (user is null) return Results.Unauthorized();

                //Rotate: revoke the presented token and mint a fresh one, linking them for reuse detection.
                var (newRaw, newHash) = Tokens.GenerateRefreshToken();
                stored.RevokedAt = DateTime.UtcNow;
                stored.ReplacedByHash = newHash;
                db.RefreshTokens.Add(new RefreshToken
                {
                    ProfileId = user.Id,
                    TokenHash = newHash,
                    ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
                });
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    accessToken = Tokens.GenerateJwt(user, config),
                    refreshToken = newRaw
                });
            });

            //Revoke the presented refresh token so it can't be used again after logout.
            app.MapPost("/logout", async (RefreshRequest body, GiftrDbContext db) =>
            {
                if (!string.IsNullOrWhiteSpace(body.RefreshToken))
                {
                    var hash = Tokens.HashToken(body.RefreshToken);
                    await db.RefreshTokens
                        .Where(rt => rt.TokenHash == hash && rt.RevokedAt == null)
                        .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow));
                }
                return Results.Ok();
            });

            app.MapGet("/me", async (ClaimsPrincipal user, GiftrDbContext db) =>
            {

                var userId = int.Parse(user.FindFirstValue("sub")!);

                var userProfile = await db.Profiles.FirstOrDefaultAsync<Profile>(p => p.Id == userId);

                if (userProfile is null) return Results.Unauthorized();

                return Results.Ok(new { id = userProfile.Id, firstName = userProfile.FirstName, lastName = userProfile.LastName, email = userProfile.Email });

            }).RequireAuthorization();
        }

        //Create + persist a new refresh token for the user, returning the raw value to hand back to the client.
        private static async Task<string> IssueRefreshToken(Profile user, GiftrDbContext db)
        {
            var (raw, hash) = Tokens.GenerateRefreshToken();
            db.RefreshTokens.Add(new RefreshToken
            {
                ProfileId = user.Id,
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            });
            await db.SaveChangesAsync();
            return raw;
        }
    }
}
