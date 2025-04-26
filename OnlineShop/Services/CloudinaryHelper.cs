using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Net;

namespace OnlineShop.Services
{
    public static class CloudinaryHelper
    {
        public static async Task<string?> UploadImageAsync(Cloudinary cloudinary, IFormFile image)
        {
            long maxFileSize = 5 * 1024 * 1024; // 5MB

            if (image == null || image.Length == 0)
            {
                return null;
            }

            if (image.Length > maxFileSize)
            {
                return null;
            }

            var id = await GenerateShortUniqueCode();
            try
            {
                using (var stream = image.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(image.FileName, stream),
                        Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                        UploadPreset = "SWP391Project",
                        UseFilename = true,     // Dùng tên file gốc làm PublicId
                        UniqueFilename = true
                    };

                    var uploadResult = await cloudinary.UploadAsync(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> DeleteImageFromCloudinaryAsync(Cloudinary cloudinary, string imageUrl)
        {
            string publicId = ExtractPublicIdFromUrl(imageUrl);

            if (string.IsNullOrEmpty(publicId))
            {
                return false;
            }

            try
            {
                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error deleting image: {deletionResult.Error?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public static string ExtractPublicIdFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string path = uri.AbsolutePath;
            string[] pathParts = path.Split('/');
            string publicId = pathParts[pathParts.Length - 1].Split('.')[0]; // Lấy phần cuối cùng trước đuôi file
            return publicId;
        }

        public static async Task<string> GenerateShortUniqueCode()
        {
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            byte[] shortBytes = new byte[6];
            Array.Copy(bytes, 0, shortBytes, 0, 6);
            return Convert.ToBase64String(shortBytes).Substring(0, 8);
        }
    }
}

