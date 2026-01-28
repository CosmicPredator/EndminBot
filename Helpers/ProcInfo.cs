namespace EndfieldBot.Helpers;

using System;
using System.IO;
using System.Linq;
using System.Threading;

public class LinuxMetrics
{
    public static (long totalMb, long availableMb) GetSystemMemory()
    {
        long total = 0;
        long available = 0;

        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (line.StartsWith("MemTotal"))
                total = ParseKb(line);
            else if (line.StartsWith("MemAvailable"))
                available = ParseKb(line);
        }

        return (total / 1024, available / 1024);
    }

    public static double GetSystemCpuUsage(int delayMs = 500)
    {
        var (idle1, total1) = ReadCpuStat();
        Thread.Sleep(delayMs);
        var (idle2, total2) = ReadCpuStat();

        var idleDelta = idle2 - idle1;
        var totalDelta = total2 - total1;

        var usage = (1.0 - (double)idleDelta / totalDelta) * 100.0;
        return Math.Round(usage, 2);
    }

    public static long GetProcessMemoryMb()
    {
        foreach (var line in File.ReadLines("/proc/self/status"))
        {
            if (line.StartsWith("VmRSS"))
                return ParseKb(line) / 1024;
        }

        return 0;
    }

    public static async Task<double> GetProcessCpuUsage(int delayMs = 500)
    {
        var (startProcTime, startTotalTime) = ReadProcessCpu();
        await Task.Delay(delayMs);
        var (endProcTime, endTotalTime) = ReadProcessCpu();

        var procDelta = endProcTime - startProcTime;
        var totalDelta = endTotalTime - startTotalTime;

        var usage = 100.0 * procDelta / totalDelta * Environment.ProcessorCount;
        return Math.Round(usage, 2);
    }

    private static long ParseKb(string line)
    {
        return long.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
    }

    private static (long idle, long total) ReadCpuStat()
    {
        var parts = File.ReadLines("/proc/stat")
                        .First()
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .Select(long.Parse)
                        .ToArray();

        long idle = parts[3] + parts[4]; // idle + iowait
        long total = parts.Sum();

        return (idle, total);
    }

    private static (long procTime, long totalTime) ReadProcessCpu()
    {
        var stat = File.ReadAllText("/proc/self/stat").Split(' ');
        long utime = long.Parse(stat[13]);
        long stime = long.Parse(stat[14]);
        long procTime = utime + stime;

        var cpuParts = File.ReadLines("/proc/stat")
                           .First()
                           .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                           .Skip(1)
                           .Select(long.Parse);
        long totalTime = cpuParts.Sum();

        return (procTime, totalTime);
    }
}
