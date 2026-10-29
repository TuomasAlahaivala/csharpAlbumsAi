namespace AlbumsWebApp.Services;

internal static class DotEnv
{
    private static readonly string[] SupportedKeys = ["ADMIN_EMAIL", "ADMIN_PASSWORD_HASH"];

    public static void Load(string directory)
    {
        var path = Path.Combine(directory, ".env");
        if (!File.Exists(path))
            return;

        foreach (var (line, lineNumber) in File.ReadLines(path).Select((line, index) => (line, index + 1)))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
                throw new InvalidOperationException($"Invalid .env entry on line {lineNumber}.");

            var key = trimmed[..separator].Trim();
            if (!SupportedKeys.Contains(key, StringComparer.Ordinal))
                continue;

            var value = trimmed[(separator + 1)..].Trim();
            if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                value = value[1..^1];
            else if (value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
                value = value[1..^1];

            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
