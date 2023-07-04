namespace elasticnest.Extensions;

using elasticnest.Models;
using Nest;
public static class ElasticSearchExtensions
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ElasticConfig:Uri"];
        var defaultIndex = configuration["ElasticConfig:index"];

        var settings = new ConnectionSettings(new Uri(uri))
            .PrettyJson()
            .DefaultIndex(defaultIndex);

        AddDefaultMappings(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
        CreateIndex(client, defaultIndex);
    }

    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings.DefaultMappingFor<Product>(p => p.Ignore(x => x.Quantity));
    }
    private static void CreateIndex(IElasticClient client, String indexName = "intalio")
    {
        client.Indices.Create(indexName, i => i.Map<Product>(x => x.AutoMap()));
    }
}