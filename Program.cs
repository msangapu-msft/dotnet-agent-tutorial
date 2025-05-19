using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

bool hasWarmedUp = false; // For slot swap compatibility

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var host = context.Request.Host.Host;
    bool isBrokenSlot = host.Contains("broken", StringComparison.OrdinalIgnoreCase);
    bool safeMode = context.Request.Query.ContainsKey("safe");

    // Random number for initial page load
    var rng = new Random();
    int number = rng.Next(1, 10001); // 1 to 10000

    // Button color based on slot
    string buttonColor = isBrokenSlot ? "#dc2626" : "#22c55e";   // Red or green
    string buttonHover = isBrokenSlot ? "#b91c1c" : "#15803d";   // Dark red or green
    string buttonText = isBrokenSlot ? "Throw Exception" : "Refresh";

    // Simulate memory exhaustion for the broken slot (AFTER writing HTML!)
    if (isBrokenSlot && !safeMode)
    {
        if (!hasWarmedUp)
        {
            hasWarmedUp = true; // allow warmup for slot swap
        }
        else
        {
            throw new Exception("Simulated memory exhaustion: Out of memory!"); // causes HTTP 500
        }
    }


    await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>.NET Random Number Demo</title>
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
        .number {{
            font-size: 3.2em;
            color: #2563eb;
            margin-bottom: 18px;
        }}
        .note {{
            margin-top: 12px;
            color: #ad6800;
            font-size: 1em;
        }}
        .warning {{
            margin-top: 30px;
            color: #b91c1c;
            font-weight: bold;
            font-size: 1.3em;
        }}
        button {{
            margin-top: 30px;
            background: {buttonColor};
            color: #fff;
            border: none;
            border-radius: 6px;
            font-size: 1.2em;
            padding: 12px 28px;
            cursor: pointer;
            transition: background 0.2s;
        }}
        button:disabled {{
            opacity: 0.5;
            cursor: not-allowed;
        }}
        button:hover:enabled {{
            background: {buttonHover};
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='number' id='randNum'>{number}</div>
        <button id='refreshBtn' onclick='window.location.reload()' disabled>{buttonText}</button>
        {(isBrokenSlot ? "<div class='warning'>BROKEN SLOT: Simulating memory exhaustion!<br/>Page may crash.</div>" : "")}
        <div class='note'>Note: For the demo to work, your deployment slot <b>MUST</b> be named <code>broken</code>!</div>
    </div>
    <script>
        window.onload = () => {{
            document.getElementById('refreshBtn').disabled = false;
        }};
    </script>
</body>
</html>
    ");

    await context.Response.Body.FlushAsync();


}
);

app.Run();
