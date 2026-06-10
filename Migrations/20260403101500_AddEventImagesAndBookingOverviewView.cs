using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Web.Migrations
{
    public partial class AddEventImagesAndBookingOverviewView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<string>(
                    name: "ImageUrl",
                    table: "Event",
                    type: "TEXT",
                    maxLength: 500,
                    nullable: false,
                    defaultValue: "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=900&q=80");

                migrationBuilder.Sql(
                    """
                    DROP VIEW IF EXISTS vwBookingOverview;
                    CREATE VIEW vwBookingOverview AS
                    SELECT
                        b.BookingId,
                        b.BookingDate,
                        e.EventId,
                        e.EventName,
                        e.EventDate,
                        e.EndDate,
                        e.Description AS EventDescription,
                        e.ImageUrl AS EventImageUrl,
                        v.VenueId,
                        v.VenueName,
                        v.Location AS VenueLocation,
                        v.Capacity AS VenueCapacity,
                        v.ImageUrl AS VenueImageUrl
                    FROM Booking AS b
                    INNER JOIN Event AS e ON b.EventId = e.EventId
                    INNER JOIN Venue AS v ON b.VenueId = v.VenueId;
                    """);

                return;
            }

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Event",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=900&q=80");

            migrationBuilder.Sql(
                """
                CREATE OR ALTER VIEW dbo.vwBookingOverview AS
                SELECT
                    b.BookingId,
                    b.BookingDate,
                    e.EventId,
                    e.EventName,
                    e.EventDate,
                    e.EndDate,
                    e.Description AS EventDescription,
                    e.ImageUrl AS EventImageUrl,
                    v.VenueId,
                    v.VenueName,
                    v.Location AS VenueLocation,
                    v.Capacity AS VenueCapacity,
                    v.ImageUrl AS VenueImageUrl
                FROM dbo.Booking AS b
                INNER JOIN dbo.Event AS e ON b.EventId = e.EventId
                INNER JOIN dbo.Venue AS v ON b.VenueId = v.VenueId;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("DROP VIEW IF EXISTS vwBookingOverview;");

                migrationBuilder.DropColumn(
                    name: "ImageUrl",
                    table: "Event");

                return;
            }

            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vwBookingOverview;");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Event");
        }
    }
}
