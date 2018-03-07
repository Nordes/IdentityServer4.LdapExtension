using System.Collections.Generic;
using MvcVueClient.Models;

namespace MvcVueClient.Providers
{
    public interface IWeatherProvider
    {
        List<WeatherForecast> GetForecasts();
    }
}
