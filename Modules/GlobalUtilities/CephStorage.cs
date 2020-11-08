using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class CephStorage
    {
        private static bool Inited = false;

        private static AmazonS3Config config;

        private static void init()
        {
            if (Inited || !RaaiVanSettings.CephStorage.Enabled) return;
            Inited = true;

            config = new AmazonS3Config()
            {
                ServiceURL = RaaiVanSettings.CephStorage.URL
            };
        }

        private static AmazonS3Client get_client()
        {
            init();

            if (config == null) return null;

            AmazonS3Client client = new AmazonS3Client(
                RaaiVanSettings.CephStorage.AccessKey,
                RaaiVanSettings.CephStorage.SecretKey,
                config);

            return client;
        }

        public static List<string> get_buckets()
        {
            AmazonS3Client client = get_client();

            return client == null ? new List<string>() :
                client.ListBuckets().Buckets.Select(b => b.BucketName).ToList();
        }

        public static bool add_file(string fileName, byte[] content)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return false;

                using (MemoryStream stream = new MemoryStream(content)) {
                    TransferUtilityUploadRequest request = new TransferUtilityUploadRequest()
                    {
                        BucketName = RaaiVanSettings.CephStorage.Bucket,
                        Key = fileName,
                        InputStream = stream,
                        StorageClass = S3StorageClass.Standard,
                        PartSize = 6291456 // 6 MB
                        //CannedACL = S3CannedACL.Private
                    };

                    TransferUtility utility = new TransferUtility(client);

                    utility.Upload(request);
                }

                return true;
            }
            catch { return false; }
        }

        public static List<string> files(string folderName, int maxCount = 0)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return new List<string>();

                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = RaaiVanSettings.CephStorage.Bucket;
                request.Prefix = folderName + "/";
                request.MaxKeys = maxCount <= 0 ? 1000000 : maxCount;

                ListObjectsResponse response = client.ListObjects(request);
                
                return response.S3Objects == null ? new List<string>() : response.S3Objects.Select(o => o.Key).ToList();
            }

            catch (AmazonS3Exception ex)
            {
                bool notFound = ex.StatusCode == System.Net.HttpStatusCode.NotFound;
                return new List<string>();
            }
        }

        public static bool folder_exists(string folderName)
        {
            return files(folderName, maxCount: 1).Count > 0;
        }

        public static bool file_exists(string fileName)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return false;

                GetObjectMetadataRequest request = new GetObjectMetadataRequest();
                request.BucketName = RaaiVanSettings.CephStorage.Bucket;
                request.Key = fileName;

                GetObjectMetadataResponse response = client.GetObjectMetadata(request);

                return true;
            }

            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
                return false;
            }
        }

        public static byte[] get_file(string fileName)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return new byte[0];

                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = RaaiVanSettings.CephStorage.Bucket;
                request.Key = fileName;
                
                using (GetObjectResponse response = client.GetObject(request))
                using (MemoryStream stream = new MemoryStream())
                {
                    response.ResponseStream.CopyTo(stream);
                    return stream.ToArray();
                }
            }
            catch { return new byte[0]; }
        }

        public static bool rename_file(string oldFileName, string newFileName)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return false;

                CopyObjectRequest request = new CopyObjectRequest();
                request.SourceBucket = request.DestinationBucket = RaaiVanSettings.CephStorage.Bucket;
                request.SourceKey = oldFileName;
                request.DestinationKey = newFileName;

                CopyObjectResponse response = client.CopyObject(request);
                bool result = response.HttpStatusCode == System.Net.HttpStatusCode.OK;

                if (result) delete_file(oldFileName);

                return result;
            }
            catch { return false; }
        }

        public static bool delete_file(string fileName)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return false;
                
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.BucketName = RaaiVanSettings.CephStorage.Bucket;
                request.Key = fileName;

                DeleteObjectResponse response = client.DeleteObject(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch { return false; }
        }

        public static string get_download_url(string fileName, int expiresInMinutes = 60)
        {
            try
            {
                AmazonS3Client client = get_client();
                if (client == null) return string.Empty;

                if (expiresInMinutes <= 0) expiresInMinutes = 60;

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
                request.BucketName = RaaiVanSettings.CephStorage.Bucket;
                request.Key = fileName;
                request.Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);
                request.Protocol = RaaiVanSettings.CephStorage.URL.ToLower().StartsWith("https") ? Protocol.HTTPS : Protocol.HTTP;

                return client.GetPreSignedURL(request);
            }
            catch { return string.Empty; }
        }
    }
}
