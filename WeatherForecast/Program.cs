using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

class Program
{
    static async Task Main(string[] args)
    {
        // Užduočiai atlikti naudosiu https://api.meteo.lt/
        // Miestų sąrašas, kurių orų informaciją norime gauti
        string[] cities = { "Vilnius", "Kaunas", "Klaipėda" };
        // Dictionary saugoti orų informaciją kiekvienam miestui
        Dictionary<string, WeatherInfo> weatherData = new Dictionary<string, WeatherInfo>();

        // Naudodamiesi HttpClient gaunam orų prognozę iš API
        using (var httpClient = new HttpClient())
        {
            foreach (var city in cities)
            {
                try
                {
                    // API URL konstravimas su nurodytu miestu
                    string apiUrl = $"https://api.meteo.lt/v1/places/{city}/forecasts/long-term";

                    // Siuntimo užklausa į API, kad gauti orų prognozę
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    // Gaunamo atsakymo turinio nuskaitymas
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Deserializuojame gautą JSON į ForecastResponse objektą
                    var forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(responseBody);

                    // Ištraukiama reikalingą orų informaciją
                    // Nustatome paskutinį laiko žymą(lastTimestamp), kuri dažniausiai atspindi artimiausią prognozuojamą laikotarpį
                    // Taigi, naudojame "forecastResponse.forecastTimestamps.Count - 1", kad pasiektume paskutinę orų prognozės žymą
                    var lastTimestamp = forecastResponse.forecastTimestamps[forecastResponse.forecastTimestamps.Count - 1];
                    // Jei bendras kritulių kiekis yra daugiau nei nulis, nustatome kintamojo 'precipitationType' reikšmę į "Rain"
                    // kitaip, jei kritulių nėra, nustatome 'precipitationType' reikšmę į "None"
                    string precipitationType = lastTimestamp.totalPrecipitation > 0 ? "Rain" : "None";

                    // Sukuriamas objektas ir tvarkingai sudedama visa reikalinga informacija
                    var weatherInfo = new WeatherInfo
                    {
                        Temperature = lastTimestamp.airTemperature,
                        Precipitation = precipitationType,
                        WindSpeed = lastTimestamp.windSpeed
                    };

                    // Paruoštus duomenys pridedame prie Dictionary
                    weatherData.Add(city, weatherInfo);
                }
                catch (Exception ex)
                {
                    // Klaidos spausdinimas, jei nepavyksta gauti orų duomenų...
                    Console.WriteLine($"Error fetching weather data for {city}: {ex.Message}");
                }
            }
        }

        // Užtikrina, kad neASCII simboliai nebus pakeisti užkoduotam JSON pvz.: \u0117 yra ė 
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Serializuoti gauta Dictionary į JSON
        string json = JsonSerializer.Serialize(weatherData, jsonOptions);

        // Įrašo JSON į failą
        File.WriteAllText("WeatherForecast.json", json);
    }
}

// Sukurta klasė WeatherInfo skirta saugoti vienos orų prognozės informaciją
// Ši klasė turės tris savybes: Temperatūrą (Temperature), Kritulių tipą (Precipitation) ir Vėjo greitį (WindSpeed)
public class WeatherInfo
{
    public double Temperature { get; set; }
    public string Precipitation { get; set; }
    public double WindSpeed { get; set; }
}

// Sukurta klasė ForecastResponse skirta deserializuoti gautą orų prognozės atsakymą iš API į C# objektą
// Ši klasė turės vieną savybę: Sąrašą (List) žymių (ForecastTimestamp) orų prognozei
public class ForecastResponse
{
    public List<ForecastTimestamp> forecastTimestamps { get; set; }
}

// Sukurta klasė ForecastTimestamp skirta saugoti vienos orų prognozės žymos informaciją
// Ši klasė turės tris savybes: Oro temperatūrą (airTemperature), Vėjo greitį (windSpeed) ir Bendrą kritulių kiekį (totalPrecipitation).
public class ForecastTimestamp
{
    public double airTemperature { get; set; }
    public double windSpeed { get; set; }
    public double totalPrecipitation { get; set; }
}
