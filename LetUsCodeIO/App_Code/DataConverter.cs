using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LetUsCodeIO.App_Code
{
    ///
    /// <summary>
    /// The DataAccess class provides methods for the access and manipulation of
    /// data stored in various formats.
    /// </summary>
    /// 
    public class DataConverter
    {

        /// <summary>A code for formatting date values</summary>
        public enum DateFormat
        {
            /// <summary>The standard mm/dd/yyyy</summary>
            Standard = 0,
            /// <summary>Long name, "x day of Month year"</summary>
            LongName = 1,
            /// <summary>Month name, "Month dd, yyyy"</summary>
            MonthName = 2,
            /// <summary>Univeral format yyyy/mm/dd</summary>
            Universal = 3,
            /// <summary>No Month format yyyyddd</summary>
            Julian = 4
        }

        /// <summary>
        /// Options for data "normalization"
        /// </summary>
        [Flags]
        public enum NormalizationOptions
        {
            /// <summary>Trim a string</summary>
            Trim = 1,
            /// <summary>Convert string to upper case</summary>
            LowerCase = 2,
            /// <summary>Convert string to lower case</summary>
            UpperCase = 4,
            /// <summary>Remove quotes from string</summary>
            DeQuote = 8,
            /// <summary>Trim trailing whitespace only</summary>
            TrimEnd = 16,
            /// <summary>Convert between data types (e.g. int to string)</summary>
            ConvertType = 32,
            /// <summary>Remove any multiple spaces between words</summary>
            Compress = 64
        };



        ///
        /// <summary>
        /// Helper function to read a numeric value into an integer variable.
        /// </summary>
        /// <remarks>
        /// This function is typically used after a database retrieve to convert
        /// a returned value to a numeric integer value.
        /// <para>
        /// This function will throw an exception if the value is present but 
        /// not a numeric data type.
        /// </para>
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>int value, or 0 if null.</returns>
        ///
        public static int ReadInteger(object Val)
        {
            int Result;
            if (Val == null || Val is DBNull)
                Result = 0;
            else if (Val is string)
            {
                Result = ((string)Val).Length > 0 ? int.Parse((string)Val) : 0;
            }
            else if (Val is decimal)
                Result = Decimal.ToInt32((decimal)Val);
            else if (Val is short)
                Result = (short)Val;
            else if (Val is long)
                Result = (int)((long)Val);
            else if (Val is float)
                Result = (int)((float)Val);
            else
                Result = (int)Val;
            return Result;
        }

        ///
        /// <summary>
        /// Helper function to read a numeric value into an integer variable.
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadInteger function. 0 is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>int value, or 0 if null.</returns>
        ///
        public static int SafeReadInteger(object Val)
        {
            int Ret;
            try
            {
                Ret = ReadInteger(Val);
            }
            catch (Exception)
            {
                Ret = 0;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Returns an integer value from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as 0.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static int SafeReadInteger(object[] Items, int nItem)
        {
            return SafeReadInteger(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        ///
        /// <summary>
        /// Helper function to read a retrieved numeric value into a decimal variable.
        /// </summary>
        /// <remarks>
        /// This function is typically used after a RetrieveSingleItem call to convert
        /// a returned database value to a numeric decimal value.
        /// <para>
        /// This function will throw an exception if the value is present but not a numeric data type.
        /// </para>
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>decimal value, or 0 if null.</returns>
        ///
        public static decimal ReadNumeric(object Val)
        {
            decimal Result;
            if (Val == null || Val is DBNull)
                Result = 0;
            else if (Val is string)
                Result = string.IsNullOrEmpty((string)Val) ? 0 : decimal.Parse((string)Val);
            else if (Val is Int32)
                Result = (int)Val;
            else if (Val is Int64)
                Result = (long)Val;
            else if (Val is short)
                Result = (short)Val;
            else if (Val is float)
                Result = (decimal)((float)Val);
            else if (Val is double)
                Result = (decimal)((double)Val);
            else
                Result = (decimal)Val;
            return Result;
        }

        ///
        /// <summary>
        /// Helper function to read a retrieved numeric value into a decimal variable.
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadNumeric function. 0 is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>decimal value, or 0 if null.</returns>
        ///
        public static decimal SafeReadNumeric(object Val)
        {
            decimal Ret;
            try
            {
                Ret = ReadNumeric(Val);
            }
            catch (Exception)
            {
                Ret = 0M;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Returns a decimal value from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as 0.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static decimal SafeReadNumeric(object[] Items, int nItem)
        {
            return SafeReadNumeric(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        ///
        /// <summary>
        /// Helper function to read a retrieved numeric array into an array of decimal variables.
        /// </summary>
        /// <remarks>
        /// This function is typically used after a RetrieveSingleColumn call to convert
        /// a returned array of database values to numeric decimal values.
        /// <para>
        /// If the "MinOne" parameter is true, the return value will always be an array of
        /// at least one element - If a null input array is supplied, that element will be
        /// "-1" and if an empty input array is supplied, that element will be "0". Otherwise,
        /// a null array will return a null array and an empty array will return an empty 
        /// array.
        /// </para>
        /// <para>
        /// Any non-numeric value retrieved will result in a null array element.
        /// </para>
        /// </remarks>
        /// <param name="Vals">The array of value to be converted.</param>
        /// <param name="MinOne">true to always return at least one value.</param>
        /// <returns>decimal values, or null if nothing retrieved</returns>
        ///
        public static decimal[] ReadNumericArray(object[] Vals, bool MinOne)
        {
            decimal[] Result;
            if (Vals == null)
                Result = MinOne ? new decimal[1] { -1 } : null;
            else if (Vals.Length == 0)
                Result = MinOne ? new decimal[1] { 0 } : new decimal[0];
            else
            {
                Result = new decimal[Vals.Length];
                for (int i = 0; i < Vals.Length; i++)
                {
                    try { Result[i] = ReadNumeric(Vals[i]); }
                    catch { }
                }
            }
            return Result;
        }

        ///
        /// <summary>
        /// Helper function to read a retrieved floating-point value into a double variable.
        /// </summary>
        /// <remarks>
        /// This function is typically used after a RetrieveSingleItem call to convert
        /// a returned database value to a double value.
        /// <para>
        /// This function will throw an exception if the value is present but not a 
        /// floating-point data type.
        /// </para>
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>double value, or 0 if null.</returns>
        ///
        public static double ReadFloat(object Val)
        {
            double Result;
            if (Val == null || Val is DBNull)
                Result = 0;
            else
                Result = Convert.ToDouble(Val);
            return Result;
        }

        ///
        /// <summary>
        /// Helper function to read a retrieved floating-point value into a double variable.
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadFloat function. 0 is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>double value, or 0 if null.</returns>
        ///
        public static double SafeReadFloat(object Val)
        {
            double Ret;
            try
            {
                Ret = ReadFloat(Val);
            }
            catch (Exception)
            {
                Ret = 0D;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Returns a floating-point value from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as 0.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static double SafeReadFloat(object[] Items, int nItem)
        {
            return SafeReadFloat(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        /// 
        /// <summary>
        /// Read a boolean value (YES/NO, TRUE/FALSE, 1/0)
        /// </summary>
        /// <remarks>
        /// A null value is interpreted as false.
        /// <para>
        /// This function will throw an exception if the value is present but not a boolean type.
        /// </para>
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>boolean result.</returns>
        ///
        public static bool ReadBool(object Val)
        {
            return ReadBool(Val, false);
        }

        /// 
        /// <summary>
        /// Read a boolean value (YES/NO, TRUE/FALSE, 1/0)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function will throw an exception if the value is present but not a boolean type.
        /// </para>
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <param name="Default">Default used for null or empty value.</param>
        /// <returns>boolean result.</returns>
        ///
        public static bool ReadBool(object Val, bool Default)
        {
            bool Result;
            if (Val == null || Val is DBNull)
                Result = Default;
            else if (Val is string)
            {
                string Value = ((string)Val).Trim();
                int Check = Keywords.CheckYesNo(Value);
                if (Value.Length == 0)
                    Result = Default;
                else if (Check > 0)
                    Result = true;
                else if (Check == 0)
                    Result = false;
                else
                    throw new ApplicationException(string.Format("'{0}' is not a recognizable boolean value.", Value));
            }
            else if (Val is short || Val is int || Val is decimal || Val is float || Val is double)
                Result = ReadInteger(Val) != 0 ? true : false;
            else
                Result = (bool)Val;
            return Result;
        }

        /// 
        /// <summary>
        /// Read a boolean value (YES/NO, TRUE/FALSE, 1/0)
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadBool function. false is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>boolean result.</returns>
        ///
        public static bool SafeReadBool(object Val)
        {
            return SafeReadBool(Val, false);
        }

        /// 
        /// <summary>
        /// Read a boolean value (YES/NO, TRUE/FALSE, 1/0)
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadBool function. false is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <param name="Default">Default used for null or empty value.</param>
        /// <returns>boolean result.</returns>
        ///
        public static bool SafeReadBool(object Val, bool Default)
        {
            bool Ret;
            try
            {
                Ret = ReadBool(Val, Default);
            }
            catch (Exception)
            {
                Ret = false;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Returns a boolean value from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as 0.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static bool SafeReadBool(object[] Items, int nItem)
        {
            return SafeReadBool(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        /// 
        /// <summary>
        /// Read a byte array.
        /// </summary>
        /// <remarks>
        /// If a string is supplied it will be converted to a byte array
        /// using ASCII encoding.
        /// </remarks>
        /// <param name="Val"></param>
        /// <returns></returns>
        /// 
        public static byte[] ReadBytes(object Val)
        {
            if (Val == null || Val is DBNull)
                return null;

            byte[] Result = null;
            if (Val is string || Val is SqlString)
            {
                ASCIIEncoding Asc = new ASCIIEncoding();
                Result = Asc.GetBytes((string)Val);
            }

            if (Result == null)
                Result = Val as byte[];

            return Result;
        }

        /// 
        /// <summary>
        /// Read a single-character value 
        /// </summary>
        /// <remarks>
        /// The first character of a string value is returned. A null value is returned as a null character (0).
        /// <para>
        /// This function will throw an exception if the value is present but not a character or string type.
        /// </para>
        /// </remarks>
        /// <param name="Val">The value to be converted.</param>
        /// <returns>boolean result.</returns>
        ///
        public static char ReadChar(object Val)
        {
            char Result;
            if (Val == null || Val is DBNull)
                Result = '\0';
            else if (Val is string)
            {
                string Value = (string)Val;
                Result = (Value.Length > 0) ? Value[0] : '\0';
            }
            else
                Result = (char)Val;
            return Result;
        }

        /// 
        /// <summary>
        /// Read a retrieved date value into a datetime variable.
        /// </summary>
        /// <remarks>
        /// If the value is stored as a string, it will be parsed into a DateTime.
        /// <para>
        /// This function will throw an exception if the value is a string but not in a valid datetime format.
        /// </para>
        /// </remarks>
        /// <param name="Val"></param>
        /// <returns></returns>
        /// 
        public static DateTime ReadDate(object Val)
        {
            if (Val == null || Val is DBNull)
                return DateTime.MinValue;

            DateTime Result;
            if (Val is DateTime)
                Result = (DateTime)Val;
            else if ((Val is string || Val is SqlString) && !string.IsNullOrEmpty((string)Val))
            {
                string DateVal = (string)Val;
                //  Check for dates in the format yyyy-dd-mm-hh.mm.ss.tt (returned by DB/2)           
                if (DateVal.Length >= 19 && DateVal[4] == '-' && DateVal[10] == '-')
                {
                    DateVal = DateVal.Substring(0, 10) + "T" + DateVal.Substring(11, 8).Replace('.', ':')
                              + DateVal.Substring(19);
                }
                Result = DateTime.Parse(DateVal);
            }
            else
                Result = DateTime.MinValue;
            return Result;
        }


        /// 
        /// <summary>
        /// Read a retrieved date value into a datetime variable, to 1-second resolution.
        /// </summary>
        /// <remarks>
        /// Due to the way time precision is stored in different databases, sub-second
        /// resolutions do not always compare properly.
        /// </remarks>
        /// 
        public static DateTime ReadDateToSecond(object Val)
        {
            DateTime TheTime = DataConverter.ReadDate(Val);
            TheTime = new DateTime(TheTime.Year, TheTime.Month, TheTime.Day, TheTime.Hour, TheTime.Minute, TheTime.Second);
            return TheTime;
        }

        /// 
        /// <summary>
        /// Read a retrieved date value into a datetime variable, adding time of day from a string.
        /// </summary>
        /// <remarks>
        /// If the value is stored as a string, it will be parsed into a DateTime.
        /// <para>
        /// This function will throw an exception if the value is a string but not in a valid datetime format.
        /// </para>
        /// </remarks>
        /// <param name="Val"></param>
        /// <param name="TimeOfDay"></param>
        /// <returns></returns>
        /// 
        public static DateTime ReadDate(object Val, string TimeOfDay)
        {
            DateTime Base = ReadDate(Val);
            if (Base == DateTime.MinValue)
                return Base;

            TimeSpan Add = ReadTime(TimeOfDay);
            return Base + Add;
        }

        /// 
        /// <summary>
        /// Read a retrieved date value into a datetime variable.
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadDate function. 1/1/1900 is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val"></param>
        /// <returns></returns>
        /// 
        public static DateTime SafeReadDate(object Val)
        {
            DateTime Ret;
            try
            {
                Ret = ReadDate(Val);
            }
            catch (Exception)
            {
                Ret = DateTime.MinValue;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Read a retrieved date value into a datetime variable, adding time of day from a string.
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadDate function. 1/1/1900 is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val"></param>
        /// <param name="TimeOfDay"></param>
        /// <returns></returns>
        /// 
        public static DateTime SafeReadDate(object Val, string TimeOfDay)
        {
            DateTime Ret;
            try
            {
                Ret = ReadDate(Val, TimeOfDay);
            }
            catch (Exception)
            {
                Ret = DateTime.MinValue;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Returns a DateTime value from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as 0.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static DateTime SafeReadDate(object[] Items, int nItem)
        {
            return SafeReadDate(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        /// 
        /// <summary>
        /// Read a retrieved date value into a datetime variable, adding version suffix as time of day.
        /// </summary>
        /// <remarks>
        /// This function recognizes a string of the form "mm-dd-yyx" and converts "x" from A=1 hour 
        /// to W=23 hours, X-Z as 23 hours plus 1-3 minutes.
        /// <para>
        /// This is an exception-free form of the ReadDate function. 1/1/1900 is returned
        /// for any invalid value.
        /// </para>
        /// </remarks>
        /// <param name="InVal">Input string</param>
        /// 
        public static DateTime SafeReadVersionDate(object InVal)
        {
            if (InVal == null || InVal == DBNull.Value)
                return DateTime.MinValue;
            if (InVal is DateTime)
                return (DateTime)InVal;

            DateTime Ret;
            try
            {
                string Val = InVal.ToString().ToUpper();
                int Hours = 0, Minutes = 0;
                char Ver = Val[Val.Length - 1];
                if (char.IsLetter(Ver))
                {
                    Val = Val.Substring(0, Val.Length - 1);
                    Hours = (int)Ver - (int)'A' + 1;
                    if (Hours < 0) Hours = 0;
                    if (Hours > 23)
                    {
                        Minutes = Hours - 23;
                        if (Minutes > 59) Minutes = 59;
                        Hours = 23;
                    }
                }
                Ret = ReadDate(Val);
                Ret = Ret.AddHours(Hours);
                Ret = Ret.AddMinutes(Minutes);
            }
            catch (Exception)
            {
                Ret = DateTime.MinValue;
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Read a retrieved time-of-day value into a timespan variable.
        /// </summary>
        /// <remarks>
        /// If the value is stored as a string, it will be parsed into a TimeSpan.
        /// <para>
        /// This function will throw an exception if the value is a string but not in a valid time format,
        /// although special checks are made for a 4-character string value, which is assumed to be "hhmm" or
        /// a 6-character string value, which is assumed to be "hhmmss". Also, the string can end with 'am' 
        /// or 'pm' ('a' or 'p' are accepted) and will be adjusted accordingly.
        /// </para>
        /// </remarks>
        /// <param name="Val"></param>
        /// <returns></returns>
        /// 
        public static TimeSpan ReadTime(object Val)
        {
            if (Val == null || Val is DBNull)
                return new TimeSpan(0);

            TimeSpan Result;
            if (Val is TimeSpan)
                Result = (TimeSpan)Val;
            else if (Val is DateTime)
                Result = ((DateTime)Val).TimeOfDay;
            else if ((Val is string || Val is SqlString) && !string.IsNullOrEmpty((string)Val))
            {
                string sVal = ((string)Val).ToLower();
                int nLast = sVal.Length - 1;
                bool IsAM = false, IsPM = false;
                if (char.IsLetter(sVal[nLast]))
                {
                    if (sVal.EndsWith("am") || sVal[nLast] == 'a')
                    {
                        IsAM = true;
                        sVal = sVal.Substring(0, sVal[nLast] == 'a' ? nLast : nLast - 1).Trim();
                    }
                    else if (sVal.EndsWith("pm") || sVal[nLast] == 'p')
                    {
                        IsPM = true;
                        sVal = sVal.Substring(0, sVal[nLast] == 'p' ? nLast : nLast - 1).Trim();
                    }
                }

                int nHours, nMins;
                if ((sVal.Length == 4 || sVal.Length == 6)
                        && Int32.TryParse(sVal.Substring(0, 2), out nHours)
                        && Int32.TryParse(sVal.Substring(2, 2), out nMins))
                {
                    int nSecs = (sVal.Length == 6) ? Int32.Parse(sVal.Substring(4, 2)) : 0;
                    Result = new TimeSpan(nHours, nMins, nSecs);
                }
                else
                {
                    Result = TimeSpan.Parse(sVal);
                }

                if (IsAM)
                {
                    if (Result.Hours >= 12)
                        Result -= new TimeSpan(12, 0, 0);
                }

                else if (IsPM)
                {
                    if (Result.Hours < 12)
                        Result += new TimeSpan(12, 0, 0);
                }
            }
            else
                Result = new TimeSpan(0);
            return Result;
        }

        /// 
        /// <summary>
        /// Read a retrieved time-of-day value into a timespan variable.
        /// </summary>
        /// <remarks>
        /// This is an exception-free form of the ReadDate function. 00:00:00 is returned
        /// for any invalid value.
        /// </remarks>
        /// <param name="Val"></param>
        /// <returns></returns>
        /// 
        public static TimeSpan SafeReadTime(object Val)
        {
            TimeSpan Ret;
            try
            {
                Ret = ReadTime(Val);
            }
            catch (Exception)
            {
                Ret = new TimeSpan(0);
            }
            return Ret;
        }

        /// 
        /// <summary>
        /// Returns a time-of-day value from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as 0.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static TimeSpan SafeReadTime(object[] Items, int nItem)
        {
            return SafeReadTime(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        /// 
        /// <summary>
        /// Returns a string value with trailing whitespace trimmed. 
        /// </summary>
        /// <remarks>
        /// A null value is returned as an empty string.
        /// </remarks>
        /// <param name="Input">Input string.</param>
        /// <returns></returns>
        /// 
        public static string ReadString(object Input)
        {
            string InStr = Input as string;
            if (InStr == null)
            {
                if (Input != null && Input != DBNull.Value)
                    InStr = Input.ToString();
                else
                    return string.Empty;
            }

            InStr = InStr.TrimEnd(null);
            return InStr;
        }

        /// 
        /// <summary>
        /// Returns a string value, limited by a maximum length.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="MaxLen"></param>
        /// <returns></returns>
        /// 
        public static string ReadString(object Input, int MaxLen)
        {
            string InStr = Input as string;
            if (InStr == null)
            {
                if (Input != null && Input != DBNull.Value)
                    InStr = Input.ToString();
                else
                    return string.Empty;
            }

            return InStr == null ? null : (InStr.Length <= MaxLen ? InStr : InStr.Substring(0, MaxLen));
        }

        /// 
        /// <summary>
        /// Returns a string value, with a default to replace null.
        /// </summary>
        /// <remarks>
        /// Any non-string type is converted to a string. Trailing whitespace is also eliminated.
        /// </remarks>
        /// <param name="Input">Input string.</param>
        /// <param name="Default">Default value.</param>
        /// <returns></returns>
        /// 
        public static string ReadString(object Input, string Default)
        {
            return ReadString(Input, Default, NormalizationOptions.TrimEnd | NormalizationOptions.ConvertType);
        }

        /// 
        /// <summary>
        /// Returns a string value, with a default to replace null.
        /// </summary>
        /// <param name="Input">Input string.</param>
        /// <param name="Default">Default value.</param>
        /// <param name="Opts">Normalization options</param>
        /// <returns></returns>
        /// 
        public static string ReadString(object Input, string Default, NormalizationOptions Opts)
        {
            string InStr = Input as string;
            if (InStr == null)
            {
                if (Input != null && Input != DBNull.Value && (Opts & NormalizationOptions.ConvertType) != 0)
                    InStr = Input.ToString();
                else
                    return Default;
            }

            if ((Opts & NormalizationOptions.DeQuote) != 0)
                InStr = StringParser.RemoveQuotes(InStr.Trim());
            if ((Opts & NormalizationOptions.Trim) != 0)
                InStr = InStr.Trim();
            else if ((Opts & NormalizationOptions.TrimEnd) != 0)
                InStr = InStr.TrimEnd(null);
            if ((Opts & NormalizationOptions.LowerCase) != 0)
                InStr = InStr.ToLower();
            else if ((Opts & NormalizationOptions.UpperCase) != 0)
                InStr = InStr.ToUpper();
            if ((Opts & NormalizationOptions.Compress) != 0)
                InStr = StringParser.CompressString(InStr);
            return InStr;
        }

        /// 
        /// <summary>
        /// Returns a string item from an array of objects.
        /// </summary>
        /// <remarks>
        /// A null or invalid value is returned as an empty string.
        /// </remarks>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static string ReadString(object[] Items, int nItem)
        {
            return ReadString(Items == null ? null : (Items.Length < nItem ? null : Items[nItem]));
        }

        /// 
        /// <summary>
        /// Converts an object to a specified type with an equivalent value, using the
        /// DataConverter.ReadXxx() methods if possible.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <param name="ConversionType">The Type we want to convert Value to.</param>
        /// <param name="Default">
        /// A default to return if Value can not be converted.
        /// </param>
        /// <returns>
        /// An object whose Type is ConversionType and with a value equivalent to Value,
        /// or the supplied default if the conversion could not be performed.
        /// </returns>
        /// 
        public static object ChangeType(object Value, Type ConversionType, object Default)
        {
            object result;

            if (!TryChangeType(Value, ConversionType, out result))
                result = Default;

            return result;
        }

        /// 
        /// <summary>
        /// Converts an object to a specified type with an equivalent value, using the
        /// DataConverter.ReadXxx() methods if possible.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <param name="ConversionType">The Type we want to convert Value to.</param>
        /// <param name="ConvertedValue">
        /// An object whose Type is ConversionType and with a value equivalent to Value.
        /// Null if the conversion could not be performed, or if Value is null (in which
        /// case result will be false if ConversionType is a value type).
        /// </param>
        /// <returns>
        /// True if the value's type was successfully changed, false otherwise.
        /// </returns>
        /// 
        public static bool TryChangeType(object Value, Type ConversionType, out object ConvertedValue)
        {
            //
            // Return quickly if no conversion needs to be performed.
            //
            if (Value == null)
            {
                if (!ConversionType.IsValueType)
                {
                    //
                    // Translate null to string.Empty when converting to string, same as
                    // DataConverter.ReadString() would.
                    //
                    ConvertedValue = (ConversionType == typeof(string) ? string.Empty : Value);
                    return true;
                }
            }
            else if (ConversionType.IsAssignableFrom(Value.GetType()))
            {
                ConvertedValue = Value;
                return true;
            }

            //
            // First try to convert with the DataConverter.ReadXxx() methods.
            //
            bool result = true;
            try
            {
                if (ConversionType == typeof(bool))
                    ConvertedValue = DataConverter.ReadBool(Value);
                else if (ConversionType == typeof(char))
                    ConvertedValue = DataConverter.ReadChar(Value);
                else if (ConversionType == typeof(DateTime))
                    ConvertedValue = DataConverter.ReadDate(Value);
                else if (ConversionType == typeof(string))
                {
                    ConvertedValue = (Value == DBNull.Value ? string.Empty :
                        Value.ToString());
                }
                else if (ConversionType == typeof(TimeSpan))
                    ConvertedValue = DataConverter.ReadTime(Value);
                else if (ConversionType == typeof(byte[]))
                    ConvertedValue = DataConverter.ReadBytes(Value);
                else if (StandardImplicitConversionExists(ConversionType,
                    typeof(decimal)))
                {
                    ConvertedValue = (ConversionType == typeof(decimal) ?
                        DataConverter.ReadNumeric(Value) : Convert.ChangeType(
                        DataConverter.ReadNumeric(Value), ConversionType));
                }
                else if (StandardImplicitConversionExists(ConversionType,
                    typeof(double)))
                {
                    ConvertedValue = (ConversionType == typeof(double) ?
                        DataConverter.ReadFloat(Value) : Convert.ChangeType(
                        DataConverter.ReadFloat(Value), ConversionType));
                }
                else
                {
                    ConvertedValue = null;
                    result = false;
                }
            }
            catch
            {
                ConvertedValue = null;
                result = false;
            }

            if (!result && Value != null)
            {
                //
                // If the DataConverter.ReadXxx() methods failed, try to convert using
                // IConvertible or TypeConverters.
                //
                try
                {
                    if (Value is IConvertible && iConvertibleConvertsTo(ConversionType))
                    {
                        ConvertedValue = Convert.ChangeType(Value, ConversionType);
                        result = true;
                    }
                    else
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(
                            ConversionType);
                        if (converter.CanConvertFrom(Value.GetType()))
                        {
                            ConvertedValue = converter.ConvertFrom(Value);
                            result = (ConvertedValue != null);
                        }
                        else
                        {
                            converter = TypeDescriptor.GetConverter(Value);
                            if (converter.CanConvertTo(ConversionType))
                            {
                                ConvertedValue = converter.ConvertTo(Value,
                                    ConversionType);
                                result = (ConvertedValue != null);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        /// 
        /// <summary>
        /// Converts an object to a specified type with an equivalent value, using the
        /// DataConverter.ReadXxx() methods if possible.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <param name="OrgValue">The original value whose type we want to matahc.</param>
        /// <param name="ConvertedValue">
        /// An object whose Type is OrgValue.GetType() and with a value equivalent to Value.
        /// Null if the conversion could not be performed, or if Value is null (in which
        /// case result will be false if ConversionType is a value type).
        /// </param>
        /// <returns>
        /// True if the value's type was successfully changed, false otherwise.
        /// </returns>
        /// 
        public static bool TryChangeType(object Value, object OrgValue, out object ConvertedValue)
        {
            if (OrgValue == null)
            {
                ConvertedValue = null;
                return false;
            }

#if NOTUSED
        if ((Value is bool || Value is Boolean) && (OrgValue is char || OrgValue is string))
            {
            //  Try to match boolean to char as Y/N vs. 1/0:
            string Check = OrgValue.ToString().ToUpper();
            if (Check == "Y" || Check == "N")
                {
                Check = ((bool)Value) ? "Y" : "N";
                if (OrgValue is char)
                    ConvertedValue = Check[0];
                else
                    ConvertedValue = Check;
                return true;
                }
            }
#endif

            return TryChangeType(Value, OrgValue.GetType(), out ConvertedValue);
        }

        /// 
        /// <summary>
        /// Indicates whether IConvertible defines a method to convert to the specified
        /// Type.
        /// </summary>
        /// <param name="TgtType">
        /// Type to check for whether IConvertible converts to it.
        /// </param>
        /// <returns>
        /// True if IConvertible defines a method to convert to TgtType, false otherwise.
        /// </returns>
        /// 
        private static bool iConvertibleConvertsTo(Type TgtType)
        {
            return (TgtType == typeof(bool) || TgtType == typeof(char) ||
                TgtType == typeof(sbyte) || TgtType == typeof(byte) ||
                TgtType == typeof(short) || TgtType == typeof(ushort) ||
                TgtType == typeof(int) || TgtType == typeof(uint) ||
                TgtType == typeof(long) || TgtType == typeof(ulong) ||
                TgtType == typeof(float) || TgtType == typeof(double) ||
                TgtType == typeof(decimal) || TgtType == typeof(DateTime) ||
                TgtType == typeof(string));
        }

        /// 
        /// <summary>
        /// Returns a version value parsed from a string.
        /// </summary>
        /// <remarks>
        /// An invalid string returns a version of "0.0.0.0".
        /// </remarks>
        /// <param name="sVer">String to parse</param>
        /// <returns>Version value</returns>
        /// 
        public static Version ReadVersion(string sVer)
        {
            Version Ver = null;
            try
            {
                if (!string.IsNullOrEmpty(sVer) && char.IsDigit(sVer[0]))
                    Ver = new Version(sVer);
            }
            catch (Exception)
            {
            }
            if (Ver == null)
                Ver = new Version(0, 0, 0, 0);
            return Ver;
        }

        /// 
        /// <summary>
        /// Read a list of integer values from a CSV string.
        /// </summary>
        /// <remarks>
        /// If the supplied string contains less than a minimum number of elements,
        /// additional 0-value elements will be added.
        /// </remarks>
        /// <param name="sList">The list of values</param>
        /// <param name="nMin">Minimum number of elements to return (0 to leave unchanged).</param>
        /// <returns></returns>
        /// 
        public static int[] ReadIntList(string sList, int nMin)
        {
            string[] sVals = sList.Split(',');
            if (nMin <= 0 || nMin < sVals.Length)
                nMin = sVals.Length;

            int[] nVals = new int[nMin];
            for (int i = 0; i < sVals.Length; i++)
            {
                int.TryParse(sVals[i], out nVals[i]);
            }

            return nVals;
        }

        /// 
        /// <summary>
        /// Write a list of integer values to a CSV string.
        /// </summary>
        /// <param name="nVals">Values to write</param>
        /// <returns></returns>
        /// 
        public static string WriteIntList(params int[] nVals)
        {
            StringBuilder Out = new StringBuilder(10 * nVals.Length);
            foreach (int nVal in nVals)
            {
                if (Out.Length > 0)
                    Out.Append(',');
                Out.Append(nVal);
            }
            return Out.ToString();
        }

        ///
        /// <summary>
        /// Read a list of float values from a CSV string.
        /// </summary>
        /// <remarks>
        /// If the supplied string contains less than a minimum number of elements,
        /// additional 0-value elements will be added.
        /// </remarks>
        /// <param name="sList">The list of values</param>
        /// <param name="nMin">Minimum number of elements to return (0 to leave unchanged).</param>
        /// <returns></returns>
        ///
        public static float[] ReadFloatList(string sList, int nMin)
        {
            string[] sVals = sList.Split(',');
            if (nMin <= 0 || nMin < sVals.Length)
                nMin = sVals.Length;

            float[] nVals = new float[nMin];
            for (int i = 0; i < sVals.Length; i++)
            {
                float.TryParse(sVals[i], out nVals[i]);
            }

            return nVals;
        }

        ///
        /// <summary>
        /// Write a list of float values to a CSV string.
        /// </summary>
        /// <param name="nVals">Values to write</param>
        /// <returns></returns>
        ///
        public static string WriteFloatList(params float[] nVals)
        {
            StringBuilder Out = new StringBuilder(10 * nVals.Length);
            foreach (float nVal in nVals)
            {
                if (Out.Length > 0)
                    Out.Append(',');
                Out.Append(nVal);
            }
            return Out.ToString();
        }

        /// 
        /// <summary>
        /// Returns an item from an array of objects.
        /// </summary>
        /// <param name="Items">The item array</param>
        /// <param name="nItem">The desired index.</param>
        /// <returns>The item or null if array is null or shorter than index.</returns>
        ///         
        public static object ReadItem(object[] Items, int nItem)
        {
            return Items == null ? null : (Items.Length < nItem ? null : Items[nItem]);
        }

        /// 
        /// <summary>
        /// Converts a base-64 string to a byte array, handling exceptions
        /// </summary>
        /// <param name="Input">The input string to convert.</param>
        /// <returns>converted byte array or null if invalid input.</returns>
        /// 
        public static byte[] ReadBase64String(string Input)
        {
            byte[] Result;
            try
            {
                Result = Convert.FromBase64String(Input);
            }
            catch (Exception Ex)
            {
                Result = null;

            }
            return Result;
        }

        /// <summary>
        /// Converts a string value to base 64 encoding
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static string ToBase64(string Value)
        {
            string RetValue = string.Empty;
            try
            {
                if (Value != null && Value != string.Empty)
                {
                    RetValue = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Value));
                }
            }
            catch (Exception)
            {
                RetValue = null;
            }
            return RetValue;

        }

        /// <summary>
        /// Convert a byte array to a hexadecimal-format string.
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>

        public static string ToHexString(byte[] Bytes)
        {
            StringBuilder OutVal = new StringBuilder();
            foreach (byte InVal in Bytes)
            {
                OutVal.Append(InVal.ToString("X2"));
            }
            return OutVal.ToString();
        }

        /// <summary>
        /// Convert from the hex string to an byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// 
        /// <summary>
        /// Converts a time value from hh:mm[:ss] to hh:mm[:ss]AM/PM
        /// </summary>
        /// <param name="TimeVal">24-hour time value as a string.</param>
        /// <returns>12-hour time value as a string.</returns>
        /// 
        public static string GetTimeAMPM(string TimeVal)
        {
            int Hours;
            if (TimeVal.Length < 5 || !Int32.TryParse(TimeVal.Substring(0, 2), out Hours))
            {
                //  Invalid value: Return unchanged
            }
            else if (Hours < 12)
            {
                TimeVal = TimeVal + "AM";
            }
            else
            {
                TimeVal = TimeVal + "PM";
                if (Hours > 12)
                {
                    Hours -= 12;
                    TimeVal = Hours.ToString("d2") + TimeVal.Substring(2);
                }
            }

            return TimeVal;
        }

        /// 
        /// <summary>
        /// Converts a time value to a string of the form "hhmmss".
        /// </summary>
        /// <param name="TimeVal">Timespan value</param>
        /// <returns>24-hour time value as a string.</returns>
        /// 
        public static string GetTimeHMS(TimeSpan TimeVal)
        {
            DateTime Val = DateTime.Today + TimeVal;
            return Val.ToString("HHmmss");
        }

        /// 
        /// <summary>
        /// Converts a time value to a string of the form "hh:mm:ss".
        /// </summary>
        /// <param name="TimeVal">Timespan value</param>
        /// <returns>24-hour time value as a string.</returns>
        /// 
        public static string GetTimeHMS8(TimeSpan TimeVal)
        {
            DateTime Val = DateTime.Today + TimeVal;
            return Val.ToString("HH:mm:ss");
        }

        /// 
        /// <summary>
        /// Reformat date based upon a standard code
        /// </summary>
        /// <param name="Value">object variable containing date to be reformated</param>
        /// <param name="ReCalcDateMethod">Which format method to use</param>
        /// <returns>string in requested format</returns>
        /// 
        public static string GetFormattedDate(object Value, DateFormat ReCalcDateMethod)
        {
            string RetValue;
            DateTime RecDate = DataConverter.SafeReadDate(Value);

            switch (ReCalcDateMethod)
            {
                // standard date formatting
                case DateFormat.Standard:
                    RetValue = RecDate.ToString("MM/dd/yyyy");
                    break;

                // special date formatting
                // example
                // 1st day of July 2011
                case DateFormat.LongName:
                    StringBuilder SB = new StringBuilder();

                    SB.Append(RecDate.Day.ToString());

                    switch (RecDate.Day)
                    {
                        case 1:
                        case 21:
                        case 31:
                            SB.Append("st");
                            break;
                        case 2:
                        case 22:
                            SB.Append("nd");
                            break;
                        case 3:
                        case 23:
                            SB.Append("rd");
                            break;
                        default:
                            SB.Append("th");
                            break;
                    }

                    SB.Append(" day of ");
                    SB.Append(RecDate.ToString("MMMM"));
                    SB.Append(" ");
                    SB.Append(RecDate.ToString("yyyy"));
                    RetValue = SB.ToString();
                    break;

                // Month name date formatting
                case DateFormat.MonthName:
                    RetValue = RecDate.ToString("MMMM dd, yyyy");
                    break;

                // Universal Format for date year-month-day
                case DateFormat.Universal:
                    RetValue = RecDate.ToString("yyyy-MM-dd");
                    break;

                // Julian format of with no month just day representation
                case DateFormat.Julian:
                    RetValue = RecDate.ToString("yyyyddd");
                    break;

                // return date value unchanged
                default:
                    RetValue = Value.ToString();
                    break;
            }

            return RetValue;
        }

        /// <summary>
        /// Helper function to get the default of a value type
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type T)
        {
            object RtnVal = null;
            if (T == null || T == typeof(void))
                return RtnVal;
            if (T == typeof(Nullable))
                T = Nullable.GetUnderlyingType(T);
            if (T.IsValueType)
            {
                //Todo handle formatting datatime
                //NewVision.CommonSupport.BaseConfigSchema.Common.MinDateTime
                RtnVal = Activator.CreateInstance(T);
            }
            else
            {
                // Handle Default for objects
                if (T == typeof(string))
                    RtnVal = string.Empty;
                // All the rest we will disclose as null for now
            }

            return RtnVal;
        }

        /// 
        /// <summary>
        /// Splits a string into an alphabetic prefix and numeric suffix.
        /// </summary>
        /// <param name="Input">Input string.</param>
        /// <returns>Array of two strings: Prefix and Suffix.</returns>
        ///         
        public static string[] SplitNumericString(string Input)
        {
            string[] Parts = new string[2];
            int n = Input.Length;
            while (--n >= 0)
            {
                if (!char.IsDigit(Input[n]))
                    break;
            }
            Parts[0] = ++n > 0 ? Input.Substring(0, n) : string.Empty;
            Parts[1] = Input.Substring(n);
            return Parts;
        }

        /// 
        /// <summary>
        /// Increment a decimal value encoded in a string with a prefix and numeric suffix.
        /// </summary>
        /// <param name="Input">Input string.</param>
        /// <returns>Incremented string.</returns>
        /// 
        public static string IncrementNumericString(string Input)
        {
            string[] Parts = DataConverter.SplitNumericString(Input);
            decimal NextValue;
            if (decimal.TryParse(Parts[1], out NextValue))
            {
                NextValue++;
                Input = Parts[0] + NextValue.ToString().PadLeft(Parts[1].Length, '0');
            }
            return Input;
        }

        /// 
        /// <summary>
        /// Remove "illegal" characters from a string.
        /// </summary>
        /// <remarks>
        /// This function removes characters from a string that are not valid in
        /// SQL literals. Although SQL parameters should generally be used, certain
        /// functions in certain databases require literals.
        /// </remarks>
        /// <param name="Input"></param>
        /// <returns>String with illegal characters removed</returns>
        /// 
        public static string RemoveIllegalChars(string Input)
        {
            if (!string.IsNullOrEmpty(Input))
            {
                int i;
                char[] Invalid = { '\'', '\"', ';' };
                while ((i = Input.IndexOfAny(Invalid)) >= 0)
                    Input = Input.Remove(i, 1);
            }
            return Input;
        }

        /// 
        /// <summary>
        /// Remove "control" characters from a string.
        /// </summary>
        /// <remarks>
        /// This function removes all characters from a string that are defined
        /// as control characters, optionally replacing with a valid character.
        /// </remarks>
        /// <param name="Input">Input string</param>
        /// <param name="RepChr">Replacement character (0 to remove).</param>
        /// <returns>String with control characters removed</returns>
        /// 
        public static string RemoveControlChars(string Input, char RepChr)
        {
            for (int i = 0; i < Input.Length; i++)
            {
                if (char.IsControl(Input, i))
                {
                    if ((int)RepChr == 0)
                        Input = Input.Remove(i--, 1);
                    else
                        Input = Input.Replace(Input[i], RepChr);
                }
            }

            return Input;
        }

        /// 
        /// <summary>Replaces invalid path and filename characters in a string with underscores.</summary>
        /// <param name="Input">Input string.</param>
        /// <returns>Input string with invalid path and filename characters replaced with underscores.</returns>
        /// 
        public static string ReplaceInvalidPathAndFilenameChars(string Input)
        {
            StringBuilder result = new StringBuilder(Input);
            foreach (char c in Path.GetInvalidPathChars())
                result.Replace(c, '_');
            foreach (char c in Path.GetInvalidFileNameChars())
                result.Replace(c, '_');
            return result.ToString();
        }

        /// 
        /// <summary>Compares two objects of unknown type.</summary>
        /// <remarks>
        /// <para>If neither object is null, will first attempt to compare using objA as
        /// an IComparable and DataConverter.TryChangeType() to coerce objB to the same
        /// type as objA. Will then attempt to do the same using objB as the IComparable
        /// and coercing objA's type. If all else fails, will compare the two
        /// object's as strings.
        /// </para>
        /// <para>Null is considered less than any object and equal to other nulls.
        /// </para>
        /// </remarks>
        /// <param name="objA">An object to compare to objB.</param>
        /// <param name="objB">An object to compare with objA.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects
        /// being compared. The return value has the following meanings: Less than zero,
        /// objA is less than objB; Zero, objA is equal to objB; Greater than zero,
        /// objA is greater than objB.
        /// </returns>
        /// 
        public static int GeneralizedCompare(object objA, object objB)
        {
            int result;
            IComparable comparable;
            object other;

            if (objA == objB)
                result = 0;
            else if (objA == null)
                result = -1;
            else if (objB == null)
                result = 1;
            else if ((comparable = objA as IComparable) != null && TryChangeType(objB,
                objA.GetType(), out other))
            {
                result = comparable.CompareTo(other);
            }
            else if ((comparable = objB as IComparable) != null && TryChangeType(objA,
                objB.GetType(), out other))
            {
                result = comparable.CompareTo(other) * -1;
            }
            else
                result = objA.ToString().CompareTo(objB.ToString());

            return result;
        }

        /// 
        /// <summary>
        /// Provides a comparer that uses DataConverter.GeneralizedCompare().
        /// </summary>
        /// 
        public static IComparer GeneralizedComparer
        {
            get { return _generalizedComparer; }
        }
        private static readonly GeneralizedComparerClass _generalizedComparer =
            new GeneralizedComparerClass();

        /// 
        /// <summary>
        /// Provides a comparer that compares strings case-insensitively, and falls back
        /// to DataConverter.GeneralizedCompare() for other types.
        /// </summary>
        /// 
        public static IComparer CaseInsensitiveGeneralizedComparer
        {
            get { return _caseInsensitiveGeneralizedComparer; }
        }
        private static readonly CaseInsensitiveGeneralizedComparerClass
            _caseInsensitiveGeneralizedComparer =
            new CaseInsensitiveGeneralizedComparerClass();

        /// 
        /// <summary>
        /// Checks whether a standard implicit conversion exists from SrcType to TgtType,
        /// as defined in section 6.3.1 of C# Language Specification 1.2.
        /// </summary>
        /// <param name="SrcType">Type to convert from.</param>
        /// <param name="TgtType">Type to convert to.</param>
        /// <returns>True if a standard implicit conversion exists, false otherwise.</returns>
        /// 
        public static bool StandardImplicitConversionExists(Type SrcType, Type TgtType)
        {
            bool result;

            if (TgtType.IsAssignableFrom(SrcType))
                result = true;
            else if (SrcType == typeof(char))
            {
                result = (TgtType == typeof(ushort)) || (TgtType == typeof(int)) ||
                    (TgtType == typeof(uint)) || (TgtType == typeof(long)) ||
                    (TgtType == typeof(ulong)) || (TgtType == typeof(float)) ||
                    (TgtType == typeof(double)) || (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(sbyte))
            {
                result = (TgtType == typeof(short)) || (TgtType == typeof(int)) ||
                    (TgtType == typeof(long)) || (TgtType == typeof(float)) ||
                    (TgtType == typeof(double)) || (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(byte))
            {
                result = (TgtType == typeof(short)) || (TgtType == typeof(ushort)) ||
                    (TgtType == typeof(int)) || (TgtType == typeof(uint)) ||
                    (TgtType == typeof(long)) || (TgtType == typeof(ulong)) ||
                    (TgtType == typeof(float)) || (TgtType == typeof(double)) ||
                    (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(short))
            {
                result = (TgtType == typeof(int)) || (TgtType == typeof(long)) ||
                    (TgtType == typeof(float)) || (TgtType == typeof(double)) ||
                    (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(ushort))
            {
                result = (TgtType == typeof(int)) || (TgtType == typeof(uint)) ||
                    (TgtType == typeof(long)) || (TgtType == typeof(ulong)) ||
                    (TgtType == typeof(float)) || (TgtType == typeof(double)) ||
                    (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(int))
            {
                result = (TgtType == typeof(long)) || (TgtType == typeof(float)) ||
                    (TgtType == typeof(double)) || (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(uint))
            {
                result = (TgtType == typeof(long)) || (TgtType == typeof(ulong)) ||
                    (TgtType == typeof(float)) || (TgtType == typeof(double)) ||
                    (TgtType == typeof(decimal));
            }
            else if ((SrcType == typeof(long)) || (SrcType == typeof(ulong)))
            {
                result = (TgtType == typeof(float)) || (TgtType == typeof(double)) ||
                    (TgtType == typeof(decimal));
            }
            else if (SrcType == typeof(float))
                result = (TgtType == typeof(double));
            else
                result = false;

            return result;
        }

        /// 
        /// <summary>
        /// Convert form one a string value to any any simple type
        /// </summary>
        /// <typeparam name="T">Must be IConverable.</typeparam>
        /// <remarks>IComverable include DateTime, int, bool, string, ectra </remarks>
        /// <param name="Value">String Value that should be used checked</param>
        /// <param name="DefaultValue">Default Value if Exception is throw.</param>
        /// <returns>T representation of String.</returns>
        /// 
        public static T TryParse<T>(string Value, T DefaultValue)
            where T : IConvertible
        {
            T TestValue = DefaultValue;
            try
            { TestValue = (T)Convert.ChangeType(Value, typeof(T)); }
            catch
            { }
            return TestValue;
        }

        /// 
        /// <summary>
        /// Creates a concatenated string of ToString() values for each element in Value,
        /// separated by Separator.
        /// </summary>
        /// <typeparam name="T">Type of the input array.</typeparam>
        /// <param name="Separator">
        /// String with which to separate each element's string value.
        /// </param>
        /// <param name="Value">Array of values to join.</param>
        /// <returns>
        /// String of Separator-separated string values for each element in Value.
        /// </returns>
        /// 
        public static string Join<T>(string Separator, T[] Value)
        {
            return (string.Join(Separator, Array.ConvertAll<T, string>(Value,
                delegate (T element) { return (element == null ? string.Empty : element.ToString()); })));
        }

        /// 
        /// <summary>
        /// Determines whether two arrays contain equal elements in the same order.
        /// </summary>
        /// <typeparam name="T">Type of the arrays' elements.</typeparam>
        /// <param name="ArrayA">First array.</param>
        /// <param name="ArrayB">Second array.</param>
        /// <returns>
        /// True if the arrays' contents are equal, false if they are not equal.
        /// </returns>
        /// 
        public static bool ArrayEquals<T>(T[] ArrayA, T[] ArrayB)
        {
            if (ArrayA == ArrayB)
                return true;
            else if (ArrayA == null || ArrayB == null || ArrayA.Length != ArrayB.Length)
                return false;

            for (int i = 0; i < ArrayA.Length; i++)
            {
                for (int j = 0; j < ArrayB.Length; j++)
                {
                    if (ArrayA[i] == null ? ArrayB[j] != null : !ArrayA[i].Equals(
                        ArrayB[j]))
                    {
                        //
                        // Unequal element found.
                        //
                        return false;
                    }
                }
            }

            //
            // If we get here, all array elements were equal.
            //
            return true;
        }

        /// 
        /// <summary>Similar to List&lt;T&gt;.RemoveAt(), but for Arrays.</summary>
        /// <param name="Array">Array to remove an element from.</param>
        /// <param name="Index">Index of element to remove.</param>
        /// 
        public static void ArrayRemoveAt<T>(ref T[] Array, int Index)
        {
            System.Array.Copy(Array, Index + 1, Array, Index, Array.Length - Index - 1);
            System.Array.Resize(ref Array, Array.Length - 1);
        }

        /// 
        /// <summary>
        /// Attempts to convert a column name in the format "column_name" to a decent
        /// display name. The transform simply capitalizes the first letter and letters
        /// following a non-underscore and an underscore. The underscore in the second
        /// case is replaced with a space.
        /// </summary>
        /// <param name="ColumnName">Database column name.</param>
        /// <returns>Automatically generated display name.</returns>
        /// 
        public static string DisplayNameFromColumnName(string ColumnName)
        {
            return (ColumnName == null ? null : Regex.Replace(ColumnName.ToLower(),
                @"^\p{Ll}|_+.", delegate (Match match)
                {
                    return (match.Length == 1 ? match.Value.ToUpper() : " " +
                    char.ToUpper(match.Value[match.Length - 1]));
                }));
        }


        #region Internal Types

        /// 
        /// <summary>
        /// Defines an IComparer based on DataConverter.GeneralizedCompare().
        /// </summary>
        /// 
        private class GeneralizedComparerClass : IComparer
        {
            /// 
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less
            /// than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// Less than zero, x is less than y. Zero, x equals y. Greater than zero, x
            /// is greater than y.
            /// </returns>
            /// 
            public int Compare(object x, object y)
            {
                return DataConverter.GeneralizedCompare(x, y);
            }
        }

        /// 
        /// <summary>
        /// Defines an IComparer based on DataConverter.GeneralizedCompare().
        /// </summary>
        /// 
        private class CaseInsensitiveGeneralizedComparerClass : IComparer
        {
            /// 
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less
            /// than, equal to, or greater than the other. If both objects are strings, a
            /// case-insensitive compare will be performed.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// Less than zero, x is less than y. Zero, x equals y. Greater than zero, x
            /// is greater than y.
            /// </returns>
            /// 
            public int Compare(object x, object y)
            {
                string xStr, yStr;
                return ((xStr = x as string) != null && (yStr = y as string) != null ?
                    string.Compare(xStr, yStr, true) :
                    DataConverter.GeneralizedCompare(x, y));
            }
        }

        #endregion


        /// <summary>
        /// Aliases for simpler statements
        /// </summary>
        public class CV : DataConverter
        {
        }

        public static object ChangeType(decimal value, Type idColType)
        {
            throw new NotImplementedException();
        }
    }
}
