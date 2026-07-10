using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGCN.Application.DTOs.Certificates;
using SGCN.Application.Interfaces;
using SGCN.Domain.Enums;

namespace SGCN.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    private const string CertificateTemplateName = "certificate";

    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePdfAsync(
        string templateName,
        object model,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(templateName, CertificateTemplateName, StringComparison.OrdinalIgnoreCase) ||
            model is not CertificateResponse certificate)
        {
            throw new ArgumentException("The PDF template or model is not supported.", nameof(templateName));
        }

        var document = Document.Create(documentContainer =>
        {
            documentContainer.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(style => style.FontSize(11));

                page.Header().Column(header =>
                {
                    header.Item()
                        .AlignCenter()
                        .Text("SGCN")
                        .Bold()
                        .FontSize(24)
                        .FontColor(Colors.Blue.Darken3);
                    header.Item()
                        .PaddingTop(4)
                        .AlignCenter()
                        .Text("Système de Gestion des Certificats de Naissance")
                        .SemiBold()
                        .FontSize(13);
                });

                page.Content().PaddingVertical(24).Column(column =>
                {
                    column.Spacing(9);

                    column.Item()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten1)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(12)
                        .AlignCenter()
                        .Text($"CERTIFICAT DE NAISSANCE — {certificate.CertificateNumber}")
                        .Bold()
                        .FontSize(15);

                    AddField("Prénom de l'enfant", certificate.ChildFirstName);
                    AddField("Nom de l'enfant", certificate.ChildLastName);
                    AddField("Sexe", FormatGender(certificate.Gender));
                    AddField("Date de naissance", certificate.BirthDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    AddField("Heure de naissance", certificate.BirthTime.ToString("HH:mm", CultureInfo.InvariantCulture));
                    AddField("Lieu de naissance", certificate.BirthPlace);
                    AddField("Hôpital", certificate.HospitalName);
                    AddField("Commune", certificate.CommuneName);
                    AddField("Département", certificate.DepartmentName);
                    AddField("Nom complet de la mère", certificate.MotherFullName);
                    AddField("Nom complet du père", string.IsNullOrWhiteSpace(certificate.FatherFullName)
                        ? "Non renseigné"
                        : certificate.FatherFullName);
                    AddField("Identifiant SGCN de l'acte", certificate.SgcnId);
                    AddField("Code de vérification", certificate.VerificationCode);
                    AddField("Date d'émission", certificate.CreatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

                    column.Item()
                        .PaddingTop(12)
                        .Border(1)
                        .BorderColor(Colors.Blue.Lighten2)
                        .Background(Colors.Blue.Lighten5)
                        .Padding(12)
                        .Text($"Ce certificat peut être vérifié à l'aide du code de vérification {certificate.VerificationCode}.")
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken3);

                    void AddField(string label, string value)
                    {
                        column.Item().Row(row =>
                        {
                            row.ConstantItem(175).Text(label).SemiBold().FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().Text(value);
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("SGCN — Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        });

        var pdf = document.GeneratePdf();
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(pdf);
    }

    private static string FormatGender(Gender gender)
    {
        return gender == Gender.Female ? "Féminin" : "Masculin";
    }
}
