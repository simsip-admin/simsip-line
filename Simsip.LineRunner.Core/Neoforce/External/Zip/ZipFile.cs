using System;
using System.Diagnostics;
#if NETFX_CORE
using System.IO;
using Simsip.LineRunner.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#endif


namespace TomShane.Neoforce.External.Zip
{

    internal class ZipFile : System.Collections.Generic.IEnumerable<ZipEntry>,
      IDisposable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
        }



      // when this is set, we trim the volume (eg C:) off any fully-qualified pathname, 
      // before writing the ZipEntry into the ZipFile. 
      // We default this to true.  This allows Windows Explorer to read the zip archives properly. 
      private bool _TrimVolumeFromFullyQualifiedPaths= true;
      public bool TrimVolumeFromFullyQualifiedPaths
      {
            get { return _TrimVolumeFromFullyQualifiedPaths; }
            set { _TrimVolumeFromFullyQualifiedPaths= value; }
      }

#if NETFX_CORE
      private async Task<Stream> ReadStream()
      {
              if (_readstream == null)
              {
                  StorageFolder zipFolder = ApplicationData.Current.LocalFolder;
                  StorageFile zipFile = await zipFolder.CreateFileAsync(_name, CreationCollisionOption.ReplaceExisting);
                  IRandomAccessStream randomAccessStream = await zipFile.OpenAsync(FileAccessMode.Read);
                  
                  IInputStream inputStream = randomAccessStream.GetInputStreamAt(0);
                  _readstream = inputStream.AsStreamForRead();
              }
              return _readstream;
      }
#else
        private System.IO.Stream ReadStream
        {
            get
            {
                if (_readstream == null)
                {
                    _readstream = System.IO.File.OpenRead(_name);
                }
                return _readstream;
            }
        }
#endif

#if NETFX_CORE
      private async Task<Stream> WriteStream()
      {
          if (_writestream == null)
          {
              StorageFolder zipFolder = ApplicationData.Current.LocalFolder;
              StorageFile zipFile = await zipFolder.CreateFileAsync(_name, CreationCollisionOption.ReplaceExisting);
              IRandomAccessStream randomAccessStream = await zipFile.OpenAsync(FileAccessMode.Read);

              IOutputStream outputStream = randomAccessStream.GetOutputStreamAt(0);
              _writestream = outputStream.AsStreamForWrite();
          }
          return _writestream;
      }

#else
        private System.IO.FileStream WriteStream
        {
            get
            {
                if (_writestream == null)
                {
                    _writestream = new System.IO.FileStream(_name, System.IO.FileMode.CreateNew);
                }
                return _writestream;
            }
        }
#endif

        private ZipFile() { }


        #region For Writing Zip Files

        public ZipFile(string NewZipFileName)
        {
            // create a new zipfile
            _name = NewZipFileName;
#if !NETFX_CORE
            if (System.IO.File.Exists(_name))
                throw new System.Exception(String.Format("That file ({0}) already exists.", NewZipFileName));
#endif
            _entries = new System.Collections.Generic.List<ZipEntry>();
        }


        public void AddItem(string FileOrDirectoryName)
        {
            AddItem(FileOrDirectoryName, false);
        }

#if NETFX_CORE
        public async void AddItem(string FileOrDirectoryName, bool WantVerbose)
        {
            if (await FileUtils.FileExistsAsync(FileOrDirectoryName))
                AddFile(FileOrDirectoryName, WantVerbose);
            else if (await FileUtils.FolderExistsAsync(FileOrDirectoryName))
                AddDirectory(FileOrDirectoryName, WantVerbose);

            else
                throw new Exception(String.Format("That file or directory ({0}) does not exist!", FileOrDirectoryName));
        }
#else
        public void AddItem(string FileOrDirectoryName, bool WantVerbose)
        {
            if (System.IO.File.Exists(FileOrDirectoryName))
                AddFile(FileOrDirectoryName, WantVerbose);
            else if (System.IO.Directory.Exists(FileOrDirectoryName))
                AddDirectory(FileOrDirectoryName, WantVerbose);

            else
                throw new Exception(String.Format("That file or directory ({0}) does not exist!", FileOrDirectoryName));
        }
#endif

        public void AddFile(string FileName)
        {
            AddFile(FileName, false);
        }

