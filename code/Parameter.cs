namespace S1P_Multiviewer
{
    // NOTE: This is still process-wide mutable static state, exactly as before the
    // refactor - only moved into its own file for readability. Turning this into
    // instance-based state (passed around instead of accessed globally) would touch
    // every method in Form1.cs and S1PReader.cs and is a bigger, riskier change than
    // what was asked for here. Worth doing as a deliberate follow-up, ideally with
    // the ability to compile/test along the way.
    public static class Parameter
    {
        public static string Projectname = "";
        public static string ProjectInfo = "";
        public static string Path = "";
        public static string[] Files = System.Array.Empty<string>();

        // Parsed (Param1..Param4) for each entry in Files, same index - built once
        // by SelectFile() so the rest of the code never has to re-parse filenames.
        public static (int Param1, int Param2, int Param3, int Param4)[] FileParams = System.Array.Empty<(int, int, int, int)>();

        public static int Points = 0;

        public static int ParamsNo
        {
            get
            {
                if (Param4Min != 0 || Param4Max != 0) return 4;
                if (Param3Min != 0 || Param3Max != 0) return 3;
                if (Param2Min != 0 || Param2Max != 0) return 2;
                if (Param1Min != 0 || Param1Max != 0) return 1;
                return 0;
            }
        }

        // Param 1
        public static string Param1Name = "P1";
        public static int Param1Min = 0;
        public static int Param1Max = 0;
        public static int Param1Value = 0;

        // Param 2
        public static string Param2Name = "P2";
        public static int Param2Min = 0;
        public static int Param2Max = 0;
        public static int Param2Value = 0;

        // Param 3
        public static string Param3Name = "P3";
        public static int Param3Min = 0;
        public static int Param3Max = 0;
        public static int Param3Value = 0;
        
        // Param 4
        public static string Param4Name = "P4";
        public static int Param4Min = 0;
        public static int Param4Max = 0;
        public static int Param4Value = 0;


        public static int refImp = 50;  // reference impedance (typically 50 ohms)

        public static double MinFrequencyHz = 0;
        public static double MaxFrequencyHz = 0;

        // Nearby "corner" files found by Form1.getFilenames() for the currently
        // requested (Param1..Param4) point, together with their distance. With 4
        // parameters a full corner search has up to 2^4 = 16 corners (one per
        // combination of "above/below" on each axis), so this is a list rather
        // than four fixed named fields as it was for 2 parameters. Only corners
        // that actually resolved to a file are present - no empty placeholders.
        public static List<string> CornerFiles = new List<string>();
        public static List<int> CornerDiffs = new List<int>();

        public static bool SetExpectedFile = true;
        public static List<HeatMap> HeatMapValues = new List<HeatMap>();
        public static List<CenterFrequencyMap> CenterFrequencyMapValues = new List<CenterFrequencyMap>();
        public static List<BandwidthMap> BandwidthMapValues = new List<BandwidthMap>();

        /// <summary>
        /// Returns the valid range of one axis (targetAxis: 1-4) given the current
        /// values of all four - based on the files actually found, not a formula.
        /// The axis being asked about is excluded from the "closeness" comparison;
        /// among the files whose OTHER three axes are closest (by squared distance)
        /// to the given values, this returns the min/max of the target axis.
        ///
        /// This is a straight nearest-neighbour lookup rather than interpolation:
        /// with 4 axes there's no single well-defined "below/above" to interpolate
        /// between the way there was for the old 2-parameter Param1->Param2 case
        /// (that only had one other axis to be "below" or "above" on). If the other
        /// three axes match an existing file combination exactly, this returns the
        /// real measured range for it; otherwise it returns the range found at the
        /// single closest known combination.
        /// </summary>
        public static (int Min, int Max) GetParamRange(int targetAxis, int param1, int param2, int param3, int param4)
        {
            if (FileParams.Length == 0)
            {
                // No folder scanned yet - fall back to the full axis range.
                return targetAxis switch
                {
                    1 => (Param1Min, Param1Max),
                    2 => (Param2Min, Param2Max),
                    3 => (Param3Min, Param3Max),
                    _ => (Param4Min, Param4Max),
                };
            }

            long bestDist = long.MaxValue;
            int min = 0, max = 0;

            foreach (var p in FileParams)
            {
                long dist = 0;
                if (targetAxis != 1) { long d = param1 - p.Param1; dist += d * d; }
                if (targetAxis != 2) { long d = param2 - p.Param2; dist += d * d; }
                if (targetAxis != 3) { long d = param3 - p.Param3; dist += d * d; }
                if (targetAxis != 4) { long d = param4 - p.Param4; dist += d * d; }

                int value = targetAxis switch
                {
                    1 => p.Param1,
                    2 => p.Param2,
                    3 => p.Param3,
                    _ => p.Param4,
                };

                if (dist < bestDist)
                {
                    bestDist = dist;
                    min = max = value;
                }
                else if (dist == bestDist)
                {
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
            }

            return (min, max);
        }
    }
}
