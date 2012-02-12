using System;
using System.Text.RegularExpressions;

namespace ChevonChristieCode.Misc
{
   public static class GetHtmlText
   {
      /// <summary>
      /// Gets the inner htmltext.
      /// </summary>
      /// <param name="data">The data.</param>
      /// <returns></returns>
      public static string GetInnerHtmltext(string data)
      {
         if (data == null)
            throw new ArgumentException("data: must not be null!");
         
         string decode = System.Net.HttpUtility.HtmlDecode(data);
         Regex objRegExp = new Regex("<(.|\n)+?>");
         string replace = objRegExp.Replace(decode, "");
         return replace.Trim(new char[]{'\t','\r','\n'});
      }
   }
}
