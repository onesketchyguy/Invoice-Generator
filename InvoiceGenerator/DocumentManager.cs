using System;
using System.IO;
using System.Collections.Generic;

// Deprecated
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

/* New
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
 */

namespace InvoiceGenerator
{
    public class DocumentManager
    {
        public Dictionary<string, double> workLogs = new Dictionary<string, double>();

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

        public void CreateDocument(double totalHoursWorked)
        {
            double totalCharge = totalHoursWorked * data.chargePerHour;

            var lineSeparator = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
            var line = new Chunk(lineSeparator);

            void SetFont(ref Paragraph paragraph, bool bold, int size, bool accentColor = false)
            {
                paragraph.Font = FontFactory.GetFont(bold ? FontFactory.HELVETICA_BOLD : FontFactory.HELVETICA, size, accentColor ? BaseColor.DARK_GRAY : BaseColor.BLACK);
            }

            void AddParagraph(ref Paragraph paragraph, ref Document document)
            {
                document.Add(paragraph);
                paragraph.Clear();
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 10, 10, 10, 10);

                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                Paragraph paragraph = new Paragraph();
                paragraph.IndentationLeft = 20;
                paragraph.IndentationRight = 20;
                paragraph.Alignment = Element.ALIGN_RIGHT;

                // Date the document
                SetFont(ref paragraph, false, 8, true);
                paragraph.Add($"DATE ");
                SetFont(ref paragraph, true, 10);
                paragraph.Add($"{DateTime.Now.ToString("dd")}-{DateTime.Now.ToString("MMM")}-{DateTime.Now.Year}");

                AddParagraph(ref paragraph, ref document);

                SetFont(ref paragraph, true, 10, true);
                paragraph.Add($"INVOICE_{data.invoiceNumber}");

                AddParagraph(ref paragraph, ref document);

                document.Add(line);

                // Detail who is billing
                paragraph.Alignment = Element.ALIGN_LEFT;

                SetFont(ref paragraph, false, 10, true);
                paragraph.Add("BILL FROM\n");

                SetFont(ref paragraph, true, 16);
                paragraph.Add(data.biller);

                AddParagraph(ref paragraph, ref document);

                SetFont(ref paragraph, false, 14);
                paragraph.Add(data.billerAddress);
                paragraph.Add(data.billerContact);

                AddParagraph(ref paragraph, ref document);

                document.Add(line);

                // Explain who we're billing
                SetFont(ref paragraph, false, 10, true);
                paragraph.Add("BILL TO\n");

                SetFont(ref paragraph, true, 16);
                paragraph.Add(data.billing);

                AddParagraph(ref paragraph, ref document);

                SetFont(ref paragraph, false, 14);
                paragraph.Add(data.billingAddress);
                paragraph.Add(data.billingContact);

                AddParagraph(ref paragraph, ref document);

                document.Add(line);
                document.Add(new Chunk("\n"));

                // Add work items
                paragraph.Clear();
                SetFont(ref paragraph, true, 12, true);
                paragraph.Alignment = Element.ALIGN_CENTER;

                document.Add(new Chunk("\n"));

                paragraph.Add("WORK ITEMS ARE AS FOLLOWS:");

                AddParagraph(ref paragraph, ref document);

                paragraph.SpacingBefore = 10;
                paragraph.SpacingAfter = 10;
                paragraph.Alignment = Element.ALIGN_LEFT;

                foreach (var item in workLogs)
                {
                    SetFont(ref paragraph, true, 11);
                    paragraph.Add($"•{item.Key}");

                    SetFont(ref paragraph, false, 10);
                    paragraph.Add($"   for   ");

                    SetFont(ref paragraph, true, 11);
                    paragraph.Add($"{item.Value}");

                    SetFont(ref paragraph, true, 10);
                    paragraph.Add($"   hours.\n");
                }

                AddParagraph(ref paragraph, ref document);

                // Display costs
                paragraph.SpacingBefore = 20;
                paragraph.SpacingAfter = 20;
                paragraph.Alignment = Element.ALIGN_RIGHT;

                SetFont(ref paragraph, true, 12, true);
                paragraph.Add($"TOTAL WORK HOURS: ");
                SetFont(ref paragraph, true, 15);
                paragraph.Add($"{totalHoursWorked}\n");

                SetFont(ref paragraph, true, 12, true);
                paragraph.Add($"CHARGE PER HOUR: ");
                SetFont(ref paragraph, true, 15);
                paragraph.Add($"${data.chargePerHour}\n");

                SetFont(ref paragraph, true, 12, true);
                paragraph.Add($"SUBTOTAL: ");
                SetFont(ref paragraph, true, 15);
                paragraph.Add($"${totalCharge}\n");

                AddParagraph(ref paragraph, ref document);

                document.Add(line);

                paragraph.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 25f, BaseColor.RED);
                paragraph.Add($"BALANCE DUE: ${totalCharge}");

                document.Add(paragraph);

                // Add trade marks
                document.AddAuthor("Forrest Lowe 2020");
                document.AddCreator("Forrest Lowe 2020");
                document.AddCreationDate();
                document.AddTitle($"Invoice_{data.invoiceNumber}");
                document.AddSubject($"Invoice_{data.invoiceNumber}");
                document.AddLanguage("English(US)");

                // Finish off and close the document
                document.Close();
                byte[] bytes = memoryStream.ToArray();
                File.WriteAllBytes(PDF_DIRECTORY, bytes);

                memoryStream.Close();
            }
        }
    }
}