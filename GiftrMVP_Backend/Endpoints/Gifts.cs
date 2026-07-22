using GiftrMVP_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GiftrMVP_Backend.Endpoints
{
    public class ImageAssetDto
    {
        public int imageAssetId { get; set; }
        public Guid imageKey { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
    public class GiftDto
    {
        public int giftId { get; set; }
        public string title { get; set; } = string.Empty;
        public Boolean given { get; set; }
        public string? url { get; set; }
        public string? notes { get; set; }

        //Sent as the enum name rather than its int, matching how the column is persisted.
        public string location { get; set; } = nameof(GiftLocation.Unsure);
        public int? imageAssetId { get; set; }
        public int? recipientId { get; set; }

        //Denormalised so the gift list can label rows without also fetching every recipient.
        public string? recipientName { get; set; }
    }

    public record NewGiftForm(string title, Boolean given, string? url, string? notes, string? location, int? imageAssetId, int? recipientId);

    public static class Gifts
    {
        private static GiftDto ToDto(Gift g) => new GiftDto
        {
            giftId = g.GiftId,
            title = g.Title,
            given = g.Given,
            url = g.Url,
            notes = g.Notes,
            location = g.Location.ToString(),
            imageAssetId = g.ImageAsset?.ImageAssetId,
            recipientId = g.RecipientId,
            recipientName = g.Recipient is null
                ? null
                : $"{g.Recipient.FirstName} {g.Recipient.LastName}".Trim()
        };

        //Unknown/missing values fall back to Unsure rather than 400ing - the enum already treats
        //"I don't know" as a real answer.
        private static GiftLocation ParseLocation(string? raw) =>
            Enum.TryParse<GiftLocation>(raw, ignoreCase: true, out var parsed) ? parsed : GiftLocation.Unsure;

        //A gift may only point at a recipient the caller owns. Returns false when the id is set but bogus.
        private static async Task<bool> RecipientIsOwned(GiftrDbContext db, int userId, int? recipientId)
        {
            if (recipientId is null) return true;
            return await db.Recipients.AnyAsync(r => r.RecipientId == recipientId && r.ProfileId == userId);
        }

        public static void MapGiftEndpoints(this WebApplication app)
        {
            app.MapPost("/gifts", async (ClaimsPrincipal user, GiftrDbContext db, NewGiftForm body) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                if (string.IsNullOrWhiteSpace(body.title))
                    return Results.BadRequest(new { error = "Title is required." });

                if (!await RecipientIsOwned(db, userId, body.recipientId))
                    return Results.BadRequest(new { error = "Unknown recipient." });

                var result = await db.Gifts.AddAsync(new Gift
                {
                    ProfileId = userId,
                    Title = body.title.Trim(),
                    Given = body.given,
                    Url = body.url,
                    Notes = body.notes,
                    Location = ParseLocation(body.location),
                    RecipientId = body.recipientId,
                });

                await db.SaveChangesAsync();

                //Reload with the nav properties the DTO reads - the tracked entity from AddAsync has neither.
                var created = await db.Gifts
                    .Include(g => g.Recipient)
                    .Include(g => g.ImageAsset)
                    .FirstAsync(g => g.GiftId == result.Entity.GiftId);

                return Results.Created($"/gifts/{created.GiftId}", ToDto(created));
            }).RequireAuthorization();

            //Optional ?recipientId= filter; ?unassigned=true narrows to loose ideas.
            app.MapGet("/gifts", async (ClaimsPrincipal user, GiftrDbContext db, int? recipientId, bool? unassigned) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var query = db.Gifts
                    .Where(g => g.ProfileId == userId)
                    .Include(g => g.Recipient)
                    .Include(g => g.ImageAsset)
                    .AsQueryable();

                if (recipientId is not null) query = query.Where(g => g.RecipientId == recipientId);
                if (unassigned == true) query = query.Where(g => g.RecipientId == null);

                //Outstanding ideas first - a list of things already given is the less useful default.
                var result = await query
                    .OrderBy(g => g.Given)
                    .ThenByDescending(g => g.GiftId)
                    .ToListAsync();

                return Results.Ok(result.Select(ToDto).ToList());
            }).RequireAuthorization();

            app.MapGet("/gifts/{id:int}", async (ClaimsPrincipal user, GiftrDbContext db, int id) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var gift = await db.Gifts
                    .Include(g => g.Recipient)
                    .Include(g => g.ImageAsset)
                    .FirstOrDefaultAsync(g => g.GiftId == id && g.ProfileId == userId);

                return gift is null ? Results.NotFound() : Results.Ok(ToDto(gift));
            }).RequireAuthorization();

            app.MapPut("/gifts/{id:int}", async (ClaimsPrincipal user, GiftrDbContext db, int id, NewGiftForm body) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                if (string.IsNullOrWhiteSpace(body.title))
                    return Results.BadRequest(new { error = "Title is required." });

                if (!await RecipientIsOwned(db, userId, body.recipientId))
                    return Results.BadRequest(new { error = "Unknown recipient." });

                var gift = await db.Gifts
                    .Include(g => g.Recipient)
                    .Include(g => g.ImageAsset)
                    .FirstOrDefaultAsync(g => g.GiftId == id && g.ProfileId == userId);

                if (gift is null) return Results.NotFound();

                gift.Title = body.title.Trim();
                gift.Given = body.given;
                gift.Url = body.url;
                gift.Notes = body.notes;
                gift.Location = ParseLocation(body.location);
                gift.RecipientId = body.recipientId;

                await db.SaveChangesAsync();

                //Recipient may have just changed - reload so the returned name isn't the stale one.
                await db.Entry(gift).Reference(g => g.Recipient).LoadAsync();

                return Results.Ok(ToDto(gift));
            }).RequireAuthorization();

            app.MapDelete("/gifts/{id:int}", async (ClaimsPrincipal user, GiftrDbContext db, int id) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var gift = await db.Gifts
                    .FirstOrDefaultAsync(g => g.GiftId == id && g.ProfileId == userId);

                if (gift is null) return Results.NotFound();

                db.Gifts.Remove(gift);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
