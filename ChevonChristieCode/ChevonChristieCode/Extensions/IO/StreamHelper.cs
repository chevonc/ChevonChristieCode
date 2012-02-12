
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace ChevonChristie.Extentions.IO
{
   public static class IOHelpers
   {
      /// <summary>
      /// Reads to end.
      /// </summary>
      /// <param name="stream">The stream.</param>
      /// <returns></returns>
      public static byte[] ReadToEndExtension(this Stream stream)
      {
         long originalPosition = stream.Position;
         stream.Position = 0;

         try
         {
            byte[] readBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
               totalBytesRead += bytesRead;

               if (totalBytesRead == readBuffer.Length)
               {
                  int nextByte = stream.ReadByte();
                  if (nextByte != -1)
                  {
                     byte[] temp = new byte[readBuffer.Length * 2];
                     Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                     Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                     readBuffer = temp;
                     totalBytesRead++;
                  }
               }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
               buffer = new byte[totalBytesRead];
               Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
         }
         finally
         {
            stream.Position = originalPosition;
         }
      }

      /// <summary>
      /// Reads to end.
      /// </summary>
      /// <param name="stream">The stream.</param>
      /// <returns></returns>
      public static byte[] ReadToEnd(Stream stream)
      {
         return stream.ReadToEndExtension();
      }

      /// <summary>
      /// Datas the load serialize.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="fileLocation">The file location.</param>
      /// <param name="defaultValue">The default value.</param>
      /// <returns></returns>
      public static T DeSerializeData<T>(string fileLocation, T defaultValue)
      {
         using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
         {
            try
            {
               TextReader tr = new StreamReader(isf.OpenFile(fileLocation, FileMode.Open));
               XmlSerializer sr = new XmlSerializer(typeof(T));
               return (T)sr.Deserialize(tr);
            }
            catch (Exception e)
            {
               string x = e.Message;
               return defaultValue;
            }
         }
      }

      /// <summary>
      /// Serializes the data.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="fileLocation">Name of the file.</param>
      /// <param name="collection">The collection.</param>
      /// <returns></returns>
      public static bool SerializeData<T>(string fileLocation, T collection)
      {

         try
         {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
               TextWriter tw = new StreamWriter(new IsolatedStorageFileStream(fileLocation, FileMode.Create, isf));
               XmlSerializer sr = new XmlSerializer(typeof(T));
               sr.Serialize(tw, collection);
               tw.Close();
            }
         }

         catch (Exception e)
         {
            string x = e.Message;
            return false;
         }

         return true;
      }
   }
}
