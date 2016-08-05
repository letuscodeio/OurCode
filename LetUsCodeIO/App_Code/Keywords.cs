using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetUsCodeIO.App_Code
{
    /// 
    /// <summary>
    /// This class provides functions to manage "keyword" strings.
    /// </summary>
    /// 
    public class Keywords
    {

        /// 
        /// <summary>
        /// Check for a YES/NO or TRUE/FALSE value
        /// </summary>
        /// <param name="Value">The value to check</param>
        /// <returns>1 if YES/TRUE, 0 if NO/FALSE, -1 if neither</returns>
        ///
        public static int CheckYesNo(string Value)
        {
            Value = Value.ToLower().Trim();
            if (Value == "yes" || Value == "y" || Value == "true" || Value == "on" || Value == "1")
                return 1;
            if (Value == "no" || Value == "n" || Value == "false" || Value == "off" || Value == "0")
                return 0;
            return -1;
        }
        ///
        /// <summary>
        /// Extends the CheckYesNo Just returns a bool value for the result 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// 
        public static bool YesNoToBool(string Value)
        {
            return (CheckYesNo(Value) > 0) ? true : false;
        }

        /// 
        /// <summary>
        /// Get YES/NO option value.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// 
        public static string GetYesNo(bool Value)
        {
            return Value ? "YES" : "NO";
        }

        /// 
        /// <summary>
        /// Get "0"/"1" option value 
        /// </summary>
        /// <remarks>
        /// This provides a standard character respresentation for boolean values.
        /// </remarks>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// 
        public static string GetBoolChar(bool Value)
        {
            return Value ? "1" : "0";
        }
    }

}
