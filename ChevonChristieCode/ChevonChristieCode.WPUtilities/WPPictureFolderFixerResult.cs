using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ChevonChristieCode.WPUtilities
{
   public class WPPictureFolderFixerResult
   {
      public bool IsSuccess { get; set; }

      public string Message { get; set; }

      public override string ToString()
      {
         return string.Format("Folder Fix Success: {0}\nMessage: {1}", IsSuccess ? "Yes" : "No", Message);
      }

   }
}
