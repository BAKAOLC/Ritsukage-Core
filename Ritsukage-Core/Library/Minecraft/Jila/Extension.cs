using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.Minecraft.Jila
{
    public static class JiraExtension
    {
        public static Issue[] GetMCFixedIssues(DateTime from, DateTime to)
            => Issue.GetIssues($"project = MC AND resolution = Fixed AND resolved > \"{from:yyyy-MM-dd HH:mm}\" AND resolved < \"{to:yyyy-MM-dd HH:mm}\" ORDER BY resolved ASC, updated DESC, created DESC");

        public static Issue[] GetMCFixedIssues(DateTime date)
            => GetMCFixedIssues(date.Date, date.Date.AddDays(1));
    }
}
