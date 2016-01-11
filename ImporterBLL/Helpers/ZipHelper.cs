using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using ImporterBLL.Exceptions;
using ImporterBLL.Objects;

namespace ImporterBLL.Helpers
{
    internal static class ZipHelper
    {
        //For now keeping this enum in FlatFiles class, only for backward compatability
        public static void UnzipFile(string filePath, string outputPath, FlatFiles.CompressType compressType, string tempFolder)
        {
            switch (compressType)
            {
                case FlatFiles.CompressType.GZip:
                    UnGzipFile(filePath, outputPath, tempFolder);
                    break;

                case FlatFiles.CompressType.Zip:
                default:
                     UnzipFile(filePath, outputPath, tempFolder);
                    break;
            }

        }

        private static void UnGzipFile(string filePath, string outputPath, string tempFolder)
        {
            var dataBuffer = new byte[4096];

            var gzip = new FileInfo(filePath);
            // if its not a zipped file, assume its already been unzipped
            if (!gzip.Extension.Contains("gz"))
            {
                return;
            }

            var tempFile = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(filePath));
            try
            {
                using (var fileStreamIn = FlatFiles.GetS3File(filePath))
                using (var gzipStreamIn = new GZipInputStream(fileStreamIn))
                {
                    using (var fileStreamOut = File.Create(tempFile))
                    {
                        // copy unzipped file locally temporily
                        StreamUtils.Copy(gzipStreamIn, fileStreamOut, dataBuffer);
                    }
                }

                using (var tempFileStream = new FileStream(tempFile,FileMode.Open, FileAccess.Read))
                {
                    // upload unzipped version to s3
                    var tempFileInfo = new FileInfo(tempFile);
                    outputPath = outputPath.EndsWith("\\") ? outputPath : outputPath + "\\";
                    var outputFile = string.Format("{0}{1}", outputPath, tempFileInfo.Name).Replace("\\", "/");
                    var key = outputFile.Replace("\\", "/");
                    FlatFiles.AddS3File(tempFileStream, key, tempFolder);
                }
            }
            finally
            {
                // clean up temp file
                if(File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            

        }

        private static void UnzipFile(string filePath, string outputPath, string tempFolder)
        {
            var zip = new FileInfo(filePath);
            // if its not a zipped file, assume its already been unzipped
            if (!zip.Extension.Contains("zip"))
            {
                return;
            }

            using (var fileStreamIn = FlatFiles.GetS3File(filePath))
            using (var zipStreamIn = new ZipInputStream(fileStreamIn))
            {
                ZipEntry entry = null;

                outputPath = outputPath.EndsWith("\\") ? outputPath : outputPath + "\\";

                while ((entry = zipStreamIn.GetNextEntry()) != null)
                {
                    /*
                    using (var fileStreamOut = new MemoryTributary())
                    {
                        var data = new byte[2048];
                        while (true)
                        {
                            var readLength = zipStreamIn.Read(data, 0, data.Length);
                            if (readLength > 0)
                                fileStreamOut.Write(data, 0, readLength);
                            else
                                break;
                        }
                     * */

                        // copy this to s3
                        var outputFile = string.Format("{0}{1}", outputPath, entry.Name).Replace("\\", "/");
                       // fileStreamOut.Seek(0, SeekOrigin.Begin);

                        if (!zipStreamIn.CanSeek || zipStreamIn.Length > 0)
                        {
                            FlatFiles.AddS3File(zipStreamIn, outputFile, tempFolder);
                            
                        }
                        else
                        {
                            // just unzipped a 0 bytes file. Don't bother processing and archive it
                            throw new EmptyFileException("Unzipped zile contained no data");
                        }

                        
                    //}

                }

                //fileStreamIn.Close();
            }

        }


        public static void UnzipImages(string filePath, string outputPath)
        {
            var zip = new FileInfo(filePath);
            // if its not a zipped file, assume its already been unzipped
            if (!zip.Extension.Contains("zip"))
            {
                return;
            }

            using (var fileStreamIn = FlatFiles.GetS3File(filePath))
            using (var zipStreamIn = new ZipInputStream(fileStreamIn))
            {
                ZipEntry entry = null;

                outputPath = outputPath.EndsWith("\\") ? outputPath : outputPath + "\\";

                while ((entry = zipStreamIn.GetNextEntry()) != null)
                {
                    if (entry.IsDirectory)
                    {
                        var outputDir = string.Format("{0}{1}", outputPath, entry.Name);

                        // some entries have a trailing "/" so remove it
                        if (outputDir.EndsWith("/"))
                            outputDir = outputDir.TrimEnd(new char[] { '/' });

                        // an extra check to see if there are sub directories
                        var subDirs = outputDir.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                        // create the necessary sub directories
                        if (subDirs.Length > 1)
                        {
                            for (var i = 1; i < subDirs.Length; i++)
                            {
                                outputDir = outputDir.Replace("/", "\\");
                                if (!Directory.Exists(outputDir))
                                    Directory.CreateDirectory(outputDir);
                            }
                        }
                        else
                        {
                           Directory.CreateDirectory(outputDir);
                        }
                    }
                    else if (entry.IsFile)
                    {
                        var outputFile = string.Format("{0}{1}", outputPath, entry.Name).Replace("/", "\\");

                        //for some stupid reason, if a single file is in a folder in the zip file, it treats the entire
                        //folder-file as 1 file, and then complains because the directory doesn't exist. 
                        //Below is a fix: note that it uses the outputFile variable, not the outputPath
                        if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
                          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                        
                        using (var fileStreamOut = File.Create(outputFile, 2048, FileOptions.None))
                        {
                            var data = new byte[2048];
                            var readLength = 0;
                            while (true)
                            {
                                readLength = zipStreamIn.Read(data, 0, data.Length);
                                if (readLength > 0)
                                    fileStreamOut.Write(data, 0, readLength);
                                else
                                    break;
                            }
                        }
                         
                    }

                    //if it's not a file or a directory, ignore it.

                }
                fileStreamIn.Close();
            }
        }
    }
}
