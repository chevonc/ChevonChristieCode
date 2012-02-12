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
   public class SQLCEDatabse : DataContext
   {
      private static readonly AutoResetEvent r_DBSync = new AutoResetEvent(true);

      public static bool OutputInfo = true;
      protected internal static string STR_DataSourceIsostoreLocation = "SQLCEDatabase.sdf";
      private const string DataStorePrefix = "Data Source=isostore:";

      private static bool m_IsBeginCalled;
      private static bool m_YieldToUI;
      private static bool m_IsUIAccessing;
      private static SQLCEDatabse m_Singleton;

      /// <summary>
      /// Initializes a new instance of the <see cref="CardManagerDB"/> class.
      /// </summary>
      /// <param name="fileOrConnection">The file or connection.</param>
      public SQLCEDatabse(string fileOrConnection)
         : base(fileOrConnection)
      {

      }

      /// <summary>
      /// Initializes a new instance of the <see cref="CardManagerDB"/> class.
      /// </summary>
      /// <param name="fileOrConnection">The file or connection.</param>
      /// <param name="mapping">The mapping.</param>
      public SQLCEDatabse(string fileOrConnection, System.Data.Linq.Mapping.MappingSource mapping)
         : base(fileOrConnection, mapping)
      {
      }

      /// <summary>
      /// Gets the scan biz cards data base.
      /// </summary>
      public static SQLCEDatabse GlobalInstance
      {
         get
         {
            if (!isInitialized)
               SQLCEDatabse.InitializeDatabase();
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
               m_Singleton = new SQLCEDatabse(string.Format("{0}{1}", DataStorePrefix, STR_DataSourceIsostoreLocation));
         }
      }

      private static bool isInitialized;

      /// <summary>
      /// Initializes the database.
      /// </summary>
      private static void InitializeDatabase()
      {
         if (SQLCEDatabse.isInitialized)
            return;

         SQLCEDatabse.Load();
         SQLCEDatabse.isInitialized = true;

         SQLCEDatabse.GlobalInstance.ObjectTrackingEnabled = true;
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
            // Generate the database (with structure) from the code-based data context
            SQLCEDatabse.GlobalInstance.CreateDatabase();
            SQLCEDatabse.BeginDatabaseInteraction();
            SQLCEDatabse.EndDatabaseInteraction();
            SQLCEDatabse.GlobalInstance.SubmitChanges();
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
      public new void SubmitChanges()
      {
         this.SubmitChanges(false);
      }


      /// <summary>
      /// Submits the changes.
      /// </summary>
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
                  SQLCEDatabse.BeginUIDatabaseInteraction();
               else
                  SQLCEDatabse.BeginDatabaseInteraction();

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
                        SQLCEDatabse.EndUIDatabaseInteraction();
                     else
                        SQLCEDatabse.EndDatabaseInteraction(); //TODO: EXTRA call to END methods

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
               SQLCEDatabse.EndUIDatabaseInteraction();
            else
               SQLCEDatabse.EndDatabaseInteraction();
#if DEBUG
         }
#endif
      }


      /// <summary>
      /// Inserts the collection into store.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="items">The items.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void InsertCollectionIntoStore<T>(IEnumerable<T> items, Table<T> table, bool saveNow = true) where T: class
      {
         try
         {
            SQLCEDatabse.BeginDatabaseInteraction();
            table.InsertAllOnSubmit<T>(items);
         }
         catch (Exception ex)
         {

         }
         SQLCEDatabse.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabse.GlobalInstance.SubmitChanges();
      }

      /// <summary>
      /// Deletes the collection from store.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="items">The items.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void DeleteCollectionFromStore<T>(IEnumerable<T> items, Table<T> table, bool saveNow = true) where T : class
      {
         try
         {
            SQLCEDatabse.BeginDatabaseInteraction();
            table.DeleteAllOnSubmit<T>(items);
         }
         catch (Exception ex)
         {

         }
         SQLCEDatabse.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabse.GlobalInstance.SubmitChanges();
      }


      /// <summary>
      /// Inserts the specified item.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="item">The item.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void Insert<T>(T item, Table<T> table, bool saveNow = true) where T : class
      {

         try
         {
            SQLCEDatabse.BeginDatabaseInteraction();
            table.InsertOnSubmit(item);
         }
         catch { }

         SQLCEDatabse.EndDatabaseInteraction();

         if (saveNow)
            SQLCEDatabse.GlobalInstance.SubmitChanges();
      }



      /// <summary>
      /// Deletes the specified item.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="item">The item.</param>
      /// <param name="table">The table.</param>
      /// <param name="saveNow">if set to <c>true</c> [save now].</param>
      public static void Delete<T>(T item, Table<T> table, bool saveNow = true) where T : class
      {
         try
         {
            SQLCEDatabse.BeginDatabaseInteraction();
            table.DeleteOnSubmit(item);
         }
         catch (Exception ex)
         {

         }
         SQLCEDatabse.BeginDatabaseInteraction();

         if (saveNow)
            SQLCEDatabse.GlobalInstance.SubmitChanges();
      }

      /// <summary>
      /// Deletes all tables.
      /// </summary>
      /// <returns></returns>
      protected static bool DeleteAllTables()
      {
         bool success = true;
         try
         {
            SQLCEDatabse.InternalBeginDatabaseInteraction();
            SQLCEDatabse.GlobalInstance.DeleteDatabase();
            m_Singleton.Dispose();
            m_Singleton = null;
            SQLCEDatabse.Load();
            SQLCEDatabse.isInitialized = true;
            SQLCEDatabse.GlobalInstance.ObjectTrackingEnabled = true;
            SQLCEDatabse.GlobalInstance.CreateDatabase();
            ((DataContext)SQLCEDatabse.GlobalInstance).SubmitChanges(ConflictMode.ContinueOnConflict);
         }
         catch (Exception ex)
         {
            success = false;
         }
         try
         {
            ((DataContext)SQLCEDatabse.GlobalInstance).SubmitChanges(ConflictMode.ContinueOnConflict);
         }
         catch
         {
            success = false;
         }
         SQLCEDatabse.EndDatabaseInteraction();

         return success;
      }
   }
}
