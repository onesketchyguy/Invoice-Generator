using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.IO;

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

                var dir = DATA_PATH + "\\billing\\";

                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }

        private static string SAVE_PATH
        {
            get
            {
                // Check path
                if (Directory.Exists(DATA_PATH) == false)
                    Directory.CreateDirectory(DATA_PATH);

                var dir = DATA_PATH + "\\work items\\";

                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }

        private static string BILLING
        {
            get
            {
                // Check path
                if (Directory.Exists(DATA_PATH) == false)
                    Directory.CreateDirectory(DATA_PATH);

                return DATA_PATH + "billing.json";
            }
        }

        private static string SAVE_WORKITEMS
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
            WriteObject(saveData, BILLING);
        }

        public BillingObject LoadBilling()
        {
            // Load any existing data
            return ReadObject<BillingObject>(BILLING);
        }

        public void SaveClient(Client client, DataObject saveData)
        {
            WriteObject(client, BILLING_PATH + client.name + ".json");
            WriteObject(saveData, SAVE_PATH + client.name + "-workItems.json");
        }

        public void SaveContractor(Contractor client)
        {
            WriteObject(client, BILLING_PATH + "contractorData.json");
        }

        public void SaveData(DataObject saveData)
        {
            // Save all the current data so we can access it later
            WriteObject(saveData, SAVE_WORKITEMS);
        }

        public DataObject LoadData()
        {
            // Load any existing data
            return ReadObject<DataObject>(SAVE_WORKITEMS);
        }
    }

    /// <summary>
    /// To be simplified much much farther. Perhaps even depricated.
    /// </summary>
    [Obsolete("BillingObject is deprecated, please use Contractor and Client objects instead.")]
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

    [Serializable]
    public class Contractor : Person
    {
        public string invoiceDirectory;
    }

    [Serializable]
    public class Client : Person
    {
        public int invoiceNumber = 1;

        public double chargePerHour = 10;
    }

    [Serializable]
    public class Person
    {
        public string name = "Anony Mous";
        public string address = "12435 se street ave, City ZIP";
        public string contact = "PHONE/eMail";
    }

    public class DataObject
    {
        public string[] description;
        public double[] value;
    }
}