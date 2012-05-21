using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChevonChristieCode.WPUtilities;

namespace ChevonChristieCode.WPConsoleTool
{
   class Program
   {
      static void Main(string[] args)
      {

         string folder;

         if (args.Length == 0)
         {
            Console.WriteLine("For the future. Usage: WPConsoleTool.exe \"folder\\path\"\n");
            Console.WriteLine("Please, enter the folder location for your phone's images: ");
            folder = Console.ReadLine();
         }
         else
            folder = args[0];

         WPPictureFolderFixer fixer = new WPPictureFolderFixer(folder);

         Console.WriteLine(@"How would you like to restructure the folder?
      D - using taken/creation date of photos (default) to rename files. Will be slower.
         *This option is best if you have also deleted some photos from your phone since using it to take pictures.
      
      C - using duplicate gap detection to rename files.
         If there are no duplicates files (ex: WP_000234 (2).jpg), this option will quit with an appropriate message. In this case, use the D option.
         *This option is best if you have copied and pasted images to/from the picture folder. The images may or may not have been from other windows phones.
");

         Console.Write("Enter an option (D or C): ");

         char key;
         while ((key = Char.ToUpper(Convert.ToChar(Console.Read()))) != 'D' && key != 'C') { }

         WPPictureFolderFixerResult result = null;
         try
         {
            result = fixer.Run(key == 'D' ? true : false, Console.WriteLine);
         }
         catch (Exception e)
         {
            Console.WriteLine("FATAL ERROR! Please, see below:\n");
            Console.WriteLine(e);
            return;
         }

         Console.WriteLine(string.Format("\nOperation Complete!\n{0}", result));
         Console.WriteLine("Press any key to continue... ");
         Console.ReadKey();
      }
   }
}
