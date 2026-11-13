using System.Reflection;

namespace PickupPoint.Core;

/// <summary>Версія збірки, задана в Directory.Build.props.</summary>
public static class VersionInfo
{
    /// <summary>Повертає версію без суфікса з хешем коміта.</summary>
    /// <returns>Рядок виду 0.8.0.</returns>
    public static string Current()
    {
        Assembly assembly = typeof(VersionInfo).Assembly;
        AssemblyInformationalVersionAttribute? attribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string raw = attribute?.InformationalVersion ?? "0.0.0";
        int plus = raw.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? raw : raw[..plus];
    }
}
