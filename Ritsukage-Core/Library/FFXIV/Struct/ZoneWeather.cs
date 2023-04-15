using Ritsukage.Library.FFXIV.Data;
using System;

namespace Ritsukage.Library.FFXIV.Struct
{
    public struct ZoneWeather
    {
        public const int MillisecondsPerEorzeaHour = 175000;
        public const int SecondsPerEorzeaHour = MillisecondsPerEorzeaHour / 1000;
        public const int MillisecondsPerEorzeaWeather = 8 * MillisecondsPerEorzeaHour;
        public const int MillisecondsPerEorzeaDay = 24 * MillisecondsPerEorzeaHour;
        public const int SecondsPerEorzeaDay = MillisecondsPerEorzeaDay / 1000;

        public struct ZoneWeatherStep
        {
            public int ZoneID { get; init; }
            public int WeatherID { get; init; }
            public EorzeaTime BeginTime { get; init; }
            public EorzeaTime EndTime { get; init; }

            public ZoneWeatherStep(int zoneID, int weatherID, EorzeaTime beginTime)
            {
                ZoneID = zoneID;
                WeatherID = weatherID;
                BeginTime = beginTime;
                EndTime = new((BeginTime.UnixTime * 1000 + MillisecondsPerEorzeaWeather) / 1000);
            }
        }

        public int ZoneID { get; init; }
        readonly int WeatherRateIndex;

        public ZoneWeather(int zoneID)
        {
            ZoneID = zoneID;
            WeatherRateIndex = ZoneWeatherIndex.GetZoneWeatherIndex(ZoneID);
        }

        public ZoneWeatherStep GetWeather()
            => GetWeather(EorzeaTime.Now);

        public ZoneWeatherStep GetWeather(EorzeaTime time, int index = 0)
        {
            var beginTime = SyncToEorzeaWeather(time, index);
            var weatherID = WeatherRate.GetWeather(WeatherRateIndex, GetForcast(beginTime));
            return new(ZoneID, weatherID, beginTime);
        }

        public ZoneWeatherStep[] GetWeatherList(EorzeaTime time, int fromIndex = -1, int toIndex = 10)
        {
            var count = toIndex - fromIndex + 1;
            var result = new ZoneWeatherStep[count];
            for (int i = 0; i < count; i++)
                result[i] = GetWeather(time, fromIndex + i);
            return result;
        }

        public (bool, ZoneWeatherStep) FindWeather(EorzeaTime time, int weatherID, int index = 0, int maxStep = int.MaxValue)
        {
            bool found = false;
            ZoneWeatherStep step = default;
            if (WeatherRate.HaveWeather(WeatherRateIndex, weatherID))
            {
                int skip = index < 0 ? (-index + 1) : index;
                int currentIndex = 0;
                if (index < 0)
                {
                    while (maxStep > 0)
                    {
                        if ((step = GetWeather(time, currentIndex)).WeatherID == weatherID)
                        {
                            if (skip > 0)
                                skip--;
                            else
                            {
                                found = true;
                                break;
                            }
                        }
                        currentIndex--;
                        maxStep--;
                    }
                }
                else
                {
                    while (maxStep > 0)
                    {
                        if ((step = GetWeather(time, currentIndex)).WeatherID == weatherID)
                        {
                            if (skip > 0)
                                skip--;
                            else
                            {
                                found = true;
                                break;
                            }
                        }
                        currentIndex++;
                        maxStep--;
                    }
                }
            }
            return (found, step);
        }

        public static EorzeaTime SyncToEorzeaWeather(EorzeaTime time, int index = 0)
            => new((time.UnixTime * 1000 - (time.UnixTime * 1000 % MillisecondsPerEorzeaWeather) + index * MillisecondsPerEorzeaWeather) / 1000);

        public static TimeSpan GetTimeSpanForIndexWeather(EorzeaTime time, int index = 0)
            => SyncToEorzeaWeather(time, index) - time;

        public static int GetForcast(EorzeaTime time)
        {
            var ts = (time.UnixTime * 1000 + MillisecondsPerEorzeaWeather) / 1000;
            var hour = ts / SecondsPerEorzeaHour;
            var increment = (uint)(hour + 8 - hour % 8) % 24;
            var day = (uint)(ts / SecondsPerEorzeaDay);
            var ret = day * 100 + increment;
            ret = (ret << 11) ^ ret;
            ret = (ret >> 8) ^ ret;
            ret %= 100;
            return (int)ret;
        }
    }
}
