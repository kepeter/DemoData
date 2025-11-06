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
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Terminal.Gui;

using Attribute = Terminal.Gui.Attribute;

namespace DemoData;

/// <summary>
/// The main program class for the DemoData application.
/// </summary>
internal partial class Program
{
    private static void Edit(ParseResult parseResult)
    {
        EditType editType = parseResult.GetRequiredValue<EditType>("--type");
        string culture = parseResult.GetValue<string>("--culture");
        string name = parseResult.GetValue<string>("--name");
        string filePath = null;

        if (editType != EditType.Function && name == null)
        {
            terminal.SetFgColor(CLI.Terminal.Color.Red).SetStyle(CLI.Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Missing name for edit type '{editType}'.");

            return;
        }

        if (editType != EditType.Command && culture == null)
        {
            terminal.SetFgColor(CLI.Terminal.Color.Red).SetStyle(CLI.Terminal.Style.Bold).SpaceTab().WriteLine($"ERROR: Missing culture for edit type '{editType}'.");

            return;
        }

        if (editType == EditType.Command)
        {
            if (string.IsNullOrEmpty(culture))
            {
                culture = CultureInfo.CurrentCulture.Name;
            }
        }

        CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture, false);

        switch (editType)
        {
            case EditType.Resource:
                {
                    filePath = Path.Combine(cultureRoot, culture, $"{name}.json");
                }
                break;

            case EditType.Function:
                {
                    name = Path.GetFileNameWithoutExtension(functionFile);
                    filePath = Path.Combine(cultureRoot, culture, $"{functionFile}saved successfully");
                }
                break;

            case EditType.Command:
                {
                    filePath = Path.Combine(commandRoot, $"{name}.json");
                }
                break;
        }

        Application.UseSystemConsole = true;
        Application.Init();

        Window win = new Window()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Border = new Border(),
            ColorScheme = new ColorScheme() { Normal = new Attribute(Color.White, Color.Black) }
        };

        SyntaxView syntaxView = new SyntaxView(editType, culture, filePath)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        StatusItem cursorPositionItem = new StatusItem(Key.Null, "~Line:~ 0 ~Column:~ 0", null);
        StatusItem fileNameItem = new StatusItem(Key.Null, "", null);
        StatusItem fileModifiedItem = new StatusItem(Key.Null, "", null);

        StatusBar statusBar = new StatusBar(
        [
            new StatusItem(Key.CtrlMask | Key.S, "~Ctrl+S~ave", () => { Save(syntaxView); }),
            new StatusItem(Key.CtrlMask | Key.Q, "~Ctrl+Q~uit", () => { Quit(syntaxView); }),
            new StatusItem(Key.Null, $"~File type:~ {editType} / ~Culture:~ {cultureInfo.NativeName}", null),
            fileNameItem,
            fileModifiedItem,
            cursorPositionItem
        ]);

        syntaxView.UnwrappedCursorPosition += (e) =>
        {
            cursorPositionItem.Title = $"~Line:~ {e.Y + 1} ~Column:~ {e.X + 1}";
        };

        syntaxView.ContentsChanged += (e) =>
        {
            if (syntaxView.IsDirty)
            {
                fileModifiedItem.Title = "~Modified~";
            }
        };

        if (editType == EditType.Resource)
        {
            if (syntaxView.IsNew)
            {
                fileNameItem.Title = $"~Resource name:~ {name}~*~";
            }
            else
            {
                fileNameItem.Title = $"~Resource name:~ {name}";
            }
        }
        else if (editType == EditType.Command)
        {
            if (syntaxView.IsNew)
            {
                fileNameItem.Title = $"~Command file:~ {name}~*~";
            }
            else
            {
                fileNameItem.Title = $"~Command file:~ {name}";
            }
        }
        else
        {
            statusBar.RemoveItem(3);
        }

        win.Add(syntaxView, statusBar);

        Application.Top.Add(win);

