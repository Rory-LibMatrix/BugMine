using LibMatrix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ServiceInstaller {
    /// <summary>
    /// Adds BugMine SDK services to the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="config">Optional BugMine SDK configuration</param>
    /// <returns>Input service collection</returns>
    public static IServiceCollection AddBugMine(this IServiceCollection services, BugMineSdkConfiguration? config = null) {
        services.AddRoryLibMatrixServices(new() {
            AppName = config?.AppName ?? "BugMine SDK app"
        });
        return services;
    }
}

/// <summary>
/// Configuration for the BugMine SDK.
/// </summary>
public class BugMineSdkConfiguration {
    public string AppName { get; set; } = "BugMine SDK app";
}