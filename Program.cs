using EventEase.Web.Data;
using EventEase.Web.Options;
using EventEase.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<AzureBlobStorageOptions>(builder.Configuration.GetSection(AzureBlobStorageOptions.SectionName));
builder.Services.PostConfigure<AzureBlobStorageOptions>(options =>
{
    options.StorageConnection = builder.Configuration["BlobStorageConnection"] ?? options.StorageConnection;
    options.VenueContainerName = builder.Configuration["BlobVenueContainerName"] ?? options.VenueContainerName;
    options.EventContainerName = builder.Configuration["BlobEventContainerName"] ?? options.EventContainerName;

    if (long.TryParse(builder.Configuration["BlobMaxFileSizeBytes"], out var maxFileSizeBytes))
    {
        options.MaxFileSizeBytes = maxFileSizeBytes;
    }

    if (bool.TryParse(builder.Configuration["BlobEnablePublicReadAccess"], out var enablePublicReadAccess))
    {
        options.EnablePublicReadAccess = enablePublicReadAccess;
    }
});
builder.Services.AddSingleton<IImageStorageService, AzureBlobImageStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync(); // ← auto-applies migrations on startup
    await SeedData.InitializeAsync(dbContext);
}

app.Run();
