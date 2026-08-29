using System.Numerics;

namespace S1P_Multiviewer
{
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
            if (Magnitude >= 1.0) return 99.0; // Om 1 så error...

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
            // Return Loss är det positiva värdet av LogMag (omvänt tecken)
            // Genom att anropa LogMag får vi automatiskt med "golvet" på -150 dB
            return -S11Converter.LogMag(real, imag);
        }
        public static double LogMag(double real, double imag)
        {
            //LogMag = Logarithmic Magnitude
            double m = S11Converter.GammaMagnitude(real, imag);

            // Om magnituden är 0 eller extremt liten, returnera golvet direkt
            if (m <= 1e-15) // Motsvarar ca -300 dB, säkerhetsmarginal för Log10
            {
                return -150.0;
            }

            double logMag = 20 * Math.Log10(m);

            // Begränsa även resultatet så det aldrig blir mindre än -150
            return Math.Max(logMag, -150.0);
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
            //Complex z = S11Converter.Impedance_Z_Complex(real, imag, refImp); // fasen för impedansen
            Complex z = S11Converter.Gamma_Complex(real, imag); // fasen för S11 

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
