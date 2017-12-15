using LightCore.Contracts;
using System;
using System.IO;
using System.Net;

namespace LightCore.Business
{
    public class TelldusManager : ITelldus
    {
        private enum ApiAction
        {
            turn_on,
            turn_off
        }

        public void TurnOff(string section)
        {
            CallHomeAssistantApi(ApiAction.turn_off, section);
        }

        public void TurnOn(string section)
        {
            CallHomeAssistantApi(ApiAction.turn_on, section);
        }

        private void CallHomeAssistantApi(ApiAction action, string section)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var url = $"https://192.168.1.48:8123/api/services/switch/{action.ToString()}?api_password={Program.Configuration["Password"]}";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            //httpWebRequest.Headers.Add("x-ha-access", "");
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = $"{{\"entity_id\": \"switch.{section}\"}}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Misslyckades med anrop {httpResponse.ResponseUri}");
                }
            }
        }
    }
}
