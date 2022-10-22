using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using System.Runtime.CompilerServices;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon;
using System.Security.Cryptography;
using System.Text;

namespace FileS31.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.USWest1;
        public static IAmazonS3 _amazonS3 = new AmazonS3Client("accessKey", "secretAccessKey", regionEndpoint);
        private readonly ILogger<FileController> _logger;

        public string BucketName = "FileUpload";

        public FileController(ILogger<FileController> logger, IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
            _logger = logger;
        }

        #region POST

        /// Upload
        [HttpPost("UploadFile")]
        public async Task PostFile(IFormFile formFile)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, BucketName);

            if (!bucketExists)
            {
                var bucketRequest = new PutBucketRequest()
                {
                    BucketName = BucketName,
                    UseClientRegion = true
                };
                await _amazonS3.PutBucketAsync(bucketRequest);
            }

            var objectRequest = new PutObjectRequest()
            {
                BucketName = BucketName,
                Key = $"{DateTime.Now:yyyy\\/MM\\/dd\\/hhmmss}-{formFile.FileName}",
                InputStream = formFile.OpenReadStream(),
                StorageClass = S3StorageClass.Standard
            };

            objectRequest.Metadata.Add("Test", "Metadata");
            var response = await _amazonS3.PutObjectAsync(objectRequest);
        }

        #endregion


        #region GET

        /// Download
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var response = await _amazonS3.GetObjectAsync(BucketName, fileName);

            using var reader = new StreamReader(response.ResponseStream);
            var fileContents = await reader.ReadToEndAsync();

            return File(response.ResponseStream, response.Headers.ContentType);
        }

        /// Download
        [HttpGet("DownloadFiles")]
        public async Task<IActionResult> GetFiles(string prefix)
        {
            var request = new ListObjectsV2Request()
            {
                BucketName = BucketName,
                Prefix = prefix
            };

            var response = await _amazonS3.ListObjectsV2Async(request);
            var preSignedURL = response.S3Objects.Select(o =>
            {
                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = BucketName,
                    Key = o.Key,
                    Expires = DateTime.UtcNow.AddSeconds(30)
                };
                return _amazonS3.GetPreSignedURL(request);
            });

            return Ok(preSignedURL);
        }


        #endregion

        #region DELETE

        /// Delete
        [HttpDelete("DeleteFile")]
        public async Task DeleteFile(string file)
        {
            await _amazonS3.DeleteObjectAsync(BucketName, file);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// MD5
        public static string GetMD5(string str)
        {
            MD5 md5 = MD5.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream;
            StringBuilder sb = new StringBuilder();
            stream = md5.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

    }
}
