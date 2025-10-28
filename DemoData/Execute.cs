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
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// The main program class for the DemoData application.
/// </summary>
internal partial class Program
{
    private static void Execute(ParseResult parseResult)
    {
        string commandFile = parseResult.GetRequiredValue<string>("--file");
        string culture = parseResult.GetValue<string>("--culture");
        string target = parseResult.GetValue<string>("--target");
        bool? compile = parseResult.GetValue<bool?>("--compile");
        Format? format = parseResult.GetValue<Format?>("--format");

        terminal.SetFgColor(Terminal.Color.Yellow).SetStyle(Terminal.Style.Bold).WriteLine("Executing command...");

        commandFile = Path.Combine(commandRoot, commandFile);

        if (!File.Exists(commandFile))
        {
            terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Can not find command file.");
        }
        else
        {
            terminal.SpaceTab().WriteLine($"Using command file at: {Path.GetFullPath(commandFile)}.");

            CommandFile command = JsonSerializer.Deserialize<CommandFile>(File.ReadAllText(commandFile), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (string.IsNullOrEmpty(culture))
            {
                culture = command.Culture ?? CultureInfo.CurrentCulture.Name;
            }

            if (!IsCultureExists(culture))
            {
                terminal.SetFgColor(Terminal.Color.Red).SetStyle(Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Invalid culture identifier '{culture}'.");

                return;
            }

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture, true);

            terminal.SpaceTab().WriteLine($"Using culture: {culture} - ({cultureInfo.DisplayName}/{cultureInfo.NativeName}).");

            if (compile.Value || command.Compile)
            {
                terminal.SpaceTab().WriteLine($"Compiling culture's functions.");

                if (!Compile(culture, true))
                {
                    return;
                }
            }
            else
            {
                terminal.SpaceTab().WriteLine($"Using pre-compiled culture functions.");

                if (!File.Exists($"Data{culture.ToUpper()}.dll"))
                {
                    terminal.SetFgColor(Terminal.Color.Red).SpaceTab().WriteLine($"ERROR: Can not find file (Data{culture.ToUpper()}.dll).");

                    return;
                }
            }

            if (string.IsNullOrEmpty(format?.ToString()))
            {
                format = command.OutputFormat ?? Format.CSV;
            }

            terminal.SpaceTab().WriteLine($"Formating results as: {format}.");

            if (string.IsNullOrEmpty(target))
            {
                target = command.Name ?? Path.GetFileNameWithoutExtension(commandFile);
            }

            target = Path.Combine(commandRoot, target, culture);

            terminal.SpaceTab().WriteLine($"Saving results at: {target}.");

            StringBuilder execCode = new StringBuilder();
            List<string> tableNames = new List<string>();

            execCode.AppendLine("using System.Collections;");
            execCode.AppendLine("using System.Collections.Generic;");
            execCode.AppendLine("using System.Linq;");
            execCode.AppendLine("using System.Text.Json.Nodes;");

            execCode.AppendLine("namespace DemoData {");

            foreach (Table table in command.Tables)
            {
                WriteTable(table, execCode, tableNames, culture);
            }

            execCode.AppendLine("public class Execute {");
            execCode.AppendLine("public static void Run() {");

            execCode.AppendLine("Dictionary<string, Stack<JsonObject>> storage = new Dictionary<string, Stack<JsonObject>>( );");
            execCode.AppendLine($"Data.Reset( \"{culture}\" );");

            foreach (string tableName in tableNames)
            {
                execCode.AppendLine($"storage.Add( \"{tableName}\", new Stack<JsonObject>( ) );");
            }

            foreach (Table table in command.Tables)
            {
                execCode.AppendLine($"{table.Name}.LoadData( storage );");
            }

            execCode.AppendLine("int order = 0;");

            execCode.AppendLine("foreach ( KeyValuePair<string, Stack<JsonObject>> table in storage ) {");

            switch (format)
            {
                case Format.JSON:
                    execCode.AppendLine($"Export.ToJson(table.Value.ToList(), $\"{target}{Path.DirectorySeparatorChar}{{order:D4}} {{table.Key}}.json\");");
                    break;
                case Format.CSV:
                    execCode.AppendLine($"Export.ToCsv(table.Value.ToList(), $\"{target}{Path.DirectorySeparatorChar}{{order:D4}} {{table.Key}}.csv\");");
                    break;
            }

            execCode.AppendLine("order++;");
            execCode.AppendLine("}");
            execCode.AppendLine("}");
            execCode.AppendLine("}");
            execCode.AppendLine("}");

            string finalCode = execCode.ToString();
            string runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(finalCode);
            CSharpCompilation compilation = CSharpCompilation.Create(
                $"Execute{culture.ToUpper()}.dll",
                syntaxTrees:
                [
                    syntaxTree
                ],
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references:
                [
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Collections.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Linq.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Text.Json.dll")),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile("Data.dll"),
                    MetadataReference.CreateFromFile($"Data{culture.ToUpper()}.dll"),
                    MetadataReference.CreateFromFile("Export.dll")
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
                }
                else
                {
                    AppDomain.CurrentDomain.AssemblyResolve += AppDomain_AssemblyResolve;

                    Assembly execAssembly = Assembly.Load(assemblyFile.ToArray());
                    Type type = execAssembly.GetType("DemoData.Execute");
                    MethodInfo method = type.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);

                    method.Invoke(null, null);

                    terminal.SpaceTab().WriteLine("...done.");

                    AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_AssemblyResolve;
                }
            }
        }

        terminal.SetStyle(Terminal.Style.Reset).WriteLine();
    }

