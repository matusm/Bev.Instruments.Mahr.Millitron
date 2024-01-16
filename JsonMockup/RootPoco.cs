namespace JsonMockup
{
    public class RootPoco
    {
        public string SoftwareID { get; set; }
        public string ClientID { get; set; }
        public OrderPoco Order { get; set; }
        public string[] Qm { get; set; }
        public PersonPoco[] RespPersons { get; set; }
        public MeasurementDevicePoco[] MeasurementDevices { get; set;}
        // influenceConditions
        public string TsID { get; set; }
        public long TsStart { get; set; }
        public long TsEnd { get; set; }
        // measurementParameters
        // measurementValues
        public MeasurementResultPoco[] MeasurementResults { get; set; }
        public string[] Error { get; set; }
    }
}
