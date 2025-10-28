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
namespace CLI;

using System;

/// <summary>
/// A class for manipulating the terminal using ANSI escape codes.
/// It is actually a wrapper around Console methods, so it can be used in any console application.
/// It supports method chaining, and adds methods for setting colors, styles, cursor movement.
/// </summary>
public class Terminal
{
    private static readonly int bkBase = 40;
    private static readonly int fgBase = 30;

    /// <summary>
    /// Colors supported by ANSI escape codes.
    /// </summary>
    public enum Color
    {
        /// <summary>
        /// Black color.
        /// </summary>
        Black,
        /// <summary>
        /// Red color.
        /// </summary>
        Red,
        /// <summary>
        /// Green color.
        /// </summary>
        Green,
        /// <summary>
        /// Yellow color.
        /// </summary>
        Yellow,
        /// <summary>
        /// Blue color.
        /// </summary>
        Blue,
        /// <summary>
        /// Magenta color.
        /// </summary>
        Magenta,
        /// <summary>
        /// Cyan color.
        /// </summary>
        Cyan,
        /// <summary>
        /// White color.
        /// </summary>
        White,
    }

    /// <summary>
    /// Erase modes for screen and line erasing.
    /// </summary>
    public enum EraseMode
    {
        /// <summary>
        /// Erase from the cursor to the end of the line.
        /// </summary>
        FromCursorToEnd = 0,
        /// <summary>
        /// Erase from the cursor to the beginning of the line.
        /// </summary>
        FromCursorToBeginning = 1,
        /// <summary>
        /// Erase the entire line.
        /// </summary>
        Full = 2,
    }

    /// <summary>
    /// Text styles supported by ANSI escape codes.
    /// </summary>
    public enum Style
    {
        /// <summary>
        /// Reset all styles.
        /// </summary>
        Reset = 0,

        /// <summary>
        /// Bold text.
        /// </summary>
        Bold = 1,
        /// <summary>
        /// Dim text.
        /// </summary>
        Dim = 2,
        /// <summary>
        /// Italic text.
        /// </summary>
        Italic = 3,
        /// <summary>
        /// Underlined text.
        /// </summary>
        Underline = 4,
        /// <summary>
        /// Blinking text.
        /// </summary>  
        Blink = 5,
        /// <summary>
        /// Inverse colors.
        /// </summary>
        Inverse = 7,
        /// <summary>
        /// Hidden text.
        /// </summary>
        Hidden = 8,
        /// <summary>
        /// Strikethrough text.
        /// </summary>
        StrikeThrough = 9,

        /// <summary>
        /// Turn off bold or dim text.
        /// </summary>
        BoldOff = 22,
        /// <summary>
        /// Same as <see cref="BoldOff"/>.
        /// </summary>
        DimOff = 22,
        /// <summary>
        /// Turn off italic text.
        /// </summary>
        ItalicOff = 23,
        /// <summary>
        /// Turn off underlined text.
        /// </summary>
        UnderlineOff = 24,
        /// <summary>
        /// Turn off blinking text.
        /// </summary>
        BlinkOff = 25,
        /// <summary>
        /// Turn off inverse colors.
        /// </summary>
        InverseOff = 27,
        /// <summary>
        /// Turn off hidden text.
        /// </summary>
        Visible = 28,
        /// <summary>
        /// Turn off strikethrough text.
        /// </summary>
        StrikeThroughOff = 29,
    }

    /// <summary>
    /// Cursor movement directions supported by ANSI escape codes.
    /// </summary>
    public enum Cursor
    {
        /// <summary>
        /// Move cursor to home position.
        /// </summary>
        Home = 'H',
        /// <summary>
        /// Move cursor up.
        /// </summary>
        Up = 'A',
        /// <summary>
        /// Move cursor down.
        /// </summary>
        Down = 'B',
        /// <summary>
        /// Move cursor forward.
        /// </summary>
        Forward = 'C',
        /// <summary>
        /// Move cursor backward.
        /// </summary>
        Backward = 'D',
        /// <summary>
        /// Move cursor to the next line.
        /// </summary>
        NextLine = 'E',
        /// <summary>
        /// Move cursor to the previous line.
        /// </summary>
        PreviousLine = 'F',
        /// <summary>
        /// Move cursor to a specific column.
        /// </summary>
        ToColumn = 'G',
        /// <summary>
        /// Scroll the view up.
        /// </summary>
        ScrollUp = 'S',
        /// <summary>
        /// Scroll the view down.
        /// </summary>
        ScrollDown = 'T',
    }

    /// <summary>
    /// Clear the console screen.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Clear()
    {
        Console.Clear();
        return this;
    }

    /// <summary>
    /// Sound the terminal bell.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Bell(int times = 1)
    {
        Console.Write(new string('\a', times));
        return this;
    }

    /// <summary>
    /// Backspace character.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Backspace(int times = 1)
    {
        Console.Write(new string('\b', times));
        return this;
    }

    /// <summary>
    /// Horizontal tab character.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal HTab(int times = 1)
    {
        Console.Write(new string('\t', times));
        return this;
    }

    /// <summary>
    /// Fake tab using spaces (default is 2 spaces).
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal SpaceTab(int times = 2)
    {
        Console.Write(new string(' ', times));
        return this;
    }

    /// <summary>
    /// Line feed character.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal LineFeed(int times = 1)
    {
        Console.Write(new string('\n', times));
        return this;
    }

    /// <summary>
    /// Vertical tab character.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal VTab(int times = 1)
    {
        Console.Write(new string('\v', times));
        return this;
    }

