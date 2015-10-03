using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Projbook.Core.Projbook.Core.Snippet.CSharp
{
    public class CSharpMatchingRule
    {
        public string File { get; set; }

        public string[] MatchingChunks { get; set; }

        public static CSharpMatchingRule Parse(string pattern)
        {
            Regex regex = new Regex(@"^\s*([^\s]+)(\s+([^(\s]+)\s*(\([^)]*\s*\))?)?\s*$", RegexOptions.Compiled);
            Match match = regex.Match(pattern);

            if (!match.Success)
            {
                return null; // Todo raise error
            }

            string file = match.Groups[1].Value;
            string rawMember = match.Groups[3].Value;
            string rawParameters = match.Groups[4].Value;

            List<string> matchingChunks = new List<string>();
            matchingChunks.AddRange(rawMember.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));

            string parameterChunk = Regex.Replace(rawParameters, @"\s", string.Empty);
            if (parameterChunk.Length >= 1)
            {
                matchingChunks.Add(Regex.Replace(rawParameters, @"\s", string.Empty));
            }

            return new CSharpMatchingRule
            {
                File = file,
                MatchingChunks = matchingChunks.ToArray()
            };
        }
    }
}
