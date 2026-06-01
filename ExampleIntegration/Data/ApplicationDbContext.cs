using ExampleIntegration.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExampleIntegration.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<WeatherForecast> WeatherForecasts { get; set; }
}
