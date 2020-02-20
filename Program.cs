using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace FreqMon
{
    public static class Program
    {
        public static Dictionary<float, int> CpuFrequencies = new Dictionary<float, int>();
        public static StreamReader[] Readers = new StreamReader[Environment.ProcessorCount];
        static void Main(string[] args)
        {
            for (int i = 0; i < Environment.ProcessorCount; i++)
                Readers[i] = new StreamReader(File.OpenRead("/sys/devices/system/cpu/cpu" + i + "/cpufreq/scaling_cur_freq"));

            while (true)
            {
                UpdateCpuCoreFrequencies();
                var highestFreqPair = CpuFrequencies.OrderByDescending(kvp => kvp.Key).Take(1);

                foreach (var (cores, frequency) in highestFreqPair)
                    Console.WriteLine($"  {cores} C @ {frequency.ToString("0.0")}Ghz ");

                Thread.Sleep(1000);
            }
        }

        private static void UpdateCpuCoreFrequencies()
        {
            CpuFrequencies.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Readers[i].BaseStream.Seek(0, SeekOrigin.Begin);

                var stringSpeed = Readers[i].ReadLine().Trim();
                int cpuFreqHz = int.Parse(stringSpeed);

                var cpuFreqGhz = (float)Math.Round(((float)cpuFreqHz / 1000000), 1, MidpointRounding.AwayFromZero);

                if (!CpuFrequencies.ContainsKey(cpuFreqGhz))
                    CpuFrequencies.TryAdd(cpuFreqGhz, 1);
                else
                    CpuFrequencies[cpuFreqGhz]++;
            }
        }
    }
}