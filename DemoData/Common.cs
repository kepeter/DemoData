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

using CLI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// The main program class for the DemoData application.
/// </summary>
internal partial class Program
{
    private static readonly Terminal terminal = new Terminal();

    private static readonly string commandRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DemoData");
    private static readonly string cultureRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DemoData", "culture");
    private static readonly string binRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DemoData", "bin");

    private static readonly string functionFile = "func.json";

    private static readonly Dictionary<string, string> squareToQuote = new Dictionary<string, string> { { "[", "\"" }, { "]", "\"" } };
    private static readonly Dictionary<string, string> squareToNothing = new Dictionary<string, string> { { "[", string.Empty }, { "]", string.Empty } };
    private static readonly Dictionary<string, string> angleToNothing = new Dictionary<string, string> { { "<", string.Empty }, { ">", string.Empty } };

    private static readonly Regex squareToQuoteRegex = new Regex("(" + string.Join("|", squareToQuote.Keys.Select(Regex.Escape)) + ")");
    private static readonly Regex squareToNothingRegex = new Regex("(" + string.Join("|", squareToNothing.Keys.Select(Regex.Escape)) + ")");
    private static readonly Regex angleToNothingRegex = new Regex("(" + string.Join("|", angleToNothing.Keys.Select(Regex.Escape)) + ")");

    private static readonly Regex resourceMatch = new Regex(@"\[(.*?)\]");
    private static readonly Regex functionMatch = new Regex(@"\<(.*?)\>");

    private static readonly TextInfo textInfo = CultureInfo.GetCultureInfo("en", true).TextInfo;

    /// <summary>
    /// The output format.
    /// </summary>
    private enum Format
    {
        CSV,
        JSON
    }

    private enum EditType
    {
        Resource,
        Function,
        Command
    }

    /// <summary>
    /// Represents a function definition.
    /// </summary>
    private struct Function
    {
        public string Name { get; set; }
        public string Func { get; set; }
    }

    /// <summary>
    /// Represents a file of functions.
    /// </summary>
    private struct FunctionFile
    {
        public string Inherit { get; set; }
        public Function[] Local { get; set; }
    }

    /// <summary>
    /// Represents a column in a table.
    /// </summary>
    private struct Column
    {
        public string Name { get; set; }
        public string Func { get; set; }
    }

    /// <summary>
    /// Represents a relation between two tables.
    /// </summary>
    private struct Relation
    {
        public string Parent { get; set; }
        public string Child { get; set; }
    }

    /// <summary>
    /// Represents a database table.
    /// </summary>
    private struct Table
    {
        public string Name { get; set; }
        public int Rows { get; set; }
        public Relation[] Relations { get; set; }
        public Table[] ChildTables { get; set; }
        public Column[] Columns { get; set; }
    }

    /// <summary>
    /// Represents a command file.
    /// </summary>
    private struct CommandFile
    {
        public string Name { get; set; }
        public bool Compile { get; set; }
        public string Culture { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("output")]
        public Format? OutputFormat { get; set; }
        public Table[] Tables { get; set; }
    }

    /// <summary>
    /// Prints the application header.
    /// </summary>
    private static void PrintHeader()
    {
        terminal
            .Write($"DemoData Version: {Assembly.GetEntryAssembly().GetName().Version}")
            .WriteLine()
            .SpaceTab().SetStyle(Terminal.Style.Bold).Write($"Culture location: ").SetStyle(Terminal.Style.Reset).WriteLine(cultureRoot)
            .SpaceTab().SetStyle(Terminal.Style.Bold).Write($"Command and Storage location: ").SetStyle(Terminal.Style.Reset).WriteLine(commandRoot)
            .WriteLine();
    }

    /// <summary>
    /// Checks if a culture exists.
    /// </summary>
    /// <param name="culture">The culture to check</param>
    /// <returns>True if the culture exists, false otherwise</returns>
    private static bool IsCultureExists(string culture)
    {
        try
        {
            CultureInfo.GetCultureInfo(culture, true);

            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    /// <summary>
    /// Dumps code with line numbers and indentation.
    /// </summary>
    /// <param name="code">Original code</param>
    private static void Dump(string code)
    {
        string[] lines = code.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        int lineCount = 0;
        int tabs = 1;

        terminal.SetStyle(Terminal.Style.Reset).WriteLine();

        foreach (string line in lines)
        {
            if (line.StartsWith("}"))
            {
                tabs--;
            }

            terminal.SpaceTab().WriteLine(string.Format("{0,4}{2}{1}", lineCount, line, new string(' ', tabs * 2)));

            if (line.EndsWith("{"))
            {
                tabs++;
            }

            lineCount++;
        }
    }

    /// <summary>
    /// Replaces values in a string based on a regex condition and a replacement list.
    /// </summary>
    /// <param name="condition">Regex condition to match</param>
    /// <param name="replaceList">Replacement list</param>
    /// <param name="value">Replacement value</param>
    /// <returns>Replaced string</returns>
    private static string Replace(Regex condition, Dictionary<string, string> replaceList, string value)
    {
        return condition.Replace(value, oMatch => replaceList[oMatch.Value]);
    }
}
