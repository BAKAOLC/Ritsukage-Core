using System;
using System.Text;

namespace Ritsukage.Tools.Console
{
    /// <summary>
    /// <para>控制台格式化Log类</para>
    /// <para>用于输出格式化Log</para>
    /// </summary>
    public static class ConsoleLog
    {
        #region Log等级设置
        private static LogLevel Level = LogLevel.Info;

        /// <summary>
        /// 设置日志等级
        /// </summary>
        /// <param name="level">LogLevel</param>
        public static void SetLogLevel(LogLevel level) => Level = level;

        /// <summary>
        /// 禁用log
        /// </summary>
        public static void SetNoLog() => Level = (LogLevel)5;
        #endregion

        #region 控制台锁
        private static readonly object ConsoleWriterLock = new();
        #endregion

        #region 格式化错误Log
        static string FormatException(Exception e)
            => new StringBuilder()
            .AppendLine("Error:" + e.GetType().FullName)
            .AppendLine("Message:" + e.Message)
            .AppendLine("Stack Trace:")
            .Append(e.StackTrace).ToString();

        /// <summary>
        /// 生成格式化的错误Log文本
        /// </summary>
        /// <param name="e">错误</param>
        /// <returns>格式化Log</returns>
        public static string ErrorLogBuilder(Exception e, bool showInnerException)
        {
            var sb = new StringBuilder().AppendLine()
            .AppendLine("==============ERROR==============")
            .AppendLine(FormatException(e));
            if (showInnerException)
            {
                while (e.InnerException != null)
                {
                    sb.AppendLine("==============INNER==============")
                        .AppendLine(FormatException(e.InnerException));
                    e = e.InnerException;
                }
            }
            sb.Append("=================================");
            return sb.ToString();
        }
        public static string ErrorLogBuilder(Exception e) => ErrorLogBuilder(e, false);
        public static string GetFormatString(this Exception e, bool showInnerException) => ErrorLogBuilder(e, showInnerException);
        public static string GetFormatString(this Exception e) => ErrorLogBuilder(e);
        #endregion

        #region 格式化控制台Log函数
        /// <summary>
        /// 向控制台发送Info信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">信息内容</param>
        public static void Info(object type, object message)
        {
            if (Level > LogLevel.Info) return;
            lock (ConsoleWriterLock)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine($"[{DateTime.Now}][INFO][{type}]{message}");
            }
        }

        /// <summary>
        /// 向控制台发送Warning信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">信息内容</param>
        public static void Warning(object type, object message)
        {
            if (Level > LogLevel.Warn) return;
            lock (ConsoleWriterLock)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"[{DateTime.Now}][");
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("WARNINIG");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"][{type}]");
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine($"{message}");
                System.Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// 向控制台发送Error信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">信息内容</param>
        public static void Error(object type, object message)
        {
            if (Level > LogLevel.Error) return;
            lock (ConsoleWriterLock)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"[{DateTime.Now}][");
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write("ERROR");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"][{type}]");
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(message);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// 向控制台发送Fatal信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">信息内容</param>
        public static void Fatal(object type, object message)
        {
            if (Level > LogLevel.Fatal) return;
            lock (ConsoleWriterLock)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"[{DateTime.Now}][");
                System.Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.Write("FATAL");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"][{type}]");
                System.Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine(message);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// 向控制台发送Debug信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">信息内容</param>
        public static void Debug(object type, object message)
        {
            if (Level != LogLevel.Debug) return;
            lock (ConsoleWriterLock)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"[{DateTime.Now}][");
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.Write("DEBUG");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"][{type}]");
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine(message);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
        }
        #endregion
    }
}
