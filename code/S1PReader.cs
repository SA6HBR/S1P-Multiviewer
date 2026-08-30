using System.Globalization;
using System.Numerics;

namespace S1P_Multiviewer
{
    /// <summary>
    /// Reads Touchstone (.s1p/.s2p) files and caches the parsed data per file path
    /// (index-aligned with <see cref="Parameter.Files"/>). S21 is read from .s2p
    /// files when present (see SParameter.HasS21); S12/S22 are not used yet.
    /// </summary>
    public class S1PReader
    {
        public static List<SParameter>[] S1PFiles = { };

        public static List<SParameter> ReadS1PFile(string filePath)
        {
            var sParameters = new List<SParameter>();

            if (!File.Exists(filePath))
            {
                // No file yet (e.g. not measured for this parameter combination) -
                // return a flat placeholder series so downstream code has something to plot.
                for (int i = 1; i <= Parameter.Points; i++)
                {
                    sParameters.Add(new SParameter { FrequencyHz = 0, Real = 0, Imag = 0, VSWR = 0 });
                }
                return sParameters;
            }

            var lines = File.ReadAllLines(filePath);
            string freqUnit = "Hz";

            for (int lineNo = 0; lineNo < lines.Length; lineNo++)
            {
                string line = lines[lineNo].Trim();

                // Skip empty lines or comments
                if (string.IsNullOrEmpty(line) || line.StartsWith("!"))
                    continue;

                if (line.StartsWith("#"))
                {
                    // Parse options line (e.g. "# MHz S RI R 50")
                    var tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        if (token.Equals("MHZ", StringComparison.OrdinalIgnoreCase)) freqUnit = "MHz";
                        else if (token.Equals("GHZ", StringComparison.OrdinalIgnoreCase)) freqUnit = "GHz";
                        else if (token.Equals("KHZ", StringComparison.OrdinalIgnoreCase)) freqUnit = "kHz";
                        else if (token.Equals("HZ", StringComparison.OrdinalIgnoreCase)) freqUnit = "Hz";
                    }
                    continue;
                }

                // Read frequency and S-parameters. A .s1p file has 3 columns
                // (freq, S11 real, S11 imag); a .s2p file has 9 (freq, S11, S21,
                // S12, S22, each as a real/imag pair) - S12/S22 aren't used by
                // this app yet, but S21 (columns 3-4) is read when present.
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    continue;

                double freq, real, imag;
                double s21Real = 0, s21Imag = 0;
                bool hasS21 = parts.Length >= 5;
                try
                {
                    freq = ParseNumber(parts[0]);
                    real = ParseNumber(parts[1]);
                    imag = ParseNumber(parts[2]);
                    if (hasS21)
                    {
                        s21Real = ParseNumber(parts[3]);
                        s21Imag = ParseNumber(parts[4]);
                    }
                }
                catch (FormatException ex)
                {
                    throw new FormatException(
                        $"Kunde inte tolka tal på rad {lineNo + 1} i \"{Path.GetFileName(filePath)}\": \"{line}\"", ex);
                }

                switch (freqUnit.ToUpperInvariant())
                {
                    case "GHZ": freq *= 1e9; break;
                    case "MHZ": freq *= 1e6; break;
                    case "KHZ": freq *= 1e3; break;
                }

                sParameters.Add(new SParameter
                {
                    FrequencyHz = freq,
                    Real = real,
                    Imag = imag,
                    VSWR = S11Converter.VSWR(real, imag),
                    HasS21 = hasS21,
                    S21Real = s21Real,
                    S21Imag = s21Imag
                });
            }

            Parameter.Points = sParameters.Count;
            return sParameters;
        }