    /// <summary>
    /// Carriage return character.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal CarriageReturn()
    {
        Console.Write('\r');
        return this;
    }

    /// <summary>
    /// Delete character.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Delete(int times = 1)
    {
        Console.Write(new string((char)127, times));
        return this;
    }

    /// <summary>
    /// Sets the background color.
    /// </summary>
    /// <param name="color">The background color to set.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal SetBkColor(Color color)
    {
        Console.Write($"\u001b[{bkBase + (int)color}m");
        return this;
    }

    /// <summary>
    /// Sets the foreground color.
    /// </summary>
    /// <param name="color">The foreground color to set.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal SetFgColor(Color color)
    {
        Console.Write($"\u001b[{fgBase + (int)color}m");
        return this;
    }

    /// <summary>
    /// Sets the text style.
    /// </summary>
    /// <param name="style">The text style to set.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal SetStyle(Style style)
    {
        Console.Write($"\u001b[{(int)style}m");
        return this;
    }

    /// <summary>
    /// Erases part or all of the screen.
    /// </summary>
    /// <param name="mode">The erase mode to use. Defaults to <see cref="EraseMode.Full"/>.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal EraseScreen(EraseMode mode = EraseMode.Full)
    {
        Console.Write($"\u001b[{(int)mode}J");
        return this;
    }

    /// <summary>
    /// Erases part or all of the current line.
    /// </summary>
    /// <param name="mode">The erase mode to use. Defaults to <see cref="EraseMode.Full"/>.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal EraseLine(EraseMode mode = EraseMode.Full)
    {
        Console.Write($"\u001b[{(int)mode}K");
        return this;
    }

    /// <summary>
    /// Moves the cursor in the specified direction by the specified number of positions.
    /// </summary>
    /// <param name="direction">The direction to move the cursor.</param>
    /// <param name="n">The number of positions to move the cursor. Defaults to 0, which means 1 position.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal MoveCursor(Cursor direction, int n = 0)
    {
        Console.Write($"\u001b[{n}{(char)direction}");
        return this;
    }

    /// <summary>
    /// Moves the cursor to the specified row and column.
    /// </summary>
    /// <param name="row">The row to move the cursor to. Defaults to 0.</param>
    /// <param name="col">The column to move the cursor to. Defaults to 0.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal MoveCursorTo(int row = 0, int col = 0)
    {
        Console.Write($"\u001b[{row};{col}H");
        return this;
    }

    /// <summary>
    /// Writes a boolean value to the console.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(bool value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a character to the console.
    /// </summary>
    /// <param name="value">The character to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(char value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a character array to the console.
    /// </summary>
    /// <param name="buffer">The character array to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(char[] buffer)
    {
        Console.Write(buffer);
        return this;
    }

    /// <summary>
    /// Writes a subarray of characters to the console.
    /// </summary>
    /// <param name="buffer">The character array to write from.</param>
    /// <param name="index">The starting index in the array.</param>
    /// <param name="count">The number of characters to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(char[] buffer, int index, int count)
    {
        Console.Write(buffer, index, count);
        return this;
    }

    /// <summary>
    /// Writes a decimal value to the console.
    /// </summary>
    /// <param name="value">The decimal value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(decimal value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a double value to the console.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(double value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a float value to the console.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(float value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes an integer value to the console.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(int value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes an unsigned integer value to the console.
    /// </summary>
    /// <param name="value">The unsigned integer value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(uint value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a long value to the console.
    /// </summary>
    /// <param name="value">The long value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(long value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes an unsigned long value to the console.
    /// </summary>
    /// <param name="value">The unsigned long value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(ulong value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes an object to the console.
    /// </summary>
    /// <param name="value">The object to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(object value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a string to the console.
    /// </summary>
    /// <param name="value">The string to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal Write(string value)
    {
        Console.Write(value);
        return this;
    }

    /// <summary>
    /// Writes a new line to the console.
    /// </summary>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine()
    {
        Console.WriteLine();
        return this;
    }

    /// <summary>
    /// Writes a boolean value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(bool value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes a character followed by a new line to the console.
    /// </summary>
    /// <param name="value">The character to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(char value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes a character array followed by a new line to the console.
    /// </summary>
    /// <param name="buffer">The character array to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(char[] buffer)
    {
        Console.WriteLine(buffer);
        return this;
    }

    /// <summary>
    /// Writes a subarray of characters followed by a new line to the console.
    /// </summary>
    /// <param name="buffer">The character array to write from.</param>
    /// <param name="index">The starting index in the array.</param>
    /// <param name="count">The number of characters to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(char[] buffer, int index, int count)
    {
        Console.WriteLine(buffer, index, count);
        return this;
    }

    /// <summary>
    /// Writes a decimal value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The decimal value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(decimal value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes a double value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(double value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes a float value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(float value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes an integer value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(int value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes an unsigned integer value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The unsigned integer value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(uint value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes a long value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The long value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(long value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes an unsigned long value followed by a new line to the console.
    /// </summary>
    /// <param name="value">The unsigned long value to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(ulong value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes an object followed by a new line to the console.
    /// </summary>
    /// <param name="value">The object to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(object value)
    {
        Console.WriteLine(value);
        return this;
    }

    /// <summary>
    /// Writes a string followed by a new line to the console.
    /// </summary>
    /// <param name="value">The string to write.</param>
    /// <returns>The current Terminal instance.</returns>
    public Terminal WriteLine(string value)
    {
        Console.WriteLine(value);
        return this;
    }
}
