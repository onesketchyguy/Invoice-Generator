// Forrest Lowe 2020-2021
using InvoiceGenerator.InputPropmts;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InvoiceGenerator
{
    public class DisplayWindow : Form
    {
        public const string version = "0.1.8";

        private DocumentManager document;
        private JsonManager saveManager;
        private BillingObject data;
        private Contractor contractorData;
        private Client clientData;

        private const string EXPORT = "Export PDF";
        private const string EXPORT_LOCATION = "Change export location";
        private const string SAVE_WORKITEMS = "Save current work items";
        private const string LOAD_WORKITEMS = "Load last work items";
        private const string MODIFY_CONTACT_INFO = "Change billing info";

        // Need a button region for adding work
        private Button removeWorkButton;

        // Need a string input for describing a job
        private TextBox inputBox;

        // Need a digit input for hours worked (TO BE REPLACED BY A TIMER)
        private TextBox workedHoursInput;

        private TextBox totalHoursWorkedDisplay;
        private TextBox totalChargeDisplay;

        private TextBox debugTextObject;

        public static string Format(string input)
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

        // Initialization
        public DisplayWindow(DocumentManager document, JsonManager jsonManager)
        {
            this.document = document;
            saveManager = jsonManager;

            data = new BillingObject();

            this.Text = $"Invoice generator - version {version}";
            this.Size = new Size(800, 550);
            this.UseWaitCursor = false;
            this.BackColor = Color.AliceBlue;

            // Debug box
            debugTextObject = new TextBox()
            {
                Parent = this,
                Enabled = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(800, 50),
                Location = new Point(0, Size.Height - 60),
                ForeColor = Color.Black,
                PlaceholderText = $""
            };

            // Toolbar
            var menuStrip = new MenuStrip();
            menuStrip.Parent = this;

            // HOW TO ADD A NEW MENU DROPDOWN ITEM
            var export = new ToolStripMenuItem("Export...");

            var exportPDF = new ToolStripMenuItem(EXPORT, null, new EventHandler(ExportPDF_Click));
            var exportLocation = new ToolStripMenuItem(EXPORT_LOCATION, null, new EventHandler((object s, EventArgs e) => SetExportLocation()));

            export.DropDownItems.AddRange(new ToolStripItem[] { exportPDF, exportLocation });
            menuStrip.Items.Add(export);

            // END HOW TO

            //menuStrip.Items.Add(EXPORT);
            //menuStrip.Items.Add(EXPORT_LOCATION);
            menuStrip.Items.Add(MODIFY_CONTACT_INFO);

            var workItems = new ToolStripMenuItem("Work items...");

            var saveWorkItems = new ToolStripMenuItem(SAVE_WORKITEMS, null, new EventHandler((object s, EventArgs e) => SaveWorkItems()));
            var loadWorkItems = new ToolStripMenuItem(LOAD_WORKITEMS, null, new EventHandler((object s, EventArgs e) => LoadWorkItems()));

            workItems.DropDownItems.AddRange(new ToolStripItem[] { saveWorkItems, loadWorkItems });
            menuStrip.Items.Add(workItems);

            //menuStrip.Items.Add(SAVE_WORKITEMS);
            //menuStrip.Items.Add(LOAD_WORKITEMS);

            menuStrip.ItemClicked += Toolbar_ItemClicked;

            // Input box
            var descriptionBoxHeader = new TextBox()
            {
                Parent = this,
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Size = new Size(300, 50),
                Location = new Point(0, 25),
                ForeColor = Color.Black,
                PlaceholderText = "Description of work"
            };

            inputBox = new TextBox()
            {
                Parent = this,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Size = new Size(300, 50),
                Location = new Point(0, 50),
                ForeColor = Color.Black,
                PlaceholderText = "Description"
            };

            // Work hours input
            var workBoxHeader = new TextBox()
            {
                Parent = this,
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Size = new Size(300, 75),
                Location = new Point(0, 75),
                ForeColor = Color.Black,
                PlaceholderText = "Worked hours",
            };

            workedHoursInput = new TextBox()
            {
                Parent = this,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Size = new Size(300, 50),
                Location = new Point(0, 100),
                ForeColor = Color.Black,
                PlaceholderText = "0.0",
            };

            workedHoursInput.TextChanged += WorkedHours_TextChanged;

            // Show work
            workedItemsDisplay = new FlowLayoutPanel()
            {
                Parent = this,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(200, 300),
                Location = new Point(600, 25),
                ForeColor = Color.Black,
                BackColor = Color.LightBlue,
                AutoScroll = true
            };

            totalHoursWorkedDisplay = new TextBox()
            {
                Parent = this,
                Enabled = false,
                Anchor = AnchorStyles.Bottom,
                Size = new Size(200, 50),
                Location = new Point(600, 350),
                ForeColor = Color.Black,
                PlaceholderText = "0.0",
            };

            totalChargeDisplay = new TextBox()
            {
                Parent = this,
                Enabled = false,
                Anchor = AnchorStyles.Bottom,
                Size = new Size(200, 50),
                Location = new Point(600, 380),
                ForeColor = Color.Black,
                PlaceholderText = "0.0",
            };

            // Add work button
            var addWorkButton = new Button()
            {
                Parent = workedItemsDisplay,
                Size = new Size(180, 50),
                ForeColor = Color.Black,
                BackColor = Color.Lime,
                Text = "Add work",
            };

            addWorkButton.Click += (object sender, EventArgs e) => AddWork();

            // Remove work button
            removeWorkButton = new Button()
            {
                Parent = this,
                Enabled = false,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(480, 80),
                Size = new Size(100, 35),
                ForeColor = Color.Black,
                BackColor = Color.IndianRed,
                Text = "Delete work log",
            };

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

            var clients = saveManager.LoadClients();
            if (clients != null && clients.Length > 0)
            {
                clientData = clients[0]; // Take the first client by default
            }

            contractorData = saveManager.LoadContractor();

            if (contractorData == null && clientData == null)
            {
                if (loadData != null)
                {
                    // Get data
                    data = loadData;

                    contractorData = new Contractor();
                    contractorData.name = data.biller;
                    contractorData.address = data.billerAddress;
                    contractorData.contact = data.billerContact;

                    clientData = new Client();
                    clientData.name = data.billing;
                    clientData.address = data.billingAddress;
                    clientData.contact = data.billingContact;
                    clientData.chargePerHour = data.chargePerHour;
                    clientData.invoiceNumber = data.invoiceNumber;

                    if (string.IsNullOrEmpty(data.invoiceDirectory) || string.IsNullOrEmpty(contractorData.invoiceDirectory))
                        SetExportLocation();
                }
                else
                {
                    // First launch
                    formContext = new MultiFormContext(this,
                        new FirstLaunchSurvey(new BillingObject(), (BillingObject recievedData) =>
                        {
                            data = recievedData;

                            contractorData = new Contractor();
                            contractorData.name = data.biller;
                            contractorData.address = data.billerAddress;
                            contractorData.contact = data.billerContact;

                            clientData = new Client();
                            clientData.name = data.billing;
                            clientData.address = data.billingAddress;
                            clientData.contact = data.billingContact;
                            clientData.chargePerHour = data.chargePerHour;
                            clientData.invoiceNumber = data.invoiceNumber;

                            SetExportLocation();
                        }));
                }
            }

            LoadWorkItems();

            UpdateWorkCountDisplay();

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

            workedHoursInput.Text = chars;
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

            contractorData.invoiceDirectory = filePath;
            saveManager.SaveContractor(contractorData);
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

            saveManager.SaveClient(clientData, data);
        }

        private void LoadWorkItems()
        {
            var loadData = saveManager.LoadData(clientData.name);

            if (loadData == null || loadData.description == null)
            {
                // OLD METHOD, this is still here so that we can maintain
                // backwards compatability
                loadData = saveManager.LoadData();

                if (loadData == null || loadData.description == null) return;

                for (int i = 0; i < loadData.description.Length; i++)
                {
                    // Remove any existing logs
                    RemoveWork(loadData.description[i]);

                    // Add new logs
                    AddWorkToWorkList(loadData.description[i], loadData.value[i]);
                }
            }
            else
            {
                for (int i = 0; i < loadData.description.Length; i++)
                {
                    // Remove any existing logs
                    RemoveWork(loadData.description[i]);

                    // Add new logs
                    AddWorkToWorkList(loadData.description[i], loadData.value[i]);
                }
            }
        }

        // Need to be able to display text
        public void WriteLine(string text)
        {
            debugTextObject.Text = $"{text}";
            debugTextObject.Update();
        }

        public void AddWork()
        {
            if (string.IsNullOrWhiteSpace(workedHoursInput.Text)) return;
            if (string.IsNullOrWhiteSpace(inputBox.Text)) return;

            double value = double.Parse(workedHoursInput.Text);
            string description = inputBox.Text;

            // Add an item to the worked items display
            AddWorkToWorkList(description, value);
        }

        private void AddWorkToWorkList(string description, double value)
        {
            document.AddWorkLog(description, value);

            // Instead of this we could remove all items and have a list of just the data we need.
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

                    UpdateWorkCountDisplay();
                    return;
                }
            }

            // Add a new item
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

            UpdateWorkCountDisplay();
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

            UpdateWorkCountDisplay();
        }

        private void UpdateWorkCountDisplay()
        {
            double totalHoursWorked = 0.0;

            var list = workedItemsDisplay.Controls;
            for (int i = list.Count - 1; i > 0; i--)
            {
                var item = (Button)list[i];
                var text = item.Text.Split(' ');

                double itemValue = 0.0;

                for (int j = text.Length - 1; j > 0; j--)
                {
                    double.TryParse(text[j], out itemValue);

                    if (itemValue > 0) break;
                }

                totalHoursWorked += itemValue;
            }

            totalHoursWorkedDisplay.PlaceholderText = $"Total hours worked: {totalHoursWorked}";
            totalChargeDisplay.PlaceholderText = $"Total charge: {Math.Round((totalHoursWorked * data.chargePerHour), 2)}";
        }

        // Need a region to display existing work
        private FlowLayoutPanel workedItemsDisplay;
    }
}