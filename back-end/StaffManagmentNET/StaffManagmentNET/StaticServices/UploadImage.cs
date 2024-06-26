﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;

namespace StaffManagmentNET.StaticServices
{
    public class UploadImage
    {
        private readonly Cloudinary _cloudinary;

        public UploadImage()
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            _cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            _cloudinary.Api.Secure = true;
        }

        public static UploadImage Instance = new UploadImage();

        public async Task<string> UploadAsync(string fileName, string base64)
        {
            try
            {
                var uploadResult = new ImageUploadResult();

                if (string.IsNullOrEmpty(base64) || base64 == "empty")
                {
                    return string.Empty;
                }

                if (base64.StartsWith("data:"))
                {
                    base64 = base64.Substring(base64.IndexOf(',') + 1);
                }

                byte[] bytes = Convert.FromBase64String(base64);

                using (var stream = new MemoryStream(bytes))
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileName, stream),
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true
                    };

                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }

                return uploadResult.Url.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
