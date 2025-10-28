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

using System.CommandLine;
using System.IO;

/// <summary>
/// The main program class for the DemoData application.
/// </summary>
internal partial class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>An integer exit code.</returns>
    private static int Main(string[] args)
    {
        Directory.CreateDirectory(cultureRoot);

        PrintHeader();

        if (args.Length == 0)
        {
            args = ["--help"];
        }

        RootCommand rootCommand = new RootCommand("Generates pseudo-random relational data for testing purposes.") { };

        Command listCommand = new Command("list", "List cultures and their resources.")
        {
            Options = { new Option<string>("--culture") { Description = "The culture to use. If not specified, all cultures will be listed.", Required = false } },
        };

        Command compileCommand = new Command("compile", $"Compile the specified culture's functions (as defined in {functionFile}).")
        {
            Options = { new Option<string>("--culture") { Description = "The culture to use.", Required = true } },
        };

        Command executeCommand = new Command("execute", "Execute the specified command file.")
        {
            Options =
            {
                { new Option<string>("--file") { Description = "The command file to execute.", Required = true } },
                { new Option<string>("--culture") { Description = "The culture to use. If not specified, the culture from the command file will be used. If no culture specified in the command file the current culture of the running environment will be used.", Required = false } },
                { new Option<string>("--target") { Description = "The target folder to store output to. If not specified, the Name defined in the command file will be used. If no Name is specified in the command file, the file name will be used.", Required = false } },
                { new Option<bool>("--compile") { Description = "If set, the selected culture's functions will be compiled before execution. If not specified, the behavior defined in the command file will be used.", Required = false } },
                { new Option<Format>("--format") { Description = "The output format to use. If not specified, the format from the command file will be used. If no format is specified in the command file, CSV format will be used.", Required = false } },
            },
        };

        listCommand.SetAction(List);
        compileCommand.SetAction(Compile);
        executeCommand.SetAction(Execute);

        rootCommand.Subcommands.Add(listCommand);
        rootCommand.Subcommands.Add(compileCommand);
        rootCommand.Subcommands.Add(executeCommand);

        ParseResult parseResult = rootCommand.Parse(args);

        return parseResult.Invoke();
    }
}
