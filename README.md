# Weather Forecast Integration

## Objective

Develop an integration to retrieve real-time weather forecasts from a public API.

## Requirements

1. Fetch the current weather forecast from a publicly accessible API.
2. Provide forecasts for Vilnius, Kaunas, and Klaipėda.
3. Generate a JSON file in the following format:

```json
{
    "Kaunas": {
        "Temperature": 0,
        "Precipitation": "Rain",
        "WindSpeed": "0 m/s"
    }
}
