namespace EventEase.Web.Options;

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; set; } = string.Empty;
    public string StorageConnection { get; set; } = string.Empty;
    public string VenueContainerName { get; set; } = "venue-images";
    public string EventContainerName { get; set; } = "event-images";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public bool EnablePublicReadAccess { get; set; } = true;
}
