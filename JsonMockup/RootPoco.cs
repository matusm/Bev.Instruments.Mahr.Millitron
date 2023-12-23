namespace JsonMockup
{
    public class RootPoco
    {
        public string SoftwareID { get; set; }
        public string ClientID { get; set; }
        public OrderPoco Order { get; set; }
        public string[] Qm { get; set; }
        public string TsID { get; set; }
        public int TsStart { get; set; }
        public int TsEnd { get; set; }
        public string[] Error { get; set; }
    }
}
