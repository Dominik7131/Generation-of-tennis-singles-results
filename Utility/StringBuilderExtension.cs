using System;
using System.Text;

namespace Utility
{
    public static class StringBuilderExtension
    {
        public static void RemoveLast(this StringBuilder builder, char characterToRemove)
        {
            if (builder.Length != 0 && builder[^1] == characterToRemove)
            {
                // Remove last char if it is characterToRemove
                builder.Length--;
            }
        }

        // From https://stackoverflow.com/a/17580258
        public static bool EndsWith(this StringBuilder builder, string test)
        {
            if (builder.Length < test.Length)
            {
                return false;
            }

            string end = builder.ToString(builder.Length - test.Length, test.Length);
            return end.Equals(test);
        }

        // From https://stackoverflow.com/a/23627283
        public static int LastIndexOf(this StringBuilder builder, char find, bool ignoreCase = false, int startIndex = -1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (startIndex == -1)
            {
                startIndex = builder.Length - 1;
            }
            if (startIndex < 0 || startIndex >= builder.Length)
            {
                throw new ArgumentException("startIndex must be between 0 and sb.Lengh-1", nameof(builder));
            }

            int lastIndex = -1;
            if (ignoreCase)
            {
                find = char.ToUpper(find);
            }
            for (int i = startIndex; i >= 0; i--)
            {
                char c = ignoreCase ? char.ToUpper(builder[i]) : (builder[i]);
                if (find == c)
                {
                    lastIndex = i;
                    break;
                }
            }
            return lastIndex;
        }
    }
}