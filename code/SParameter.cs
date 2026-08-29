namespace S1P_Multiviewer
{
    public class SParameter
    {
        public double FrequencyHz { get; set; } = 0.0;

        // S11 - reflection coefficient at port 1 (present in both .s1p and .s2p files)
        public double Real { get; set; } = 0.0;
        public double Imag { get; set; } = 0.0;
        public double VSWR { get; set; } = 0.0;

        // S21 - transmission coefficient, port 1 -> port 2 (only present in .s2p
        // files). HasS21 tells you whether these actually came from the file,
        // since 0/0 would otherwise be indistinguishable from "no data here".
        public bool HasS21 { get; set; } = false;
        public double S21Real { get; set; } = 0.0;
        public double S21Imag { get; set; } = 0.0;
    }
}
