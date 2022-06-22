namespace Neolution.OrchardCoreModules.PageViewStats.Resources;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// Utility to work with embedded resources.
/// </summary>
internal static class EmbeddedResources
{
    /// <summary>
    /// The assembly to read the embedded resources from.
    /// </summary>
    private static readonly Assembly Assembly = typeof(EmbeddedResources).Assembly;

    /// <summary>
    /// Read embedded binary resource file.
    /// </summary>
    /// <param name="resourceNamespacePath">The path to the resource file.</param>
    /// <returns>The binary content of the resource.</returns>
    public static byte[] ReadAllBytes(string resourceNamespacePath)
    {
        using var ms = new MemoryStream();
        using var stream = Assembly.GetManifestResourceStream(resourceNamespacePath);
        if (stream == null)
        {
            return null;
        }

        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Read embedded text resource file.
    /// </summary>
    /// <param name="resourceNamespacePath">The path to the resource file.</param>
    /// <returns>The text content of the resource.</returns>
    public static string ReadAllText(string resourceNamespacePath)
    {
        var resourceFile = ReadAllBytes(resourceNamespacePath);
        return Encoding.UTF8.GetString(resourceFile);
    }

    /// <summary>
    /// Gets the embedded files from the folder
    /// </summary>
    /// <param name="assemblyFolderName">Name of the assembly folder.</param>
    /// <returns>list of namespace path of files in this folder</returns>
    public static List<string> GetEmbeddedFiles(string assemblyFolderName)
    {
        var embeddedFiles = Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith(assemblyFolderName))
            .ToList();

        return embeddedFiles;
    }
}