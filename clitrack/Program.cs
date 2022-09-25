using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using clitrack;
using libmtrack.Tracker;
using Pie.Audio;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.ResetColor();
    Console.Clear();
    Console.CursorVisible = true;
};

Console.CursorVisible = false;
Console.BackgroundColor = ConsoleColor.Magenta;
Console.Clear();
Console.BackgroundColor = ConsoleColor.DarkGray;
Console.ForegroundColor = ConsoleColor.Black;
Console.SetCursorPosition(0, 0);
Console.WriteLine("MTRACK CLI 1.0.0a1".PadRight(Console.WindowWidth));
Console.ResetColor();
Console.BackgroundColor = ConsoleColor.Magenta;
TrackAudio audio = new TrackAudio();

Track track = null;
try
{
    track = Loader.LoadFile(args[0]);
    audio.Play(track);
}
catch (Exception e)
{
    //Console.WriteLine(e.Message);
    Console.WriteLine(e);
    Console.ResetColor();
    Console.CursorVisible = true;
    Environment.Exit(1);
}

Stopwatch stopwatch = Stopwatch.StartNew();
Console.SetCursorPosition(0, Console.WindowHeight);
string span = TimeSpan.FromSeconds(0).ToString() + ' ';
Console.Write(span);
while (!audio.IsFinished)
{
    Thread.Sleep(1000);
    Console.SetCursorPosition(0, 1);
    Console.BackgroundColor = ConsoleColor.Magenta;
    Console.SetCursorPosition(0, Console.WindowHeight);
    int seconds = (int) (stopwatch.ElapsedMilliseconds / 1000);
    span = TimeSpan.FromSeconds(seconds).ToString() + ' ';
    Console.Write(span);
    Console.BackgroundColor = ConsoleColor.DarkGray;
    for (int i = 0; i < (int) (seconds / (float) track.Patterns[0].Rows * (Console.WindowWidth - span.Length)); i++)
        Console.Write(' ');
}

Console.ResetColor();
Console.Clear();
Console.CursorVisible = true;

audio.Dispose();