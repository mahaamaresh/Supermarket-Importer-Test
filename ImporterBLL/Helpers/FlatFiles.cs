using System;
using System.IO;
using Amazon.CloudWatchLogs.Model;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ImporterBLL.Objects;
using ImporterBLL.Properties;

namespace ImporterBLL.Helpers
{
    public class FlatFiles
    {

        public enum FileStatus
        {
            FileNotFound,
            NoRecordFound,
            FileNotAccessible,
            Success
        }

        public enum FileFormat
        {
            TXT,
            CSV,
            JSON
        }

        public enum CompressType
        {
            Zip,
            GZip,
            None
        }

        /// <summary>
        /// Archives a file, zipping it up and moving it or just moving it based on the passed in params. Also removes the un-zipped file from the file system
        /// </summary>
        /// <param name="sourcePath">Absolute path to the source file including file name</param>
        /// <param name="destinationPath">Path to the directory that source file will be archived to</param>
        /// <param name="zip">Set this to true if the file you are archiving needs to be zipped</param>
        /// <param name="format">Enum based</param>
        /// <param name="tempFolder"></param>
        /// <returns>Bool</returns>
        public static bool Archive(string sourcePath, string destinationPath, bool zip, FileFormat format, string tempFolder)
        {
            if (zip)
            {
                if (!S3FileExists(sourcePath))
                    throw new FileNotFoundException(String.Format("File {0} does not exist.", sourcePath));

                // zip and if successful, delete the unzipped source file
                var file = new FileInfo(sourcePath);
                var tempFile = tempFolder + Guid.NewGuid().ToString() + ".zip";
                var zipSuccess = zipFile(sourcePath, tempFile);
                if (zipSuccess)
                {
                    try
                    {
                        using (var zipStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                        {

                            CopyFileToS3Archive(destinationPath, zipStream, file.Name.Replace(file.Extension, ".zip"), tempFolder);
                            DeleteS3File(sourcePath);
                            return true;
                        }
                    }
                    finally
                    {
                        if (File.Exists(tempFile))
                            File.Delete(tempFile);
                    }


                }

            }
            using (var fileStream = GetS3File(sourcePath))
                if (fileStream != null)
                {
                    var file = new FileInfo(sourcePath);
                    CopyFileToS3Archive(destinationPath, fileStream, file.Name, tempFolder);
                    DeleteS3File(sourcePath);
                    return true;
                }
            throw new FileNotFoundException(String.Format("File {0} does not exist.", sourcePath));
        }

        /// <summary>
        /// gets this file from s3
        /// </summary>
        /// <param name="sourcePath"></param>
        public static Stream GetS3File(string sourcePath)
        {
            var s3 = new S3UploadHelper(Settings.Default.AWSAccessKey,
                 Settings.Default.AWSSecretKey,
                 Settings.Default.AWSRegionName,
                 Settings.Default.AWSS3BucketName);

            var response = s3.GetFile(sourcePath);

            //  convert to memory stream so that we can seek

            if (response.Item == null)
            {
                return null;
            }
            else
            {
                return response.Item;
            }


        }

        /// <summary>
        /// deletes this file from s3
        /// </summary>
        /// <param name="sourcePath"></param>
        public static void DeleteS3File(string sourcePath)
        {
            var s3 = new S3UploadHelper(Settings.Default.AWSAccessKey,
                 Settings.Default.AWSSecretKey,
                 Settings.Default.AWSRegionName,
                 Settings.Default.AWSS3BucketName);

            s3.DeleteFile(sourcePath);
        }

        /// <summary>
        /// returns whether the specified item exits in the s3 bucket
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static bool S3FileExists(string sourcePath)
        {
            var s3 = new S3UploadHelper(Settings.Default.AWSAccessKey,
                  Settings.Default.AWSSecretKey,
                  Settings.Default.AWSRegionName,
                  Settings.Default.AWSS3BucketName);

            var response = s3.FileExists(sourcePath);
            return !string.IsNullOrEmpty(response.Item);
        }

        /// <summary>
        /// adds the stream to s3 with the specified key
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="key"></param>
        /// <param name="tempFolder"></param>
        public static void AddS3File(Stream fileStream, string key, string tempFolder)
        {
            var uploader = new S3UploadHelper(Settings.Default.AWSAccessKey,
                Settings.Default.AWSSecretKey,
                Settings.Default.AWSRegionName,
                Settings.Default.AWSS3BucketName);

            uploader.Upload(fileStream, key, tempFolder);
        }

        /// <summary>
        /// uploads this file stream to the S3 Archive bucket
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="fileStream"></param>
        /// <param name="fileName"></param>
        /// <param name="tempFolder"></param>
        public static void CopyFileToS3Archive(string archivePath, Stream fileStream, string fileName, string tempFolder)
        {
            var uploader = new S3UploadHelper(Settings.Default.AWSAccessKey,
                Settings.Default.AWSSecretKey,
                Settings.Default.AWSRegionName,
                Settings.Default.AWSS3BucketName);

            var archiveFileName = archivePath + fileName;
            uploader.Upload(fileStream, archiveFileName, tempFolder);
        }

        /// <summary>
        /// Zips a file
        /// </summary>
        /// <param name="filePath">Path to the source file</param>
        /// <param name="tempOutputFile">temp file to store the zip</param>
        /// <returns>Bool</returns>
        private static bool zipFile(string filePath, string tempOutputFile)
        {
            if (!S3FileExists(filePath)) return false;
            var file = new FileInfo(filePath);

            var parent = Directory.GetParent(tempOutputFile);
            if (!parent.Exists)
                parent.Create();

            using (var fileStreamIn = File.Create(tempOutputFile))
            using (var zipStreamOut = new ZipOutputStream(fileStreamIn))
            {
                // Zip with highest compression.
                zipStreamOut.SetLevel(9);
                var crc = new Crc32();

                using (var fileStream = GetS3File(filePath))
                {

                    // Create a new entry for the current file.
                    var entry = new ZipEntry(file.Name) { DateTime = DateTime.Now };

                    // set Size and the crc, because the information
                    // about the size and crc should be stored in the header
                    // if it is not set it is automatically written in the footer.
                    // (in this case size == crc == -1 in the header)
                    // Some ZIP programs have problems with zip files that don't store
                    // the size and crc in the header.

                    // Reset and update the crc.
                    crc.Reset();

                    zipStreamOut.PutNextEntry(entry);


                    const int maxbufferSize = 1024 * 8;
                    // Read full stream to in-memory buffer, for larger files, only buffer 8 kBytes
                    var buffer = new byte[maxbufferSize];

                    int bytesRead;
                    do
                    {
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                        crc.Update(buffer, 0, bytesRead);
                        zipStreamOut.Write(buffer, 0, bytesRead);

                    } while (bytesRead > 0);

                    // Update entry and write to zip stream.
                    entry.Crc = crc.Value;

                    // Get rid of the buffer, because this
                    // is a huge impact on the memory usage.
                    buffer = null;
                    fileStream.Close();
                }
                // Finalize the zip output.
                zipStreamOut.Finish();


                // Flushes the create and close.
                zipStreamOut.Flush();
                zipStreamOut.Close();
            }
            return true;
        }



    }
}
