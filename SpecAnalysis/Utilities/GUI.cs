using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecAnalysis.Utilities;

namespace SpecAnalysis
{
    class GUI
    {
        public static void ConvertBytesToFloat(byte[] input, float[] output, int numberOfSamples)
        {
            float k = 1.0f / 2147483647.0f;

            for (int i = 0, j = 0; i < numberOfSamples; i++, j += 4)
            {
                int tempInt = BitConverter.ToInt32(input, j);

                if (tempInt > 2147483647)
                    tempInt = 2147483647;
                else
                if (tempInt < -2147483647)
                    tempInt = -2147483647;

                float floatValue = Convert.ToSingle(tempInt) * k;
                output[i] = floatValue;
            }

        }
        /// <summary>
        /// Creates a linearly spaced table of values and optionally calculates their differences.
        /// </summary>
        /// <param name="table">The array to store the calculated values.</param>
        /// <param name="initialValue">The initial value of the table.</param>
        /// <param name="finalValue">The final value of the table.</param>
        /// <param name="numberOfElements">The number of elements to generate in the table.</param>
        /// <param name="calculateDifferences">Whether to calculate and store the differences between consecutive elements.</param>
        public static void CreateLinearSpacedTable(double[] table, float initialValue, float finalValue, int numberOfElements, bool calculateDifferences)
        {
            double currentValue = initialValue;
            double stepSize = Convert.ToDouble(finalValue - initialValue) / Convert.ToDouble(numberOfElements - 1);
            int i;
            for (i = 0; i < numberOfElements; i++)
            {
                table[i] = (float)currentValue;
                currentValue += stepSize;
            }
            if (calculateDifferences)
            {
                double scale = 1.0 / Convert.ToDouble(128);
                for (i = 0; i < numberOfElements; i++)
                {
                    table[i + numberOfElements] = scale * (table[i + 1] - table[i]);
                }
                table[2 * numberOfElements - 1] = 0;
            }
        }


        /// <summary>
        /// Creates a logarithmically spaced table of values and optionally calculates their differences.
        /// </summary>
        /// <param name="table">The array to store the calculated values.</param>
        /// <param name="initialValue">The initial value of the table.</param>
        /// <param name="finalValue">The final value of the table.</param>
        /// <param name="numberOfElements">The number of elements to generate in the table.</param>
        /// <param name="calculateDifferences">Whether to calculate and store the differences between consecutive elements.</param>
        public static void CreateLogSpacedTable(double[] table, double initialValue, double finalValue, int numberOfElements, bool calculateDifferences)
        {
            double currentValue = initialValue;
            double stepSize = Math.Pow(finalValue / initialValue, 1.0 / (numberOfElements - 1));
            int i;
            for (i = 0; i < numberOfElements; i++)
            {
                table[i] = currentValue;
                currentValue *= stepSize;
            }
            if (calculateDifferences)
            {
                double scale = (numberOfElements == 256) ? 1.0 / 64.0 : 1.0 / 128.0;

                for (i = 0; i < numberOfElements; i++)
                {
                    table[i + numberOfElements] = scale * (table[i + 1] - table[i]);
                }
                table[2 * numberOfElements - 1] = 0;
            }
        }

        public static float FindMax(float[] input, int inputLength, int startIndex, int endIndex)
        {
            float maxVal = -1000.0f;

            for (int i = startIndex; i < endIndex; i++)
            {
                float data = (i < inputLength) ? input[i] : 0.0f;

                if (data > maxVal)
                    maxVal = data;
            }

            return maxVal;
        }
        public static float transformCoordinateFloat(float sourceValue, float sourceStart, float sourceEnd, float destStart, float destEnd)
        {
            if (sourceEnd - sourceStart == 0)
            {
                return (destStart + destEnd) / 2;
            }

            else
                return destStart + (destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart);
        }
        public static int transformCoordinate(double sourceCoordinate, double sourceMin, double sourceMax, int destMin, int destMax)
        {
            if (sourceMax - sourceMin == 0)
            {
                return (destMin + destMax) / 2;
            }

            else
                return Convert.ToInt32(destMin + (destMax - destMin) * (sourceCoordinate - sourceMin) / (sourceMax - sourceMin));

            //return destMin + Convert.ToInt32((sourceCoordinate - sourceMin) * Convert.ToDouble(destMax - destMin) / (sourceMax - sourceMin));
        }
        public static double transformCoordinate(double sourceCoordinate, double sourceMin, double sourceMax, double destMin, double destMax)
        {
            if (sourceMax - sourceMin == 0)
            {
                return (destMin + destMax) / 2;
            }

            else
                return Convert.ToInt32(destMin + (destMax - destMin) * (sourceCoordinate - sourceMin) / (sourceMax - sourceMin));

            //return destMin + Convert.ToInt32((sourceCoordinate - sourceMin) * Convert.ToDouble(destMax - destMin) / (sourceMax - sourceMin));
        }
        public static int transformCoordinateInt(int sourceCoordinate, int sourceMin, int sourceMax, int destMin, int destMax)
        {
            if (sourceMax - sourceMin == 0)
            {
                return (destMin + destMax) / 2;
            }

            else
                return Convert.ToInt32(destMin + (destMax - destMin) * (sourceCoordinate - sourceMin) / (sourceMax - sourceMin));

            //return destMin + Convert.ToInt32(Convert.ToDouble(sourceCoordinate - sourceMin) * Convert.ToDouble(destMax - destMin) / Convert.ToDouble(sourceMax - sourceMin));
        }

