using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthRecordsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BirthRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SgcnId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChildFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChildLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BirthTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BirthPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HospitalId = table.Column<Guid>(type: "uuid", nullable: false),
                    MotherFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FatherFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    HospitalFileNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BirthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BirthRecords_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BirthRecords_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BirthRecords_CreatedByUserId",
                table: "BirthRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BirthRecords_HospitalId",
                table: "BirthRecords",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_BirthRecords_SgcnId",
                table: "BirthRecords",
                column: "SgcnId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BirthRecords");
        }
    }
}
