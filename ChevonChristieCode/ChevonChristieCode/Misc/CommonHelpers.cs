using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace ChevonChristieCode.Misc
{
   public static class CommonHelpers
   {

      /// <summary>
      /// Outputs the error.
      /// </summary>
      /// <param name="msg">The MSG.</param>
      public static void OutputError(string msg)
      {

         Debug.WriteLine(msg);

      }

      /// <summary>
      /// Outputs the begin.
      /// </summary>
      public static void OutputBegin()
      {
         Debug.WriteLine("-------------------------------");

      }

      /// <summary>
      /// Outputs the end.
      /// </summary>
      public static void OutputEnd()
      {
         Debug.WriteLine("********************************");

      }

      /// <summary>
      /// Outputs the error.
      /// </summary>
      /// <param name="e">The e.</param>
      public static void OutputError(Exception e)
      {

         OutputBegin();
         StackTrace stack = new StackTrace();

         Debug.WriteLine("Stack trace: ");

         Debug.WriteLine(e.StackTrace);
         Debug.WriteLine(e.Message);

         OutputEnd();
      }
   }
}
