using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SpecAnalysis.Utilities
{
    public class DSPHelpers
    {
        /// <summary>
        /// Calculates the magnitude of a spectrum array and populates a magnitude array.
        /// </summary>
        /// <param name="spectrumArray">An array of complex numbers representing the spectrum.</param>
        /// <param name="magnitudeArray">An array to store the calculated magnitudes.</param>
        /// <param name="logMagnitude">If true, calculates the logarithmic magnitude; otherwise, calculates the linear magnitude.</param>
        /// <returns>The maximum magnitude value calculated from the spectrum.</returns>
        public static float CalculateMagnitude(ComplexNumber[] spectrumArray, float[] magnitudeArray, bool logMagnitude)
        {
            int spectrumLength = spectrumArray.Length / 2 + 1;
            float maxValue = -100.0f;

            if (logMagnitude)
            {
                for (int i = 0; i < spectrumLength; i++)
                {
                    float magnitude = (float)spectrumArray[i].LogMagnitude();

                    if (magnitude > maxValue)
                        maxValue = magnitude;

                    magnitudeArray[i] = magnitude;
                }
            }
            else
            {
                for (int i = 0; i < spectrumLength; i++)
                {
                    float magnitude = (float)spectrumArray[i].Magnitude;

                    if (magnitude > maxValue)
                        maxValue = magnitude;

                    magnitudeArray[i] = magnitude;
                }
            }

            // return max spectrum amplitude
            return maxValue;
        }

        /// <summary>
        /// Normalizes the peak magnitude of a complex number array to 1.
        /// </summary>
        /// <param name="array">An array of complex numbers to be normalized.</param>
        /// <remarks>
        /// This method finds the maximum magnitude in the array and scales all complex numbers 
        /// so that the maximum magnitude becomes 1. The imaginary parts are also scaled accordingly.
        /// </remarks>
        public static void NormalizePeak(ComplexNumber[] array)
        {
            int length = array.Length;
            double max = 0.0f;

            for (int i = 0; i < length; i++)
            {
                double magnitude = array[i].Magnitude;

                if (magnitude > max)
                    max = magnitude;
            }

            float scaler = (float)(1.0 / max);

            for (int i = 0; i < length; i++)
            {
                array[i].x *= scaler;
                array[i].y *= scaler;
            }
        }

        /// <summary>
        /// Applies a Moving Average filter to the input array.
        /// </summary>
        /// <param name="input">The input array to be filtered.</param>
        /// <param name="filterSize">The size of the moving average filter.</param>
        /// <remarks>
        /// This method modifies the input array by applying a moving average filter of the specified size.
        /// It pads the input array with the first and last values to handle edge cases.
        /// </remarks>
        public static void MAFilter(float[] input, int filterSize)
        {
            float fGain = 1.0f / Convert.ToSingle(filterSize);
            float[] temp = new float[input.Length + filterSize];
            int offset = filterSize / 2;

            for (int i = 0; i < offset; i++)
                temp[i] = input[0];

            for (int i = 0, j = input.Length + offset; i < offset; i++, j++)
                temp[j] = input[input.Length - 1];

            for (int i = 0, j = offset; i < input.Length; i++, j++)
            {
                temp[j] = input[i];
            }

            for (int i = 0; i < input.Length; i++)
            {
                float fSum = 0.0f;

                for (int j = 0, k = i; j < filterSize; j++, k++)
                {
                    fSum += temp[k];
                }

                input[i] = fSum * fGain;
            }

        }

        /// <summary>
        /// Pads the input array with zeros at the beginning and end.
        /// </summary>
        /// <param name="input">The array to be padded.</param>
        /// <param name="padBegin">The number of zeros to add at the beginning of the array.</param>
        /// <param name="padEnd">The number of zeros to add at the end of the array.</param>
        /// <returns>A new array that includes the original data padded with zeros.</returns>
        /// <remarks>
        /// The new array will have a length equal to the original array length plus the specified padding amounts.
        /// </remarks>
        public static float[] ZeroPad(float[] input, int padBegin, int padEnd)
        {
            int newLength = input.Length + padBegin + padEnd;
            float[] newData = new float[newLength];
            int index = 0;

            for (int i = 0; i < padBegin; i++)
                newData[index++] = 0.0f;

            Array.Copy(input, 0, newData, index, input.Length);
            index += input.Length;

            for (int i = 0; i < padEnd; i++)
                newData[index++] = 0.0f;

            return newData;
        }

        /// <summary>
        /// Transforms a floating-point value from one coordinate system to another.
        /// </summary>
        /// <param name="sourceValue">The value to be transformed.</param>
        /// <param name="sourceStart">The start of the source range.</param>
        /// <param name="sourceEnd">The end of the source range.</param>
        /// <param name="destStart">The start of the destination range.</param>
        /// <param name="destEnd">The end of the destination range.</param>
        /// <returns>The transformed value in the destination coordinate system.</returns>
        public static float TransformCoordinate(float sourceValue, float sourceStart, float sourceEnd, float destStart, float destEnd)
        {
            return destStart + (destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart);
        }

        /// <summary>
        /// Transforms an integer value from one coordinate system to another.
        /// </summary>
        /// <param name="sourceValue">The value to be transformed.</param>
        /// <param name="sourceStart">The start of the source range.</param>
        /// <param name="sourceEnd">The end of the source range.</param>
        /// <param name="destStart">The start of the destination range.</param>
        /// <param name="destEnd">The end of the destination range.</param>
        /// <returns>The transformed value in the destination coordinate system.</returns>
        public static int TransformCoordinate(int sourceValue, int sourceStart, int sourceEnd, int destStart, int destEnd)
        {
            return destStart + (destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart);
        }

        /// <summary>
        /// Transforms a floating-point value to an integer in a specified coordinate system.
        /// </summary>
        /// <param name="sourceValue">The value to be transformed.</param>
        /// <param name="sourceStart">The start of the source range.</param>
        /// <param name="sourceEnd">The end of the source range.</param>
        /// <param name="destStart">The start of the destination range.</param>
        /// <param name="destEnd">The end of the destination range.</param>
        /// <returns>The transformed value as an integer in the destination coordinate system.</returns>
        public static int TransformCoordinate(float sourceValue, float sourceStart, float sourceEnd, int destStart, int destEnd)
        {
            return Convert.ToInt32(Convert.ToSingle(destStart) + Convert.ToSingle(destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart));
        }

        /// <summary>
        /// Finds the maximum value in a specified range of a float array.
        /// </summary>
        /// <param name="input">The input array of floats.</param>
        /// <param name="inputLength">The actual length of the input array.</param>
        /// <param name="startIndex">The starting index for the search.</param>
        /// <param name="endIndex">The ending index for the search (exclusive).</param>
        /// <returns>The maximum value found in the specified range.</returns>
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

        /// <summary>
        /// Interpolates between two tables at specified indices based on a given position.
        /// </summary>
        /// <param name="pInput1">The first input table for interpolation.</param>
        /// <param name="index1">The index in the first input table.</param>
        /// <param name="pInput2">The second input table for interpolation.</param>
        /// <param name="index2">The index in the second input table.</param>
        /// <param name="position">The interpolation position between the two tables (0 to 1).</param>
        /// <param name="pOutput">The output table to store the interpolated values.</param>
        /// <param name="outputIndex">The index in the output table where results will be stored.</param>
        /// <param name="tableLength">The length of the tables.</param>
        public static void interpolateTables(float[,] pInput1, int index1, float[,] pInput2, int index2, float position, float[,] pOutput, int outputIndex, int tableLength)
        {
            for (int i = 0; i <= tableLength; i++)
            {
                pOutput[outputIndex, i] = pInput1[index1, i] + position * (pInput2[index2, i] - pInput1[index1, i]);
            }
        }

        /// <summary>
        /// Creates a linear table and fills it with scaled values based on the specified index.
        /// </summary>
        /// <param name="pTable">The table to be populated with linear values.</param>
        /// <param name="index">The index in the table to fill.</param>
        /// <param name="tableLength">The length of the table to create.</param>
        public static void createLinearTable(float[,] pTable, int index, int tableLength)
        {
            float scaler = 1.0f / Convert.ToSingle(tableLength);

            for (int i = 0; i <= tableLength; i++)
            {
                pTable[index, i] = scaler * Convert.ToSingle(i);
            }
        }
    }
}
