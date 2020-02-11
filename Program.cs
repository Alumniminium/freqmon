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
        public static bool average = false;
        public static StreamReader[] Readers = new StreamReader[Environment.ProcessorCount];
        static void Main(string[] args)
        {
            var cpus = new int[Environment.ProcessorCount];
            for (int i = 0; i < Environment.ProcessorCount; i++)
                Readers[i] = new StreamReader(File.OpenRead("/sys/devices/system/cpu/cpu" + i + "/cpufreq/scaling_cur_freq"));

            while (true)
            {
                CpuFrequencies.Clear();
                
                var totalSpeed = 0;
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    Readers[i].BaseStream.Seek(0,SeekOrigin.Begin);

                    var stringSpeed = Readers[i].ReadLine().Trim();

                    int intSpeed = int.Parse(stringSpeed);
                    cpus[i] = intSpeed;
                    totalSpeed += intSpeed;
                    var floatSpeed = (float)Math.Round(((float)intSpeed / 1000000), 1, MidpointRounding.AwayFromZero);
                    
                    if (!CpuFrequencies.ContainsKey(floatSpeed))
                        CpuFrequencies.TryAdd(floatSpeed, 1);
                    else
                        CpuFrequencies[floatSpeed]++;

                }
                if (average)
                {
                    var averageSpeed = totalSpeed / Environment.ProcessorCount;
                    var averageGhz = Math.Round(((float)averageSpeed / 1000000), 1, MidpointRounding.AwayFromZero);

                    Console.WriteLine($"{averageGhz.ToString("0.00")}Ghz");
                }
                else
                {
                    float freq = CpuFrequencies.OrderByDescending(kvp => kvp.Value).First().Key;
                    int count = CpuFrequencies.OrderByDescending(kvp => kvp.Value).First().Value;
                    var nanan = CpuFrequencies.OrderByDescending(kvp => kvp.Key).Where(kvp => kvp.Value > 0).Take(1).ToArray();

                    foreach (var kvp in nanan)
                        Console.Write($"  {kvp.Value} C @ {kvp.Key.ToString("0.0")}Ghz ");

                    Console.WriteLine();
                }
                Thread.Sleep(1000);
            }

        }
    }
}