        Application.Run();
        Application.Shutdown();
    }

    private static void Quit(SyntaxView syntaxView)
    {
        if (syntaxView.IsDirty)
        {
            int answer = MessageBox.Query(50, 7, "Quit", "File has been modified. Do you want to save changes?", "Yes", "No");

            if (answer == 0)
            {
                Save(syntaxView);
            }
        }

        Application.RequestStop();
    }

    private static void Save(SyntaxView syntaxView)
    {
        bool valid = ValidateText(syntaxView);

        if (!valid)
        {
            return;
        }
        
        syntaxView.Save();

        MessageBox.Query(50, 7, "Save", "File saved successfully.", "OK");
    }

    private static bool ValidateText(SyntaxView syntaxView)
    {
        bool valid = true;
        string content = syntaxView.Text.ToString();

        try
        {
            switch (syntaxView.EditType)
            {
                case EditType.Resource:
                    {
                        string[] resource = JsonSerializer.Deserialize<string[]>(content);
                    }
                    break;

                case EditType.Function:
                    {
                        FunctionFile customFunctions = JsonSerializer.Deserialize<FunctionFile>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    break;

                case EditType.Command:
                    {
                        CommandFile command = JsonSerializer.Deserialize<CommandFile>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    break;
            }
        }
        catch (JsonException ex)
        {
            valid = false;

            MessageBox.ErrorQuery(60, 10, "Validation Error", $"Resource JSON is invalid:\n{ex.Message}", "OK");
        }

        return valid;
    }

    private class SyntaxView : TextView
    {
        private enum Flags : byte
        {
            None,
            Name,
            Literal,
            JsonBase,
            Resource,
            Function
        }

        private Dictionary<string, Flags[]> lineCache = new Dictionary<string, Flags[]>(StringComparer.CurrentCultureIgnoreCase);
        private LinkedList<string> leastRecentlyUsed = new LinkedList<string>();
        private int cacheLimit = 1024;

        private Regex nameMatch = new Regex(@""".+""\s*:", RegexOptions.IgnoreCase);
        private Attribute nameColor = new Attribute(Color.White, Color.Black);

        private Regex literalMatch = new Regex(@""".+""", RegexOptions.IgnoreCase);
        private Attribute literalColor = new Attribute(Color.BrightBlue, Color.Black);

        private Regex jsonBaseMatch = new Regex(@"\{|\}|\[|\]|:|,|true|false|null", RegexOptions.IgnoreCase);
        private Attribute jsonBaseColor = new Attribute(Color.BrightGreen, Color.Black);

        private Regex resourceMatch = new Regex(@"\[.+\]", RegexOptions.IgnoreCase);
        private Attribute resourceColor = new Attribute(Color.BrightMagenta, Color.Black);

        private Regex functionMatch = new Regex(@"\<.+\>", RegexOptions.IgnoreCase);
        private Attribute functionColor = new Attribute(Color.BrightYellow, Color.Black);

        private string filePath;
        private bool isNew = false;

        public bool IsNew => isNew;
        public EditType EditType { get; private set; }

        public SyntaxView(EditType type, string culture, string filePath)
        {
            this.EditType = type;
            this.filePath = filePath;

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            if (File.Exists(filePath))
            {
                LoadFile(filePath);
            }
            else
            {
                isNew = true;

                switch (type)
                {
                    case EditType.Resource:
                        {
                            LoadTemplate("resourceTemplate", 5, 1);
                        }
                        break;

                    case EditType.Function:
                        {
                            LoadTemplate("functionTemplate", 21, 4);
                        }
                        break;

                    case EditType.Command:
                        {
                            LoadTemplate("commandTemplate", 13, 1);
                        }
                        break;
                }
            }

            if (type != EditType.Resource)
            {
                List<string> resourceList = LoadResourceList(culture);
                List<string> functionList = LoadFunctionList(culture);

                Autocomplete.ColorScheme = new ColorScheme()
                {
                    Normal = new Attribute(Color.White, Color.Gray),
                    Focus = new Attribute(Color.White, Color.BrightBlue)
                };

                Autocomplete.MaxHeight = 12;
                Autocomplete.MaxWidth = 48;
                Autocomplete.PopupInsideContainer = true;
                Autocomplete.Reopen = Key.CtrlMask | Key.Space;

                Autocomplete.AllSuggestions.AddRange(resourceList);
                Autocomplete.AllSuggestions.AddRange(functionList);
            }
        }

        public void Save()
        {
            File.WriteAllText(filePath, Text.ToString());

            ClearHistoryChanges();
        }

        protected List<string> LoadResourceList(string culture)
        {
            List<string> resourceList = new List<string>();
            string culturePath = Path.Combine(cultureRoot, culture);
            DirectoryInfo dir = new DirectoryInfo(culturePath);

            foreach (FileInfo file in dir.GetFiles())
            {
                if (!file.Name.ToLower().Equals(functionFile))
                {
                    resourceList.Add(Path.GetFileNameWithoutExtension(file.Name));
                }
            }

            return resourceList;
        }

        protected List<string> LoadFunctionList(string culture)
        {
            List<string> functionList = new List<string>();

            foreach (string cultureValue in new[] { culture, null })
            {
                Assembly assembly = null;

                if (cultureValue == null)
                {
                    assembly = Assembly.LoadFrom("Data.dll");
                }
                else
                {
                    assembly = Assembly.LoadFrom(Path.Combine(binRoot, $"Data{cultureValue.ToUpper()}.dll"));
                }

                Type[] types = assembly.GetExportedTypes();

                foreach (Type type in types)
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                    foreach (MethodInfo method in methods)
                    {
                        string methodSignature = $"{method.Name.ToLower()}(";

                        foreach (ParameterInfo parameter in method.GetParameters())
                        {
                            if (parameter.HasDefaultValue)
                            {
                                methodSignature += $"{parameter.Name.ToLower()} = {parameter.DefaultValue.ToString().ToLower()}, ";
                            }
                            else
                            {
                                methodSignature += $"{parameter.Name.ToLower()}, ";
                            }
                        }

                        methodSignature = methodSignature.TrimEnd(',', ' ') + ")";

                        functionList.Add(methodSignature);
                    }
                }
            }

            return functionList;
        }

        protected override void SetNormalColor(List<Rune> line, int idx)
        {
            string strLine = new string(line.Select(r => (char)r).ToArray());

            if (!TryGetCachedFlags(strLine, out Flags[] flags))
            {
                flags = ComputeFlagsForLine(strLine);
                AddToCache(strLine, flags);
            }

            if (idx < 0 || idx >= flags.Length)
            {
                Driver.SetAttribute(jsonBaseColor);

                return;
            }

            switch (flags[idx])
            {
                case Flags.Name:
                    {
                        Driver.SetAttribute(nameColor);
                    }
                    break;

                case Flags.Literal:
                    {
                        Driver.SetAttribute(literalColor);
                    }
                    break;

                case Flags.JsonBase:
                    {
                        Driver.SetAttribute(jsonBaseColor);
                    }
                    break;

                case Flags.Resource:
                    {
                        Driver.SetAttribute(resourceColor);
                    }
                    break;

                case Flags.Function:
                    {
                        Driver.SetAttribute(functionColor);
                    }
                    break;

                default:
                    {
                        base.SetNormalColor(line, idx);
                    }
                    break;
            }
        }

        private bool TryGetCachedFlags(string line, out Flags[] flags)
        {
            if (lineCache.TryGetValue(line, out flags))
            {
                leastRecentlyUsed.Remove(line);
                leastRecentlyUsed.AddFirst(line);

                return true;
            }

            flags = null;

            return false;
        }

        private void AddToCache(string line, Flags[] flags)
        {
            if (lineCache.ContainsKey(line))
            {
                leastRecentlyUsed.Remove(line);
                leastRecentlyUsed.AddFirst(line);

                return;
            }

            lineCache[line] = flags;
            leastRecentlyUsed.AddFirst(line);

            if (leastRecentlyUsed.Count > cacheLimit)
            {
                var last = leastRecentlyUsed.Last;

                if (last != null)
                {
                    lineCache.Remove(last.Value);
                    leastRecentlyUsed.RemoveLast();
                }
            }
        }

        private Flags[] ComputeFlagsForLine(string strLine)
        {
            Flags[] flags = new Flags[strLine.Length];

            if (strLine.Length == 0)
                return flags;

            foreach (Match match in nameMatch.Matches(strLine))
            {
                int start = match.Index;
                int end = Math.Min(match.Index + match.Length, strLine.Length);
                for (int i = start; i < end; i++)
                {
                    flags[i] = Flags.Name;
                }
            }

            foreach (Match match in literalMatch.Matches(strLine))
            {
                int start = match.Index;
                int end = Math.Min(match.Index + match.Length, strLine.Length);
                for (int i = start; i < end; i++)
                {
                    if (flags[i] == Flags.Name)
                        continue;

                    flags[i] = Flags.Literal;
                }
            }

            foreach (Match match in jsonBaseMatch.Matches(strLine))
            {
                int start = match.Index;
                int end = Math.Min(match.Index + match.Length, strLine.Length);
                for (int i = start; i < end; i++)
                {
                    if (flags[i] == Flags.Name)
                        continue;

                    if (flags[i] == Flags.Literal)
                        continue;

                    flags[i] = Flags.JsonBase;
                }
            }

            foreach (Match match in resourceMatch.Matches(strLine))
            {
                int start = match.Index;
                int end = Math.Min(match.Index + match.Length, strLine.Length);
                for (int i = start; i < end; i++)
                {
                    if (flags[i] == Flags.Literal)
                    {
                        flags[i] = Flags.Resource;
                    }
                }
            }

            foreach (Match match in functionMatch.Matches(strLine))
            {
                int start = match.Index;
                int end = Math.Min(match.Index + match.Length, strLine.Length);
                for (int i = start; i < end; i++)
                {
                    if (flags[i] == Flags.Literal)
                    {
                        flags[i] = Flags.Function;
                    }
                }
            }

            return flags;
        }

        private void LoadTemplate(string resourceName, int x, int y)
        {
            Text = GetEmbeddedTextResource(resourceName);
            CursorPosition = new Point(x, y);
            ScrollTo(0);
        }

        private string GetEmbeddedTextResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}