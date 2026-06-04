using System.IO;
using HumanPlus.Application.Interfaces;
using HumanPlus.Domain.Entities.Candidates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HumanPlus.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        static DocumentService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> GenerateCandidateRegistrationFormAsync(Candidate candidate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().AlignCenter().Text("HumanPlus Manpower Consulting").Bold().FontSize(16);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("Candidate Registration Form").Bold().FontSize(13);
                        col.Item().PaddingVertical(2).LineHorizontal(1);

                        // 1. Personal Information
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(leftCol =>
                            {
                                leftCol.Item().PaddingVertical(3).Text("1. Personal Information").Bold().FontSize(11);
                                AddField(leftCol, "Full Name", candidate.User?.FullName);
                                AddField(leftCol, "Father's Name", candidate.FatherName);
                                AddField(leftCol, "Mother's Name", candidate.MotherName);
                                AddField(leftCol, "Gender", candidate.Gender.ToString());
                                AddField(leftCol, "Date of Birth", candidate.DateOfBirth?.ToString("dd-MMM-yyyy"));
                                AddField(leftCol, "Marital Status", candidate.MaritalStatus?.ToString());
                                AddField(leftCol, "Blood Group", candidate.BloodGroup);
                                AddField(leftCol, "Aadhaar Number", candidate.AadhaarNumber);
                                AddField(leftCol, "PAN Number", candidate.PanNumber);
                                AddField(leftCol, "Mobile", candidate.User?.PhoneNumber);
                                AddField(leftCol, "Email", candidate.User?.Email);
                                AddField(leftCol, "Alternate Mobile", candidate.AlternateMobile);
                                AddField(leftCol, "Languages Known", candidate.LanguagesKnown);
                            });

                            if (!string.IsNullOrEmpty(candidate.ProfileImagePath))
                            {
                                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", candidate.ProfileImagePath.TrimStart('/'));
                                if (File.Exists(imgPath))
                                {
                                    row.ConstantItem(110).AlignTop().Image(imgPath).FitWidth();
                                }
                            }
                        });

                        // 2. Address Details
                        col.Item().PaddingVertical(3).Text("2. Address Details").Bold().FontSize(11);
                        AddField(col, "Current Address", candidate.CurrentAddress);
                        AddField(col, "Permanent Address", candidate.PermanentAddress);
                        AddField(col, "State", candidate.State?.Name);
                        AddField(col, "District", candidate.District?.Name);
                        AddField(col, "PIN Code", candidate.PinCode);

                        // 3. Experience & Previous Employment
                        col.Item().PaddingVertical(3).Text("3. Experience & Previous Employment").Bold().FontSize(11);
                        if (candidate.IsFresher)
                        {
                            AddField(col, "Type", "Fresher");
                        }
                        else
                        {
                            AddField(col, "Type", "Experienced");
                            AddField(col, "Total Experience (Years)", candidate.TotalExperienceYears?.ToString());
                            AddField(col, "Previous Employer", candidate.PreviousEmployer);
                            AddField(col, "Previous Designation", candidate.PreviousDesignation);
                            AddField(col, "Previous Salary (Annual)", candidate.PreviousSalary?.ToString("N0"));
                            AddField(col, "Previous Industry", candidate.PreviousIndustry?.Name);
                        }

                        // 4. Job Preferences
                        col.Item().PaddingVertical(3).Text("4. Job Preferences").Bold().FontSize(11);
                        AddField(col, "Preferred Industry", candidate.PreferredIndustry?.Name);
                        AddField(col, "Preferred Location", candidate.PreferredLocation);
                        AddField(col, "Expected Salary (Annual)", candidate.ExpectedSalary?.ToString("N0"));
                        AddField(col, "Shift Preference", candidate.ShiftPreference?.ToString());
                        AddField(col, "Employment Type", candidate.PreferredEmploymentType?.ToString());
                        AddField(col, "Willing to Relocate", candidate.WillingToRelocate == true ? "Yes" : "No");

                        // 5. Skills
                        col.Item().PaddingVertical(3).Text("5. Skills").Bold().FontSize(11);
                        if (candidate.Skills != null && candidate.Skills.Any())
                        {
                            var skillNames = string.Join(", ", candidate.Skills.Where(s => s.Skill != null).Select(s => s.Skill!.Name));
                            AddField(col, "Skills", skillNames);
                        }
                        else
                        {
                            AddField(col, "Skills", "None");
                        }

                        // 6. Education
                        col.Item().PaddingVertical(3).Text("6. Education").Bold().FontSize(11);
                        if (candidate.Educations != null && candidate.Educations.Any())
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Qualification").Bold().FontSize(9);
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Board / University").Bold().FontSize(9);
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Passing Year").Bold().FontSize(9);
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Percentage/Grade").Bold().FontSize(9);
                                });
                                foreach (var edu in candidate.Educations)
                                {
                                    table.Cell().Padding(2).Text(edu.Qualification?.Name ?? "N/A").FontSize(9);
                                    table.Cell().Padding(2).Text(edu.BoardOrUniversity ?? "N/A").FontSize(9);
                                    table.Cell().Padding(2).Text(edu.PassingYear?.ToString() ?? "N/A").FontSize(9);
                                    table.Cell().Padding(2).Text(edu.PercentageOrGrade ?? "N/A").FontSize(9);
                                }
                            });
                        }
                        else
                        {
                            AddField(col, "Education", "None");
                        }

                        // 7. Uploaded Documents
                        col.Item().PaddingVertical(3).Text("7. Uploaded Documents").Bold().FontSize(11);
                        if (candidate.Documents != null && candidate.Documents.Any())
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(4);
                                    c.RelativeColumn(2);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Type").Bold().FontSize(9);
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("File Name").Bold().FontSize(9);
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Uploaded On").Bold().FontSize(9);
                                });
                                foreach (var doc in candidate.Documents)
                                {
                                    table.Cell().Padding(2).Text(doc.DocumentType).FontSize(9);
                                    table.Cell().Padding(2).Text(doc.OriginalFileName ?? "N/A").FontSize(9);
                                    table.Cell().Padding(2).Text(doc.UploadedAt.ToString("dd-MMM-yyyy")).FontSize(9);
                                }
                            });
                        }
                        else
                        {
                            AddField(col, "Documents", "None uploaded");
                        }

                        // 8. Declaration
                        col.Item().PaddingVertical(4).LineHorizontal(1);
                        col.Item().PaddingVertical(3).Text("8. Declaration").Bold().FontSize(11);
                        col.Item().PaddingVertical(3).Text("I hereby declare that the information provided by me in this registration form and all uploaded documents is true, complete, and correct to the best of my knowledge. I understand that any false information may result in disqualification or termination of my registration.").FontSize(9).Italic();

                        col.Item().PaddingVertical(8).AlignRight().Column(decCol =>
                        {
                            decCol.Item().Text($"Date: {DateTime.Now:dd MMMM yyyy}").FontSize(10);
                            decCol.Item().PaddingTop(8).Text("_________________________").FontSize(10).AlignRight();
                            decCol.Item().Text($"Candidate Signature").FontSize(9).AlignRight();
                            decCol.Item().PaddingTop(4).Text($"Name: {candidate.User?.FullName ?? "N/A"}").FontSize(10).AlignRight();
                        });

                        col.Item().PaddingVertical(3).LineHorizontal(1);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on: ");
                        x.Span($"{DateTime.Now:dd-MMM-yyyy HH:mm}");
                        x.Span("  |  HumanPlus Manpower Consulting");
                    });
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }

        private static void AddField(ColumnDescriptor col, string label, string? value)
        {
            col.Item().Text(text =>
            {
                text.Span($"{label}: ").Bold().FontSize(9);
                text.Span(value ?? "N/A").FontSize(9);
            });
        }

        public Task<byte[]> GenerateReceiptAsync(string receiptNumber, string userName, decimal amount, string description)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);

                    page.Header().AlignCenter().Text("HumanPlus Manpower Consulting").Bold().FontSize(14);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("RECEIPT").Bold().FontSize(12);
                        col.Item().PaddingVertical(3).LineHorizontal(1);
                        col.Item().Text($"Receipt No: {receiptNumber}");
                        col.Item().Text($"Date: {DateTime.Now:dd-MMM-yyyy}");
                        col.Item().Text($"Received from: {userName}");
                        col.Item().Text($"Amount: Rs. {amount:N2}");
                        col.Item().Text($"Description: {description}");
                    });
                    page.Footer().AlignCenter().Text("Thank you for using HumanPlus!");
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }

        public Task<byte[]> GenerateInvoiceAsync(string invoiceNumber, string companyName, decimal amount, string description)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);

                    page.Header().AlignCenter().Text("HumanPlus Manpower Consulting").Bold().FontSize(14);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("INVOICE").Bold().FontSize(12);
                        col.Item().PaddingVertical(3).LineHorizontal(1);
                        col.Item().Text($"Invoice No: {invoiceNumber}");
                        col.Item().Text($"Date: {DateTime.Now:dd-MMM-yyyy}");
                        col.Item().Text($"Bill To: {companyName}");
                        col.Item().Text($"Amount: Rs. {amount:N2}");
                        col.Item().Text($"Description: {description}");
                    });
                    page.Footer().AlignCenter().Text("Thank you for your business!");
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
    }
}
