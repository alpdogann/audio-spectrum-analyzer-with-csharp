using SpecAnalysis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpecAnalysis
{
    public enum WindowTypeEnum
    {
        eRectangular = 0,
        eHammingWindow,
        eHanningWindow,
        eBlackmanWindow,
        eBlackmanHarrisWindow
    }

    public class FFTProcessor
    {


        int m_frameLength;
        int m_fftLength;
        int m_fftPower;
        float[] m_window;
        ComplexNumber[] m_sinCosTable;
        ComplexNumber[] m_buffer;
        int[] m_bitReversedIndexes;
        WindowTypeEnum m_windowType = WindowTypeEnum.eHammingWindow;
        public static float[,] GetWindowResponse(int frameSize, int fftSize, int oversampleRatio, int interpolationSize, WindowTypeEnum windowType)
        {
            fftSize *= oversampleRatio;

            int offset = -(interpolationSize / 2) * oversampleRatio;
            int mask = fftSize - 1;
            FFTProcessor processor = new FFTProcessor(fftSize, frameSize, windowType);
            ComplexNumber[] fftBuffer = new ComplexNumber[fftSize];
            float[,] outputBuffer = new float[oversampleRatio, interpolationSize];

            processor.FFT(fftBuffer);

            DSPHelpers.NormalizePeak(fftBuffer);

            for (int i = 0, index = offset; i < oversampleRatio; i++, index--)
            {
                for (int j = 0, index2 = index; j < interpolationSize; j++, index2 += oversampleRatio)
                {
                    outputBuffer[i, j] = (float)fftBuffer[index2 & mask].Magnitude;
                }
            }

            return outputBuffer;

        }

        public void Initialize(int fftLength, int frameLength, WindowTypeEnum windowType)
        {
            double dPower = Math.Log(Convert.ToDouble(fftLength)) / Math.Log(2.0);

            m_windowType = windowType;
            m_frameLength = frameLength;
            m_fftLength = fftLength;
            m_fftPower = (int)dPower;
            m_sinCosTable = new ComplexNumber[fftLength];
            m_bitReversedIndexes = new int[fftLength];
            m_window = new float[frameLength];
            m_buffer = new ComplexNumber[fftLength];

            // load sin cos lookup table
            double ang = 2.0 * Math.PI / Convert.ToDouble(fftLength);
            for (int i = 0; i < fftLength; i++)
            {
                m_sinCosTable[i].x = (float)Math.Cos(ang * Convert.ToDouble(i));
                m_sinCosTable[i].y = (float)Math.Sin(ang * Convert.ToDouble(i));
            }

            // create bit reversed indexes
            for (int k = 0; k < fftLength; k++)
            {
                int bitL = fftLength >> 1;
                int C = 0;
                int tmp = k;

                for (int i = 0; i < m_fftPower; i++)
                {
                    if ((tmp & 1) > 0)
                        C += bitL;
                    tmp >>= 1;
                    bitL >>= 1;
                }

                m_bitReversedIndexes[k] = C;
            }

            // create Hamming window
            float a0 = 0.0f;
            float a1 = 0.0f;
            float a2 = 0.0f;
            float a3 = 0.0f;
            float angle1 = (float)(2.0 * Math.PI / Convert.ToDouble(frameLength - 1));
            float angle2 = (float)(4.0 * Math.PI / Convert.ToDouble(frameLength - 1));
            float angle3 = (float)(6.0 * Math.PI / Convert.ToDouble(frameLength - 1));

            switch (windowType)
            {
                case WindowTypeEnum.eRectangular:
                    a0 = 1.0f;
                    break;

                case WindowTypeEnum.eHammingWindow:
                    a0 = 0.54f;
                    a1 = 0.46f;
                    break;

                case WindowTypeEnum.eHanningWindow:
                    a0 = 0.5f;
                    a1 = 0.5f;
                    break;

                case WindowTypeEnum.eBlackmanWindow:
                    a0 = 0.42659f;
                    a1 = 0.49656f;
                    a2 = 0.076849f;
                    break;

                case WindowTypeEnum.eBlackmanHarrisWindow:
                    a0 = 0.35875f;
                    a1 = 0.48829f;
                    a2 = 0.14128f;
                    a3 = 0.01168f;
                    break;
            }

            for (int i = 0; i < frameLength; i++)
            {
                double result = a0 - a1 * Math.Cos(angle1 * Convert.ToDouble(i)) + a2 * Math.Cos(angle2 * Convert.ToDouble(i)) - a3 * Math.Cos(angle3 * Convert.ToDouble(i));
                m_window[i] = (float)result;
            }
        }

        public FFTProcessor(int fftLength)
        {
            this.Initialize(fftLength, fftLength, WindowTypeEnum.eHammingWindow);
        }

        public FFTProcessor(int fftLength, int frameLength)
        {
            this.Initialize(fftLength, frameLength, WindowTypeEnum.eHammingWindow);
        }

        public FFTProcessor(int fftLength, int frameLength, WindowTypeEnum windowType)
        {
            this.Initialize(fftLength, frameLength, windowType);
        }

        public void IDFT(ComplexNumber[] input, float[] output)
        {
            float gain = 1.0f;
            float maxVal = 0.0f;
            bool bNormalize = false;

            for (int k = 0; k < m_fftLength; k += 1)
            {
                float sum;
                int freq = 0;
                int mask = m_fftLength - 1;

                sum = 0.0f;

                for (int n = 0; n < m_fftLength; n++)
                {
                    sum += input[n].x * m_sinCosTable[freq].x - input[n].y * m_sinCosTable[freq].y;
                    freq = (freq + k);

                    if (freq >= m_fftLength)
                        freq -= m_fftLength;
                }

                output[k] = sum * gain;
                if (Math.Abs(output[k]) > maxVal)
                    maxVal = Math.Abs(output[k]);
            }

            if (bNormalize == true)
            {
                gain = 1.0F / maxVal;
                for (int i = 0; i < m_fftLength; i++)
                {
                    output[i] *= gain;
                }
            }
        }

        public void DFT(float[] input, ComplexNumber[] output, int interval)
        {
            WindowInput(input, m_buffer);
            int lenOver2 = m_fftLength >> 1;

            for (int k = 0; k < m_fftLength; k += interval)
            {
                ComplexNumber sum;
                int freq = 0;
                int mask = m_fftLength - 1;

                sum.x = 0.0f;
                sum.y = 0.0f;

                for (int n = 0; n < m_fftLength; n++)
                {
                    sum.x += input[n] * m_sinCosTable[freq].x;
                    sum.y -= input[n] * m_sinCosTable[freq].y;

                    freq = freq + k;

                    if (freq >= m_fftLength)
                        freq -= m_fftLength;

                }

                output[k] = sum;
            }

            float scaler = 1.0f / Convert.ToSingle(m_fftLength);
            for (int k = 0; k < m_fftLength; k++)
            {
                output[k].x *= scaler;
                output[k].y *= scaler;
            }
        }

        public void FFT(float[] input, ComplexNumber[] output)
        {
            WindowInput(input, m_buffer);
            DecimationInFrequencyFFT(m_buffer, output);
        }

        public void FFT(float[] input, int offset, ComplexNumber[] output)
        {
            WindowInput(input, offset, m_buffer);
            DecimationInFrequencyFFT(m_buffer, output);
        }

        public void FFT(ComplexNumber[] output)
        {
            WindowInput(null, m_buffer);
            DecimationInFrequencyFFT(m_buffer, output);
        }

        public void FFT(ComplexNumber[] input, ComplexNumber[] output)
        {
            DecimationInFrequencyFFT(input, output);
        }

        public void IFFT(ComplexNumber[] input, float[] output)
        {
            DecimationInFrequencyIFFT(input, output);
        }

        public void IFFT(ComplexNumber[] input, float[] output, int offset)
        {
            DecimationInFrequencyIFFT(input, output, offset);
        }

        private void WindowInput(float[] input, ComplexNumber[] output)
        {
            // this function multiplies the time signals with the window signal
            // padds zeros to the end of the sequences if necessary
            float scaler = 1.0f / Convert.ToSingle(m_frameLength);

            if (input != null)
            {
                for (int i = 0; i < m_frameLength; i++)
                {
                    output[i].x = scaler * input[i] * m_window[i];
                    output[i].y = 0.0F;
                }
            }
            else
            {
                for (int i = 0; i < m_frameLength; i++)
                {
                    output[i].x = scaler * m_window[i];
                    output[i].y = 0.0F;
                }
            }

            // zero pad the rest
            for (int i = m_frameLength; i < m_fftLength; i++)
            {
                output[i].x = 0.0f;
                output[i].y = 0.0f;
            }
        }

        private void WindowInput(float[] input, int offset, ComplexNumber[] output)
        {
            // this function multiplies the time signals with the window signal
            // padds zeros to the end of the sequences if necessary
            float scaler = 1.0f / Convert.ToSingle(m_frameLength);
            int inputLength = input.Length;

            for (int i = 0, j = offset; i < m_frameLength; i++, j++)
            {
                float data = (j < inputLength) ? input[j] : 0.0f;
                output[i].x = scaler * data * m_window[i];
                output[i].y = 0.0F;
            }

            // zero pad the rest
            for (int i = m_frameLength; i < m_fftLength; i++)
            {
                output[i].x = 0.0f;
                output[i].y = 0.0f;
            }
        }

        private void DecimationInFrequencyFFT(ComplexNumber[] Input, ComplexNumber[] output)
        {
            // this fft algorithm uses decimation in frequency
            // the input may come in bit reversed index form
            // n is the FFT length
            // m is the FFT power -> log2(n);
            int n = m_fftLength;
            int m = m_fftPower - 1;
            int indexIncrement = n;
            int innerLoops = 1;
            int innerLoopLength = n >> 1;

            for (int iter = 0; iter < m; iter++)
            {
                for (int index = 0, loops = 0; loops < innerLoops; loops++, index += indexIncrement)
                {
                    for (int i = index, j = 0, angle = 0, angleIncrement = innerLoops; j < innerLoopLength; j++, i++, angle += angleIncrement)
                    {
                        int k = i + innerLoopLength;
                        ComplexNumber T = Input[k];
                        float x = Input[i].x - T.x;
                        float y = Input[i].y - T.y;
                        Input[i].x += T.x;
                        Input[i].y += T.y;
                        Input[k].x = x * m_sinCosTable[angle].x + y * m_sinCosTable[angle].y;
                        Input[k].y = y * m_sinCosTable[angle].x - x * m_sinCosTable[angle].y;
                    }
                }

                indexIncrement >>= 1;
                innerLoops <<= 1;
                innerLoopLength >>= 1;
            }

            for (int i = 0; i < n; i += 2)
            {
                int j = i + 1;
                int reverseI = m_bitReversedIndexes[i];
                int reverseJ = m_bitReversedIndexes[j];
                ComplexNumber InputI = Input[i];
                ComplexNumber InputJ = Input[j];
                output[reverseI].x = Input[i].x + Input[j].x;
                output[reverseI].y = Input[i].y + Input[j].y;
                output[reverseJ].x = Input[i].x - Input[j].x;
                output[reverseJ].y = Input[i].y - Input[j].y;
            }

            for (int i = 0; i < n; i++)
            {
                if (output[i].Magnitude < 0.000001)
                {
                    output[i].x = 0.0f;
                    output[i].y = 0.0f;
                }
            }
        }

        private void DecimationInFrequencyIFFT(ComplexNumber[] Input, float[] output)
        {
            // this fft algorithm uses decimation in frequency
            // the input may come in bit reversed index form
            // n is the FFT length
            // m is the FFT power -> log2(n);
            int n = m_fftLength;
            int m = m_fftPower - 1;
            int indexIncrement = n;
            int innerLoops = 1;
            int innerLoopLength = n >> 1;

            for (int iter = 0; iter < m; iter++)
            {
                for (int index = 0, loops = 0; loops < innerLoops; loops++, index += indexIncrement)
                {
                    for (int i = index, j = 0, angle = 0, angleIncrement = innerLoops; j < innerLoopLength; j++, i++, angle += angleIncrement)
                    {
                        int k = i + innerLoopLength;
                        ComplexNumber T = Input[k];
                        float x = Input[i].x - T.x;
                        float y = Input[i].y - T.y;
                        Input[i].x += T.x;
                        Input[i].y += T.y;
                        Input[k].x = x * m_sinCosTable[angle].x - y * m_sinCosTable[angle].y;
                        Input[k].y = y * m_sinCosTable[angle].x + x * m_sinCosTable[angle].y;
                    }
                }

                indexIncrement >>= 1;
                innerLoops <<= 1;
                innerLoopLength >>= 1;
            }

            for (int i = 0; i < n; i += 2)
            {
                int j = i + 1;
                int reverseI = m_bitReversedIndexes[i];
                int reverseJ = m_bitReversedIndexes[j];
                output[reverseI] = Input[i].x + Input[j].x;
                output[reverseJ] = Input[i].x - Input[j].x;
            }
        }
        private void DecimationInFrequencyIFFT(ComplexNumber[] Input, float[] output, int offset)
        {
            // this fft algorithm uses decimation in frequency
            // the input may come in bit reversed index form
            // n is the FFT length
            // m is the FFT power -> log2(n);
            int n = m_fftLength;
            int m = m_fftPower - 1;
            int indexIncrement = n;
            int innerLoops = 1;
            int innerLoopLength = n >> 1;

            for (int iter = 0; iter < m; iter++)
            {
                for (int index = 0, loops = 0; loops < innerLoops; loops++, index += indexIncrement)
                {
                    for (int i = index, j = 0, angle = 0, angleIncrement = innerLoops; j < innerLoopLength; j++, i++, angle += angleIncrement)
                    {
                        int k = i + innerLoopLength;
                        ComplexNumber T = Input[k];
                        float x = Input[i].x - T.x;
                        float y = Input[i].y - T.y;
                        Input[i].x += T.x;
                        Input[i].y += T.y;
                        Input[k].x = x * m_sinCosTable[angle].x - y * m_sinCosTable[angle].y;
                        Input[k].y = y * m_sinCosTable[angle].x + x * m_sinCosTable[angle].y;
                    }
                }

                indexIncrement >>= 1;
                innerLoops <<= 1;
                innerLoopLength >>= 1;
            }

            for (int i = 0; i < n; i += 2)
            {
                int j = i + 1;
                int reverseI = offset + m_bitReversedIndexes[i];
                int reverseJ = offset + m_bitReversedIndexes[j];
                output[reverseI] = Input[i].x + Input[j].x;
                output[reverseJ] = Input[i].x - Input[j].x;
            }
        }
    }
}
