using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Bank
{
    public static class Forex
    {
        public struct ForexResult
        {
            public string From;
            public string FromName;
            public string To;
            public string ToName;
            public double Value;
            public double Result;

            public ForexResult(string from, string to, double value, double result)
            {
                From = from;
                FromName = Currency[from] ?? From;
                To = to;
                ToName = Currency[to] ?? To;
                Value = value;
                Result = result;
            }

            public override string ToString()
                => $"{Value:F2} {(From == "RMB" ? "CNY" : From)}({FromName}) = {Result:F2} {(To == "RMB" ? "CNY" : To)}({ToName})";
        }

        const string Api = "http://vip.stock.finance.sina.com.cn/forex/api/openapi.php/ForexService.getBankForex";

        static readonly Dictionary<string, string> Currency = new()
        {
            { "AUD", "澳大利亚元" },
            { "BRL", "巴西雷亚尔" },
            { "CAD", "加拿大元" },
            { "CHF", "瑞士法郎" },
            { "DKK", "丹麦克朗" },
            { "EUR", "欧元" },
            { "GBP", "英镑" },
            { "HKD", "港元" },
            { "JPY", "日元" },
            { "KRW", "韩元" },
            { "MOP", "澳门元" },
            { "MYR", "马来西亚林吉特" },
            { "NOK", "挪威克朗" },
            { "NZD", "新西兰元" },
            { "PHP", "菲律宾比索" },
            { "RMB", "人民币" },
            { "RUB", "俄罗斯卢布" },
            { "SEK", "瑞典克朗" },
            { "SGD", "新加坡元" },
            { "THB", "泰铢" },
            { "TWD", "新台币" },
            { "USD", "美元" },
            { "ZAR", "南非兰特" },
        };

        static readonly Dictionary<string, double> ExchangeRate = new();

        static bool _updating = false;
        static DateTime LastUpdateTime;
        static readonly double UpdateDelay = 60 * 20;

        static bool Update()
        {
            if (_updating) return false;
            _updating = true;
            try
            {
                var rawData = Utils.HttpGET(Api);
                var data = JObject.Parse(rawData);
                var refer = data["result"]["data"]["refer"];
                foreach (var kv in (JObject)refer)
                {
                    if (Currency.ContainsKey(kv.Key) && kv.Key != "RMB")
                    {
                        ExchangeRate[kv.Key] = (double)kv.Value;
                    }
                }
                return true;
            }
            catch (Exception)
            {
            }
            finally
            {
                _updating = false;
            }
            return false;
        }

        public static async Task<ForexResult> GetFromCNY(string to, double value)
        {
            while (_updating) await Task.Delay(1000);
            if ((DateTime.Now - LastUpdateTime).TotalSeconds >= UpdateDelay)
            {
                while (!Update()) ;
                LastUpdateTime = DateTime.Now;
            }
            to = to.ToUpper();
            if (to == "CNY") to = "RMB";
            if (Currency.ContainsValue(to))
                to = Currency.FirstOrDefault(x => x.Value == to).Key;
            if (Currency.ContainsKey(to))
            {
                if (to == "RMB")
                {
                    return new("RMB", to, value, value);
                }
                else
                {
                    return new("RMB", to, value, value * 100 / ExchangeRate[to]);
                }
            }
            return default;
        }

        public static async Task<ForexResult> GetToCNY(string from, double value)
        {
            while (_updating) await Task.Delay(1000);
            if ((DateTime.Now - LastUpdateTime).TotalSeconds >= UpdateDelay)
            {
                while (!Update()) ;
                LastUpdateTime = DateTime.Now;
            }
            from = from.ToUpper();
            if (from == "CNY") from = "RMB";
            if (Currency.ContainsValue(from))
                from = Currency.FirstOrDefault(x => x.Value == from).Key;
            if (Currency.ContainsKey(from))
            {
                if (from == "RMB")
                {
                    return new(from, "RMB", value, value);
                }
                else
                {
                    return new(from, "RMB", value, value * ExchangeRate[from] / 100);
                }
            }
            return default;
        }

        public static Dictionary<string, string> GetForexList() => Currency.ToDictionary(x => x.Key, x => x.Value);
    }
}
