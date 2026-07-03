using dotenv.net;
using Microsoft.EntityFrameworkCore;


var envVars = DotEnv.Read();

var builder = WebApplication.CreateBuilder(args);


//Add database
builder.Services.AddDbContext<GiftrDbContext>(options =>
    options
        .UseNpgsql(envVars["DB_CONNECTION_STRING"])
        .UseSnakeCaseNamingConvention()); //Snake case for PG SQL convenience - otherwise default PascalCase requires quoting in SQL queries.

//Allow CORS for React frontend
var ViteCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: ViteCorsPolicy,
            policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:5106");
            });
});


//Not sure what this guy is yet - I think OpenAPI is for Swagger docs?
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseCors(ViteCorsPolicy);
app.UseHttpsRedirection();//Note: This doesn't really work with CORS.. Just causes HTTP to fail.

app.MapGet("/", () => "Hello World!");

app.MapPost("/", (ClientHello incoming) =>
{
    Console.WriteLine("Client sent: " + incoming.message);
    var data = new { Message = "Hello from the server" };
    return Results.Ok(data);
});

app.MapGet("/profiles", async (GiftrDbContext db) =>
{
    try
    {
        var profiles = db.Profiles.ToList();
        return Results.Ok(profiles);
    }
    catch(Exception ex)
    {
        Console.WriteLine("Error retrieving users: " + ex.Message);
        return Results.Problem("An error occurred while retrieving users.");
    }
});

app.MapPost("/profiles", async (GiftrDbContext db, ProfileData data) =>
{
    try
    {
        var newProfile = new Profile
        {
            FirstName = data.firstName,
            LastName = data.lastName ?? string.Empty,
            Email = data.email
        };
        db.Profiles.Add(newProfile);
        db.SaveChanges();
        return Results.Ok(newProfile);
    }
    catch(Exception ex)
    {
        Console.WriteLine("Error creating user: " + ex.Message);
        return Results.Problem("An error occurred while creating the user.");
    }
    


});

app.Run();

public record ProfileData(string firstName, string lastName, string email);
public record ClientHello(string message);