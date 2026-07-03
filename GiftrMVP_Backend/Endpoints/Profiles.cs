using GiftrMVP_Backend.Models;

namespace GiftrMVP_Backend.Endpoints
{
    public record ProfileForm(string firstName, string lastName, string email, string password);
    public static class Profiles
    {
        public static void MapProfilesEndpoints(this WebApplication app)
        {

            app.MapPost("/profiles", async (GiftrDbContext db) =>
            {

            });
        }
    }
    
}
