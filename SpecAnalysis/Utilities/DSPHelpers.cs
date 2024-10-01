using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SpecAnalysis.Utilities
{
    public class DSPHelpers
    {
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

        //public static bool CalculatePeaks(float[] spectrumData, int dataLength, List<WaveformPeak> peakList, int zeroPadRatio)
        //{
        //    float spectrumThreshold = -100.0f;
        //    float spectrumPeak = -100.0f;
        //    float diffThreshold = 80.0f;
        //    float fundamentalPeakThreshold = 20.0f;
        //    int offset;

        //    // calculate the spectrum peak
        //    for (int i = 0, j = dataLength; i < j; i++)
        //    {
        //        if (spectrumData[i] > spectrumPeak)
        //            spectrumPeak = spectrumData[i];
        //    }

        //    switch (zeroPadRatio)
        //    {
        //        case 1:
        //            offset = 1;
        //            break;

        //        case 2:
        //            offset = 2;
        //            break;

        //        case 4:
        //            offset = 3;
        //            break;

        //        default:
        //            offset = 4;
        //            break;
        //    }

        //    peakList.Clear();

        //    // if derivative[n-1] > 0 and derivative [n+1] < 0 -> find zero crossing point between n-1, n+1, using sinc interpolator
        //    for (int i = offset, j = dataLength - offset; i < j; i++)
        //    {
        //        bool hasPeak = false;
        //        float previous1 = spectrumData[i - 1];
        //        float current = spectrumData[i];
        //        float next1 = spectrumData[i + 1];

        //        if (offset == 1)
        //        {
        //            hasPeak = (current > previous1 && current > next1);
        //        }
        //        else
        //            if (offset == 2)
        //        {
        //            float previous2 = spectrumData[i - 2];
        //            float next2 = spectrumData[i + 2];

        //            hasPeak = (current > previous1 && current > next1 && previous1 > previous2 && next1 > next2);
        //        }
        //        else
        //                if (offset == 3)
        //        {
        //            float previous2 = spectrumData[i - 2];
        //            float previous3 = spectrumData[i - 3];
        //            float next2 = spectrumData[i + 2];
        //            float next3 = spectrumData[i + 3];

        //            hasPeak = (current > previous1 && current > next1 && previous1 > previous2 && next1 > next2 && next2 > next3 && previous2 > previous3);
        //        }
        //        else
        //        {
        //            float previous2 = spectrumData[i - 2];
        //            float previous3 = spectrumData[i - 3];
        //            float previous4 = spectrumData[i - 4];
        //            float next2 = spectrumData[i + 2];
        //            float next3 = spectrumData[i + 3];
        //            float next4 = spectrumData[i + 4];

        //            hasPeak = (current > previous1 && current > next1 && previous1 > previous2 && next1 > next2 && next2 > next3 && previous2 > previous3 && next3 > next4 && previous3 > previous4);

        //        }

        //        if (hasPeak)
        //        {
        //            float peakX = 0.0f;
        //            float peakY = -120.0f;

        //            FindPeakLocationQuadratic(previous1, current, next1, ref peakX, ref peakY);

        //            peakX = Convert.ToSingle(i) + peakX;

        //            if (peakY >= spectrumThreshold && (spectrumPeak - peakY) < diffThreshold)
        //            {
        //                peakList.Add(new WaveformPeak(peakX, peakY));
        //                i += 2;
        //            }
        //        }
        //    }

        //    // we found the peaks, now it's time to find the root note
        //    // find the peak with the maximum number of harrmonics, that's gonna be our root note
        //    int lastIndex = -1;
        //    int lastNumberOfHarmonics = 0;
        //    float lastFrequency = 0;

        //    for (int i = 0; i < peakList.Count; i++)
        //    {
        //        float currentFrequency = peakList[i].bin;
        //        float currentPeak = peakList[i].peak;

        //        if ((spectrumPeak - currentPeak) <= fundamentalPeakThreshold)
        //        {
        //            int numberOfHarmonics = 0;

        //            for (int j = i + 1; j < peakList.Count; j++)
        //            {
        //                float nextFrequency = peakList[j].bin;
        //                float frequencyRatio = nextFrequency / currentFrequency;
        //                int harmonicNumber = Convert.ToInt32(frequencyRatio + 0.2f);
        //                float diff = Math.Abs(frequencyRatio - Convert.ToSingle(harmonicNumber));

        //                if (diff <= 0.15f)
        //                    numberOfHarmonics++;
        //            }

        //            if (numberOfHarmonics < lastNumberOfHarmonics)
        //                break;

        //            lastNumberOfHarmonics = numberOfHarmonics;
        //            lastFrequency = currentFrequency;
        //            lastIndex = i;
        //        }
        //    }

        //    if (lastIndex >= 0)
        //    {
        //        float fundamentalFrequency = peakList[lastIndex].bin;

        //        peakList[lastIndex].harmonicNumber = 1;

        //        for (int i = lastIndex + 1; i < peakList.Count; i++)
        //        {
        //            float nextFrequency = peakList[i].bin;
        //            float frequencyRatio = nextFrequency / fundamentalFrequency;

        //            int harmonicNumber = Convert.ToInt32(frequencyRatio + 0.2f);
        //            float diff = Math.Abs(frequencyRatio - Convert.ToSingle(harmonicNumber));

        //            if (diff <= 0.15f)
        //                peakList[i].harmonicNumber = harmonicNumber;
        //        }


        //    }
        //    return lastIndex > 0;

        //}

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

        public static float TransformCoordinate(float sourceValue, float sourceStart, float sourceEnd, float destStart, float destEnd)
        {
            return destStart + (destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart);
        }

        public static int TransformCoordinate(int sourceValue, int sourceStart, int sourceEnd, int destStart, int destEnd)
        {
            return destStart + (destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart);
        }

        public static int TransformCoordinate(float sourceValue, float sourceStart, float sourceEnd, int destStart, int destEnd)
        {
            return Convert.ToInt32(Convert.ToSingle(destStart) + Convert.ToSingle(destEnd - destStart) * (sourceValue - sourceStart) / (sourceEnd - sourceStart));
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

        public static void interpolateTables(float[,] pInput1, int index1, float[,] pInput2, int index2, float position, float[,] pOutput, int outputIndex, int tableLength)
        {
            for (int i = 0; i <= tableLength; i++)
            {
                pOutput[outputIndex, i] = pInput1[index1, i] + position * (pInput2[index2, i] - pInput1[index1, i]);
            }
        }

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
