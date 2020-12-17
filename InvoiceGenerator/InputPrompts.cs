using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace InvoiceGenerator.InputPropmts
{
    public class FirstLaunchSurvey : Form
    {
        public delegate void OnFinishedSurvey(BillingObject data);

        public static OnFinishedSurvey onFinishedSurveyCallback;

        private BillingObject data;

        // Biller area

        // Name input
        // Addy input
        private string biller_email_phone;

        private string billerAddy;

        // Charge amount

        // Billing area

        // Name input
        // Addy input
        private string billing_email_phone;

        private string billingAddy;

        public FirstLaunchSurvey(OnFinishedSurvey onFinished)
        {
            onFinishedSurveyCallback += onFinished;

            this.Size = new Size(500, 500);

            data = new BillingObject();

            // Biller
            TextBox billerTitle = new TextBox();
            billerTitle.Parent = this;
            billerTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            billerTitle.Size = new Size(300, 50);
            billerTitle.Location = new Point(0, 25);
            billerTitle.Text = "BILLER";
            billerTitle.ForeColor = Color.Black;
            billerTitle.Enabled = false;

            TextBox biller_nameInput = new TextBox();
            biller_nameInput.Parent = this;
            biller_nameInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            biller_nameInput.Size = new Size(300, 50);
            biller_nameInput.Location = new Point(0, 50);
            biller_nameInput.PlaceholderText = "Name";
            biller_nameInput.ForeColor = Color.Black;
            biller_nameInput.TextChanged += Biller_nameInput_TextChanged;

            TextBox biller_addyInput = new TextBox();
            biller_addyInput.Parent = this;
            biller_addyInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            biller_addyInput.Size = new Size(300, 50);
            biller_addyInput.Location = new Point(0, 75);
            biller_addyInput.PlaceholderText = "Address";
            biller_addyInput.ForeColor = Color.Black;
            biller_addyInput.TextChanged += Biller_addyInput_TextChanged;

            TextBox biller_email_phone = new TextBox();
            biller_email_phone.Parent = this;
            biller_email_phone.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            biller_email_phone.Size = new Size(300, 50);
            biller_email_phone.Location = new Point(0, 100);
            biller_email_phone.PlaceholderText = "email and phone";
            biller_email_phone.ForeColor = Color.Black;
            biller_email_phone.TextChanged += Biller_email_phone_TextChanged;

            // Billing
            TextBox billingTitle = new TextBox();
            billingTitle.Parent = this;
            billingTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            billingTitle.Size = new Size(300, 50);
            billingTitle.Location = new Point(0, 150);
            billingTitle.Text = "BILLING";
            billingTitle.ForeColor = Color.Black;
            billingTitle.Enabled = false;

            TextBox billing_nameInput = new TextBox();
            billing_nameInput.Parent = this;
            billing_nameInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            billing_nameInput.Size = new Size(300, 50);
            billing_nameInput.Location = new Point(0, 175);
            billing_nameInput.PlaceholderText = "Name";
            billing_nameInput.ForeColor = Color.Black;
            billing_nameInput.TextChanged += Billing_nameInput_TextChanged;

            TextBox billing_addyInput = new TextBox();
            billing_addyInput.Parent = this;
            billing_addyInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            billing_addyInput.Size = new Size(300, 50);
            billing_addyInput.Location = new Point(0, 200);
            billing_addyInput.PlaceholderText = "Address";
            billing_addyInput.ForeColor = Color.Black;
            billing_addyInput.TextChanged += Billing_addyInput_TextChanged;

            TextBox billing_email_phone = new TextBox();
            billing_email_phone.Parent = this;
            billing_email_phone.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            billing_email_phone.Size = new Size(300, 50);
            billing_email_phone.Location = new Point(0, 225);
            billing_email_phone.PlaceholderText = "email and phone";
            billing_email_phone.ForeColor = Color.Black;
            billing_email_phone.TextChanged += Billing_email_phone_TextChanged;

            // Cost box
            TextBox billerCharge = new TextBox();
            billerCharge.Parent = this;
            billerCharge.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            billerCharge.Size = new Size(300, 50);
            billerCharge.Location = new Point(0, 300);
            billerCharge.PlaceholderText = "Cost per work hour";
            billerCharge.ForeColor = Color.Black;
            billerCharge.TextChanged += Cost_Changed;

            // Finished button
            var doneButton = new Button();
            doneButton.Parent = this;
            doneButton.Size = new Size(50, 25);
            doneButton.Location = new Point(0, 400);
            doneButton.Text = "Finished";
            doneButton.ForeColor = Color.Black;
            doneButton.BackColor = Color.LightGray;

            doneButton.Click += DoneButton_Click;
        }

        private void Biller_nameInput_TextChanged(object sender, EventArgs e)
        {
            data.biller = ((TextBox)sender).Text;
        }

        private void Biller_addyInput_TextChanged(object sender, EventArgs e)
        {
            billerAddy = ((TextBox)sender).Text;
            data.billerAddress = billerAddy + '\n' + biller_email_phone;
        }

        private void Biller_email_phone_TextChanged(object sender, EventArgs e)
        {
            biller_email_phone = ((TextBox)sender).Text;
            data.billerAddress = billerAddy + '\n' + biller_email_phone;
        }

        private void Billing_nameInput_TextChanged(object sender, EventArgs e)
        {
            data.billing = ((TextBox)sender).Text;
        }

        private void Billing_addyInput_TextChanged(object sender, EventArgs e)
        {
            billingAddy = ((TextBox)sender).Text;
            data.billingAddress = billingAddy + '\n' + billing_email_phone;
        }

        private void Billing_email_phone_TextChanged(object sender, EventArgs e)
        {
            billing_email_phone = ((TextBox)sender).Text;
            data.billingAddress = billingAddy + '\n' + billing_email_phone;
        }

        private void Cost_Changed(object sender, EventArgs e)
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

            ((TextBox)sender).Text = chars;

            data.chargePerHour = double.Parse(chars);
        }

        private void HandleExitRequest()
        {
            var exitWindow = new VerifyYesNo("Finished?", (bool close) =>
            {
                if (close)
                {
                    this.Close();
                }
            });

            exitWindow.Show();
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            HandleExitRequest();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            onFinishedSurveyCallback?.Invoke(data);

            base.OnClosing(e);
        }
    }

    public class VerifyYesNo : Form
    {
        public delegate void OnFinished(bool wasYes);

        public static OnFinished onFinishedCallback;

        private TextBox text = new TextBox();

        private Button yesButton, noButton;

        public VerifyYesNo(string question, OnFinished onFinished)
        {
            this.Size = new System.Drawing.Size(400, 200);

            text.Parent = this;
            text.Text = question;
            text.Location = new System.Drawing.Point(100, 10);
            text.Anchor = AnchorStyles.Top;

            yesButton = new Button();
            yesButton.Text = "Yes";
            yesButton.Parent = this;
            yesButton.Location = new System.Drawing.Point(50, 100);
            yesButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

            yesButton.Click += YesButton_Click;

            noButton = new Button();
            noButton.Text = "No";
            noButton.Parent = this;
            noButton.Location = new System.Drawing.Point(300, 100);
            noButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            noButton.Click += NoButton_Click;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            onFinishedCallback?.Invoke(false);

            base.OnFormClosing(e);
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            onFinishedCallback?.Invoke(true);

            this.Close();
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            onFinishedCallback?.Invoke(true);

            this.Close();
        }
    }
}