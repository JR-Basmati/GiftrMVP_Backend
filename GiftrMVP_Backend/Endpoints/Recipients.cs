using GiftrMVP_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace GiftrMVP_Backend.Endpoints
{

    public class RecipientDto
    {
        public int recipientId { get; set; }
        public string name { get; set; } = "";
        public string? notes { get; set; }

    }
    public record NewRecipientForm(string name, string notes);
    public static class Recipients
    {

        public static void MapRecipientEndpoints(this WebApplication app)
        {
            app.MapPost("/recipients", async (ClaimsPrincipal user, GiftrDbContext db, NewRecipientForm body) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var result = await db.Recipients.AddAsync(new Recipient
                {
                    Id = userId,
                    Name = body.name,
                    Notes = body.notes,
                });

                await db.SaveChangesAsync();

                var newRow = result.Entity;
                var responseDto = new RecipientDto
                {
                    recipientId = newRow.RecipientId,
                    name = newRow.Name,
                    notes = newRow.Notes
                };

                return Results.Ok(responseDto);
            }).RequireAuthorization();

            app.MapGet("/recipients", async (ClaimsPrincipal user, GiftrDbContext db) =>
            {
                var userId = int.Parse(user.FindFirstValue("sub")!);

                var result = await db.Recipients.ToListAsync<Recipient>();

                var responseDto = new List<RecipientDto>();
                result.ForEach((Recipient r) =>
                {
                    responseDto.Add(new RecipientDto
                    {
                        recipientId = r.RecipientId,
                        name = r.Name,
                        notes = r.Notes
                    });
                });

                return Results.Ok(responseDto);
            }).RequireAuthorization();
        }
    }
}
