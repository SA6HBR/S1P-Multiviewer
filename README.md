# S1P Multiviewer

### Info:  
When I measured my new gamma-matched antenna from Sirio and adjusted L2 and L3, I realized how difficult it was to find the best settings for the antenna. I could use the settings recommended by the manufacturer, but are those really the best for the frequencies I want to use the antenna for?
With this program, you can virtually adjust L2 and L3 to view the Smith chart and VSWR based on the measurements you've performed, as well as the averages between them.
To test the program, you can use my measurement data for the SD68.
![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/Sirio%20SDXX.png)  

---

### START:  

![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/NanoVNA-app.png)  
Create a matrix of the min and max values of L2 and L3.  
Mark every 20mm of L2 and every 10mm of L3.  
Set the antenna for these values and measure using, for example, the NanoVNA app.  
Save s1p files for each measurement with the filename format:  
project_L2 setting_L3 setting.s1p  

If you are measuring an antenna from Sirio named SD68, you can create files according to this template:  
sd68_384_34.s1p  
  
---

### LOAD:

![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/Load.png)  
Select one of the .s1p files from the folder where you saved all the measurements.
All files in the folder should belong to the same project name.
Depending on the number of files and your computer’s performance, this may take a moment.

First time, enter the minimum and maximum values for both Parameter 1 and Parameter 2, as well as size of "tuning element" in Diff.
I use L2 and L3 as name for Parameter 1 and Parameter 2.
Click Save settings.

---

### EXPLORE:

![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/Explore.png)  
Click directly on the filenames or adjust the sliders for Parameter 1 and Parameter 2 to view graphs generated from the values in the files or the averages between them.
You can save the value as a favorite to easily return to an interesting setting.  
Adjust chart min-max and frequencies min-max, to optimize the chart display.  
Chart Type: Conductance, GammaMagnitude, ImpedanceMagnitude, LinMag, LogMag, PhaseDegrees, PhaseRadians, Reactance, ReflectedPower, Resistance, ReturnLoss, Susceptance and VSWR. 

---

### HEATMAP:

![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/Heatmap.png)  
Here you can see darker dots representing lower VSWR for the average/maximum of the selected frequencies.
Enter the frequencies of interest (in MHz), separated by commas (,) and finish by clicking Load map.
Adjust the Max VSWR setting to improve the graph visualization.

---

### CENTER FREQUENCY:

![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/CenterFrequency.png)  
This graph displays the frequencies that most commonly exhibit the lowest VSWR across all values of Parameter 1 and Parameter 2.
Click Load map.
Adjust Max VSWR to optimize the graph display.

---

### BANDWIDTH:  

![](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/image/Bandwith.png)  
Select the maximum acceptable VSWR to determine the widest usable bandwidth and finish by clicking Load map.
Adjust the percentage value to show only the results exceeding that threshold.

---

## Useful Links

* [Download latest S1P-Multiviewer](https://github.com/SA6HBR/S1P-Multiviewer/releases/download/1.0.0/S1P-Multiviewer.zip)
* [My measurements for SD68](https://github.com/SA6HBR/Sirio-antenna-SD68-dipole)
* [NanoVNA-App from DiSlord](https://github.com/DiSlord/NanoVNA-App/blob/main/Win32/Release/NanoVNA-App.exe)

---


## License

GNU General Public License v3.0, see [LICENSE](https://github.com/SA6HBR/S1P-Multiviewer/blob/main/LICENSE) for details.

### Third-party libraries

This project includes third-party libraries:

- [ScottPlot](https://scottplot.net/) - MIT License  
  See [third_party_licenses/ScottPlot.LICENSE.txt](./third_party_licenses/ScottPlot.LICENSE.txt)







