# Guide: Filter Analysis with S-parameters (S11 & S21)

This guide describes how to interpret various measurement values when analyzing a filter (e.g., a low-pass filter) in a 50-ohm system.

## 1. Overall Filter Performance

| Region | S21 (Transmission) | S11 (Reflection) | Status |
| :--- | :--- | :--- | :--- |
| **Passband** | -0.1 to -1.0 dB | < -20 dB | **Perfect:** Signal passes, minimal reflection. |
| **Passband (Marginal)**| -1.0 to -3.0 dB | -10 to -15 dB | **Acceptable:** Some attenuation and reflection. |
| **Cutoff (f_c)** | -3.0 dB | ~ -7 dB | **Limit:** Here the filter begins to block the signal. |
| **Stopband** | < -40 dB | > -0.5 dB | **Effective:** Signal is blocked and reflected back. |

---

## 2. Parameter Guide (S11 & S21)

Below is an explanation of all available graphs and their expected values for a functioning filter.

### Magnitude & Logarithmic Scale
*   **LogMag (S21):** Shows the filter's attenuation. 0 dB is no loss. -40 dB means only 1/10000 of the power passes through.
*   **LogMag (S11):** Shows how well the filter "matches" 50 Ohms. The lower (more negative) the value, the better.
*   **ReturnLoss_RL:** The positive equivalent of LogMag (S11). 20 dB return loss is very good; 0 dB means total reflection.
*   **LinMag (S21):** The voltage ratio (0 to 1.0). At cutoff ($f_c$), this value is exactly $0.707$.
*   **GammaMagnitude:** The magnitude of the reflection coefficient (0 to 1.0). Should be near 0 in the passband and near 1 in the stopband.

### Impedance & Admittance (S11-based)
*   **ImpedanceMagnitude ($|Z|$):** Should be as close to **50 $\Omega$** as possible in the passband.
*   **Resistance_R:** The real part of the impedance. Should be **50 $\Omega$** in the passband. Often drops towards 0 in the stopband for LC filters.
*   **Reactance_X:** The imaginary part (inductance/capacitance). Should be **0 $\Omega$** in the passband for a pure resistive match.
*   **Conductance_G:** The reciprocal of resistance. For 50 $\Omega$, the goal is **20 mS** (milliSiemens).
*   **Susceptance_B:** The imaginary part of admittance. Should be **0 mS** in the passband.

### Power & Standing Wave
*   **VSWR:** (Voltage Standing Wave Ratio). 1.0 is perfect. Values under 1.5 are considered a good match in a radio system.
*   **ReflectedPower:** Indicates how much of the power bounces back. 
    *   -20 dB S11 = 0.01 (1% reflection).
    *   -10 dB S11 = 0.10 (10% reflection).

### Phase
*   **PhaseDegrees / PhaseRadians:** Shows the phase shift. In the passband, one often sees a linear downward trend (phase delay). Sudden jumps in the stopband are usually due to noise when the signal becomes too weak to measure accurately.

---

## 3. Practical Tips for Analysis

1.  **Identify the Noise:** If `LogMag (S21)` flattens out at, for example, -70 dB and looks "jagged," you have reached the dynamic range (noise floor) of the network analyzer.
2.  **Check for Ripple:** If `LogMag (S21)` waves up and down in the passband, it suggests poor design or incorrect component values causing internal reflections.
3.  **Symmetry:** For most passive filters, S11 and S22 are nearly identical. If your measurements show large differences, the filter may be damaged or incorrectly assembled.
4.  **Energy Balance:** If both S11 and S21 are low simultaneously (e.g., S11 = -20 dB and S21 = -20 dB), the power is being dissipated in the filter as heat. This indicates components with a low Q-value or high resistance in the coils.