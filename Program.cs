using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var host = context.Request.Host.Host;
    bool isBrokenSlot = host.Contains("broken", StringComparison.OrdinalIgnoreCase);
    bool safeMode = context.Request.Query.ContainsKey("safe");
    bool buttonPressed = context.Request.Query.ContainsKey("crash");

    // Track button presses via cookie
    int pressCount = 0;
    if (context.Request.Cookies.TryGetValue("crashCount", out var cookieVal))
        int.TryParse(cookieVal, out pressCount);

    // If the button was pressed, increment the count
    if (buttonPressed)
        pressCount++;

    // Set the cookie for next round (expires in 1 hour)
    context.Response.Cookies.Append("crashCount", pressCount.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddHours(1) });

    // On broken slot, after 5 clicks, throw exception
    if (isBrokenSlot && !safeMode && buttonPressed && pressCount > 5)
    {
        throw new Exception("Simulated error after 5 button clicks!");
    }

    // Button color and label
    string buttonColor = isBrokenSlot ? "#dc2626" : "#22c55e";
    string buttonHover = isBrokenSlot ? "#b91c1c" : "#15803d";
    string buttonText = isBrokenSlot ? "Throw Exception" : "Refresh";

    string warningText = isBrokenSlot
        ? "<div class='warning'>BROKEN SLOT: Simulated error will occur after 5 clicks.<br/>This is for troubleshooting demos.</div>"
        : "";

    await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>.NET Button Click Demo</title>
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
        <div class='number' id='counter'>{pressCount}</div>
        <form method='GET'>
            <input type='hidden' name='crash' value='1' />
            <button id='refreshBtn' type='submit'>{buttonText}</button>
        </form>
        {(isBrokenSlot ? $"<div class='note'>Button clicked <b>{pressCount}</b> times (error on 6th click).</div>" : "")}
        {warningText}
        <div class='note'>Note: For the demo to work, your deployment slot <b>MUST</b> be named <code>broken</code>!</div>
    </div>
</body>
</html>
    ");
});

app.Run();
