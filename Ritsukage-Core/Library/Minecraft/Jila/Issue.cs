using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ritsukage.Library.Minecraft.Jila
{
    public partial class Issue
    {
        public string Id { get; init; }
        public string Project { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Summary { get; init; }
        public string Type { get; init; }
        public string Status { get; init; }
        public string Resolution { get; init; }
        public Reporter Reporter { get; init; }
        public string[] Labels { get; init; }
        public DateTime CreatedTime { get; init; }
        public DateTime UpdatedTime { get; init; }
        public DateTime? ResolvedTime { get; init; }
        public string[] Versions { get; init; }
        public string[] FixVersions { get; init; }
        public int Votes { get; init; }
        public int Watches { get; init; }
        public string Category { get; init; }
        public string ConfirmationStatus { get; init; }
        public string MojangPriority { get; init; }
        public string Platform { get; init; }
        public Comment[] Comments { get; init; }
        public Attachment[] Attachments { get; init; }
        public IssueLink[] IssueLinks { get; init; }
        public string Url => $"https://bugs.mojang.com/browse/{Id}";

        public Issue(XmlNode data) //data from <item> label
        {
            Id = data["key"].InnerText;
            Project = data["project"].InnerText;
            Title = data["title"].InnerText;
            Description = Utils.RemoveEmptyLine(GetHtmlTagRegex().Replace(data["description"].InnerText, (s) =>
            {
                var text = s.Value;
                if (text == "<br/>")
                    return Environment.NewLine;
                else if (text.StartsWith("<img"))
                {
                    var xml = new XmlDocument();
                    xml.LoadXml(text);
                    var src = xml.DocumentElement.GetAttribute("src");
                    if (src.StartsWith("https://bugs.mojang.com/images/icons/"))
                        return "";
                    else
                        return src;
                }
                else
                    return "";
            }));
            Summary = data["summary"].InnerText;
            Type = data["type"].InnerText;
            Status = data["status"].InnerText;
            Resolution = data["resolution"].InnerText;
            Reporter = new(data["reporter"].GetAttribute("username"), data["reporter"].InnerText);
            if (data["labels"].HasChildNodes)
            {
                var lables = new List<string>();
                foreach (XmlNode lable in data["labels"].SelectNodes("label"))
                    lables.Add(lable.InnerText);
                Labels = lables.ToArray();
            }
            else
                Labels = Array.Empty<string>();
            CreatedTime = Convert.ToDateTime(data["created"].InnerText);
            UpdatedTime = Convert.ToDateTime(data["updated"].InnerText);
            if (data["resolved"] != null)
                ResolvedTime = Convert.ToDateTime(data["resolved"].InnerText);
            var versions = new List<string>();
            foreach (XmlNode version in data.SelectNodes("version"))
                versions.Add(version.InnerText);
            Versions = versions.ToArray();
            var fixVersions = new List<string>();
            foreach (XmlNode version in data.SelectNodes("fixVersion"))
                fixVersions.Add(version.InnerText);
            FixVersions = fixVersions.ToArray();
            Votes = int.Parse(data["votes"].InnerText);
            Watches = int.Parse(data["watches"].InnerText);
            foreach (XmlNode node in data.SelectNodes("customfields/customfield"))
            {
                switch (node["customfieldname"].InnerText)
                {
                    case "Category":
                        Category = node.SelectSingleNode("customfieldvalues/customfieldvalue").InnerText;
                        break;
                    case "Confirmation Status":
                        ConfirmationStatus = node.SelectSingleNode("customfieldvalues/customfieldvalue").InnerText;
                        break;
                    case "Mojang Priority":
                        MojangPriority = node.SelectSingleNode("customfieldvalues/customfieldvalue").InnerText;
                        break;
                    case "Platform":
                        Platform = node.SelectSingleNode("customfieldvalues/customfieldvalue").InnerText;
                        break;
                }
            }
            var comments = data.SelectNodes("comments/comment");
            if (comments != null)
            {
                var c = new List<Comment>();
                foreach (XmlNode node in comments)
                    c.Add(new Comment(node.Attributes["id"].Value,
                        node.Attributes["author"].Value,
                        node.Attributes["created"].Value,
                        node.InnerText));
                Comments = c.ToArray();
            }
            else
                Comments = Array.Empty<Comment>();
            var attachments = data.SelectNodes("attachments/attachment");
            if (attachments != null)
            {
                var c = new List<Attachment>();
                foreach (XmlNode node in attachments)
                    c.Add(new Attachment(node.Attributes["id"].Value,
                        node.Attributes["name"].Value,
                        int.Parse(node.Attributes["size"].Value),
                        node.Attributes["author"].Value,
                        node.Attributes["created"].Value));
                Attachments = c.ToArray();
            }
            else
                Attachments = Array.Empty<Attachment>();
            var issuelinks = data.SelectNodes("issuelinks/issuelinktype");
            if (issuelinks != null)
            {
                var c = new List<IssueLink>();
                foreach (XmlNode node in issuelinks)
                {
                    var inward = node.SelectSingleNode("inwardlinks");
                    List<string> inwards = new();
                    if (inward != null)
                        foreach (XmlNode link in inward.SelectNodes("issuelink/issuekey"))
                            inwards.Add(link.InnerText);
                    var outward = node.SelectSingleNode("outwardlinks");
                    List<string> outwards = new();
                    if (outward != null)
                        foreach (XmlNode link in outward.SelectNodes("issuelink/issuekey"))
                            outwards.Add(link.InnerText);
                    c.Add(new()
                    {
                        Type = node["name"].InnerText,
                        InwardDescription = inward == null ? "" : node["inwardlinks"].GetAttribute("description"),
                        Inwardlinks = inwards.ToArray(),
                        OutwardDescription = outward == null ? "" : node["outwardlinks"].GetAttribute("description"),
                        Outwardlinks = outwards.ToArray()
                    });
                }
                IssueLinks = c.ToArray();
            }
            else
                IssueLinks = Array.Empty<IssueLink>();
        }

        public override string ToString()
            => Title;

        static readonly object _lock = new();
        static Certificate Token;

        public static Issue GetIssue(string id)
        {
            lock (_lock)
            {
                if (!Token.IsOK || Token.Expires < DateTime.Now)
                {
                    ConsoleLog.Debug("Mojang Jira", "Try to login...");
                    Token = Certificate.Login(Program.Config.MoJiraUsername, Program.Config.MoJiraPassword);
                }
                if (!Token.IsOK || Token.Expires < DateTime.Now)
                {
                    ConsoleLog.Debug("Mojang Jira", "Login failed.");
                    return null;
                }
                var url = $"https://bugs.mojang.com/si/jira.issueviews:issue-xml/{id}/{id}.xml";
                ConsoleLog.Debug("Mojang Jira", $"Getting issue {id} from " + url);
                var page = Utils.HttpGET(url, "", 20000, Token.Cookie);
                var xml = new XmlDocument();
                var xmlreader = XmlReader.Create(new StringReader(page), new()
                {
                    IgnoreComments = true
                });
                xml.Load(xmlreader);
                var item = xml.DocumentElement.SelectSingleNode("channel/item");
                if (item == null)
                {
                    ConsoleLog.Debug("Mojang Jira", "Failed");
                    return null;
                }
                else
                {
                    ConsoleLog.Debug("Mojang Jira", "Success");
                    return new(item);
                }
            }
        }

        public static Issue[] GetIssues(string search)
        {
            lock (_lock)
            {
                if (!Token.IsOK || Token.Expires < DateTime.Now)
                {
                    ConsoleLog.Debug("Mojang Jira", "Try to login...");
                    Token = Certificate.Login(Program.Config.MoJiraUsername, Program.Config.MoJiraPassword);
                }
                if (!Token.IsOK || Token.Expires < DateTime.Now)
                {
                    ConsoleLog.Debug("Mojang Jira", "Login failed.");
                    return Array.Empty<Issue>();
                }
                var url = $"https://bugs.mojang.com/sr/jira.issueviews:searchrequest-xml/temp/SearchRequest.xml?jqlQuery={Utils.UrlEncode(search)}";
                ConsoleLog.Debug("Mojang Jira", "Getting issues from " + url);
                var page = Utils.HttpGET(url, "", 20000, Token.Cookie);
                var xml = new XmlDocument();
                var xmlreader = XmlReader.Create(new StringReader(page), new()
                {
                    IgnoreComments = true
                });
                xml.Load(xmlreader);
                List<Issue> issues = new();
                var items = xml.DocumentElement.SelectNodes("channel/item");
                if (items != null)
                    foreach (XmlNode e in items)
                        issues.Add(new(e));
                if (issues.Count == 0)
                    ConsoleLog.Debug("Mojang Jira", "No issue was found.");
                else
                    ConsoleLog.Debug("Mojang Jira", $"Get {issues.Count} issues.");
                return issues.ToArray();
            }
        }

        [GeneratedRegex("<[^>]+>")]
        private static partial Regex GetHtmlTagRegex();
    }
}