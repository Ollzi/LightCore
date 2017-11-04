using LightCore.Business;
using System;

namespace LightCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var lightManager = new LightManager();
            lightManager.Run();

            Console.ReadLine();
        }
    }
}
