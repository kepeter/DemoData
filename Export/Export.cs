// -----------------------------------------------------------------------
// <copyright>
// Copyright (c) Peter Eliyahu Kornfeld. All rights reserved.
// </copyright>
//
// <license>
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.
// </license>
// -----------------------------------------------------------------------
namespace DemoData;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Export functionality.
/// 
///     This class provides methods to export data in various formats.
/// </summary>
public class Export
{
    /// <summary>
    /// Exports the given set of JsonObjects to a JSON file at the specified target path.
    /// </summary>
    /// <param name="Set">The set of JsonObjects to export.</param>
    /// <param name="Target">The target file path.</param>
    public static void ToJson(List<JsonObject> Set, string Target)
    {
        TextWriter writer = EnsureTarget(Target);

        writer.WriteLine(JsonSerializer.Serialize(Set));
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// Exports the given set of JsonObjects to a CSV file at the specified target path.
    /// </summary>
    /// <param name="Set">The set of JsonObjects to export.</param>
    /// <param name="Target">The target file path.</param>
    public static void ToCsv(List<JsonObject> Set, string Target)
    {
        TextWriter writer = EnsureTarget(Target);
        StringBuilder oSB = new StringBuilder();
        IEnumerable<string> szColumns = new List<string>(Set[0].Select(oCol => oCol.Key));

        oSB.AppendLine(string.Join(",", szColumns));

        foreach (JsonObject oRow in Set)
        {
            IEnumerable<string> szValues = oRow.Select(oCol => string.Format("\"{0}\"", oCol.Value.ToString().Replace("\"", "\"\"")));

            oSB.AppendLine(string.Join(",", szValues));
        }

        writer.WriteLine(oSB.ToString());
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// Ensures that the target file path exists and returns a TextWriter for it.
    /// </summary>
    /// <param name="Target">The target file path.</param>
    /// <returns>A TextWriter for the target file path.</returns>
    private static TextWriter EnsureTarget(string Target)
    {
        string directoryPath = Path.GetDirectoryName(Target);

        TextWriter writer;

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        writer = new StreamWriter(Target, false, Encoding.UTF8);

        return writer;
    }
}
