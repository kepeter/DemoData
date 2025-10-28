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
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;

/// <summary>
/// The main program class for the DemoData application.
/// </summary>
internal partial class Program
{
    /// <summary>
    /// Lists either the specified culture or all available cultures and their resources.
    /// </summary>
    /// <param name="parseResult">The parsed command-line arguments.</param>
    private static void List(ParseResult parseResult)
    {
        CultureInfo cultureInfo;

        terminal.SetFgColor(Terminal.Color.Yellow).SetStyle(Terminal.Style.Bold);

        string culture = parseResult.GetValue<string>("--culture")?.ToLower();

        if (!string.IsNullOrEmpty(culture))
        {
            if (!IsCultureExists(culture))
            {
                terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Invalid culture identifier '{culture}'.");

                return;
            }

            cultureInfo = CultureInfo.GetCultureInfo(culture, true);

            terminal.WriteLine($"Listing culture {culture} - ({cultureInfo.DisplayName}/{cultureInfo.NativeName})...");
        }
        else
        {
            terminal.WriteLine("Listing all cultures...");
        }

        if (!string.IsNullOrEmpty(culture))
        {
            string culturePath = Path.Combine(cultureRoot, culture);

            if (!Directory.Exists(culturePath))
            {
                terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: The specified culture '{culture}' does not exist.");
            }
            else
            {
                ListCulture(culturePath, true);

                terminal.SpaceTab().WriteLine("...done.");
            }
        }
        else
        {
            string[] cultureList = Directory.GetDirectories(cultureRoot)?.OrderBy(o => o).ToArray();

            if (cultureList.Length > 0)
            {
                foreach (string culturePath in cultureList)
                {
                    culture = Path.GetFileName(culturePath);

                    if (!IsCultureExists(culture))
                    {
                        terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Invalid culture identifier '{culture}'.");

                        return;
                    }

                    ListCulture(Path.Combine(cultureRoot, culture));
                }

                terminal.SpaceTab().WriteLine("...done.");
            }
            else
            {
                terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: No cultures found.");
            }
        }

        terminal.SetStyle(Terminal.Style.Reset).WriteLine();
    }

    /// <summary>
    /// Lists the resources for a specific culture.
    /// </summary>
    /// <param name="culture">The culture to list resources for.</param>
    /// <param name="single">Indicates whether to list a single culture.</param>
    private static void ListCulture(string culture, bool single = false)
    {
        DirectoryInfo dir = new DirectoryInfo(culture);
        bool funcStatus = File.Exists(Path.Combine(dir.FullName, functionFile)) ? true : false;
        CultureInfo cultureInfo = new CultureInfo(dir.Name, false);

        terminal.WriteLine().SetStyle(Terminal.Style.Reset);

        if (!single)
        {
            terminal.SpaceTab().SetFgColor(Terminal.Color.Yellow).WriteLine($"{dir.Name} - ({cultureInfo.DisplayName}/{cultureInfo.NativeName})");
        }
        terminal.SpaceTab(4).SetFgColor(funcStatus ? Terminal.Color.Green : Terminal.Color.Red).WriteLine($"Functions definitions file is {(funcStatus ? "present" : "missing")}.");
        terminal.SpaceTab(4).SetFgColor(Terminal.Color.White).WriteLine("Resources:");

        foreach (FileInfo file in dir.GetFiles())
        {
            if (!file.Name.ToLower().Equals(functionFile))
            {
                terminal.SpaceTab(6).SetFgColor(Terminal.Color.Green).WriteLine($"{Path.GetFileNameWithoutExtension(file.Name)}");
            }
        }
    }
}
