using System;
using System.Collections.Generic;
using System.IO;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using ImporterBLL.Properties;

namespace ImporterBLL.Importers
{
    public class Images : ServiceImporterBase
    {
        private readonly List<string> _fileNames = new List<string>();
        // used mainly for the images importer
        protected string DestinationDirectoryPath { get; private set; }

        // used for S3 upload
        protected S3UploadHelper imageUploadHelper;

        public Images(string fileDirectoryPath, string archiveDirectoryPath, IEnumerable<string> fileNames, string stagingTableName, string formatFilePath, string summaryReportErrorToEmailAddress,
            string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, string destinationPath,
            string sqlaServerPath, string sqlbServerPath,string localSqlPath, string temporaryUploadPath, string daysToRun)
            : base(ServiceLogger.FlatFileType.Images, FlatFiles.FileFormat.TXT, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath, localSqlPath, temporaryUploadPath, daysToRun, false)
        {
            foreach (var fileName in fileNames)
            {
                _fileNames.Add(fileName);
            }

            DestinationDirectoryPath = destinationPath;

            imageUploadHelper = new S3UploadHelper(Settings.Default.AWSAccessKey, Settings.Default.AWSSecretKey, Settings.Default.AWSRegionName, Settings.Default.AWSS3ImageBucketName);

        }

        /*
        protected void CopyFilesToDestinationFolder(DirectoryInfo currentDirectory, string parentPath)
        {
            var files = Directory.GetFiles(currentDirectory.FullName);

            // Copy the files and overwrite destination files if they already exist.
            foreach (var s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                var targetPath = Path.Combine(DestinationDirectoryPath, parentPath + currentDirectory.Name);
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                var fileName = Path.GetFileName(s);
                var destFile = Path.Combine(targetPath, fileName);
                File.Copy(s, destFile, true);
            }

            //recursively create directories and copy files
            foreach (var dir in currentDirectory.GetDirectories())
            {
                CopyFilesToDestinationFolder(dir, string.Format(@"{0}{1}\", parentPath, currentDirectory.Name));
            }
        }
        */

        protected override void UnzipImages(string path)
        {
            var unzipPath = DestinationDirectoryPath;
            if (!Directory.Exists(unzipPath))
                Directory.CreateDirectory(unzipPath);


            ZipHelper.UnzipImages(path, unzipPath);
        }


        protected void UploadImagesFolderToS3(DirectoryInfo currentDirectory, string parentPath)
        {
            var files = Directory.GetFiles(currentDirectory.FullName);

            var targetPath = Path.Combine((currentDirectory.FullName.Contains(Settings.Default.PelImageRootFolderName) ? Settings.Default.PelImageFolder : Settings.Default.WowImageFolder), currentDirectory.Name) + "\\";

            // Copy the files and overwrite destination files if they already exist.
            foreach (var s in files)
            {
                var fileName = Path.GetFileName(s);
                if (string.Equals(Path.GetExtension(s),".jpg"))
                {
                    using(var fileStream = new FileStream(s, FileMode.Open, FileAccess.Read))
                    {
                        imageUploadHelper.Upload(fileStream, (targetPath + fileName).Replace(@"\", @"/"), TempUploadFolder);
                    }
                }

                  //  Console.WriteLine((targetPath + fileName).Replace(@"\", @"/"));
                  
            }
            
            //recursively create directories and copy files
            foreach (var dir in currentDirectory.GetDirectories())
            {
                UploadImagesFolderToS3(dir, string.Format(@"{0}{1}\", parentPath, currentDirectory.Name));
            }

        }

     
        protected override void ProcessBespoke()
        {
            Log(LogType.Log, String.Format("Executing bespoke import on path {0}", FileDirectoryPath));

            // move everything from path\unzippedimages into the DestinationPath
            var path = FileDirectoryPath;
            var images = new DirectoryInfo(DestinationDirectoryPath);
            if (images != null)
            {
                Log(LogType.Log, "Copying image files to destination directory");
                foreach (var dir in images.GetDirectories())
                {
                   // CopyFilesToDestinationFolder(dir, string.Empty);
                    UploadImagesFolderToS3(dir, string.Empty);
                }             
                Log(LogType.Log, "Finished copying image files to destination directory");
            }
            else
                Log(LogType.Log, String.Format("Unzipped images directory {0} doesn't exist", Path.Combine(path, "unzippedimages")));


            // clean up unzipped directories
            if (Directory.Exists(DestinationDirectoryPath))
                Directory.Delete(DestinationDirectoryPath, true);

            Log(LogType.Log, String.Format("Finished bespoke import on path {0}", FileDirectoryPath));

        }
        
        protected override bool ExecuteDataProcessingProc()
        {
            return true;
        }

        protected override void ExecuteResetProc()
        {
            return;
        }

        protected override List<string> FileNames
        {
            get { return _fileNames; }
        }

        protected override string DataProcessProcName
        {
            get { return string.Empty; }
        }

        protected override string ResetProcName
        {
            get { return string.Empty; }
        }
    }
}
