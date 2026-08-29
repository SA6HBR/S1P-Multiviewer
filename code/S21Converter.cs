using System.Numerics;

namespace S1P_Multiviewer
{
    /// <summary>
    /// Conversions for S21 (or any transmission coefficient - S12 works the same
    /// way). S21 is NOT a reflection coefficient, so the one-port bilinear
    /// transform S11Converter uses to derive impedance/admittance
    /// (Z = Zref*(1+G)/(1-G)) does not apply here - there is no single-port
    /// impedance "behind" a transmission measurement. This class intentionally
    /// only has magnitude/phase-based conversions, which are the ones that are
    /// actually meaningful for a transmission parameter. Don't add
    /// Impedance/Admittance/Resistance/Reactance/Conductance/Susceptance here -
    /// use S11Converter on S11/S22 for those. VSWR is included below despite
    /// this, but explicitly marked as non-standard - see its own comment.
    /// </summary>
    public class S21Converter
    {
        public static Complex Gamma_Complex(double real, double imag)
        {
            // S21 itself, as a complex number
            return new Complex(real, imag);
        }

        public static double Magnitude(double real, double imag)
        {
            // |S21|
            return S21Converter.Gamma_Complex(real, imag).Magnitude;
        }

        public static double InsertionGain_dB(double real, double imag)
        {
            // 20*log10(|S21|) - positive means gain, negative means loss (the
            // standard way S21 is expressed in dB). Floored at -150 dB and
            // guarded against log(0) the same way S11Converter.LogMag is.
            double m = S21Converter.Magnitude(real, imag);

            if (m <= 1e-15) // ca -300 dB, safety margin before Log10
            {
                return -150.0;
            }

            double dB = 20 * Math.Log10(m);

            return Math.Max(dB, -150.0);
        }

        public static double InsertionLoss_dB(double real, double imag)
        {
            // Insertion Loss is conventionally reported as a positive number
            // when the network attenuates the signal - the mirror of Return Loss
            // on the S11 side, but for a transmission parameter instead of a
            // reflection one.
            return -S21Converter.InsertionGain_dB(real, imag);
        }

        public static double LinMag(double real, double imag)
        {
            // Magnitude recomputed via the floored dB value, so it stays
            // consistent with InsertionGain_dB/InsertionLoss_dB at the floor.
            double dB = S21Converter.InsertionGain_dB(real, imag);

            return Math.Pow(10, dB / 20);
        }

        public static double PhaseRadians(double real, double imag)
        {
            // Transmission (insertion) phase in radians
            return S21Converter.Gamma_Complex(real, imag).Phase;
        }

        public static double PhaseDegrees(double real, double imag)
        {
            // Transmission (insertion) phase in degrees
            double pr = S21Converter.PhaseRadians(real, imag);

            return pr * (180 / Math.PI);
        }

        public static double TransmittedPower(double real, double imag, double incidentPower = 1)
        {
            // |S21|^2 * incident power - the fraction of incident power that
            // makes it through to port 2 (power gain if the network is active
            // and this exceeds 1, though this app targets passive networks).
            double m = S21Converter.Magnitude(real, imag);

            return Math.Pow(m, 2) * incidentPower;
        }

        /// <summary>
        /// EXPERIMENTAL - VSWR is a reflection-coefficient concept (it comes
        /// from a standing wave formed by an incident and a reflected wave on
        /// the SAME line). S21 measures a wave arriving at a different port
        /// after passing through the network, not a reflection, so there is no
        /// physical standing wave for this formula to describe. This applies
        /// the same (1+|S|)/(1-|S|) formula to |S21| purely as a curiosity/
        /// experiment - the result is a number, not a real VSWR. Same 99.0
        /// clamp as S11Converter.VSWR for |S21| >= 1, since a blended S21 could
        /// in principle reach or exceed 1 even for a passive network.
        /// </summary>
        public static double VSWR(double real, double imag)
        {
            double m = S21Converter.Magnitude(real, imag);
            if (m >= 1.0) return 99.0;

            return (1 + m) / (1 - m);
        }
    }
}
