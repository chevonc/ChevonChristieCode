using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChevonChristieCode.Misc
{
    public static class WordProcessor
    {
       /// <summary>
       /// //parameter sytntax with "this" and static declaration of method marks it as extension
       /// </summary>
       /// <param name="sentence">The sentence.</param>
       /// <param name="capitalizeWords">if set to <c>true</c> [capitalize words].</param>
       /// <param name="reverseOrder">if set to <c>true</c> [reverse order].</param>
       /// <param name="reverseWords">if set to <c>true</c> [reverse words].</param>
       /// <returns></returns>
        public static List<string> GetWords(this string sentence,
            bool capitalizeWords = false,
            bool reverseOrder = false,
            bool reverseWords = false)
        {
            List<string> words = new List<string>(sentence.Split(' '));
            if (capitalizeWords)
                words = CapitalizeWords(words);
            if (reverseOrder)
                words = ReverseOrder(words);
            if (reverseWords)
                words = ReverseWords(words);

            return words;
        }

        /// <summary>
        /// Capitalizes the words.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <returns></returns>
        public static List<string> CapitalizeWords(List<string> words)
        {
            List<string> capitlaizedWords = new List<string>();
            foreach (string word in words)
            {
                if (word.Length == 0)
                    continue;
                if (word.Length == 1)
                    capitlaizedWords.Add(word[0].ToString().ToUpper()); //Changes first letter to upper case
                else
                    capitlaizedWords.Add(word[0].ToString().ToUpper() + word.Substring(1)); //substring gets all letters but first
            }
            return capitlaizedWords;
        }

        /// <summary>
        /// Reverses the order.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <returns></returns>
        public static List<string> ReverseOrder(List<string> words)
        {
            List<string> reversedWords = new List<string>();
            for (int wordIndex = words.Count - 1; wordIndex >= 0; wordIndex--)
                reversedWords.Add(words[wordIndex]);

            return reversedWords;
        }


        /// <summary>
        /// Reverses the words.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <returns></returns>
        public static List<string> ReverseWords(List<string> words)
        {
            List<string> reversedWords = new List<string>();
            foreach (string word in words)
                reversedWords.Add(ReverseWord(word)); //calls the other reverseword method... not recursive
            return reversedWords;
        }

        /// <summary>
        /// Reverses the word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string ReverseWord(string word)
        {
            StringBuilder sb = new StringBuilder();
            for (int charIndex = word.Length - 1; charIndex >= 0; charIndex--)
                sb.Append(word[charIndex]);

            return sb.ToString();
        }

        /// <summary>
        /// Extended Method to revervse string version of a variable.
        /// parameter sytntax with "this" and static declaration of method marks it as extension
        /// </summary>
        /// <param name="inputObject">The input object.</param>
        /// <returns></returns>
        public static string ToStringReversed(this object inputObject)
        {
            return ReverseWord(inputObject.ToString());
        }

        /// <summary>
        /// //parameter sytntax with "this" and static declaration of method marks it as extension
        /// </summary>
        /// <param name="words">The words.</param>
        /// <returns></returns>
        public static string AsSentence(this List<string> words) 
        {
            StringBuilder sb = new StringBuilder();
            for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
            {
                sb.Append(words[wordIndex]);
                if (wordIndex != words.Count - 1)
                    sb.Append(' ');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Uppers the case first letter.
        /// </summary>
        /// <param name="StringToUpperCase">The string to upper case.</param>
        /// <returns></returns>
       public static string UpperCaseFirstLetter(this string StringToUpperCase)
       {
          return char.ToUpper(StringToUpperCase[0]) + StringToUpperCase.Substring(1).ToLower();
       }
    }
}
