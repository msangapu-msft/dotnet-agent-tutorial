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
app.MapGet("/forecast-ui", () =>
    Results.Content(
        """
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <title>Weather Forecast</title>
            <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; background: #f8fafc; }
                .container { margin: 40px auto; max-width: 600px; }
                .toggle { margin-bottom: 20px; }
                .cards { display: flex; flex-wrap: wrap; gap: 16px; }
                .card {
                    background: #fff; border-radius: 10px; box-shadow: 0 4px 12px #0001;
                    padding: 24px; flex: 1 1 120px; min-width: 120px; text-align: center;
                    transition: transform 0.2s;
                }
                .card:hover { transform: scale(1.06); }
                .icon { font-size: 2.5em; margin-bottom: 8px; }
                .date { color: #888; margin-bottom: 5px; }
                .summary { font-weight: bold; }
                .temp { font-size: 1.7em; margin: 10px 0; }
            </style>
        </head>
        <body>
            <div class="container">
                <h1>Weather Forecast</h1>
                <div class="toggle">
                    <button onclick="toggleUnit()">Switch to <span id="otherUnit">°F</span></button>
                </div>
                <div class="cards" id="cards"></div>
            </div>
            <script>
                let unit = 'C';
                let data = [];
                const icons = {
                    "Freezing": "❄️", "Bracing": "🌬️", "Chilly": "🌫️", "Cool": "🍃",
                    "Mild": "🌤️", "Warm": "🌞", "Balmy": "🏖️", "Hot": "🔥", "Sweltering": "🥵", "Scorching": "🌡️"
                };
                function toF(c) { return Math.round(32 + c / 0.5556); }
                function toC(f) { return Math.round((f-32)*0.5556); }
                function render() {
                    document.getElementById("cards").innerHTML = data.map(d => `
                        <div class="card">
                            <div class="icon">${icons[d.summary]||"❓"}</div>
                            <div class="date">${d.date}</div>
                            <div class="temp">${unit=="C" ? d.temperatureC+"°C" : toF(d.temperatureC)+"°F"}</div>
                            <div class="summary">${d.summary}</div>
                        </div>
                    `).join('');
                    document.getElementById("otherUnit").innerText = unit=="C" ? "°F" : "°C";
                }
                function toggleUnit() {
                    unit = (unit === "C" ? "F" : "C");
                    render();
                }
                fetch("/weatherforecast")
                    .then(r => r.json())
                    .then(arr => {
                        data = arr.map(d => ({
                            date: d.date, temperatureC: d.temperatureC, summary: d.summary
                        }));
                        render();
                    });
            </script>
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
