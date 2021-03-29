using Ritsukage.Library.Subscribe.Listener;
using Ritsukage.Library.Subscribe.Listener.Base;
using Ritsukage.Tools.Console;
using System.Collections.Generic;
using System.Threading;

namespace Ritsukage.Library.Subscribe
{
    public static class SubscribeManager
    {
        const int RefreshDelay = 20 * 1000;

        static readonly Dictionary<SubscribeListener, int> Listeners = new();

        static bool _init = false;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            Listeners.Add(new BilibiliLiveListener(), 3 * 60 * 1000);
            Listeners.Add(new BilibiliDynamicListener(), 8 * 60 * 1000);
            Listeners.Add(new MinecraftVersionListener(), 60 * 1000);
            Listeners.Add(new MinecraftJiraListener(), 20 * 60 * 1000);
            CreateRefreshThread();
            CreateListenerThread();
        }

        static void CreateRefreshThread()
            => new Thread(() =>
            {
                Thread.Sleep(5 * 1000);
                while (true)
                {
                    foreach (var listener in Listeners)
                    {
                        ConsoleLog.Debug("Subscribe", "Refresh subscribe listener: " + listener.Key.GetType().FullName);
                        listener.Key.RefreshListener();
                    }
                    Thread.Sleep(RefreshDelay);
                }
            })
            {
                IsBackground = true
            }.Start();

        static int __index = 0;
        static void CreateListenerThread(SubscribeListener listener, int delay = 60000)
        {
            ConsoleLog.Debug("Subscribe", $"Register subscribe listener: {listener.GetType().FullName} with {delay} ms delay");
            int startDelay = (++__index) * 10 * 1000;
            new Thread(() =>
            {
                Thread.Sleep(startDelay);
                while (true)
                {
                    Thread.Sleep(delay);
                    ConsoleLog.Debug("Subscribe", "Trigger subscribe listener: " + listener.GetType().FullName);
                    listener.Listen();
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        static void CreateListenerThread()
        {
            ConsoleLog.Debug("Subscribe", "Start loading...");
            foreach (var listener in Listeners)
                CreateListenerThread(listener.Key, listener.Value);
            ConsoleLog.Debug("Subscribe", "Finish.");
        }
    }
}
