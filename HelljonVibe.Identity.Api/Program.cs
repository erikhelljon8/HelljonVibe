using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();


try {
    Log.Information("Starting HelljonVibe.Identity.Api...");

    // Add services to the container.
    builder.Host.UseSerilog();

    // Configuration


    // Database





} catch {

} finally {

}