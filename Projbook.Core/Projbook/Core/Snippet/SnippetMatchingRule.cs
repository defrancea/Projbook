using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Projbook.Core.Projbook.Core.Snippet
{
    public class SnippetMatchingRule
    {
        public string File { get; set; }

        public string[] MemberChunks { get; set; }

        public string[] Parameters { get; set; }

        public bool IsMethod { get; set; }

        public static SnippetMatchingRule Parse(string pattern)
        {
            Regex regex = new Regex(@"^\s*([^\s]+)(\s+([^(\s]+)\s*(\(([^)]*\s*)\))?)?\s*$", RegexOptions.Compiled);
            Match match = regex.Match(pattern);

            if (!match.Success)
            {
                return null;
            }

            string file = match.Groups[1].Value;
            string rawMember = match.Groups[3].Value;
            bool isMethod = match.Groups[4].Length > 0;
            string rawParameters = match.Groups[5].Value;

            string[] memberChunk = rawMember.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            string[] parameters = rawParameters.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            return new SnippetMatchingRule
            {
                File = file,
                MemberChunks = memberChunk,
                IsMethod = isMethod,
                Parameters = parameters
            };
        }
    }
}