#if NETFX_CORE
        public async void AddFile(string FileName, bool WantVerbose)
        {
            ZipEntry ze = await ZipEntry.Create(FileName);
            ze.TrimVolumeFromFullyQualifiedPaths= TrimVolumeFromFullyQualifiedPaths;
            if (WantVerbose)
            {
                Debug.WriteLine("adding {0}...", FileName);
            }
			ze.Write(await WriteStream());

            _entries.Add(ze);
        }
#else
        public void AddFile(string FileName, bool WantVerbose)
        {
            ZipEntry ze = ZipEntry.Create(FileName);
            ze.TrimVolumeFromFullyQualifiedPaths = TrimVolumeFromFullyQualifiedPaths;
            if (WantVerbose)
            {
                Console.WriteLine("adding {0}...", FileName);
            }
            ze.Write(WriteStream);

            _entries.Add(ze);
        }
#endif

        public void AddDirectory(string DirectoryName)
        {
            AddDirectory(DirectoryName, false);
        }

#if NETFX_CORE
        public async void AddDirectory(string DirectoryName, bool WantVerbose)
        {
            IList<string> filenames = await FileUtils.GetFilenamesAsync(DirectoryName);
            foreach (String filename in filenames)
            {
                if (WantVerbose) Debug.WriteLine("adding {0}...", filename);
                AddFile(filename);
            }
        }
#else
        public void AddDirectory(string DirectoryName, bool WantVerbose)
        {
            String[] filenames = System.IO.Directory.GetFiles(DirectoryName);
            foreach (String filename in filenames)
            {
                if (WantVerbose) Console.WriteLine("adding {0}...", filename);
                AddFile(filename);
            }
        }
#endif

        public void Save()
        {
            WriteCentralDirectoryStructure();
#if NETFX_CORE
            if (_writestream != null)
            {
                _writestream.Dispose();
            }
#else
            WriteStream.Close();
#endif
            _writestream = null;
        }


#if NETFX_CORE
        private async void WriteCentralDirectoryStructure()
        {
            // the central directory structure
            var writeStream = await WriteStream();

            long Start = (long)writeStream.Position;
            foreach (ZipEntry e in _entries)
            {
                e.WriteCentralDirectoryEntry(writeStream);
            }
            long Finish = (long)writeStream.Position;

            // now, the footer
            WriteCentralDirectoryFooter(Start, Finish);
        }
#else
        private void WriteCentralDirectoryStructure()
        {
            // the central directory structure
            long Start = WriteStream.Length;
            foreach (ZipEntry e in _entries)
            {
                e.WriteCentralDirectoryEntry(WriteStream);
            }
            long Finish = WriteStream.Length;

            // now, the footer
            WriteCentralDirectoryFooter(Start, Finish);
        }
#endif

#if NETFX_CORE
        private async void WriteCentralDirectoryFooter(long StartOfCentralDirectory, long EndOfCentralDirectory)
#else
        private void WriteCentralDirectoryFooter(long StartOfCentralDirectory, long EndOfCentralDirectory)