        /// <summary>
        /// Parses a numeric field regardless of whether it uses "." or "," as decimal
        /// separator, since different export tools (NanoVNA-App, standard Touchstone
        /// exporters, etc.) disagree on this. Touchstone number fields never use a
        /// thousands separator, so treating "," as a decimal point is unambiguous.
        /// </summary>
        private static double ParseNumber(string field)
        {
            string normalized = field.Replace(',', '.');
            return double.Parse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Looks up (and lazily loads from disk if needed) the S-parameter data for
        /// every corner file Form1.getFilenames() found (Parameter.CornerFiles /
        /// CornerDiffs) and bundles it together with inverse-square-distance
        /// weights. This centralizes logic that used to be duplicated across
        /// PlotSmithChart, PlotExploreMultiChart and the heatmap/center-frequency/
        /// bandwidth-map loops.
        /// </summary>
        public static WeightedSParamSet GetWeightedSParams(bool loadMissingFromDisk = true)
        {
            var result = new WeightedSParamSet();

            List<SParameter> Lookup(string fileName)
            {
                if (string.IsNullOrEmpty(fileName)) return new List<SParameter>();

                int idx = Array.IndexOf(Parameter.Files, fileName);
                if (idx < 0 || S1PFiles == null || idx >= S1PFiles.Length) return new List<SParameter>();

                if ((S1PFiles[idx] == null || S1PFiles[idx].Count == 0) && loadMissingFromDisk)
                {
                    S1PFiles[idx] = ReadS1PFile(Parameter.Files[idx]);
                }

                return S1PFiles[idx] ?? new List<SParameter>();
            }

            for (int i = 0; i < Parameter.CornerFiles.Count; i++)
            {
                var data = Lookup(Parameter.CornerFiles[i]);
                if (data.Count == 0) continue;

                // Exact match (diff 0) carries the point alone with full weight -
                // getFilenames() only ever returns a single corner in that case.
                double weight = Parameter.CornerDiffs[i] == 0 ? 1 : 1 / Math.Pow(Parameter.CornerDiffs[i], 2);

                result.Params.Add(data);
                result.Weights.Add(weight);
            }

            return result;
        }
    }

    /// <summary>
    /// Weighted S-parameter datasets for the nearby "corner" files found for the
    /// requested parameter point, ready to be blended together. With N
    /// parameters there can be up to 2^N corners, so this is a parallel-list
    /// structure (Params[i] uses Weights[i]) rather than fixed named slots.
    /// </summary>
    public class WeightedSParamSet
    {
        public List<List<SParameter>> Params = new();
        public List<double> Weights = new();

        public double WeightSum => Weights.Sum();

        /// <summary>Number of frequency points, taken from the first corner (all corners are expected to share the same sweep).</summary>
        public int PointCount => Params.Count > 0 ? Params[0].Count : 0;

        public Complex BlendComplex(int freqIndex)
        {
            if (Params.Count == 0) return Complex.Zero;

            Complex sum = Complex.Zero;
            double weightSum = 0;
            for (int i = 0; i < Params.Count; i++)
            {
                // TILLÄGG: Hoppa över filen om den saknar detta index (förhindrar krasch)
                if (freqIndex >= Params[i].Count) continue; 
                
                sum += new Complex(Params[i][freqIndex].Real, Params[i][freqIndex].Imag) * Weights[i];
                weightSum += Weights[i];
            }
            return weightSum > 0 ? sum / weightSum : Complex.Zero;
        }

        /// <summary>
        /// Same as BlendComplex, but for S21. Corners whose file didn't have S21
        /// data (a plain .s1p was used there instead of .s2p) are skipped rather
        /// than treated as 0+0i, so they don't drag the blend toward zero.
        /// </summary>
        public Complex BlendComplexS21(int freqIndex)
        {
            if (Params.Count == 0) return Complex.Zero;

            Complex sum = Complex.Zero;
            double weightSum = 0;
            for (int i = 0; i < Params.Count; i++)
            {
                var p = Params[i][freqIndex];

                // TILLÄGG: Hoppa över om index saknas eller S21 saknas
                if (freqIndex >= Params[i].Count || !p.HasS21) continue;

                sum += new Complex(p.S21Real, p.S21Imag) * Weights[i];
                weightSum += Weights[i];
            }
            return weightSum > 0 ? sum / weightSum : Complex.Zero;
        }

        public double BlendVSWR(int freqIndex)
        {
            if (Params.Count == 0) return 0;

            double sum = 0, weightSum = 0;
            for (int i = 0; i < Params.Count; i++)
            {
                // TILLÄGG: Hoppa över filen om den saknar detta index
                if (freqIndex >= Params[i].Count) continue;

                sum += Params[i][freqIndex].VSWR * Weights[i];
                weightSum += Weights[i];
            }
            return weightSum > 0 ? sum / weightSum : 0;
        }

        public double MinVSWR(int freqIndex) => Params.Count == 0 ? 0 : Params.Min(p => p[freqIndex].VSWR);
        public double MaxVSWR(int freqIndex) => Params.Count == 0 ? 0 : Params.Max(p => p[freqIndex].VSWR);
    }
}
