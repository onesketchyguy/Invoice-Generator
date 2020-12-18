using System.IO;
using Newtonsoft.Json;
using Microsoft.VisualBasic.FileIO;

namespace InvoiceGenerator
{
    public class JsonManager
    {
        private static string DATA_PATH
        {
            get
            {
                return SpecialDirectories.MyDocuments + "\\InvoiceGenerator\\data\\";
            }
        }

        private static string BILLING_PATH
        {
            get
            {
                // Check path
                if (Directory.Exists(DATA_PATH) == false)
                    Directory.CreateDirectory(DATA_PATH);

                return DATA_PATH + "billing.json";
            }
        }

        private static string SAVE_PATH
        {
            get
            {
                // Check path
                if (Directory.Exists(DATA_PATH) == false)
                    Directory.CreateDirectory(DATA_PATH);

                return DATA_PATH + "workitems.json";
            }
        }

        public void WriteObject(object myObj, string path)
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(JsonConvert.SerializeObject(myObj));
                file.Close();
            }
        }

        public T ReadObject<T>(string path)
        {
            string filedata = "";

            if (File.Exists(path))
            {
                using (StreamReader file = new StreamReader(path))
                {
                    filedata = file.ReadToEnd();
                    file.Close();
                }
            }

            return JsonConvert.DeserializeObject<T>(filedata);
        }

        public void SaveBilling(BillingObject saveData)
        {
            // Save all the current data so we can access it later
            WriteObject(saveData, BILLING_PATH);
        }

        public BillingObject LoadBilling()
        {
            // Load any existing data
            return ReadObject<BillingObject>(BILLING_PATH);
        }

        public void SaveData(DataObject saveData)
        {
            // Save all the current data so we can access it later
            WriteObject(saveData, SAVE_PATH);
        }

        public DataObject LoadData()
        {
            // Load any existing data
            return ReadObject<DataObject>(SAVE_PATH);
        }
    }

    public class BillingObject
    {
        public string invoiceDirectory;

        public string biller = "Anony Mous";

        public string billerAddress = "12435 se street ave, City Zip";

        public string billerContact = "PHONE eMail";

        public string billing = "Anony Mous";

        public string billingAddress = "12435 se street ave, City Zip";

        public string billingContact = "PHONE eMail";

        public int invoiceNumber = 1;
        public double chargePerHour = 10;
    }

    public class DataObject
    {
        public string[] description;
        public double[] value;
    }
}