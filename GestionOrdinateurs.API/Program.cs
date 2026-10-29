using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection with the MySQL connection details.");
}

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {token}"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Configure Jwt:Key with at least 32 bytes.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuthentication");
                logger.LogInformation(
                    "Utilisateur utilisant un token valide : {User}",
                    context.Principal?.Identity?.Name ?? "Anonymous");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuthentication");
                logger.LogWarning(context.Exception, "Token JWT invalide.");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5000", "http://localhost:5180")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddDbContextFactory<ParcInformatiqueContext>(options =>
    options.UseMySQL(connectionString));
builder.Services.AddScoped<ParcInformatiqueContext>(services =>
    services.GetRequiredService<IDbContextFactory<ParcInformatiqueContext>>().CreateDbContext());
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<JournalService>(_ =>
    new JournalService(builder.Environment.ContentRootPath));
builder.Services.AddScoped<ReparationService>();
builder.Services.AddScoped<StockService>();
var app = builder.Build();

// Applique les migrations puis crée les comptes de démonstration à la première exécution.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login = "admin", Role = RoleEnum.Admin },
            new User { Login = "utilisateur 1", Role = RoleEnum.Gestionnaire },
            new User { Login = "Utilisateur 2", Role = RoleEnum.Gestionnaire });

        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();