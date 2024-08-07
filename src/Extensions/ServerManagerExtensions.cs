using Microsoft.Web.Administration;

namespace Extensions;

public static partial class ServerManagerSiteExtensions
{
    private static void ThrowIfContainsInvalidCharacters(string name, Type type, char[] invalidChars)
    {
        if (name.IndexOfAny(invalidChars) == -1)
        {
            throw new ArgumentException($"{type.Name}: {name} contains invalid characters", nameof(name));
        }
    }
}
