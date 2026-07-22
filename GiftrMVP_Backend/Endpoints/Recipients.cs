using GiftrMVP_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftrMVP_Backend.Endpoints
{

    public class RecipientDto
    {
        public int recipientId { get; set; }
        public string firstName { get; set; } = "";
        public string? lastName { get; set; }
        public string? notes { get; set; }

        //Cheap summary so the list view can show "3 ideas" without a second round trip.
        public int giftCount { get; set; }
    }
    public record NewRecipientForm(string firstName, string? lastName, string? notes);
    public static class Recipients
    {
        //One place that knows how to flatten the entity, so every endpoint returns the same shape.
        private static RecipientDto ToDto(Recipient r) => new RecipientDto
        {
            recipientId = r.RecipientId,
            firstName = r.FirstName,
            lastName = r.LastName,
            notes = r.Notes,
            giftCount = r.Gifts.Count
        };

        public static void MapRecipientEndpoints(this WebApplication app)
        {
            app.MapPost("/recipients", async (ClaimsPrincipal user, GiftrDbContext db, NewRecipientForm body) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                if (string.IsNullOrWhiteSpace(body.firstName))
                    return Results.BadRequest(new { error = "First name is required." });

                var result = await db.Recipients.AddAsync(new Recipient
                {
                    ProfileId = userId,
                    FirstName = body.firstName.Trim(),
                    LastName = body.lastName?.Trim(),
                    Notes = body.notes,
                });

                await db.SaveChangesAsync();

                return Results.Ok(ToDto(result.Entity));
            }).RequireAuthorization();

            app.MapGet("/recipients", async (ClaimsPrincipal user, GiftrDbContext db) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                //Scoped to the caller - without the Where this hands back every user's recipients.
                var result = await db.Recipients
                    .Where(r => r.ProfileId == userId)
                    .Include(r => r.Gifts)
                    .OrderBy(r => r.FirstName)
                    .ThenBy(r => r.LastName)
                    .ToListAsync();

                return Results.Ok(result.Select(ToDto).ToList());
            }).RequireAuthorization();

            app.MapGet("/recipients/{id:int}", async (ClaimsPrincipal user, GiftrDbContext db, int id) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var recipient = await db.Recipients
                    .Include(r => r.Gifts)
                    .FirstOrDefaultAsync(r => r.RecipientId == id && r.ProfileId == userId);

                //404 rather than 403 on someone else's row - don't confirm that the id exists.
                return recipient is null ? Results.NotFound() : Results.Ok(ToDto(recipient));
            }).RequireAuthorization();

            app.MapPut("/recipients/{id:int}", async (ClaimsPrincipal user, GiftrDbContext db, int id, NewRecipientForm body) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                if (string.IsNullOrWhiteSpace(body.firstName))
                    return Results.BadRequest(new { error = "First name is required." });

                var recipient = await db.Recipients
                    .Include(r => r.Gifts)
                    .FirstOrDefaultAsync(r => r.RecipientId == id && r.ProfileId == userId);

                if (recipient is null) return Results.NotFound();

                recipient.FirstName = body.firstName.Trim();
                recipient.LastName = body.lastName?.Trim();
                recipient.Notes = body.notes;

                await db.SaveChangesAsync();

                return Results.Ok(ToDto(recipient));
            }).RequireAuthorization();

            app.MapDelete("/recipients/{id:int}", async (ClaimsPrincipal user, GiftrDbContext db, int id) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var recipient = await db.Recipients
                    .FirstOrDefaultAsync(r => r.RecipientId == id && r.ProfileId == userId);

                if (recipient is null) return Results.NotFound();

                //Gifts outlive the recipient - they fall back to being un-assigned ideas.
                await db.Gifts
                    .Where(g => g.RecipientId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(g => g.RecipientId, (int?)null));

                db.Recipients.Remove(recipient);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
