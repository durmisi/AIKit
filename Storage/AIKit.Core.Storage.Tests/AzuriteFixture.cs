using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace AIKit.Storage.Tests;

/// <summary>
/// Fixture for setting up Azurite (Azure Storage emulator) for testing.
/// </summary>
public class AzuriteFixture : IAsyncLifetime
{
    private IContainer? _container;

    /// <summary>
    /// Gets a value indicating whether Azurite is available.
    /// </summary>
    public bool IsAvailable { get; private set; }

    /// <summary>
    /// Gets the connection string for Azurite.
    /// </summary>
    public string ConnectionString => "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    /// <summary>
    /// Initializes the Azurite container asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        try
        {
            _container = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/azure-storage/azurite:3.30.0")
                .WithPortBinding(10000, 10000)
                .WithPortBinding(10001, 10001) // For queue and table if needed
                .WithEnvironment("AZURITE_SKIP_API_VERSION_CHECK", "true")
                .WithEnvironment("AZURITE_BLOB_VERSIONING_ENABLED", "true")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(10000))
                .Build();

            await _container.StartAsync();
            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    /// <summary>
    /// Disposes the Azurite container asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
}