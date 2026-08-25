using HelljonVibe.Identity.Data.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelljonVibe.Identity.Data.Extentions;

/// <summary>
/// Extension methods for registering the data layer services.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds the Identity database context to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="useSqlServer">True to use SQL Server; otherwise, false.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityDbContext(
        this IServiceCollection services,
        string connectionString,
        bool useSqlServer = true) {
        if (useSqlServer) {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions => {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
        } else {
            // For SQLite or other providers, add appropriate configuration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString, sqliteOptions => {
                    sqliteOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                }));
        }

        return services;
    }

    /// <summary>
    /// Adds the Identity repositories to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityRepositories(this IServiceCollection services) {
        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register specific repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds the Identity data protection to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityDataProtection(this IServiceCollection services) {
        services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>()
            .SetApplicationName("HellMinCore.Identity");

        return services;
    }

    /// <summary>
    /// Adds the Identity database migrations to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityMigrations(this IServiceCollection services) {
        // Migrations are applied separately, this is just a placeholder
        return services;
    }
}