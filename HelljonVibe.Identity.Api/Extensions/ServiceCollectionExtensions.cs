using HelljonVibe.Identity.Api.Services;
using Microsoft.IdentityModel.Tokens;

namespace HelljonVibe.Identity.Api.Extensions;

/// <summary>
/// Extension methods for registering API services.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds the Identity API services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services) {
        // Encryption
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Token service
        services.AddScoped<ITokenService, TokenService>();

        // Auth service
        services.AddScoped<IAuthService, AuthService>();

        // Email service
        services.AddScoped<IEmailService, EmailService>();

        // User service
        services.AddScoped<IUserService, UserService>();

        // Client service
        services.AddScoped<IClientService, ClientService>();

        // Role service
        services.AddScoped<IRoleService, RoleService>();

        // Audit service
        services.AddScoped<IAuditService, AuditService>();

        // GDPR services
        services.AddScoped<IGdprService, GdprService>();

        return services;
    }

    /// <summary>
    /// Adds the Identity API validators to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityValidators(this IServiceCollection services) {
        // Register validators
        services.AddScoped<Validators.RegisterRequestValidator>();
        services.AddScoped<Validators.LoginRequestValidator>();
        services.AddScoped<Validators.ForgotPasswordRequestValidator>();
        services.AddScoped<Validators.ResetPasswordRequestValidator>();

        return services;
    }

    /// <summary>
    /// Adds the Identity API configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration) {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<EncryptionSettings>(configuration.GetSection("Encryption"));
        services.Configure<AppSettings>(configuration.GetSection("App"));

        return services;
    }
}
