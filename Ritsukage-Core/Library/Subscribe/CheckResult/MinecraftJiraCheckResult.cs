using Ritsukage.Library.Minecraft.Jila;
using System;

namespace Ritsukage.Library.Subscribe.CheckResult
{
    public class MinecraftJiraCheckResult : Base.SubscribeCheckResult
    {
        public DateTime From { get; init; }

        public DateTime To { get; init; }

        public Issue[] Data { get; init; }
    }
}
