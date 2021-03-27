using System.Text;

namespace Ritsukage.Library.Minecraft.Jila
{
    public class IssueLink
    {
        public string Type { get; init; }
        public string InwardDescription { get; init; }
        public string[] Inwardlinks { get; init; }
        public string OutwardDescription { get; init; }
        public string[] Outwardlinks { get; init; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            bool flag = false;
            if (!string.IsNullOrWhiteSpace(InwardDescription) && Inwardlinks != null && Inwardlinks.Length > 0)
            {
                flag = true;
                sb.Append($"{InwardDescription} {string.Join(", ", Inwardlinks)}");
            }
            if (!string.IsNullOrWhiteSpace(OutwardDescription) && Outwardlinks != null && Outwardlinks.Length > 0)
            {
                if (flag) sb.AppendLine();
                sb.Append($"{OutwardDescription} {string.Join(", ", Outwardlinks)}");
            }
            return sb.ToString();
        }
    }
}
