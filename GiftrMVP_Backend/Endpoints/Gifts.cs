using GiftrMVP_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;


namespace GiftrMVP_Backend.Endpoints
{
    public class ImageAssetDto
    {
        public int imageAssetId { get; set; }
        public string imageKey { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
    public class GiftDto
    {
        public int giftId { get; set; }
        public string title { get; set; } = string.Empty;
        public Boolean given { get; set; }
        public string? url { get; set; }
        public int? imageAssetId { get; set; }
        public int? recipientId { get; set; }
        
    }

    public record NewGiftForm(string title, Boolean given, string? url, int? imageAssetId, int? recipientId);

    public static class Gifts
    {
        public static void MapGiftEndpoints(this WebApplication app)
        {
            app.MapPost("/gifts", async (ClaimsPrincipal user, GiftrDbContext db, NewGiftForm body) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var result = await db.Gifts.AddAsync(new Gift
                {
                    Id = userId,
                    Title = body.title,
                    Given = body.given,
                    Url = body.url,
                    RecipientId = body.recipientId,
                });

                await db.SaveChangesAsync();

                var newRow = result.Entity as Gift;

                var responseDto = new GiftDto
                {
                    giftId = newRow.GiftId,
                    title = newRow.Title,
                    given = newRow.Given,
                    url = newRow.Url,
                    imageAssetId = newRow.ImageAsset?.GiftId,
                    recipientId = newRow.RecipientId,
                };


            }).RequireAuthorization();
        }
    }
}
