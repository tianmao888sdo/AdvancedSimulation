using System.Diagnostics;
using System;

public class Watch
{
    private static Stopwatch watch=new Stopwatch();

    public static void Start()
    {
        watch.Reset();
        watch.Start();
    }

    public static void Stop()
    {
        watch.Stop();
    }

    public static double Duration
    {
        get
        {
            TimeSpan sp = watch.Elapsed;
            return sp.TotalMilliseconds;
        }
    }
}
