using Microsoft.AspNetCore.Http;

namespace EventEase.Web.Services;

public interface IImageStorageService
{
    Task<string> UploadVenueImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<string> UploadEventImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}
