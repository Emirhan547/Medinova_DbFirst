using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Medinova.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Medinova.Services
{
    public class AwsImageUploadService
    {
        private readonly string accessKey;
        private readonly string secretKey;
        private readonly string regionName;
        private readonly string bucketName;

        public AwsImageUploadService()
        {
            accessKey = Env.Get("MEDINOVA_AWS_ACCESS_KEY");
            secretKey = Env.Get("MEDINOVA_AWS_SECRET_KEY");
            regionName = Env.Get("MEDINOVA_AWS_REGION");
            bucketName = Env.Get("MEDINOVA_AWS_BUCKET_NAME");
        }

        public string UploadImage(HttpPostedFileBase file, string folder)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var extension = Path.GetExtension(file.FileName);
            var key = $"{folder}/{Guid.NewGuid():N}{extension}";

            using (var client = CreateClient())
            using (var transferUtility = new TransferUtility(client))
            {
                var request = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName, // 🔥 ARTIK NULL DEĞİL
                    InputStream = file.InputStream,
                    Key = key,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                RunSync(() => transferUtility.UploadAsync(request));
            }

            return $"https://{bucketName}.s3.{regionName}.amazonaws.com/{key}";
        }

        private AmazonS3Client CreateClient()
        {
            var region = RegionEndpoint.GetBySystemName(regionName);
            return new AmazonS3Client(accessKey, secretKey, region);
        }

        private static void RunSync(Func<Task> task)
        {
            task().GetAwaiter().GetResult();
        }
    }
}
