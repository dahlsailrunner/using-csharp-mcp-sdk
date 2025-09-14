namespace Aspire.Hosting;

internal static class LoggingHelper
{    
    internal static IResourceBuilder<T> WithOtherOpenTelemetryService<T>(this IResourceBuilder<T> builder, string apmAuthHeader)
        where T : IResourceWithEnvironment
    {
        builder = builder.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://YOUR_REAL_OTEL_ENDPOINT");
        builder = builder.WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", apmAuthHeader);
        return builder;
    }

}
