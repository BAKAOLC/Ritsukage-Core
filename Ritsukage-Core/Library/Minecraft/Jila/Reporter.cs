namespace Ritsukage.Library.Minecraft.Jila
{
    public struct Reporter
    {
        public string Id;
        public string Name;

        public Reporter(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public Reporter(string id) : this(id, "@" + id) { }

        public override string ToString() => Name;
    }
}
