using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecAnalysis.Utilities
{
    public enum FilterTypeEnum
    {
        eLowpass = 0,
        eHighpass,
        eBandpass,
        eBandstop,
        eLowShelve,
        eHighShelve,
        eAllpass,
        eComb,
        ePeaking,
        eNoFilter
    };
    public class BiquadFilterCoefficients
    {
        protected FilterTypeEnum filterType;
        protected double cutoff, resonance, gain;
        protected int filterOrder;
        public double b0, b1, b2;
        public double a1, a2;
        public double x1, x2, y1, y2;


        /// <summary>
        /// <remarks>
        // cutoff is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        /// 
        public BiquadFilterCoefficients(FilterTypeEnum eType, double filterCutoff, double filterGain)
        {
            this.filterOrder = 1;
            this.filterType = eType;
            this.gain = filterGain;
            this.cutoff = filterCutoff;

            double wc = Math.Tan(0.5 * Math.PI * this.cutoff);
            double oneOverWc = 1.0 / (wc + 1.0);

            this.b2 = 0.0f;
            this.a1 = (1.0 - wc) * oneOverWc;
            this.a2 = 0.0f;

            switch (eType)
            {
                case FilterTypeEnum.eLowpass:
                    this.b0 = this.gain * wc * oneOverWc;
                    this.b1 = this.b0;
                    break;

                case FilterTypeEnum.eHighpass:
                    this.b0 = this.gain * oneOverWc;
                    this.b1 = -this.b0;
                    break;
            }

        }
        public BiquadFilterCoefficients(FilterTypeEnum eType, double filterCutoff, double q, double filterGain)
        {
            this.filterType = eType;
            this.resonance = q;
            this.gain = filterGain;
            this.cutoff = filterCutoff;

            // do bilinear transform
            double wc = Math.Tan(0.5 * Math.PI * filterCutoff);
            double wc2 = wc * wc;
            double qwc2q = q * (1.0 + wc2);
            double qwc2 = q * wc2;
            double y0 = 1.0 / (wc + qwc2q);

            this.a1 = y0 * (2.0f * q - 2.0f * qwc2);
            this.a2 = y0 * (wc - qwc2q);

            switch (eType)
            {
                case FilterTypeEnum.ePeaking:
                    gain -= 1.0;
                    this.b0 = 1.0 + (gain * y0 * wc);
                    this.b1 = a1;
                    this.b2 = a2 - (gain * y0 * wc);
                    break;

                case FilterTypeEnum.eHighShelve:
                    gain -= 1.0;
                    this.b0 = 1.0 + gain * y0 * q;
                    this.b1 = a1 - 2.0 * (gain * y0 * q);
                    this.b2 = a2 + (gain * y0 * q);
                    break;

                case FilterTypeEnum.eLowShelve:
                    gain -= 1.0;
                    this.b0 = 1.0 + (gain * y0 * qwc2);
                    this.b1 = a1 + 2.0 * (gain * y0 * qwc2);
                    this.b2 = a2 + (gain * y0 * qwc2);
                    break;

                case FilterTypeEnum.eLowpass:
                    this.b0 = gain * y0 * qwc2;
                    this.b1 = 2.0 * this.b0;
                    this.b2 = this.b0;
                    break;

                case FilterTypeEnum.eHighpass:
                    this.b0 = gain * y0 * q;
                    this.b1 = -2.0 * this.b0;
                    this.b2 = this.b0;
                    break;

                case FilterTypeEnum.eBandpass:
                    this.b0 = gain * y0 * wc;
                    this.b1 = 0.0;
                    this.b2 = -this.b0;
                    break;

                case FilterTypeEnum.eBandstop:
                    this.b0 = y0 * gain * (q + qwc2);
                    this.b1 = -gain * a1;
                    this.b2 = this.b0;
                    break;
            }
        }
        public float[] Filter(float[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                float sample = Convert.ToSingle(b0 * input[i] + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2);

                x2 = x1;
                x1 = input[i];

                y2 = y1;
                y1 = sample;

                input[i] = (float)y1;
            }     
            return input;
        }
        /// <summary>
        /// <remarks>
        // Calculates complex filter response at frequency. frequency is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        public ComplexNumber ComplexResponseAt(double frequency)
        {
            double f = frequency * Math.PI;
            double f2 = f * 2.0;
            double cosf = Math.Cos(f);
            double cos2f = Math.Cos(f2);
            double sinf = -Math.Sin(f);
            double sin2f = -Math.Sin(f2);

            ComplexNumber upperSide = new ComplexNumber(b0 + b1 * cosf + b2 * cos2f, b1 * sinf + b2 * sin2f);
            ComplexNumber lowerSide = new ComplexNumber(1.0 - a1 * cosf - a2 * cos2f, -a1 * sinf - a2 * sin2f);

            return upperSide / lowerSide;
        }

        /// <summary>
        /// <remarks>
        // Calculates filter magnitude response at frequency. frequency is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        public double MagnitudeResponseAt(double frequency)
        {
            double f = frequency * Math.PI;
            double f2 = f * 2.0;
            double cosf = Math.Cos(f);
            double cos2f = Math.Cos(f2);
            double sinf = -Math.Sin(f);
            double sin2f = -Math.Sin(f2);

            ComplexNumber upperSide = new ComplexNumber(b0 + b1 * cosf + b2 * cos2f, b1 * sinf + b2 * sin2f);
            ComplexNumber lowerSide = new ComplexNumber(1.0 - a1 * cosf - a2 * cos2f, -a1 * sinf - a2 * sin2f);
            ComplexNumber response = upperSide / lowerSide;

            return response.Norm;
        }

        /// <summary>
        /// <remarks>
        // Calculates 20Log10(filter magnitude response) at frequency. frequency is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        public double LogMagnitudeResponseAt(double frequency)
        {
            double response = this.MagnitudeResponseAt(frequency);
            if (response < 0.000001)
                response = 0.000001;
            return 20.0 * Math.Log10(response);
        }

    }
    public class FirstOrderFilterCoefficients
    {
        public FilterTypeEnum filterType;
        public double cutoff, gain;
        public double b0, b1;
        public double a1;
        public double[] m_frequencyTable;
        public double[] m_frequencyResponse;

        public FirstOrderFilterCoefficients(FilterTypeEnum eType, double filterCutoff, double filterGain)
        {
            this.filterType = eType;
            this.gain = filterGain;
            this.cutoff = filterCutoff;

            // do bilinear transform
            double wc = Math.Tan(0.5 * Math.PI * filterCutoff);
            double oneOverWc = 1.0 / (wc + 1.0);

            this.a1 = (1.0 - wc) * oneOverWc;

            switch (this.filterType)
            {
                case FilterTypeEnum.eLowpass:
                    this.b0 = this.gain * wc * oneOverWc;
                    this.b1 = this.b0;
                    break;

                case FilterTypeEnum.eHighpass:
                    this.b0 = this.gain * oneOverWc;
                    this.b1 = -this.b0;
                    break;

                case FilterTypeEnum.eAllpass:
                    this.b0 = -a1;
                    this.b1 = 1.0f;
                    break;
            }
        }

        /// <summary>
        /// <remarks>
        // Calculates complex filter response at frequency. frequency is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        public ComplexNumber ComplexResponseAt(double frequency)
        {
            double f = frequency * Math.PI;
            double f2 = f * 2.0;
            double cosf = Math.Cos(f);
            double sinf = -Math.Sin(f);

            ComplexNumber upperSide = new ComplexNumber(b0 + b1 * cosf, b1 * sinf);
            ComplexNumber lowerSide = new ComplexNumber(1.0 - a1 * cosf, -a1 * sinf);

            return upperSide / lowerSide;
        }

        /// <summary>
        /// <remarks>
        // Calculates filter magnitude response at frequency. frequency is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        public double MagnitudeResponseAt(double frequency)
        {
            double f = frequency * Math.PI;
            double f2 = f * 2.0;
            double cosf = Math.Cos(f);
            double sinf = -Math.Sin(f);

            ComplexNumber upperSide = new ComplexNumber(b0 + b1 * cosf, b1 * sinf);
            ComplexNumber lowerSide = new ComplexNumber(1.0 - a1 * cosf, -a1 * sinf);
            ComplexNumber response = upperSide / lowerSide;

            return response.Norm;
        }

        /// <summary>
        /// <remarks>
        // Calculates 20Log10(filter magnitude response) at frequency. frequency is between 0-1, 1 corresponds to nyquist sampling rate/2
        /// </remarks>
        /// </summary>
        public double LogMagnitudeResponseAt(double frequency)
        {
            double response = this.MagnitudeResponseAt(frequency);
            if (response < 0.000001)
                response = 0.000001;
            return 20.0 * Math.Log10(response);
        }
        
    }

}
