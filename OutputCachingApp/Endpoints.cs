using System.Text.Json;
using Microsoft.AspNetCore.OutputCaching;

namespace OutputCachingApp;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapGet("/weather", GetWeather).CacheOutput(x =>
        {
            x.Expire(TimeSpan.FromMinutes(3)).Tag("my-weather");
        })
        .WithName("GetWeather")
        .WithOpenApi();

        app.MapGet("/clear-cache", ClearCache)
        .WithName("ClearCache")
        .WithOpenApi();
    }

    static async Task<IResult> GetWeather(IWeatherService weatherService, string city)
    {
        var data = await weatherService.GetWeather(city);

        return data is null ? Results.NotFound() : Results.Ok(data);
    }

    static async Task<IResult> ClearCache(IOutputCacheStore cacheStore)
    {

        await cacheStore.EvictByTagAsync("my-weather", default);

        return Results.Ok();
    }
}


interface IWeatherService
{
    Task<dynamic> GetWeather(string city);
}

class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }


    public async Task<dynamic> GetWeather(string city)
    {

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://weatherapi-com.p.rapidapi.com/current.json?q={city}"),
            Headers =    {
                            { "X-RapidAPI-Key", _config.GetValue<string>("ApiKey") },
                            { "X-RapidAPI-Host", _config.GetValue<string>("ApiHost") },
                         },
        };

        string body;

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            body = await response.Content.ReadAsStringAsync();
        }

        return JsonSerializer.Deserialize<dynamic>(body);
    }
}