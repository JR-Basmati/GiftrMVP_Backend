using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using GiftrMVP_Backend.Models;
using GiftrMVP_Backend.Endpoints;

var envVars = DotEnv.Read();

var builder = WebApplication.CreateBuilder(args);

//Make env vars accessible via builder.configuration
builder.Configuration.AddInMemoryCollection(envVars!);


//Add database
builder.Services.AddDbContext<GiftrDbContext>(options =>
    options
        .UseNpgsql(envVars["DB_CONNECTION_STRING"])
        .UseSnakeCaseNamingConvention()); //Snake case for PG SQL convenience - otherwise default PascalCase requires quoting in SQL queries.

//Microsoft Identity - Core only, no cookies. We'll use JWT instead since we're cross-origin.
//Also can use .AddRoles(), but this is just an MVP so no need.
builder.Services.AddIdentityCore<Profile>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<GiftrDbContext>()
.AddSignInManager();

//JWT Bearer Auth - Separate from IdentityCore, since we don't want default cookie auth. This just validates the tokens.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; //Don't map to long schemas.xmlsoap.org URIs
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = envVars["JWT_ISSUER"],
            ValidAudience = envVars["JWT_AUDIENCE"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(envVars["JWT_SECRET"]!))

        };
    });

//Add Authorization
builder.Services.AddAuthorization();

//Allow CORS for React frontend
var ViteCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: ViteCorsPolicy,
            policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
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
app.UseAuthentication();
app.UseAuthorization();



app.MapGet("/", () => "Hello World!");

app.MapPost("/", (ClientHello incoming) =>
{
    Console.WriteLine("Client sent: " + incoming.message);
    var data = new { Message = "Hello from the server" };
    return Results.Ok(data);
});


//Register my custom endpoint defs from the /Endpoints dir
Auth.MapAuthEndpoints(app);
Recipients.MapRecipientEndpoints(app);



app.Run();

public record ProfileData(string firstName, string lastName, string email);
public record ClientHello(string message);