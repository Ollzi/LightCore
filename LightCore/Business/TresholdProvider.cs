using Newtonsoft.Json;
using System.Net;
using System.Runtime.Serialization;
using System;

namespace LightCore.Business
{
    public interface ITresholdProvider
    {
        int GetTreshold();

        int GetCurrentValue();
    }

    public class TresholdProvider : ITresholdProvider
    {
        public int GetTreshold()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var client = new WebClient();
            var stringData = client.DownloadString($"https://192.168.1.48:8123/api/states/input_number.slider1?api_password={Program.Configuration["Password"]}");
            var serializer = JsonSerializer.Create();
            var state = JsonConvert.DeserializeObject<HomeAssistantState>(stringData);
            return Convert.ToInt32(System.Math.Floor(state.State));
        }

        public int GetCurrentValue()
        {
            var value = System.IO.File.ReadAllText(LightCore.Program.Configuration["TresholdFile"]);
            return Convert.ToInt32(value);
        }

        [DataContract]
        private class HomeAssistantState
        {
            [DataMember]
            [JsonProperty("state")]
            public double State { get; set; }
        }
    }
}
