using NonsensicalKit.Core.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 文件操作工具类
    /// </summary>
    public static class FileTool
    {

        public static void WriteVector2(this BinaryWriter writer, Vector2 v2)
        {
            writer.Write(v2.x);
            writer.Write(v2.y);
        }

        public static void WriteVector3(this BinaryWriter writer, Vector3 v3)
        {
            writer.Write(v3.x);
            writer.Write(v3.y);
            writer.Write(v3.z);
        }

        public static void WriteColor(this BinaryWriter writer, Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Create(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Close();
        }

        public static string DirSelector()
        {
            string directoryPath = "null";
            try
            {
                IntPtr pidlRet = IntPtr.Zero;
                int publicOptions = (int)Win32API.BffStyles.RestrictToFilesystem |
                (int)Win32API.BffStyles.RestrictToDomain;
                int privateOptions = (int)Win32API.BffStyles.NewDialogStyle;

                // Construct a BROWSEINFO.
                Win32API.BROWSEINFO bi = new Win32API.BROWSEINFO();
                IntPtr buffer = Marshal.AllocHGlobal(1024);
                int mergedOptions = (int)publicOptions | (int)privateOptions;
                bi.pidlRoot = IntPtr.Zero;
                bi.pszDisplayName = buffer;
                bi.lpszTitle = "文件夹";
                bi.ulFlags = mergedOptions;

                Win32Instance w = new Win32Instance();
                bool bSuccess = false;
                IntPtr P = w.GetHandle(ref bSuccess);
                if (true == bSuccess)
                {
                    bi.hwndOwner = P;
                }

                pidlRet = Win32API.Shell32.SHBrowseForFolder(ref bi);
                Marshal.FreeHGlobal(buffer);

                if (pidlRet == IntPtr.Zero)
                {
                    // User clicked Cancel.
                    return null;
                }

                byte[] pp = new byte[2048];
                if (0 == Win32API.Shell32.SHGetPathFromIDList(pidlRet, pp))
                {
                    return null;
                }

                int nSize = 0;
                for (int i = 0; i < 2048; i++)
                {
                    if (0 != pp[i])
                    {
                        nSize++;
                    }
                    else
                    {
                        break;
                    }

                }

                if (0 == nSize)
                {
                    return null;
                }

                byte[] pReal = new byte[nSize];
                Array.Copy(pp, pReal, nSize);
                // 关键转码部分
                Encoding gbk = Encoding.GetEncoding("gb2312");
                Encoding utf8 = Encoding.UTF8;
                byte[] utf8Bytes = Encoding.Convert(gbk, utf8, pReal);
                string utf8String = utf8.GetString(utf8Bytes);
                utf8String = utf8String.Replace("\0", "");
                directoryPath = utf8String.Replace("\\", "/") + "/";

            }
            catch (Exception e)
            {
                Debug.Log("获取文件夹目录出错:" + e.Message);
            }

            return directoryPath;
        }
        public static string FileSelectorWithMultiFilter(params string[][] filter)
        {
            var openFileName = new Win32API.OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            if (filter == null || filter.Length == 0)
            {
                openFileName.filter = "所有文件(*.*)\0*.*\0";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder typeStr1 = new StringBuilder();
                StringBuilder typeStr2 = new StringBuilder();
                for (int i = 0; i < filter.Length; i++)
                {
                    if (filter.Length < 2)
                    {
                        sb.Append("所有文件(*.*)\0*.*\0");
                    }

                    sb.Append(filter[i][0]);
                    typeStr1.Clear();
                    typeStr2.Clear();

                    typeStr1.Append("*.");
                    typeStr1.Append(filter[i][1]);
                    typeStr2.Append("*.");
                    typeStr2.Append(filter[i][1]);

                    for (int j = 2; j < filter[i].Length; j++)
                    {
                        typeStr1.Append(",*.");
                        typeStr1.Append(filter[i][j]);
                        typeStr2.Append(";*.");
                        typeStr2.Append(filter[i][j]);
                    }

                    sb.Append("(");
                    sb.Append(typeStr1.ToString());
                    sb.Append(")\0");
                    sb.Append(typeStr2.ToString());
                    sb.Append("\0");
                }
                openFileName.filter = sb.ToString();
            }
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.dataPath.Replace('/', '\\');//默认路径
            openFileName.title = "选择文件";
            openFileName.flags = 0x00000004 | 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            // Create buffer for file names
            string fileNames = new String(new char[2048]);
            openFileName.file = Marshal.StringToBSTR(fileNames);
            openFileName.maxFile = fileNames.Length;
            openFileName.dlgOwner = Win32API.GetForegroundWindow();


            if (Win32API.Comdlg32.GetOpenFileName(openFileName))
            {
                return Marshal.PtrToStringAuto(openFileName.file);
            }
            else
            {
                return null;
            }
        }

        public static string FileSelector(string typeName = null, params string[] filter)
        {
            var openFileName = new Win32API.OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            if (typeName == null || filter == null || filter.Length == 0)
            {
                openFileName.filter = "所有文件(*.*)\0*.*\0";
            }
            else
            {
                string typeStr1 = string.Empty;
                string typeStr2 = string.Empty;

                typeStr1 = "*." + filter[0];
                typeStr2 = "*." + filter[0];

                for (int i = 1; i < filter.Length; i++)
                {
                    typeStr1 += "," + "*." + filter[i];
                    typeStr2 += ";" + "*." + filter[i];
                }

                openFileName.filter = $"{typeName}({typeStr1})\0{typeStr2}\0";
            }
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.dataPath.Replace('/', '\\');//默认路径
            openFileName.title = "选择文件";
            openFileName.flags = 0x00000004 | 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            openFileName.dlgOwner = Win32API.GetForegroundWindow();

            // Create buffer for file names
            string fileNames = new String(new char[2048]);
            openFileName.file = Marshal.StringToBSTR(fileNames);
            openFileName.maxFile = fileNames.Length;
            openFileName.dlgOwner = Win32API.GetForegroundWindow();

            if (Win32API.Comdlg32.GetOpenFileName(openFileName))
            {
                return Marshal.PtrToStringAuto(openFileName.file);
            }
            else
            {
                return null;
            }
        }

        public static List<string> FilesSelector(string typeName = null, params string[] filter)
        {
            List<string> fileFullNames = new List<string>();

            var openFileName = new Win32API.OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            if (filter == null || filter.Length == 0)
            {
                openFileName.filter = "所有文件(*.*)\0*.*\0";
            }
            else
            {
                string typeStr1 = string.Empty;
                string typeStr2 = string.Empty;

                typeStr1 = "*." + filter[0];
                typeStr2 = "*." + filter[0];

                for (int i = 1; i < filter.Length; i++)
                {
                    typeStr1 += "," + "*." + filter[i];
                    typeStr2 += ";" + "*." + filter[i];
                }

                openFileName.filter = $"{typeName}({typeStr1})\0{typeStr2}\0";
            }
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.dataPath.Replace('/', '\\');//默认路径
            openFileName.title = "选择文件";
            openFileName.flags = 0x00000004 | 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008 | 0x00000200;
            openFileName.dlgOwner = Win32API.GetForegroundWindow();

            // Create buffer for file names
            string fileNames = new String(new char[2048]);
            openFileName.file = Marshal.StringToBSTR(fileNames);
            openFileName.maxFile = fileNames.Length;

            if (Win32API.Comdlg32.GetOpenFileName(openFileName))
            {
                List<string> selectedFilesList = new List<string>();

                long pointer = (long)openFileName.file;
                string file = Marshal.PtrToStringAuto(openFileName.file);

                while (file.Length > 0)
                {
                    selectedFilesList.Add(file);

                    pointer += file.Length * 2 + 2;
                    openFileName.file = (IntPtr)pointer;
                    file = Marshal.PtrToStringAuto(openFileName.file);
                }

                if (selectedFilesList.Count == 1)
                {
                    fileFullNames = selectedFilesList;
                }
                else
                {
                    string[] selectedFiles = new string[selectedFilesList.Count - 1];

                    for (int i = 0; i < selectedFiles.Length; i++)
                    {
                        selectedFiles[i] = selectedFilesList[0] + "\\" + selectedFilesList[i + 1];
                    }
                    fileFullNames = new List<string>(selectedFiles);
                }
            }

            if (fileFullNames.Count > 0)
            {
                return fileFullNames;
            }
            else
            {
                return null;
            }
        }

        public static string FileSaveSelector(string typeName, params string[] filter)
        {
            var openFileName = new Win32API.OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            if (filter == null || filter.Length == 0)
            {
                openFileName.filter = "所有文件(*.*)\0*.*\0";
            }
            else
            {
                string typeStr1 = string.Empty;
                string typeStr2 = string.Empty;

                typeStr1 = "*." + filter[0];
                typeStr2 = "*." + filter[0];

                for (int i = 1; i < filter.Length; i++)
                {
                    typeStr1 += "," + "*." + filter[i];
                    typeStr2 += ";" + "*." + filter[i];
                }

                openFileName.filter = $"{typeName}({typeStr1})\0{typeStr2}\0";
            }
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.dataPath.Replace('/', '\\');//默认路径
            openFileName.title = "保存项目";
            openFileName.defExt = "dat";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

            // Create buffer for file names
            string fileNames = new String(new char[2048]);
            openFileName.file = Marshal.StringToBSTR(fileNames);
            openFileName.maxFile = fileNames.Length;
            openFileName.dlgOwner = Win32API.GetForegroundWindow();

            if (Win32API.Comdlg32.GetSaveFileName(openFileName))
            {
                return Marshal.PtrToStringAuto(openFileName.file);
            }
            else
            {
                return null;
            }
        }

        public static void FileSave(byte[] data, string fullpath)
        {
            string dirpath = StringTool.GetDirpathByPath(fullpath);

            EnsureDir(dirpath);

            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public static bool TransferData(string filepath1, string filepath2)
        {
            if (!System.IO.File.Exists(filepath1) || !System.IO.File.Exists(filepath1))
            {
                return false;
            }

            try
            {
                using (var f1 = new FileStream(filepath1,FileMode.Open))
                using (var f2 = new FileStream(filepath2,FileMode.OpenOrCreate))
                {
                    byte[] buffer=new byte[1024];
                    int realRead;
                    while ((realRead= f1.Read(buffer,0,1024))>0)
                    {
                        f2.Write(buffer,0,realRead);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 创建或读取一个txt文件并往其中写入文本(覆盖)
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="text">写入的文本</param>
        public static void WriteTxt(string path, string text)
        {
            string dirpath = StringTool.GetDirpathByPath(path);
            EnsureDir(dirpath);
            File.WriteAllText(path, text);
        }

        public static void AutoWriteTxt(string text)
        {
            EnsureDir(Application.streamingAssetsPath);
            string path = Path.Combine(Application.streamingAssetsPath, StringTool.GetDateTimeString() + ".txt");
            File.WriteAllText(path, text);
            Debug.Log("文件已写入：" + path);
        }

        public static bool FileAppendWrite(string fullpath, string text)
        {
            return FileAppendWrite(Path.GetDirectoryName(fullpath), Path.GetFileName(fullpath), text);
        }

        public static bool FileAppendWrite(string path, string name, string text)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string pathStr = Path.Combine(path, name);
            try
            {
                using (FileStream fs = new FileStream(pathStr, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {
                        sw.Write(text);
                        sw.Flush();
                        sw.Close();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                LogCore.Warning("文件写入错误");
                return false;
            }
        }

        /// <summary>
        /// 获取当前工作目录的完全限定路径
        /// </summary>
        /// <returns>当前工作目录的完全限定路径</returns>
        public static string GetCurrentPath()
        {
            string path = null;
            if (System.Environment.CurrentDirectory == AppDomain.CurrentDomain.BaseDirectory)//Windows应用程序则相等  
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "Bin\\";
            }
            return path;
        }

        /// <summary>
        /// 获取文件内容字符串
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileString(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            using (StreamReader file = File.OpenText(path))
            {
                string fileContent = file.ReadToEnd();
                return fileContent;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 确保文件夹路径存在
        /// </summary>
        /// <param name="dirPath"></param>
        public static void EnsureFileDir(string filePath)
        {
            var dirPath=Path.GetDirectoryName(filePath);
            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// 确保文件夹路径存在
        /// </summary>
        /// <param name="dirPath"></param>
        public static void EnsureDir(string dirPath)
        {
            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public static string ReadAllText(string fullPath)
        {
            if (File.Exists(fullPath) == false)
            {
                return null;
            }
            string data = File.ReadAllText(fullPath);
            return data;
        }

        class Win32API
        {
            // C# representation of the IMalloc interface.
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
               Guid("00000002-0000-0000-C000-000000000046")]
            public interface IMalloc
            {
                [PreserveSig]
                IntPtr Alloc([In] int cb);
                [PreserveSig]
                IntPtr Realloc([In] IntPtr pv, [In] int cb);
                [PreserveSig]
                void Free([In] IntPtr pv);
                [PreserveSig]
                int GetSize([In] IntPtr pv);
                [PreserveSig]
                int DidAlloc(IntPtr pv);
                [PreserveSig]
                void HeapMinimize();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            public struct BROWSEINFO
            {
                public IntPtr hwndOwner;
                public IntPtr pidlRoot;
                public IntPtr pszDisplayName;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpszTitle;
                public int ulFlags;
                [MarshalAs(UnmanagedType.FunctionPtr)]
                public Shell32.BFFCALLBACK lpfn;
                public IntPtr lParam;
                public int iImage;
            }

            [Flags]
            public enum BffStyles
            {
                RestrictToFilesystem = 0x0001, // BIF_RETURNONLYFSDIRS
                RestrictToDomain = 0x0002, // BIF_DONTGOBELOWDOMAIN
                RestrictToSubfolders = 0x0008, // BIF_RETURNFSANCESTORS
                ShowTextBox = 0x0010, // BIF_EDITBOX
                ValidateSelection = 0x0020, // BIF_VALIDATE
                NewDialogStyle = 0x0040, // BIF_NEWDIALOGSTYLE
                BrowseForComputer = 0x1000, // BIF_BROWSEFORCOMPUTER
                BrowseForPrinter = 0x2000, // BIF_BROWSEFORPRINTER
                BrowseForEverything = 0x4000, // BIF_BROWSEINCLUDEFILES
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class OpenFileName
            {
                public int structSize = 0;
                public IntPtr dlgOwner = IntPtr.Zero;
                public IntPtr instance = IntPtr.Zero;
                public String filter = null;
                public String customFilter = null;
                public int maxCustFilter = 0;
                public int filterIndex = 0;
                public IntPtr file;
                public int maxFile = 0;
                public String fileTitle = null;
                public int maxFileTitle = 0;
                public String initialDir = null;
                public String title = null;
                public int flags = 0;
                public short fileOffset = 0;
                public short fileExtension = 0;
                public String defExt = null;
                public IntPtr custData = IntPtr.Zero;
                public IntPtr hook = IntPtr.Zero;
                public String templateName = null;
                public IntPtr reservedPtr = IntPtr.Zero;
                public int reservedInt = 0;
                public int flagsEx = 0;
            }
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetForegroundWindow();

            public class Shell32
            {
                public delegate int BFFCALLBACK(IntPtr hwnd, uint uMsg, IntPtr lParam, IntPtr lpData);

                [DllImport("Shell32.DLL", CharSet = CharSet.Auto)]
                public static extern int SHGetMalloc(out IMalloc ppMalloc);

                [DllImport("Shell32.DLL", CharSet = CharSet.Auto)]
                public static extern int SHGetSpecialFolderLocation(
                            IntPtr hwndOwner, int nFolder, out IntPtr ppidl);

                [DllImport("Shell32.DLL", CharSet = CharSet.Auto)]
                public static extern int SHGetPathFromIDList(
                            IntPtr pidl, byte[] pszPath);

                [DllImport("Shell32.DLL", CharSet = CharSet.Auto)]
                public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO bi);
            }

            public class User32
            {
                public delegate bool delNativeEnumWindowsProc(IntPtr hWnd, IntPtr lParam);

                [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
                public static extern bool EnumWindows(delNativeEnumWindowsProc callback, IntPtr extraData);

                [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
                public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
            }

            public class Comdlg32
            {
                [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
                public static extern bool GetSaveFileName([In, Out] OpenFileName ofd);
                //链接指定系统函数       打开文件对话框
                [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
                public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

            }

        }

        private class Win32Instance
        {
            //-------------------------------------------------------------------------
            private HandleRef unityWindowHandle;
            private bool bUnityHandleSet;
            //-------------------------------------------------------------------------
            public IntPtr GetHandle(ref bool bSuccess)
            {
                bUnityHandleSet = false;
                Win32API.User32.EnumWindows(__EnumWindowsCallBack, IntPtr.Zero);
                bSuccess = bUnityHandleSet;
                return unityWindowHandle.Handle;
            }
            //-------------------------------------------------------------------------
            private bool __EnumWindowsCallBack(IntPtr hWnd, IntPtr lParam)
            {
                int procid;

                int returnVal =
                    Win32API.User32.GetWindowThreadProcessId(new HandleRef(this, hWnd), out procid);

                int currentPID = System.Diagnostics.Process.GetCurrentProcess().Id;

                HandleRef handle =
                    new HandleRef(this,
                    System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

                if (procid == currentPID)
                {
                    unityWindowHandle = new HandleRef(this, hWnd);
                    bUnityHandleSet = true;
                    return false;
                }

                return true;
            }
        }
    }
}
