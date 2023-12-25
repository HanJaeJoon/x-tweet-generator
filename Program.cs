using XGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json")
    .Build()
    .GetSection("X")
    .Get<Configuration>() ?? throw new Exception("Can not load appsettings")
    ;

builder.Services.AddSingleton(config);
builder.Services.AddTransient<TweetService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
