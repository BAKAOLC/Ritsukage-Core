namespace SimpleWatchDog
{
    public class ArgsResolver
    {
        public bool IsHelp { get; private set; }

        public ushort? PID { get; init; }

        public string? PipeName { get; init; }

        public uint? Duration { get; init; }

        public ArgsResolver(ArgsGrouper grouper)
        {
            if (grouper.MainParams.Count > 0 && ushort.TryParse(grouper.MainParams[0], out var pid))
            {
                PID = pid;
            }
            if (grouper.AdditionalParams.ContainsKey("h"))
            {
                IsHelp = true;
            }
            if (grouper.AdditionalParams.ContainsKey("n"))
            {
                if (grouper.AdditionalParams["n"].Count > 0)
                {
                    PipeName = grouper.AdditionalParams["n"][0];
                }
                else
                {
                    throw new ArgumentException($"Insufficient argument for \"{nameof(PipeName)}\".");
                }
            }
            if (grouper.AdditionalParams.ContainsKey("d"))
            {
                if (grouper.AdditionalParams["d"].Count > 0 && uint.TryParse(grouper.AdditionalParams["d"][0], out var duration))
                {
                    Duration = duration;
                }
                else
                {
                    throw new ArgumentException($"Insufficient argument for \"{nameof(Duration)}\".");
                }
            }
        }
    }
}
