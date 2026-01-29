namespace Eroad.BFF.IntegrationTest;

/// <summary>
/// Test fixture for BFF Gateway integration tests.
/// Provides HttpClient configured to call the Docker-hosted BFF Gateway.
/// </summary>
public class BFFTestFixture : IDisposable
{
    public HttpClient HttpClient { get; }
    public string BaseUrl { get; }

    public BFFTestFixture()
    {
        // Point to the BFF Gateway running in Docker
        BaseUrl = Environment.GetEnvironmentVariable("BFF_BASE_URL") ?? "http://localhost:5000";
        
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
    }
}
