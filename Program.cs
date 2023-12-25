using XGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json")
    .Build();

var xConfig = config.GetSection("X").Get<XConfiguration>() ?? throw new Exception("Can not load appsettings");
var openAiConfig = config.GetSection("OpenAi").Get<OpenAiConfiguration>() ?? throw new Exception("Can not load appsettings");

builder.Services.AddSingleton(xConfig);
builder.Services.AddSingleton(openAiConfig);
builder.Services.AddTransient<TweetService>();
builder.Services.AddTransient<OpenAiService>();

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
