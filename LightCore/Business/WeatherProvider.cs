using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;

namespace LightCore.Business
{
    public interface IWeatherProvider
    {
        DateTime GetSunsetTime();
    }

    public class WeatherProvider : IWeatherProvider
    {
        public DateTime GetSunsetTime()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.yr.no/place/Sweden/V%C3%A4sterbotten/Skellefte%C3%A5/forecast.xml");
            request.Method = "GET";
            request.Accept = "Accept=application/xml";
            request.Timeout = 5000;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    XDocument doc = XDocument.Load(reader);
                    var sunNode = doc.Descendants("sun");

                    var sunset = Convert.ToDateTime(sunNode.Attributes("set").First().Value);

                    return sunset;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            
        }
    }
}
