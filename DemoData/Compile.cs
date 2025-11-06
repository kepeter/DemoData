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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// The main program class for the DemoData application.
/// </summary>
internal partial class Program
{
    /// <summary>
    /// Compiles the specified culture.
    /// </summary>
    /// <param name="parseResult">The parsed command-line arguments.</param>
    private static void Compile(ParseResult parseResult)
    {
        terminal.SetFgColor(Terminal.Color.Yellow).SetStyle(Terminal.Style.Bold);

        string culture = parseResult.GetRequiredValue<string>("--culture");

        Compile(culture);

        terminal.SetStyle(Terminal.Style.Reset).WriteLine();
    }

    private static bool Compile(string culture, bool internalUse = false)
    {
        if (!IsCultureExists(culture))
        {
            terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Invalid culture identifier '{culture}'.");

            return false;
        }

        CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture, false);

        if (!internalUse)
        {
            terminal.WriteLine($"Compiling culture {culture} - ({cultureInfo.DisplayName}/{cultureInfo.NativeName})... ");
        }

        string funcFile = Path.Combine(cultureRoot, culture, functionFile);

        if (!File.Exists(funcFile))
        {
            terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab(internalUse ? 4 : 2).WriteLine("ERROR: Can not find functions definition file.");

            return false;
        }
        else
        {
            FunctionFile customFunctions = JsonSerializer.Deserialize<FunctionFile>(File.ReadAllText(funcFile), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            StringBuilder dllCode = new StringBuilder();

            dllCode.AppendLine("namespace DemoData {");
            dllCode.AppendLine($"public class Data{culture.ToUpper()} : Data{customFunctions.Inherit} {{");

            foreach (Function customFunction in customFunctions.Local)
            {
                string finalFormat = customFunction.Func;

                Match match = resourceMatch.Match(customFunction.Func);

                while (match.Success)
                {
                    string resourceCall = $"{{Resource({Replace(squareToQuoteRegex, squareToQuote, match.Value)})}}";

                    finalFormat = finalFormat.Replace(match.Value, $"{resourceCall}");

                    match = match.NextMatch();
                }

                match = functionMatch.Match(finalFormat);

                while (match.Success)
                {
                    string functionCall = $"{{{textInfo.ToTitleCase($"{Replace(angleToNothingRegex, angleToNothing, match.Value)}")}}}";

                    finalFormat = finalFormat.Replace(match.Value, $"{functionCall}");

                    match = match.NextMatch();
                }

                string result = $"return $\"{finalFormat}\";";

                dllCode.AppendLine($"public static string {textInfo.ToTitleCase(customFunction.Name)} () {{");
                dllCode.AppendLine(result);
                dllCode.AppendLine("}");
            }

            dllCode.AppendLine("}");
            dllCode.AppendLine("}");

            string finalCode = dllCode.ToString();
            string runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(finalCode);
            CSharpCompilation compilation = CSharpCompilation.Create(
                $"Data{culture.ToUpper()}.dll",
                syntaxTrees:
                [
                    syntaxTree
                ],
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references:
                [
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile("Data.dll")
                ]);

            using (MemoryStream assemblyFile = new MemoryStream())
            {
                EmitResult result = compilation.Emit(assemblyFile);

                if (!result.Success)
                {
                    terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).WriteLine();

                    foreach (Diagnostic error in result.Diagnostics)
                    {
                        terminal.SpaceTab().WriteLine($"({error.Id}): {error.GetMessage()}, at ({error.Location.GetLineSpan().StartLinePosition.Line}, {error.Location.GetLineSpan().StartLinePosition.Character})");
                    }

                    Dump(finalCode);

                    return false;
                }
                else
                {
                    assemblyFile.Seek(0, SeekOrigin.Begin);

                    using (FileStream fileStream = File.Create(Path.Combine(binRoot, compilation.AssemblyName)))
                    {
                        assemblyFile.CopyTo(fileStream);
                    }

                    if (!internalUse)
                    {
                        terminal.SpaceTab().WriteLine("...done.");
                    }

                    return true;
                }
            }
        }
    }
}
