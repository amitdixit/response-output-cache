using OutputCachingApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IWeatherService, WeatherService>();
builder.Services.AddOutputCache().AddStackExchangeRedisOutputCache(x =>
{
    // x.ConnectionMultiplexerFactory = async () => await ConnectionMultiplexer.ConnectAsync("");

    x.InstanceName = nameof(WeatherService);
    x.Configuration = "localhost:6379";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOutputCache();

app.MapEndpoints();

app.Run();

