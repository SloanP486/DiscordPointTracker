using System;

namespace PointTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot.PointTracker pointTracker = new Bot.PointTracker();
            pointTracker.BotMain().GetAwaiter().GetResult();
        }
    }
}
