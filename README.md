# DemoData
A powerful command-line tool for generating realistic relational demo data with support for multiple cultures and customizable data structures.

## Overview
DemoData is a flexible data generation tool that allows you to create structured demo data based on culture-specific templates (names, addresses, etc.). It compiles culture info into reusable libraries and executes command files to generate data in CSV or JSON format.

## Features
* **Multi-Culture Support:** Support for multiple cultures (some samples for English (en) and French (fr) are included as examples)
* **Extensible Resource System:** Define custom resources like first names, last names, streets, etc. for each culture
* **Custom Functions:** Create reusable functions combining resources and random data generators
* **Hierarchical Data:** Generate parent-child table relationships with automatic reference handling
* **Multiple Output Formats:** Export data to CSV or JSON

## Usage
DemoData [command] [options]

Commands:
* **list**     List cultures and their resources.
* **compile**  Compile the specified culture's functions (as defined in func.json).
* **execute**  Execute the specified command file.

#### List Command
Options \
**--culture**       The culture to use. If not specified, all cultures will be listed.

#### Compile Command
Options \
**--culture (REQUIRED)**  The culture to use.

#### Execute Command
Options \
**--file (REQUIRED)**    The command file to execute. \
**--culture**            The culture to use. If not specified, the culture from the command file will be used. If no culture specified in the command file the current culture of the running environment will be used. \
**--target**             The target folder to store output to. If not specified, the Name defined in the command file will be used. If no Name is specified in the command file, the file name will be used. \
**--compile**            If set, the selected culture's functions will be compiled before execution. If not specified, the behavior defined in the command file will be used. \
**--format <CSV|JSON>**  The output format to use. If not specified, the format from the command file will be used. If no format is specified in the command file, CSV format will be used.

### Resource Files
These files are JSON formated lists of culture specific data. When used one of the values randomly picked to generate the actual data. \
A sample can be street names in French: \
[
	"Rue Abel",
	"Rue La Condamine",
	"Square Charles Hermite",
	"Place Monge",
	"Cité Condorcet",
	"Passage Poncelet",
	"Rue Esclangon",
	"Rue Fermat"
] \
\
A resource identified by its filename, creating the same filename/resource under different cultures can ensure the reuse of the same command file to generate multilingual data.

### Function File
There can be only one such file (always named func.json) in every culture (can be omitted). A function file defines different formating functions, that can combine pre-defined random values with resource valuesa and literals. \
A function file can inherit from other cultures, so in case of same languages in different countries (en, en-US, en-UK) the common functions can be reused.
A sample for such function can be phone numbers: \
In France for instance a phone number is ten digits - usually formatted as 5 two digit numbers, where the first pair is an area code from 01 to 09. \
A possible function to create such number may look like this \
`
{
    "name": "phone",
    "func": "0<range(1,9)>-<number(2)>-<number(2)>-<number(2)>-<number(2)>"
}
`
\
Canada also uses ten digit phone numbers, but formatted as 3-3-4, where the first parts is an area code and in most cases enclosed in parenthesis
 \
A possible function to create such number may look like this \
`
{
    "name": "phone",
    "func": "(<number(3)>)-<number(3)>-<number(4)>"
}
`
\
\
In a function file you can call a function enclosed in <> (pre-defined, inherited and local functions all can be used, but you must prevent circular dependencies). \
A resource can be referenced by its name enclosed in []. For example a random street name can be created using `[street]` in the function defintition. \
Anything not enclosed as function or resource will be interpreted as literal.

### Command File
This file defines the data tables with all the columns in them and the relations - if any. \
The definition is hierarchical where every table will have a definition to how many rows to create and what columns to include (the content of a column defined as a function the very same way function are defined). In addition any table can have child table(s) with the very same structure and additonal relation between the two. For instance a table 'person' can have a child table 'mail' with multiple email addresses for the very same person. \
The row count in child tables tells how many rows to create for **each** parent row, while in a parent row it is the number of the total rows to create.

### List of pre-defined functions
* **Sign** - +/-, can be usefull for monetary values
* **Number(minLenght, maxLengt)** - creates a variable length number
* **Range(min, max)** - creates a number in the range defined
* **Guid** - creates a GUID
* **Alpha(minLenght, maxLengt)** - creates avariable lenght aplhanumeric value
* **Ip4** - IP address according to v4
* **Ip6** - IP address according to v6
* **Latitude(formatted)** - a latitude value as decimal or degrees
* **Longitude(formatted)** - a longitude value as decimal or degrees
* **Date(maxYear)** - date formatted as sort date format
* **Time** - time formatted as sort time format
* **DateTime(maxYear)** - a combination of the previous two
* **Sid** - a sequential number starting at 1 and incrementing on each call by 1
* **Resource(name)** - a random value form the named resource

## Editor
Each file type (resource, function and command) can be edited with a simple terminal based editor (using Terminal.Gui). 
* The content will be validated before saving
* New files for new or existing cultures can be created using the editor
* When fitting list of resources and functions will be provided (Intellisense-like)

## Contributing

Contributions are welcome! Please open issues or submit pull requests.

## License

This project is licensed under the MIT License.