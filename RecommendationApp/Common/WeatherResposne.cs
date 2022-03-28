using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common
{

    public partial class WeatherResposne
    {
        [JsonProperty("request")]
        public Request Request { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("current")]
        public Current Current { get; set; }
    }

    public partial class Current
    {
        [JsonProperty("observation_time")]
        public string ObservationTime { get; set; }

        [JsonProperty("temperature")]
        public long Temperature { get; set; }

        [JsonProperty("weather_code")]
        public long WeatherCode { get; set; }

        [JsonProperty("weather_icons")]
        public List<Uri> WeatherIcons { get; set; }

        [JsonProperty("weather_descriptions")]
        public List<string> WeatherDescriptions { get; set; }

        [JsonProperty("wind_speed")]
        public long WindSpeed { get; set; }

        [JsonProperty("wind_degree")]
        public long WindDegree { get; set; }

        [JsonProperty("wind_dir")]
        public string WindDir { get; set; }

        [JsonProperty("pressure")]
        public long Pressure { get; set; }

        [JsonProperty("precip")]
        public long Precip { get; set; }

        [JsonProperty("humidity")]
        public long Humidity { get; set; }

        [JsonProperty("cloudcover")]
        public long Cloudcover { get; set; }

        [JsonProperty("feelslike")]
        public long Feelslike { get; set; }

        [JsonProperty("uv_index")]
        public long UvIndex { get; set; }

        [JsonProperty("visibility")]
        public long Visibility { get; set; }

        [JsonProperty("is_day")]
        public string IsDay { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("lon")]
        public string Lon { get; set; }

        [JsonProperty("timezone_id")]
        public string TimezoneId { get; set; }

        [JsonProperty("localtime")]
        public string Localtime { get; set; }

        [JsonProperty("localtime_epoch")]
        public long LocaltimeEpoch { get; set; }

        [JsonProperty("utc_offset")]
        public string UtcOffset { get; set; }
    }

    public partial class Request
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }

    public partial class WeatherResposne
    {
        public static WeatherResposne FromJson(string json) => JsonConvert.DeserializeObject<WeatherResposne>(json);
    }

    public static class Serialize
    {
        public static string ToJson(this WeatherResposne self) => JsonConvert.SerializeObject(self);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
        };
    }
    

}
