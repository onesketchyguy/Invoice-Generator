// Forrest Lowe 2020-2021
using System;
using System.Collections.Generic;
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace InvoiceGenerator
{
    public enum WorkItemDisplayType
    {
        table,
        bullets,
    }

    public class DocumentManager
    {
        public Dictionary<string, double> workLogs = new Dictionary<string, double>();

        public float FONT_SIZE { get; private set; } = 20;

        public WorkItemDisplayType workItemDisplayType { get; private set; } = WorkItemDisplayType.table;

        private PdfFont standardFont;
        private PdfFont boldFont;

        private Style fontStyle = new Style();
        private bool fontsInitialized;

        public void SetFontSize(int size)
        {
            FONT_SIZE = size;
        }

        public void SetWorkItemDisplayType(WorkItemDisplayType type)
        {
            workItemDisplayType = type;
        }

        private void InitializeFonts()
        {
            standardFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            fontsInitialized = true;
        }

        public void AddWorkLog(string description, double value)
        {
            if (workLogs.ContainsKey(description))
            {
                workLogs[description] += value;
            }
            else workLogs.Add(description, value);
        }

        private Client client;
        private Contractor contractor;

        public void SetData(ref Client client, ref Contractor contractor)
        {
            this.client = client;
            this.contractor = contractor;
        }

        public string PDF_FILENAME
        {
            get
            {
                return $"Invoice_{client.invoiceNumber}.pdf";
            }
        }

        public string PDF_DIRECTORY
        {
            get
            {
                if (Directory.Exists(contractor.invoiceDirectory) == false)
                    Directory.CreateDirectory(contractor.invoiceDirectory);

                return $"{contractor.invoiceDirectory + '\\' + PDF_FILENAME}";
            }
        }

        public int CheckInvoiceDirectory()
        {
            // Check invoice number
            while (true)
            {
                if (File.Exists(PDF_DIRECTORY))
                    client.invoiceNumber++;
                else break;
            }

            return client.invoiceNumber;
        }

        private void SetFont(bool bold, float size, bool accentColor = false)
        {
            SetFont(bold, size, accentColor ? ColorConstants.DARK_GRAY : ColorConstants.BLACK);
        }

        private void SetFont(bool bold, float size, Color color)
        {
            if (!fontsInitialized) InitializeFonts();

            fontStyle.SetFontSize(FONT_SIZE * size);
            fontStyle.SetFontColor(color);

            fontStyle.SetFont(bold ? boldFont : standardFont);
        }

        private void AddParagraph(ref Paragraph paragraph, ref Document document)
        {
            document.Add(paragraph.AddStyle(fontStyle));
            paragraph = new Paragraph();
        }

        private void AddWorkItems(ref Document document)
        {
            switch (workItemDisplayType)
            {
                case WorkItemDisplayType.table:
                    // Table
                    Table table = new Table(2, true);

                    Cell cellHeaderLeft = new Cell(1, 1)
                       .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph("DESCRIPTION OF WORK").SetFont(boldFont));
                    Cell cellHeaderRight = new Cell(1, 1)
                       .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph("HOURS WORKED").SetFont(boldFont));

                    table.AddCell(cellHeaderLeft);
                    table.AddCell(cellHeaderRight);

                    foreach (var item in workLogs)
                    {
                        Cell left = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .Add(new Paragraph($"{item.Key}").SetFont(standardFont));
                        Cell right = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .Add(new Paragraph($"{item.Value}").SetFont(standardFont));

                        table.AddCell(left);
                        table.AddCell(right);
                    }

                    document.Add(table);
                    break;

                case WorkItemDisplayType.bullets:
                    // Bullets
                    var paragraph = new Paragraph();

                    SetFont(true, 0.6f, true);
                    paragraph.SetTextAlignment(TextAlignment.CENTER);

                    document.Add(new Cell());

                    paragraph.Add("WORK ITEMS ARE AS FOLLOWS:");

                    AddParagraph(ref paragraph, ref document);

                    paragraph.SetMarginLeft(10);
                    paragraph.SetMarginRight(10);
                    paragraph.SetTextAlignment(TextAlignment.LEFT);

                    // Add all work items
                    foreach (var item in workLogs)
                    {
                        SetFont(true, 0.55f);
                        paragraph.Add($"•{item.Key}");

                        SetFont(false, 0.5f);
                        paragraph.Add($"   for   ");

                        SetFont(true, 0.55f);
                        paragraph.Add($"{item.Value}");

                        SetFont(true, 0.5f);
                        paragraph.Add($"   hours.\n");
                    }

                    AddParagraph(ref paragraph, ref document);
                    break;

                default:
                    break;
            }
        }

        public string Format(string input)
        {
            var val = "";

            for (int i = 0; i < input.Length; i++)
            {
                // Check for a new line character and format appropriatly.
                if (input[i] == '\\' && i + 1 < input.Length && input[i + 1] == 'n')
                {
                    val += '\n';
                    i++; // Skip the next index
                }
                else val += input[i];
            }

            return val;
        }

        public void CreateDocument(double totalHoursWorked)
        {
            contractor.address = Format(contractor.address);
            client.address = Format(client.address);

            double totalCharge = Math.Round(totalHoursWorked * client.chargePerHour, 2);

            var lineSeparator = new LineSeparator(new SolidLine());

            // Open the document
            PdfWriter writer = new PdfWriter(PDF_DIRECTORY);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf, PageSize.A4);

            document.SetMargins(20, 20, 20, 20);

            Paragraph paragraph = new Paragraph();
            paragraph.SetBorder(Border.NO_BORDER);
            paragraph.SetMarginLeft(20);
            paragraph.SetMarginRight(20);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);

            // Date the document
            SetFont(false, 0.4f, true);
            paragraph.Add($"DATE ");
            SetFont(true, 0.5f);
            paragraph.Add($"{DateTime.Now.ToString("dd")}-{DateTime.Now.ToString("MMM")}-{DateTime.Now.Year}");

            AddParagraph(ref paragraph, ref document);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);

            SetFont(true, 0.5f, true);
            paragraph.Add($"INVOICE_{client.invoiceNumber}");

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);

            // Detail who is billing
            paragraph.SetTextAlignment(TextAlignment.LEFT);

            SetFont(false, 0.5f, true);
            paragraph.Add("BILL FROM");
            AddParagraph(ref paragraph, ref document);

            SetFont(true, .8f);
            paragraph.Add(contractor.name);

            AddParagraph(ref paragraph, ref document);

            SetFont(false, .7f);
            paragraph.Add(contractor.address);
            paragraph.Add(contractor.contact);

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);

            // Explain who we're billing
            SetFont(false, 0.5f, true);
            paragraph.Add("BILL TO");
            AddParagraph(ref paragraph, ref document);

            SetFont(true, .8f);
            paragraph.Add(client.name);

            AddParagraph(ref paragraph, ref document);

            SetFont(false, .7f);
            paragraph.Add(client.address);
            paragraph.Add(client.contact);

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);
            document.Add(new Cell());

            // Add work items
            AddWorkItems(ref document);

            document.Add(lineSeparator);

            // Display costs
            paragraph.SetMarginTop(20);
            paragraph.SetMarginBottom(20);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);

            SetFont(true, 0.6f, true);
            paragraph.Add($"TOTAL WORK HOURS: ");
            SetFont(true, 0.75f);
            paragraph.Add($"{totalHoursWorked}\n");

            SetFont(true, 0.6f, true);
            paragraph.Add($"CHARGE PER HOUR: ");
            SetFont(true, 0.75f);
            paragraph.Add($"${client.chargePerHour}\n");

            SetFont(true, 0.6f, true);
            paragraph.Add($"SUBTOTAL: ");
            SetFont(true, 0.75f);
            paragraph.Add($"${totalCharge}\n");

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);

            // Add an empty line
            AddParagraph(ref paragraph, ref document);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);
            SetFont(true, 1.0f, ColorConstants.RED);
            paragraph.Add($"BALANCE DUE: ${totalCharge}");

            AddParagraph(ref paragraph, ref document);

            // Add trade marks
            /*document.AddAuthor("Forrest Lowe 2020-2021");
            document.AddCreator("Forrest Lowe 2020-2021");
            document.AddCreationDate();
            document.AddTitle($"Invoice_{data.invoiceNumber}");
            document.AddSubject($"Invoice_{data.invoiceNumber}");
            document.AddLanguage("English(US)");*/

            // Finish off and close the document
            document.Close();
        }
    }
}