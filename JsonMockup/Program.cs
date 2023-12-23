using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace JsonMockup
{
    class Program
    {
        static void Main(string[] args)
        {

            OrderPoco order = new OrderPoco
            {
                OrderID = "XXX",
                T_nr = "T23-9999",
                Description = "Messung für ersten Programm-Test",
                Comment = "keine Endmaß-Bewegung, Antastung immer am gleichen Punkt"
            };

            RootPoco result = new RootPoco
            {
                SoftwareID = "BE230SM0001R0002",
                ClientID = "W003-E2-M-N58",
                Order = order,
                Qm = new string[] { "A_001203", "A_011501" }
            };



            JsonSerializerOptions jOptions = new JsonSerializerOptions 
            {
                WriteIndented = true,
                IncludeFields = false
            };
            string jsonString = JsonSerializer.Serialize(result, jOptions);

            Console.WriteLine();
            Console.WriteLine(jsonString);
            Console.WriteLine();

        }
    }
}
