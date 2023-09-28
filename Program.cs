var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/trends", async () =>
    // https://api.twitter.com/1.1/trends/place.json
    "Hello World!");

app.Run();
