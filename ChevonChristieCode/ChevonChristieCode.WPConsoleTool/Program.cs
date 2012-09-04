using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChevonChristieCode.WPUtilities;
using System.Windows.Forms;
using System.IO;

namespace ChevonChristieCode.WPConsoleTool
{
   class Program
   {
      private const string STR_PleaseChooseTheFolderYourPhonesImages = "Please, choose the 'camera roll' folder with phone's images. Ex: My Pictures\\Chevon's Nokia Lumia 800\\Camera Roll";
      private const char CHAR_Option1 = '1';
      private const char CHAR_Option2 = '2';
      private const string STR_InstructionsAndOptionsPrompt = @"How would you like to restructure the folder?
1 - using taken/creation date of photos (default/recommended) to rename files.
*This option is best if you have also deleted some photos from your phone since using it to take pictures.

2 - using duplicate gap detection to rename files.
If there are no duplicates files (ex: WP_000234 (2).jpg), this option will quit with an appropriate message. In this case, use the option 1.
*This option is best if you have copied and pasted images to/from the picture folder. The images may or may not have been from other windows phones, where the 
Date and Time setting on the one or more of the phones may have been incorrect or inconsistent";

      [STAThread]
      static void Main(string[] args)
      {

         string folder;

         if (args.Length == 0)
         {
            Console.WriteLine("For the future. Usage: WPConsoleTool.exe \"folder\\path\"\n");
            Console.WriteLine(" ");
            FolderBrowserDialog folderChooser = new FolderBrowserDialog() { Description = STR_PleaseChooseTheFolderYourPhonesImages, RootFolder = Environment.SpecialFolder.MyPictures, ShowNewFolderButton = false };
            folderChooser.ShowDialog();
            while (!Directory.Exists(folder = folderChooser.SelectedPath))
            {
               DialogResult res = MessageBox.Show("You must choose the folder with your phone's images. Would you like to try again?", "No folder selected", MessageBoxButtons.RetryCancel, MessageBoxIcon.Stop);

               if (res == DialogResult.Cancel)
               {
                  return;
               }

               folderChooser.ShowDialog();
            }

            folderChooser.Dispose();
         }
         else
         {
            folder = args[0];
         }

         WPPictureFolderFixer fixer = new WPPictureFolderFixer(folder);

         Console.WriteLine(STR_InstructionsAndOptionsPrompt);

         Console.Write("\n\nEnter an option (1 or 2): ");

         char key;
         while ((key = Char.ToUpper(Convert.ToChar(Console.Read()))) != CHAR_Option1 && key != CHAR_Option2) { }

         WPPictureFolderFixerResult result = null;
         try
         {
            result = fixer.Run(key == CHAR_Option1 ? true : false, Console.WriteLine);
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
