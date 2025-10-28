# DemoData

A powerful .NET command-line tool for generating realistic demo/mock data with support for multiple cultures and customizable data structures.

## Overview

DemoData is a flexible data generation tool that allows you to create structured demo data based on culture-specific templates (names, addresses, etc.) and custom functions. It compiles culture data into reusable libraries and executes commands to generate data in CSV or JSON format.

## Features

- **Multi-Culture Support**: Built-in support for English (en), French (fr), and Hebrew (he) cultures
- **Extensible Resource System**: Define custom resources like first names, last names, streets, etc. for each culture
- **Custom Functions**: Create reusable functions combining resources and random data generators
- **Hierarchical Data**: Generate parent-child table relationships with automatic reference handling
- **Multiple Output Formats**: Export data to CSV or JSON
- **Dynamic Compilation**: Compiles culture-specific data libraries on-demand

## Installation

### Prerequisites

- .NET Framework 4.5.2 or higher
- Visual Studio 2015 or higher (for building from source)

### Building from Source

1. Clone the repository
2. Open `DemoData.sln` in Visual Studio
3. Restore NuGet packages (Newtonsoft.Json)
4. Build the solution (F6)

The compiled executable will be available in `DemoData/bin/Debug/DemoData.exe` or `DemoData/bin/Release/DemoData.exe`.

## Usage

### Command Syntax

```
DemoData -list | -comp {culture} | -cmd {file} {culture}
```

### Commands

#### List Cultures
Display all available cultures and their resources:
```
DemoData -list
```

#### Compile Culture
Compile a culture's resources into a reusable library:
```
DemoData -comp en
```
This compiles the `en` (English) culture data into `Dataen.dll`.

#### Execute Command File
Generate data using a command file and culture:
```
DemoData -cmd cmd.json en
```

## Culture Resources

Culture data is stored in `Culture/{culture}/` directories. Each culture can define:

### Resources
JSON files containing arrays of data (e.g., `first.json`, `last.json`, `street.json`):

```json
[
  "John",
  "Jane",
  "Bob"
]
```

### Functions
`func.json` defines reusable functions combining resources and generators:

```json
{
  "inherit": "",
  "local": [
    {
      "name": "name",
      "func": "[first] [last]"
    },
    {
      "name": "address",
      "func": "<number(3)> [street]"
    },
    {
      "name": "phone",
      "func": "<number(3)>-<number(7)>"
    },
    {
      "name": "email",
      "func": "[first]@dummy.com"
    },
    {
      "name": "ballance",
      "func": "<sign()><number(2,5)>.<number(2)>"
    }
  ]
}
```

## Command Files

Command files (JSON) define the data structure to generate:

```json
{
  "compile": false,
  "output": "csv",
  "tables": [
    {
      "name": "person",
      "rows": 1000,
      "columns": [
        {
          "name": "id",
          "func": "<sid()>"
        },
        {
          "name": "first_name",
          "func": "[first]"
        },
        {
          "name": "last_name",
          "func": "[last]"
        },
        {
          "name": "age",
          "func": "<number(2,2)>"
        }
      ],
      "childTables": [
        {
          "name": "email",
          "rows": 1,
          "relations": [
            {
              "parent": "id",
              "child": "person"
            }
          ],
          "columns": [
            {
              "name": "person",
              "func": ""
            },
            {
              "name": "id",
              "func": "<sid()>"
            },
            {
              "name": "email",
              "func": "<alpha(7,15)>@example.com"
            }
          ]
        }
      ]
    }
  ]
}
```

### Command File Options

- **compile**: `true` to compile culture before execution, `false` to use existing compiled culture
- **output**: Output format - `"csv"` or `"json"`
- **tables**: Array of table definitions

### Table Options

- **name**: Table name (used for output file)
- **rows**: Number of rows to generate
- **columns**: Array of column definitions
- **childTables**: Optional array of child tables (one-to-many relationships)
- **relations**: Optional array defining parent-child column relationships

### Column Definition

- **name**: Column name
- **func**: Data generation function using resources `[name]` or generators `<function()>`

## Data Generators

### Built-in Generators

- `<sid()>` - Sequential ID (auto-incrementing)
- `<number(min,max)>` - Random number with specified digit length
- `<alpha(min,max)>` - Random alphabetic string
- `<sign()>` - Random sign (+ or -)

### Resource References

Use square brackets to reference culture resources:
- `[first]` - Random first name
- `[last]` - Random last name
- `[street]` - Random street name

### Custom Functions

Reference custom functions from `func.json`:
- `<name()>` - Uses the "name" function defined in func.json
- `<address()>` - Uses the "address" function
- `<phone()>` - Uses the "phone" function
- `<email()>` - Uses the "email" function

## Example Workflow

1. **List available cultures:**
   ```
   DemoData -list
   ```

2. **Compile the English culture:**
   ```
   DemoData -comp en
   ```

3. **Generate data:**
   ```
   DemoData -cmd cmd.json en
   ```

4. **Output:**
   - Data files will be created in the `results/` directory
   - Each table becomes a separate file (e.g., `person.csv`, `email.csv`)

## Adding a New Culture

1. Create a new directory: `Culture/{culture-code}/`
2. Add resource files (e.g., `first.json`, `last.json`, `street.json`)
3. Create `func.json` with culture-specific functions
4. Compile the culture: `DemoData -comp {culture-code}`

## Architecture

The project consists of three main components:

- **DemoData** (Main): Command-line interface and data generation logic
- **DAL (Data)**: Core data access and random generation functions
- **Export**: Data export functionality (CSV, JSON)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Copyright (c) 2017 Peter Eliyahu Kornfeld

## Additional Resources

For more detailed information and examples, visit:
https://www.codeproject.com/Articles/1198666/Demo-data
