using Cocos2D;
using Simsip.LineRunner.GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
#if IOS
using Foundation;
#endif

#if WINDOWS_PHONE || NETFX_CORE
using Windows.Storage;
#endif

namespace Simsip.LineRunner.Utils
{
    public class FileUtils
    {
#if (ANDROID || IOS || DESKTOP)
        public static void InitializeFolders()
        {
            if (!FolderExists(GameConstants.FOLDER_CONFIG))
            {
                CreateFolder(GameConstants.FOLDER_CONFIG);
            }
#if IOS
            if (!FolderExists(GameConstants.FOLDER_DATA, libraryFolder: true))
            {
                CreateFolder(GameConstants.FOLDER_DATA, libraryFolder: true);
            }
#else
            if (!FolderExists(GameConstants.FOLDER_DATA))
            {
                CreateFolder(GameConstants.FOLDER_DATA);
            }
#endif
            if (!FolderExists(GameConstants.FOLDER_LOGS))
            {
                CreateFolder(GameConstants.FOLDER_LOGS);
            }
            if (!FolderExists(GameConstants.FOLDER_RESOURCE_PACKS))
            {
                CreateFolder(GameConstants.FOLDER_RESOURCE_PACKS);
            }
            if (!FolderExists(GameConstants.FOLDER_SAVES))
            {
                CreateFolder(GameConstants.FOLDER_SAVES);
            }
            if (!FolderExists(GameConstants.FOLDER_SHADER_PACKS))
            {
                CreateFolder(GameConstants.FOLDER_SHADER_PACKS);
            }
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task InitializeFoldersAsync()
        {
            if (!await FolderExistsAsync(GameConstants.FOLDER_CONFIG))
            {
                await CreateFolderAsync(GameConstants.FOLDER_CONFIG);
            }
            if (!await FolderExistsAsync(GameConstants.FOLDER_DATA))
            {
                await CreateFolderAsync(GameConstants.FOLDER_DATA);
            }
            if (!await FolderExistsAsync(GameConstants.FOLDER_LOGS))
            {
                await CreateFolderAsync(GameConstants.FOLDER_LOGS);
            }
            if (!await FolderExistsAsync(GameConstants.FOLDER_RESOURCE_PACKS))
            {
                await CreateFolderAsync(GameConstants.FOLDER_RESOURCE_PACKS);
            }
            if (!await FolderExistsAsync(GameConstants.FOLDER_SAVES))
            {
                await CreateFolderAsync(GameConstants.FOLDER_SAVES);
            }
            if (!await FolderExistsAsync(GameConstants.FOLDER_SHADER_PACKS))
            {
                await CreateFolderAsync(GameConstants.FOLDER_SHADER_PACKS);
            }
        }
#endif

#if (ANDROID)
        public static bool FolderExists(string folderPath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFolderPath = Path.Combine(documentsPath, folderPath);
            return Directory.Exists(fullFolderPath);
        }
#endif


#if DESKTOP
        public static bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }
#endif

#if (IOS)
        public static bool FolderExists(string folderPath, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFolderPath = Path.Combine(rootPath, folderPath);
            return Directory.Exists(fullFolderPath);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task<bool> FolderExistsAsync(string folderPath)
        {
            // Currently only way to do this on WS 8.0, moving to 8.1 we get the api
            try
            {
                var local = Windows.Storage.ApplicationData.Current.LocalFolder;
                var folder = await local.GetItemAsync(folderPath);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
#endif

#if (ANDROID)
        public static void DeleteFolder(string folderPath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFolderPath = Path.Combine(documentsPath, folderPath);
            Directory.Delete(fullFolderPath);
        }
#endif

#if DESKTOP
        public static void DeleteFolder(string folderPath)
        {
            Directory.Delete(folderPath);
        }
#endif

#if (IOS)
        public static void DeleteFolder(string folderPath, bool libraryFolder = false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFolderPath = Path.Combine(rootPath, folderPath);
            Directory.Delete(fullFolderPath);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task DeleteFolderAsync(string folderPath)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folder = await local.GetFolderAsync(folderPath);
            await folder.DeleteAsync();
        }
#endif

#if ANDROID
        public static void CreateFolder(string folderPath)
        {
            /*
            Reference:http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/files/
            OS 8 has changed the directory structure used by apps (see Apple's note). 
            Use the following code in iOS 8 instead of the Environment class
            var documentsPath = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User) [0];
            */

            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFolderPath = Path.Combine(documentsPath, folderPath);
            Directory.CreateDirectory(fullFolderPath);    
        }
#endif

#if DESKTOP
        public static void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }
#endif

#if IOS
        public static void CreateFolder(string folderPath, bool libraryFolder=false)
        {
            // References:
            // http://developer.xamarin.com/guides/ios/application_fundamentals/working_with_the_file_system/ 
            // https://developer.apple.com/library/ios/technotes/tn2406/_index.html
            // https://developer.apple.com/library/ios/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html
            // https://developer.apple.com/library/ios/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/MacOSXDirectories/MacOSXDirectories.html

            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;    
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;    
            }

            var fullFolderPath = Path.Combine(rootPath, folderPath);
            Directory.CreateDirectory(fullFolderPath);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task CreateFolderAsync(string folderPath)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            await localFolder.CreateFolderAsync(folderPath, CreationCollisionOption.FailIfExists);
        }
#endif

#if ANDROID 

        public static void CopyFolder(string sourceDirName, string destDirName)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullSourceDirName = Path.Combine(documentsPath, sourceDirName);
            var fullDestDirName = Path.Combine(documentsPath, destDirName);

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(fullSourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(fullDestDirName))
            {
                Directory.CreateDirectory(fullDestDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(fullDestDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // Now recursively copy subdirectories
            // TODO: Needs to be tested
            /*
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                FileUtils.CopyFolder(subdir.FullName, temppath);
            }
            */
        }

#endif

#if DESKTOP 

        public static void CopyFolder(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // Now recursively copy subdirectories
            // TODO: Needs to be tested
            /*
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                FileUtils.CopyFolder(subdir.FullName, temppath);
            }
            */
        }

#endif

#if IOS

        public static void CopyFolder(string sourceDirName, string destDirName, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullSourceDirName = Path.Combine(rootPath, sourceDirName);
            var fullDestDirName = Path.Combine(rootPath, destDirName);

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(fullSourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(fullDestDirName))
            {
                Directory.CreateDirectory(fullDestDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(fullDestDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // Now recursively copy subdirectories
            // TODO: Needs to be tested
            /*
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                FileUtils.CopyFolder(subdir.FullName, temppath);
            }
            */
        }

#endif

#if WINDOWS_PHONE || NETFX_CORE
        public async static void CopyFolder(string sourceFolderPath, string destFolderPath)
        {
            // Get source files
            var sourceRootFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var sourceFolder = await sourceRootFolder.GetFolderAsync(sourceFolderPath);
            var sourceFiles = await sourceFolder.GetFilesAsync();
            
            // Get dest folder
            var destRootFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var destFolder = await destRootFolder.GetFolderAsync(destFolderPath);

            // Copy source files to dest folder
            foreach(var sourceFile in sourceFiles)
            {
                await sourceFile.CopyAsync(destFolder);
            }

            // Now recursively copy subdirectories
            // TODO: Needs to be tested
            /*
            var sourceSubfolders = await sourceFolder.GetFoldersAsync();
            foreach (var sourceSubfolder in sourceSubfolders)
            {
                FileUtils.CopyFolder(sourceSubfolder.Path, sourceSubfolder.Path);
            }
            */
        }

#endif

#if ANDROID
        public static IList<string> GetFolderNames(string folderPath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFolderPath = Path.Combine(documentsPath, folderPath);
            var folders = System.IO.Directory.GetDirectories(fullFolderPath);
            var folderNames = new List<string>(folders);

            return folderNames;
        }
#endif

#if DESKTOP
        public static IList<string> GetFolderNames(string folderPath)
        {
            var files = new List<string>(Directory.EnumerateDirectories(folderPath));

            return files;
        }
#endif

#if IOS
        public static IList<string> GetFolderNames(string folderPath, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFolderPath = Path.Combine(rootPath, folderPath);
            var folders = System.IO.Directory.GetDirectories(fullFolderPath);
            var folderNames = new List<string>(folders);

            return folderNames;
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task<IList<string>> GetFolderNamesAsync(string folderPath)
        {
            var local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folder = await local.GetFolderAsync(folderPath);
            var folders = await folder.GetFoldersAsync();
            var folderNames = folders
                .Select(x => x.Name)
                .ToList();

            return folderNames;
        }
#endif

#if ANDROID
        public static bool FileExists(string filepath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFilepath = Path.Combine(documentsPath, filepath);
            return File.Exists(fullFilepath);
        }
#endif

#if DESKTOP
        public static bool FileExists(string filepath)
        {
            return File.Exists(filepath);
        }
#endif

#if IOS
        public static bool FileExists(string filepath, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFilepath = Path.Combine(rootPath, filepath);
            return File.Exists(fullFilepath);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task<bool> FileExistsAsync(string filepath)
        {
            // Currently only way to do this on WS 8.0, moving to 8.1 we get the api
            try
            {
                var local = Windows.Storage.ApplicationData.Current.LocalFolder;
                var file = await local.GetItemAsync(filepath);
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }
#endif

#if ANDROID
        public static void DeleteFile(string filepath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFilepath = Path.Combine(documentsPath, filepath);
            File.Delete(fullFilepath);
        }
#endif

#if DESKTOP
        public static void DeleteFile(string filepath)
        {
            File.Delete(filepath);
        }
#endif

#if IOS
        public static void DeleteFile(string filepath, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFilepath = Path.Combine(rootPath, filepath);
            File.Delete(fullFilepath);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task DeleteFileAsync(string filepath)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await local.GetFileAsync(filepath);
            await file.DeleteAsync();
        }
#endif

#if ANDROID
        public static IList<string> GetFilenames(string folderPath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFolderPath = Path.Combine(documentsPath, folderPath);
            var files = System.IO.Directory.GetFiles(fullFolderPath);
            var filenames = new List<string>(files);

            return filenames;
        }
#endif

#if DESKTOP
        public static IList<string> GetFilenames(string folderPath)
        {
            var files = new List<string>(Directory.EnumerateFiles(folderPath));

            return files;
        }
#endif

#if IOS
        public static IList<string> GetFilenames(string folderPath, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFolderPath = Path.Combine(rootPath, folderPath);
            var files = System.IO.Directory.GetFiles(fullFolderPath);
            var filenames = new List<string>(files);

            return filenames;
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public static async Task<IList<string>> GetFilenamesAsync(string folderPath)
        {
            var local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folder = await local.GetFolderAsync(folderPath);
            var files = await folder.GetFilesAsync();
            var filenames = files
                .Select(x => x.Name)
                .ToList();

            return filenames;
        }
#endif

#if ANDROID
        public static void SaveBinary(string filepath, byte[] bytes)
        {

            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFilePath = Path.Combine(documentsPath, filepath);
            System.IO.File.WriteAllBytes(fullFilePath, bytes);
        }
        
        public static void SaveText(string filepath, string text)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFilePath = Path.Combine(documentsPath, filepath);
            System.IO.File.WriteAllText(fullFilePath, text);
        }
#endif

#if DESKTOP
        public static void SaveBinary(string filepath, byte[] bytes)
        {
            // TODO: Folder structure
            System.IO.File.WriteAllBytes(filepath, bytes);
        }

        public static void SaveText(string filepath, string text)
        {
            // TODO: Folder structure
            using (StreamWriter outfile = new StreamWriter(filepath))
            {
                outfile.Write(text);
            }
        }
#endif

#if IOS
        public static void SaveBinary(string filepath, byte[] bytes, bool libraryFolder = false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFilePath = Path.Combine(rootPath, filepath);
            System.IO.File.WriteAllBytes(fullFilePath, bytes);
        }

        public static void SaveText(string filepath, string text, bool libraryFolder = false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFilePath = Path.Combine(rootPath, filepath);
            System.IO.File.WriteAllText(fullFilePath, text);
        }
#endif

#if WINDOWS_PHONE
        public static async Task SaveTextAsync(string filepath, string text)
        {
            // Get the text data from the text 
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(text.ToCharArray());

            // Get the local folder
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create a new file based on the passed in file name
            var file = await localFolder.CreateFileAsync(filepath, CreationCollisionOption.ReplaceExisting);

            // Write the data from the text
            using (var s = await file.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }
        }
#endif

#if NETFX_CORE
        public static async Task SaveTextAsync(string filepath, string text)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync(filepath, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, text);
        }
#endif

#if ANDROID
        public static byte[] LoadBinary(string filepath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFilePath = Path.Combine(documentsPath, filepath);
            return System.IO.File.ReadAllBytes(fullFilePath);
        }
        
        public static string LoadText(string filepath)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fullFilePath = Path.Combine(documentsPath, filepath);
            return System.IO.File.ReadAllText(fullFilePath);
        }            
#endif

#if DESKTOP
        public static string LoadText(string filepath)
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                var text = sr.ReadToEnd();

                return text;
            }
        }
#endif

#if IOS
        public static string LoadText(string filepath, bool libraryFolder=false)
        {
            string rootPath;
            if (libraryFolder)
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
            }
            else
            {
                rootPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
            }

            var fullFilePath = Path.Combine(rootPath, filepath);
            return System.IO.File.ReadAllText(fullFilePath);
        }
#endif

#if WINDOWS_PHONE
        public static async Task<string> LoadTextAsync(string filepath)
        {
            // Get the local folder.
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var text = string.Empty;

            // Get the file stream
            using (var stream = await localFolder.OpenStreamForReadAsync(filepath))
            {
                // Read the data
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    text = streamReader.ReadToEnd();
                }
            }

            return text;
        }
#endif

#if NETFX_CORE
        public static async Task<string> LoadTextAsync(string filepath)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.GetFileAsync(filepath);
            var text = await FileIO.ReadTextAsync(file);
                
            return text;
        }
#endif

    }
}