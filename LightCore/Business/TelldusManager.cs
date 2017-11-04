using LightCore.Contracts;
using System.Diagnostics;

namespace LightCore.Business
{
    public class TelldusManager : ITelldus
    {
        public void TurnOff(string section)
        {
            var processStartInfo = new ProcessStartInfo(@"tdtool", $"--off {section}")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            RepeatProcess(5, processStartInfo);
        }

        public void TurnOn(string section)
        {
            var processStartInfo = new ProcessStartInfo(@"tdtool", $"--on {section}")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            RepeatProcess(3, processStartInfo);
        }

        private void RepeatProcess(int numberOfTimes, ProcessStartInfo processStartInfo)
        {
            int count = 0;

            while (count <= numberOfTimes)
            {
                var process = Process.Start(processStartInfo);
                process?.WaitForExit();

                System.Threading.Thread.Sleep(200);
                count++;
            }
        }
    }
}
