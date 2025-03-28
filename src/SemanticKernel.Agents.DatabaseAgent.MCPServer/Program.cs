﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel;
using ModelContextProtocol;
using SemanticKernel.Agents.DatabaseAgent.MCPServer.Configuration;
using SemanticKernel.Agents.DatabaseAgent.MCPServer.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Agents.DatabaseAgent.MCPServer;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);

        IConfiguration configuration = builder.Configuration
                                                .AddEnvironmentVariables()
                                                .AddCommandLine(args)
                                                .Build();

        var memory = ConfigureMemory(configuration);
        var kernel = ConfigureKernel(configuration);

        var agent = await DatabaseAgentFactory.CreateAgentAsync(kernel, memory);

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport();

        await builder
            .Build()
            .RunAsync();
    }


    static IKernelMemory ConfigureMemory(IConfiguration configuration)
    {
        var memorySettings = configuration.GetSection("memory").Get<MemorySettings>()!;

        var memoryBuilder = new KernelMemoryBuilder();

        switch (memorySettings.Kind)
        {
            case MemorySettings.StorageType.Volatile:
                memoryBuilder.WithSimpleTextDb(new SimpleTextDbConfig()
                {
                    StorageType = FileSystemTypes.Volatile
                });
                memoryBuilder.WithSimpleFileStorage(new SimpleFileStorageConfig()
                {
                    StorageType = FileSystemTypes.Volatile
                });
                break;
            case MemorySettings.StorageType.Disk:
                memoryBuilder.WithSimpleTextDb(new SimpleTextDbConfig()
                {
                    StorageType = FileSystemTypes.Disk,
                    Directory = Path.Combine(memorySettings.Path, "index-data")
                });
                memoryBuilder.WithSimpleFileStorage(new SimpleFileStorageConfig()
                {
                    StorageType = FileSystemTypes.Disk,
                    Directory = Path.Combine(memorySettings.Path, "file-data")
                });
                break;
        }

        return memoryBuilder
                .AddCompletionServiceFromConfiguration(configuration, memorySettings.Completion)
                .AddTextEmbeddingFromConfiguration(configuration, memorySettings.Embedding)
                .Build();
    }

    static Kernel ConfigureKernel(IConfiguration configuration)
    {
        var kernelSettings = configuration.GetSection("kernel").Get<KernelSettings>()!;
        var databaseSettings = configuration.GetSection("database").Get<DatabaseSettings>()!;

        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.Services
                    .UseDatabaseAgentQualityAssurance();

        kernelBuilder.Services.AddScoped(sp => DbConnectionFactory.CreateDbConnection(databaseSettings.ConnectionString, databaseSettings.Provider));

        return kernelBuilder
                     .AddTextEmbeddingFromConfiguration(configuration, kernelSettings.Embedding)
                     .AddCompletionServiceFromConfiguration(configuration, kernelSettings.Completion)
                     .Build();
    }
}