#endif
        {
            byte[] bytes = new byte[1024];
            int i = 0;
            // signature
            UInt32 EndOfCentralDirectorySignature = 0x06054b50;
            bytes[i++] = (byte)(EndOfCentralDirectorySignature & 0x000000FF);
            bytes[i++] = (byte)((EndOfCentralDirectorySignature & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((EndOfCentralDirectorySignature & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((EndOfCentralDirectorySignature & 0xFF000000) >> 24);

            // number of this disk
            bytes[i++] = 0;
            bytes[i++] = 0;

            // number of the disk with the start of the central directory
            bytes[i++] = 0;
            bytes[i++] = 0;

            // total number of entries in the central dir on this disk
            bytes[i++] = (byte)(_entries.Count & 0x00FF);
            bytes[i++] = (byte)((_entries.Count & 0xFF00) >> 8);

            // total number of entries in the central directory
            bytes[i++] = (byte)(_entries.Count & 0x00FF);
            bytes[i++] = (byte)((_entries.Count & 0xFF00) >> 8);

            // size of the central directory
            Int32 SizeOfCentralDirectory = (Int32)(EndOfCentralDirectory - StartOfCentralDirectory);
            bytes[i++] = (byte)(SizeOfCentralDirectory & 0x000000FF);
            bytes[i++] = (byte)((SizeOfCentralDirectory & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((SizeOfCentralDirectory & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((SizeOfCentralDirectory & 0xFF000000) >> 24);

            // offset of the start of the central directory 
            Int32 StartOffset = (Int32)StartOfCentralDirectory;  // cast down from Long
            bytes[i++] = (byte)(StartOffset & 0x000000FF);
            bytes[i++] = (byte)((StartOffset & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((StartOffset & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((StartOffset & 0xFF000000) >> 24);

            // zip comment length
            bytes[i++] = 0;
            bytes[i++] = 0;

#if NETFX_CORE
            var writeStream = await WriteStream();
            writeStream.Write(bytes, 0, i);
#else
            WriteStream.Write(bytes, 0, i);
#endif
        }

        #endregion

        #region For Reading Zip Files

#if NETFX_CORE
        internal async static Task<ZipFile> Read(string zipfilename)
        {
            return await Read(zipfilename, false);
        }
#else
        internal static ZipFile Read(string zipfilename)
        {
            return Read(zipfilename, false);
        }
#endif

#if NETFX_CORE
        internal async static Task<ZipFile> Read(string zipfilename, bool TurnOnDebug)
#else
        internal static ZipFile Read(string zipfilename, bool TurnOnDebug)
#endif
        {

            ZipFile zf = new ZipFile();
            zf._Debug = TurnOnDebug;
            zf._name = zipfilename;
            zf._entries = new System.Collections.Generic.List<ZipEntry>();
            ZipEntry e;
#if NETFX_CORE
            while ((e = ZipEntry.Read(await zf.ReadStream(), zf._Debug)) != null)
#else
            while ((e = ZipEntry.Read(zf.ReadStream, zf._Debug)) != null)
#endif
            {
                if (zf._Debug)
                {
                    Debug.WriteLine("  ZipFile::Read(): ZipEntry: {0}", e.FileName);
                }
                zf._entries.Add(e);
            }

            // read the zipfile's central directory structure here.
            zf._direntries = new System.Collections.Generic.List<ZipDirEntry>();

            ZipDirEntry de;
#if NETFX_CORE
            while ((de = ZipDirEntry.Read(await zf.ReadStream(), zf._Debug)) != null)
#else
            while ((de = ZipDirEntry.Read(zf.ReadStream, zf._Debug)) != null)
#endif
            {
                if (zf._Debug)
                {
                    Debug.WriteLine("  ZipFile::Read(): ZipDirEntry: {0}", de.FileName);
                }
                zf._direntries.Add(de);
            }

            return zf;
        }

        public System.Collections.Generic.IEnumerator<ZipEntry> GetEnumerator()
        {
            foreach (ZipEntry e in _entries)
                yield return e;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void ExtractAll(string path)
        {
            ExtractAll(path, false);
        }


        public void ExtractAll(string path, bool WantVerbose)
        {
            bool header = WantVerbose;
            foreach (ZipEntry e in _entries)
            {
                if (header)
                {
                    Debug.WriteLine("\n{1,-22} {2,-6} {3,4}   {4,-8}  {0}",
                                 "Name", "Modified", "Size", "Ratio", "Packed");
                    Debug.WriteLine(new System.String('-', 72));
                    header = false;
                }
                if (WantVerbose)
                {
                    Debug.WriteLine("{1,-22} {2,-6} {3,4:F0}%   {4,-8} {0}",
                                 e.FileName,
                                 e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                 e.UncompressedSize,
                                 e.CompressionRatio,
                                 e.CompressedSize);
                }
                e.Extract(path);
            }
        }


        public void Extract(string filename)
        {
            this[filename].Extract();
        }


        public void Extract(string filename, System.IO.Stream s)
        {
            this[filename].Extract(s);
        }


        public ZipEntry this[String filename]
        {
            get
            {
                foreach (ZipEntry e in _entries)
                {
                    if (e.FileName == filename) return e;
                }
                return null;
            }
        }

        #endregion

        // the destructor
        ~ZipFile()
        {
            // call Dispose with false.  Since we're in the
            // destructor call, the managed resources will be
            // disposed of anyways.
            Dispose(false);
        }

        public void Dispose()
        {
            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (!this._disposed)
            {
                if (disposeManagedResources)
                {
                    // dispose managed resources
                    if (_readstream != null)
                    {
                        _readstream.Dispose();
                        _readstream = null;
                    }
                    if (_writestream != null)
                    {
                        _writestream.Dispose();
                        _writestream = null;
                    }
                }
                this._disposed = true;
            }
        }


#if NETFX_CORE
        private System.IO.Stream _readstream;
        private System.IO.Stream _writestream;
#else
        private System.IO.Stream _readstream;
        private System.IO.FileStream _writestream;
#endif
        private bool _Debug = false;
        private bool _disposed = false;
        private System.Collections.Generic.List<ZipEntry> _entries = null;
        private System.Collections.Generic.List<ZipDirEntry> _direntries = null;
    }

}

#region More Info
// Example usage: 
// 1. Extracting all files from a Zip file: 
//
//     try 
//     {
//       using(ZipFile zip= ZipFile.Read(ZipFile))
//       {
//         zip.ExtractAll(TargetDirectory, true);
//       }
//     }
//     catch (System.Exception ex1)
//     {
//       System.Console.Error.WriteLine("exception: " + ex1);
//     }
//
// 2. Extracting files from a zip individually:
//
//     try 
//     {
//       using(ZipFile zip= ZipFile.Read(ZipFile)) 
//       {
//         foreach (ZipEntry e in zip) 
//         {
//           e.Extract(TargetDirectory);
//         }
//       }
//     }
//     catch (System.Exception ex1)
//     {
//       System.Console.Error.WriteLine("exception: " + ex1);
//     }
//
// 3. Creating a zip archive: 
//
//     try 
//     {
//       using(ZipFile zip= new ZipFile(NewZipFile)) 
//       {
//
//         String[] filenames= System.IO.Directory.GetFiles(Directory); 
//         foreach (String filename in filenames) 
//         {
//           zip.Add(filename);
//         }
//
//         zip.Save(); 
//       }
//
//     }
//     catch (System.Exception ex1)
//     {
//       System.Console.Error.WriteLine("exception: " + ex1);
//     }
//
//
// ==================================================================
//
//
//
// Information on the ZIP format:
//
// From
// http://www.pkware.com/business_and_developers/developer/popups/appnote.txt
//
//  Overall .ZIP file format:
//
//     [local file header 1]
//     [file data 1]
//     [data descriptor 1]  ** sometimes
//     . 
//     .
//     .
//     [local file header n]
//     [file data n]
//     [data descriptor n]   ** sometimes
//     [archive decryption header] 
//     [archive extra data record] 
//     [central directory]
//     [zip64 end of central directory record]
//     [zip64 end of central directory locator] 
//     [end of central directory record]
//
// Local File Header format:
//         local file header signature     4 bytes  (0x04034b50)
//         version needed to extract       2 bytes
//         general purpose bit flag        2 bytes
//         compression method              2 bytes
//         last mod file time              2 bytes
//         last mod file date              2 bytes
//         crc-32                          4 bytes
//         compressed size                 4 bytes
//         uncompressed size               4 bytes
//         file name length                2 bytes
//         extra field length              2 bytes
//         file name                       varies
//         extra field                     varies
//
//
// Data descriptor:  (used only when bit 3 of the general purpose bitfield is set)
//         local file header signature     4 bytes  (0x08074b50)
//         crc-32                          4 bytes
//         compressed size                 4 bytes
//         uncompressed size               4 bytes
//
//
//   Central directory structure:
//
//       [file header 1]
//       .
//       .
//       . 
//       [file header n]
//       [digital signature] 
//
//
//       File header:  (This is ZipDirEntry in the code above)
//         central file header signature   4 bytes  (0x02014b50)
//         version made by                 2 bytes
//         version needed to extract       2 bytes
//         general purpose bit flag        2 bytes
//         compression method              2 bytes
//         last mod file time              2 bytes
//         last mod file date              2 bytes
//         crc-32                          4 bytes
//         compressed size                 4 bytes
//         uncompressed size               4 bytes
//         file name length                2 bytes
//         extra field length              2 bytes
//         file comment length             2 bytes
//         disk number start               2 bytes
//         internal file attributes        2 bytes
//         external file attributes        4 bytes
//         relative offset of local header 4 bytes
//         file name (variable size)
//         extra field (variable size)
//         file comment (variable size)
//
// End of central directory record:
//
//         end of central dir signature    4 bytes  (0x06054b50)
//         number of this disk             2 bytes
//         number of the disk with the
//         start of the central directory  2 bytes
//         total number of entries in the
//         central directory on this disk  2 bytes
//         total number of entries in
//         the central directory           2 bytes
//         size of the central directory   4 bytes
//         offset of start of central
//         directory with respect to
//         the starting disk number        4 bytes
//         .ZIP file comment length        2 bytes
//         .ZIP file comment       (variable size)
//
// date and time are packed values, as MSDOS did them
// time: bits 0-4 : second
//            5-10: minute
//            11-15: hour
// date  bits 0-4 : day
//            5-8: month
//            9-15 year (since 1980)
//
// see http://www.vsft.com/hal/dostime.htm

#endregion