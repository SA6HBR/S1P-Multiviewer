namespace S1P_Multiviewer
{
    public class HeatMap
    {
        public int Param1Value { get; set; } = 0;
        public int Param2Value { get; set; } = 0;
        public double Min { get; set; } = 0.0;
        public double Avg { get; set; } = 0.0;
        public double Max { get; set; } = 0.0;
    }

    public class CenterFrequencyMap
    {
        public double FrequencyHz { get; set; } = 0.0;
        public int FileRowNo { get; set; } = 0;
        public int VSWR12 { get; set; } = 0;
        public int VSWR13 { get; set; } = 0;
        public int VSWR14 { get; set; } = 0;
        public int VSWR15 { get; set; } = 0;
        public int VSWR16 { get; set; } = 0;
        public int VSWR17 { get; set; } = 0;
        public int VSWR18 { get; set; } = 0;
        public int VSWR19 { get; set; } = 0;
        public int VSWR20 { get; set; } = 0;
        public int VSWR30 { get; set; } = 0;
        public int VSWR40 { get; set; } = 0;
        public int VSWR50 { get; set; } = 0;
    }

    public class BandwidthMap
    {
        public int Param1Value { get; set; } = 0;
        public int Param2Value { get; set; } = 0;
        public double Avg { get; set; } = 0.0;
    }
}
