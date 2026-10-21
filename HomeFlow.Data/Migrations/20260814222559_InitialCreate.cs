using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VirtualServiceInformationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UPRN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoundLeg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Round = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoundGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduleDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolutionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolutionSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MissedCollectionAllowed = table.Column<bool>(type: "bit", nullable: false),
                    LastCollection = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastCollectionCompleted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextCollection = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SubscriptionEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinCollections", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinCollections");
        }
    }
}
