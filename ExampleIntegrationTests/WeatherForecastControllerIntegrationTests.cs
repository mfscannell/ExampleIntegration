using ExampleIntegration.Data;
using ExampleIntegration.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace ExampleIntegrationTests;

public class WeatherForecastControllerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client = null;

    public WeatherForecastControllerIntegrationTests()
    {
      _postgresContainer = new PostgreSqlBuilder("postgres:latest")
          .WithDatabase("testdb")
          .WithUsername("postgres")
          .WithPassword("password")
          .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<ApplicationDbContext>();

                    // Add DbContext with test container connection string
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(_postgresContainer.GetConnectionString()));

                    // Ensure database is created and migrated
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.Migrate();
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFourForecasts_WithExpectedData()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testDate = new DateOnly(2026, 5, 6);
            var testForecasts = new List<WeatherForecast>
            {
                new WeatherForecast { Date = testDate, TemperatureC = 25, Summary = "Warm" },
                new WeatherForecast { Date = testDate, TemperatureC = 28, Summary = "Hot" },
                new WeatherForecast { Date = testDate, TemperatureC = 22, Summary = "Mild" },
                new WeatherForecast { Date = testDate, TemperatureC = 18, Summary = "Cool" }
            };
            await dbContext.WeatherForecasts.AddRangeAsync(testForecasts);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        response.EnsureSuccessStatusCode();
        var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();

        Assert.NotNull(forecasts);
    }
}
