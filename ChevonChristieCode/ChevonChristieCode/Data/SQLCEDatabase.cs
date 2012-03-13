using System;
using System.Windows;
using System.Data.Linq;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using ChevonChristieCode.Misc;

namespace ChevonChristieCode.Data
{
   /// <summary>
   /// This class provides synchronized access to a SQLCE database for multithreaded scenarios.
   /// SQL CE doesn't like to be queried by multiple threads, and this quickly becomes an issue with 
   /// apps that include synchronizing data in the background and reading data to present to the UI.
   /// Use BeginDatabaseInteraction to block all other threads, except the current, from access the database
   /// Use EndDatabaseInteraction when all databse queries/actions have been completed to allow other threads to access.
   /// Note that when accessing from the UI Thread one should use BeingUIDatabaseInteraction and EndUIDatabaseInteraction
   /// these methods have higher priority that the regular Begin and End, and will trump other threads when it comes to
   /// accessing the database - this way the UI does not have to wait as long.
   /// Note - Do not call any Begin method twice before calling end, you will get a deadlock
   /// Note - Methods internal to the database call Begin and End automatically. You are only responsible for using begin and end when directly
   /// accessing databse tables
   /// </summary>
   public class SQLCEDatabase : DataContext
   {
      private static readonly AutoResetEvent r_DBSync = new AutoResetEvent(true);

      public static bool OutputInfo = true;
      protected internal static string STR_DataSourceIsostoreLocation = "SQLCEDatabase.sdf";
      private const string DataStorePrefix = "Data Source=isostore:";

      private static bool m_IsBeginCalled;
      private static bool m_YieldToUI;
      private static bool m_IsUIAccessing;
      private static SQLCEDatabase m_Singleton;

      /// <summary>
      /// Initializes a new instance of the <see cref="CardManagerDB"/> class.
      /// </summary>
      /// <param name="fileOrConnection">The file or connection.</param>
      public SQLCEDatabase(string fileOrConnection)
         : base(fileOrConnection)
      {

      }

      /// <summary>
      /// Initializes a new instance of the <see cref="CardManagerDB"/> class.
      /// </summary>
      /// <param name="fileOrConnection">The file or connection.</param>
      /// <param name="mapping">The mapping.</param>
      public SQLCEDatabase(string fileOrConnection, System.Data.Linq.Mapping.MappingSource mapping)
         : base(fileOrConnection, mapping)
      {
      }

      /// <summary>
      /// Gets the scan biz cards data base.
      /// </summary>
      public static SQLCEDatabase GlobalInstance
      {
         get
         {
            if (!isInitialized)
               SQLCEDatabase.InitializeDatabase();
            return m_Singleton;
         }
      }

      private static readonly object r_syncRoot = new object();

      /// <summary>
      /// Loads this instance.
      /// </summary>
      private static void Load()
      {
         lock (r_syncRoot)
         {
            if (m_Singleton == null)
               m_Singleton = new SQLCEDatabase(string.Format("{0}{1}", DataStorePrefix, STR_DataSourceIsostoreLocation));
         }
      }

      private static bool isInitialized;

      /// <summary>
      /// Initializes the database.
      /// </summary>
      private static void InitializeDatabase()
      {
         if (SQLCEDatabase.isInitialized)
            return;

         SQLCEDatabase.Load();
         SQLCEDatabase.isInitialized = true;

         SQLCEDatabase.GlobalInstance.ObjectTrackingEnabled = true;
         using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
         {
            if (iso.FileExists(STR_DataSourceIsostoreLocation))
               return;

         }
         CreateMyDatabase();
      }

      /// <summary>
      /// Creates my database.
      /// </summary>
      private static void CreateMyDatabase()
      {
         try
         {
            SQLCEDatabase.GlobalInstance.CreateDatabase();
            SQLCEDatabase.GlobalInstance.SubmitChanges();
            //Insert default objects into database here
         }
         catch (Exception ex)
         {
            CommonHelpers.OutputBegin();
            CommonHelpers.OutputError("Error while creating the DB: " + ex.Message);
            CommonHelpers.OutputEnd();
         }
      }

