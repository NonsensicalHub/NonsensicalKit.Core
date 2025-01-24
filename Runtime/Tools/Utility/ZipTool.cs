using System;
using System.IO;
using System.Linq;
using NonsensicalKit.SharpZipLib.Zip;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    public interface IZipCallback
    {
        /// <summary>
        /// 压缩单个文件或文件夹前执行的回调
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹</returns>
        public bool OnPreZip(ZipEntry entry);

        /// <summary>
        /// 压缩单个文件或文件夹后执行的回调
        /// </summary>
        /// <param name="entry"></param>
        public void OnPostZip(ZipEntry entry);

        /// <summary>
        /// 压缩执行完毕后的回调
        /// </summary>
        /// <param name="result">true表示压缩成功，false表示压缩失败</param>
        public void OnFinished(bool result);
    }

    public interface IUnzipCallback
    {
        /// <summary>
        /// 解压单个文件或文件夹前执行的回调
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹</returns>
        public bool OnPreUnzip(ZipEntry entry);

        /// <summary>
        /// 解压单个文件或文件夹后执行的回调
        /// </summary>
        /// <param name="entry"></param>
        public void OnPostUnzip(ZipEntry entry);

        /// <summary>
        /// 解压执行完毕后的回调
        /// </summary>
        /// <param name="result">true表示解压成功，false表示解压失败</param>
        public void OnFinished(bool result);
    }

    /// <summary>
    /// https://blog.csdn.net/u014361280/article/details/109677502
    /// 待整改
    /// </summary>
    public class ZipTool : MonoBehaviour
    {
        /// <summary>
        /// 压缩文件和文件夹
        /// </summary>
        /// <param name="fileOrDirectoryArray">文件夹路径和文件名</param>
        /// <param name="outputPathName">压缩后的输出路径文件名</param>
        /// <param name="password">压缩密码</param>
        /// <param name="zipCallback">ZipCallback对象，负责回调</param>
        /// <returns></returns>
        public static bool Zip(string[] fileOrDirectoryArray, string outputPathName, string password = null, IZipCallback zipCallback = null)
        {
            if ((null == fileOrDirectoryArray) || string.IsNullOrEmpty(outputPathName))
            {
                if (null != zipCallback)
                    zipCallback.OnFinished(false);

                return false;
            }

            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(outputPathName));
            zipOutputStream.SetLevel(6); // 压缩质量和压缩速度的平衡点
            if (!string.IsNullOrEmpty(password))
                zipOutputStream.Password = password;

            for (int index = 0; index < fileOrDirectoryArray.Length; ++index)
            {
                bool result = false;
                string fileOrDirectory = fileOrDirectoryArray[index];
                if (Directory.Exists(fileOrDirectory))
                    result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream, zipCallback);
                else if (File.Exists(fileOrDirectory))
                    result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream, zipCallback);

                if (!result)
                {
                    if (null != zipCallback)
                        zipCallback.OnFinished(false);

                    return false;
                }
            }

            zipOutputStream.Finish();
            zipOutputStream.Close();

            if (null != zipCallback)
                zipCallback.OnFinished(true);

            return true;
        }

        /// <summary>
        /// 解压Zip包
        /// </summary>
        /// <param name="filePathName">Zip包的文件路径名</param>
        /// <param name="outputPath">解压输出路径</param>
        /// <param name="password">解压密码</param>
        /// <param name="unzipCallback">UnzipCallback对象，负责回调</param>
        /// <returns></returns>
        public static bool UnzipFile(string filePathName, string outputPath, string password = null, IUnzipCallback unzipCallback = null)
        {
            if (string.IsNullOrEmpty(filePathName) || string.IsNullOrEmpty(outputPath))
            {
                unzipCallback?.OnFinished(false);

                return false;
            }

            try
            {
                return UnzipFile(File.OpenRead(filePathName), outputPath, password, unzipCallback);
            }
            catch (Exception e)
            {
                Debug.LogError("[ZipUtility.UnzipFile]: " + e);

                if (null != unzipCallback)
                    unzipCallback.OnFinished(false);

                return false;
            }
        }

        /// <summary>
        /// 解压Zip包
        /// </summary>
        /// <param name="fileBytes">Zip包字节数组</param>
        /// <param name="outputPath">解压输出路径</param>
        /// <param name="password">解压密码</param>
        /// <param name="unzipCallback">UnzipCallback对象，负责回调</param>
        /// <returns></returns>
        public static bool UnzipFile(byte[] fileBytes, string outputPath, string password = null, IUnzipCallback unzipCallback = null)
        {
            if ((null == fileBytes) || string.IsNullOrEmpty(outputPath))
            {
                unzipCallback?.OnFinished(false);

                return false;
            }

            bool result = UnzipFile(new MemoryStream(fileBytes), outputPath, password, unzipCallback);
            if (!result)
            {
                unzipCallback?.OnFinished(false);
            }

            return result;
        }

        /// <summary>
        /// 解压Zip包
        /// </summary>
        /// <param name="inputStream">Zip包输入流</param>
        /// <param name="outputPath">解压输出路径</param>
        /// <param name="password">解压密码</param>
        /// <param name="unzipCallback">UnzipCallback对象，负责回调</param>
        /// <returns></returns>
        public static bool UnzipFile(Stream inputStream, string outputPath, string password = null, IUnzipCallback unzipCallback = null)
        {
            if ((null == inputStream) || string.IsNullOrEmpty(outputPath))
            {
                unzipCallback?.OnFinished(false);

                return false;
            }

            // 创建文件目录
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // 解压Zip包
            using (ZipInputStream zipInputStream = new ZipInputStream(inputStream))
            {
                if (!string.IsNullOrEmpty(password))
                    zipInputStream.Password = password;

                while (zipInputStream.GetNextEntry() is { } entry)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if ((null != unzipCallback) && !unzipCallback.OnPreUnzip(entry))
                        continue; // 过滤

                    string filePathName = Path.Combine(outputPath, entry.Name);

                    // 创建文件目录
                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(filePathName);
                        continue;
                    }

                    // 写入文件
                    try
                    {
                        using FileStream fileStream = File.Create(filePathName);
                        byte[] bytes = new byte[1024];
                        while (true)
                        {
                            int count = zipInputStream.Read(bytes, 0, bytes.Length);
                            if (count > 0)
                                fileStream.Write(bytes, 0, count);
                            else
                            {
                                unzipCallback?.OnPostUnzip(entry);

                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[ZipUtility.UnzipFile]: " + e);

                        unzipCallback?.OnFinished(false);

                        return false;
                    }
                }
            }

            unzipCallback?.OnFinished(true);

            return true;
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="filePathName">文件路径名</param>
        /// <param name="parentRelPath">要压缩的文件的父相对文件夹</param>
        /// <param name="zipOutputStream">压缩输出流</param>
        /// <param name="zipCallback">ZipCallback对象，负责回调</param>
        /// <returns></returns>
        private static bool ZipFile(string filePathName, string parentRelPath, ZipOutputStream zipOutputStream, IZipCallback zipCallback = null)
        {
            //Crc32 crc32 = new Crc32();
            ZipEntry entry;
            try
            {
                string entryName = parentRelPath + '/' + Path.GetFileName(filePathName);
                entry = new ZipEntry(entryName);
                entry.DateTime = DateTime.Now;

                if ((null != zipCallback) && !zipCallback.OnPreZip(entry))
                    return true; // 过滤

                byte[] buffer = File.ReadAllBytes(filePathName);

                entry.Size = buffer.Length;

                //crc32.Reset();
                //crc32.Update(buffer);
                //entry.Crc = crc32.Value;

                zipOutputStream.PutNextEntry(entry);
                zipOutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("[ZipUtility.ZipFile]: " + e);
                return false;
            }

            zipCallback?.OnPostZip(entry);

            return true;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="path">要压缩的文件夹</param>
        /// <param name="parentRelPath">要压缩的文件夹的父相对文件夹</param>
        /// <param name="zipOutputStream">压缩输出流</param>
        /// <param name="zipCallback">ZipCallback对象，负责回调</param>
        /// <returns></returns>
        private static bool ZipDirectory(string path, string parentRelPath, ZipOutputStream zipOutputStream, IZipCallback zipCallback = null)
        {
            ZipEntry entry;
            try
            {
                string entryName = Path.Combine(parentRelPath, Path.GetFileName(path) + '/');
                entry = new ZipEntry(entryName)
                {
                    DateTime = DateTime.Now,
                    Size = 0
                };

                if ((null != zipCallback) && !zipCallback.OnPreZip(entry))
                    return true; // 过滤

                zipOutputStream.PutNextEntry(entry);
                zipOutputStream.Flush();

                string[] files = Directory.GetFiles(path);
                foreach (var t in files)
                {
                    // 排除Unity中可能的 .meta 文件
                    if (t.EndsWith(".meta"))
                    {
                        Debug.LogWarning(t + " not to zip");
                        continue;
                    }

                    ZipFile(t, Path.Combine(parentRelPath, Path.GetFileName(path)), zipOutputStream, zipCallback);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ZipUtility.ZipDirectory]: " + e);
                return false;
            }

            string[] directories = Directory.GetDirectories(path);
            if (directories.Any(t => !ZipDirectory(t, Path.Combine(parentRelPath, Path.GetFileName(path)), zipOutputStream, zipCallback)))
            {
                return false;
            }

            zipCallback?.OnPostZip(entry);

            return true;
        }
    }
}