        /// <summary>
        /// Fills a rounded rectangle on the specified <see cref="Graphics"/> surface.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on.</param>
        /// <param name="rectangle">The rectangle defining the bounds of the rounded rectangle.</param>
        /// <param name="fillBrush">The <see cref="Brush"/> used to fill the rectangle.</param>
        /// <param name="radiusTopLeft">The radius of the top-left corner.</param>
        /// <param name="radiusTopRight">The radius of the top-right corner.</param>
        /// <param name="radiusBottomLeft">The radius of the bottom-left corner.</param>
        /// <param name="radiusBottomRight">The radius of the bottom-right corner.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphics"/> is null.</exception>
        public static void FillRoundedRectangle(System.Drawing.Graphics graphics, Rectangle rectangle, Brush fillBrush, int radiusTopLeft, int radiusTopRight, int radiusBottomLeft, int radiusBottomRight)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics null");

            int radiusSum = radiusTopLeft + radiusTopRight + radiusBottomLeft + radiusBottomRight;

            if (radiusSum == 0)
            {
                graphics.FillRectangle(fillBrush, rectangle);
            }
            else
            {
                SmoothingMode mode = graphics.SmoothingMode;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                using (GraphicsPath path = RoundedRectangle(rectangle, radiusTopLeft, radiusTopRight, radiusBottomLeft, radiusBottomRight))
                {
                    graphics.FillPath(fillBrush, path);
                }
                graphics.SmoothingMode = mode;
            }
        }

        /// <summary>
        /// Draws a rounded rectangle on the specified <see cref="Graphics"/> surface.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on.</param>
        /// <param name="rectangle">The rectangle defining the bounds of the rounded rectangle.</param>
        /// <param name="borderPen">The <see cref="Pen"/> used to draw the border of the rectangle.</param>
        /// <param name="radiusTopLeft">The radius of the top-left corner.</param>
        /// <param name="radiusTopRight">The radius of the top-right corner.</param>
        /// <param name="radiusBottomLeft">The radius of the bottom-left corner.</param>
        /// <param name="radiusBottomRight">The radius of the bottom-right corner.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphics"/> is null.</exception>
        public static void DrawRoundedRectangle(Graphics graphics, Rectangle rectangle, Pen borderPen, int radiusTopLeft, int radiusTopRight, int radiusBottomLeft, int radiusBottomRight)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics null");

            int radiusSum = radiusTopLeft + radiusTopRight + radiusBottomLeft + radiusBottomRight;

            if (radiusSum == 0)
            {
                graphics.DrawRectangle(borderPen, rectangle);
            }
            else
            {
                SmoothingMode mode = graphics.SmoothingMode;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = RoundedRectangle(rectangle, radiusTopLeft, radiusTopRight, radiusBottomLeft, radiusBottomRight))
                {
                    graphics.DrawPath(borderPen, path);
                }
                graphics.SmoothingMode = mode;
            }
        }

        /// <summary>
        /// Creates a rounded rectangle <see cref="GraphicsPath"/> with customizable corner radii.
        /// </summary>
        /// <param name="r">The rectangle defining the bounds of the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The radius of the top-left corner.</param>
        /// <param name="radiusTopRight">The radius of the top-right corner.</param>
        /// <param name="radiusBottomLeft">The radius of the bottom-left corner.</param>
        /// <param name="radiusBottomRight">The radius of the bottom-right corner.</param>
        /// <returns>A <see cref="GraphicsPath"/> representing the rounded rectangle.</returns>
        public static System.Drawing.Drawing2D.GraphicsPath RoundedRectangle(Rectangle r, int radiusTopLeft, int radiusTopRight, int radiusBottomLeft, int radiusBottomRight)
        {
            int dTL = radiusTopLeft * 2;
            int dTR = radiusTopRight * 2;
            int dBL = radiusBottomLeft * 2;
            int dBR = radiusBottomRight * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddLine(r.Left + radiusTopLeft, r.Top, r.Right - radiusTopRight, r.Top);

            if (radiusTopRight > 0)
                path.AddArc(Rectangle.FromLTRB(r.Right - dTR, r.Top, r.Right, r.Top + dTR), -90, 90);

            path.AddLine(r.Right, r.Top + radiusTopRight, r.Right, r.Bottom - radiusBottomRight);

            if (radiusBottomRight > 0)
                path.AddArc(Rectangle.FromLTRB(r.Right - dBR, r.Bottom - dBR, r.Right, r.Bottom), 0, 90);

            path.AddLine(r.Right - radiusBottomRight, r.Bottom, r.Left + radiusBottomLeft, r.Bottom);

            if (radiusBottomLeft > 0)
                path.AddArc(Rectangle.FromLTRB(r.Left, r.Bottom - dBL, r.Left + dBL, r.Bottom), 90, 90);

            path.AddLine(r.Left, r.Bottom - radiusBottomLeft, r.Left, r.Top + radiusTopLeft);

            if (radiusTopLeft > 0)
                path.AddArc(Rectangle.FromLTRB(r.Left, r.Top, r.Left + dTL, r.Top + dTL), 180, 90);

            path.CloseFigure();

            return path;
        }

    }
}