      /// <summary>
      /// Databases the exists.
      /// </summary>
      /// <param name="overrideResult">if set to <c>true</c> [override result].</param>
      /// <returns></returns>
      internal static bool DatabaseExists(bool overrideResult = false)
      {
         if (!overrideResult)
         {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
               if (iso.FileExists(STR_DataSourceIsostoreLocation))
                  return true;
               else
                  return false;
            }
         }
         else
            return false;
      }


      /// <summary>
      /// Begins the database interaction.
      /// </summary>
      public static void BeginDatabaseInteraction()
      {
         while (m_YieldToUI)
         {
            if (OutputInfo)
            {
               CommonHelpers.OutputBegin();
               CommonHelpers.OutputError("Waiting for UI to finish...\n");
               CommonHelpers.OutputEnd();
            }
            Thread.Sleep(1000);
         }

         InternalBeginDatabaseInteraction();
      }

      /// <summary>
      /// Internals the begin database interaction.
      /// </summary>
      private static void InternalBeginDatabaseInteraction()
      {
         r_DBSync.WaitOne();
         m_IsBeginCalled = true;

         if (OutputInfo)
         {
            StackTrace st = new StackTrace();
            CommonHelpers.OutputBegin();
            CommonHelpers.OutputError(st.ToString());
            CommonHelpers.OutputEnd();
         }
      }

      /// <summary>
      /// Ends the database interaction.
      /// </summary>
      public static void EndDatabaseInteraction()
      {
         if (!m_IsBeginCalled)
            throw new InvalidOperationException("You must call BeginDatabaseInteraction before calling this method!");

         if (OutputInfo)
         {
            CommonHelpers.OutputBegin();
            CommonHelpers.OutputError("End called - getting ready to signal\n");
            CommonHelpers.OutputEnd();

         }
         m_IsBeginCalled = false;
         r_DBSync.Set();
      }

      /// <summary>
      /// Begins the UI database interaction.
      /// </summary>
      public static void BeginUIDatabaseInteraction()
      {
         m_YieldToUI = true;
         InternalBeginDatabaseInteraction();
         m_IsUIAccessing = true;
      }

      /// <summary>
      /// Ends the UI database interaction.
      /// </summary>
      /// <param name="endUIAccess">if set to <c>true</c> [end UI access].</param>
      public static void EndUIDatabaseInteraction(bool endUIAccess = true)
      {
         if (!m_IsUIAccessing)
            throw new InvalidOperationException("BeingUIDatabaseInteraction must be call first!");

         EndDatabaseInteraction();
         m_IsUIAccessing = false;
         m_YieldToUI = !endUIAccess;
      }


      /// <summary>
      /// Computes the set of modified objects to be inserted, updated, or deleted, and executes the appropriate commands to implement the changes to the database.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      public new void SubmitChanges()
      {
         this.SubmitChanges(false);
      }


      /// <summary>
      /// Submits the changes.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      /// <param name="useUIPriority">if set to <c>true</c> [use UI priority].</param>
      public void SubmitChanges(bool useUIPriority = false)
      {
#if DEBUG
         int x = 1;

         while (x-- > 0)
         {
#endif

            bool conflicted = false;
            try
            {
               if (useUIPriority)
                  SQLCEDatabase.BeginUIDatabaseInteraction();
               else
                  SQLCEDatabase.BeginDatabaseInteraction();

               base.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException e1)
            {
               conflicted = true;
               foreach (var item in this.ChangeConflicts)
               {
                  foreach (var memberConflict in item.MemberConflicts)
                  {
                     memberConflict.Resolve(RefreshMode.KeepChanges);

                     Console.WriteLine("Original: " + memberConflict.OriginalValue);
                     Console.WriteLine("Database value: " + memberConflict.DatabaseValue);
                     Console.WriteLine("CurrentValue: " + memberConflict.CurrentValue);
                  }

                  if (!item.IsResolved)
                     item.Resolve(RefreshMode.KeepCurrentValues);
               }
            }
            catch (Exception e)
            {
               conflicted = true;
            }

            if (conflicted)
            {
               try
               {
                  base.SubmitChanges(ConflictMode.ContinueOnConflict);
               }
               catch (Exception e)
               {
                  if (e is NullReferenceException)
                  {
                     Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                           MessageBox.Show("An unrecoverable error has occurred. This application will now quit. If the problem persists, consider reinstalling the application.", "Fatal error", MessageBoxButton.OK);
                           throw new NullReferenceException("Database threw null reference exception");
                        });

                     if (useUIPriority)
                        SQLCEDatabase.EndUIDatabaseInteraction();
                     else
                        SQLCEDatabase.EndDatabaseInteraction(); //TODO: EXTRA call to END methods

                     return;
                  }
               }
            }
#if DEBUG
            CommonHelpers.OutputBegin();
            CommonHelpers.OutputError("getting ready to calling end database access");
            CommonHelpers.OutputEnd();
#endif
            if (useUIPriority)
               SQLCEDatabase.EndUIDatabaseInteraction();
            else
               SQLCEDatabase.EndDatabaseInteraction();
#if DEBUG
         }
