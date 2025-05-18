using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// The summaries array must be defined before use!
string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Home page: Shows a friendly message.
app.MapGet("/", () =>
    Results.Content(
        """
        <!DOCTYPE html>
        <html>
        <head>
            <title>Hello from .NET!</title>
            <style>
                body {
                    font-family: 'Segoe UI', Arial, sans-serif;
                    background: #f8fafc;
                    text-align: center;
                    margin: 0;
                    padding: 0;
                }
                .container {
                    margin-top: 100px;
                }
                h1 {
                    font-size: 3em;
                    color: #2563eb;
                    margin-bottom: 0.2em;
                    letter-spacing: 2px;
                }
                .emoji {
                    font-size: 4em;
                }
                p {
                    color: #555;
                }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="emoji">🚀🎉🌤️</div>
                <h1>Hello, World!</h1>
                <p>Your .NET app is running in Azure App Service.</p>
                <p>Try the <a href="/weatherforecast">weatherforecast</a> endpoint for sample data.</p>
            </div>
        </body>
        </html>
        """,
        "text/html"
    )
);

// Weatherforecast endpoint (with memory leak in broken slot)
app.MapGet("/weatherforecast", (HttpContext context) =>
{
    // Check if we're in the "broken" slot by looking for "-broken" in the host name
    var host = context.Request.Host.Host;
    if (host.Contains("-broken", StringComparison.OrdinalIgnoreCase))
    {
        // Simulate a memory leak
        List<byte[]> memoryLeak = new();
        for (int i = 0; i < 50_000; i++)
        {
            memoryLeak.Add(new byte[1024 * 1024]); // Allocate 1 MB per loop (~50 GB)
        }
    }

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run(); // <-- Must be BEFORE the record definition

// Put record(s) after app.Run()
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
