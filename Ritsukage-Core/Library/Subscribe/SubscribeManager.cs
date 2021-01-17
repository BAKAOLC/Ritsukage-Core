using Ritsukage.Library.Subscribe.Listener;
using Ritsukage.Library.Subscribe.Listener.Base;
using Ritsukage.Tools.Console;
using System.Collections.Generic;
using System.Threading;

namespace Ritsukage.Library.Subscribe
{
    public static class SubscribeManager
    {
        static readonly Dictionary<SubscribeListener, int> Listeners = new();

        static bool _init = false;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            Listeners.Add(new BilibiliLiveListener(), 30000);
            Listeners.Add(new BilibiliDynamicListener(), 60000);
            CreateListenerThread();
        }

        static void CreateListenerThread()
        {
            ConsoleLog.Debug("Subscribe", "Start loading...");
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(20000);
                    foreach (var listener in Listeners)
                    {
                        ConsoleLog.Debug("Subscribe", "Refresh subscribe listener: " + listener.Key.GetType().FullName);
                        listener.Key.RefreshListener();
                    }
                }
            })
            {
                IsBackground = true
            }.Start();
            foreach (var listener in Listeners)
            {
                ConsoleLog.Debug("Subscribe", $"Register subscribe listener: {listener.Key.GetType().FullName} with {listener.Value} ms delay");
                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(listener.Value);
                        ConsoleLog.Debug("Subscribe", "Trigger subscribe listener: " + listener.Key.GetType().FullName);
                        listener.Key.Listen();
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
            ConsoleLog.Debug("Subscribe", "Finish.");
        }
    }
}
