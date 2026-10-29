var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5000", "http://localhost:5180", "http://localhost:5269")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("AllowBlazorClient");
app.MapReverseProxy();
app.Run();
