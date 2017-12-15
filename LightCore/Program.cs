using LightCore.Business;
using Microsoft.Extensions.Configuration;
using System;


namespace LightCore
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            var lightManager = new LightManager();
            lightManager.Run();

            Console.ReadLine();
        }
    }
}
