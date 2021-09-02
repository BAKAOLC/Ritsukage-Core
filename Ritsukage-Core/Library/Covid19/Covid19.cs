using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ritsukage.Library.Covid19
{
    public static class Covid19Api
    {
        /// <summary>
        /// 新冠Api数据链接
        /// </summary>
        public const string Api = "https://c.m.163.com/ug/api/wuhan/app/data/list-total";

        /// <summary>
        /// 新冠Api来源站点名称
        /// </summary>
        public const string ApiHost = "网易163";

        /// <summary>
        /// Api数据更新时间
        /// </summary>
        public static DateTime UpdateTime = default;

        /// <summary>
        /// 中国新冠疫情
        /// </summary>
        public static Covid19DateReport InChina = default;

        /// <summary>
        /// 世界新冠疫情
        /// </summary>
        public static class InWorld
        {

            /// <summary>
            /// 累积确诊人数
            /// </summary>
            public static int TotalDiagnosis = 0;

            /// <summary>
            /// 累积治愈人数
            /// </summary>
            public static int TotalCure = 0;

            /// <summary>
            /// 累积死亡人数
            /// </summary>
            public static int TotalDeaths = 0;

            /// <summary>
            /// 现存确诊人数
            /// </summary>
            public static int ExistingDiagnosed => TotalDiagnosis - TotalCure - TotalDeaths;

            /// <summary>
            /// 较昨日确诊人数
            /// </summary>
            public static int DifferenceDiagnosis = 0;

            /// <summary>
            /// 较昨日治愈人数
            /// </summary>
            public static int DifferenceCure = 0;

            /// <summary>
            /// 较昨日死亡人数
            /// </summary>
            public static int DifferenceDeaths = 0;

            /// <summary>
            /// 较昨日现存确诊人数
            /// </summary>
            public static int DifferenceExistingDiagnosed => DifferenceDiagnosis - DifferenceCure - DifferenceDeaths;

            public static Covid19DateReport[] AreaList = Array.Empty<Covid19DateReport>();

            public static new string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("累积确诊: ").Append(TotalDiagnosis)
                    .Append($"({Utils.ToSignNumberString(DifferenceDiagnosis)})");
                sb.AppendLine().Append("现有确诊: ").Append(ExistingDiagnosed)
                    .Append($"({Utils.ToSignNumberString(DifferenceExistingDiagnosed)})");
                sb.AppendLine().Append("累积治愈: ").Append(TotalCure)
                    .Append($"({Utils.ToSignNumberString(DifferenceCure)})");
                sb.AppendLine().Append("累积死亡: ").Append(TotalDeaths)
                    .Append($"({Utils.ToSignNumberString(DifferenceDeaths)})");
                return sb.ToString();
            }
        }

        /// <summary>
        /// 获取当前Api数据
        /// </summary>
        /// <returns>数据json字符串</returns>
        static string GetData() => Utils.HttpGET(Api);

        public static bool Update()
        {
            #region 获取数据
            JToken data = null;
            try
            {
                data = JObject.Parse(GetData())["data"];
            }
            catch
            {
            }
            if (data == null)
                return false;
            #endregion
            UpdateTime = Convert.ToDateTime(data["lastUpdateTime"]);
            InChina = new Covid19DateReport("中国", data["chinaTotal"], UpdateTime);
            var list = new List<Covid19DateReport>();
            foreach (var area in (JArray)data["areaTree"])
                list.Add(new Covid19DateReport((string)area["name"], area, UpdateTime));
            InWorld.AreaList = list.ToArray();
            InWorld.TotalDiagnosis = list.Sum(x => x.TotalDiagnosis);
            InWorld.DifferenceDiagnosis = list.Sum(x => x.DifferenceDiagnosis);
            InWorld.TotalCure = list.Sum(x => x.TotalCure);
            InWorld.DifferenceCure = list.Sum(x => x.DifferenceCure);
            InWorld.TotalDeaths = list.Sum(x => x.TotalDeaths);
            InWorld.DifferenceDeaths = list.Sum(x => x.DifferenceDeaths);
            return true;
        }

        public static new string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("#中国疫情#");
            sb.AppendLine(InChina.ToString());
            sb.AppendLine("#全球疫情#");
            sb.AppendLine(InWorld.ToString());
            sb.AppendLine("数据更新时间: " + UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append("数据来源: ").Append(ApiHost);
            return sb.ToString();
        }
    }

    public struct Covid19DateReport
    {
        #region 属性
        /// <summary>
        /// 地区
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 累积确诊人数
        /// </summary>
        public int TotalDiagnosis { get; }

        /// <summary>
        /// 累积治愈人数
        /// </summary>
        public int TotalCure { get; }

        /// <summary>
        /// 累积死亡人数
        /// </summary>
        public int TotalDeaths { get; }

        /// <summary>
        /// 现存确诊人数
        /// </summary>
        public int ExistingDiagnosed => TotalDiagnosis - TotalCure - TotalDeaths;

        /// <summary>
        /// 境外输入患者人数
        /// </summary>
        public int OverseasInput { get; }

        /// <summary>
        /// 无症状感染者人数
        /// </summary>
        public int AsymptomaticPatient { get; }

        /// <summary>
        /// 较昨日确诊人数
        /// </summary>
        public int DifferenceDiagnosis { get; }

        /// <summary>
        /// 较昨日治愈人数
        /// </summary>
        public int DifferenceCure { get; }

        /// <summary>
        /// 较昨日死亡人数
        /// </summary>
        public int DifferenceDeaths { get; }

        /// <summary>
        /// 较昨日现存确诊人数
        /// </summary>
        public int DifferenceExistingDiagnosed => DifferenceDiagnosis - DifferenceCure - DifferenceDeaths;

        /// <summary>
        /// 较昨日境外输入人数
        /// </summary>
        public int DifferenceOverseasInput { get; }

        /// <summary>
        /// 较昨日无症状感染者人数
        /// </summary>
        public int DifferenceAsymptomaticPatient { get; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime UpdateTime { get; }

        /// <summary>
        /// 内部城市数据
        /// </summary>
        public Covid19DateReport[] Children { get; }
        #endregion

        static readonly JToken Null = JValue.CreateNull();
        static readonly JToken Zero = JToken.FromObject(0);
        static JToken GetValue(JToken token)
            => (JToken.DeepEquals(token, Null) || token == null) ? Zero : token;
        static JToken GetValue(JToken token, JToken defaultValue)
            => (JToken.DeepEquals(token, Null) || token == null) ? defaultValue : token;

        public Covid19DateReport(string name, JToken data, DateTime updateTime)
        {
            Name = (string)GetValue(data["name"], name);
            TotalDiagnosis = (int)GetValue(data["total"]["confirm"]);
            DifferenceDiagnosis = (int)GetValue(data["today"]["confirm"]);
            TotalCure = (int)GetValue(data["total"]["heal"]);
            DifferenceCure = (int)GetValue(data["today"]["heal"]);
            TotalDeaths = (int)GetValue(data["total"]["dead"]);
            DifferenceDeaths = (int)GetValue(data["today"]["dead"]);

            OverseasInput = (int)GetValue(data["total"]["input"]);
            DifferenceOverseasInput = (int)GetValue(data["today"]["input"]);

            if (data["extData"] != null)
            {
                AsymptomaticPatient = (int)GetValue(data["extData"]["noSymptom"]);
                DifferenceAsymptomaticPatient = (int)GetValue(data["extData"]["incrNoSymptom"]);
            }
            else
            {
                AsymptomaticPatient = 0;
                DifferenceAsymptomaticPatient = 0;
            }

            if (data["lastUpdateTime"] != null)
            {
                UpdateTime = Convert.ToDateTime(data["lastUpdateTime"]);
            }
            else
                UpdateTime = updateTime;

            if (data["children"] != null)
            {
                var list = new List<Covid19DateReport>();
                foreach (var child in (JArray)data["children"])
                    list.Add(new Covid19DateReport((string)child["name"], child, UpdateTime));
                Children = list.ToArray();
            }
            else
                Children = Array.Empty<Covid19DateReport>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("累积确诊: ").Append(TotalDiagnosis)
                .Append($"({Utils.ToSignNumberString(DifferenceDiagnosis)})");
            sb.AppendLine().Append("现有确诊: ").Append(ExistingDiagnosed)
                .Append($"({Utils.ToSignNumberString(DifferenceExistingDiagnosed)})");
            sb.AppendLine().Append("累积治愈: ").Append(TotalCure)
                .Append($"({Utils.ToSignNumberString(DifferenceCure)})");
            sb.AppendLine().Append("累积死亡: ").Append(TotalDeaths)
                .Append($"({Utils.ToSignNumberString(DifferenceDeaths)})");

            if (OverseasInput > 0)
                sb.AppendLine().Append("境外输入: ").Append(OverseasInput)
                    .Append($"({Utils.ToSignNumberString(DifferenceOverseasInput)})");

            if (AsymptomaticPatient > 0)
                sb.AppendLine().Append("无症状感染者: ").Append(AsymptomaticPatient)
                    .Append($"({Utils.ToSignNumberString(DifferenceAsymptomaticPatient)})");

            return sb.ToString();
        }
    }
}
