using ExampleIntegration.Data;
using ExampleIntegration.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ExampleIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];
        private readonly ApplicationDbContext _dbContext;

        public WeatherForecastController(ApplicationDbContext dbContext)
        {
          _dbContext = dbContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public ActionResult<IEnumerable<WeatherForecast>> Get()
        {
            var result = _dbContext.WeatherForecasts.ToList();

            return Ok(result);
        }
    }
}
