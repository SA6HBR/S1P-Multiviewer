using ScottPlot;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace S1P_Multiviewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = Version.NameAndNumber;

            ResetSmithChart();

            formsPlot2.Plot.XLabel("Frequency (MHz)");
            formsPlot2.Plot.YLabel("VSWR S11");
            formsPlot2.Plot.Axes.SetLimitsY(1, (int)numericUpDown1.Value);
            formsPlot2.Plot.Axes.SetLimitsX(50, 900);
            formsPlot2.Refresh();
        }
        public void ResetSmithChart()
        {
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Add.SmithChartAxis();
            formsPlot1.Refresh();
        }
        public void PlotSmithChart()
        {

            getFilenames(Parameter.Param1Value, Parameter.Param2Value, Parameter.Param3Value, Parameter.Param4Value);

            var weighted = S1PReader.GetWeightedSParams();

            formsPlot1.Plot.Clear();
            var smith = formsPlot1.Plot.Add.SmithChartAxis();

            double MinFrequencyHz = (double)numericUpDown11.Value * 1000000;
            double MaxFrequencyHz = (double)numericUpDown10.Value * 1000000;

            for (int i = 0; i < weighted.PointCount; i++)
            {

                if (MinFrequencyHz <= weighted.Params[0][i].FrequencyHz && weighted.Params[0][i].FrequencyHz <= MaxFrequencyHz)
                {
                    Complex complexIn1 = weighted.BlendComplex(i);

                    Coordinates location = smith.GetCoordinates(S11Converter.Resistance_R(complexIn1.Real, complexIn1.Imaginary, 1), S11Converter.Reactance_X(complexIn1.Real, complexIn1.Imaginary, 1));
                    formsPlot1.Plot.Add.Marker(location, MarkerShape.FilledCircle, size: 5, Colors.Red);
                }
            }

            formsPlot1.UserInputProcessor.IsEnabled = false; //Mouse and keyboard events are disabled
            formsPlot1.Refresh();
        }
        public void PlotExploreChart()
        {
            PlotSmithChart();
            PlotExploreMultiChart();
        }
        // Metrics available on both S11 and S21, sharing the same underlying
        // magnitude/phase math (just applied to a different port pair). Some
        // pairs use the same base name on both sides (e.g. "LogMag"), others
        // use different conventional names for the same formula (Return Loss
        // on S11 is called Insertion Loss on S21) - so each row carries its
        // own S11/S21/combined combobox labels rather than assuming a shared
        // base name. Centralizing this here avoids repeating the same
        // S11-vs-S21-vs-both branching seven times over.
        private static readonly (string S11Label, string S21Label, string ComboLabel, Func<double, double, double> S11Func, Func<double, double, double> S21Func, string YLabel)[] DualPortMetrics =
        {
            ("Magnitude S11", "Magnitude S21", "Magnitude S11 + S21", S11Converter.GammaMagnitude, S21Converter.Magnitude, "Magnitude"),
            ("LinMag S11", "LinMag S21", "LinMag S11 + S21", S11Converter.LinMag, S21Converter.LinMag, "LinMag"),
            ("LogMag S11", "LogMag S21", "LogMag S11 + S21", S11Converter.LogMag, S21Converter.InsertionGain_dB, "LogMag (dB)"),
            // PhaseDegrees/PhaseRadians/ReflectedPower on the S11 side, and
            // TransmittedPower on the S21 side, all have a 3rd optional
            // parameter (refImp / incidentPower) - a method group with an
            // optional parameter can't convert directly to a 2-arg Func (the
            // default isn't applied in delegate conversions), so these are
            // wrapped in a lambda that calls them normally instead.
            ("PhaseDegrees S11", "PhaseDegrees S21", "PhaseDegrees S11 + S21", (real, imag) => S11Converter.PhaseDegrees(real, imag), S21Converter.PhaseDegrees, "Phase (deg)"),
            ("PhaseRadians S11", "PhaseRadians S21", "PhaseRadians S11 + S21", (real, imag) => S11Converter.PhaseRadians(real, imag), S21Converter.PhaseRadians, "Phase (rad)"),
            ("ReflectedPower S11", "TransmittedPower S21", "ReflectedPower S11 + TransmittedPower S21", (real, imag) => S11Converter.ReflectedPower(real, imag), (real, imag) => S21Converter.TransmittedPower(real, imag), "Power fraction"),
            ("ReturnLoss_RL S11", "InsertionLoss S21", "ReturnLoss_RL S11 + InsertionLoss S21", S11Converter.ReturnLoss_RL, S21Converter.InsertionLoss_dB, "dB"),
            // Experimental - see S21Converter.VSWR's doc comment. VSWR on S21
            // isn't a real physical VSWR, just the same formula applied to
            // |S21| out of curiosity.
            ("VSWR S11", "VSWR S21", "VSWR S11 + S21", S11Converter.VSWR, S21Converter.VSWR, "VSWR"),
        };

        public void PlotExploreMultiChart()
        {

            getFilenames(Parameter.Param1Value, Parameter.Param2Value, Parameter.Param3Value, Parameter.Param4Value);

            var weighted = S1PReader.GetWeightedSParams();

            var frequencies = weighted.PointCount > 0 ? weighted.Params[0].Select(d => (d.FrequencyHz / 1000000)).ToArray() : Array.Empty<double>();

            formsPlot1.Plot.Clear();
            var smith = formsPlot1.Plot.Add.SmithChartAxis();

            string selected = comboBox1.SelectedItem?.ToString() ?? "";

            formsPlot2.Plot.Clear();

            var comboMetric = DualPortMetrics.FirstOrDefault(m => m.ComboLabel == selected);
            var s21Metric = DualPortMetrics.FirstOrDefault(m => m.S21Label == selected);
            var s11Metric = DualPortMetrics.FirstOrDefault(m => m.S11Label == selected);

            if (comboMetric.ComboLabel != null)
            {
                // Both traces on the same chart: S11 blue (as it always has been),
                // S21 red (new).
                double[] s11Values = new double[frequencies.Length];
                double[] s21Values = new double[frequencies.Length];

                for (int i = 0; i < weighted.PointCount; i++)
                {
                    Complex s11 = weighted.BlendComplex(i);
                    Complex s21 = weighted.BlendComplexS21(i);

                    s11Values[i] = comboMetric.S11Func(s11.Real, s11.Imaginary);
                    s21Values[i] = comboMetric.S21Func(s21.Real, s21.Imaginary);
                }

                var s11Scatter = formsPlot2.Plot.Add.Scatter(frequencies, s11Values);
                s11Scatter.Color = Colors.Blue;
                s11Scatter.LegendText = "S11";

                var s21Scatter = formsPlot2.Plot.Add.Scatter(frequencies, s21Values);
                s21Scatter.Color = Colors.Red;
                s21Scatter.LegendText = "S21";

                formsPlot2.Plot.YLabel(comboMetric.YLabel);
                formsPlot2.Plot.ShowLegend();
            }
            else if (s21Metric.S21Label != null)
            {
                double[] plotValues = new double[frequencies.Length];

                for (int i = 0; i < weighted.PointCount; i++)
                {
                    Complex s21 = weighted.BlendComplexS21(i);
                    plotValues[i] = s21Metric.S21Func(s21.Real, s21.Imaginary);
                }

                var scatter = formsPlot2.Plot.Add.Scatter(frequencies, plotValues);
                scatter.Color = Colors.Blue;

                formsPlot2.Plot.YLabel(selected);
            }
            else if (s11Metric.S11Label != null)
            {
                double[] plotValues = new double[frequencies.Length];

                for (int i = 0; i < weighted.PointCount; i++)
                {
                    Complex complexIn1 = weighted.BlendComplex(i);
                    plotValues[i] = s11Metric.S11Func(complexIn1.Real, complexIn1.Imaginary);
                }

                var scatter = formsPlot2.Plot.Add.Scatter(frequencies, plotValues);
                scatter.Color = Colors.Blue;

                formsPlot2.Plot.YLabel(selected);
            }
            else
            {
                // S11-only metrics that have no S21/combined counterpart, since
                // they rely on the one-port impedance transform (meaningless
                // for a transmission parameter): Conductance_G, ImpedanceMagnitude,
                // Reactance_X, Resistance_R, Susceptance_B.
                double[] plotValues = new double[frequencies.Length];

                for (int i = 0; i < weighted.PointCount; i++)
                {
                    Complex complexIn1 = weighted.BlendComplex(i);

                    if (selected == "Conductance_G S11") { plotValues[i] = S11Converter.Conductance_G(complexIn1.Real, complexIn1.Imaginary); }
                    else if (selected == "ImpedanceMagnitude S11") { plotValues[i] = S11Converter.ImpedanceMagnitude(complexIn1.Real, complexIn1.Imaginary); }
                    else if (selected == "Reactance_X S11") { plotValues[i] = S11Converter.Reactance_X(complexIn1.Real, complexIn1.Imaginary); }
                    else if (selected == "Resistance_R S11") { plotValues[i] = S11Converter.Resistance_R(complexIn1.Real, complexIn1.Imaginary); }
                    else { plotValues[i] = S11Converter.Susceptance_B(complexIn1.Real, complexIn1.Imaginary); }

                }

                var scatter = formsPlot2.Plot.Add.Scatter(frequencies, plotValues);
                scatter.Color = Colors.Blue; // S11 (or whichever single trace) drawn as before.

                formsPlot2.Plot.YLabel(selected);
            }

            formsPlot2.Plot.XLabel("Frequency (MHz)");
            formsPlot2.Plot.Axes.AutoScale();
            formsPlot2.Plot.Axes.SetLimitsY((double)numericUpDown9.Value, (double)numericUpDown1.Value);
            formsPlot2.Plot.Axes.SetLimitsX((double)numericUpDown11.Value, (double)numericUpDown10.Value);
            //  formsPlot2.UserInputProcessor.IsEnabled = false; // Mouse and keyboard events are disabled
            formsPlot2.Refresh();

        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                Parameter.Param1Value = trackBarExploreParam1.Value;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }
        }
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                Parameter.Param2Value = trackBarExploreParam2.Value;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }
        }
        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                Parameter.Param3Value = trackBarExploreParam3.Value;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }
        }
        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                Parameter.Param4Value = trackBarExploreParam4.Value;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }
        }
        public void SetExpectedFile()
        {
            int Param1 = Parameter.Param1Value; // trackBar1.Value;
            int Param2 = Parameter.Param2Value; // trackBar2.Value;
            int Param3 = Parameter.Param3Value;
            int Param4 = Parameter.Param4Value;

            //var (Param2Min, Param2Max) = Parameter.GetParamRange(2, Param1, Param2, Param3, Param4);
            //if (Param2 < Param2Min) { Param2 = Param2Min; }
            //if (Param2 > Param2Max) { Param2 = Param2Max; }

            //var (Param3Min, Param3Max) = Parameter.GetParamRange(3, Param1, Param2, Param3, Param4);
            //if (Param3 < Param3Min) { Param3 = Param3Min; }
            //if (Param3 > Param3Max) { Param3 = Param3Max; }

            //var (Param4Min, Param4Max) = Parameter.GetParamRange(4, Param1, Param2, Param3, Param4);
            //if (Param4 < Param4Min) { Param4 = Param4Min; }
            //if (Param4 > Param4Max) { Param4 = Param4Max; }

            Parameter.Param1Value = Param1;
            Parameter.Param2Value = Param2;
            Parameter.Param3Value = Param3;
            Parameter.Param4Value = Param4;

            trackBarExploreParam1.Value = Param1;

            trackBarExploreParam2.Value = Math.Max(Math.Min(Param2, trackBarExploreParam2.Maximum), trackBarExploreParam2.Minimum);
            //trackBarExploreParam2.Minimum = Param2Min;
            //trackBarExploreParam2.Maximum = Param2Max;
            trackBarExploreParam2.Value = Param2;

            //labelExploreMinParam2.Text = Param2Min.ToString();
            //labelExploreMaxParam2.Text = Param2Max.ToString();

            trackBarExploreParam3.Value = Math.Max(Math.Min(Param3, trackBarExploreParam3.Maximum), trackBarExploreParam3.Minimum);
            //trackBarExploreParam3.Minimum = Param3Min;
            //trackBarExploreParam3.Maximum = Param3Max;
            trackBarExploreParam3.Value = Param3;

            //labelExploreMinParam3.Text = Param3Min.ToString();
            //labelExploreMaxParam3.Text = Param3Max.ToString();

            trackBarExploreParam4.Value = Math.Max(Math.Min(Param4, trackBarExploreParam4.Maximum), trackBarExploreParam4.Minimum);
            //trackBarExploreParam4.Minimum = Param4Min;
            //trackBarExploreParam4.Maximum = Param4Max;
            trackBarExploreParam4.Value = Param4;

            //labelExploreMinParam4.Text = Param4Min.ToString();
            //labelExploreMaxParam4.Text = Param4Max.ToString();

            numericUpDownExploreParam1.Value = Param1;
            numericUpDownExploreParam2.Value = Param2;
            numericUpDownExploreParam3.Value = Param3;
            numericUpDownExploreParam4.Value = Param4;
        }
        public void SetLabel()
        {

            // Param 1
            textBoxLoadParamName1.Text = Parameter.Param1Name;
            labelExploreParam1.Text = Parameter.Param1Name;
            labelExploreMinParam1.Text = Parameter.Param1Min.ToString();
            labelExploreMaxParam1.Text = Parameter.Param1Max.ToString();
            trackBarExploreParam1.Minimum = Parameter.Param1Min;
            trackBarExploreParam1.Maximum = Parameter.Param1Max;
            trackBarExploreParam1.Value = Parameter.Param1Value;
            numericUpDownExploreParam1.Value = Parameter.Param1Value;

            // Param 2
            textBoxLoadParamName2.Text = Parameter.Param2Name;
            labelExploreParam2.Text = Parameter.Param2Name;
            labelExploreMinParam2.Text = Parameter.Param2Min.ToString();
            labelExploreMaxParam2.Text = Parameter.Param2Max.ToString();
            trackBarExploreParam2.Minimum = Parameter.Param2Min;
            trackBarExploreParam2.Maximum = Parameter.Param2Max;
            trackBarExploreParam2.Value = Parameter.Param2Value;
            numericUpDownExploreParam2.Value = Parameter.Param2Value;

            // Param 3
            textBoxLoadParamName3.Text = Parameter.Param3Name;
            labelExploreParam3.Text = Parameter.Param3Name;
            labelExploreMinParam3.Text = Parameter.Param3Min.ToString();
            labelExploreMaxParam3.Text = Parameter.Param3Max.ToString();
            trackBarExploreParam3.Minimum = Parameter.Param3Min;
            trackBarExploreParam3.Maximum = Parameter.Param3Max;
            trackBarExploreParam3.Value = Parameter.Param3Value;
            numericUpDownExploreParam3.Value = Parameter.Param3Value;

            // Param 4
            textBoxLoadParamName4.Text = Parameter.Param4Name;
            labelExploreParam4.Text = Parameter.Param4Name;
            labelExploreMinParam4.Text = Parameter.Param4Min.ToString();
            labelExploreMaxParam4.Text = Parameter.Param4Max.ToString();
            trackBarExploreParam4.Minimum = Parameter.Param4Min;
            trackBarExploreParam4.Maximum = Parameter.Param4Max;
            trackBarExploreParam4.Value = Parameter.Param4Value;
            numericUpDownExploreParam4.Value = Parameter.Param4Value;


            textBoxLoadImpedance.Text = Parameter.refImp.ToString();
            textBoxLoadProjectname.Text = Parameter.Projectname;

            numericUpDown11.Minimum = (decimal)Math.Round(Parameter.MinFrequencyHz / 1000000, 0);
            numericUpDown11.Maximum = (decimal)Math.Round(Parameter.MaxFrequencyHz / 1000000, 0);
            numericUpDown11.Value = (decimal)Math.Round(Parameter.MinFrequencyHz / 1000000, 0);

            numericUpDown10.Minimum = (decimal)Math.Round(Parameter.MinFrequencyHz / 1000000, 0);
            numericUpDown10.Maximum = (decimal)Math.Round(Parameter.MaxFrequencyHz / 1000000, 0);
            numericUpDown10.Value = (decimal)Math.Round(Parameter.MaxFrequencyHz / 1000000, 0);

        }
        public void SelectFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select a file";
                dialog.Filter = "S1P/S2P-files (*.s1p;*.s2p)|*.s1p;*.s2p|S1P-files (*.s1p)|*.s1p|S2P-files (*.s2p)|*.s2p|All files (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fullPath = dialog.FileName.ToLower();
                    string folderPath = Path.GetDirectoryName(fullPath).ToLower() ?? "";

                    if (!S1PFileName.TryParse(fullPath, out string projectName, out int selectedParam1, out int selectedParam2, out int selectedParam3, out int selectedParam4))
                    {
                        MessageBox.Show("Filnamnet matchar inte \"Namn[_Param1[_Param2[_Param3[_Param4]]]].s1p/.s2p\"");
                        return;
                    }

                    Parameter.Param1Value = selectedParam1;
                    Parameter.Param2Value = selectedParam2;
                    Parameter.Param3Value = selectedParam3;
                    Parameter.Param4Value = selectedParam4;
                    Parameter.Projectname = projectName;
                    Parameter.Path = folderPath;

                    string[] filesInFolder = Directory.GetFiles(folderPath, "*.s1p")
                        .Concat(Directory.GetFiles(folderPath, "*.s2p"))
                        .ToArray();

                    Parameter.Param1Min = 100000;
                    Parameter.Param2Min = 100000;
                    Parameter.Param3Min = 100000;
                    Parameter.Param4Min = 100000;
                    Parameter.Param1Max = 0;
                    Parameter.Param2Max = 0;
                    Parameter.Param3Max = 0;
                    Parameter.Param4Max = 0;

                    var matchedFiles = new List<string>();
                    var matchedFileParams = new List<(int, int, int, int)>();

                    foreach (string file in filesInFolder)
                    {
                        string lowerFile = file.ToLower();

                        // Skip files that don't follow "Name[_Param1[_Param2[_Param3[_Param4]]]].s1p",
                        // and files belonging to a different project - only files with the
                        // same ParamNamn as the one picked are loaded, even if others sit in
                        // the same folder.
                        if (!S1PFileName.TryParse(lowerFile, out string fileProjectName, out int filePart1, out int filePart2, out int filePart3, out int filePart4))
                            continue;
                        if (!fileProjectName.Equals(Parameter.Projectname, StringComparison.Ordinal))
                            continue;

                        matchedFiles.Add(lowerFile);
                        matchedFileParams.Add((filePart1, filePart2, filePart3, filePart4));

                        if (filePart1 < Parameter.Param1Min) { Parameter.Param1Min = filePart1; }
                        if (filePart1 > Parameter.Param1Max) { Parameter.Param1Max = filePart1; }
                        if (filePart2 < Parameter.Param2Min) { Parameter.Param2Min = filePart2; }
                        if (filePart2 > Parameter.Param2Max) { Parameter.Param2Max = filePart2; }
                        if (filePart3 < Parameter.Param3Min) { Parameter.Param3Min = filePart3; }
                        if (filePart3 > Parameter.Param3Max) { Parameter.Param3Max = filePart3; }
                        if (filePart4 < Parameter.Param4Min) { Parameter.Param4Min = filePart4; }
                        if (filePart4 > Parameter.Param4Max) { Parameter.Param4Max = filePart4; }
                    }

                    Parameter.Files = matchedFiles.ToArray();
                    Parameter.FileParams = matchedFileParams.ToArray();

                    XMLfileHandler.ReadFile(Parameter.Path + "\\" + Parameter.Projectname + "_param.xml", listBox1);

                    S1PReader.S1PFiles = new List<SParameter>[Parameter.Files.Length];
                }
            }
        }
        private void ButtonSelectFile_Click(object sender, EventArgs e)
        {
            Parameter.SetExpectedFile = false;
            SelectFile();

            getFilenames(Parameter.Param1Value, Parameter.Param2Value, Parameter.Param3Value, Parameter.Param4Value);

            string markedFile = Parameter.CornerFiles.Count > 0 ? Parameter.CornerFiles[0] : "";
            loadFiles(markedFile); // Load marked file

            if (markedFile != "")
            {
                var markedData = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, markedFile)];
                Parameter.MinFrequencyHz = markedData.Select(d => d.FrequencyHz).Min();
                Parameter.MaxFrequencyHz = markedData.Select(d => d.FrequencyHz).Max();
            }

            SetLabel();
            SetExpectedFile();
            Parameter.SetExpectedFile = true;

            PlotExploreChart();
            loadFiles(); // load all files
            MessageBox.Show($"In folder there is {Parameter.Files.Length} s1p/s2p-files");
        }
        public void getFilenames(int part1, int part2, int part3, int part4)
        {
            Parameter.CornerFiles.Clear();
            Parameter.CornerDiffs.Clear();

            int exactIdx = -1;
            for (int i = 0; i < Parameter.Files.Length; i++)
            {
                var p = Parameter.FileParams[i];
                if (p.Param1 == part1 && p.Param2 == part2 && p.Param3 == part3 && p.Param4 == part4)
                {
                    exactIdx = i;
                    break;
                }
            }

            if (exactIdx >= 0)
            {
                Parameter.CornerFiles.Add(Parameter.Files[exactIdx]);
                Parameter.CornerDiffs.Add(0);
            }
            else
            {
                const int cornerCount = 16;
                string[] cornerFile = new string[cornerCount];
                int[] cornerDiff = new int[cornerCount];
                for (int c = 0; c < cornerCount; c++) { cornerDiff[c] = int.MaxValue; }

                for (int i = 0; i < Parameter.Files.Length; i++)
                {
                    var p = Parameter.FileParams[i];
                    int diffPart = (int)Math.Abs(Math.Sqrt(
                        Math.Pow(10 * (part1 - p.Param1), 2) +
                        Math.Pow(10 * (part2 - p.Param2), 2) +
                        Math.Pow(10 * (part3 - p.Param3), 2) +
                        Math.Pow(10 * (part4 - p.Param4), 2)));

                    for (int c = 0; c < cornerCount; c++)
                    {
                        bool ok =
                            ((c & 1) == 0 ? part1 >= p.Param1 : part1 <= p.Param1) &&
                            ((c & 2) == 0 ? part2 >= p.Param2 : part2 <= p.Param2) &&
                            ((c & 4) == 0 ? part3 >= p.Param3 : part3 <= p.Param3) &&
                            ((c & 8) == 0 ? part4 >= p.Param4 : part4 <= p.Param4);

                        if (ok && diffPart < cornerDiff[c])
                        {
                            cornerDiff[c] = diffPart;
                            cornerFile[c] = Parameter.Files[i];
                        }
                    }
                }

                // Samla unika kandidater för att kunna beräkna totalvikt innan vi filtrerar bort de små
                var tempFiles = new List<string>();
                var tempDiffs = new List<int>();
                var seen = new HashSet<string>();
                for (int c = 0; c < cornerCount; c++)
                {
                    if (cornerDiff[c] != int.MaxValue && seen.Add(cornerFile[c]))
                    {
                        tempFiles.Add(cornerFile[c]);
                        tempDiffs.Add(cornerDiff[c]);
                    }
                }

                if (tempFiles.Count > 0)
                {
                    double weightTot = tempDiffs.Sum(d => 1.0 / Math.Pow(d, 2));

                    for (int i = 0; i < tempFiles.Count; i++)
                    {
                        // Lägg bara till filer vars viktbidrag är större än 1%
                        double pct = 100 * (1.0 / Math.Pow(tempDiffs[i], 2)) / weightTot;
                        if (pct > 0.1)
                        {
                            Parameter.CornerFiles.Add(tempFiles[i]);
                            Parameter.CornerDiffs.Add(tempDiffs[i]);
                        }
                    }
                }
            }

            textBoxExplore.Clear();

            if (Parameter.CornerFiles.Count == 1 && Parameter.CornerDiffs[0] == 0)
            {
                textBoxExplore.AppendText(" 100.0%  " + Path.GetFileName(Parameter.CornerFiles[0]));
            }
            else if (Parameter.CornerFiles.Count > 0)
            {
                // Beräkna om vikten baserat på de kvarvarande filerna för att få exakt 100% totalt i listan
                double finalWeightTot = Parameter.CornerDiffs.Sum(d => 1.0 / Math.Pow(d, 2));

                for (int i = 0; i < Parameter.CornerFiles.Count; i++)
                {
                    double pct = 100 * (1.0 / Math.Pow(Parameter.CornerDiffs[i], 2)) / finalWeightTot;
                    textBoxExplore.AppendText(pct.ToString("F1").PadLeft(6) + "%  " + Path.GetFileName(Parameter.CornerFiles[i]) + "\r\n");
                }
            }
        }
        //public void getFilenames(int part1, int part2, int part3, int part4)
        //{
        //    Parameter.CornerFiles.Clear();
        //    Parameter.CornerDiffs.Clear();

        //    int diffMax = 10000;

        //    int exactIdx = -1;
        //    for (int i = 0; i < Parameter.Files.Length; i++)
        //    {
        //        var p = Parameter.FileParams[i];
        //        if (p.Param1 == part1 && p.Param2 == part2 && p.Param3 == part3 && p.Param4 == part4)
        //        {
        //            exactIdx = i;
        //            break;
        //        }
        //    }

        //    if (exactIdx >= 0)
        //    {
        //        Parameter.CornerFiles.Add(Parameter.Files[exactIdx]);
        //        Parameter.CornerDiffs.Add(0);
        //    }
        //    else
        //    {
        //        // Full 4D corner search: one "corner" per combination of
        //        // above/below on each of the 4 axes (2^4 = 16 corners), each
        //        // holding the closest file that satisfies that combination.
        //        // This generalizes the old 2D quadrant search (4 corners) so
        //        // Param3/Param4 get proper neighbours too, instead of being
        //        // ignored or forcing an exact match.
        //        const int cornerCount = 16;
        //        string[] cornerFile = new string[cornerCount];
        //        int[] cornerDiff = new int[cornerCount];
        //        for (int c = 0; c < cornerCount; c++) { cornerDiff[c] = diffMax; }

        //        for (int i = 0; i < Parameter.Files.Length; i++)
        //        {
        //            var p = Parameter.FileParams[i];
        //            int diffPart = (int)Math.Abs(Math.Sqrt(
        //                Math.Pow(part1 - p.Param1, 2) +
        //                Math.Pow(part2 - p.Param2, 2) +
        //                Math.Pow(part3 - p.Param3, 2) +
        //                Math.Pow(part4 - p.Param4, 2)));

        //            for (int c = 0; c < cornerCount; c++)
        //            {
        //                // Bit 0..3 of c selects, per axis, whether this corner wants
        //                // a file at or below (0) or at or above (1) the requested value.
        //                bool ok =
        //                    ((c & 1) == 0 ? part1 >= p.Param1 : part1 <= p.Param1) &&
        //                    ((c & 2) == 0 ? part2 >= p.Param2 : part2 <= p.Param2) &&
        //                    ((c & 4) == 0 ? part3 >= p.Param3 : part3 <= p.Param3) &&
        //                    ((c & 8) == 0 ? part4 >= p.Param4 : part4 <= p.Param4);

        //                if (ok && diffPart < cornerDiff[c])
        //                {
        //                    cornerDiff[c] = diffPart;
        //                    cornerFile[c] = Parameter.Files[i];
        //                }
        //            }
        //        }

        //        // The same file can be the closest match for several corners at
        //        // once (especially when not all 4 parameters vary) - keep each
        //        // distinct file only once so it isn't double-weighted.
        //        var seen = new HashSet<string>();
        //        for (int c = 0; c < cornerCount; c++)
        //        {
        //            if (cornerDiff[c] < diffMax && seen.Add(cornerFile[c]))
        //            {
        //                Parameter.CornerFiles.Add(cornerFile[c]);
        //                Parameter.CornerDiffs.Add(cornerDiff[c]);
        //            }
        //        }
        //    }

        //    textBoxExplore.Clear();

        //    if (Parameter.CornerFiles.Count == 1 && Parameter.CornerDiffs[0] == 0)
        //    {
        //        textBoxExplore.AppendText(" 100.0%  " + Path.GetFileName(Parameter.CornerFiles[0]));
        //    }
        //    else if (Parameter.CornerFiles.Count > 0)
        //    {
        //        double weightTot = Parameter.CornerDiffs.Sum(d => 1.0 / Math.Pow(d, 2));

        //        for (int i = 0; i < Parameter.CornerFiles.Count; i++)
        //        {
        //            double pct = 100 * (1.0 / Math.Pow(Parameter.CornerDiffs[i], 2)) / weightTot;
        //            textBoxExplore.AppendText(pct.ToString("F1").PadLeft(6) + "%  " + Path.GetFileName(Parameter.CornerFiles[i]) + "\r\n");
        //        }
        //    }
        //}
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                PlotExploreChart();
            }
        }
        private void ButtonSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                Parameter.refImp = int.Parse(textBoxLoadImpedance.Text);
                Parameter.Param1Name = textBoxLoadParamName1.Text;
                Parameter.Param2Name = textBoxLoadParamName2.Text;
                Parameter.Param3Name = textBoxLoadParamName3.Text;
                Parameter.Param4Name = textBoxLoadParamName4.Text;

                XMLfileHandler.SaveFile(Parameter.Path + "\\" + Parameter.Projectname + "_param.xml", listBox1.Items.Cast<object>(), true);
                SetLabel();
                MessageBox.Show("Settings saved!");
            }
            catch (Exception)
            {
                MessageBox.Show("Incorrect value in a text box!");
            }

        }
        private void ButtonSaveFavorites_Click(object sender, EventArgs e)
        {
            string newValue = Parameter.Param1Value.ToString() + "-" + Parameter.Param2Value.ToString();
            if (!listBox1.Items.Contains(newValue))
            {
                listBox1.Items.Add(newValue);
                XMLfileHandler.SaveFile(Parameter.Path + "\\" + Parameter.Projectname + "_param.xml", listBox1.Items.Cast<object>(), true);
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Parameter.SetExpectedFile = false;
                string[] paramx = listBox1.SelectedItem.ToString().Split("-");
                if (paramx.Length == 2)
                {
                    Parameter.Param1Value = int.Parse(paramx[0]);
                    Parameter.Param2Value = int.Parse(paramx[1]);
                }
                loadFiles();
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
            }

        }
        private void ButtonRemoveFavorites_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                XMLfileHandler.SaveFile(Parameter.Path + "\\" + Parameter.Projectname + "_param.xml", listBox1.Items.Cast<object>(), true);
            }

        }
        private void ButtonRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Application.Exit();
        }
        private void ButtonLoadHeatmap_Click(object sender, EventArgs e)
        {

            progressBar3.Minimum = Parameter.Param1Min;
            progressBar3.Maximum = Parameter.Param1Max;
            progressBar3.Step = 1;
            progressBar3.Value = Parameter.Param1Min;

            if (S1PReader.S1PFiles == null || S1PReader.S1PFiles.Length <= 0)
            {
                MessageBox.Show("No files loaded!");
                return;
            }

            string[] parts = textBox11.Text.Split(',');
            double[] freqInIntrest = new double[parts.Length];
            int freqNo = 0;
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    try
                    {
                        double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out freqInIntrest[freqNo]);

                        if (freqInIntrest[freqNo] > 0)
                        {
                            freqInIntrest[freqNo] = freqInIntrest[freqNo] * 1000000;
                            freqNo += 1;
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }

            if (freqNo == 0)
            {
                MessageBox.Show("No valid frequencies in your list?");
                return;
            }


            int freqNoTemp = 0;
            int[] freq = new int[freqNo];

            loadFiles(Parameter.Files[0]);

            for (int i = 0; i < freqNo; i++)
            {
                double freqHz = 0;
                int freqOk = 0;

                for (int ii = 0; ii < S1PReader.S1PFiles[0].Count; ii++)
                {
                    if (S1PReader.S1PFiles[0][ii].FrequencyHz <= freqInIntrest[i])
                    {
                        freqHz = S1PReader.S1PFiles[0][ii].FrequencyHz;
                        freqOk = ii;
                    }
                    if (S1PReader.S1PFiles[0][ii].FrequencyHz > freqInIntrest[i])
                    {
                        if (freqHz > 0)
                        {
                            freq[freqNoTemp] = freqOk;
                            freqNoTemp += 1;
                        }
                        break;
                    }
                }
            }
            freqNo = freqNoTemp;

            if (freqNo <= 0)
            {
                MessageBox.Show("No frequencies from your list in the files?");
                return;
            }

            Parameter.HeatMapValues.Clear();
            int lastP1 = 0;

            for (int P1 = Parameter.Param1Min; P1 < Parameter.Param1Max; P1 += 1)
            {
                var (xMin, xMax) = Parameter.GetParamRange(2, P1, Parameter.Param2Value, Parameter.Param3Value, Parameter.Param4Value);

                for (int P2 = xMin; P2 < xMax; P2 += 1)
                {

                    getFilenames(P1, P2, Parameter.Param3Value, Parameter.Param4Value);
                    var weighted = S1PReader.GetWeightedSParams(loadMissingFromDisk: false);

                    double minVSWR = 100;
                    double avgVSWR = 0;
                    double maxVSWR = 0;

                    for (int i = 0; i < freqNo; i++)
                    {
                        minVSWR = Math.Min(minVSWR, weighted.MinVSWR(freq[i]));
                        maxVSWR = Math.Max(maxVSWR, weighted.MaxVSWR(freq[i]));
                        avgVSWR += weighted.BlendVSWR(freq[i]);
                    }

                    avgVSWR = avgVSWR / freqNo;


                    if (lastP1 != P1)
                    {
                        progressBar3.Value = P1;
                        lastP1 = P1;
                    }


                    Parameter.HeatMapValues.Add(new HeatMap
                    {
                        Param1Value = P1,
                        Param2Value = P2,
                        Min = minVSWR,
                        Avg = avgVSWR,
                        Max = maxVSWR
                    });

                }
            }

            progressBar3.Value = progressBar3.Maximum;
            writeHeatMap(sender, EventArgs.Empty);
            MessageBox.Show("Heatmap loaded!");
        }
        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            int selStart = textBox11.SelectionStart;
            textBox11.Text = Regex.Replace(textBox11.Text, @"[^0-9\., ]", "").Replace("..", ".").Replace(",,", ",");
            textBox11.SelectionStart = selStart;
        }
        public void loadFiles(string filename = "")
        {
            if (S1PReader.S1PFiles == null || S1PReader.S1PFiles.Length <= 0)
            {
                MessageBox.Show("No files loaded!");
                return;
            }


            int loadFileNo = -2;

            if (filename != "") { loadFileNo = Array.IndexOf(Parameter.Files, filename); }

            if (loadFileNo == -2)
            {
                for (int i = 0; i < Parameter.Files.Length; i++)
                {
                    if (S1PReader.S1PFiles[i] == null || S1PReader.S1PFiles[i].Count == 0)
                    {
                        S1PReader.S1PFiles[i] = S1PReader.ReadS1PFile(Parameter.Files[i]);

                        // Kontrollera att filen matchar den första filens svep-inställningar
                        if (i > 0 && Parameter.Points > 0)
                        {
                            if (S1PReader.S1PFiles[i].Count != Parameter.Points
                                   || Math.Abs(S1PReader.S1PFiles[i][0].FrequencyHz - Parameter.MinFrequencyHz) > 0
                                   || Math.Abs(S1PReader.S1PFiles[i][S1PReader.S1PFiles[i].Count - 1].FrequencyHz - Parameter.MaxFrequencyHz) > 0
                                   )
                            {
                                MessageBox.Show($"File {Path.GetFileName(Parameter.Files[i])} has different sweep settings than {Path.GetFileName(Parameter.Files[0])}!", "Sync error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                            }
                        }
                    }

                    textBox12.AppendText("Load: " + Path.GetFileName(Parameter.Files[i]) + "\r\n");
                }
                if (S1PReader.S1PFiles[0] == null || S1PReader.S1PFiles[0].Count == 0)
                {
                    MessageBox.Show("No files?");
                    return;
                }
            }
            else if (loadFileNo >= 0)
            {
                if (S1PReader.S1PFiles[loadFileNo] == null || S1PReader.S1PFiles[loadFileNo].Count == 0)
                {
                    S1PReader.S1PFiles[loadFileNo] = S1PReader.ReadS1PFile(Parameter.Files[loadFileNo]);

                    // Kontrollera mot fil 0 även vid enstaka inläsning
                    //if (loadFileNo != 0 && S1PReader.S1PFiles[0] != null && S1PReader.S1PFiles[0].Count > 0 && Parameter.Points > 0)
                    //{
                    //    if (S1PReader.S1PFiles[loadFileNo].Count != S1PReader.S1PFiles[0].Count
                    //        || S1PReader.S1PFiles[loadFileNo].Count != Parameter.Points)
                    //    {
                    //        MessageBox.Show($"File {Path.GetFileName(filename)} is not compatible with project sweep settings!", "Sync error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    }
                    //}
                    if (loadFileNo != 0 && Parameter.Points > 0 && Parameter.MaxFrequencyHz > 0)
                    {
                        if (S1PReader.S1PFiles[loadFileNo].Count != Parameter.Points
                           || Math.Abs(S1PReader.S1PFiles[loadFileNo][0].FrequencyHz - Parameter.MinFrequencyHz) > 0
                           || Math.Abs(S1PReader.S1PFiles[loadFileNo][S1PReader.S1PFiles[loadFileNo].Count - 1].FrequencyHz - Parameter.MaxFrequencyHz) > 0)
                        {
                            MessageBox.Show($"File {Path.GetFileName(filename)} is not compatible with project sweep settings!", "Sync error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                if (S1PReader.S1PFiles[loadFileNo] == null || S1PReader.S1PFiles[loadFileNo].Count == 0)
                {
                    MessageBox.Show("No files?");
                    return;
                }
            }
            else
            {
                MessageBox.Show("No files? " + filename);
            }
        }
        public void writeHeatMap(object sender, EventArgs e)
        {
            ResetSmithChart();

            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                if (!radioButton.Checked)
                {
                    return;
                }
            }

            if (Parameter.HeatMapValues == null || Parameter.HeatMapValues.Count() <= 0)
            {
                MessageBox.Show("No Heatmap loaded!");
                return;
            }

            formsPlot2.Plot.Clear();
            Coordinates3d[] cs = new Coordinates3d[Parameter.Param1Max * Parameter.Param2Max];

            int csCount = 0;
            foreach (var heatMap in Parameter.HeatMapValues)
            {
                double value = 0;
                if (radioButton3.Checked)
                {
                    value = heatMap.Max;
                }
                else
                {
                    value = heatMap.Avg;
                }


                if (1 <= value && value < 5)
                {
                    cs[csCount] = new(heatMap.Param1Value, heatMap.Param2Value, value);
                    csCount += 1;
                }
            }

            int csNo = 0;
            // place markers at each data point
            double minZtemp = 1;
            double maxZtemp = (double)numericUpDown3.Value / 10;
            double minZ = maxZtemp;
            double maxZ = minZtemp;
            //IColormap cmap = new ScottPlot.Colormaps.Algae();
            IColormap cmap = new ScottPlot.Colormaps.Greens();
            //IColormap cmap = new ScottPlot.Colormaps.Blues();
            for (int ii = 0; ii < cs.Length; ii++)
            {
                if (cs[ii].Z >= minZtemp && cs[ii].Z <= maxZtemp)
                {
                    if (cs[ii].Z <= minZ)
                    {
                        minZ = cs[ii].Z;
                    }
                    if (cs[ii].Z >= maxZ)
                    {
                        maxZ = cs[ii].Z;
                    }
                }
            }

            double spanZ = maxZ - minZ;

            for (int ii = 0; ii < cs.Length; ii++)
            {
                if (cs[ii].Z >= minZ && cs[ii].Z <= maxZ)
                {
                    csNo += 1;
                    double fraction = (cs[ii].Z - minZ) / (spanZ);
                    var marker = formsPlot2.Plot.Add.Marker(cs[ii].Y, cs[ii].X);
                    marker.Color = cmap.GetColor(fraction).WithAlpha(.8);
                    //marker.Color = cmap.GetColor(fraction).WithAlpha((double)numericUpDown4.Value/10);
                    marker.Size = 10;
                }
            }

            // show contour lines
            //var contour = formsPlot2.Plot.Add.ContourLines(cs);

            // style the plot
            //formsPlot2.Plot.Axes.TightMargins();
            //formsPlot2.Plot.HideGrid();
            formsPlot2.Plot.Axes.AutoScale();
            //formsPlot2.AutoSize = true;

            formsPlot2.Plot.XLabel(Parameter.Param2Name);
            formsPlot2.Plot.YLabel(Parameter.Param1Name);
            // formsPlot2.UserInputProcessor.IsEnabled = true; // Mouse and keyboard events are disabled
            formsPlot2.Refresh();

            //MessageBox.Show("The End!");

        }
        public void writeBandwidthMap(object sender, EventArgs e)
        {
            ResetSmithChart();

            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                if (!radioButton.Checked)
                {
                    return;
                }
            }

            if (Parameter.BandwidthMapValues == null || Parameter.BandwidthMapValues.Count() <= 0)
            {
                MessageBox.Show("No BandwidthMap loaded!");
                return;
            }

            formsPlot2.Plot.Clear();
            // Coordinates3d[] cs = new Coordinates3d[Parameter.Param1Max * Parameter.Param2Max];
            //            Coordinates3d[] cs;
            List<Coordinates3d> coordList = new List<Coordinates3d> { };

            int csCount = 0;

            double maxValue = Parameter.BandwidthMapValues.Select(d => d.Avg).Max();
            double minValue = Parameter.BandwidthMapValues.Select(d => d.Avg).Min();

            minValue = Math.Min(minValue, maxValue / 2);


            foreach (var BandwidthMap in Parameter.BandwidthMapValues)
            {
                double value = BandwidthMap.Avg;

                if ((double)numericUpDown7.Value <= (100 * value / maxValue))
                {
                    //minValue = Math.Min(minValue, value);
                    //cs[csCount] = new(BandwidthMap.Param1Value, BandwidthMap.Param2Value, value);
                    //Coordinates3d newPoint = new Coordinates3d(7, 8, 9);
                    coordList.Add(new Coordinates3d(BandwidthMap.Param1Value, BandwidthMap.Param2Value, value));

                    // Lägg till den nya punkten genom att skapa en ny array:
                    //                    cs = cs.Append(newPoint).ToArray();
                    //                  csCount += 1;
                }
            }
            Coordinates3d[] cs = coordList.ToArray();

            int csNo = 0;
            IColormap cmap = new ScottPlot.Colormaps.Algae();
            //IColormap cmap = new ScottPlot.Colormaps.Greens();
            //IColormap cmap = new ScottPlot.Colormaps.Blues();

            double spanZ = maxValue - minValue;

            for (int ii = 0; ii < cs.Length; ii++)
            {
                if (cs[ii].Z >= minValue && cs[ii].Z <= maxValue)
                {
                    csNo += 1;
                    double fraction = (cs[ii].Z - minValue) / (spanZ);
                    var marker = formsPlot2.Plot.Add.Marker(cs[ii].Y, cs[ii].X);
                    marker.Color = cmap.GetColor(fraction).WithAlpha(.8);
                    //marker.Color = cmap.GetColor(fraction).WithAlpha((double)numericUpDown4.Value/10);
                    marker.Size = 10;
                }
            }

            // show contour lines
            //var contour = formsPlot2.Plot.Add.ContourLines(cs);

            // style the plot
            //formsPlot2.Plot.Axes.TightMargins();
            //formsPlot2.Plot.HideGrid();
            formsPlot2.Plot.Axes.AutoScale();
            //formsPlot2.AutoSize = true;

            formsPlot2.Plot.XLabel(Parameter.Param2Name);
            formsPlot2.Plot.YLabel(Parameter.Param1Name);
            // formsPlot2.UserInputProcessor.IsEnabled = true; // Mouse and keyboard events are disabled
            formsPlot2.Refresh();

            //MessageBox.Show("The End!");

        }
        public void PlotCenterFrequencyMap(object sender, EventArgs e)
        {
            ResetSmithChart();

            formsPlot2.Plot.Clear();

            if (Parameter.CenterFrequencyMapValues == null || Parameter.CenterFrequencyMapValues.Count <= 0)
            {
                MessageBox.Show("Load CenterFrequencyMap first!");
                return;
            }
            double inVSWR = (double)numericUpDown2.Value;


            int minMHz = (int)Parameter.CenterFrequencyMapValues.Min(row => row.FrequencyHz);
            int maxMHz = (int)Parameter.CenterFrequencyMapValues.Max(row => row.FrequencyHz);


            double[] MHz = new double[maxMHz + 1];
            double[] Counts = new double[maxMHz + 1];

            for (int i = 0; i < Parameter.CenterFrequencyMapValues.Count; i++)
            {
                MHz[i] = Parameter.CenterFrequencyMapValues[i].FrequencyHz;

                int VSWRvalues = 0;
                double[] countVSWR = new double[5];
                double[] VSWR = new double[5];

                if (inVSWR == 12) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR12; VSWR[VSWRvalues] = 12; VSWRvalues += 1; }
                if (inVSWR == 13) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR13; VSWR[VSWRvalues] = 13; VSWRvalues += 1; }
                if (inVSWR == 14) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR14; VSWR[VSWRvalues] = 14; VSWRvalues += 1; }
                if (inVSWR == 15) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR15; VSWR[VSWRvalues] = 15; VSWRvalues += 1; }
                if (inVSWR == 16) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR16; VSWR[VSWRvalues] = 16; VSWRvalues += 1; }
                if (inVSWR == 17) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR17; VSWR[VSWRvalues] = 17; VSWRvalues += 1; }
                if (inVSWR == 18) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR18; VSWR[VSWRvalues] = 18; VSWRvalues += 1; }
                if (inVSWR == 19) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR19; VSWR[VSWRvalues] = 19; VSWRvalues += 1; }
                if (inVSWR >= 20 && inVSWR < 30) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR20; VSWR[VSWRvalues] = 20; VSWRvalues += 1; }
                if (inVSWR >= 21 && inVSWR < 40) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR30; VSWR[VSWRvalues] = 30; VSWRvalues += 1; }
                if (inVSWR >= 31 && inVSWR < 50) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR40; VSWR[VSWRvalues] = 40; VSWRvalues += 1; }
                if (inVSWR >= 41) { countVSWR[VSWRvalues] += Parameter.CenterFrequencyMapValues[i].VSWR50; VSWR[VSWRvalues] = 50; VSWRvalues += 1; }

                if (VSWRvalues == 1)
                {
                    Counts[i] = countVSWR[0];
                }
                else
                {
                    double diff = countVSWR[1] - countVSWR[0];
                    double pr = (inVSWR - VSWR[0]) / (VSWR[1] - VSWR[0]);
                    Counts[i] = countVSWR[0] + diff * pr;
                }
            }

            double maxCount = Counts.Max();
            double[] prCounts = Counts.Select(n => (n / maxCount) * 100).ToArray();


            var bars1 = formsPlot2.Plot.Add.Bars(MHz, prCounts);
            // bars1.LegendText = "Alpha";

            formsPlot2.Plot.ShowLegend(Alignment.UpperLeft);
            formsPlot2.Plot.Axes.Margins(bottom: 0);

            formsPlot2.Plot.XLabel("Frequency (MHz)");
            formsPlot2.Plot.YLabel("%");
            formsPlot2.Plot.Axes.SetLimitsX(minMHz, maxMHz);

            formsPlot2.Refresh();

        }
        private void ButtonLoadCentermap_Click(object sender, EventArgs e)
        {
            progressBar2.Minimum = Parameter.Param1Min;
            progressBar2.Maximum = Parameter.Param1Max;
            progressBar2.Step = 1;
            progressBar2.Value = Parameter.Param1Min;


            List<SParameter> S1PFile0 = new List<SParameter> { };
            S1PFile0 = S1PReader.S1PFiles[0];

            int FrequencyMHz = 0;

            Parameter.CenterFrequencyMapValues.Clear();

            for (int RowNo = 0; RowNo < S1PFile0.Count(); RowNo++)
            {

                if (FrequencyMHz != (int)(S1PFile0[RowNo].FrequencyHz / 1000000))
                {

                    FrequencyMHz = (int)(S1PFile0[RowNo].FrequencyHz / 1000000);

                    Parameter.CenterFrequencyMapValues.Add(new CenterFrequencyMap
                    {
                        FrequencyHz = FrequencyMHz,
                        FileRowNo = RowNo
                    });
                }
            }




            int lastP1 = 0;

            for (int P1 = Parameter.Param1Min; P1 < Parameter.Param1Max; P1 += 1)
            {
                var (xMin, xMax) = Parameter.GetParamRange(2, P1, Parameter.Param2Value, Parameter.Param3Value, Parameter.Param4Value);

                for (int P2 = xMin; P2 < xMax; P2 += 1)
                {

                    getFilenames(P1, P2, Parameter.Param3Value, Parameter.Param4Value);
                    var weighted = S1PReader.GetWeightedSParams(loadMissingFromDisk: false);

                    int countVSWR = 0;

                    for (int i = 0; i < Parameter.CenterFrequencyMapValues.Count; i++)
                    {
                        double avgTemp = weighted.BlendVSWR(Parameter.CenterFrequencyMapValues[i].FileRowNo);

                        if (avgTemp <= 1.2) { Parameter.CenterFrequencyMapValues[i].VSWR12 += 1; }
                        if (avgTemp <= 1.3) { Parameter.CenterFrequencyMapValues[i].VSWR13 += 1; }
                        if (avgTemp <= 1.4) { Parameter.CenterFrequencyMapValues[i].VSWR14 += 1; }
                        if (avgTemp <= 1.5) { Parameter.CenterFrequencyMapValues[i].VSWR15 += 1; }
                        if (avgTemp <= 1.6) { Parameter.CenterFrequencyMapValues[i].VSWR16 += 1; }
                        if (avgTemp <= 1.7) { Parameter.CenterFrequencyMapValues[i].VSWR17 += 1; }
                        if (avgTemp <= 1.8) { Parameter.CenterFrequencyMapValues[i].VSWR18 += 1; }
                        if (avgTemp <= 1.9) { Parameter.CenterFrequencyMapValues[i].VSWR19 += 1; }
                        if (avgTemp <= 2.0) { Parameter.CenterFrequencyMapValues[i].VSWR20 += 1; }
                        if (avgTemp <= 3.0) { Parameter.CenterFrequencyMapValues[i].VSWR30 += 1; }
                        if (avgTemp <= 4.0) { Parameter.CenterFrequencyMapValues[i].VSWR40 += 1; }
                        if (avgTemp <= 5.0) { Parameter.CenterFrequencyMapValues[i].VSWR50 += 1; }

                    }

                    if (lastP1 != P1)
                    {
                        progressBar2.Value = P1;
                        lastP1 = P1;
                    }
                }
            }

            progressBar2.Value = progressBar2.Maximum;
            PlotCenterFrequencyMap(sender, EventArgs.Empty);
            MessageBox.Show("CountMap loaded!");
        }
        private void ButtonViewHeatmap_Click_1(object sender, EventArgs e)
        {
            writeHeatMap(sender, EventArgs.Empty);
        }
        private void ButtonViewExploremap_Click(object sender, EventArgs e)
        {
            PlotExploreChart();
        }
        private void numericUpDownExploreParam1_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                int newValue = (int)numericUpDownExploreParam1.Value;
                if (newValue > Parameter.Param1Max)
                {
                    newValue = Parameter.Param1Max;
                }
                if (newValue < Parameter.Param1Min)
                {
                    newValue = Parameter.Param1Min;
                }

                Parameter.Param1Value = newValue;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }
        }
        private void numericUpDownExploreParam2_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                int newValue = (int)numericUpDownExploreParam2.Value;
                if (newValue > Parameter.Param2Max)
                {
                    newValue = Parameter.Param2Max;
                }
                if (newValue < Parameter.Param2Min)
                {
                    newValue = Parameter.Param2Min;
                }
                Parameter.Param2Value = newValue;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }

        }
        private void numericUpDownExploreParam3_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                int newValue = (int)numericUpDownExploreParam3.Value;
                if (newValue > Parameter.Param3Max)
                {
                    newValue = Parameter.Param3Max;
                }
                if (newValue < Parameter.Param3Min)
                {
                    newValue = Parameter.Param3Min;
                }
                Parameter.Param3Value = newValue;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }

        }
        private void numericUpDownExploreParam4_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                int newValue = (int)numericUpDownExploreParam4.Value;
                if (newValue > Parameter.Param4Max)
                {
                    newValue = Parameter.Param4Max;
                }
                if (newValue < Parameter.Param4Min)
                {
                    newValue = Parameter.Param4Min;
                }
                Parameter.Param4Value = newValue;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }

        }
        private void ButtonViewCentermap_Click(object sender, EventArgs e)
        {
            PlotCenterFrequencyMap(sender, EventArgs.Empty);
        }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            PlotCenterFrequencyMap(sender, EventArgs.Empty);
        }
        private void ButtonLoadBandwidthmap_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = Parameter.Param1Min;
            progressBar1.Maximum = Parameter.Param1Max;
            progressBar1.Step = 1;
            progressBar1.Value = Parameter.Param1Min;

            double inVSWR = (double)numericUpDown4.Value / 10;

            if (S1PReader.S1PFiles == null || S1PReader.S1PFiles.Length <= 0)
            {
                MessageBox.Show("No files loaded!");
                return;
            }


            Parameter.BandwidthMapValues.Clear();
            int lastP1 = 0;

            for (int P1 = Parameter.Param1Min; P1 < Parameter.Param1Max; P1 += 1)
            {
                var (xMin, xMax) = Parameter.GetParamRange(2, P1, Parameter.Param2Value, Parameter.Param3Value, Parameter.Param4Value);


                for (int P2 = xMin + 1; P2 < xMax; P2 += 1)
                {
                    getFilenames(P1, P2, Parameter.Param3Value, Parameter.Param4Value);
                    var weighted = S1PReader.GetWeightedSParams(loadMissingFromDisk: false);

                    bool avgBool = false;
                    int avgNo = 0;
                    int[] avgCount = new int[weighted.PointCount + 1];

                    for (int i = 0; i < weighted.PointCount; i++)
                    {
                        double avgTemp = weighted.BlendVSWR(i);

                        if (!avgBool && avgTemp <= inVSWR)
                        {
                            avgBool = true;
                            avgNo += 1;
                        }
                        else if (avgTemp > inVSWR)
                        {
                            avgBool = false;
                        }

                        if (avgBool)
                        {
                            avgCount[avgNo] += 1;
                        }

                    }

                    if (lastP1 != P1)
                    {
                        progressBar1.Value = P1;

                        lastP1 = P1;
                    }

                    if (avgCount.Max() > 0)
                    {
                        Parameter.BandwidthMapValues.Add(new BandwidthMap
                        {
                            Param1Value = P1,
                            Param2Value = P2,
                            Avg = avgCount.Max()
                        });

                    }

                }
            }

            //progressBar1.MarqueeAnimationSpeed = 0;
            //progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Value = progressBar1.Maximum;

            writeBandwidthMap(sender, EventArgs.Empty);
            MessageBox.Show("BandwidthMap loaded!");

        }
        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            writeBandwidthMap(sender, EventArgs.Empty);
        }
        private void ButtonViewBandwidth_Click(object sender, EventArgs e)
        {
            writeBandwidthMap(sender, EventArgs.Empty);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            Parameter.SetExpectedFile = false;

            if (comboBox1.SelectedItem == "Conductance_G S11") { numericUpDown9.Value = -0.1m; numericUpDown1.Value = 0.1m; }
            else if (comboBox1.SelectedItem == "Magnitude S11") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "ImpedanceMagnitude S11") { numericUpDown9.Value = 0; numericUpDown1.Value = 100; }
            else if (comboBox1.SelectedItem == "LinMag S11") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "Magnitude S21") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "Magnitude S11 + S21") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "LinMag S21") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "LinMag S11 + S21") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "LogMag S11") { numericUpDown9.Value = -60; numericUpDown1.Value = 0; }
            else if (comboBox1.SelectedItem == "LogMag S21") { numericUpDown9.Value = -60; numericUpDown1.Value = 0; }
            else if (comboBox1.SelectedItem == "LogMag S11 + S21") { numericUpDown9.Value = -60; numericUpDown1.Value = 0; }
            else if (comboBox1.SelectedItem == "PhaseDegrees S11") { numericUpDown9.Value = -90; numericUpDown1.Value = 90; }
            else if (comboBox1.SelectedItem == "PhaseDegrees S21") { numericUpDown9.Value = -90; numericUpDown1.Value = 90; }
            else if (comboBox1.SelectedItem == "PhaseDegrees S11 + S21") { numericUpDown9.Value = -90; numericUpDown1.Value = 90; }
            else if (comboBox1.SelectedItem == "PhaseRadians S11") { numericUpDown9.Value = -2; numericUpDown1.Value = 2; }
            else if (comboBox1.SelectedItem == "PhaseRadians S21") { numericUpDown9.Value = -2; numericUpDown1.Value = 2; }
            else if (comboBox1.SelectedItem == "PhaseRadians S11 + S21") { numericUpDown9.Value = -2; numericUpDown1.Value = 2; }
            else if (comboBox1.SelectedItem == "Reactance_X S11") { numericUpDown9.Value = -20; numericUpDown1.Value = 20; }
            else if (comboBox1.SelectedItem == "ReflectedPower S11") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "TransmittedPower S21") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "ReflectedPower S11 + TransmittedPower S21") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "Resistance_R S11") { numericUpDown9.Value = 0; numericUpDown1.Value = 100; }
            else if (comboBox1.SelectedItem == "ReturnLoss_RL S11") { numericUpDown9.Value = 1; numericUpDown1.Value = 50; }
            else if (comboBox1.SelectedItem == "InsertionLoss S21") { numericUpDown9.Value = 1; numericUpDown1.Value = 50; }
            else if (comboBox1.SelectedItem == "ReturnLoss_RL S11 + InsertionLoss S21") { numericUpDown9.Value = 1; numericUpDown1.Value = 50; }
            else if (comboBox1.SelectedItem == "Susceptance_B S11") { numericUpDown9.Value = -0.1m; numericUpDown1.Value = 0.1m; }
            else if (comboBox1.SelectedItem == "VSWR S11") { numericUpDown9.Value = 1; numericUpDown1.Value = 10; }
            else if (comboBox1.SelectedItem == "VSWR S21") { numericUpDown9.Value = 1; numericUpDown1.Value = 40; }
            else if (comboBox1.SelectedItem == "VSWR S11 + S21") { numericUpDown9.Value = 1; numericUpDown1.Value = 40; }
            else { numericUpDown9.Value = 1; numericUpDown1.Value = 10; }

            PlotExploreChart();

            Parameter.SetExpectedFile = true;
        }
        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                PlotExploreChart();
            }
        }
        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                PlotExploreChart();
            }
        }
        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                PlotExploreChart();
            }
        }
        private bool AreFilesCompatible(List<SParameter> master, List<SParameter> current, string currentFileName, out string errorMessage)
        {
            errorMessage = "";
            if (master == null || current == null) return true;

            if (master.Count != current.Count)
            {
                errorMessage = $"Filen '{currentFileName}' har {current.Count} punkter, men huvudfilen har {master.Count}.";
                return false;
            }

            // Kontrollera första och sista frekvensen (räcker oftast för att se om svepet är likadant)
            if (Math.Abs(master[0].FrequencyHz - current[0].FrequencyHz) > 1.0 ||
                Math.Abs(master.Last().FrequencyHz - current.Last().FrequencyHz) > 1.0)
            {
                errorMessage = $"Filen '{currentFileName}' har ett annat frekvensområde ({current[0].FrequencyHz / 1e6:F1} - {current.Last().FrequencyHz / 1e6:F1} MHz).";
                return false;
            }

            return true;
        }
    }
}
