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
            PersonPoco pers1 = new PersonPoco
            {
                PersonID = "matusm",
                MainSigner = true
            };
            PersonPoco pers2 = new PersonPoco
            {
                PersonID = "zelenkaz",
                MainSigner = false
            };

            MeasurementDevicePoco pruefling = new MeasurementDevicePoco
            {
                Type = "examinee",
                MdID = "MD000001"
            };
            MeasurementDevicePoco normal = new MeasurementDevicePoco
            {
                Type = "standard",
                MdID = "MD000002"
            };
            MeasurementDevicePoco environmet = new MeasurementDevicePoco
            {
                Type = "environment",
                MdID = "MD000003"
            };


            MeasurementResultPoco nom = new MeasurementResultPoco
            {
                MrID = "MR000001",
                Label = "Nennlänge",
                Abbreviation = "l_n",
                ComplexReal = new ComplexReal
                {
                    Unit = "_milli_meter",
                    Value = new Value
                    {
                        DecimalPositions = 0,
                        Float = 25,
                        Sign = 1
                    }
                }
            };
            MeasurementResultPoco f_c = new MeasurementResultPoco
            {
                MrID = "MR000002",
                Label = "Mittenmaßabweichung",
                Abbreviation = "f_c",
                ComplexReal = new ComplexReal
                {
                    Unit = "_micro_meter",
                    Value = new Value
                    {
                        DecimalPositions = 3,
                        Float = -0.234,
                        Sign = -1
                    }
                }
            };
            MeasurementResultPoco length = new MeasurementResultPoco
            {
                MrID = "MR000003",
                Label = "Mittenmaß",
                Abbreviation = "l_c",
                ComplexReal = new ComplexReal
                {
                    Unit = "_milli_meter",
                    Value = new Value
                    {
                        DecimalPositions = 6,
                        Float = 25 + (-0.234)/1000,
                        Sign = 1
                    }
                }
            };
            MeasurementResultPoco v = new MeasurementResultPoco
            {
                MrID = "MR000004",
                Label = "Abweichungsspanne",
                Abbreviation = "v",
                ComplexReal = new ComplexReal
                {
                    Unit = "_micro_meter",
                    Value = new Value
                    {
                        DecimalPositions = 3,
                        Float = 0.050,
                        Sign = 1
                    }
                }
            };

            OrderPoco order = new OrderPoco
            {
                OrderID = "XXX",
                T_nr = "T01-9999",
                Description = "Messung für ersten Programm-Test",
                Comment = "keine Endmaß-Bewegung, Antastung immer am gleichen Punkt"
            };

            RootPoco result = new RootPoco
            {
                SoftwareID = "BE230SM0001R0002",
                ClientID = "W003-E2-M-N58",
                Order = order,
                Qm = new string[] { "A_011203" },
                RespPersons = new PersonPoco[] { pers1, pers2 },
                MeasurementDevices = new MeasurementDevicePoco[] { pruefling, normal, environmet },
                MeasurementResults = new MeasurementResultPoco[] { nom, f_c, length, v },
                Error = new string[] { "" }
            };



            #region Output of the root POCO
            JsonSerializerOptions jOptions = new JsonSerializerOptions 
            {
                WriteIndented = true,
                IncludeFields = false
            };
            string jsonString = JsonSerializer.Serialize(result, jOptions);
            Console.WriteLine();
            Console.WriteLine(jsonString);
            Console.WriteLine();
            #endregion

        }
    }
}
