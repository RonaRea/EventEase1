using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventEase.Web.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EventEase.Web.Services;

public class AzureBlobImageStorageService : IImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private readonly AzureBlobStorageOptions _options;

    public AzureBlobImageStorageService(IOptions<AzureBlobStorageOptions> options)
    {
        _options = options.Value;
    }

    public Task<string> UploadVenueImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => UploadAsync(file, _options.VenueContainerName, cancellationToken);

    public Task<string> UploadEventImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => UploadAsync(file, _options.EventContainerName, cancellationToken);

    private async Task<string> UploadAsync(IFormFile file, string containerName, CancellationToken cancellationToken)
    {
        var connectionString = GetConfiguredConnectionString();
        ValidateFile(file);

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName.ToLowerInvariant());
        var accessType = _options.EnablePublicReadAccess ? PublicAccessType.Blob : PublicAccessType.None;

        await containerClient.CreateIfNotExistsAsync(accessType, cancellationToken: cancellationToken);
        await containerClient.SetAccessPolicyAsync(accessType, cancellationToken: cancellationToken);

        var extension = Path.GetExtension(file.FileName);
        var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var blobClient = containerClient.GetBlobClient(safeFileName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            },
            cancellationToken);

        return blobClient.Uri.ToString();
    }

    private string GetConfiguredConnectionString()
    {
        var connectionString = !string.IsNullOrWhiteSpace(_options.StorageConnection)
            ? _options.StorageConnection
            : _options.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Azure Blob Storage is not configured. Add AzureBlobStorage settings before uploading images.");
        }

        return connectionString;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Please choose an image file before saving.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException("The selected file is too large. Upload an image smaller than 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only JPG, PNG, WEBP, or GIF images are allowed.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException("The uploaded file is not a supported image format.");
        }
    }
}
