using Simsip.LineRunner.GameFramework;
using Simsip.LineRunner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if IOS
using Foundation;
#endif

#if WINDOWS_PHONE || NETFX_CORE
using System.Threading.Tasks;
using Windows.Storage;
#endif


namespace Simsip.LineRunner.Data
{
    public class Database
    {
        public static readonly object DATABASE_LOCK = new object();
        public const string DATABASE_NAME = "linerunner.db";
        public const int SKIP_COUNT_NONE = -1;
        public const int PAGE_SIZE_DEFAULT = -1;

#if ANDROID || IOS || DESKTOP
        /// <summary>
        /// Test for platform specific location if database exists.
        /// </summary>
        /// <returns>True if database file is in platform specific location, false if not.</returns>
        public static bool Exists()
        {
            // Construct where we want to test if the datbase exists
            var dbPath = DatabasePath();

            return FileUtils.FileExists(dbPath);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        /// <summary>
        /// Test for platform specific location if database exists.
        /// </summary>
        /// <returns>True if database file is in platform specific location, false if not.</returns>
        public static async Task<bool> ExistsAsync()
        {
            // Construct RELATIVE path where we want to test if the datbase exists
            //
            // IMPORTANT: The api below "DatabasePath" will give a full path to the database as needed
            // by SQLite. This is NOT what we need for our test here.
            var dbPath = Path.Combine(GameConstants.FOLDER_DATA, Database.DATABASE_NAME);

            return await FileUtils.FileExistsAsync(dbPath);
        }
#endif

#if ANDROID
        /// <summary>
        /// Copies an embedded database in the Assets folder over to an appropriate device
        /// specific folder.
        /// 
        /// Important: Does not check if the file is already there. This is assumed to be done
        /// in code logic before it is determined that a call is necessary to this function.
        /// </summary>
        public static void CopyFromAssets()
        {
            // Construct where we copy to
            var dbPath = DatabasePath();
            
            // Construct where we copy from
            var assetsPath = Path.Combine("Database", Database.DATABASE_NAME);
            
            // Copy the file from assets to device specific location
            using (var assetStream = Program.SharedProgram.Assets.Open(assetsPath))
            {
                using (var dbStream = new FileStream(dbPath, FileMode.OpenOrCreate))
                {
                    assetStream.CopyTo(dbStream);
                }
            }
        }
#endif

#if IOS || DESKTOP
        /// <summary>
        /// Copies an embedded database in the Assets folder over to an appropriate device
        /// specific folder.
        /// 
        /// Important: Does not check if the file is already there. This is assumed to be done
        /// in code logic before it is determined that a call is necessary to this function.
        /// </summary>
        public static void CopyFromAssets()
        {
            // Construct where we copy from/to
            var sourcePath = Path.Combine("Database", "linerunner.db");
            var dbPath = DatabasePath();
            
            File.Copy(sourcePath, dbPath);
        }
#endif


#if WINDOWS_PHONE || NETFX_CORE
        /// <summary>
        /// Copies an embedded database in the Assets folder over to an appropriate device
        /// specific folder.
        /// 
        /// Important: Does not check if the file is already there. This is assumed to be done
        /// in code logic before it is determined that a call is necessary to this function.
        /// </summary>
        public static async Task CopyFromAssetsAsync()
        {
            var installPath = Path.Combine("Database", Database.DATABASE_NAME);
            var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var installFile = await installFolder.GetFileAsync(installPath);

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var dataFolder = await localFolder.GetFolderAsync(GameConstants.FOLDER_DATA);
            
            await installFile.CopyAsync(dataFolder);
        }
#endif

        public static string DatabasePath()
        {
            var dbPath = string.Empty;

#if ANDROID
            // Just use whatever directory SpecialFolder.Personal returns
            var localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var libraryPath = Path.Combine(localFolderPath, GameConstants.FOLDER_DATA);
#elif IOS
            // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
            // (they don't want non-user-generated data in Documents)
            // But note for MVVMCross Sqlite plugin that this might not be correct:
            // http://stackoverflow.com/questions/17665669/with-mvvmcross-what-is-the-preferred-way-to-copy-a-prefilled-sqlite-database
            // var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            // var localFolderPath = Path.Combine(documentsPath, "..", "Library"); // Library folder instead
            // var libraryPath = Path.Combine(localFolderPath, GameConstants.FOLDER_DATA);
            var libraryFolder = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            var libraryPath = Path.Combine(libraryFolder, GameConstants.FOLDER_DATA);
#elif WINDOWS_PHONE || NETFX_CORE
            var localFolderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var libraryPath = Path.Combine(localFolderPath, GameConstants.FOLDER_DATA);

#elif DESKTOP
            var libraryPath = GameConstants.FOLDER_DATA;
#endif

            // A platform specific string to use
            dbPath = Path.Combine(libraryPath, Database.DATABASE_NAME);

            return dbPath;
        }
    }
}