#endif
      }


      /// <summary>
      /// Inserts the collection into store.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      /// <typeparam name="T"></typeparam>
      /// <param name="items">The items.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void InsertCollectionIntoStore<T>(IEnumerable<T> items, Table<T> table, bool saveNow = true) where T: class
      {
         try
         {
            SQLCEDatabase.BeginDatabaseInteraction();
            table.InsertAllOnSubmit<T>(items);
         }
         catch (Exception ex)
         {

         }
         SQLCEDatabase.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabase.GlobalInstance.SubmitChanges();
      }

      /// <summary>
      /// Deletes the collection from store.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      /// <typeparam name="T"></typeparam>
      /// <param name="items">The items.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void DeleteCollectionFromStore<T>(IEnumerable<T> items, Table<T> table, bool saveNow = true) where T : class
      {
         try
         {
            SQLCEDatabase.BeginDatabaseInteraction();
            table.DeleteAllOnSubmit<T>(items);
         }
         catch (Exception ex)
         {

         }
         SQLCEDatabase.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabase.GlobalInstance.SubmitChanges();
      }


      /// <summary>
      /// Inserts the specified item.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      /// <typeparam name="T"></typeparam>
      /// <param name="item">The item.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void Insert<T>(T item, Table<T> table, bool saveNow = true) where T : class
      {

         try
         {
            SQLCEDatabase.BeginDatabaseInteraction();
            table.InsertOnSubmit(item);
         }
         catch { }

         SQLCEDatabase.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabase.GlobalInstance.SubmitChanges();
      }

      /// <summary>
      /// Deletes the specified item.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      /// <typeparam name="T"></typeparam>
      /// <param name="item">The item.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void Delete<T>(T item, Table<T> table, bool saveNow = true) where T : class
      {
         try
         {
            SQLCEDatabase.BeginDatabaseInteraction();
            table.DeleteOnSubmit(item);
         }
         catch (Exception ex)
         {

         }
         SQLCEDatabase.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabase.GlobalInstance.SubmitChanges();
      }

      /// <summary>
      /// Deletes all tables.
      /// </summary>
      /// <remarks>No need to call BeginDatabseInteraction. It is called internally.</remarks>
      /// <returns></returns>
      protected static bool DeleteAllTables()
      {
         bool success = true;
         try
         {
            SQLCEDatabase.InternalBeginDatabaseInteraction();
            SQLCEDatabase.GlobalInstance.DeleteDatabase();
            m_Singleton.Dispose();
            m_Singleton = null;
            SQLCEDatabase.Load();
            SQLCEDatabase.isInitialized = true;
            SQLCEDatabase.GlobalInstance.ObjectTrackingEnabled = true;
            SQLCEDatabase.GlobalInstance.CreateDatabase();
            ((DataContext)SQLCEDatabase.GlobalInstance).SubmitChanges(ConflictMode.ContinueOnConflict);
         }
         catch (Exception ex)
         {
            success = false;
         }
         try
         {
            ((DataContext)SQLCEDatabase.GlobalInstance).SubmitChanges(ConflictMode.ContinueOnConflict);
         }
         catch
         {
            success = false;
         }
         SQLCEDatabase.EndDatabaseInteraction();

         return success;
      }
   }
}
