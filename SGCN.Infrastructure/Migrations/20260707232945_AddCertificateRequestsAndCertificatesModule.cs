using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateRequestsAndCertificatesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedByUserId = table.Column<string>(type: "text", nullable: false),
                    TargetFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetGender = table.Column<int>(type: "integer", nullable: false),
                    TargetBirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MotherFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FatherFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    BirthPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HospitalFileNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelationshipToTarget = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedBirthRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CertificateRequests_BirthRecords_LinkedBirthRecordId",
                        column: x => x.LinkedBirthRecordId,
                        principalTable: "BirthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CertificateRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    BirthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChildLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BirthTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BirthPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HospitalName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CommuneName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DepartmentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MotherFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FatherFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerificationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PdfPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QrCodePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AnnulledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnnulledReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_BirthRecords_BirthRecordId",
                        column: x => x.BirthRecordId,
                        principalTable: "BirthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificateRequests_CertificateRequestId",
                        column: x => x.CertificateRequestId,
                        principalTable: "CertificateRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateRequests_LinkedBirthRecordId",
                table: "CertificateRequests",
                column: "LinkedBirthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateRequests_RequestedByUserId",
                table: "CertificateRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateRequests_RequestNumber",
                table: "CertificateRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_BirthRecordId",
                table: "Certificates",
                column: "BirthRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateRequestId",
                table: "Certificates",
                column: "CertificateRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CreatedByUserId",
                table: "Certificates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_VerificationCode",
                table: "Certificates",
                column: "VerificationCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CertificateRequests");
        }
    }
}
