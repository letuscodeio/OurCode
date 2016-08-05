using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LetUsCodeIO.App_Code
{
    /// 
    /// <summary>
    /// This class provides a variety of helper functions used to access
    /// local application and data files.
    /// </summary>
    /// 
    public class LocalFileAccess
    {
        #region Helper Classes and Definitions

        /// <summary>
        /// Actions to take for file error events.
        /// </summary>
        public enum FileErrorAction
        {
            /// <summary>Cancel (abort) the operation.</summary>
            Cancel = 0,

            /// <summary>Retry the operation.</summary>
            Retry = 1,

            /// <summary>Ignore the error.</summary>
            Ignore = 2
        }

        /// <summary>
        /// Event class to describe a file access error.
        /// </summary>
        public class FileErrorEvent
        {
            /// <summary>The file being operated on.</summary>
            public FileInfo File;

            /// <summary>A filename, typically for an output file.</summary>
            public string FileName;

            /// <summary>An exception that occurred handling the file.</summary>
            public Exception Error;

            /// <summary>An error message to return to the caller.</summary>
            public string ReturnMsg;

            /// <summary>
            /// Constructor for a file error.
            /// </summary>
            /// <param name="F">A file</param>
            /// <param name="N">A file name</param>
            /// <param name="Ex">An exception</param>
            /// 
            public FileErrorEvent(FileInfo F, string N, Exception Ex)
            {
                File = F;
                FileName = N;
                Error = Ex;
                if (Error != null)
                    ReturnMsg = Error.Message;
            }
        }

        /// 
        /// <summary>
        /// Function signature for file-error handler.
        /// </summary>
        /// <param name="Ev"></param>
        /// <returns></returns>
        /// 
        public delegate FileErrorAction FileErrorHandler(FileErrorEvent Ev);

        /// <summary>Default program used to view local files.</summary>
        public static string sLocalFileViewer = "NOTEPAD.EXE";

        //  Native WIN32 function to lookup device mapping
        [DllImport("kernel32.dll")]
        private static extern UInt32 QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        /// <summary>
        /// Supported file sequencing
        /// <remarks>
        /// Supported braces do put space before openning of brace
        /// </remarks>
        /// </summary>
        public enum SequenceBracket
        {
            /// <summary>Option or not using brackets
            /// <remarks>
            /// Does not put a space before the sequence number
            /// </remarks>
            /// </summary>
            None,
            /// <summary>Option: using "[" and "]" </summary>
            Square,
            /// <summary>Option: using "(" and ")"</summary>
            Rounded,
            /// <summary>Option: using "{" and "}"</summary>
            Curly
        }

        #endregion


        #region Directory Helper Methods



        /// 
        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="DirPath">The dir path.</param>
        /// <param name="CheckRules">if set to <c>true</c> [check rules].</param>
        /// <param name="LogMsg">Pass a messaging to the Debug log during DEBUG .</param>
        /// <returns><value>True</value>if directry created</returns>
        /// <remarks>
        /// Makes sure that the direcory attriubutes are set to Normal
        /// </remarks>
        /// 
        //[System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand,Name="FullTrust")]
        public static bool CreateDirectory(string DirPath, bool CheckRules, string LogMsg)
        {
            bool Success = false;
            try
            {
#if NOTUSED
            if (CheckRules)
                {
                string ChildDir = DirPath;
                if (DirPath.EndsWith("\\"))
                    ChildDir = Path.GetDirectoryName(DirPath);
                ChildDir = Path.GetFileName(ChildDir);
                
                //
                // Make sure version is not set to "0.0.x)"
                //
                if (ChildDir.StartsWith("0.0."))
                    return false;
                }
#endif
                DirectoryInfo Info = Directory.CreateDirectory(DirPath);
                if (Success = Info.Exists)
                {
                    Info.Attributes = FileAttributes.Normal;
                    //Info.Attributes &= ~FileAttributes.ReadOnly;
                    Info.Refresh();
                }

                // 
                // Testing for windows 7+ make sure user have full control
                // BaseApplication.ConfigString().Length > 0
                // ChangeDirectoryRights(DirPath, FileSystemRights.FullControl, "", true);
                //
                if (CheckRules)
                {
                    //System.Security.Permissions.PermissionSetAttribute P = new System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand);
                    //string Node = LocalApplication.Common.NodeName;
                    string AllUsers = "Users";
                    try
                    {
                        ChangeDirectoryRights(DirPath, FileSystemRights.FullControl, AllUsers, true);
                    }
                    catch
                    {
                    }

                }
            }
            catch (Exception ex)
            {
                //LocalApplication.LogAppError(ex, null, null);
            }
            return Success;
        }

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="DirPath">The dir path.</param>
        /// <param name="Recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static bool DeleteDirectory(string DirPath, bool Recursive)
        {
            bool Result;
            try
            {
                Directory.Delete(DirPath, Recursive);
                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
                //=
            }
            return Result;
        }

        /// 
        /// <summary>
        /// Create a temporary sub folder in the system TEMP directory.
        /// </summary>
        /// <remarks>
        /// If an empty sub-folder name is supplied, the TEMP directory name
        /// is returned.
        /// </remarks>
        /// <param name="SubDir">Subdirectory name desired.</param>
        /// <param name="tempFolder">output directory name</param>
        /// <returns>true if successful, false if directory cannot be created.</returns>
        /// 
        public static bool CreateTempFolder(string SubDir, out string tempFolder)
        {
            bool ret = false;
            tempFolder = string.Empty;
            for (int i = 0; i < 2; i++)
            {
                string folder;
                // try to create the folder under TEMP or the current application folder
                folder = i == 0 ? Path.GetTempPath() : "";// BaseApplication.Common.AppOutputDirectory;
                if (!string.IsNullOrEmpty(SubDir))
                    folder = Path.Combine(folder, SubDir);
                if (Directory.Exists(folder))
                {
                    tempFolder = folder;
                    ret = true;
                    break;
                }
                else
                {
                    DirectoryInfo di;
                    try
                    {
                        di = Directory.CreateDirectory(folder);
                    }
                    catch (Exception e)
                    {
                        di = null;
                        if (i == 1)
                            ;// LocalApplication.LogAppError(e, "Error on creating the temp folder ", folder);
                    }

                    if (di != null && di.Exists)
                    {
                        tempFolder = folder;
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        ///
        /// <summary>
        /// Append a file with some data
        /// <remarks>if an error accures a rollback is done</remarks>
        /// </summary>
        /// <param name="fileName">File to write write to</param>
        /// <param name="data">data to copy</param>
        /// <returns>weather able to write or not</returns>
        ///
        public static bool SafeFileAppend(string fileName, byte[] data)
        {
            bool success = true;
            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            {
                FileStream f = new FileStream(fs.SafeFileHandle, FileAccess.ReadWrite);
                try
                {
                    foreach (byte bite in data)
                    {
                        f.WriteByte(bite);
                    }
                }
                catch
                {
                    success = false;
                    f = fs;
                }
                finally
                {
                    if (f != null) f.Dispose();
                }
            }
            return success;
        }

        ///
        /// <summary>
        /// Read an entire text file into a string.
        /// </summary>
        /// <param name="FileName">The name of the file.</param>
        /// <returns>The file data as a string, or null if the file cannot be read.</returns>
        /// 
        public static string ReadFileAsString(string FileName)
        {
            string FileData;
            try
            {
                using (StreamReader input = new StreamReader(FileName))
                {
                    FileData = input.ReadToEnd();
                }
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogSysError(Ex, "Can't read file: ", FileName);
                FileData = null;
            }
            return FileData;
        }

        /// <summary>
        /// For each line return the 
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string[] ReadFileLines(string FileName)
        {

            List<string> FileData = new List<string>();
            try
            {
                using (StreamReader input = new StreamReader(FileName))
                {
                    while (!input.EndOfStream)
                        FileData.Add(input.ReadLine());

                }
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogSysError(Ex, "Can't read file: ", FileName);

            }
            return FileData.ToArray();
        }

        ///
        /// <summary>
        /// Gets the size with all files of the directory
        /// </summary>
        /// <param name="directoryPath">Path for the directory</param>
        /// <returns>Size of direcoty </returns>
        /// 
        public static long DirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new Exception("Directory Not found");
            }

            DirectoryInfo di = new DirectoryInfo(directoryPath);
            return DirectorySize(di);
        }

        /// 
        /// <summary>
        /// Get the size of the directory
        /// </summary>
        /// <param name="dir">DirectoryInfo object</param>
        /// <returns>size in bytes</returns>
        /// 
        public static long DirectorySize(DirectoryInfo dir)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirectorySize(di);
            }
            return (Size);
        }

        ///
        /// <summary>
        /// Copy a directory, including all files and [optionally] subdirectories.
        /// </summary>
        /// <param name="InName">Input directory name.</param>
        /// <param name="OutName">Output directory name.</param>
        /// <param name="IncludeDirs">true to include subdirectories.</param>
        /// <param name="CheckAttr">true to reset attributes of existing files before copying 
        /// (allows r/o files to be overwritten).</param>
        /// <param name="PreserveTime">true to preserve the timestamp from the input file.</param>
        /// <param name="OnlyNew">True to only copy new files (no existing file).</param>
        /// <param name="KeepNewest">true to retain any newer existing file.</param>
        /// <param name="ErrorHandler">Error handler for a file copy failure.</param>
        /// <returns>error message if copy failed, otherwise null.</returns>
        /// 
        public static string CopyDirectory(string InName, string OutName,
                        bool IncludeDirs, bool CheckAttr, bool PreserveTime,
                        bool OnlyNew, bool KeepNewest, FileErrorHandler ErrorHandler)
        {
            if (!Directory.Exists(InName))
            {
                return "Directory " + InName + " does not exist!";
            }

            try
            {
                DirectoryInfo Input = new DirectoryInfo(InName);
                if (!Directory.Exists(OutName))
                    Directory.CreateDirectory(OutName);

                if (IncludeDirs)
                {
                    DirectoryInfo[] SubDirs = Input.GetDirectories();
                    foreach (DirectoryInfo DirName in SubDirs)
                    {
                        string ErrMsg = CopyDirectory(Path.Combine(InName, DirName.Name),
                                            Path.Combine(OutName, DirName.Name), true,
                                            CheckAttr, PreserveTime, false, ErrorHandler);
                        if (ErrMsg != null)
                            return ErrMsg;
                    }
                }

                FileInfo[] InFiles = Input.GetFiles();
                foreach (FileInfo InFile in InFiles)
                {
                    string FileName = Path.Combine(OutName, InFile.Name);

                    if (File.Exists(FileName))
                    {
                        if (OnlyNew)
                            continue;
                        if (KeepNewest)
                        {
                            DateTime OldFileTime = DataConverter.ReadDateToSecond(File.GetLastWriteTimeUtc(FileName));
                            DateTime NewFileTime = DataConverter.ReadDateToSecond(InFile.LastWriteTimeUtc);
                            if (OldFileTime > NewFileTime)
                                continue;
                        }
                        if (CheckAttr)
                            File.SetAttributes(FileName, FileAttributes.Normal);
                    }

                    // Make sure not Hidden
                    FileAttributes TempAtt = InFile.Attributes;
                    if ((TempAtt & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        bool Retry = true;
                        bool Copied = false;
                        while (Retry)
                        {
                            try
                            {
                                Retry = false;
                                InFile.CopyTo(FileName, true);
                                Copied = true;
                            }
                            catch (Exception ex)
                            {
                                if (ErrorHandler == null)
                                    throw;

                                FileErrorEvent Ev = new FileErrorEvent(InFile, FileName, ex);
                                FileErrorAction Action = ErrorHandler(Ev);
                                switch (Action)
                                {
                                    case FileErrorAction.Cancel:
                                        return Ev.ReturnMsg;
                                    case FileErrorAction.Retry:
                                        Retry = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        if (PreserveTime && Copied)
                        {
                            FileAttributes NewAttr = 0;
                            if (InFile.IsReadOnly)
                            {
                                NewAttr = File.GetAttributes(FileName);
                                NewAttr &= ~FileAttributes.ReadOnly;
                                File.SetAttributes(FileName, NewAttr);
                            }
                            File.SetCreationTimeUtc(FileName, InFile.CreationTimeUtc);
                            File.SetLastAccessTimeUtc(FileName, InFile.LastAccessTimeUtc);
                            File.SetLastWriteTimeUtc(FileName, InFile.LastWriteTimeUtc);
                            if (InFile.IsReadOnly)
                            {
                                NewAttr |= FileAttributes.ReadOnly;
                                File.SetAttributes(FileName, NewAttr);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        ///
        /// <summary>
        /// Copy a directory, including all files and [optionally] subdirectories.
        /// </summary>
        /// <param name="InName">Input directory name.</param>
        /// <param name="OutName">Output directory name.</param>
        /// <param name="IncludeDirs">true to include subdirectories.</param>
        /// <param name="CheckAttr">true to reset attributes of existing files before copying 
        /// (allows r/o files to be overwritten).</param>
        /// <param name="PreserveTime">true to preserve the timestamp from the input file.</param>
        /// <param name="OnlyNew">True to only copy new files (no existing file).</param>
        /// <param name="ErrorHandler">Error handler for a file copy failure.</param>
        /// <returns>error message if copy failed, otherwise null.</returns>
        /// 
        public static string CopyDirectory(string InName, string OutName,
                        bool IncludeDirs, bool CheckAttr, bool PreserveTime,
                        bool OnlyNew, FileErrorHandler ErrorHandler)
        {
            return CopyDirectory(InName, OutName, IncludeDirs, CheckAttr, PreserveTime, OnlyNew, false, ErrorHandler);
        }

        /// 
        /// <summary>
        /// Alternate form of directory-copy, always copy subdirectories.
        /// </summary>
        /// <param name="InName">Input directory name.</param>
        /// <param name="OutName">Output directory name.</param>
        /// <param name="CheckAttr">true to reset attributes of existing files before copying 
        /// (allows r/o files to be overwritten).</param>
        /// <param name="PreserveTime">true to preserve the timestamp from the input file.</param>
        /// <returns>error message if copy failed, otherwise null.</returns>
        /// 
        public static string CopyDirectory(string InName, string OutName,
                                bool CheckAttr, bool PreserveTime)
        {
            return CopyDirectory(InName, OutName, true, CheckAttr, PreserveTime, false, false, null);
        }

        /// 
        /// <summary>
        /// Alternate form of directory-copy, always copy subdirectories.
        /// </summary>
        /// <param name="InName">Input directory name.</param>
        /// <param name="OutName">Output directory name.</param>
        /// <param name="CheckAttr">true to reset attributes of existing files before copying 
        /// (allows r/o files to be overwritten).</param>
        /// <param name="PreserveTime">true to preserve the timestamp from the input file.</param>
        /// <param name="ErrorHandler">Error handler for a file copy failure.</param>
        /// <returns>error message if copy failed, otherwise null.</returns>
        /// 
        public static string CopyDirectory(string InName, string OutName,
                                bool CheckAttr, bool PreserveTime, FileErrorHandler ErrorHandler)
        {
            return CopyDirectory(InName, OutName, true, CheckAttr, PreserveTime, false, false, ErrorHandler);
        }

        /// 
        /// <summary>
        /// Alter the access rights for a directory.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="rights"></param>
        /// <param name="username"></param>
        /// <param name="inherentsubDir"></param>
        /// <returns></returns>
        /// 
        public static bool ChangeDirectoryRights(string Path, FileSystemRights rights, string username, bool inherentsubDir)
        {
            //check to see if path exists
            if (!Directory.Exists(Path))
            {
                return false;
            }

            // *** Add Access Rule to the actual directory
            FileSystemAccessRule AccessRule = new FileSystemAccessRule(username, rights,
                                                 InheritanceFlags.None,
                                                 PropagationFlags.NoPropagateInherit,
                                                 AccessControlType.Allow);
            //Get the direcory to set the info for
            DirectoryInfo Info = new DirectoryInfo(Path);
            DirectorySecurity Security = Info.GetAccessControl(AccessControlSections.Access);

            bool Result = false;
            //set the the rull
            Security.ModifyAccessRule(AccessControlModification.Set, AccessRule, out Result);
            //add a see if  worked
            if (!Result)
                return false;

            //just files
            InheritanceFlags iFlags = InheritanceFlags.ObjectInherit;
            if (inherentsubDir)
            {////check for the sub directories
                iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            }

            // *** Add Access rule for the inheritance
            AccessRule = new FileSystemAccessRule(username, rights,
                                                  iFlags, //flags for inheritance
                                                  PropagationFlags.InheritOnly, //local already set 
                                                  AccessControlType.Allow);    //make sure we allow this user


            Result = false;

            // Info.Attributes = FileAttributes.Directory | ~FileAttributes.ReadOnly;
            Security.ModifyAccessRule(AccessControlModification.Add, AccessRule, out Result);

            if (!Result)
                return false;

            Info.SetAccessControl(Security);

            return true;
        }

        #endregion


        #region File Methods

        ///
        /// <summary>
        /// Write an entire text file from a string.
        /// </summary>
        /// <param name="FileName">The name of the file.</param>
        /// <param name="FileData">The file data as a string.</param>
        /// <returns></returns>
        /// 
        public static bool WriteFileFromString(string FileName, string FileData)
        {
            bool Success = false;
            try
            {
                using (StreamWriter output = new StreamWriter(FileName))
                {
                    output.Write(FileData);
                    Success = true;
                }
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogSysError(Ex, "Can't write file: ", FileName);
            }

            return Success;
        }

        ///
        /// <summary>
        /// Read an entire file into a byte array.
        /// </summary>
        /// <param name="FileName">The name of the file.</param>
        /// <returns>The file data as a byte array, or null if the file cannot be read.</returns>
        /// 
        public static byte[] ReadFileAsBytes(string FileName)
        {
            byte[] FileData;
            try
            {
                using (FileStream input = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    BinaryReader binrd = new BinaryReader(input);
                    FileData = binrd.ReadBytes((int)input.Length);
                }
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogSysError(Ex, "Can't read file: ", FileName);
                FileData = null;
            }
            return FileData;
        }

        /// 
        /// <summary>
        /// Update the "last written" time for a file.
        /// </summary>
        /// <param name="FileName">The file name.</param>
        /// <param name="NewTime">The new time value.</param>
        /// <param name="IsUTC">true for UTC, false for Local time.</param>
        /// <returns>true if successful, else false</returns>
        /// 
        public static bool UpdateFileTime(string FileName, DateTime NewTime, bool IsUTC)
        {
            bool Success = false;
            try
            {
                if (IsUTC)
                    File.SetLastWriteTimeUtc(FileName, NewTime);
                else
                    File.SetLastWriteTime(FileName, NewTime);
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogSysError(Ex, "Can't update file: ", FileName);
            }
            return Success;
        }

        ///
        /// <summary>
        /// Write an entire text file from a byte array.
        /// </summary>
        /// <param name="FileName">The name of the file.</param>
        /// <param name="FileData">The file data as a string.</param>
        /// <returns>true if successful, else false</returns>
        /// 
        public static bool WriteFileFromBytes(string FileName, byte[] FileData)
        {
            return WriteFileFromBytes(FileName, FileData, false);
        }

        /// 
        /// <summary>
        /// Write an entire text file from a byte array.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <param name="FileData">The file data as a string.</param>
        /// <param name="SuppressError">if set to <c>true</c> [suppress error].</param>
        /// <returns>true if successful, else false</returns>
        /// 
        public static bool WriteFileFromBytes(string FileName, byte[] FileData, bool SuppressError)
        {
            bool Success = false;
            try
            {
                using (FileStream output = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    BinaryWriter binwrt = new BinaryWriter(output);
                    binwrt.Write(FileData);
                    binwrt.Close();
                    Success = true;
                }
            }
            catch (Exception Ex)
            {
                if (!SuppressError)
                    ;// LocalApplication.LogSysError(Ex, "Can't write file: ", FileName);
            }
            return Success;
        }

        /// <summary>
        /// Prepend to the beginning of the file
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FileData"></param>
        /// <returns></returns>
        public static bool WriteFilePrepend(string FileName, byte[] FileData)
        {
            bool Success = false;
            try
            {
                string TempFile = Path.GetTempFileName();
                Success = WriteFileFromBytes(FileName, FileData, false);
                if (Success)
                {
                    using (FileStream Original = new FileStream(FileName, FileMode.Open))
                    {
                        BinaryReader binReader = new BinaryReader(Original);
                        byte[] Bytes = new byte[Original.Length];
                        // TODO: Bread up into file into chunks 
                        binReader.Read(Bytes, 0, (int)Original.Length);
                        Success = WriteFileFromBytes(TempFile, FileData, false);
                        binReader.Close();
                    }
                }

                if (Success)
                {
                    // TODO : Incorporate transactions
                    //TransactionScope
                    Success = CopyFile(TempFile, TempFile, false, true);
                    DeleteFile(FileName);
                }
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogSysError(Ex, "Can't write file: ", FileName);
            }
            return Success;
        }

        /// 
        /// <summary>
        /// Will find a file in the in the search path. 
        /// <remarks>
        /// File name is not root it will search the executing directory.
        /// </remarks>
        /// </summary>
        /// <param name="FileSearchPath">The path - file name to search for the proper case.</param>
        /// <returns>
        /// Return the propper case of path.
        /// <remarks>Returns null if the path could not be found.</remarks>
        /// </returns>
        /// 
        public static string FindFile(string FileSearchPath)
        {

            string RootedPath = (!Path.IsPathRooted(FileSearchPath))
                                 ? "" // BaseApplication.Common.GetFileName(FileSearchPath)
                                 : FileSearchPath;

            string[] Files = Directory.GetFiles(Path.GetDirectoryName(FileSearchPath));
            string FilePath = RootedPath;
            foreach (string F in Files)
            {
                if (string.Compare(F, RootedPath, true) == 0)
                {
                    FilePath = F;
                    break;
                }
            }

            return File.Exists(FilePath) ? FilePath : null;
        }


        /// 
        /// <summary>
        /// Copy a file or directory path.
        /// </summary>
        /// <param name="fromFile"></param>
        /// <param name="toPath">this will support file path or a directory path </param>
        /// <param name="CheckAttr"></param>
        /// <param name="PreserveTime"></param>
        /// <returns></returns>
        /// 
        public static bool CopyFile(string fromFile, string toPath, bool CheckAttr, bool PreserveTime)
        {
            return CopyFile(fromFile, toPath, CheckAttr, PreserveTime, false);
        }

        /// 
        /// <summary>
        /// Copy a file or directory path.
        /// </summary>
        /// <param name="fromFile"></param>
        /// <param name="toPath">this will support file path or a directory path </param>
        /// <param name="CheckAttr"></param>
        /// <param name="PreserveTime"></param>
        /// <param name="KeepNewest">true to keep the newest version of an existing file.</param>
        /// <returns></returns>
        /// 
        public static bool CopyFile(string fromFile, string toPath, bool CheckAttr, bool PreserveTime, bool KeepNewest)
        {
            //  Make that there is something to copy 
            if (!File.Exists(fromFile))
            {
                throw new FileNotFoundException();
            }

            //  Get the existing information of the current file
            FileInfo InFile = new FileInfo(fromFile);

            //  Check to see if the the file is a path or a file 
            string filename = Path.GetFileName(toPath);
            if (filename == string.Empty)
            {
                //  This is just a path
                toPath = Path.Combine(toPath, InFile.FullName);
            }

            //  Check to see if path exist to copy the file to 
            string dirname = Path.GetDirectoryName(toPath);

            if (!string.IsNullOrEmpty(dirname))
            {
                if (!Directory.Exists(dirname))
                    Directory.CreateDirectory(dirname);
            }

            try
            {

                // Make sure file is visible, check timestamp if needed.
                if ((CheckAttr || KeepNewest) && File.Exists(toPath))
                {
                    if (KeepNewest)
                    {
                        DateTime OldFileTime = DataConverter.ReadDateToSecond(File.GetLastWriteTimeUtc(toPath));
                        DateTime NewFileTime = DataConverter.ReadDateToSecond(InFile.LastWriteTimeUtc);
                        if (OldFileTime > NewFileTime)
                            return true;
                    }
                    File.SetAttributes(toPath, FileAttributes.Normal);
                }

                //  Copy the file.
                InFile.CopyTo(toPath, true);
                if (PreserveTime)
                {
                    FileAttributes NewAttr = 0;
                    if (InFile.IsReadOnly)
                    {
                        NewAttr = File.GetAttributes(toPath);
                        NewAttr &= ~FileAttributes.ReadOnly;
                        File.SetAttributes(toPath, NewAttr);
                    }
                    File.SetCreationTimeUtc(toPath, InFile.CreationTimeUtc);
                    File.SetLastAccessTimeUtc(toPath, InFile.LastAccessTimeUtc);
                    File.SetLastWriteTimeUtc(toPath, InFile.LastWriteTimeUtc);
                    if (InFile.IsReadOnly)
                    {
                        NewAttr |= FileAttributes.ReadOnly;
                        File.SetAttributes(toPath, NewAttr);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// 
        /// <summary>
        /// Delete a single file.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>true if successful.</returns>
        /// 
        public static bool DeleteFile(string FileName)
        {
            try
            {
                File.Delete(FileName);
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogAppError(Ex, null, null);
                return false;
            }

            return true;
        }

        /// 
        /// <summary>
        /// Delete all files that match a given mask.
        /// </summary>
        /// <param name="BaseDir">The directory from which to delete.</param>
        /// <param name="Mask">The file mask</param>
        /// <returns>Count of deleted files, negative for file number on which an error occurred</returns>
        public static int DeleteFiles(string BaseDir, string Mask)
        {
            int nCount = 0;
            try
            {
                string[] Names = Directory.GetFiles(BaseDir, Mask);
                foreach (string FileName in Names)
                {
                    nCount++;
                    File.Delete(Path.Combine(BaseDir, FileName));
                }
            }
            catch (Exception Ex)
            {
                //LocalApplication.LogAppError(Ex, null, null);
                nCount = -nCount;
            }

            return nCount;
        }


        /// 
        /// <summary>
        /// Determine true (non-subst) path name for a path.
        /// </summary>
        /// <param name="path">The path name.</param>
        /// <returns>True path name.</returns>
        /// 
        public static string GetTruePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            //  Get the drive letter
            string pathRoot = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(pathRoot))
                return path;

            //  Query for a device mapping
            string lpDeviceName = pathRoot.Replace("\\", "");
            const string substPrefix = @"\??\";
            StringBuilder lpTargetPath = new StringBuilder(260);

            if (QueryDosDevice(lpDeviceName, lpTargetPath, lpTargetPath.Capacity) > 0)
            {
                // If drive is substed, the result will be in the format of "\??\C:\RealPath\".
                if (lpTargetPath.ToString().StartsWith(substPrefix))
                {
                    // Strip the \??\ prefix.
                    string root = lpTargetPath.ToString().Remove(0, substPrefix.Length);
                    string result = Path.Combine(root, path.Replace(Path.GetPathRoot(path), ""));
                    return result;
                }
                else
                {
                    // TODO: deal with other types of mappings.
                    // For now, if not SUBSTed, just assume it's not mapped.
                }
            }

            //  Unable to determine the path root.
            return path;
        }

        /// 
        /// <summary>
        /// Search for a file using an extended "Windows" search order.
        /// </summary>
        /// <remarks>
        /// This function searches the following directories, in this order:
        /// 1. the directory from where the executing app. is loaded
        /// 2. the current directory
        /// 3. any "hint" paths supplied
        /// 4. the \Windows\System32 directory
        /// 5. the \Windows\System directory
        /// 6. the \Windows directory
        /// 7. the directories listed in the PATH environment variable    
        /// </remarks>
        /// <param name="FileName">The name of the file to locate.</param>
        /// <param name="HintPaths">Any additional paths to search.</param>
        /// <returns>The path for this file, or null if not found.</returns>
        /// 
        public static string SearchForFile(string FileName, params string[] HintPaths)
        {
            string BaseDir = AppDomain.CurrentDomain.BaseDirectory; //BaseApplication.Common == null ? AppDomain.CurrentDomain.BaseDirectory
                                                                    //: BaseApplication.Common.AppBaseDirectory;
            if (File.Exists(Path.Combine(BaseDir, FileName)))
                return BaseDir;

            string CurrDir = Directory.GetCurrentDirectory();
            if (CurrDir != BaseDir && File.Exists(Path.Combine(CurrDir, FileName)))
                return CurrDir;

            if (HintPaths != null)
            {
                foreach (string PathName in HintPaths)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(PathName) && File.Exists(Path.Combine(PathName, FileName)))
                            return PathName;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            string SysDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (File.Exists(Path.Combine(SysDir, FileName)))
                return SysDir;

            if (SysDir.EndsWith("32"))
            {
                string CheckDir = SysDir.Substring(0, SysDir.Length - 2);
                if (File.Exists(Path.Combine(CheckDir, FileName)))
                    return CheckDir;
            }

            SysDir = Path.GetDirectoryName(SysDir);
            if (File.Exists(Path.Combine(SysDir, FileName)))
                return SysDir;

            string[] Paths = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string PathName in Paths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(PathName) && File.Exists(Path.Combine(PathName, FileName)))
                        return PathName;
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Build a unique file name for a file 
        /// </summary>
        /// <param name="BasePath">Base path to create file</param>
        /// <param name="BaseName">Base name , including replacments</param>
        /// <remarks>
        /// Base Name can contain replacement string
        /// %seq% - for sequence of file
        /// %date% - for short datetime format
        /// %date(.net date format)% - for seqcified format of string
        /// </remarks>
        /// <param name="Date">date for date replacement</param>
        /// <param name="SeqStart">The sequence number to start at</param>
        /// <param name="Bracket">Type of brackets for sequencing in file name</param>
        /// <returns><value>null if containing any invalid charectors</value>Defined file name</returns>
        public static string BuildFileName(string BasePath, string BaseName, DateTime Date, string SeqStart, SequenceBracket Bracket)
        {
            //%seq%
            //%date%
            string FileName = string.Empty;
            if (BaseName.Contains("%date") && Date != DateTime.MinValue)
            {
                // Date format for finding the building a proper date string
                string DateReplace = "%date";
                string sDate = string.Empty;
                int nAt = BaseName.IndexOf(DateReplace);
                if (BaseName[nAt + 5] == '%')
                {
                    sDate = Date.ToShortDateString();
                }
                else if (BaseName[nAt + 5] == '(')
                {
                    // Dateformat is inset in basename
                    int nCloseBrace = BaseName.IndexOf(')', nAt + 5);
                    string DateFormat = BaseName.Substring(nAt + 6, nCloseBrace - (nAt + 6));
                    sDate = Date.ToString(DateFormat);
                    DateReplace += "(" + DateFormat + ")";
                }
                else
                {
                    // Date format not supported
                }
                DateReplace += "%";
                BaseName = BaseName.Replace(DateReplace, sDate);
            }

            if (BaseName.Contains("%seq%"))
            {
                string TempName = BaseName.Replace("%seq%", SeqStart);
                string FullName = Path.Combine(BasePath, TempName.Trim());
                if (File.Exists(FullName))
                {
                    for (int i = 2; ; i++)
                    {
                        string SeqReplace = string.Empty;
                        switch (Bracket)
                        {
                            case SequenceBracket.Square:
                                SeqReplace = " [" + i.ToString() + "]";
                                break;
                            case SequenceBracket.Rounded:
                                SeqReplace = " (" + i.ToString() + ")";
                                break;
                            case SequenceBracket.Curly:
                                SeqReplace = " {" + i.ToString() + "}";
                                break;
                            case SequenceBracket.None:
                            default:
                                SeqReplace = i.ToString();
                                break;
                        }
                        TempName = BaseName.Replace("%seq%", SeqReplace);
                        FullName = Path.Combine(BasePath, TempName.Trim());
                        if (!File.Exists(FullName))
                            break;
                    }
                }
                //else if(TempName.EndsWith("_"))
                //    TempName = TempName.Substring(0, TempName.Length - 1);
                FileName = TempName;
            }
            else
            {

            }
            //

            return FileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? null : FileName;
        }



        /// <summary>
        /// View a file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool ViewFile(string filename)
        {
            try
            {
                Process.Start(sLocalFileViewer, filename);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// 
        /// <summary>
        /// Checks if the Files the is locked.
        /// </summary>
        /// <param name="strFullFileName">Name of the STRing full file.</param>
        /// <returns><value>true</value>when file is locked</returns>
        /// 
        public static bool FileIsLocked(string strFullFileName)
        {
            bool blnReturn = false;
            System.IO.FileStream fs = null;
            try
            {
                fs = System.IO.File.Open(strFullFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read, System.IO.FileShare.None);
                fs.Close();
            }
            catch (System.IO.IOException)
            {
                blnReturn = true;
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
            return blnReturn;
        }

        /// 
        /// <summary>Gets a unique file name.</summary>
        /// <param name="FileName">
        /// A file name including enough path information to determine its parent
        /// directory.
        /// </param>
        /// <returns>
        /// File name, including path, with the file name portion modified as needed to
        /// be unique within its ultimate parent directory.
        /// </returns>
        /// 
        public static string GetUniqueFileName(string FileName)
        {
            string result = FileName;

            string dir = Path.GetDirectoryName(FileName);
            string baseName = Path.GetFileNameWithoutExtension(FileName);
            string ext = Path.GetExtension(FileName);

            int next = 1;
            while (File.Exists(result))
            {
                result = Path.Combine(dir, string.Format("{0} ({1}){2}", baseName,
                    next.ToString(), ext));
            }

            return result;
        }
        #endregion

    }
}
