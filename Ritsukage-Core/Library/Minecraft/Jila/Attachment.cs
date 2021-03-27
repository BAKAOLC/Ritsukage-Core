using Ritsukage.Tools;
using System;

namespace Ritsukage.Library.Minecraft.Jila
{
    public struct Attachment
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public int Size { get; init; }
        public string Author { get; init; }
        public DateTime CreatedTime { get; init; }

        public Attachment(string id, string name, int size, string author, DateTime datetime)
        {
            Id = id;
            Name = name;
            Size = size;
            Author = author;
            CreatedTime = datetime;
        }
        public Attachment(string id, string name, int size, string author, string datetime)
            : this(id, name, size, author, Convert.ToDateTime(datetime)) { }

        public string Url => $"https://bugs.mojang.com/secure/attachment/{Id}/{Utils.UrlEncode(Name)}";

        public override string ToString()
            => $"[Id:{Id},File:{Name},Size:{Size}]";
    }
}
