using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using InvoiceGenerator.InputPropmts;
using System.Linq;

namespace InvoiceGenerator
{
    public class DisplayWindow : Form
    {
        private DocumentManager document;
        private JsonManager saveManager;
        private BillingObject data;

        private const string EXPORT = "Export PDF";
        private const string EXPORT_LOCATION = "Change export location";
        private const string SAVE_WORKITEMS = "Save current work items";
        private const string LOAD_WORKITEMS = "Load last work items";
        private const string MODIFY_CONTACT_INFO = "Change billing info";

        public static string Format(string input)
        {
            var val = "";

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\\' && i + 1 < input.Length && input[i + 1] == 'n')
                {
                    val += '\n';
                    i++; // Skip the next index
                }
                else val += input[i];
            }

            return val;
        }

        // Initialization
        public DisplayWindow(DocumentManager document, JsonManager jsonManager)
        {
            this.document = document;
            saveManager = jsonManager;

            data = new BillingObject();

            this.Size = new Size(800, 550);
            this.UseWaitCursor = false;
            this.BackColor = Color.AliceBlue;

            // Toolbar
            var toolbar = new ToolStrip();
            toolbar.Parent = this;
            toolbar.Items.Add(EXPORT);
            toolbar.Items.Add(EXPORT_LOCATION);
            toolbar.Items.Add(MODIFY_CONTACT_INFO);
            toolbar.Items.Add(SAVE_WORKITEMS);
            toolbar.Items.Add(LOAD_WORKITEMS);

            toolbar.ItemClicked += Toolbar_ItemClicked;

            // Input box
            var descriptionBoxHeader = new TextBox();
            descriptionBoxHeader.Parent = this;
            descriptionBoxHeader.Enabled = false;
            descriptionBoxHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            descriptionBoxHeader.Size = new Size(300, 50);
            descriptionBoxHeader.Location = new Point(0, 25);
            descriptionBoxHeader.PlaceholderText = "Description of work";
            descriptionBoxHeader.ForeColor = Color.Black;

            inputBox = new TextBox();
            inputBox.Parent = this;
            inputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            inputBox.Size = new Size(300, 50);
            inputBox.Location = new Point(0, 50);
            inputBox.PlaceholderText = "Description";
            inputBox.ForeColor = Color.Black;

            // Work hours input
            var workBoxHeader = new TextBox();
            workBoxHeader.Parent = this;
            workBoxHeader.Enabled = false;
            workBoxHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            workBoxHeader.Size = new Size(300, 75);
            workBoxHeader.Location = new Point(0, 75);
            workBoxHeader.PlaceholderText = "Worked hours";
            workBoxHeader.ForeColor = Color.Black;

            workedHours = new TextBox();
            workedHours.Parent = this;
            workedHours.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            workedHours.Size = new Size(300, 50);
            workedHours.Location = new Point(0, 100);
            workedHours.PlaceholderText = "0.0";
            workedHours.ForeColor = Color.Black;
            workedHours.TextChanged += WorkedHours_TextChanged;

            // Show work
            workedItemsDisplay = new FlowLayoutPanel();
            workedItemsDisplay.Parent = this;
            workedItemsDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            workedItemsDisplay.Size = new Size(200, 300);
            workedItemsDisplay.Location = new Point(600, 25);
            workedItemsDisplay.ForeColor = Color.Black;
            workedItemsDisplay.BackColor = Color.LightBlue;

            // Add work button
            var addWorkButton = new Button();
            addWorkButton.Parent = workedItemsDisplay;
            addWorkButton.Size = new Size(180, 50);
            addWorkButton.Text = "Add work";
            addWorkButton.ForeColor = Color.Black;
            addWorkButton.BackColor = Color.Lime;

            addWorkButton.Click += (object sender, EventArgs e) => AddWork();

            // Remove work button
            removeWorkButton = new Button();
            removeWorkButton.Parent = this;
            removeWorkButton.Enabled = false;
            removeWorkButton.Visible = false;
            removeWorkButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            removeWorkButton.Location = new Point(480, 80);
            removeWorkButton.Size = new Size(100, 35);
            removeWorkButton.Text = "Delete work log";
            removeWorkButton.ForeColor = Color.Black;
            removeWorkButton.BackColor = Color.IndianRed;

            removeWorkButton.Click += (object sender, EventArgs e) =>
            {
                removeWorkButton.Enabled = false;
                removeWorkButton.Visible = false;

                RemoveWork(inputBox.Text);
            };

            // Start app
            var formContext = new MultiFormContext(this);

            // Start
            var loadData = saveManager.LoadBilling();
            if (loadData != null)
            {
                // Get data
                data = loadData;

                if (string.IsNullOrEmpty(data.invoiceDirectory))
                    SetExportLocation();
            }
            else // First launch
            {
                formContext = new MultiFormContext(this,
                    new FirstLaunchSurvey(new BillingObject(), (BillingObject recievedData) =>
                    {
                        data = recievedData;
                        SetExportLocation();
                    }));
            }

            LoadWorkItems();

            Application.Run(formContext);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveWorkItems();

            base.OnFormClosing(e);
        }

        private void Toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            WriteLine(e.ClickedItem.Text);

