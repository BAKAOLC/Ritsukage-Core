﻿using Newtonsoft.Json;
using System.IO;

namespace Ritsukage
{
    public class Config
    {
        const string ConfigPath = "config.json";

        /// <summary>
        /// 数据库储存位置
        /// </summary>
        public string DatabasePath = "data.db";

        /// <summary>
        /// 诗词数据库储存位置
        /// </summary>
        public string PoetryDatabasePath = "";

        /// <summary>
        /// Http网络代理
        /// </summary>
        public string ProxyHttp = "";

        /// <summary>
        /// 是否使用调试模式
        /// </summary>
        public bool IsDebug = false;

        #region Roll Api
        /// <summary>
        /// Roll Api Id (https://github.com/MZCretin/RollToolsApi)
        /// </summary>
        public string Roll_Api_Id = string.Empty;
        /// <summary>
        /// Roll Api Secret
        /// </summary>
        public string Roll_Api_Secret = string.Empty;
        #endregion

        /// <summary>
        /// 缩链API(http://suolink.cn/) Token(用于短链接)
        /// </summary>
        public string SuoLinkToken = "";

        #region Mojang Jira
        /// <summary>
        /// Mojang Jira Username
        /// </summary>
        public string MoJiraUsername = "";
        /// <summary>
        /// Mojang Jira Password
        /// </summary>
        public string MoJiraPassword = "";
        #endregion

        #region Baidu
        /// <summary>
        /// 百度翻译Api App Id
        /// </summary>
        public string BaiduTranslateAppId = "";
        /// <summary>
        /// 百度翻译Api Key
        /// </summary>
        public string BaiduTranslateKey = "";
        #endregion

        #region
        /// <summary>
        /// OpenAI Api Key
        /// </summary>
        public string OpenAIApiKey = "";
        /// <summary>
        /// OpenAI Organization
        /// </summary>
        public string OpenAIOrganization = "";
        #endregion

        /// <summary>
        /// OCRSpace Token
        /// </summary>
        public string OCRSpaceToken = "";

        #region QQ
        /// <summary>
        /// 是否启用QQ相关功能
        /// </summary>
        public bool QQ = false;

        /// <summary>
        /// 要连接的目标IP
        /// </summary>
        public string Host = "[::]";

        /// <summary>
        /// 要监听的目标端口
        /// </summary>
        public ushort Port = 23150;

        /// <summary>
        /// token
        /// </summary>
        public string AccessToken = string.Empty;

        /// <summary>
        /// 心跳超时间隔
        /// </summary>
        public int HeartBeatTimeOut = 30000;

        /// <summary>
        /// QQ相关功能超级权限者
        /// </summary>
        public long QQSuperUser = -1;
        #endregion

        #region Discord
        /// <summary>
        /// 是否启用Discord相关功能
        /// </summary>
        public bool Discord = false;

        /// <summary>
        /// Discord Bot Token
        /// </summary>
        public string DiscordToken = string.Empty;
        #endregion

        public static Config LoadConfig()
        {
            Config cfg = null;
            try
            {
                if (File.Exists(ConfigPath))
                    cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            catch
            {
            }
            if (cfg == null)
            {
                cfg = new Config();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(cfg, Formatting.Indented));
            }
            return cfg;
        }
    }
}
