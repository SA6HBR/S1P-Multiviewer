using ScottPlot;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
            formsPlot2.Plot.YLabel("VSWR");
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

            List<SParameter> sParams1 = new List<SParameter> { };
            List<SParameter> sParams2 = new List<SParameter> { };
            List<SParameter> sParams3 = new List<SParameter> { };
            List<SParameter> sParams4 = new List<SParameter> { };

            getFilenames(Parameter.Param1Value, Parameter.Param2Value);

            if (Parameter.FileName1diff < 10000)
            {
                loadFiles(Parameter.FileName1);
                sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
            }
            if (Parameter.FileName2diff < 10000)
            {
                loadFiles(Parameter.FileName2);
                sParams2 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName2)];
            }
            if (Parameter.FileName3diff < 10000)
            {
                loadFiles(Parameter.FileName3);
                sParams3 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName3)];
            }
            if (Parameter.FileName4diff < 10000)
            {
                loadFiles(Parameter.FileName4);
                sParams4 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName4)];
            }

            double weight1 = 1 / Math.Pow(Parameter.FileName1diff, 2);
            double weight2 = 1 / Math.Pow(Parameter.FileName2diff, 2);
            double weight3 = 1 / Math.Pow(Parameter.FileName3diff, 2);
            double weight4 = 1 / Math.Pow(Parameter.FileName4diff, 2);

            int fileStatus1 = 1;
            int fileStatus2 = 1;
            int fileStatus3 = 1;
            int fileStatus4 = 1;

            if (sParams1.Count == 0) { fileStatus1 = 0; }
            if (sParams2.Count == 0) { fileStatus2 = 0; }
            if (sParams3.Count == 0) { fileStatus3 = 0; }
            if (sParams4.Count == 0) { fileStatus4 = 0; }

            if (Parameter.FileName1 == Parameter.FileName2) { fileStatus2 = 0; }
            if (Parameter.FileName1 == Parameter.FileName3 || Parameter.FileName2 == Parameter.FileName3) { fileStatus3 = 0; }
            if (Parameter.FileName1 == Parameter.FileName4 || Parameter.FileName2 == Parameter.FileName4 || Parameter.FileName3 == Parameter.FileName4) { fileStatus4 = 0; }

            formsPlot1.Plot.Clear();
            var smith = formsPlot1.Plot.Add.SmithChartAxis();

            double MinFrequencyHz = (double)numericUpDown11.Value * 1000000;
            double MaxFrequencyHz = (double)numericUpDown10.Value * 1000000;

            for (int i = 0; i < sParams1.Count(); i++)
            {

                if (MinFrequencyHz <= sParams1[i].FrequencyHz && sParams1[i].FrequencyHz <= MaxFrequencyHz)
                {

                    Complex complexIn1 = 0;
                    Complex complexIn2 = 0;
                    Complex complexIn3 = 0;
                    Complex complexIn4 = 0;

                    if (sParams1.Count() > 0) { complexIn1 = new Complex(sParams1[i].Real, sParams1[i].Imag); }
                    if (sParams2.Count() > 0) { complexIn2 = new Complex(sParams2[i].Real, sParams2[i].Imag); }
                    if (sParams3.Count() > 0) { complexIn3 = new Complex(sParams3[i].Real, sParams3[i].Imag); }
                    if (sParams4.Count() > 0) { complexIn4 = new Complex(sParams4[i].Real, sParams4[i].Imag); }

                    if (Parameter.FileName1diff > 0)
                    {
                        if (fileStatus1 > 0) { complexIn1 = complexIn1 * weight1; }
                        if (fileStatus2 > 0) { complexIn1 += complexIn2 * weight2; }
                        if (fileStatus3 > 0) { complexIn1 += complexIn3 * weight3; }
                        if (fileStatus4 > 0) { complexIn1 += complexIn4 * weight4; }

                        complexIn1 = complexIn1 / (weight1 * fileStatus1 + weight2 * fileStatus2 + weight3 * fileStatus3 + weight4 * fileStatus4);
                    }

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
        public void PlotExploreMultiChart()
        {

            List<SParameter> sParams1 = new List<SParameter> { };
            List<SParameter> sParams2 = new List<SParameter> { };
            List<SParameter> sParams3 = new List<SParameter> { };
            List<SParameter> sParams4 = new List<SParameter> { };

            getFilenames(Parameter.Param1Value, Parameter.Param2Value);


            if (Parameter.FileName1diff < 10000)
            {
                loadFiles(Parameter.FileName1);
                sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
            }
            if (Parameter.FileName2diff < 10000)
            {
                loadFiles(Parameter.FileName2);
                sParams2 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName2)];
            }
            if (Parameter.FileName3diff < 10000)
            {
                loadFiles(Parameter.FileName3);
                sParams3 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName3)];
            }
            if (Parameter.FileName4diff < 10000)
            {
                loadFiles(Parameter.FileName4);
                sParams4 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName4)];
            }

            var frequencies = sParams1.Select(d => (d.FrequencyHz / 1000000)).ToArray();
            double[] plotValues = new double[frequencies.Length];


            double weight1 = 1 / Math.Pow(Parameter.FileName1diff, 2);
            double weight2 = 1 / Math.Pow(Parameter.FileName2diff, 2);
            double weight3 = 1 / Math.Pow(Parameter.FileName3diff, 2);
            double weight4 = 1 / Math.Pow(Parameter.FileName4diff, 2);

            int fileStatus1 = 1;
            int fileStatus2 = 1;
            int fileStatus3 = 1;
            int fileStatus4 = 1;

            if (sParams1.Count == 0) { fileStatus1 = 0; }
            if (sParams2.Count == 0) { fileStatus2 = 0; }
            if (sParams3.Count == 0) { fileStatus3 = 0; }
            if (sParams4.Count == 0) { fileStatus4 = 0; }

            if (Parameter.FileName1 == Parameter.FileName2) { fileStatus2 = 0; }
            if (Parameter.FileName1 == Parameter.FileName3 || Parameter.FileName2 == Parameter.FileName3) { fileStatus3 = 0; }
            if (Parameter.FileName1 == Parameter.FileName4 || Parameter.FileName2 == Parameter.FileName4 || Parameter.FileName3 == Parameter.FileName4) { fileStatus4 = 0; }

            formsPlot1.Plot.Clear();
            var smith = formsPlot1.Plot.Add.SmithChartAxis();


            //foreach (var s in sParams)
            for (int i = 0; i < sParams1.Count(); i++)
            {


                Complex complexIn1 = 0;
                Complex complexIn2 = 0;
                Complex complexIn3 = 0;
                Complex complexIn4 = 0;

                if (sParams1.Count() > 0) { complexIn1 = new Complex(sParams1[i].Real, sParams1[i].Imag); }
                if (sParams2.Count() > 0) { complexIn2 = new Complex(sParams2[i].Real, sParams2[i].Imag); }
                if (sParams3.Count() > 0) { complexIn3 = new Complex(sParams3[i].Real, sParams3[i].Imag); }
                if (sParams4.Count() > 0) { complexIn4 = new Complex(sParams4[i].Real, sParams4[i].Imag); }

                if (Parameter.FileName1diff > 0)
                {
                    if (fileStatus1 > 0) { complexIn1 = complexIn1 * weight1; }
                    if (fileStatus2 > 0) { complexIn1 += complexIn2 * weight2; }
                    if (fileStatus3 > 0) { complexIn1 += complexIn3 * weight3; }
                    if (fileStatus4 > 0) { complexIn1 += complexIn4 * weight4; }

                    complexIn1 = complexIn1 / (weight1 * fileStatus1 + weight2 * fileStatus2 + weight3 * fileStatus3 + weight4 * fileStatus4);
                }

                if (comboBox1.SelectedItem == "Conductance_G") { plotValues[i] = S11Converter.Conductance_G(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "GammaMagnitude") { plotValues[i] = S11Converter.GammaMagnitude(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "ImpedanceMagnitude") { plotValues[i] = S11Converter.ImpedanceMagnitude(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "LinMag") { plotValues[i] = S11Converter.LinMag(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "LogMag") { plotValues[i] = S11Converter.LogMag(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "PhaseDegrees") { plotValues[i] = S11Converter.PhaseDegrees(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "PhaseRadians") { plotValues[i] = S11Converter.PhaseRadians(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "Reactance_X") { plotValues[i] = S11Converter.Reactance_X(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "ReflectedPower") { plotValues[i] = S11Converter.ReflectedPower(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "Resistance_R") { plotValues[i] = S11Converter.Resistance_R(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "ReturnLoss_RL") { plotValues[i] = S11Converter.ReturnLoss_RL(complexIn1.Real, complexIn1.Imaginary); }
                else if (comboBox1.SelectedItem == "Susceptance_B") { plotValues[i] = S11Converter.Susceptance_B(complexIn1.Real, complexIn1.Imaginary); }
                else { plotValues[i] = S11Converter.VSWR(complexIn1.Real, complexIn1.Imaginary); }

            }

            formsPlot2.Plot.Clear();
            formsPlot2.Plot.Add.Scatter(frequencies, plotValues);
            formsPlot2.Plot.XLabel("Frequency (MHz)");
            formsPlot2.Plot.YLabel(comboBox1.SelectedItem.ToString());
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
                Parameter.Param1Value = trackBar1.Value;
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
                Parameter.Param2Value = trackBar2.Value;
                Parameter.SetExpectedFile = false;
                SetExpectedFile();
                PlotExploreChart();
                Parameter.SetExpectedFile = true;
                listBox1.ClearSelected();
            }
        }
        public void SetExpectedFile()
        {
            int ParamTotMin = Parameter.Min - Parameter.Diff;
            int ParamTotMax = Parameter.Max - Parameter.Diff;
            int Param1 = Parameter.Param1Value; // trackBar1.Value;
            int Param2 = Parameter.Param2Value; // trackBar2.Value;
            int Param2Min = Parameter.Param2Min;
            int Param2Max = Parameter.Param2Max;


            if (Param1 < ParamTotMin)
            {
                Param2Min = Math.Max(ParamTotMin - Param1, Param2Min);
                if (Param2 < Param2Min)
                {
                    Param2 = Param2Min;
                }
            }

            if ((Param1 + Param2Max) > ParamTotMax)
            {
                Param2Max = Param2Max - ((Param1 + Param2Max) - ParamTotMax);
                if (Param2 > Param2Max)
                {
                    Param2 = Param2Max;
                }
            }

            Parameter.Param1Value = Param1;
            Parameter.Param2Value = Param2;

            trackBar1.Value = Param1;

            trackBar2.Value = Math.Max(Math.Min(Param2, trackBar2.Maximum), trackBar2.Minimum);
            trackBar2.Minimum = Param2Min;
            trackBar2.Maximum = Param2Max;
            trackBar2.Value = Param2;

            label4.Text = Param2Min.ToString();
            label8.Text = Param2Max.ToString();

            numericUpDown5.Value = Param1;
            numericUpDown6.Value = Param2;
        }
        public void SetLabel()
        {
            label1.Text = Parameter.Param1Name;
            label4.Text = Parameter.Param1Value.ToString();
            trackBar1.Minimum = Parameter.Param1Min;
            trackBar1.Maximum = Parameter.Param1Max;
            trackBar1.Value = Parameter.Param1Value;

            label5.Text = Parameter.Param2Name;
            label8.Text = Parameter.Param2Value.ToString();
            trackBar2.Minimum = Parameter.Param2Min;
            trackBar2.Maximum = Parameter.Param2Max;
            trackBar2.Value = Parameter.Param2Value;
            numericUpDown5.Value = Parameter.Param1Value;
            numericUpDown6.Value = Parameter.Param2Value;

            textBox1.Text = Parameter.Min.ToString();
            textBox7.Text = Parameter.Max.ToString();
            textBox8.Text = Parameter.Diff.ToString();
            textBox9.Text = Parameter.refImp.ToString();
            textBox2.Text = Parameter.Projectname;
            textBox5.Text = Parameter.Param1Name;
            textBox6.Text = Parameter.Param2Name;

            label16.Text = Parameter.Param1Min.ToString();
            label17.Text = Parameter.Param1Max.ToString();

            numericUpDown11.Minimum = (decimal)Math.Round(Parameter.MinFrequencyHz / 1000000, 0);
            numericUpDown11.Value = (decimal)Math.Round(Parameter.MinFrequencyHz / 1000000, 0);
            numericUpDown11.Maximum = (decimal)Math.Round(Parameter.MaxFrequencyHz / 1000000, 0);

            numericUpDown10.Minimum = (decimal)Math.Round(Parameter.MinFrequencyHz / 1000000, 0);
            numericUpDown10.Value = (decimal)Math.Round(Parameter.MaxFrequencyHz / 1000000, 0);
            numericUpDown10.Maximum = (decimal)Math.Round(Parameter.MaxFrequencyHz / 1000000, 0);

        }
        public void SelectFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select a file";
                dialog.Filter = "S1P-files (*.s1p)|*.s1p|All files (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fullPath = dialog.FileName.ToLower();
                    string folderPath = Path.GetDirectoryName(fullPath).ToLower() ?? "";
                    string fileName = Path.GetFileNameWithoutExtension(fullPath).ToLower() ?? "";
                    string[] fileNamePart = fileName.Split('_');

                    if (fileNamePart.Length == 3)
                    {
                        Parameter.Param1Value = int.Parse(fileNamePart[1]);
                        Parameter.Param2Value = int.Parse(fileNamePart[2]);
                        Parameter.Projectname = fileNamePart[0].ToLower();
                        Parameter.Path = folderPath;

                        string[] files = Directory.GetFiles(folderPath, "*.s1p");
                        Parameter.Files = files.Select(f => f.ToLower()).ToArray();

                        //Parameter.Files = Directory.GetFiles(folderPath, "*.s1p");

                        Parameter.Param1Min = 100000;
                        Parameter.Param2Min = 100000;
                        Parameter.Param1Max = 0;
                        Parameter.Param2Max = 0;

                        foreach (string file in Parameter.Files)
                        {
                            //listBox2.Items.Add(Path.GetFileNameWithoutExtension(file));
                            listBox2.Items.Add(Path.GetFileName(file));

                            string[] fileNamePart2 = Path.GetFileNameWithoutExtension(file).Split('_');

                            if (int.Parse(fileNamePart2[1]) < Parameter.Param1Min)
                            {
                                Parameter.Param1Min = int.Parse(fileNamePart2[1]);
                            }

                            if (int.Parse(fileNamePart2[1]) > Parameter.Param1Max)
                            {
                                Parameter.Param1Max = int.Parse(fileNamePart2[1]);
                            }

                            if (int.Parse(fileNamePart2[2]) < Parameter.Param2Min)
                            {
                                Parameter.Param2Min = int.Parse(fileNamePart2[2]);
                            }

                            if (int.Parse(fileNamePart2[2]) > Parameter.Param2Max)
                            {
                                Parameter.Param2Max = int.Parse(fileNamePart2[2]);
                            }


                        }
                        Parameter.Min = Parameter.Param1Min + Parameter.Param2Min;
                        Parameter.Max = Parameter.Param1Max + Parameter.Param2Max;

                        XMLfileHandler.ReadFile(Parameter.Path + "\\" + Parameter.Projectname + "_param.xml", listBox1);

                        S1PReader.S1PFiles = new List<SParameter>[Parameter.Files.Length];

                    }
                }
            }
        }
        private void ButtonSelectFile_Click(object sender, EventArgs e)
        {
            Parameter.SetExpectedFile = false;
            SelectFile();

            getFilenames(Parameter.Param1Value, Parameter.Param2Value);
            loadFiles(Parameter.FileName1);

            Parameter.MinFrequencyHz = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)].Select(d => d.FrequencyHz).Min();
            Parameter.MaxFrequencyHz = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)].Select(d => d.FrequencyHz).Max();

            SetLabel();
            Parameter.SetExpectedFile = true;

            PlotExploreChart();
            loadFiles();
            MessageBox.Show($"In folder there is {Parameter.Files.Length} s1p-files");
        }
        public void getFilenames(int part1, int part2)
        {

            Parameter.FileName1 = "";
            Parameter.FileName2 = "";
            Parameter.FileName3 = "";
            Parameter.FileName4 = "";

            int diffMax = 10000;
            Parameter.FileName1diff = diffMax;
            Parameter.FileName2diff = diffMax;
            Parameter.FileName3diff = diffMax;
            Parameter.FileName4diff = diffMax;

            string[] fileName = { "", "", "", "", "" };

            int[] fileNameDiff = { diffMax, diffMax, diffMax, diffMax, diffMax };

            fileName[1] = Parameter.Path + "\\" + Parameter.Projectname + "_" + part1 + "_" + part2 + ".s1p";
            int fileArrayNo = Array.IndexOf(Parameter.Files, fileName[1]);

            double weightTot = 0;
            double[] weight = { 0, 0, 0, 0, 0 };

            int fileNo = 1;

            if (fileArrayNo >= 0)
            {
                fileNameDiff[1] = 0;
                Parameter.FileName1 = fileName[1];
                Parameter.FileName1diff = fileNameDiff[1];

            }
            else
            {
                foreach (string file in Parameter.Files)
                {
                    string[] fileNamePart2 = Path.GetFileNameWithoutExtension(file).Split('_');

                    if (fileNamePart2.Length == 3)
                    {
                        int checkPart1 = int.Parse(fileNamePart2[1]);
                        int checkPart2 = int.Parse(fileNamePart2[2]);
                        int diffPart = (int)Math.Abs(Math.Sqrt(Math.Pow(part1 - checkPart1, 2) + Math.Pow(part2 - checkPart2, 2)));

                        if (part1 >= checkPart1 && part2 >= checkPart2 && fileNameDiff[1] > diffPart)
                        {
                            fileNameDiff[1] = diffPart;
                            fileName[1] = file;
                        }

                        if (part1 >= checkPart1 && part2 <= checkPart2 && fileNameDiff[2] > diffPart)
                        {
                            fileNameDiff[2] = diffPart;
                            fileName[2] = file;
                        }

                        if (part1 <= checkPart1 && part2 >= checkPart2 && fileNameDiff[3] > diffPart)
                        {
                            fileNameDiff[3] = diffPart;
                            fileName[3] = file;
                        }

                        if (part1 <= checkPart1 && part2 <= checkPart2 && fileNameDiff[4] > diffPart)
                        {
                            fileNameDiff[4] = diffPart;
                            fileName[4] = file;
                        }
                    }
                }

                for (int i = 1; i < 5; i++)
                {
                    if (fileNameDiff[i] < diffMax)
                    {
                        bool okFile = false;
                        if (fileNo == 1)
                        {
                            Parameter.FileName1 = fileName[i];
                            Parameter.FileName1diff = fileNameDiff[i];
                            okFile = true;
                        }
                        else if (fileNo == 2 && Parameter.FileName1 != fileName[i])
                        {
                            Parameter.FileName2 = fileName[i];
                            Parameter.FileName2diff = fileNameDiff[i];
                            okFile = true;
                        }
                        else if (fileNo == 3 && Parameter.FileName1 != fileName[i] && Parameter.FileName2 != fileName[i])
                        {
                            Parameter.FileName3 = fileName[i];
                            Parameter.FileName3diff = fileNameDiff[i];
                            okFile = true;
                        }
                        else if (fileNo == 4 && Parameter.FileName1 != fileName[i] && Parameter.FileName2 != fileName[i] && Parameter.FileName3 != fileName[i])
                        {
                            Parameter.FileName4 = fileName[i];
                            Parameter.FileName4diff = fileNameDiff[i];
                            okFile = true;
                        }

                        if (okFile)
                        {
                            weight[i] = 1 / Math.Pow(fileNameDiff[i], 2);
                            weightTot += weight[i];
                            fileNo += 1;
                        }
                    }
                }

            }
            textBox3.Clear();

            if (fileNameDiff[1] == 0)
            {
                textBox3.AppendText(" 100.0%  " + Path.GetFileName(fileName[1]));
            }
            else
            {
                if (Parameter.FileName1diff < diffMax)
                {
                    textBox3.AppendText(Math.Round(100 * (1 / Math.Pow(Parameter.FileName1diff, 2)) / weightTot, 1).ToString("F1").PadLeft(6)
                                                                                + "%  " + Path.GetFileName(Parameter.FileName1) + "\r\n");
                }
                if (Parameter.FileName2diff < diffMax)
                {
                    textBox3.AppendText(Math.Round(100 * (1 / Math.Pow(Parameter.FileName2diff, 2)) / weightTot, 1).ToString("F1").PadLeft(6)
                                                                                + "%  " + Path.GetFileName(Parameter.FileName2) + "\r\n");
                }
                if (Parameter.FileName3diff < diffMax)
                {
                    textBox3.AppendText(Math.Round(100 * (1 / Math.Pow(Parameter.FileName3diff, 2)) / weightTot, 1).ToString("F1").PadLeft(6)
                                                                                + "%  " + Path.GetFileName(Parameter.FileName3) + "\r\n");
                }
                if (Parameter.FileName4diff < diffMax)
                {
                    textBox3.AppendText(Math.Round(100 * (1 / Math.Pow(Parameter.FileName4diff, 2)) / weightTot, 1).ToString("F1").PadLeft(6)
                                                                                + "%  " + Path.GetFileName(Parameter.FileName4));
                }
            }
        }
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
                Parameter.Min = int.Parse(textBox1.Text);
                Parameter.Max = int.Parse(textBox7.Text);
                Parameter.Diff = int.Parse(textBox8.Text);
                Parameter.refImp = int.Parse(textBox9.Text);
                Parameter.Param1Name = textBox5.Text;
                Parameter.Param2Name = textBox6.Text;

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
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {

                string[] paramx = listBox2.SelectedItem.ToString().Split("_");
                if (paramx.Length == 3)
                {
                    Parameter.SetExpectedFile = false;
                    Parameter.Param1Value = int.Parse(paramx[1]);
                    Parameter.Param2Value = int.Parse(paramx[2].Replace(".s1p", ""));
                    SetExpectedFile();
                    PlotExploreChart();
                    Parameter.SetExpectedFile = true;
                }
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
                int xMin = Parameter.Param2Min;
                int xMax = Parameter.Param2Max;

                if (P1 < Parameter.Min - Parameter.Diff)
                {
                    xMin = (Parameter.Min - Parameter.Diff) - P1;
                }

                if (xMax > (Parameter.Max - Parameter.Diff) - P1)
                {
                    xMax = (Parameter.Max - Parameter.Diff) - P1;
                }


                for (int P2 = xMin; P2 < xMax; P2 += 1)
                {

                    List<SParameter> sParams1 = new List<SParameter> { };
                    List<SParameter> sParams2 = new List<SParameter> { };
                    List<SParameter> sParams3 = new List<SParameter> { };
                    List<SParameter> sParams4 = new List<SParameter> { };

                    double weight1 = 0;
                    double weight2 = 0;
                    double weight3 = 0;
                    double weight4 = 0;

                    int fileStatus1 = 0;
                    int fileStatus2 = 0;
                    int fileStatus3 = 0;
                    int fileStatus4 = 0;

                    getFilenames(P1, P2);

                    if (Parameter.FileName1diff == 0)
                    {
                        sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
                        weight1 = 1;
                        fileStatus1 = 1;
                    }
                    else
                    {
                        if (Parameter.FileName1diff < 10000)
                        {
                            sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
                            weight1 = 1 / Math.Pow(Parameter.FileName1diff, 2);
                            fileStatus1 = 1;
                        }
                        if (Parameter.FileName2diff < 10000 && Parameter.FileName1 != Parameter.FileName2)
                        {
                            sParams2 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName2)];
                            weight2 = 1 / Math.Pow(Parameter.FileName2diff, 2);
                            fileStatus2 = 1;
                        }
                        if (Parameter.FileName3diff < 10000 && Parameter.FileName1 != Parameter.FileName3 && Parameter.FileName2 != Parameter.FileName3)
                        {
                            sParams3 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName3)];
                            weight3 = 1 / Math.Pow(Parameter.FileName3diff, 2);
                            fileStatus3 = 1;
                        }
                        if (Parameter.FileName4diff < 10000 && Parameter.FileName1 != Parameter.FileName4 && Parameter.FileName2 != Parameter.FileName4 && Parameter.FileName3 != Parameter.FileName4)
                        {
                            sParams4 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName4)];
                            weight4 = 1 / Math.Pow(Parameter.FileName4diff, 2);
                            fileStatus4 = 1;
                        }
                    }

                    double minVSWR = 100;
                    double avgVSWR = 0;
                    double maxVSWR = 0;

                    for (int i = 0; i < freqNo; i++)
                    {
                        double avgTemp = 0;

                        minVSWR = Math.Min(minVSWR, sParams1[freq[i]].VSWR);
                        maxVSWR = Math.Max(maxVSWR, sParams1[freq[i]].VSWR);
                        avgTemp = sParams1[freq[i]].VSWR * weight1;

                        if (fileStatus2 > 0)
                        {
                            minVSWR = Math.Min(minVSWR, sParams2[freq[i]].VSWR);
                            maxVSWR = Math.Max(maxVSWR, sParams2[freq[i]].VSWR);
                            avgTemp += sParams2[freq[i]].VSWR * weight2;
                        }

                        if (fileStatus3 > 0)
                        {
                            minVSWR = Math.Min(minVSWR, sParams3[freq[i]].VSWR);
                            maxVSWR = Math.Max(maxVSWR, sParams3[freq[i]].VSWR);
                            avgTemp += sParams3[freq[i]].VSWR * weight3;
                        }
                        if (fileStatus4 > 0)
                        {
                            minVSWR = Math.Min(minVSWR, sParams4[freq[i]].VSWR);
                            maxVSWR = Math.Max(maxVSWR, sParams4[freq[i]].VSWR);
                            avgTemp += sParams4[freq[i]].VSWR * weight4;
                        }

                        avgVSWR += avgTemp / (weight1 * fileStatus1 + weight2 * fileStatus2 + weight3 * fileStatus3 + weight4 * fileStatus4);


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
                int xMin = Parameter.Param2Min;
                int xMax = Parameter.Param2Max;

                if (P1 < Parameter.Min - Parameter.Diff)
                {
                    xMin = (Parameter.Min - Parameter.Diff) - P1;
                }

                if (P1 + xMax > Parameter.Max - Parameter.Diff)
                {
                    xMax = (Parameter.Max - Parameter.Diff) - P1;
                }


                for (int P2 = xMin; P2 < xMax; P2 += 1)
                {

                    List<SParameter> sParams1 = new List<SParameter> { };
                    List<SParameter> sParams2 = new List<SParameter> { };
                    List<SParameter> sParams3 = new List<SParameter> { };
                    List<SParameter> sParams4 = new List<SParameter> { };

                    double weight1 = 0;
                    double weight2 = 0;
                    double weight3 = 0;
                    double weight4 = 0;

                    int fileStatus1 = 0;
                    int fileStatus2 = 0;
                    int fileStatus3 = 0;
                    int fileStatus4 = 0;

                    getFilenames(P1, P2);

                    if (Parameter.FileName1diff == 0)
                    {
                        sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
                        weight1 = 1;
                        fileStatus1 = 1;
                    }
                    else
                    {
                        if (Parameter.FileName1diff < 10000)
                        {
                            sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
                            weight1 = 1 / Math.Pow(Parameter.FileName1diff, 2);
                            fileStatus1 = 1;
                        }
                        if (Parameter.FileName2diff < 10000 && Parameter.FileName1 != Parameter.FileName2)
                        {
                            sParams2 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName2)];
                            weight2 = 1 / Math.Pow(Parameter.FileName2diff, 2);
                            fileStatus2 = 1;
                        }
                        if (Parameter.FileName3diff < 10000 && Parameter.FileName1 != Parameter.FileName3 && Parameter.FileName2 != Parameter.FileName3)
                        {
                            sParams3 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName3)];
                            weight3 = 1 / Math.Pow(Parameter.FileName3diff, 2);
                            fileStatus3 = 1;
                        }
                        if (Parameter.FileName4diff < 10000 && Parameter.FileName1 != Parameter.FileName4 && Parameter.FileName2 != Parameter.FileName4 && Parameter.FileName3 != Parameter.FileName4)
                        {
                            sParams4 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName4)];
                            weight4 = 1 / Math.Pow(Parameter.FileName4diff, 2);
                            fileStatus4 = 1;
                        }
                    }

                    int countVSWR = 0;

                    for (int i = 0; i < Parameter.CenterFrequencyMapValues.Count; i++)
                    {
                        double avgTemp = 0;

                        avgTemp = sParams1[Parameter.CenterFrequencyMapValues[i].FileRowNo].VSWR * weight1;

                        if (fileStatus2 > 0)
                        {
                            avgTemp += sParams2[Parameter.CenterFrequencyMapValues[i].FileRowNo].VSWR * weight2;
                        }

                        if (fileStatus3 > 0)
                        {
                            avgTemp += sParams3[Parameter.CenterFrequencyMapValues[i].FileRowNo].VSWR * weight3;
                        }
                        if (fileStatus4 > 0)
                        {
                            avgTemp += sParams4[Parameter.CenterFrequencyMapValues[i].FileRowNo].VSWR * weight4;
                        }

                        avgTemp = avgTemp / (weight1 * fileStatus1 + weight2 * fileStatus2 + weight3 * fileStatus3 + weight4 * fileStatus4);

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
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                int newValue = (int)numericUpDown5.Value;
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
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (Parameter.SetExpectedFile)
            {
                int newValue = (int)numericUpDown6.Value;
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
                int xMin = Parameter.Param2Min;
                int xMax = Parameter.Param2Max;

                if (P1 < Parameter.Min - Parameter.Diff)
                {
                    xMin = (Parameter.Min - Parameter.Diff) - P1;
                }

                if (xMax > (Parameter.Max - Parameter.Diff) - P1)
                {
                    xMax = (Parameter.Max - Parameter.Diff) - P1;
                }


                for (int P2 = xMin + 1; P2 < xMax; P2 += 1)
                {
                    if (P1 == 420 && P2 == 20)
                    {
                        int a = 1;
                    }

                    List<SParameter> sParams1 = new List<SParameter> { };
                    List<SParameter> sParams2 = new List<SParameter> { };
                    List<SParameter> sParams3 = new List<SParameter> { };
                    List<SParameter> sParams4 = new List<SParameter> { };

                    double weight1 = 0;
                    double weight2 = 0;
                    double weight3 = 0;
                    double weight4 = 0;

                    int fileStatus1 = 0;
                    int fileStatus2 = 0;
                    int fileStatus3 = 0;
                    int fileStatus4 = 0;

                    getFilenames(P1, P2);

                    if (Parameter.FileName1diff == 0)
                    {
                        sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
                        weight1 = 1;
                        fileStatus1 = 1;
                    }
                    else
                    {
                        if (Parameter.FileName1diff < 10000)
                        {
                            sParams1 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName1)];
                            weight1 = 1 / Math.Pow(Parameter.FileName1diff, 2);
                            fileStatus1 = 1;
                        }
                        if (Parameter.FileName2diff < 10000 && Parameter.FileName1 != Parameter.FileName2)
                        {
                            sParams2 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName2)];
                            weight2 = 1 / Math.Pow(Parameter.FileName2diff, 2);
                            fileStatus2 = 1;
                        }
                        if (Parameter.FileName3diff < 10000 && Parameter.FileName1 != Parameter.FileName3 && Parameter.FileName2 != Parameter.FileName3)
                        {
                            sParams3 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName3)];
                            weight3 = 1 / Math.Pow(Parameter.FileName3diff, 2);
                            fileStatus3 = 1;
                        }
                        if (Parameter.FileName4diff < 10000 && Parameter.FileName1 != Parameter.FileName4 && Parameter.FileName2 != Parameter.FileName4 && Parameter.FileName3 != Parameter.FileName4)
                        {
                            sParams4 = S1PReader.S1PFiles[Array.IndexOf(Parameter.Files, Parameter.FileName4)];
                            weight4 = 1 / Math.Pow(Parameter.FileName4diff, 2);
                            fileStatus4 = 1;
                        }
                    }

                    bool avgBool = false;
                    int avgNo = 0;
                    int[] avgCount = new int[sParams1.Count + 1];

                    for (int i = 0; i < sParams1.Count; i++)
                    {
                        double avgTemp = 0;

                        avgTemp = sParams1[i].VSWR * weight1;

                        if (fileStatus2 > 0)
                        {
                            avgTemp += sParams2[i].VSWR * weight2;
                        }

                        if (fileStatus3 > 0)
                        {
                            avgTemp += sParams3[i].VSWR * weight3;
                        }
                        if (fileStatus4 > 0)
                        {
                            avgTemp += sParams4[i].VSWR * weight4;
                        }

                        avgTemp = avgTemp / (weight1 * fileStatus1 + weight2 * fileStatus2 + weight3 * fileStatus3 + weight4 * fileStatus4);

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

            if (comboBox1.SelectedItem == "Conductance_G") { numericUpDown9.Value = -0.1m; numericUpDown1.Value = 0.1m; }
            else if (comboBox1.SelectedItem == "GammaMagnitude") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "ImpedanceMagnitude") { numericUpDown9.Value = 0; numericUpDown1.Value = 100; }
            else if (comboBox1.SelectedItem == "LinMag") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "LogMag") { numericUpDown9.Value = -30; numericUpDown1.Value = 0; }
            else if (comboBox1.SelectedItem == "PhaseDegrees") { numericUpDown9.Value = -90; numericUpDown1.Value = 90; }
            else if (comboBox1.SelectedItem == "PhaseRadians") { numericUpDown9.Value = -2; numericUpDown1.Value = 2; }
            else if (comboBox1.SelectedItem == "Reactance_X") { numericUpDown9.Value = -20; numericUpDown1.Value = 20; }
            else if (comboBox1.SelectedItem == "ReflectedPower") { numericUpDown9.Value = 0; numericUpDown1.Value = 1; }
            else if (comboBox1.SelectedItem == "Resistance_R") { numericUpDown9.Value = 0; numericUpDown1.Value = 100; }
            else if (comboBox1.SelectedItem == "ReturnLoss_RL") { numericUpDown9.Value = 1; numericUpDown1.Value = 50; }
            else if (comboBox1.SelectedItem == "Susceptance_B") { numericUpDown9.Value = -0.1m; numericUpDown1.Value = 0.1m; }
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
        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }
    }
    public class HeatMap
    {
        public int Param1Value { get; set; } = 0;
        public int Param2Value { get; set; } = 0;
        public double Min { get; set; } = 0.0;
        public double Avg { get; set; } = 0.0;
        public double Max { get; set; } = 0.0;
        //public int Count { get; set; } = 0;
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
      //  public double Max { get; set; } = 0.0;
    }
    public class SParameter
    {
        public double FrequencyHz { get; set; } = 0.0;
        public double Real { get; set; } = 0.0;
        public double Imag { get; set; } = 0.0;
        public double VSWR { get; set; } = 0.0;
    }
    public class S1PReader
    {
        public static List<SParameter>ReadS1PFile(string filePath)
        {
            var sParameters = new List<SParameter>();
            if (File.Exists(filePath))
            {
           
                var lines = File.ReadAllLines(filePath);
                //var lines = await File.ReadAllLinesAsync(filePath);

                //bool dataSectionStarted = false;
                string freqUnit = "Hz";
                //string format = "RI";

                foreach (var rawLine in lines)
                {
                    string line = rawLine.Trim();

                    // Skip empty lines or comments
                    if (string.IsNullOrEmpty(line) || line.StartsWith("!"))
                        continue;

                    if (line.StartsWith("#"))
                    {
                        // Parse options line (e.g. "# MHz S RI R 50")
                        var tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i].Equals("MHZ", StringComparison.OrdinalIgnoreCase))
                                freqUnit = "MHz";
                            else if (tokens[i].Equals("GHZ", StringComparison.OrdinalIgnoreCase))
                                freqUnit = "GHz";
                            else if (tokens[i].Equals("KHZ", StringComparison.OrdinalIgnoreCase))
                                freqUnit = "kHz";
                            else if (tokens[i].Equals("HZ", StringComparison.OrdinalIgnoreCase))
                                freqUnit = "Hz";

//                            if (tokens[i].Equals("RI", StringComparison.OrdinalIgnoreCase))
//                                format = "RI";
                        }
                        continue;
                    }

                    // Read frequency and S-parameters
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 3)
                        continue;

                    //double freq = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    //double real = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    //double imag = double.Parse(parts[2], CultureInfo.InvariantCulture);

                    double freq = double.Parse(parts[0], new CultureInfo("sv-SE"));
                    double real = double.Parse(parts[1], new CultureInfo("sv-SE"));
                    double imag = double.Parse(parts[2], new CultureInfo("sv-SE"));


                    // Convert frequency to Hz
                    switch (freqUnit.ToUpper())
                    {
                        case "GHZ": freq *= 1e9; break;
                        case "MHZ": freq *= 1e6; break;
                        case "KHZ": freq *= 1e3; break;
                    }

                    //Complex gamma = new Complex(real, imag);    // S11
                    //Complex numerator = Complex.One + gamma;    // 1 + Γ
                    //Complex denominator = Complex.One - gamma;  // 1 - Γ
                    //Complex z = (numerator / denominator);      // the calculated load impedance




                    sParameters.Add(new SParameter
                    {
                        FrequencyHz = freq,
                        Real = real,
                        Imag = imag,
                        //Resistance = Converter.Resistance_R(real, imag),
                        //Reactance = Converter.Reactance_X(real,imag),
                        //Magnitude = Converter.GammaMagnitude(real,imag),
                        VSWR = S11Converter.VSWR(real,imag)
                    });
                }
                Parameter.Points = sParameters.Count();
            }
            else
            {
                for (int i = 1; i <= Parameter.Points; i++)
                {
                    sParameters.Add(new SParameter
                    {
                        FrequencyHz = 0,
                        Real = 0,
                        Imag = 0,
                        //Resistance = 0,
                        //Reactance = 0,
                        //Magnitude = 0,
                        VSWR = 0
                    });
                }
            }
                return sParameters;
        
        }
        public static List<SParameter>[] S1PFiles = {};
    }
    public static class Parameter
    {
        public static string Projectname = "";
        public static string Path = "";
        public static string[] Files = {""};

        public static int Points = 0;

        public static string Param1Name = "Param1Name";
        public static int Param1Min = 0;
        public static int Param1Max = 100;
        public static int Param1Value = 0;
        public static string Param2Name = "Param2Name";
        public static int Param2Min = 0;
        public static int Param2Max = 100;
        public static int Param2Value = 0;

        public static int Diff = 0;
        public static int Min = 0;
        public static int Max = 200;
        public static int refImp = 50;  // reference impedance reference impedance (typically 50 ohms)

        public static double MinFrequencyHz = 0;
        public static double MaxFrequencyHz = 0;

        public static string FileName1 = "";
        public static string FileName2 = "";
        public static string FileName3 = "";
        public static string FileName4 = "";

        public static int FileName1diff = 10000;
        public static int FileName2diff = 10000;
        public static int FileName3diff = 10000;
        public static int FileName4diff = 10000;

        public static bool SetExpectedFile = true;
        public static List<HeatMap> HeatMapValues = new List<HeatMap>();
        public static List<CenterFrequencyMap> CenterFrequencyMapValues = new List<CenterFrequencyMap>();
        public static List<BandwidthMap> BandwidthMapValues = new List<BandwidthMap>();
    }
    public class XMLfileHandler
    {
        public static void SaveFile(string filePath, IEnumerable<object> favorites = null, bool update = false)
        {
            if (update == false && File.Exists(filePath))
            {
                DialogResult result = MessageBox.Show(
                           "Settingsfile already exists!\nDo you wish to overwrite?",
                           "Warning!",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning
                       );

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
            if (favorites == null)
            {
                var doc = new XDocument(
                        new XElement("S1P",
                            new XElement("Settings",
                                new XElement("Projectname", Parameter.Projectname),
                                new XAttribute("Diff", Parameter.Diff),
                                new XAttribute("Min", Parameter.Min),
                                new XAttribute("Max", Parameter.Max),
                                new XAttribute("refImp", Parameter.refImp)
                                ),
                            new XElement("Param1",
                                new XElement("Param1Name", Parameter.Param1Name)
                                ),
                            new XElement("Param2",
                                new XElement("Param2Name", Parameter.Param2Name)
                                )
                            )
                        );
                doc.Save(filePath);
            }
            else
            {
                var doc = new XDocument(
                        new XElement("S1P",
                            new XElement("Settings",
                                new XElement("Projectname", Parameter.Projectname),
                                new XAttribute("Diff", Parameter.Diff),
                                new XAttribute("Min", Parameter.Min),
                                new XAttribute("Max", Parameter.Max),
                                new XAttribute("refImp", Parameter.refImp)
                                ),
                            new XElement("Param1",
                                new XElement("Param1Name", Parameter.Param1Name)
                                ),
                            new XElement("Param2",
                                new XElement("Param2Name", Parameter.Param2Name)
                                ),
                                 new XElement("Favorites",
                                        favorites.Select(favorite =>
                                            new XElement("Favorite", favorite.ToString()))
                                    )
                            )
                        );
                doc.Save(filePath);
            }
        }

        public static void ReadFile(string filePath, ListBox listBox)
        {
            if (File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);

                var XMLPart = doc.Descendants("S1P").Elements("Settings");
                Parameter.Projectname = XMLPart.Elements("Projectname").Count() > 0 ? XMLPart.Elements("Projectname").First().Value.ToLower() : Parameter.Projectname;
                Parameter.Diff = XMLPart.Attributes("Diff").Count() > 0 ? int.Parse(XMLPart.Attributes("Diff").First().Value) : Parameter.Diff;
                Parameter.Min = XMLPart.Attributes("Min").Count() > 0 ? int.Parse(XMLPart.Attributes("Min").First().Value) : Parameter.Min;
                Parameter.Max = XMLPart.Attributes("Max").Count() > 0 ? int.Parse(XMLPart.Attributes("Max").First().Value) : Parameter.Max;
                Parameter.refImp = XMLPart.Attributes("refImp").Count() > 0 ? int.Parse(XMLPart.Attributes("refImp").First().Value) : Parameter.refImp;

                XMLPart = doc.Descendants("S1P").Elements("Param1");
                Parameter.Param1Name = XMLPart.Elements("Param1Name").Count() > 0 ? XMLPart.Elements("Param1Name").First().Value : Parameter.Param1Name;


                XMLPart = doc.Descendants("S1P").Elements("Param2");
                Parameter.Param2Name = XMLPart.Elements("Param2Name").Count() > 0 ? XMLPart.Elements("Param2Name").First().Value : Parameter.Param2Name;


                XMLPart = doc.Descendants("S1P").Elements("Favorites");
                listBox.Items.Clear();

                foreach (var favorite in XMLPart.Elements("Favorite"))
                {
                    listBox.Items.Add(favorite.Value);
                }
            }
            else
            {
                SaveFile(filePath);
            }

        }

    }
    public class S11Converter 
    {
        public static Complex Impedance_Z_Complex(double real, double imag, double refImp)
        {
            Complex gamma = new Complex(real, imag);    // S11
            Complex numerator = Complex.One + gamma;    // 1 + Γ
            Complex denominator = Complex.One - gamma;  // 1 - Γ
            Complex z = refImp * (numerator / denominator);      // the calculated load impedance

            return z;
        }
        public static Complex Admittance_Y_Complex(double real, double imag, double refImp)
        {
            // Y = 1 / Z
            Complex z = S11Converter.Impedance_Z_Complex(real, imag, refImp);

            return Complex.Reciprocal(z); // Admittance  
        }
        public static Complex Gamma_Complex(double real, double imag)
        {
            // Reflection coefficient
            Complex gamma = new Complex(real, imag);    // S11
            return gamma;
        }
        public static double GammaMagnitude(double real, double imag)
        {
            // Reflection coefficient magnitude
            Complex gamma = S11Converter.Gamma_Complex(real, imag);    // S11
            return gamma.Magnitude;
        }
        public static double ImpedanceMagnitude(double real, double imag, double refImp = 50)
        {
            // Normalized impedance magnitude
            Complex z = S11Converter.Impedance_Z_Complex(real, imag, refImp);
            return z.Magnitude;
        }
        public static double VSWR(double real, double imag)
        {
            // Voltage Standing Wave Ratio
            double Magnitude = S11Converter.GammaMagnitude(real, imag);
            return (1 + Magnitude) / (1 - Magnitude);
        }
        public static double Resistance_R(double real, double imag, double refImp = 50)
        {
            // The resistive part in ohms
            Complex z = S11Converter.Impedance_Z_Complex(real, imag, refImp);

            return z.Real; // Resistance
        }
        public static double Reactance_X(double real, double imag, double refImp = 50)
        {
            // Represents inductive or capacitive behavior in ohms
            Complex z = S11Converter.Impedance_Z_Complex(real, imag, refImp);

            return z.Imaginary; // Reactance 
        }
        public static double Conductance_G(double real, double imag, double refImp = 50)
        {
            Complex y = S11Converter.Admittance_Y_Complex(real, imag, refImp);

            return y.Real; // Conductance 
        }
        public static double Susceptance_B(double real, double imag, double refImp = 50)
        {
            Complex y = S11Converter.Admittance_Y_Complex(real, imag, refImp);

            return y.Imaginary; // Susceptance 
        }
        public static double ReturnLoss_RL(double real, double imag)
        {
            //LogMag = Logarithmic Magnitude

            double m = S11Converter.GammaMagnitude(real, imag);

            return -20 * Math.Log10(m);
        }
        public static double LogMag(double real, double imag)
        {
            //LogMag = Logarithmic Magnitude

            double m = S11Converter.GammaMagnitude(real, imag);

            return 20 * Math.Log10(m);
        }
        public static double LinMag(double real, double imag)
        {
            //LogMag = Logarithmic Magnitude

            double logmag = S11Converter.LogMag(real, imag);

            return Math.Pow(10, logmag / 20);
        }
        public static double ReflectedPower(double real, double imag, double incidentPower = 1)
        {
            //LogMag = Logarithmic Magnitude

            double m = S11Converter.GammaMagnitude(real, imag);

            return Math.Pow(m, 2) * incidentPower;
        }
        public static double PhaseRadians(double real, double imag, double refImp = 50)
        {
            // phase angle in radians
            Complex z = S11Converter.Impedance_Z_Complex(real, imag, refImp);

            return z.Phase;
        }
        public static double PhaseDegrees(double real, double imag, double refImp = 50)
        {
            // phase angle in degrees
            double pr = S11Converter.PhaseRadians(real, imag, refImp);

            return pr * (180 / Math.PI); 
        }
    }
}
