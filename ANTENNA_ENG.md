# Guide: Antenna Analysis with S11 (Reflection)

This guide describes how to interpret measurement values when analyzing and tuning an antenna in a 50-ohm system. For antenna measurements, **Port 1 (S11)** is primarily used.

## 1. Main Indicators of Antenna Performance

| Parameter | Excellent | Acceptable | Adjustment Required |
| :--- | :--- | :--- | :--- |
| **VSWR** | 1.0 to 1.3 | 1.3 to 2.0 | > 2.0 (Risk of radio damage) |
| **LogMag (S11)** | < -18 dB | -10 to -18 dB | > -10 dB |
| **Reflected Power**| < 1.5% | 1.5% to 10% | > 10% bouncing back |
| **Resistance (R)** | 45 - 55 $\Omega$ | 30 - 70 $\Omega$ | < 20 or > 100 $\Omega$ |
| **Reactance (X)** | ~ 0 $\Omega$ | < +/- 10 $\Omega$ | High reactance = not in resonance |

---

## 2. Parameter Guide for Antenna Tuning

### Resonance and Matching
*   **LogMag (S11):** Shows where the antenna is "in resonance" (where the graph dips the lowest). The goal is a deep dip at the desired operating frequency.
*   **VSWR:** The most common metric. A VSWR of 1.5:1 means approximately 4% of the power is reflected. Most modern transmitters reduce power (foldback) if VSWR exceeds 2.0.
*   **ReturnLoss_RL:** The higher the value, the better. 20 dB means the antenna is very well matched.

### Impedance Analysis (Why doesn't it match?)
*   **Resistance_R:** If the resistance is 50 $\Omega$ at the dip, the antenna is perfectly matched. 
    *   *Too low R (< 50 $\Omega$):* The antenna might be too close to ground/metal or be too short.
    *   *Too high R (> 50 $\Omega$):* May indicate a poor ground plane or that the antenna is mounted too high.
*   **Reactance_X:** Tells you if the antenna is electrically too long or too short.
    *   **Positive value (+j):** The antenna is inductive (too long).
    *   **Negative value (-j):** The antenna is capacitive (too short).
    *   **Goal:** X = 0 at the operating frequency.

### Other Parameters
*   **PhaseDegrees:** At resonance, the phase often passes through 0 degrees. A rapid phase change indicates an antenna with a high Q-factor (narrowband).
*   **ImpedanceMagnitude ($|Z|$):** The total impedance (vector sum of R and X). Should be 50 $\Omega$.
*   **ReflectedPower:** A very pedagogical value to explain loss. Shows in Watts or Percent how much power is "wasted" as heat in the radio instead of being transmitted.

---

## 3. Practical Tuning Guide

### How do I move the resonance frequency?
1.  **Antenna resonates too low in frequency:** The antenna is physically **too long**. Clip or shorten the element.
2.  **Antenna resonates too high in frequency:** The antenna is physically **too short**. Lengthen the element or add a loading coil.

### How do I interpret the curve?
*   **Wide and shallow dip:** The antenna is forgiving and works over a wide range, but may not be highly efficient. Common for "rubber duck" antennas on handheld radios.
*   **Narrow and deep dip:** The antenna is very efficient at a specific frequency (high Q-value), but will not tolerate moving far in frequency before VSWR spikes.

### Environmental Impact
Antennas are extremely affected by their surroundings. A measurement indoors near a computer will yield completely different values than a measurement outdoors on a mast.
*   **Handheld effect:** If you are holding a handheld radio, your body affects the S11 value (you act as a counterpoise/ground plane).
*   **Near-field interference:** Metal objects within one wavelength's distance lower the resistance and shift the resonance.