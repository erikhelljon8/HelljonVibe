using HelljonVibe.Identity.Api.Configuration;
using HelljonVibe.Identity.Api.Middleware;
using HelljonVibe.Identity.Api.Services;
using HelljonVibe.Identity.Data;
using HelljonVibe.Identity.Data.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting HelljonVibe.Identity.Api...");

    // Add services to the container.
    builder.Host.UseSerilog();

    // Configuration
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<EncryptionSettings>(builder.Configuration.GetSection("EncryptionSettings"));
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    // Database Context
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? builder.Configuration.GetConnectionString("SqliteConnection");
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (connectionString.Contains("Sqlite"))
        {
            options.UseSqlite(connectionString);
        }
        else
        {
            options.UseSqlServer(connectionString);
        }
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    });

    // Data Protection
    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>();

    // Repositories and Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Services
    builder.Services.AddScoped<IEncryptionService, EncryptionService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IGdprService, GdprService>();

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.Secret))
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    // Authorization
    builder.Services.AddAuthorization(options =>
    {
        // Define policies
        options.AddPolicy(Policies.Admin, policy => policy.RequireRole(Roles.Admin));
        options.AddPolicy(Policies.UserManager, policy => policy.RequireRole(Roles.Admin, Roles.UserManager));
        options.AddPolicy(Policies.ClientManager, policy => policy.RequireRole(Roles.Admin, Roles.ClientManager));
        options.AddPolicy(Policies.Auditor, policy => policy.RequireRole(Roles.Admin, Roles.Auditor));
        options.AddPolicy(Policies.ApprovedUser, policy => policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == Claims.IsApproved && c.Value == "true")));
        options.AddPolicy(Policies.ActiveUser, policy => policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == Claims.IsActive && c.Value == "true")));
        options.AddPolicy(Policies.EmailConfirmed, policy => policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == Claims.EmailConfirmed && c.Value == "true")));
    });

    // Controllers
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
        {
            var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
            var frontendUrl = appSettings?.FrontendUrl ?? "https://localhost:3000";
            
            policy.WithOrigins(frontendUrl, "http://localhost:3000", "https://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("api", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
        });
    });

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "HelljonVibe Identity API",
            Version = "v1",
            Description = "Identity and Access Management API for HelljonVibe",
            Contact = new OpenApiContact
            {
                Name = "HelljonVibe",
                Email = "support@helljonvibe.com"
            }
        });

        // Add JWT Authentication to Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(Program));

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // HttpClient
    builder.Services.AddHttpClient();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "HelljonVibe Identity API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Middleware pipeline order is important!
    
    // 1. Exception Handling
    app.UseExceptionHandling();
    
    // 2. Security Headers
    app.UseSecurityHeaders();
    
    // 3. Request Logging
    app.UseRequestLogging();
    
    // 4. Rate Limiting
    app.UseRateLimiter();
    
    // 5. CORS
    app.UseCors("Default");
    
    // 6. HTTPS Redirection (in production)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    
    // 7. Static Files
    app.UseStaticFiles();
    
    // 8. Authentication
    app.UseAuthentication();
    
    // 9. Authorization
    app.UseAuthorization();
    
    // 10. Map Controllers
    app.MapControllers();

    // Seed Database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await Seed.InitializeAsync(services);
            Log.Information("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while seeding the database");
        }
    }

    Log.Information("HelljonVibe.Identity.Api started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
