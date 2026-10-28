using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxBytes = 5 * 1024 * 1024;
        private readonly string? _connectionString;

        public BlobStorageService(IConfiguration config)
            => _connectionString = config["AzureStorage:ConnectionString"];

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

        // Returns an error message, or null when the file is acceptable
        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0) return "Please choose an image to upload.";
            if (file.Length > MaxBytes) return "The image must be 5 MB or smaller.";
            if (!AllowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                return "Only JPG, PNG, WEBP or GIF images are allowed.";
            return null;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            var container = new BlobContainerClient(_connectionString, containerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlobClient($"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}");
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });
            return blob.Uri.ToString();
        }
    }
}