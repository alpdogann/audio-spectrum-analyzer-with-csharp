using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpecAnalysis
{
    public struct ComplexNumber
    {
        public float x;
        public float y;

        public ComplexNumber(double magnitude, double angle, bool logMagnitude)
        {
            if (logMagnitude)
            {
                magnitude = (float)Math.Pow(10.0, magnitude * 0.05);
            }

            if (magnitude < 0.0000001f)
            {
                magnitude = 0.0f;
                x = 0.0f;
                y = 0.0f;
            }
            else
            {
                double angleScale = Math.PI / 180.0;
                x = (float)(magnitude * Math.Cos(angleScale * angle));
                y = (float)(magnitude * Math.Sin(angleScale * angle));
            }
        }

        public float imag
        {
            get { return y; }
            set { y = value; }
        }

        public float real
        {
            get { return x; }
            set { x = value; }
        }
        public ComplexNumber(float r, float im)
        {
            x = r;
            y = im;
        }
        public ComplexNumber(double r, double im)
        {
            this.x = (float)r;
            this.y = (float)im;
        }
        public double Magnitude
        {
            get
            {
                double sqr = Math.Sqrt(x * x + y * y);

                if (sqr < 0.000001)
                    sqr = 0.000001;

                return sqr;
            }

            set
            {
                float ratio = (float)(value / Math.Sqrt(x * x + y * y));

                x *= ratio;
                y *= ratio;
            }
        }
        public double Norm
        {
            get { return Math.Sqrt(x * x + y * y); }
        }
        public double GetLogMagnitude(double dMinNorm, double dMaxNorm)
        {
            double norm = this.Norm;

            if (norm < 0.000000001)
                norm = 0.000000001;

            norm = 20.0 * Math.Log10(norm);

            if (norm > dMaxNorm)
                norm = dMaxNorm;
            else
            if (norm < dMinNorm)
                norm = dMinNorm;

            return norm;
        }
        public double LogMagnitude()
        {
            double sqr = 10.0 * Math.Log10(x * x + y * y);

            if (sqr < -120.0)
                sqr = -120.0;

            return sqr;
        }

        public double LogMagnitudeSquare()
        {
            double sqr = 20.0 * Math.Log10(x * x + y * y);

            if (sqr < -180.0)
                sqr = -180.0;

            return sqr;
        }

        public double AngleRadians()
        {
            // angle is between +/- PI
            if (x == 0.0)
            {
                // 90/270 degrees
                if (y >= 0)
                    return Math.PI * 0.5;
                else
                    return Math.PI * 1.5;
            }

            if (x > 0)
            {
                if (y < 0)
                {
                    // 270-360 degrees
                    return 2.0 * Math.PI - Math.Atan(-y / x);
                }
                else
                {
                    // 0-90 degrees
                    return Math.Atan(y / x);
                }
            }
            else
            {
                if (y > 0)
                {
                    // 90-180 degrees
                    return Math.PI - Math.Atan(y / -x);
                }
                else
                {
                    // 180 - 270 degrees
                    return Math.PI + Math.Atan(y / x);
                }
            }
        }

        public static void ZeroPhase(ComplexNumber[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                c[i].x = (float)c[i].Magnitude;
                c[i].y = 0.0f;
            }
        }

        public static ComplexNumber operator +(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.x + c2.x, c1.y + c2.y);
        }

        public static ComplexNumber operator +(ComplexNumber c1, float f)
        {
            return new ComplexNumber(c1.x + f, c1.y);
        }

        public static ComplexNumber operator -(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.x - c2.x, c1.y - c2.y);
        }

        public static ComplexNumber operator -(ComplexNumber c1, float f)
        {
            return new ComplexNumber(c1.x - f, c1.y);
        }

        public static ComplexNumber operator *(ComplexNumber c1, float f)
        {
            return new ComplexNumber(c1.x * f, c1.y * f);
        }

        public static ComplexNumber operator *(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.x * c2.x - c1.y * c2.y, c1.x * c2.y + c1.y * c2.x);
        }

        public static ComplexNumber operator /(ComplexNumber c1, ComplexNumber c2)
        {
            float div = 1.0f / ((c2.x * c2.x) + (c2.y * c2.y));
            return new ComplexNumber((c1.x * c2.x + c1.y * c2.y) * div, (c1.y * c2.x - c1.x * c2.y) * div);
        }
    }
};
