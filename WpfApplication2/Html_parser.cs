using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace prxSearcher
{
    static class html_parser
    {
        public static string Replace(string Str, string expr, string subStr)
        {
            Regex re = new Regex(expr, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return re.Replace(Str, subStr);
        }

        public static string Match(string Str, string expr)
        {
            Regex re = new Regex(expr, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return re.Match(Str).Value;
        }

        public static string[] Matches(string Str, string expr)
        {
            Regex re = new Regex(expr, RegexOptions.IgnoreCase);
            MatchCollection mc = re.Matches(Str);
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < mc.Count; i++)
            {
                s.Append(mc[i].Value);
                if (i + 1 < mc.Count)
                {
                    s.Append('\r');
                }
            }
            return s.ToString().Split('\r');
        }

        public static bool Test(string Str, string expr)
        {
            Regex re = new Regex(expr, RegexOptions.IgnoreCase);
            return re.IsMatch(Str);
        }

        public static string ClearUrl(string dirtyUrl)
        {
            return Replace(dirtyUrl, "<.*?>", "");
        }
    }
}
