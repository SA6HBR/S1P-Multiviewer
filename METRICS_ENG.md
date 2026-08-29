# Measurement Values in the Explore View (`comboBox1`)

Explanation of the 29 choices in the dropdown menu in the Explore tab, what they show, and when they are relevant to look at depending on whether you are measuring an **antenna** (usually one-port, only S11 is meaningful) or a **filter** (two-port, both S11 and S21 are meaningful).

## Quick Guide

**Measuring an antenna?** Start with `VSWR S11` or `LogMag S11` (same information, different units). If you want to design a matching network, also look at `Resistance_R S11` + `Reactance_X S11` (series form) or `Conductance_G S11` + `Susceptance_B S11` (parallel form).

**Measuring a filter?** Start with `LogMag S11 + S21` - shows insertion loss (S21, red) and return loss (S11, blue) in the same diagram, which is the classic filter diagram. Use `PhaseDegrees S21` if you need to calculate group delay.

## S11 - Reflection (Antenna *and* Filter Input)

S11 describes how much of the signal bounces back from the port you are feeding into. For an antenna, this is in practice **the only thing you measure** (an antenna has only one port). For a filter, S11 describes how well the filter's input is matched to 50 Ω - a filter can have perfect insertion loss but still be poorly matched (reflecting signal back toward the transmitter).

| Choice | What it shows | Antenna | Filter |
|---|---|---|---|
| `VSWR S11` | Voltage Standing Wave Ratio, 1.0 = perfect, rises toward infinity at total reflection. The standard measure in amateur radio. | **Main metric.** Goal often <2:1, ideally <1.5:1 in the band you wish to use. | Useful for input matching, but `LogMag`/`ReturnLoss_RL` is more common in filter datasheets. |
| `LogMag S11` | `20·log10(|S11|)` in dB. Negative number - the more negative, the better the match (less reflected). What you see directly on a NanoVNA/VNA S11 graph. | **Main metric**, alternative to VSWR. -10 dB ≈ VSWR 1.9, -20 dB ≈ VSWR 1.2. | **Main metric** for input matching in the passband. Should be below approx. -15 dB in the passband for a good filter. |
| `ReturnLoss_RL S11` | Same formula as `LogMag S11` but with the opposite sign (positive number). More common in professional datasheet contexts. | Same information as above, but as a positive number. | Same as above - common in filter datasheets ("Return Loss > 15 dB"). |
| `Magnitude S11` | `|Γ|`, linear reflection coefficient 0-1. 0 = perfect match, 1 = total reflection. | Rarely the main view, but good raw data if you are calculating further by hand. | Same - mostly a calculation basis. |
| `LinMag S11` | Same as `Magnitude S11` (linear `|Γ|`, 0-1), calculated via the floor-protected dB path - in practice the same number. | Rarely the main view. | Calculation basis. |
| `ReflectedPower S11` | `|Γ|²` - the portion of input power that bounces back (0-1, or specify `incidentPower` for actual power). | Good for understanding actual power loss at poor matching, e.g., how many watts are reflected at high output power. | Same - relevant for high-power filters where reflected power can be a thermal/safety issue. |
| `ImpedanceMagnitude S11` | `|Z|`, the magnitude of the impedance in ohms. | Quick check: is it close to 50 Ω? | Less common, but can reveal if the filter's input impedance is reasonable. |
| `Resistance_R S11` | Resistive part of the impedance (series form), ohms. | **Important for matching.** At resonance, you often want R close to 50 Ω (or the impedance of the feed line). | Diagnostic of input impedance if the filter does not behave as expected. |
| `Reactance_X S11` | Reactive part (series form), ohms. Positive = inductive, negative = capacitive. | **Important for matching.** At resonance, X should be near 0. Determines how much inductance/capacitance is needed to trim the antenna to resonance. | Same diagnostics as R above. |
| `Conductance_G S11` | Conductance (parallel form of admittance), siemens. | Useful if designing a parallel/stub match instead of a series match. | Rarely the first choice, but same utility as for antennas. |
| `Susceptance_B S11` | Susceptance (parallel form), siemens. | Same as above - parallel matching. | Rarely the first choice. |

## S21 - Transmission (Only relevant for two-port measurements)

