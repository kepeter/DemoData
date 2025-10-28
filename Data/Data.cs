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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Data generation class.
///
///  Provides methods to generate random data such as numbers, strings, dates, IP addresses, etc.
///  Supports localization through resource files.
///
///  Base class for custom data generation classes.
/// </summary>
public class Data
{
    private static readonly Random random = new Random();
    private static readonly Stack<int> sidStack = new Stack<int>();
    private static readonly string cultureRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DemoData", "culture");
    private static readonly string baseName = $"{cultureRoot}{Path.DirectorySeparatorChar}{{0}}{Path.DirectorySeparatorChar}{{1}}.json";
    private static readonly string missingResource = "Resource '{0}' is missing.";
    private static readonly Dictionary<string, Dictionary<string, string[]>> resources = new Dictionary<string, Dictionary<string, string[]>>();
    private static readonly string alphaChars = "ABCDEFGHIJKLMNOPQRUIString.MissingResourceSTUVWXYZ0123456789";

    private static CultureInfo cultureInfo;
    private static int localSid = 0;

    /// <summary>
    /// Resets the data generator to its initial state.
    /// If a culture is provided, it sets the culture for data generation; otherwise, it defaults to English ("en").
    /// This method also resets the sequential ID (SID) counter to zero.
    /// </summary>
    /// <param name="culture">The culture name to use for localization. If null or empty, defaults to "en".</param>
    public static void Reset(string culture)
    {
        cultureInfo = string.IsNullOrEmpty(culture) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(culture);

        localSid = 0;
    }

    /// <summary>
    /// Pushes the current SID onto a stack and resets the SID to zero.
    /// This allows for nested SID sequences, where you can save the current state,
    /// start a new sequence, and later restore the previous state by popping the stack.
    /// </summary>
    public static void PushSID()
    {
        sidStack.Push(localSid);

        localSid = 0;
    }

    /// <summary>
    /// Pops the last SID from the stack and restores it as the current SID.
    /// This method should be called after a corresponding PushSID() call to restore the previous SID state.
    /// </summary>
    public static void PopSID()
    {
        localSid = sidStack.Pop();
    }

    /// <summary>
    /// Generates a random sign, either '+' or '-'.
    /// </summary>
    /// <returns>A char representing the sign.</returns>
    public static char Sign()
    {
        return random.Next(-1, 2) > 0 ? '+' : '-';
    }

    /// <summary>
    /// Generates a random number as a string, with optional minimum and maximum length.
    /// If only minLength is provided, it generates a number with length between 0 and minLength.
    /// If both minLength and maxLength are provided, it generates a number with length between minLength and maxLength.
    /// </summary>
    /// <param name="minLength">The minimum length of the generated number.</param>
    /// <param name="maxLength">The maximum length of the generated number.</param>
    /// <returns>A string representing the generated number.</returns>
    public static string Number(int minLength = 0, int maxLength = 0)
    {
        if (maxLength == 0)
        {
            maxLength = minLength;
            minLength = 0;
        }

        return Convert.ToString(random.Next(Convert.ToInt32("1".PadRight(minLength, '0')), Convert.ToInt32("1".PadRight(maxLength + 1, '0'))));
    }

    /// <summary>
    /// Generates a random number within a specified range as a string.
    /// If only min is provided, it generates a number between 0 and min.
    /// If both min and max are provided, it generates a number between min and max.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <returns>A string representing the generated number.</returns>
    public static string Range(int min = 0, int max = 0)
    {
        if (max == 0)
        {
            max = min;
            min = 0;
        }

        return Convert.ToString(random.Next(min, max + 1));
    }

