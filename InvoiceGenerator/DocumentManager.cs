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
    public class DocumentManager
    {
        public Dictionary<string, double> workLogs = new Dictionary<string, double>();

        public float FontSize = 20;

        private PdfFont standardFont;
        private PdfFont boldFont;

        private bool fontsInitialized;

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

        public BillingObject data;

        public string PDF_FILENAME
        {
            get
            {
                return $"Invoice_{data.invoiceNumber}.pdf";
            }
        }

        public string PDF_DIRECTORY
        {
            get
            {
                if (Directory.Exists(data.invoiceDirectory) == false)
                    Directory.CreateDirectory(data.invoiceDirectory);

                return $"{data.invoiceDirectory + '\\' + PDF_FILENAME}";
            }
        }

        public void CheckInvoiceDirectory()
        {
            // Check invoice number
            while (true)
            {
                if (File.Exists(PDF_DIRECTORY))
                    data.invoiceNumber++;
                else break;
            }
        }

        private void SetFont(ref Paragraph paragraph, bool bold, float size, bool accentColor = false)
        {
            SetFont(ref paragraph, bold, size, accentColor ? ColorConstants.DARK_GRAY : ColorConstants.BLACK);
        }

        private void SetFont(ref Paragraph paragraph, bool bold, float size, Color color)
        {
            if (!fontsInitialized) InitializeFonts();

            paragraph.SetFontSize(FontSize * size);
            paragraph.SetFontColor(color);

            paragraph.SetFont(bold ? boldFont : standardFont);
        }

        private void AddParagraph(ref Paragraph paragraph, ref Document document)
        {
            document.Add(paragraph);
            paragraph = new Paragraph();
        }

        private void AddWorkItems(ref Document document)
        {
            // Table
            Table table = new Table(2, true);

            Cell cellHeaderLeft = new Cell(1, 1)
               .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
               .SetTextAlignment(TextAlignment.CENTER)
               .Add(new Paragraph("DESCRIPTION OF WORK"));
            Cell cellHeaderRight = new Cell(1, 1)
               .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
               .SetTextAlignment(TextAlignment.CENTER)
               .Add(new Paragraph("HOURS WORKED"));

            table.AddCell(cellHeaderLeft);
            table.AddCell(cellHeaderRight);

            foreach (var item in workLogs)
            {
                Cell left = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .Add(new Paragraph($"{item.Key}"));
                Cell right = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .Add(new Paragraph($"{item.Value}"));

                table.AddCell(left);
                table.AddCell(right);
            }

            document.Add(table);

            /*
            var paragraph = new Paragraph();
            SetFont(ref paragraph, true, 0.6f, true);
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
                SetFont(ref paragraph, true, 0.55f);
                paragraph.Add($"•{item.Key}");

                SetFont(ref paragraph, false, 0.5f);
                paragraph.Add($"   for   ");

                SetFont(ref paragraph, true, 0.55f);
                paragraph.Add($"{item.Value}");

                SetFont(ref paragraph, true, 0.5f);
                paragraph.Add($"   hours.\n");
            }

            AddParagraph(ref paragraph, ref document);*/
        }

        public void CreateDocument(double totalHoursWorked)
        {
            double totalCharge = Math.Round(totalHoursWorked * data.chargePerHour, 2);

            var lineSeparator = new LineSeparator(new SolidLine());

            // Open the document
            PdfWriter writer = new PdfWriter(PDF_DIRECTORY);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf, PageSize.A4);

            Paragraph paragraph = new Paragraph();
            paragraph.SetBorder(Border.NO_BORDER);
            paragraph.SetMarginLeft(20);
            paragraph.SetMarginRight(20);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);

            // Date the document
            SetFont(ref paragraph, false, 0.4f, true);
            paragraph.Add($"DATE ");
            SetFont(ref paragraph, true, 0.5f);
            paragraph.Add($"{DateTime.Now.ToString("dd")}-{DateTime.Now.ToString("MMM")}-{DateTime.Now.Year}");

            AddParagraph(ref paragraph, ref document);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);

            SetFont(ref paragraph, true, 0.5f, true);
            paragraph.Add($"INVOICE_{data.invoiceNumber}");

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);

            // Detail who is billing
            paragraph.SetTextAlignment(TextAlignment.LEFT);

            SetFont(ref paragraph, false, 0.5f, true);
            paragraph.Add("BILL FROM");
            AddParagraph(ref paragraph, ref document);

            SetFont(ref paragraph, true, .8f);
            paragraph.Add(data.biller);

            AddParagraph(ref paragraph, ref document);

            SetFont(ref paragraph, false, .7f);
            paragraph.Add(data.billerAddress);
            paragraph.Add(data.billerContact);

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);

            // Explain who we're billing
            SetFont(ref paragraph, false, 0.5f, true);
            paragraph.Add("BILL TO");
            AddParagraph(ref paragraph, ref document);

            SetFont(ref paragraph, true, .8f);
            paragraph.Add(data.billing);

            AddParagraph(ref paragraph, ref document);

            SetFont(ref paragraph, false, .7f);
            paragraph.Add(data.billingAddress);
            paragraph.Add(data.billingContact);

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

            SetFont(ref paragraph, true, 0.6f, true);
            paragraph.Add($"TOTAL WORK HOURS: ");
            SetFont(ref paragraph, true, 0.75f);
            paragraph.Add($"{totalHoursWorked}\n");

            SetFont(ref paragraph, true, 0.6f, true);
            paragraph.Add($"CHARGE PER HOUR: ");
            SetFont(ref paragraph, true, 0.75f);
            paragraph.Add($"${data.chargePerHour}\n");

            SetFont(ref paragraph, true, 0.6f, true);
            paragraph.Add($"SUBTOTAL: ");
            SetFont(ref paragraph, true, 0.75f);
            paragraph.Add($"${totalCharge}\n");

            AddParagraph(ref paragraph, ref document);

            document.Add(lineSeparator);

            // Add an empty line
            AddParagraph(ref paragraph, ref document);

            paragraph.SetTextAlignment(TextAlignment.RIGHT);
            SetFont(ref paragraph, true, 1.0f, ColorConstants.RED);
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