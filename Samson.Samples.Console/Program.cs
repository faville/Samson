using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Samson.Core;
using Samson.Models;

namespace MPStarterPack.Samples.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Initialize().Wait();
        }

        async static Task Initialize()
        {
            System.Console.WriteLine("Getting all Contacts from the database... ");
            ConsoleSpiner spin = new ConsoleSpiner();

            MinistryPlatformDataContext dataContext = new MinistryPlatformDataContext();

            Task<IEnumerable<Contact>> dataTask = new Task<IEnumerable<Contact>>(
                () =>
            {
                var data = dataContext.ExecuteStoredProcedure<Contact>("api_Example_GetAllContacts");
                return data;
            });

            Task spinTask = new Task(() => spin.StartTurning());

            spinTask.Start();
            dataTask.Start();
            IEnumerable<Contact> table = await dataTask;

            spin.StopTurning();

            foreach (var row in table)
            {
                System.Console.WriteLine("Display Name: " + row.Display_Name);
                System.Console.WriteLine("\tEmail Address: " + row.Email_Address);
                System.Console.WriteLine("\tDate of Birth: " + row.Date_of_Birth);
                System.Console.WriteLine();
            }

            //this section tests the update method that tracks changes in a single object
            System.Console.WriteLine("\nEnter the ID of the contact whose web page field will be updated.");

            int contactID = 0;
            if (Int32.TryParse(System.Console.ReadLine(), out contactID))
            {
                System.Console.WriteLine("\nGetting the contact record...");

                Task<Contact> dataTask2 = new Task<Contact>(
               () =>
               {
                   var data = dataContext.ExecuteStoredProcedure<Contact>("api_Example_GetSingleContact", new { ContactID = contactID });
                   return data.SingleOrDefault();
               });

                Task spinTask2 = new Task(() => spin.StartTurning());

                spinTask2.Start();
                dataTask2.Start();
                Contact record = await dataTask2;
                spin.StopTurning();

                record.Web_Page = "http://google.com/" + ToUnixTime(DateTime.Now);

                try
                {
                    dataContext.Update<Contact>(record);

                    System.Console.WriteLine("\nRecord Updated sucessfully!");         
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("\n " + e.Message);
                }  
            }
            else
                System.Console.WriteLine("\nInvalid integer value. You'll have to restart the application.");

            System.Console.WriteLine("\nPress Enter to Exit.");
            System.Console.ReadLine();
        }

        public class ConsoleSpiner
        {
            int counter;
            bool isTurning;
            public ConsoleSpiner()
            {
                counter = 0;
                isTurning = false;
            }

            public void StartTurning()
            {
                isTurning = true;

                while (isTurning)
                {
                    Turn();
                }
            }

            public void StopTurning()
            {
                isTurning = false;
            }

            public void Turn()
            {
                counter++;
                switch (counter % 4)
                {
                    case 0: System.Console.Write("/"); break;
                    case 1: System.Console.Write("-"); break;
                    case 2: System.Console.Write("\\"); break;
                    case 3: System.Console.Write("|"); break;
                }
                System.Console.SetCursorPosition(System.Console.CursorLeft - 1, System.Console.CursorTop);
                System.Threading.Thread.Sleep(100);
            }
        }

        public static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
    }
}
