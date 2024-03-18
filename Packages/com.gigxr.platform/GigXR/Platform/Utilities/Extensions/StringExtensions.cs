using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            {
                if (str.Length == 1)
                {
                    return char.ToLower(str[0]).ToString();
                }
                else
                {
                    int endIndex = 1;
                    
                    // Check to see if any other characters need to be made lower, the last upper case letter does not change
                    for (int n = 1; n < str.Length - 1; n++)
                    {
                        if (char.IsUpper(str[n]) && char.IsUpper(str[n + 1]))
                        {
                            endIndex = n + 1;
                        }
                    }

                    return str[0..endIndex].ToLower() + str[endIndex..];
                }
            }

            return str;
        }
    }
}