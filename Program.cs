using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Weather data arrays
string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
string[] icons = new[]
{
    "❄️", "🌬️", "🌧️", "🌦️", "⛅", "🌤️", "🌞", "🔥", "🌡️", "☀️"
};
string[] quotes = new[]
{
    "Wherever you go, no matter what the weather, always bring your own sunshine. — Anthony J. D’Angelo",
    "There is no such thing as bad weather, only different kinds of good weather. — John Ruskin",
    "Sunshine is delicious, rain is refreshing, wind braces us up, snow is exhilarating. — John Ruskin",
    "Some people feel the rain. Others just get wet. — Bob Marley",
    "Climate is what we expect, weather is what we get. — Mark Twain",
    "If you want to see the sunshine, you have to weather the storm. — Frank Lane"
};

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var host = context.Request.Host.Host;
    bool isBrokenSlot = host.Contains("-broken", StringComparison.OrdinalIgnoreCase);

    // Choose random weather for initial page load
    var rng = new Random();
    int idx = rng.Next(summaries.Length);
    int tempC = rng.Next(-10, 40);
    string summary = summaries[idx];
    string icon = icons[idx];
    string quote = quotes[rng.Next(quotes.Length)];

    // Start HTML
    await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Fun .NET Weather Dashboard</title>
    <style>
        body {{
            background: #f8fafc;
            font-family: 'Segoe UI', Arial, sans-serif;
            text-align: center;
            margin: 0; padding: 0;
        }}
        .container {{
            margin-top: 80px;
            background: #fff;
            border-radius: 18px;
            box-shadow: 0 6px 24px rgba(0,0,0,0.08);
            display: inline-block;
            padding: 40px 36px 36px 36px;
        }}
        .weather-icon {{
            font-size: 4.5em;
        }}
        .temp {{
            font-size: 2.5em;
            color: #2563eb;
        }}
        .summary {{
            font-size: 1.5em;
            margin-top: 8px;
            color: #555;
        }}
        .quote {{
            margin-top: 22px;
            font-size: 1.2em;
            color: #008080;
        }}
        .warning {{
            margin-top: 30px;
            color: #b91c1c;
            font-weight: bold;
            font-size: 1.3em;
        }}
        .slotnote {{
            margin-top: 12px;
            color: #ad6800;
            font-size: 1em;
        }}
        button {{
            margin-top: 30px;
            background: #2563eb;
            color: #fff;
            border: none;
            border-radius: 6px;
            font-size: 1.2em;
            padding: 12px 28px;
            cursor: pointer;
            transition: background 0.2s;
        }}
        button:hover {{
            background: #174bb3;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='weather-icon' id='icon'>{icon}</div>
        <div class='temp'><span id='temp'>{tempC}</span>&deg;C</div>
        <div class='summary' id='summary'>{summary}</div>
        <div class='quote' id='quote'>{quote}</div>
        <button onclick='refreshWeather()'>Refresh</button>
        {(isBrokenSlot ? "<div class='warning'>BROKEN SLOT: Simulating memory exhaustion!<br/>Page may crash.</div>" : "")}
        {(isBrokenSlot ? "<div class='slotnote'>Note: For the demo to work, your deployment slot <b>MUST</b> be named <code>broken</code>!</div>" : "")}
    </div>
    <script>
        const summaries = {JsonSerializer.Serialize(summaries)};
        const icons = {JsonSerializer.Serialize(icons)};
        const quotes = {JsonSerializer.Serialize(quotes)};
        function refreshWeather() {{
            let idx = Math.floor(Math.random() * summaries.length);
            let temp = Math.floor(Math.random() * 51) - 10; // -10 to 40
            document.getElementById('icon').textContent = icons[idx];
            document.getElementById('summary').textContent = summaries[idx];
            document.getElementById('temp').textContent = temp;
            document.getElementById('quote').textContent = quotes[Math.floor(Math.random() * quotes.length)];
        }}
    </script>
</body>
</html>
    ");

    // Flush response so user sees it before crash
    await context.Response.Body.FlushAsync();

    // Simulate memory exhaustion for the broken slot (AFTER writing HTML!)
    if (isBrokenSlot)
    {
        await Task.Delay(1100); // Let browser render the warning
        List<byte[]> memoryLeak = new();
        for (int i = 0; i < 50_000; i++)
        {
            memoryLeak.Add(new byte[1024 * 1024]); // Allocate 1MB each (~50GB)
        }
        // In the unlikely event the process doesn't crash
        await context.Response.WriteAsync("<div style='color:green'>Memory bug did not work!</div>");
    }
});

app.Run();