    private static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        string assemblyPath = Path.Combine(".", new AssemblyName(args.Name).Name);

        if (File.Exists(assemblyPath))
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        return null; ;
    }

    private static void WriteTable(Table table, StringBuilder code, List<string> tableName, string culture, string parentTable = null)
    {
        Dictionary<string, string> properties = new Dictionary<string, string>();
        bool needNext = false;

        code.AppendLine($"public class {table.Name} {{");
        List<string> lines = new List<string>();

        tableName.Add(table.Name);

        foreach (Column column in table.Columns)
        {
            Relation[] relations = table.Relations?.Where(relation => relation.Child == column.Name).ToArray();

            if (relations?.Length == 1)
            {
                lines.Add($"{{\"{textInfo.ToTitleCase(column.Name)}\", parent[\"{textInfo.ToTitleCase(relations[0].Parent)}\"].GetValue<string>()}},");

                continue;
            }

            List<string> funcList = new List<string>();
            int index = 0;

            string finalFormat = column.Func;

            Match match = resourceMatch.Match(column.Func);

            while (match.Success)
            {
                needNext = true;

                string key = textInfo.ToTitleCase(Replace(squareToNothingRegex, squareToNothing, match.Value));

                if (!properties.ContainsKey(key))
                {
                    properties.Add(key, $"Resource({Replace(squareToQuoteRegex, squareToQuote, match.Value)})");

                    code.AppendLine($"private static string {key};");
                }

                funcList.Add(key);

                finalFormat = finalFormat.Replace(match.Value, $"{{{index++}}}");

                match = match.NextMatch();
            }

            match = functionMatch.Match(column.Func);

            while (match.Success)
            {
                funcList.Add($"Data{culture.ToUpper()}.{textInfo.ToTitleCase($"{match.Value.Replace("<", "").Replace(">", "")}")}");

                finalFormat = finalFormat.Replace(match.Value, $"{{{index++}}}");

                match = match.NextMatch();
            }

            if (index > 0)
            {
                lines.Add($"{{\"{textInfo.ToTitleCase(column.Name)}\", string.Format(\"{finalFormat}\", {string.Join(", ", funcList.ToArray())})}},");
            }
            else
            {
                lines.Add($"{{\"{textInfo.ToTitleCase(column.Name)}\", \"{finalFormat}\"}},");
            }
        }

        if (needNext)
        {
            code.AppendLine("private static void Next() {");
            foreach (KeyValuePair<string, string> property in properties)
            {
                code.AppendLine($"{property.Key} = Data{culture.ToUpper()}.{property.Value};");
            }
            code.AppendLine("}");
        }

        code.AppendLine("private static JsonObject Record( JsonObject parent = null ) {");

        if (needNext)
        {
            code.AppendLine("Next();");
        }

        code.AppendLine($"return new JsonObject {{{string.Join(Environment.NewLine, lines.ToArray())}}};");

        code.AppendLine("}");

        code.AppendLine("public static void LoadData ( Dictionary<string, Stack<JsonObject>> storage, JsonObject parent = null ) {");
        code.AppendLine("Data.PushSID();");
        code.AppendLine($"for (int i = 0; i < {table.Rows}; i++ ) {{");
        code.AppendLine($"storage[\"{table.Name}\"].Push( Record( parent ) );");

        foreach (Table childTable in table.ChildTables ?? Enumerable.Empty<Table>())
        {
            code.AppendLine($"{childTable.Name}.LoadData( storage, storage[\"{table.Name}\"].Peek( ) );");
        }

        code.AppendLine("}");
        code.AppendLine("Data.PopSID();");
        code.AppendLine("}");

        code.AppendLine("}");

        foreach (Table childTable in table.ChildTables ?? Enumerable.Empty<Table>())
        {
            WriteTable(childTable, code, tableName, culture, table.Name);
        }
    }
}
