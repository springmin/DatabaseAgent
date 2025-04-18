﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using SemanticKernel.Agents.DatabaseAgent.Internals;
using SemanticKernel.Agents.DatabaseAgent.MCPServer.Configuration;
using SemanticKernel.Agents.DatabaseAgent.MCPServer.Extensions;

namespace SemanticKernel.Agents.DatabaseAgent.MCPServer.Internals;

internal static class AgentKernelFactory
{
    private static VectorStoreRecordDefinition GetVectorStoreRecordDefinition(int vectorDimensions = 1536)
    {
        return new()
        {
            Properties = new List<VectorStoreRecordProperty>
            {
                new VectorStoreRecordDataProperty(nameof(TableDefinitionSnippet.TableName), typeof(string)),
                new VectorStoreRecordKeyProperty(nameof(TableDefinitionSnippet.Key), typeof(Guid)),
                new VectorStoreRecordDataProperty(nameof(TableDefinitionSnippet.Definition), typeof(string)),
                new VectorStoreRecordDataProperty(nameof(TableDefinitionSnippet.Description), typeof(string)),
                new VectorStoreRecordVectorProperty(nameof(TableDefinitionSnippet.TextEmbedding), typeof(ReadOnlyMemory<float>)) 
                {
                    Dimensions = vectorDimensions
                }
            }
        };
    }

    private static VectorStoreRecordDefinition GetAgentStoreRecordDefinition(int vectorDimensions = 1536)
    {
        return new()
        {
            Properties = new List<VectorStoreRecordProperty>
            {
                new VectorStoreRecordDataProperty(nameof(AgentDefinitionSnippet.AgentName), typeof(string)),
                new VectorStoreRecordKeyProperty(nameof(AgentDefinitionSnippet.Key), typeof(Guid)),
                new VectorStoreRecordDataProperty(nameof(AgentDefinitionSnippet.Description), typeof(string)),
                new VectorStoreRecordDataProperty(nameof(AgentDefinitionSnippet.Instructions), typeof(string)),
                new VectorStoreRecordVectorProperty(nameof(AgentDefinitionSnippet.TextEmbedding), typeof(ReadOnlyMemory<float>))
                {
                    Dimensions = vectorDimensions
                }
            }
        };
    }

    internal static Kernel ConfigureKernel(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var kernelSettings = configuration.GetSection("kernel").Get<KernelSettings>()!;
        var databaseSettings = configuration.GetSection("database").Get<DatabaseSettings>()!;
        var memorySection = configuration.GetSection("memory");

        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.Services
                    .UseDatabaseAgentQualityAssurance(opts =>
                    {
                        configuration.GetSection("agent:qualityAssurance").Bind(opts);
                    });

        kernelBuilder.Services.AddScoped(sp => DbConnectionFactory.CreateDbConnection(databaseSettings.ConnectionString, databaseSettings.Provider));

        kernelBuilder.Services.Configure<DatabasePluginOptions>(options =>
        {
            configuration.GetSection("memory")
                            .Bind(options);
        });

        var memorySettings = memorySection.Get<MemorySettings>();

        loggerFactory.CreateLogger(nameof(AgentKernelFactory))
                .LogInformation("Using memory kind {kind}", memorySettings!.Kind);

        switch (memorySettings.Kind)
        {
            case MemorySettings.StorageType.Volatile:
                kernelBuilder.AddInMemoryVectorStoreRecordCollection<Guid, AgentDefinitionSnippet>("agent");
                kernelBuilder.AddInMemoryVectorStoreRecordCollection<Guid, TableDefinitionSnippet>("tables");
                break;
            case MemorySettings.StorageType.SQLite:
                var sqliteSettings = memorySection.Get<SQLiteMemorySettings>()!;
                kernelBuilder.Services.AddSqliteVectorStoreRecordCollection<Guid, AgentDefinitionSnippet>("agent", 
                    sqliteSettings.ConnectionString,
                    options: new SqliteVectorStoreRecordCollectionOptions<AgentDefinitionSnippet>()
                    {
                        VectorStoreRecordDefinition = GetAgentStoreRecordDefinition(sqliteSettings.Dimensions)
                    });
                kernelBuilder.Services.AddSqliteVectorStoreRecordCollection<Guid, TableDefinitionSnippet>("tables",
                    sqliteSettings.ConnectionString,
                    options: new SqliteVectorStoreRecordCollectionOptions<TableDefinitionSnippet>()
                    {
                        VectorStoreRecordDefinition = GetVectorStoreRecordDefinition(sqliteSettings.Dimensions)
                    });
                break;
            case MemorySettings.StorageType.Qdrant:
                var qdrantSettings = memorySection.Get<QdrantMemorySettings>()!;
                kernelBuilder.AddQdrantVectorStoreRecordCollection<Guid, AgentDefinitionSnippet>("agent",
                            qdrantSettings.Host,
                            qdrantSettings.Port,
                            qdrantSettings.Https,
                            qdrantSettings.APIKey,
                            new QdrantVectorStoreRecordCollectionOptions<AgentDefinitionSnippet>
                            {
                                VectorStoreRecordDefinition = GetAgentStoreRecordDefinition(qdrantSettings.Dimensions)
                            });
                kernelBuilder.AddQdrantVectorStoreRecordCollection<Guid, TableDefinitionSnippet>("tables",
                            qdrantSettings.Host,
                            qdrantSettings.Port,
                            qdrantSettings.Https,
                            qdrantSettings.APIKey,
                            new QdrantVectorStoreRecordCollectionOptions<TableDefinitionSnippet>
                            {
                                VectorStoreRecordDefinition = GetVectorStoreRecordDefinition(qdrantSettings.Dimensions)
                            });
                break;
            default: throw new ArgumentException($"Unknown storage type '{memorySection.Get<MemorySettings>()!.Kind}'");
        }

        _ = kernelBuilder.Services.AddSingleton<IPromptProvider, EmbeddedPromptProvider>();

        return kernelBuilder
                     .AddTextEmbeddingFromConfiguration(configuration, kernelSettings.Embedding, loggerFactory)
                     .AddCompletionServiceFromConfiguration(configuration, kernelSettings.Completion, loggerFactory)
                     .Build();
    }
}
