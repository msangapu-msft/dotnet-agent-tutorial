using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

string[] quotes = new[]
{
    "Wherever you go, no matter what the weather, always bring your own sunshine. – Anthony J. D’Angelo",
    "Sunshine is delicious, rain is refreshing, wind braces us up, snow is exhilarating. – John Ruskin",
    "There’s no such thing as bad weather, only inappropriate clothing. – Alfred Wainwright",
    "The sound of the rain needs no translation. – Alan Watts",
    "To appreciate the beauty of a snowflake, it is necessary to stand out in the cold. – Aristotle",
    "Some people feel the rain, others just get wet. – Bob Marley",
    "After rain comes sunshine. – Proverb",
    "Rain is grace; rain is the sky descending to the earth. – John Updike"
};

// --- Root (/) serves the interactive app ---
app.MapGet("/", (HttpContext context) =>
{
    var rng = Random.Shared;
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            rng.Next(-20, 55),
            summaries[rng.Next(summaries.Length)]
        )).ToArray();

    return Results.Content(
        $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <title>Weather Forecast</title>
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; background: #f8fafc; margin: 0; padding: 0; }
                .container { max-width: 540px; margin: 40px auto; background: #fff; border-radius: 12px; box-shadow: 0 8px 24px #0002; padding: 24px; }
                h2 { color: #2563eb; font-size: 2.2em; margin: 0 0 12px 0; }
                .quote { font-style: italic; font-size: 1.13em; margin: 14px 0 26px 0; color: #475569; background: #f1f5f9; border-radius: 8px; padding: 10px 16px; }
                .forecast-table { width: 100%; border-collapse: collapse; margin: 16px 0 0 0; }
                .forecast-table th, .forecast-table td { padding: 12px; }
                .forecast-table th { color: #64748b; background: #f3f4f6; font-weight: 500; }
                .forecast-table tr:nth-child(even) { background: #f8fafc; }
                .weather-emoji { font-size: 2em; }
                .refresh-btn {
                    background: #2563eb; color: #fff; border: none; border-radius: 7px;
                    font-size: 1em; padding: 9px 23px; cursor: pointer; margin-top: 16px;
                    box-shadow: 0 1px 6px #2563eb33; transition: background 0.15s;
                }
                .refresh-btn:hover { background: #1e40af; }
            </style>
        </head>
        <body>
            <div class="container">
                <h2>Weather Forecast</h2>
                <div class="quote" id="quote"></div>
                <table class="forecast-table">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Temp (°C)</th>
                            <th>Summary</th>
                            <th>🌡️</th>
                        </tr>
                    </thead>
                    <tbody id="forecast-body">
                        <!-- Filled by JS -->
                    </tbody>
                </table>
                <button class="refresh-btn" onclick="reloadForecast()">🔄 Refresh</button>
            </div>
            <script>
                const data = {{JsonSerializer.Serialize(forecast)}};
                const summaries = {{JsonSerializer.Serialize(summaries)}};
                const quotes = {{JsonSerializer.Serialize(quotes)}};
                const weatherEmojis = {
                    "Freezing": "❄️", "Bracing": "🌬️", "Chilly": "🥶",
                    "Cool": "🧥", "Mild": "🌤️", "Warm": "🌞",
                    "Balmy": "🌴", "Hot": "🔥", "Sweltering": "🥵", "Scorching": "🌡️"
                };

                function pickRandom(arr) {
                    return arr[Math.floor(Math.random() * arr.length)];
                }

                function renderForecast() {
                    let html = "";
                    for (const d of data) {
                        const emoji = weatherEmojis[d.summary] || "🌦️";
                        html += `<tr>
                            <td>${d.date}</td>
                            <td>${d.temperatureC}</td>
                            <td>${d.summary}</td>
                            <td class="weather-emoji">${emoji}</td>
                        </tr>`;
                    }
                    document.getElementById('forecast-body').innerHTML = html;
                }

                function setRandomQuote() {
                    document.getElementById('quote').innerText = pickRandom(quotes);
                }

                function reloadForecast() {
                    fetch("/weatherforecast/json")
                        .then(r => r.json())
                        .then(arr => {
                            data.length = 0; // clear current
                            arr.forEach(o => data.push(o));
                            renderForecast();
                            setRandomQuote();
                        });
                }

                renderForecast();
                setRandomQuote();
            </script>
        </body>
        </html>
        """,
        "text/html"
    );
});

// --- AJAX-only JSON endpoint for refresh ---
app.MapGet("/weatherforecast/json", () =>
{
    var rng = Random.Shared;
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            rng.Next(-20, 55),
            summaries[rng.Next(summaries.Length)]
        )).ToArray();
    return Results.Json(forecast);
});

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

app.Run();
