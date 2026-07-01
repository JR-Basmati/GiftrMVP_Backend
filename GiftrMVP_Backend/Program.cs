
var builder = WebApplication.CreateBuilder(args);


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

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseCors(ViteCorsPolicy);
app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.MapPost("/", (ClientHello incoming) =>
{
    Console.WriteLine("Client sent: " + incoming.message);
    var data = new { Message = "Hello from the server" };
    return Results.Ok(data);
});

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");

app.Run();
public record ClientHello(string message);