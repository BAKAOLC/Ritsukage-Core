﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Ritsukage.Library.Minecraft.Jila
{
    public partial struct Comment
    {
        public string Id { get; init; }
        public string Author { get; init; }
        public DateTime CreatedTime { get; init; }
        public string Message { get; init; }

        public Comment(string id, string author, DateTime datetime, string message)
        {
            Id = id;
            Author = author;
            CreatedTime = datetime;
            Message = GetHtmlTagRegex().Replace(message, (s) =>
            {
                var text = s.Value;
                if (text == "<br/>")
                    return Environment.NewLine;
                else
                    return "";
            });
        }
        public Comment(string id, string author, string datetime, string message)
            : this(id, author, Convert.ToDateTime(datetime), message) { }

        public override string ToString()
            => new StringBuilder()
            .AppendLine($"Author: {Author}")
            .AppendLine(Message)
            .Append(CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"))
            .ToString();

        [GeneratedRegex("<[^>]+>")]
        private static partial Regex GetHtmlTagRegex();
    }
}