S21 describes how much signal reaches port 2 from port 1. **A standard one-port antenna has no S21** - this block applies to filters, amplifiers, attenuators, cables, or if you are deliberately measuring the coupling/isolation between two antennas (then "port 1" is one antenna and "port 2" is the other).

| Choice | What it shows | Antenna | Filter |
|---|---|---|---|
| `LogMag S21` | `20·log10(|S21|)` in dB. Negative = loss (normal for a passive filter/cable), positive = gain. | Only relevant if measuring isolation/coupling between two antennas - then you often want S21 as negative (low coupling) as possible. | **Main metric.** Shows insertion loss in the passband (near 0 dB = good) and rejection in the stopband (very negative = good). |
| `InsertionLoss S21` | Same formula as `LogMag S21` but with the opposite sign (positive number, "how much loss"). Standard term in filter datasheets. | Same info, rarely relevant for pure antenna measurement. | **Main metric**, common datasheet term ("Insertion Loss < 1 dB in the passband"). |
| `Magnitude S21` | `|S21|`, linear transmission coefficient 0-1 (or >1 for active amplification). | Rarely the main view. | Calculation basis. |
| `LinMag S21` | Same as `Magnitude S21`, calculated via the floor-protected dB path - in practice the same number. | Rarely the main view. | Calculation basis. |
| `TransmittedPower S21` | `|S21|²` - the portion of input power that makes it through the network. | Coupling/isolation measurement between antennas: what portion of transmitted power "leaks" to the other antenna. | Power budget - how much power actually goes through the filter, relevant for high-power applications. |
| `PhaseDegrees S21` / `PhaseRadians S21` | Transmission phase. | Rarely relevant for a simple antenna measurement. | Used to calculate **group delay** (the slope of the phase against frequency) - important for filters handling broadband signals or data without time-domain distortion. |
| `VSWR S21` | **Experimental, not physically meaningful.** The same VSWR formula as for S11 applied to `|S21|`, for curiosity. VSWR is a reflection concept (standing wave between incident and reflected wave on the *same* line) - S21 is not a reflection, so the number corresponds to no real physical phenomenon. | Do not use as measurement data. | Do not use as measurement data - `LogMag S21`/`InsertionLoss S21` are the correct metrics. |

## Phase Measurements on S11 (`PhaseDegrees S11` / `PhaseRadians S11`)

The phase angle of the reflection coefficient. Less used as a primary metric for antennas (VSWR/LogMag/R/X usually provide what is needed), but relevant if you:
- Are calculating group delay at an antenna system's input.
- Are designing a matching network by hand and need the phase information from the Smith chart (the same information shown there, but as numbers).

## Combined Views (`... S11 + S21`)

Eight choices plot **both** curves in the same diagram - S11 in blue, S21 in red, with a legend: `Magnitude S11 + S21`, `LinMag S11 + S21`, `LogMag S11 + S21`, `PhaseDegrees S11 + S21`, `PhaseRadians S11 + S21`, `ReflectedPower S11 + TransmittedPower S21`, `ReturnLoss_RL S11 + InsertionLoss S21`, and `VSWR S11 + S21`. The content uses the same formulas as in the respective S11/S21 rows above, just plotted together.

Most useful for filters, where you want to see insertion loss and return loss simultaneously (`LogMag S11 + S21` is the classic filter characterization graph). For a pure antenna measurement, the combined view is rarely relevant since S21 is normally completely missing.

**`VSWR S11 + S21` contains the experimental S21 variant** (see the warning in the S21 table above) - the S11 curve (blue) is a valid VSWR, but the S21 curve (red) is not. Use `LogMag S11 + S21` or `ReturnLoss_RL S11 + InsertionLoss S21` instead if you want a physically meaningful combined view.

## Practical Summary

- **Quick antenna check:** `VSWR S11` or `LogMag S11`.
- **Antenna matching network design:** `Resistance_R S11` + `Reactance_X S11` (series), or `Conductance_G S11` + `Susceptance_B S11` (parallel/stub).
- **Filter characterization:** `LogMag S11 + S21` as the primary choice, then `PhaseDegrees S21` if group delay is needed.
- **Power budget/high-power:** `ReflectedPower S11` and `TransmittedPower S21`.
- **Avoid as measurement data:** `VSWR S21` (experimental, see warning above).