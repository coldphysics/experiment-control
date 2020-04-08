using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PythonUtils.ScriptExecution
{
    public class PythonScriptVariablesAnalyzer
    {

        /// <summary>
        /// Indicates whether the specified variableName is used within the script.
        /// </summary>
        /// <param name="variableName">The program-variable name</param>
        /// <returns><c>true</c> if the script uses the specified variable; otherwise, <c>false</c></returns>
        public static bool IsVariableUsedInScript(string variableName, string script)
        {
            if (String.IsNullOrEmpty(script.Trim()))
                return false;

            string[] wordsInScript = GetWords(script);

            foreach (string word in wordsInScript)
            {
                //Check if the word is a declared python variable within the script
                if (word.Equals(variableName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Replcaes or occurrences of a variable in a script with a new name
        /// </summary>
        /// <param name="originalName">The original name of the variable</param>
        /// <param name="newName">The new name of the variable</param>
        /// <param name="script">The script to handle</param>
        /// <returns>A new version of the script with the new variable name.</returns>
        public static string RenameVariableInScript(string originalName, string newName, string script)
        {
            string originalPattern = string.Format(@"\b{0}\b", originalName);
            string result = Regex.Replace(script, originalPattern, newName);
            return result;
        }
        /// <summary>
        /// Gets the words within a string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>An array of all the words within the input string.</returns>
        private static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w]+\b");
            List<string> resultList = new List<string>();

            foreach (Match match in matches)
            {
                resultList.Add(match.Value);
            }

            return resultList.ToArray();
        }
    }
}
