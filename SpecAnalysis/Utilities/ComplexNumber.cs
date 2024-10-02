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

        /// <summary>
        /// Represents a complex number with real and imaginary components.
        /// </summary>
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

        /// <summary>
        /// Calculates the angle of the complex number in radians.
        /// </summary>
        /// <returns>The angle in radians, ranging from -π to π.</returns>
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

        /// <summary>
        /// Sets the phase of each complex number in the array to zero,
        /// keeping only the magnitude.
        ///
        /// This effectively converts each complex number to a real number,
        /// discarding its imaginary component.
        /// </summary>
        /// <param name="c">An array of complex numbers to be modified.</param>
        public static void ZeroPhase(ComplexNumber[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                c[i].x = (float)c[i].Magnitude;
                c[i].y = 0.0f;
            }
        }

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        /// <param name="c1">The first complex number.</param>
        /// <param name="c2">The second complex number.</param>
        /// <returns>The sum of the two complex numbers.</returns>
        public static ComplexNumber operator +(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.x + c2.x, c1.y + c2.y);
        }

        /// <summary>
        /// Adds a complex number and a float.
        /// </summary>
        /// <param name="c1">The complex number.</param>
        /// <param name="f">The float to add.</param>
        /// <returns>The resulting complex number.</returns>
        public static ComplexNumber operator +(ComplexNumber c1, float f)
        {
            return new ComplexNumber(c1.x + f, c1.y);
        }

        /// <summary>
        /// Subtracts one complex number from another.
        /// </summary>
        /// <param name="c1">The first complex number.</param>
        /// <param name="c2">The second complex number to subtract.</param>
        /// <returns>The difference of the two complex numbers.</returns>
        public static ComplexNumber operator -(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.x - c2.x, c1.y - c2.y);
        }

        /// <summary>
        /// Subtracts a float from a complex number.
        /// </summary>
        /// <param name="c1">The complex number.</param>
        /// <param name="f">The float to subtract.</param>
        /// <returns>The resulting complex number.</returns>
        public static ComplexNumber operator -(ComplexNumber c1, float f)
        {
            return new ComplexNumber(c1.x - f, c1.y);
        }

        /// <summary>
        /// Multiplies a complex number by a float.
        /// </summary>
        /// <param name="c1">The complex number.</param>
        /// <param name="f">The float to multiply by.</param>
        /// <returns>The resulting complex number.</returns>
        public static ComplexNumber operator *(ComplexNumber c1, float f)
        {
            return new ComplexNumber(c1.x * f, c1.y * f);
        }

        /// <summary>
        /// Multiplies two complex numbers.
        /// </summary>
        /// <param name="c1">The first complex number.</param>
        /// <param name="c2">The second complex number.</param>
        /// <returns>The product of the two complex numbers.</returns>
        public static ComplexNumber operator *(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.x * c2.x - c1.y * c2.y, c1.x * c2.y + c1.y * c2.x);
        }

        /// <summary>
        /// Divides one complex number by another.
        /// </summary>
        /// <param name="c1">The numerator complex number.</param>
        /// <param name="c2">The denominator complex number.</param>
        /// <returns>The quotient of the two complex numbers.</returns>
        public static ComplexNumber operator /(ComplexNumber c1, ComplexNumber c2)
        {
            float div = 1.0f / ((c2.x * c2.x) + (c2.y * c2.y));
            return new ComplexNumber((c1.x * c2.x + c1.y * c2.y) * div, (c1.y * c2.x - c1.x * c2.y) * div);
        }

    }
};