            switch (e.ClickedItem.Text)
            {
                case EXPORT:
                    ExportPDF_Click(sender, e);
                    break;

                case EXPORT_LOCATION:
                    SetExportLocation();
                    break;

                case SAVE_WORKITEMS:
                    SaveWorkItems();
                    break;

                case LOAD_WORKITEMS:
                    LoadWorkItems();
                    break;

                case MODIFY_CONTACT_INFO:
                    var survey = new FirstLaunchSurvey(data, (BillingObject recievedData) =>
                    {
                        data = recievedData;
                    });

                    survey.Show();
                    break;

                default:
                    break;
            }
        }

        private void WorkedHours_TextChanged(object sender, EventArgs e)
        {
            // Get hours input
            var input = ((TextBox)sender).Text;

            if (string.IsNullOrWhiteSpace(input))
                return;

            string chars = "";

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]) == true || input[i] == '.')
                {
                    chars += input[i];
                }
            }

            workedHours.Text = chars;
        }

        private void ExportPDF_Click(object sender, EventArgs e)
        {
            // Verify the invoice directory
            document.data = data;
            document.CheckInvoiceDirectory();

            // Format and print to invoice
            double totalHoursWorked = 0.0;
            foreach (var item in document.workLogs)
                totalHoursWorked += item.Value;

            data.billerAddress = Format(data.billerAddress);
            data.billingAddress = Format(data.billingAddress);

            document.CreateDocument(totalHoursWorked);

            // Open invoice and display to user
            //Process.Start(Directory.GetCurrentDirectory() + "/" + pdfFilename); TO BE IMPLEMENTED

            // Finished
            saveManager.SaveBilling(data);
            WriteLine($"Done.\nYou can find your invoice at {data.invoiceDirectory + document.PDF_FILENAME}.");
        }

        private void SetExportLocation()
        {
            var filePath = string.Empty;

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Pick a location to save invoices to.";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                    //Get the path of specified file
                    filePath = folderDialog.SelectedPath;
            }

            data.invoiceDirectory = filePath;
            saveManager.SaveBilling(data);
        }

        private void SaveWorkItems()
        {
            var length = document.workLogs.Count;
            var data = new DataObject();
            data.description = new string[length];
            data.value = new double[length];

            int i = 0;

            foreach (var item in document.workLogs)
            {
                data.description[i] = item.Key;
                data.value[i] = item.Value;

                i++;
            }

            saveManager.SaveData(data);
        }

        private void LoadWorkItems()
        {
            var load = saveManager.LoadData();

            if (load == null || load.description == null) return;

            for (int i = 0; i < load.description.Length; i++)
            {
                // Remove any existing logs
                RemoveWork(load.description[i]);

                // Add new logs
                AddWorkToWorkList(load.description[i], load.value[i]);
            }
        }

        // Need to be able to display text
        public void WriteLine(string text)
        {
            this.Text = text;
            this.Update();
        }

        // Need a button region for adding work
        private Button removeWorkButton;

        private TextBox inputBox;

        private TextBox workedHours;

        public void AddWork()
        {
            if (string.IsNullOrWhiteSpace(workedHours.Text)) return;
            if (string.IsNullOrWhiteSpace(inputBox.Text)) return;

            double value = double.Parse(workedHours.Text);
            string description = inputBox.Text;

            // Add an item to the worked items display
            AddWorkToWorkList(description, value);
        }

        private void AddWorkToWorkList(string description, double value)
        {
            document.AddWorkLog(description, value);

            var list = workedItemsDisplay.Controls;
            for (int i = list.Count - 1; i > 0; i--)
            {
                var item = (Button)list[i];
                if (item.Text.Contains(description))
                {
                    // Edit item
                    var text = item.Text.Split(' ');

                    double oldValue = 0.0;

                    for (int j = text.Length - 1; j > 0; j--)
                    {
                        double.TryParse(text[j], out oldValue);

                        if (oldValue > 0) break;
                    }

                    value += oldValue;

                    item.Text = $"{description} for {Math.Round(value, 3)} hours";
                    return;
                }
            }

            var workItem = new Button();
            workItem.Parent = workedItemsDisplay;
            workItem.Anchor = AnchorStyles.Top;
            workItem.Size = new Size(180, 50);
            //workeItem.Location = new Point(400, 0);
            workItem.Text = $"{description} for {value} hours";
            workItem.ForeColor = Color.Black;
            workItem.BackColor = Color.White;

            int index = workedItemsDisplay.Controls.Count;

            // Remove this item on clicking it.
            workItem.Click += (object sender, EventArgs e) =>
            {
                inputBox.Text = description;
                removeWorkButton.Enabled = true;
                removeWorkButton.Visible = true;
            };
        }

        public void RemoveWork(string description)
        {
            if (document.workLogs.ContainsKey(description) == false) return;

            document.workLogs.Remove(description);

            var list = workedItemsDisplay.Controls;
            for (int i = list.Count - 1; i > 0; i--)
            {
                var item = (Button)list[i];
                if (item.Text.Contains(description))
                {
                    // Remove item
                    item.Parent = null;
                    item.Dispose();
                }
            }
        }

        // Need a region to display existing work
        private FlowLayoutPanel workedItemsDisplay;

        // Need a string input for describing a job

        // Need a digit input for hours worked (TO BE REPLACED BY A TIMER)
    }
}