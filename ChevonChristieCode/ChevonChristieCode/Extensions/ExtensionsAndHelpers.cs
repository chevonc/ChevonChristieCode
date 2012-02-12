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

using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;
using System.IO;
using System.Xml.Serialization;

namespace ChevonChristie.Extentions
{
   public static class ExtensionsAndHelpers
   {
      /// <summary>
      /// Updates if larger or add.
      /// </summary>
      /// <param name="Dict">The dict.</param>
      /// <param name="Key">The key.</param>
      /// <param name="Value">The value.</param>
      /// <returns></returns>
      public static bool UpdateIfLargerOrAdd(this IDictionary<string, object> Dict, string Key, double Value)
      {
         if (Dict.ContainsKey(Key))
         {
            double oldValue = (double)Dict[Key];

            if (Value > oldValue)
            {
               Dict[Key] = Value;
               return true;
            }
            else
               return false;
         }

         else
         {
            Dict.Add(Key, Value);
            return true;
         }
      }

      /// <summary>
      /// Updates if larger or add.
      /// </summary>
      /// <param name="Dict">The dict.</param>
      /// <param name="Key">The key.</param>
      /// <param name="Value">The value.</param>
      /// <returns></returns>
      public static bool UpdateIfLargerOrAdd(this IDictionary<string, object> Dict, string Key, uint Value)
      {
         if (Dict.ContainsKey(Key))
         {
            uint oldValue = (uint)Dict[Key];

            if (Value > oldValue)
            {
               Dict[Key] = Value;
               return true;
            }
            else
               return false;
         }

         else
         {
            Dict.Add(Key, Value);
            return true;
         }
      }

      /// <summary>
      /// Updates the or add.
      /// </summary>
      /// <param name="Dict">The dict.</param>
      /// <param name="Key">The key.</param>
      /// <param name="Value">The value.</param>
      public static void UpdateOrAdd(this IDictionary<string, object> Dict, string Key, object Value)
      {
         if (Dict.ContainsKey(Key))
            Dict[Key] = Value;
         else
            Dict.Add(Key, Value);
      }

      /// <summary>
      /// Updates the or add.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="Dict">The dict.</param>
      /// <param name="Key">The key.</param>
      /// <param name="Value">The value.</param>
      public static void UpdateOrAdd<T>(this IDictionary<string, T> Dict, string Key, T Value)
      {
         if (Dict.ContainsKey(Key))
            Dict[Key] = Value;
         else
            Dict.Add(Key, Value);
      }

      /// <summary>
      /// Retrieves the specified dict.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="Dict">The dict.</param>
      /// <param name="Key">The key.</param>
      /// <param name="DefaultValue">The default value.</param>
      /// <returns></returns>
      public static T Retrieve<T>(this IDictionary<string, object> Dict, string Key, T DefaultValue)
      {
         if (Dict.ContainsKey(Key))
            return (T)Dict[Key];
         else
            return DefaultValue;
      }

      /// <summary>
      /// Retrieves the specified dict.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="Dict">The dict.</param>
      /// <param name="Key">The key.</param>
      /// <param name="DefaultValue">The default value.</param>
      /// <returns></returns>
      public static T Retrieve<T>(this IDictionary<string, T> Dict, string Key, T DefaultValue)
      {
         if (Dict.ContainsKey(Key))
            return Dict[Key];
         else
            return DefaultValue;
      }

      /// <summary>
      /// Parses the or default.
      /// </summary>
      /// <param name="UnsignedInt">The unsigned int.</param>
      /// <param name="DefaultValue">The default value.</param>
      /// <returns></returns>
      public static uint ParseOrDefault(this string UnsignedInt, uint DefaultValue)
      {
         uint resultValue;
         bool parsed = uint.TryParse(UnsignedInt, out resultValue);

         if (parsed)
            return resultValue;
         else
            return DefaultValue;
      }

      /// <summary>
      /// Inserts if larger.
      /// </summary>
      /// <param name="TargetList">The target list.</param>
      /// <param name="Value">The value.</param>
      /// <param name="maxListSize">Size of the max.</param>
      /// <returns></returns>
      public static bool InsertOrReplaceIfLarger(this List<uint> TargetList, uint Value, int maxListSize)
      {
         if (TargetList.Count < maxListSize)
         {
            TargetList.Add(Value);
            return true;
         }



         uint min = TargetList[0];
         int minIndex = 0;
         bool shouldInsert = false;

         for (int i = 0; i < TargetList.Count; i++)
         {
            if (Value > TargetList[i])
               shouldInsert = true;
            if (TargetList[i] < min)
            {
               min = TargetList[i];
               minIndex = i;
            }
         }

         if (shouldInsert)
         {
            if (TargetList.Count >= maxListSize)
               TargetList.RemoveAt(minIndex);

            TargetList.Add(Value);
            return true;
         }
         else
            return false;
      }

      /// <summary>
      /// Remove items based on predicate function
      /// </summary>
      /// <typeparam name="T">Type of items in collection</typeparam>
      /// <param name="list">target collection</param>
      /// <param name="predicate">predicate function to decide what to remove</param>
      /// <param name="removeAll">determines whether or not all matching items are removed</param>
      /// <returns></returns>
      public static bool Remove<T>(this IList<T> list, Func<T, bool> predicate, bool removeAll = false)
      {
         var items = list.Where(predicate).ToList();

         if (items.Count == 0)
            return false;

         bool isRemoved = false;

         if (removeAll)
         {
            foreach (T item in items)
            {
               isRemoved = list.Remove(item);
            }

            return isRemoved;
         }

         return list.Remove(items[0]);
      }

      /// <summary>
      /// Removes the specified list.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="list">The list.</param>
      /// <param name="predicate">The predicate.</param>
      /// <param name="removeAll">if set to <c>true</c> [remove all].</param>
      /// <returns></returns>
      public static bool Remove<T>(this ICollection<T> list, Func<T, bool> predicate, bool removeAll = false)
      {
         var items = list.Where(predicate).ToList();

         if (items.Count == 0)
            return false;

         bool isRemoved = false;

         if (removeAll)
         {
            foreach (T item in items)
            {
               isRemoved = list.Remove(item);
            }

            return isRemoved;
         }

         return list.Remove(items[0]);
      }
   }
}
