# EventEase Deployment Update Guide

## Scope

This update adds:

- an `EventType` lookup table with seeded classifications
- an `IsAvailable` field on `Venue`
- advanced event filtering by event type, date range, booking status, and venue availability

## Database Update

The application now includes the Entity Framework Core migration `AddEventTypeLookupAndVenueAvailability`.

At startup, [Program.cs](/Users/rea/Desktop/EventEase.Web copy/Program.cs#L38) applies pending migrations automatically with `Database.MigrateAsync()`. That means the Azure SQL Database schema will update during application startup after deployment, assuming the app has permission to connect to the database.

Schema additions:

- `EventType` table seeded with five default categories
- `Event.EventTypeId` foreign key
- `Venue.IsAvailable` boolean field

## Web App Deployment Path

The repository already contains an Azure App Service workflow at [.github/workflows/deploy.yml](/Users/rea/Desktop/EventEase.Web copy/.github/workflows/deploy.yml).

Recommended deployment flow:

1. Build and validate locally.
2. Push the updated code to `main`.
3. Let GitHub Actions publish the app to Azure App Service using the configured publish profile secret.
4. Confirm the deployed app starts successfully and the migration runs against Azure SQL Database.
5. Smoke test the event list, event creation, venue creation/editing, and booking creation.

## Required Azure Configuration

The deployed app still needs valid configuration values for:

- `ConnectionStrings__DefaultConnection`
- `AzureBlobStorage__StorageConnection`
- `AzureBlobStorage__VenueContainerName`
- `AzureBlobStorage__EventContainerName`

For production, these should be stored in Azure App Service application settings or Azure Key Vault references rather than committed into configuration files.

## Post-Deployment Checks

Verify the following in production:

1. Existing events have a valid default event type.
2. Venue create and edit screens persist the availability flag.
3. The event index filter returns correct results for event type, dates, and venue availability.
4. Booking creation excludes unavailable venues.
5. Image upload to Azure Blob Storage still works for both venues and events.