using System;
using System.IO;
using System.Net;
using System.Security.Policy;
using Amazon.S3;
using Amazon.S3.Model;

namespace ImporterBLL.Helpers
{
    public class Status
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public string FriendlyMessage { get; set; }

    }



    public class Response<T> where T : class
    {

        public Response()
        {
            Status = new Status();
        }

        public T Item { get; set; }
        public Status Status { get; set; }
        public int Count { get; set; }
    }



    public class S3UploadHelper
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _regionName;
        private readonly string _bucketName;

        public S3UploadHelper(string AccessKey, string SecretKey, string RegionName, string BucketName)
        {
            _accessKey = AccessKey;
            _secretKey = SecretKey;
            _regionName = RegionName;
            _bucketName = BucketName;
        }

        public Response<string> Upload(Stream fileStream, string filename, string tempFolder)
        {
            var response = new Response<string>
            {
                Status = new Status { Code = 0, Description = "Success", FriendlyMessage = "Success" }
            };

            if (fileStream != null && (!fileStream.CanSeek || fileStream.Length > 0))
            {
                var tempFile = string.Empty;
                try
                {


                    //  temporarily save file locally to upload it without memory issues
                    tempFile = string.Format("{0}{1}{2}", tempFolder, Guid.NewGuid().ToString(), Path.GetExtension(filename));
                    if (!Directory.GetParent(tempFile).Exists)
                        Directory.GetParent(tempFile).Create();

                    using (var tempStream = File.Create(tempFile))
                    {
                        // write the file locally
                        fileStream.CopyTo(tempStream);
                    }
                    //var response2 = client.PutObject(request); //Uncomment this If you want some info about it
                    //http://docs.aws.amazon.com/general/latest/gr/rande.html Regions string name
                    var client = new AmazonS3Client(_accessKey,
                        _secretKey,
                        Amazon.RegionEndpoint.GetBySystemName(_regionName));


                    var request = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = filename,
                        FilePath = tempFile
                    };
                    
                    client.PutObject(request);

                    response.Item = filename; //


                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    response.Status = new Status
                    {
                        Code = -1,
                        Description = amazonS3Exception.ToString(),
                        FriendlyMessage = "A unexpected error has occurred"
                    };
                }
                finally
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }

            }
            else
            {
                response.Status = new Status
                {
                    Code = -1,
                    Description = "null or empty",
                    FriendlyMessage = "Fail, please select a valid file"
                };
            }

            return response;
        }


        public Response<Stream> GetFile(string filename)
        {
            var response = new Response<Stream>
            {
                Status = new Status { Code = 0, Description = "Success", FriendlyMessage = "Success" }
            };

            try
            {
                //http://docs.aws.amazon.com/general/latest/gr/rande.html Regions string name
                var client = new AmazonS3Client(_accessKey,
                   _secretKey,
                   Amazon.RegionEndpoint.GetBySystemName(_regionName));

                var awsResponse = client.GetObject(_bucketName, filename);
                response.Item = awsResponse.ResponseStream;

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                response.Status = new Status
                {
                    Code = -1,
                    Description = amazonS3Exception.ToString(),
                    FriendlyMessage = "A unexpected error has occurred"
                };
            }


            return response;
        }

        public Response<string> DeleteFile(string filename)
        {
            var response = new Response<string>
            {
                Status = new Status { Code = 0, Description = "Success", FriendlyMessage = "Success" }
            };


            try
            {
                //http://docs.aws.amazon.com/general/latest/gr/rande.html Regions string name
                var client = new AmazonS3Client(_accessKey,
                   _secretKey,
                   Amazon.RegionEndpoint.GetBySystemName(_regionName));

                client.DeleteObject(_bucketName, filename);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                response.Status = new Status
                {
                    Code = -1,
                    Description = amazonS3Exception.ToString(),
                    FriendlyMessage = "A unexpected error has occurred"
                };
            }

            return response;
        }





        /// <summary>
        /// returns true 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Response<string> FileExists(string filename)
        {
            var response = new Response<string>
            {
                Status = new Status { Code = 0, Description = "Success", FriendlyMessage = "Success" }
            };


            try
            {
                //http://docs.aws.amazon.com/general/latest/gr/rande.html Regions string name
                var client = new AmazonS3Client(_accessKey,
                   _secretKey,
                   Amazon.RegionEndpoint.GetBySystemName(_regionName));


                client.GetObjectMetadata(_bucketName, filename);
                response.Item = "exists";

            }
            catch (WebException)
            {
                response.Item = null;

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                response.Status = new Status
                {
                    Code = -1,
                    Description = amazonS3Exception.ToString(),
                    FriendlyMessage = "A unexpected error has occurred"
                };
            }

            return response;
        }

    }
}