    /// <summary>
    /// Generates a new GUID (Globally Unique Identifier) as a string.
    /// </summary>
    /// <returns>A string representing the generated GUID.</returns>
    public static string Guid()
    {
        return System.Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Generates a random alphanumeric string with optional minimum and maximum length.
    /// If only minLength is provided, it generates a string with length between 1 and minLength.
    /// If both minLength and maxLength are provided, it generates a string with length between minLength and maxLength.
    /// </summary>
    /// <param name="minLength">The minimum length of the generated string.</param>
    /// <param name="maxLength">The maximum length of the generated string.</param>
    /// <returns>A string representing the generated alphanumeric string.</returns>
    public static string Alpha(int minLength = 0, int maxLength = 0)
    {
        if (maxLength == 0)
        {
            maxLength = minLength;
            minLength = 1;
        }

        int length = random.Next(minLength, maxLength + 1);

        string result = new string(
            Enumerable.Repeat(alphaChars, length)
                      .Select(array => array[random.Next(array.Length)])
                      .ToArray());

        return result;
    }

    /// <summary>
    /// Generates a random IPv4 address in the format "X.X.X.X".
    /// Each octet is generated randomly within the valid range for IPv4 addresses.
    /// The first octet is between 1 and 254, while the other three octets are between 0 and 255.
    /// </summary>
    /// <returns>A string representing the generated IPv4 address.</returns>
    public static string Ip4()
    {
        return $"{random.Next(1, 255)}.{random.Next(0, 256)}.{random.Next(0, 256)}.{random.Next(0, 256)}";
    }

    /// <summary>
    /// Generates a random IPv6 address in the standard hexadecimal format.
    /// Each segment is generated randomly within the valid range for IPv6 addresses.
    /// </summary>
    /// <returns>A string representing the generated IPv6 address.</returns>
    public static string Ip6()
    {
        return $"{random.Next(1, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}:{random.Next(0, short.MaxValue + 1):X}";
    }

    /// <summary>
    /// Generates a random latitude value.
    /// If 'formatted' is true, the latitude is returned in degrees and minutes format (e.g., "45° 30'").
    /// If 'formatted' is false, the latitude is returned as a decimal value (e.g., "45.5").
    /// </summary>
    /// <param name="formatted">Indicates whether the output should be formatted. Defaults to false.</param>
    /// <returns>A string representing the generated latitude.</returns>
    public static string Latitude(bool formatted = false)
    {
        if (formatted)
        {
            return string.Format("{0}° {1}'", random.Next(-90, 90), random.Next(0, 60));
        }
        else
        {
            return string.Format("{0}.{1}", random.Next(-90, 90), random.Next(0, 100));
        }
    }

    /// <summary>
    /// Generates a random longitude value.
    /// If 'formatted' is true, the longitude is returned in degrees and minutes format (e.g., "120° 45'").
    /// If 'formatted' is false, the longitude is returned as a decimal value (e.g., "120.75").
    /// </summary>
    /// <param name="formatted">Indicates whether the output should be formatted. Defaults to false.</param>
    /// <returns>A string representing the generated longitude.</returns>
    public static string Longitude(bool formatted = false)
    {
        if (formatted)
        {
            return string.Format("{0}° {1}'", random.Next(-180, 180), random.Next(0, 60));
        }
        else
        {
            return string.Format("{0}.{1}", random.Next(-180, 180), random.Next(0, 100));
        }
    }

    /// <summary>
    /// Generates a random date as a string, formatted according to the current culture's short date pattern.
    /// The year of the generated date will be between 1900 and the specified maxYear (default is 9999).
    /// </summary>
    /// <param name="maxYear">The maximum year for the generated date. Defaults to 9999.</param>
    /// <returns>A string representing the generated date.</returns>
    public static string Date(int maxYear = 9999)
    {
        int year = random.Next(1900, Math.Max(2000, maxYear) + 1);
        int month = random.Next(1, 13);
        int day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);

        DateTime date = new DateTime(year, month, day);

        return date.ToString(cultureInfo.DateTimeFormat.ShortDatePattern);
    }

    /// <summary>
    /// Generates a random time as a string, formatted according to the current culture's short time pattern.
    /// </summary>
    /// <returns>A string representing the generated time.</returns>
    public static string Time()
    {
        DateTime date = new DateTime(1, 1, 1, random.Next(0, 24), random.Next(0, 60), 0);

        return date.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
    }

    /// <summary>
    /// Generates a random date and time as a string, formatted according to the current culture's short date and time patterns.
    /// The year of the generated date will be between 1900 and the specified maxYear (default is 9999).
    /// </summary>
    /// <param name="maxYear">The maximum year for the generated date. Defaults to 9999.</param>
    /// <returns>A string representing the generated date and time.</returns>
    public static string Datetime(int maxYear = 9999)
    {
        return string.Format("{0} {1}", Date(maxYear), Time());
    }

    /// <summary>
    /// Generates a sequential ID (SID) as a string.
    /// Each call to this method increments the SID by 1, starting from 1.
    /// </summary>
    /// <returns>A string representing the generated SID.</returns>
    public static string Sid()
    {
        return Convert.ToString(++localSid);
    }

    /// <summary>
    /// Retrieves a random resource string from a JSON file based on the current culture.
    /// The resource files are expected to be located in a "culture" directory, with subdirectories for each culture.
    /// If the resource or culture file does not exist, a missing resource message is returned.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <returns>A string representing the retrieved resource.</returns>
    public static string Resource(string name)
    {
        if (LoadResource(name))
        {
            return resources[cultureInfo.Name][name][random.Next(resources[cultureInfo.Name][name].Length)];
        }

        return string.Format(missingResource, name, cultureInfo.Name);
    }

    /// <summary>
    /// Loads a resource file for the current culture if it exists.
    /// The resource file is expected to be a JSON file containing an array of strings.
    /// If the file is successfully loaded, the resource is stored in the 'resources' dictionary.
    /// </summary>
    /// <param name="name">The name of the resource to load.</param>
    /// <returns>True if the resource was successfully loaded; otherwise, false.</returns>
    private static bool LoadResource(string name)
    {
        if (!File.Exists(string.Format(baseName, cultureInfo.Name, name)))
        {
            return false;
        }
        else
        {
            if (!resources.TryGetValue(cultureInfo.Name, out Dictionary<string, string[]> resource))
            {
                resource = new Dictionary<string, string[]>();

                resources[cultureInfo.Name] = resource;
            }

            if (!resource.ContainsKey(name))
            {
                resource[name] = JsonSerializer.Deserialize<string[]>(File.ReadAllText(string.Format(baseName, cultureInfo.Name, name)));
            }
        }

        return true;
    }
}
