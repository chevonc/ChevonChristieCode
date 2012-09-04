using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ChevonChristieCode.WPUtilities
{
   public class WPPictureFolderFixer
   {
      private readonly DirectoryInfo m_sourceFolder;
      private readonly int m_duplicateStart;
      private Action<string> m_writer;
      private const string m_fileNamePrefix = "WP_{0}.jpg";
      private const string m_WPNumFormat = "000000";

      public WPPictureFolderFixer(string folderPath, int duplicatedStartNum = 2)
      {
         m_sourceFolder = new DirectoryInfo(folderPath);
         if (!m_sourceFolder.Exists)
            throw new IOException("The picture folder specified does not exist!");

         m_duplicateStart = duplicatedStartNum;
      }

      public WPPictureFolderFixerResult Run(bool fixUsingTakenDate = true, Action<string> writer = null)
      {
         m_writer = writer;
         DirectoryInfo fixedDir = null;

         try
         {
            fixedDir = new DirectoryInfo(String.Format("{0}\\{1}_PictureFixes", m_sourceFolder.Parent.FullName, m_sourceFolder.Name));

            if (fixedDir.Exists)
               fixedDir.Delete(true);

            fixedDir.Create();
            fixedDir.Refresh();
         }
         catch
         {
            return new WPPictureFolderFixerResult() { Message = "Could not create temporary folder", IsSuccess = false };
         }

         var wpImages = m_sourceFolder.GetFiles().Where(file => file.Exists && file.Name.Contains("WP_") &&
            file.Extension != null && file.Extension.ToLower() == ".jpg").ToList();
         return fixUsingTakenDate ? FixFilesUsingTakenDate(fixedDir, wpImages) : FixFilesUsingDuplicateScheme(fixedDir, wpImages);
      }

      private WPPictureFolderFixerResult FixFilesUsingDuplicateScheme(DirectoryInfo fixedDir, List<FileInfo> files)
      {

         try
         {
            var duppedFiles = files.Where(file => file.Name.Contains('(')).ToList();

            if (duppedFiles.Count == 0)
            {
               CleanUp(fixedDir);
               return new WPPictureFolderFixerResult() { Message = "No duplicate files.", IsSuccess = true };
            }

            duppedFiles.Sort((file1, file2) => Comparer<int>.Default.Compare(GetFileNum(file1.Name), GetFileNum(file2.Name)));

            int lastDupped = GetFileNum(duppedFiles[duppedFiles.Count - 1].Name);
            int fileCounter = lastDupped + 1;
            var newestFiles = files.Where(file => GetFileNum(file.Name) > lastDupped).ToList();

            Write(String.Format("There are {0} duplicate files in {1}.", duppedFiles.Count, m_sourceFolder.FullName));
            Write(fileCounter.ToString() + " files will be ignored.");
            Write((duppedFiles.Count + newestFiles.Count - 1).ToString() + " files need fixing.");

            List<List<FileInfo>> dupBundles = new List<List<FileInfo>>();
            //grab duplicate files
            for (int i = m_duplicateStart; i < int.MaxValue; i++)
            {
               var bundle = duppedFiles.Where(file => file.Name.Contains(string.Format("({0})", i))).ToList();
               if (bundle.Count == 0)
                  break;

               dupBundles.Add(bundle);
            }

            //move all duplicates temporarily
            Write("Temporarily moving files to be fixed...");
            foreach (var file in duppedFiles)
            {
               file.MoveTo(String.Format("{0}\\{1}", fixedDir.FullName, file.Name));
               file.Refresh();
            }

            foreach (var file in newestFiles)
            {
               file.MoveTo(String.Format("{0}\\{1}", fixedDir.FullName, file.Name));
               file.Refresh();
            }

            Write("Moving finished!");
            //move them back with corrected names
            foreach (var bundle in dupBundles)
            {
               foreach (var file in bundle)
               {
                  var oldName = file.Name;
                  var dstFileName = string.Format(m_fileNamePrefix, fileCounter.ToString(m_WPNumFormat));
                  file.MoveTo(String.Format("{0}\\{1}", m_sourceFolder.FullName, dstFileName));
                  file.Refresh();
                  fileCounter++;
                  Write(string.Format("Fixed: {0} to {1}!", oldName, dstFileName));
               }
            }

            var remaining = fixedDir.GetFiles();
            foreach (var file in remaining)
            {
               var dstFileName = string.Format(m_fileNamePrefix, fileCounter.ToString(m_WPNumFormat));
               file.MoveTo(String.Format("{0}\\{1}", m_sourceFolder.FullName, dstFileName));
               file.Refresh();
               fileCounter++;
            }

            fixedDir.Refresh();
         }
         catch
         {
            CleanUp(fixedDir);
            return new WPPictureFolderFixerResult() { Message = "Could not fix files!", IsSuccess = false };
         }

         CleanUp(fixedDir);
         return new WPPictureFolderFixerResult() { Message = "No Errors", IsSuccess = true };
      }

      private WPPictureFolderFixerResult FixFilesUsingTakenDate(DirectoryInfo fixedDir, List<FileInfo> files)
      {
         try
         {
            Dictionary<FileInfo, DateTime> fileAndDates = new Dictionary<FileInfo, DateTime>();

            foreach (var file in files)
            {
               Write("Reading taken date for: " + file.Name);
               var exif = ExifLibrary.ImageFile.FromFile(file.FullName);
               DateTime orig;
               try
               {
                  orig = (DateTime)exif.Properties[ExifLibrary.ExifTag.DateTimeOriginal].Value;
               }
               catch
               {
                  orig = file.LastWriteTime; //log errors and output stats
               }

               file.MoveTo(String.Format("{0}\\{1}", fixedDir.FullName, file.Name));
               file.Refresh();
               fileAndDates.Add(file, orig);
            }

            fixedDir.Refresh();


            files = (from fds in fileAndDates orderby fds.Value ascending select fds.Key).ToList();

            int counter = 0;

            foreach (var file in files)
            {
               var oldName = file.Name;
               var newName = string.Format(m_fileNamePrefix, (counter++).ToString(m_WPNumFormat));
               
               if(oldName != newName)
                  Write(String.Format("Fixed {0} to {1}", oldName, newName));

               file.MoveTo(String.Format("{0}\\{1}", m_sourceFolder.FullName, newName));
               file.Refresh();
            }

            fixedDir.Refresh();

         }
         catch
         {
            CleanUp(fixedDir);
            return new WPPictureFolderFixerResult() { Message = "Could not fix files!", IsSuccess = false };
         }

         CleanUp(fixedDir);
         return new WPPictureFolderFixerResult() { Message = "No Errors", IsSuccess = true };
      }

      private void CleanUp(DirectoryInfo cleanUpDir)
      {
         if (cleanUpDir == null)
            return;

         var files = cleanUpDir.GetFiles();

         foreach (var file in files)
         {
            try
            {
               file.MoveTo(String.Format("{0}\\{1}", m_sourceFolder.FullName, file.Name));
            }
            catch (IOException)
            {
               file.MoveTo(String.Format("{0}\\error_{1}", m_sourceFolder.FullName, file.Name));
            }
         }

         cleanUpDir.Delete(true);
      }

      private void Write(string msg)
      {
         if (m_writer != null)
            m_writer(msg);
      }

      private static int GetFileNum(string fileName)
      {
         int space = fileName.IndexOf(' ');

         if (space != -1)
            fileName = fileName.Substring(0, space);

         var s = fileName.First(c => char.IsDigit(c));
         var si = fileName.IndexOf(s);
         var e = fileName.Last(c => char.IsDigit(c));
         var ei = fileName.LastIndexOf(e);
         var numString = fileName.Substring(si, ei - si + 1);
         return int.Parse(numString);
      }
   }
}
