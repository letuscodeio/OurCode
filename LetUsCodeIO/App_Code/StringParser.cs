using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetUsCodeIO.App_Code
{
    /// <summary>
    /// Provides a variety of useful string parsing functions.
    /// </summary>
    public class StringParser
    {

        /// <summary>Single quote character.</summary>
        public const char SingleQuote = '\'';

        /// <summary>Double quote character.</summary>
        public const char DoubleQuote = '\"';

        //  Other characters for SQL parsing
        public const char cComma = ',';

        //  Characters used for operators and separators while parsing    
        public static readonly char[] OpChars = new char[] { '+', '-', '*', '/', '%', '\\', '^', '(', ')', '&', '|', '!', '<', '>', '=', '!', ',' };

        //  Operators that are two characters
        public static readonly string[] OpChars2 = new string[] { "!=", "<>", "<=", ">=", "**", "&&", "||" };


        #region Command Line/Option Parsing
        /// 
        /// <summary>
        /// Delegate for handling string argument parsing
        /// </summary>    
        /// <param name="Index">Index of argument</param>
        /// <param name="Prefix">Prefix for argument (e.g. "/")</param>
        /// <param name="Name">Name of argument</param>
        /// <param name="Separator">Separator between name and value (e.g. ":")</param>
        /// <param name="Value">Value of argument</param>
        /// <returns>true if valid and processed, false if invalid</returns>
        /// 
        public delegate bool ArgumentHandler(int Index, string Prefix, string Name, string Separator, string Value);

        ///
        /// <summary>
        /// Parse a "standard" command-line string to read position parameters and option keywords.
        /// </summary>
        /// <param name="Line">Command line.</param>
        /// <param name="Handler">Delegate to receive arguments as they are parsed</param>
        /// <returns>true if all arguments valid, false if one or more are invalid.</returns>
        /// 
        public static bool ParseCommandLine(string Line, ArgumentHandler Handler)
        {
            if (string.IsNullOrEmpty(Line))
                return true;

            int hasSlash = Line.IndexOf('/');

            string PosArgs, OptArgs;
            if (hasSlash >= 0)
            {
                PosArgs = hasSlash == 0 ? string.Empty : Line.Substring(0, hasSlash - 1);
                OptArgs = Line.Substring(hasSlash);
            }
            else
            {
                PosArgs = Line;
                OptArgs = string.Empty;
            }

            bool Success = true;
            int Index = 0;

            string[] Args = PosArgs.Split(' ');
            foreach (string Arg in Args)
            {
                string Parm = RemoveQuotes(Arg.Trim());
                if (Parm.Length > 0)
                    Success = Handler(Index++, null, Parm, null, null);
            }

            //Args = OptArgs.Split('/');
            //foreach (string Arg in OptArgs)
            Index = 0;
            string Opt;
            while ((Opt = ScanArgument(OptArgs, ref Index, '/')) != null)
            {
                string Parm = Opt.Trim();
                if (Parm.StartsWith("/"))
                    Parm = Parm.Substring(1);
                if (Parm.Length > 0)
                {
                    string Name, Sep, Value;
                    int nAt = Parm.IndexOf(':');
                    if (nAt >= 0)
                    {
                        Name = Parm.Substring(0, nAt);
                        Sep = ":";
                        Value = RemoveQuotes(Parm.Substring(nAt + 1).Trim());
                    }
                    else
                    {
                        Name = Parm;
                        Sep = null;
                        Value = null;
                    }
                    Success = Handler(Index++, "/", Name, Sep, Value);
                }
            }

            return Success;
        }
        #endregion


        #region String Scanning and Manipulation

        /// 
        /// <summary>
        /// Remove any quotes from a string.
        /// </summary>
        /// <param name="Line">Input string.</param>
        /// <returns>String without quotes.</returns>
        /// 
        public static string RemoveQuotes(string Line)
        {
            return RemoveQuotes(Line, false, '\0');
        }

        /// 
        /// <summary>
        /// Remove any quotes from a string, optionally triming before and after.
        /// </summary>
        /// <param name="Line">Input string.</param>
        /// <param name="TrimAll">true to trim string.</param>
        /// <returns>String without quotes.</returns>
        /// 
        public static string RemoveQuotes(string Line, bool TrimAll)
        {
            return RemoveQuotes(Line, TrimAll, '\0');
        }

        /// 
        /// <summary>
        /// Remove any quotes from a string, optionally triming before and after.
        /// </summary>
        /// <param name="Line">Input string.</param>
        /// <param name="TrimAll">true to trim string.</param>
        /// <param name="QuoteChar">The quote character to use (\0 defaults to '"')</param>
        /// <returns>String without quotes.</returns>
        /// 
        public static string RemoveQuotes(string Line, bool TrimAll, char QuoteChar)
        {
            if (QuoteChar == '\0') QuoteChar = '"';
            if (TrimAll)
                Line = Line.Trim();
            if (Line.Length > 1 && Line[0] == QuoteChar)
            {
                int nLen = Line.Length;
                if (Line[--nLen] == Line[0])
                    nLen--;
                Line = Line.Substring(1, nLen);
                if (TrimAll)
                    Line = Line.Trim();
            }
            return Line;
        }

        /// <summary>
        /// Removes multiple spaces with in a string 
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static string CompressString(string Input)
        {
            if (!string.IsNullOrEmpty(Input))
            {
                int i = 0;
                string Invalid = "  ";
                while ((i = Input.IndexOf(Invalid, i)) >= 0)
                    Input = Input.Remove(i, 1);
            }
            return Input;
        }

        /// <summary>
        /// Replace "control" literals in a string (e.g. "\n")
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static string ReplaceControlLiterals(string Input)
        {
            return ReplaceControlLiterals(Input, '\\');
        }

        /// <summary>
        /// Replace "control" literals in a string (e.g. "\n")
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="CtlChar"></param>
        /// <returns></returns>
        public static string ReplaceControlLiterals(string Input, char CtlChar)
        {
            int next = Input.IndexOf(CtlChar);
            if (next < 0)
                return Input;

            StringBuilder Output = new StringBuilder(Input.Length);
            int at = 0;
            while (at < Input.Length)
            {
                if (next > at)
                    Output.Append(Input.Substring(at, next - at));

                at = next + 1;
                bool AddSep = true;
                if (at < Input.Length)
                {
                    if (Input[at] == CtlChar)
                    {
                        //  Replace "\\" with "\"
                        at++;
                    }
                    else if (char.ToLower(Input[at]) == 'n')
                    {
                        //  "\n" = new line
                        Output.Append('\n');
                        at++;
                        AddSep = false;
                    }
                }

                //  Leave unchanged:
                if (AddSep)
                    Output.Append(CtlChar);
                next = Input.IndexOf(CtlChar, at);
                if (next < 0)
                {
                    Output.Append(Input.Substring(at));
                    break;
                }
            }
            return Output.ToString();
        }

        /// 
        /// <summary>
        /// Scans for the end of an "argument" in a string.
        /// </summary>
        /// <remarks>
        /// An "argument" is a word ending with white-space or a phrase in quotes. 
        /// Optionally, an additional separator character may be supplied (e.g. '/' 
        /// for parsing command line arguments).
        /// <para>
        /// Any initial white space in the string is skipped. A null value is returned if 
        /// there is nothing else present.
        /// </para>
        /// </remarks>
        /// <param name="Line">The line to be scanned.</param>
        /// <param name="Index">The starting index, updated to reflect the next position.</param>
        /// <param name="Sep">Optional separator characters (null for only white space.)</param>
        /// <param name="StopOnSpace">True to stop on first white space found.</param>
        /// <returns>The argument substring</returns>
        /// 
        public static string ScanArgument(string Line, ref int Index, char[] Sep, bool StopOnSpace)
        {
            if (!SkipSpace(Line, ref Index))
                return null;

            int Start = Index;
            int End = -1;

            if (Line[Start] == '\"')
            {
                End = Line.IndexOf('\"', ++Start);
                if (End < 0)
                {
                    End = Line.Length;
                    Index = End;
                }
                else
                    Index = End + 1;
            }
            else
            {
                while (Index < Line.Length)
                {
                    if (Sep != null && Index > Start)
                    {
                        if (Array.IndexOf(Sep, Line[Index]) >= 0)
                            break;
                    }
                    if (StopOnSpace && char.IsWhiteSpace(Line[Index]))
                        break;
                    if (Line[Index] == '\"')
                    {
                        Index = Line.IndexOf('\"', ++Index);
                        if (Index < 0)
                            Index = Line.Length - 1;
                    }
                    Index++;
                }
            }

            if (End < 0)
                End = Index;

            string RetVal = Line.Substring(Start, End - Start);
            if (Sep != null)
                RetVal = RetVal.TrimEnd();
            return RetVal;
        }

        /// 
        /// <summary>
        /// Scans for the end of an "argument" in a string.
        /// </summary>
        /// <param name="Line">The line to be scanned.</param>
        /// <param name="Index">The starting index, updated to reflect the next position.</param>
        /// <param name="Sep">Optional separator character (0 for white space only.)</param>
        /// <returns>The argument substring</returns>
        /// 
        public static string ScanArgument(string Line, ref int Index, char Sep)
        {
            return ScanArgument(Line, ref Index, Sep == (char)0 ? null : new char[] { Sep }, true);
        }

        /// 
        /// <summary>
        /// Scans for first non-whitespace character in a string.
        /// </summary>
        /// <param name="Line">The line to be scanned.</param>
        /// <param name="Index">The starting index, updated to reflect the next position.</param>
        /// <returns>true if found, false if no more data on line</returns>
        /// 
        public static bool SkipSpace(string Line, ref int Index)
        {
            while (Index < Line.Length)
            {
                if (!char.IsWhiteSpace(Line[Index]))
                    break;
                Index++;
            }
            return (Index < Line.Length);
        }

        /// 
        /// <summary>
        /// Checks whether a string is null or consists only of whitespace.
        /// </summary>
        /// <param name="Input">String to check.</param>
        /// <returns>
        /// True if string is null or consists only of whitespace, false otherwise.
        /// </returns>
        /// 
        public static bool IsNullOrWS(string Input)
        {
            return (Input == null || Input.Trim().Length == 0);
        }

        /// 
        /// <summary>
        /// Determines whether the specified input is null.
        /// <remarks>Works much like the sql statement ifnull() </remarks>
        /// </summary>
        /// <param name="Input">The input.</param>
        /// <param name="AppName">Name of the app.</param>
        /// <param name="DefaultValue">The default value if the input value is null.</param>
        /// <returns>The desired string value</returns>
        /// 
        public static string IfNullWithAppName(string Input, string AppName, string DefaultValue)
        {
            if (Input == null)
                return DefaultValue;
            return Input.Replace("%appname%", AppName);
        }

        /// 
        /// <summary>
        /// Function to "escape" special chars in a XML string
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        ///             
        public static string EscapeXMLChars(string Input)
        {
            char[] SpecialChars = { '\"', '\'', '&', '<', '>', '\r', '\n' };
            string[] SpecialNames = { "&quot;", "&apos;", "&amp;", "&lt;", "&gt;", "&#xD;", "&#xA;" };
            StringBuilder Output = new StringBuilder(Input.Length);
            int Start = 0;
            int Next = Input.IndexOfAny(SpecialChars);
            if (Next < 0)
                return Input;

            do
            {
                if (Next > Start)
                    Output.Append(Input, Start, Next - Start);
                int Idx = Array.IndexOf(SpecialChars, Input[Next]);
                Output.Append(SpecialNames[Idx]);
                Start = ++Next;
            } while ((Next = Input.IndexOfAny(SpecialChars, Start)) > 0);

            if (Start < Input.Length)
                Output.Append(Input, Start, Input.Length - Start);

            return Output.ToString();
        }

        #endregion


        #region String Sorting Functions
        /// <summary>
        /// Helper class to compare strings using "natural" numeric comparison
        /// </summary>
        /// <remarks>
        /// Original code by Vasian Cepa, Optimized by Richard Deeming.
        /// </remarks>
        public class LogicalComparer : IComparer<string>
        {
            private bool zeroesFirst = false;

            /// <summary>
            /// Constructor.
            /// </summary>
            public LogicalComparer()
            {
            }

            /// <summary>
            /// Constructor for zero-first option.
            /// </summary>
            /// <remarks>
            /// Use this form to put "01,02, etc. before "1".
            /// </remarks>
            public LogicalComparer(bool zeroesFirst)
            {
                this.zeroesFirst = zeroesFirst;
            }

            /// <summary>
            /// Compar two values.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(string x, string y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return Compare((string)x, (string)y, zeroesFirst);
            }

            /// 
            /// <summary>
            /// Compare two strings.
            /// </summary>
            /// <param name="s1"></param>
            /// <param name="s2"></param>
            /// <param name="zeroesFirst"></param>
            /// <returns></returns>
            /// 
            public static int Compare(string s1, string s2, bool zeroesFirst)
            {
                if (null == s1 || 0 == s1.Length)
                {
                    if (null == s2 || 0 == s2.Length) return 0;
                    return -1;
                }
                else if (null == s2 || 0 == s2.Length)
                {
                    return 1;
                }

                int s1Length = s1.Length;
                int s2Length = s2.Length;

                bool sp1 = char.IsLetterOrDigit(s1[0]);
                bool sp2 = char.IsLetterOrDigit(s2[0]);

                if (sp1 && !sp2) return 1;
                if (!sp1 && sp2) return -1;

                char c1, c2;
                int i1 = 0, i2 = 0;
                int r = 0;
                bool letter1, letter2;

                while (true)
                {
                    c1 = s1[i1];
                    c2 = s2[i2];

                    sp1 = char.IsDigit(c1);
                    sp2 = char.IsDigit(c2);

                    if (!sp1 && !sp2)
                    {
                        if (c1 != c2)
                        {
                            letter1 = char.IsLetter(c1);
                            letter2 = char.IsLetter(c2);

                            if (letter1 && letter2)
                            {
                                c1 = char.ToUpper(c1);
                                c2 = char.ToUpper(c2);

                                r = c1 - c2;
                                if (0 != r) return r;
                            }
                            else if (!letter1 && !letter2)
                            {
                                r = c1 - c2;
                                if (0 != r) return r;
                            }
                            else if (letter1)
                            {
                                return 1;
                            }
                            else if (letter2)
                            {
                                return -1;
                            }
                        }

                    }
                    else if (sp1 && sp2)
                    {
                        r = CompareNumbers(s1, s1Length, ref i1, s2, s2Length, ref i2, zeroesFirst);
                        if (0 != r) return r;
                    }
                    else if (sp1)
                    {
                        return -1;
                    }
                    else if (sp2)
                    {
                        return 1;
                    }

                    i1++;
                    i2++;

                    if (i1 >= s1Length)
                    {
                        if (i2 >= s2Length) return 0;
                        return -1;
                    }
                    else if (i2 >= s2Length)
                    {
                        return 1;
                    }
                }
            }

            private static int CompareNumbers(
                string s1, int s1Length, ref int i1,
                string s2, int s2Length, ref int i2,
                bool zeroesFirst)
            {
                int nzStart1 = i1, nzStart2 = i2;
                int end1 = i1, end2 = i2;

                ScanNumber(s1, s1Length, i1, ref nzStart1, ref end1);
                ScanNumber(s2, s2Length, i2, ref nzStart2, ref end2);

                int start1 = i1;
                i1 = end1 - 1;
                int start2 = i2;
                i2 = end2 - 1;

                if (zeroesFirst)
                {
                    int zl1 = nzStart1 - start1;
                    int zl2 = nzStart2 - start2;
                    if (zl1 > zl2) return -1;
                    if (zl1 < zl2) return 1;
                }

                int length1 = end2 - nzStart2;
                int length2 = end1 - nzStart1;

                if (length1 == length2)
                {
                    int r;
                    for (int j1 = nzStart1, j2 = nzStart2; j1 <= i1; j1++, j2++)
                    {
                        r = s1[j1] - s2[j2];
                        if (0 != r) return r;
                    }

                    length1 = end1 - start1;
                    length2 = end2 - start2;

                    if (length1 == length2) return 0;
                }

                if (length1 > length2) return -1;
                return 1;
            }

            private static void ScanNumber(string s, int length, int start, ref int nzStart, ref int end)
            {
                nzStart = start;
                end = start;

                bool countZeros = true;
                char c = s[end];

                while (true)
                {
                    if (countZeros)
                    {
                        if ('0' == c)
                        {
                            nzStart++;
                        }
                        else
                        {
                            countZeros = false;
                        }
                    }

                    end++;
                    if (end >= length) break;

                    c = s[end];
                    if (!char.IsDigit(c)) break;
                }
            }
        }

        /// <summary>
        /// Helper class to compare strings using numeric comparison
        /// </summary>
        public class NaturalComparer : IComparer<string>
        {
            /// <summary>
            /// Compares two strings using numeric comparison
            /// </summary>
            /// <remarks>
            /// Originally written by Andrew Skalkin.
            /// </remarks>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(string x, string y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                int lx = x.Length, ly = y.Length;

                for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
                {
                    if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                    {
                        long vx = 0, vy = 0;

                        for (; mx < lx && char.IsDigit(x[mx]); mx++)
                            vx = vx * 10 + x[mx] - '0';

                        for (; my < ly && char.IsDigit(y[my]); my++)
                            vy = vy * 10 + y[my] - '0';

                        if (vx != vy)
                            return vx > vy ? 1 : -1;
                    }
                    if (mx < lx && my < ly && x[mx] != y[my])
                        return x[mx] > y[my] ? 1 : -1;
                }

                return lx - ly;
            }
        }

        #endregion


        #region String Splitting Functions

        /// 
        /// <summary>
        /// Split a string and cleanup/trim all values.
        /// </summary>
        /// <remarks>
        /// The comma separator is used by default if no separators are supplied.
        /// </remarks>
        /// <param name="Input"></param>
        /// <param name="ListSeparators"></param>
        /// <returns></returns>
        /// 
        public static string[] SplitClean(string Input, params char[] ListSeparators)
        {
            if (string.IsNullOrEmpty(Input))
                return new string[0];

            if (ListSeparators == null || ListSeparators.Length == 0)
                ListSeparators = new char[] { cComma };

            string[] results = Input.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = results[i].Trim();
            }
            return results;
        }

        /// <summary>
        /// Double Clean split 
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Split1"></param>
        /// <param name="ListSeparators"></param>
        /// <returns></returns>
        public static List<string[]> DoubleCleanSplit(string Input, char Split1, params char[] ListSeparators)
        {
            if (Input == null)
                return null;
            string[] Values = SplitClean(Input, Split1);
            List<string[]> ReturnValues = new List<string[]>(Values.Length);
            for (int i = 0; i < Values.Length; i++)
                ReturnValues.Add(SplitClean(Values[i], ListSeparators));

            return ReturnValues;
        }

        /// 
        /// <summary>
        /// Split a string with ranges.
        /// </summary>
        /// <remarks>This accepts numeric input ranges of the form "n-m" and creates n-m+1 items.</remarks>
        /// <param name="Input"></param>
        /// <param name="MaxItems">Maximum number of items.</param>
        /// <param name="ListSeparators"></param>
        /// <returns></returns>
        /// 
        public static string[] SplitWithRange(string Input, int MaxItems, params char[] ListSeparators)
        {
            if (string.IsNullOrEmpty(Input))
                return new string[0];

            if (Input.IndexOf('-') < 0)
                return SplitClean(Input, ListSeparators);

            if (ListSeparators == null || ListSeparators.Length == 0)
                ListSeparators = new char[] { cComma };

            string[] values = Input.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
            List<string> results = new List<string>(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
                if (values[i].IndexOf('-') < 0)
                    results.Add(values[i]);
                else
                {
                    string[] vals = values[i].Split('-');
                    int val1, val2;
                    if (int.TryParse(vals[0], out val1) && int.TryParse(vals[1], out val2)
                        && val1 > 0 && val2 >= val1)
                    {
                        for (int nval = val1; nval <= val2; nval++)
                        {
                            results.Add(nval.ToString());
                            if (MaxItems > 0 && results.Count >= MaxItems)
                                break;
                        }
                    }
                }
                if (MaxItems > 0 && results.Count >= MaxItems)
                    break;
            }
            return results.ToArray();
        }

        /// 
        /// <summary>
        /// Parse a comma-separated string into an array of values.
        /// </summary>
        /// <remarks>
        /// This function will be enhanced to provide additional options.
        /// </remarks>
        /// <param name="Value">The string to parse</param>
        /// <param name="RemoveEmpty">true to remove empty values</param>
        /// <returns></returns>
        /// 
        public static string[] FromCSV(string Value, bool RemoveEmpty)
        {
            return FromCSV(Value, RemoveEmpty, ',');
        }

        /// 
        /// <summary>
        /// Parse a comma-separated string into an array of values.
        /// </summary>
        /// <remarks>
        /// This function will be enhanced to provide additional options.
        /// </remarks>
        /// <param name="Value">The string to parse</param>
        /// <param name="RemoveEmpty">true to remove empty values</param>
        /// <param name="Comma">Separator characters</param>
        /// <returns></returns>
        /// 
        public static string[] FromCSV(string Value, bool RemoveEmpty, params char[] Comma)
        {
            StringSplitOptions Opts = RemoveEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
            string[] Vals = Value.Split(Comma, Opts);
            return Vals;
        }

        /// 
        /// <summary>
        /// Convert a string array to a single comma-separated list of values.
        /// </summary>
        /// <remarks>
        /// When adding quotes, this function does not presently "escape"
        /// existing quote characters in a string. That can be a future
        /// enhancement.
        /// </remarks>
        /// <param name="Values">The values</param>
        /// <param name="AddQuotes">true to add quotes to values</param>
        /// <param name="QuoteChar">The character to use for quoting</param>
        /// <returns></returns>
        /// 
        public static string ToCSV(string[] Values, bool AddQuotes, char QuoteChar)
        {
            StringBuilder Result = new StringBuilder(8 * Values.Length);
            foreach (string Val in Values)
            {
                if (Result.Length > 0)
                    Result.Append(',');
                if (AddQuotes)
                {
                    Result.Append(QuoteChar);
                    Result.Append(Val);
                    Result.Append(QuoteChar);
                }
                else
                    Result.Append(Val);
            }
            return Result.ToString();
        }

        /// 
        /// <summary>
        /// Convert a string array to a single comma-separated list of values.
        /// </summary>
        /// <param name="Values">The values</param>
        /// <param name="AddQuotes">true to add quotes to values</param>
        /// <returns></returns>
        /// 
        public static string ToCSV(string[] Values, bool AddQuotes)
        {
            return ToCSV(Values, AddQuotes, '\"');
        }

        /// 
        /// <summary>
        /// Convert a string array to a single comma-separated list of values.
        /// </summary>
        /// <param name="Values">The values</param>
        /// <returns></returns>
        /// 
        public static string ToCSV(params string[] Values)
        {
            return ToCSV(Values, false, '\"');
        }

        ///
        /// <summary>
        /// Splits the input line into a list of into proper sql delimited arguments
        /// <code>
        /// This will provide a string splitting opperations where commas are going to be used
        /// as part of the parametter. (e.g this could be because of a number or a string that
        ///                              has a comma as part of the syntext)
        /// </code>
        /// </summary>
        /// <remarks>
        /// This may need to be optimaized later to support more then just sql 
        /// </remarks>
        /// <param name="InputString">The string to be searched for throw for the proper location to split commas </param>
        /// <returns>List of varibles used</returns>
        /// 
        public static string[] LinearSQLSplit(string InputString)
        {
            string value = string.Empty;
            List<string> values = new List<string>();
            int position = 0, start = 0, resetPosition = 0, skip = 0;

            bool setEnd = false;
            do
            {
                if (position >= InputString.Length)
                    break;

                char current = InputString[position];
                if (char.IsWhiteSpace(current))
                {
                    ++skip;
                    ++position;
                    continue;
                }

                // 
                //  If quoted this needs a little more evaluation 
                //
                if (current == SingleQuote)
                {
                    resetPosition = QuotedString(InputString, position);
                    setEnd = true;
                }
                else
                {
                    if (char.IsDigit(InputString[((position + 2) > (InputString.Length - 1)) ? InputString.Length - 1 : position + 2])
                               && current != ':')
                    {
                        position++;
                        if (position == InputString.Length)
                        {
                            value = InputString.Substring((start + skip), (position - (start + skip)));
                            values.Add(value);
                        }
                        continue;
                    }
                    resetPosition = InputString.IndexOf(cComma, position);
                    setEnd = true;
                }

                //
                //  Set and reset the values
                //
                if (setEnd)
                {
                    if (resetPosition == -1)
                    {
                        // last string
                        resetPosition = InputString.Length;
                    }
                    value = InputString.Substring((start + skip), (resetPosition - (start + skip)));
                    value = value.Replace("\\'", "\'");
                    //if (value.Length > 1 && value[0] == cComma && value[1] == SingleQuote)
                    //    value = value.Substring(1);
                    if (value.Contains("'"))
                    {
                        QuoteTokenizer Tok = new QuoteTokenizer(value, 0);
                        //if(!Tok.Validate(value))
                        {
                            value = Tok.Scrub(value);
                        }
                    }
                    values.Add(value);
                    value = string.Empty;
                    setEnd = false;

                    position = resetPosition > 0 && InputString[(resetPosition - 1)] == SingleQuote
                                ? resetPosition : resetPosition + 1;
                    start = position;
                    skip = 0;
                }
                ++position;
            } while (true);

            return values.ToArray();
        }

        //
        // Check out what what is in the quoted string 
        //
        private static int QuotedString(string inputString, int current)
        {

            int start = current;
            int peek = current + 1;
            do
            {
                if (peek >= inputString.Length)
                    break;
                char newValue = inputString[peek];
                if (newValue == '\\')
                {
                    //  Back-slash defines an escape sequence
                    ++current;
                }

                else //if (newValue == SingleQuote)
                {
                    // check for  the end of the string
                    // 
                    QuoteTokenizer QZ = new QuoteTokenizer(inputString, current);
                    QZ.Parse();
                    current = QZ.Current;
                    break;
#if notused
                if ((peek + 1) == inputString.Length)
                    {
                    current = ++peek;
                    //newValue = inputString[--current];
                    break;
                    }
                newValue = inputString[ ++peek ];
                // '',
                if (newValue == cComma)
                    {
                    current = peek;
                    break;
                    }
                //'''
                else if (newValue == SingleQuote)
                    {
                    current = QuotedString(inputString, peek);
                    break;
                    }
#endif
                }

                ++current;
                peek = current + 1;
            } while (true);
            return current;
        }

        /// 
        /// <summary>Helper class to help parse a quoted string</summary>
        /// 
        public class QuoteTokenizer
        {
            // Input string starting out with
            private string _input;

            /// <summary>The current index in the string to start out with after parse</summary>
            public int Current
            { get { return _current == _input.Length ? _current - 1 : _current; } }
            private int _current;

#if notused
        private int getCurrent()
            {
            if(_current == _input.Length)
                return  _current - 1;
            if (_input[_current] == '\'' && _input[_current + 1] != '\'')
                ++_current;
            return _current;
            }
#endif
            /// 
            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="Input">Input string to parse</param>
            /// <param name="Current">Current index to start parse with</param>
            /// 
            public QuoteTokenizer(string Input, int Current)
            {
                _input = Input;
                _current = Current;
            }


            public bool Validate(string Value)
            {
                int Count = 0;
                foreach (Char C in Value)
                {
                    if (C == '\'')
                        ++Count;
                }
                return Count % 2 == 0;
            }

            /// <summary>
            /// Current Paring errors only show at begin of string value
            /// </summary>
            /// <param name="Value"></param>
            /// <returns></returns>
            public string Scrub(string Value)
            {
                _input = Value.Trim();
                if (_input.Length > 0 && _input[0] == '\'')
                {
                    // Fix strings that should be empty strings
                    if (_input.Length == 1)
                        _input = string.Empty;
                    else if (Validate(_input.Substring(1)))
                    {
                        _input = _input.Substring(1);
                    }
                    if (!Validate(_input))
                    {
                        if (_input[_input.Length] == SingleQuote && !Validate(_input.Substring(0, _input.Length - 2)))
                        {
                            // Fix string 'records''
                            _input = _input.Substring(0, _input.Length - 2);
                        }
                    }
                }

                if (Validate(_input) && _input.Length > 2 &&
                    _input[0] == cComma && _input[1] == SingleQuote)
                {
                    // Fix strings ",'records'"
                    _input = _input.Substring(1);
                }

                return _input;
            }

            /// 
            /// <summary>
            /// Parse the string sequement for single quotes
            /// </summary>
            /// <returns></returns>
            /// 
            public bool Parse()
            {
                Stack<Quote> Queue = new Stack<Quote>();
                int Length = _input.Length - _current;
                ++_current;
                Quote Current = InitQuote();
                Current.IsFirst = true;
                for (int i = 0; i < Length; i++)
                {
                    int nAt = _current;
                    // if(
                    if (nAt >= _input.Length)
                        break;
                    if (_input[nAt] == '\'')
                    {
                        Queue.Push(Current);
                        ++i;
                        ++nAt;
                        ++_current;
                        // Double quote '' escape string
                        bool Double = nAt >= _input.Length ? false : _input[nAt] == '\'';
                        //if(!Current.Ended)
                        //Current.Ended = Double;

                        if (Double)
                        {
                            ++_current;
                            Current = InitQuote();
                            Current.Ended = true;
                        }
                        //Queue.Push(Current);
                        else
                        {
                            do
                            {
                                Current = Queue.Pop();
                                if (!Current.Ended)
                                {
                                    Current.Ended = true;
                                    break;
                                }
                            } while (Queue.Count > 0);

                            // Found end of string start break out
                            if (Current.IsFirst)
                                break;
                        }
                    }
                    else
                    {
                        ++_current;
                    }
                }

                return Queue.Count == 0 && Current.Ended && Current.IsFirst;
            }

            private Quote InitQuote()
            {
                Quote Q = new Quote();
                Q.Started = true;
                return Q;
            }

            /// <summary>Location information for quote</summary>
            protected class Quote
            {
                public bool Started;
                public bool Ended;
                public bool IsFirst;
            }

        }

        #endregion


        #region Expression Parsing

        /// <summary>
        /// Defines an operator node for expression parsing and evaluation
        /// </summary>
        public class OPNode
        {

            /// <summary>The operator to apply to values.</summary>
            public string Operator;

            /// <summary>Value for leaf nodes only</summary>
            public string LeafValue;

            /// <summary>Parameters for a function call.</summary>
            public OPNode[] FunctionParameters;

            /// <summary>Left child node.</summary>
            public OPNode LeftChild;

            /// <summary>Right child node.</summary>
            public OPNode RightChild;

            /// <summary>True if node is a leaf node.</summary>
            public bool IsLeaf
            { get { return LeafValue != null; } }

            /// <summary>True if node represents a function call.</summary>        
            public bool IsFunctionCall
            { get { return FunctionParameters != null; } }

            /// <summary>Parent whose LeftChild = this</summary>
            /// <remarks>This is used while constructing the tree.</remarks>       
            internal OPNode LeftParent;

            /// <summary>Parent whose RightChild = this</summary>
            /// <remarks>This is used while constructing the tree.</remarks>
            internal OPNode RightParent;

            /// <summary>
            /// Default constructor.
            /// </summary>
            public OPNode()
            {
            }

            /// <summary>Constructor for an operator node.</summary>
            /// <param name="OP"></param>
            /// <param name="Left"></param>
            /// <param name="Right"></param>
            internal OPNode(string OP, OPNode Left, OPNode Right)
            {
                Operator = OP;
                SetLeftChild(Left);
                SetRightChild(Right);
            }

            /// <summary>
            /// Constructor for a leaf node.
            /// </summary>
            /// <param name="Value"></param>   
            internal OPNode(string Value)
            {
                LeafValue = Value;
            }

            /// <summary>
            /// Set the left child while building the tree.
            /// </summary>
            /// <param name="Left"></param>
            internal void SetLeftChild(OPNode Left)
            {
                LeftChild = Left;
                if (Left != null)
                    Left.LeftParent = this;
            }

            /// <summary>
            /// Set the right child while building the tree.
            /// </summary>
            /// <param name="Right"></param>
            internal void SetRightChild(OPNode Right)
            {
                RightChild = Right;
                if (Right != null)
                    Right.RightParent = this;
            }
        }

        /// 
        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <remarks>
        /// This function builds an evaluation tree. The root note is the last operation to be performed, 
        /// each node must evaluate its child nodes first.
        /// </remarks>
        /// <param name="Expression">The expression.</param>
        /// <param name="BadIndex">Index of error, -1 if successful</param>
        /// <returns>Root node for evaluation tree</returns>
        /// 
        public static OPNode ParseExpression(string Expression, out int BadIndex)
        {
            int Next = 0;
            int Level = 0;
            OPNode Root = parseExpression(Expression, ref Next, ref Level, false, out BadIndex);
            if (Root != null)
            {
                cleanNodes(Root);
                while (Root.Operator == null && Root.RightChild == null && Root.LeftChild != null && !Root.IsLeaf)
                    Root = Root.LeftChild;
            }
            return Root;
        }

        /// 
        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <remarks>
        /// This function builds an evaluation tree. The root note is the last operation to be performed, 
        /// each node must evaluate its child nodes first.
        /// </remarks>
        /// <param name="Expression">The expression.</param>
        /// <param name="Next">Next index in expression.</param>
        /// <param name="Level">Recursion level</param>
        /// <param name="FunctionCall">true if parsing a function call.</param>
        /// <param name="BadIndex">Index of error, -1 if successful</param>
        /// <returns>Root node for evaluation tree</returns>
        /// 
        private static OPNode parseExpression(string Expression, ref int Next, ref int Level, bool FunctionCall, out int BadIndex)
        {
            //    
            //  A current node is used during parsing. 
            //  A root node is used to contain the tree as it it built.
            //  The left child of the root node is the topmost node of the tree.
            //
            OPNode Current = new OPNode();
            OPNode Root = new OPNode(null, Current, null);
            List<OPNode> FuncParms = null;
            //bool LastWasOperator = false;
            bool LastWasOperand = false;

            int Max = Expression.Length - 1;
            while (StringParser.SkipSpace(Expression, ref Next))
            {
                //
                //  Parse next operand, check for operators.
                //
                string Arg;
                BadIndex = Next;

                int OpIndex = Array.IndexOf(OpChars, Expression[Next]);
                if (OpIndex >= 0)
                {
                    //  Operator:
                    int OpLen = (Next < Max && Array.IndexOf(OpChars2, Expression.Substring(Next, 2)) >= 0)
                                ? 2 : 1;
                    Arg = Expression.Substring(Next, OpLen);
                    Next += OpLen;
                    if (Next <= Max && Array.IndexOf(OpChars, Expression[Next]) >= 0
                        && Arg != "(" && Arg != ")")
                    {
                        //  Invalid operator.
                        return null;
                    }
                }
                else
                {
                    //  Not an operator: Scan for the end of an argument,
                    //  then check for "OR" and "AND" as operators:
                    Arg = StringParser.ScanArgument(Expression, ref Next, OpChars, true);
                    if (Arg.Equals("or", StringComparison.OrdinalIgnoreCase))
                    {
                        Arg = "||";
                        OpIndex = 99;
                    }
                    else if (Arg.Equals("and", StringComparison.OrdinalIgnoreCase))
                    {
                        Arg = "&&";
                        OpIndex = 99;
                    }
                }

                if (OpIndex >= 0)
                {
                    if (Arg == "(")
                    {
                        //
                        //  Left parenthesis: Recursive parsing.
                        //
                        bool IsFunction = false;
                        if (LastWasOperand)
                        {
                            //  Must be a function evaluation.
                            if (Current.RightChild == null)
                            {
                                Arg = Current.LeftChild.LeafValue;
                                Current.LeftChild = null;
                            }
                            else
                            {
                                Arg = Current.RightChild.LeafValue;
                                Current.RightChild = null;
                            }
                            if (string.IsNullOrEmpty(Arg))
                                return null;
                            IsFunction = true;
                        }

                        if (Current.RightChild != null && !IsFunction)
                        {
                            //  Invalid placement
                            return null;
                        }

                        Level++;
                        OPNode NewNode = parseExpression(Expression, ref Next, ref Level, IsFunction, out BadIndex);
                        if (NewNode == null)
                            return null;
                        Level--;
                        NewNode.Operator = Arg;
                        if (Current.LeftChild == null)
                            Current.SetLeftChild(NewNode);
                        else
                            Current.SetRightChild(NewNode);
                    }
                    else if (Arg == ")")
                    {
                        //
                        //  Right parenthesis: Terminate recursive parsing.
                        //
                        if (Level == 0)
                        {
                            //  No matching right parenthesis:
                            return null;
                        }
                        if (FunctionCall)
                            addFunctionParmNode(ref FuncParms, Root, null);
                        break;
                    }
                    else if (Arg == ",")
                    {
                        //
                        //  Comma: Separates function parameters
                        //
                        if (!FunctionCall)
                            return null;
                        Current = new OPNode();
                        addFunctionParmNode(ref FuncParms, Root, Current);
                    }
                    else if (string.IsNullOrEmpty(Current.Operator))
                    {
                        //
                        //  No previous operator: Use new one.
                        //
                        Current.Operator = Arg;
                    }
                    else if (funcPriority(Arg) <= funcPriority(Current.Operator))
                    {
                        //
                        //  New operation has lower or equal priority to the previous
                        //  operator: Evaluate the previous operator first. The new 
                        //  operation becomes the parent of the old.
                        //
                        OPNode LP = Current.LeftParent;
                        OPNode RP = Current.RightParent;
                        OPNode NewNode = new OPNode(Arg, Current, null);
                        if (LP != null)
                            LP.SetLeftChild(NewNode);
                        if (RP != null)
                            RP.SetRightChild(NewNode);
                        Current = NewNode;
                    }
                    else
                    {
                        //
                        //  New operation has higher priority than the previous: Evaluate
                        //  the new operator first. The new operation becomes a child of
                        //  the previous.
                        //
                        OPNode NewNode = new OPNode(Arg, Current.RightChild, null);
                        Current.SetRightChild(NewNode);
                        Current = NewNode;
                    }

                    //LastWasOperator = true;
                    LastWasOperand = false;
                }
                else
                {
                    if (Current.RightChild != null)
                    {
                        //  Two operands in sequence: Not valid.
                        BadIndex = Next;
                        return null;
                    }

                    //
                    //  Set operand as a leaf node.
                    //            
                    OPNode Leaf = new OPNode(Arg);
                    if (Current.LeftChild == null)
                        Current.SetLeftChild(Leaf);
                    else
                        Current.SetRightChild(Leaf);

                    //LastWasOperator = false;
                    LastWasOperand = true;
                }

            }

            if (FuncParms != null && FuncParms.Count > 0)
                Root.FunctionParameters = FuncParms.ToArray();
            BadIndex = -1;
            return Root;
        }

        //
        //  Helper function to add node to function parm. list
        //
        private static void addFunctionParmNode(ref List<OPNode> FuncParms, OPNode Root, OPNode NewCurrent)
        {
            if (FuncParms == null)
                FuncParms = new List<OPNode>();
            OPNode ParmNode = Root.LeftChild;
            if (ParmNode.Operator == null && ParmNode.RightChild == null && !ParmNode.IsLeaf)
                ParmNode = ParmNode.LeftChild;
            FuncParms.Add(ParmNode);
            Root.SetLeftChild(NewCurrent);
        }

        //
        //  Helper function to cleanup nodes after tree is built.
        //
        private static void cleanNodes(OPNode Current)
        {
            if (Current.LeftChild != null)
            {
                Current.LeftChild.LeftParent = null;
                Current.LeftChild.RightParent = null;
                cleanNodes(Current.LeftChild);
            }
            if (Current.RightChild != null)
            {
                Current.RightChild.RightParent = null;
                Current.RightChild.RightParent = null;
                cleanNodes(Current.RightChild);
            }
        }

        /// 
        /// <summary>
        /// Get the priority for an operator.
        /// </summary>
        /// <param name="Op">The operator.</param>
        /// <returns>Priority.</returns>
        /// 
        private static int funcPriority(string Op)
        {
            switch (Op)
            {
                case "&":
                case "|":
                case "||":
                case "&&":
                    return 0;

                case "<":
                case ">":
                case "<=":
                case ">=":
                case "=":
                case "==":
                case "<>":
                case "!=":
                    return 1;

                case "+":
                case "-":
                    return 2;

                case "*":
                case "/":
                case "\\":
                    return 3;

                case "^":
                case "**":
                    return 5;

                case "%":
                    return 6;

                case "(":
                    return 99;
            }

            return 0;
        }
        #endregion


        #region String Formatting Using Value Substitutions


        /// 
        /// <summary>Substitute table column values in a string.</summary>
        /// <remarks>
        /// Column names are encoded as "{}" in an input string. A name may 
        /// use a suffix of ":dn", where "dn" is a decimal numeric format string or ":tx",
        /// where "x" is a date/time format string or ":fx", where "x" is one of the
        /// DataConverter.DateFormat values.
        /// </remarks>
        /// <param name="Row">The DataRow</param>
        /// <param name="Input">The input string.</param>
        /// <returns>The substituted result.</returns>
        /// 
        public static string SubstituteValues(DataRow Row, string Input)
        {
            StringBuilder Output = new StringBuilder(Input.Length);
            int nAt = 0;
            while (nAt < Input.Length)
            {
                //  Find the opening brace.
                int nSep = Input.IndexOf('{', nAt);
                if (nSep < 0) break;

                if (nSep > nAt)
                {
                    Output.Append(Input.Substring(nAt, nSep - nAt));
                    nAt = nSep;
                }

                //  Find the closing brace.
                nSep = Input.IndexOf('}', nAt);
                if (nSep < 0) break;

                //  Get name and optional format specifier.
                string AttrName = Input.Substring(nAt + 1, nSep - nAt - 1).Trim();
                string Format = null;
                int nPrec = AttrName.IndexOf(':');
                if (nPrec > 0)
                {
                    Format = AttrName.Substring(nPrec + 1).Trim();
                    AttrName = AttrName.Substring(0, nPrec).Trim();
                }

                //  Get the attribute value.
                if (AttrName.Length > 0)
                {
                    object ValueObj = Row.Table.Columns.Contains(AttrName) ? Row[AttrName] : null;
                    string AttrValue;
                    if (string.IsNullOrEmpty(Format))
                        AttrValue = ValueObj == null ? null : ValueObj.ToString();
                    else if (Format[0] == 'd' || Format[0] == 'D')
                    {
                        int AttrNum = DataConverter.SafeReadInteger(ValueObj);
                        try { AttrValue = AttrNum.ToString(Format); }
                        catch (Exception) { AttrValue = null; }
                    }
                    else if (Format[0] == 't' || Format[0] == 'T')
                    {
                        DateTime TimeVal = DataConverter.SafeReadDate(ValueObj);
                        try { AttrValue = TimeVal.ToString(Format.Substring(1)); }
                        catch (Exception) { AttrValue = null; }
                    }
                    else if (Format[0] == 'f' || Format[0] == 'F')
                    {
                        try
                        {
                            AttrValue = DataConverter.GetFormattedDate(ValueObj, (DataConverter.DateFormat)
                                       (int.Parse(Format.Substring(1))));
                        }
                        catch (Exception) { AttrValue = null; }
                    }
                    else
                    {
                        decimal AttrNum = DataConverter.SafeReadNumeric(ValueObj);
                        try { AttrValue = AttrNum.ToString(Format); }
                        catch (Exception) { AttrValue = null; }
                    }

                    if (AttrValue != null)
                    {
                        AttrValue = AttrValue.Trim();
                        Output.Append(AttrValue);
                        nAt = ++nSep;
                        continue;
                    }
                }

                //  No such name: Leave encoded.            
                Output.Append(Input.Substring(nAt, ++nSep - nAt));
                nAt = nSep;
            }

            Output.Append(Input.Substring(nAt));
            return Output.ToString();
        }

        /// 
        /// <summary>Get attribute/column names to substitute in a format string.</summary>
        /// <remarks>
        /// Names are encoded as "{}" in an input string. A name may 
        /// use a suffix of ":dn", where "dn" is a decimal numeric format string or ":tx",
        /// where "x" is a date/time format string.
        /// </remarks>
        /// <param name="Input">The input string.</param>
        /// <returns>The substituted result.</returns>
        /// 
        public static string[] GetSubstituteNames(string Input)
        {
            List<string> Names = new List<string>();
            int nAt = 0;
            while (nAt < Input.Length)
            {
                //  Find the opening brace.
                int nSep = Input.IndexOf('{', nAt);
                if (nSep < 0) break;

                if (nSep > nAt)
                    nAt = nSep;

                //  Find the closing brace.
                nSep = Input.IndexOf('}', nAt);
                if (nSep < 0) break;

                //  Get name and optional format specifier.
                string AttrName = Input.Substring(nAt + 1, nSep - nAt - 1).Trim();
                string Format = null;
                int nPrec = AttrName.IndexOf(':');
                if (nPrec > 0)
                {
                    Format = AttrName.Substring(nPrec + 1).Trim();
                    AttrName = AttrName.Substring(0, nPrec).Trim();
                }

                Names.Add(AttrName);
                nAt = nSep;
            }

            return Names.ToArray();
        }


        

        /// <summary>
        /// Retrieve the Index of not in a Word
        /// </summary>
        /// <param name="Phase"></param>
        /// <param name="Search"></param>
        /// <returns></returns>
        public static int IndexOfNotInWord(string Phase, string Search)
        {
            int nAt = Phase.IndexOf(Search);
            // if we have a white space we are ok 
            //Search = Search.Trim();
            if (Search.StartsWith(" ") || Search.EndsWith(" "))
            {
#if notused
            int StartIndex = Search.StartsWith(" ") ? 0 : -1;
            int EndIdndex = Search.EndsWith(" ") ?  1:-1;
            while (nAt > -1 && (nAt > 0 && Char.IsWhiteSpace(Phase, nAt + StartIndex)) ||
                    (nAt + EndIdndex < Phase.Length && Char.IsWhiteSpace(Phase, nAt + Search.Length + EndIdndex)))
                    {
                    nAt = Phase.IndexOf(Search, nAt + 1);   
                    }
                
#endif
                if (nAt + Search.Length < Phase.Length)
                {
                    bool Match = System.Text.RegularExpressions.Regex.IsMatch(Phase, "\\b" + Search + "\\b");
                    if (!Match)
                        nAt = -1;
                }
            }
            else
            {
                while (nAt > -1 && (nAt + Search.Length) < Phase.Length && !Char.IsWhiteSpace(Phase, nAt + Search.Length))
                    nAt = Phase.IndexOf(Search, nAt + 1);
            }


            return nAt;
        }

        /// 
        /// <summary>Word Replacement results</summary>
        /// 
        public enum WordReplace : int
        {
            None = 0,
            WordFoundButInAother = 1,
            WordFound = 2,
            WordFoundAndInAnther = WordFound | WordFoundButInAother
        }
        #endregion

    }

}
