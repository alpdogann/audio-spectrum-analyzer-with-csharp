using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SpecAnalysis
{
    public class SignalGenerator
    {
        int counter = 0;
        bool isCounterHigh = false;
        float sinePhase = 0;
        float sineGain = 0.5f;
        float sweepCurrentTime = 0.0f;
        float sweepFrequencyFactor = 0.0f;
        float sweepCurrentCyclePosition = 0.0f;
        float sweepFrequency = 0.0f;
        Random rand = new Random();
        public void CreateWaveformSquare(float[] buffer, int numberOfSamples, float frequency, float gain)
        {
            int n = numberOfSamples;
            int n1 = numberOfSamples >> 1;
            float[] inputTable = new float[numberOfSamples];

            for (int i = 0; i < n1; i++)
            {
                inputTable[i] = 1.0f;
            }
            for (int i = n1; i < n; i++)
            {
                inputTable[i] = -1.0f;
            }

            for (int i = 0, j = 0, m = (int)frequency; i < n; i++)
            {
                float input = gain * inputTable[j];
                buffer[i] = input;

                j = (j + m) & (numberOfSamples - 1);
            }

        }
        public void CreateLogAmplitudeSineWave(float[] buffer, int numberOfSamples, int frequency, int samplingRate)
        {
            float f = Convert.ToSingle(Math.PI * 2 * frequency / Convert.ToDouble(samplingRate));
            float firstAmplitude = 0.00001f;
            float lastAmplitude = 1.0f;
            float changeRate = Convert.ToSingle(Math.Pow(lastAmplitude / firstAmplitude,  1.0f / (float)(numberOfSamples - 1)));

            for (int i = 0; i < numberOfSamples; i++)
            {
                buffer[i] = firstAmplitude * Convert.ToSingle(Math.Sin(sinePhase));
                firstAmplitude *= changeRate;

                if (sinePhase < Convert.ToSingle(Math.PI * 2.0 * frequency))
                    sinePhase += f;
                else
                    sinePhase -= Convert.ToSingle(2.0 * Math.PI * frequency);
            }
            
        }
        public void CreateExponentialSineSweep(float[] buffer, int numberOfSamples, int fStart, int fStop, int samplingRate, int sweepDuration)
        {
            float wStart = Convert.ToSingle(2.0f * Math.PI * fStart / samplingRate);
            float wStop = Convert.ToSingle(2.0f * Math.PI * fStop / samplingRate);
            float wStopDividedByStart = wStop / wStart; // sweep rate
            int sweepLength = sweepDuration * samplingRate;

            for (int i = 0; i < numberOfSamples; i++)
            {
                buffer[i] = 0.25f * Convert.ToSingle(Math.Sin(((wStart * (sweepLength - 1)) / (Math.Log(wStopDividedByStart))) * (Math.Exp((i * Math.Log(wStopDividedByStart)) / (sweepLength - 1)) - 1)));

            }
        }
        public void CreateWaveformSawtooth(float[] buffer, int numberOfSamples, float frequency, float gain, int samplingRate)
        {
            /*float k = 1.0f / Convert.ToSingle(numberOfSamples >> 1);
            int n = numberOfSamples;
            float[] inputTable = new float[numberOfSamples];

            for (int i = 0; i < n; i++)
            {
                inputTable[i] = Convert.ToSingle(i) * k - 1.0f;
            }

            for (int i = 0, j = 0, m = frequency; i < numberOfSamples; i++)
            {
                float input = gain * inputTable[j];
                buffer[i] = input;

                j = (j + m) & (numberOfSamples - 1);
            }*/

            float f = 2.0f * Convert.ToSingle(Math.PI * frequency / samplingRate);
            float phase = 0.0f;

            for (int i = 0; i < numberOfSamples; i++)
            {
                buffer[i] = Convert.ToSingle(1.0f - Math.Abs((phase % 2) - 1));             

                /*if (phase >= Convert.ToSingle(2.0f * Math.PI * frequency))
                    phase = (phase - f) % 1;
                else*/
                    phase = (phase + f) % 1;
            }
        }
        public void GenerateTriangle(float[] buffer, int numberOfSamples, int frequency, float gain)
        {
            int n = numberOfSamples;
            int n1 = n >> 2;
            int n2 = n >> 1;
            int n3 = n1 * 3;
            float k = 1.0f / Convert.ToSingle(n1);
            float[] inputTable = new float[numberOfSamples];

                for (int i = 0; i < n1; i++)
                {
                    inputTable[i] = Convert.ToSingle(i) * k;
                }

                for (int i = 0, j = n1; i < n1; i++, j++)
                {
                    inputTable[j] = 1.0f - Convert.ToSingle(i) * k;
                }

                for (int i = 0, j = n2; i < n1; i++, j++)
                {
                    inputTable[j] = Convert.ToSingle(-i) * k;
                }

                for (int i = 0, j = n3; i < n1; i++, j++)
                {
                    inputTable[j] = Convert.ToSingle(i) * k - 1.0f;
                }

            for (int i = 0, j = 0, m = frequency; i < n; i++)
            {
                float input = gain * inputTable[j];
                buffer[i] = input;

                j = (j + m) & (numberOfSamples - 1);
            }
        }
        public void GenerateSineWaveform(float[] output, int numberOfSamples, float sineFrequencyHz, int samplingRate, SineWaveType type, int driveIndex = 0)
        {
            float f = Convert.ToSingle(Math.PI * 2 * sineFrequencyHz / Convert.ToDouble(samplingRate));

            int m_driveIndex = driveIndex;

            for (int i = 0; i < numberOfSamples; i++)
            {
                if(type == SineWaveType.Sine)
                {
                    output[i] = Convert.ToSingle(sineGain * Math.Sin(sinePhase));
                }
                else
                    if(type == SineWaveType.ClippedSine)
                {
                    output[i] = Convert.ToSingle(sineGain * Math.Sin(sinePhase));

                    if (Convert.ToSingle(Math.Abs(output[i])) <= 0.33)
                    output[i] = 2.0f * output[i];
                else
                    if (Convert.ToSingle(Math.Abs(output[i])) >= 0.33)
                {
                    if (output[i] > 0)
                        output[i] = (3.0f - (Convert.ToSingle(Math.Pow(2 - (3.0f * output[i]), 2)))) / 3.0f;
                    else
                        if (output[i] < 0)
                        output[i] = -((3.0f - (Convert.ToSingle(Math.Pow(2 - (3.0f * Convert.ToSingle(Math.Abs(output[i]))), 2)))) / 3.0f);
                }                  
                else
                    if (Convert.ToSingle(Math.Abs(output[i])) >= 0.66)
                {
                    if(output[i] > 0)
                        output[i] = 1.0f;
                    else
                        if(output[i] < 0)
                        output[i] = -1.0f;
                }

                }
                else 
                    if(type == SineWaveType.HalfWaveSine)
                {
                    output[i] = Convert.ToSingle(sineGain * Math.Sin(sinePhase)) < 0 ? 0 : Convert.ToSingle(sineGain * Math.Sin(sinePhase)); // half wave rectification, even order harmonics generated
                }
                else
                    if(type == SineWaveType.FullWaveSine)
                {
                    output[i] = Convert.ToSingle(sineGain * Math.Sin(sinePhase)) < 0 ? /*Convert.ToSingle(sineGain * Math.Sin(sinePhase))*/ -Convert.ToSingle(sineGain * Math.Sin(sinePhase)) : Convert.ToSingle(sineGain * Math.Sin(sinePhase)); // full wave rectification, fundamental frequency absent
                                                                                       // keeps the original fundamental frequency
                }
                else
                    if(type == SineWaveType.Drive)
                {
                    switch (m_driveIndex)
                    {
                        case 0:
                            // 0 //
                                output[i] = Convert.ToSingle(0.01f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) + 0.025f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 1:
                            // 1 //
                                output[i] = Convert.ToSingle(0.01f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.025f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 2:
                            // 2 //
                                output[i] = Convert.ToSingle(1.259f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.014f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.032f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 3:
                            // 3 //
                                output[i] = Convert.ToSingle(1.413f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.02f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.045f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 4:
                            // 4 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.025f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.045f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 5:
                            // 5 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.035f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.056f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 6:
                            // 6 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.045f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.071f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 7:
                            // 7 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.056f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.1f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 8:
                            // 8 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.071f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.141f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) - 0.013f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 9:
                            // 9 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.079f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.178f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) - 0.014f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 10:
                            // 10 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.079f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.2f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) - 0.013f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 11:
                            // 11 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.079f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.224f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.01f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.018f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 12:
                            // 12 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.079f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.251f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.016f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.016f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) - 0.02f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 13:
                            // 13 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.079f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.282f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.018f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.028f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) - 0.022f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)) - 0.01f * Convert.ToSingle(9* sineGain * Math.Sin(sinePhase)));
                            break;
                        case 14:
                            // 14 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.079f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.282f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.022f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.04f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) - 0.02f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)) - 0.013f * Convert.ToSingle(9 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 15:
                            // 15 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.089f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.316f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.028f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.05f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) - 0.016f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)) - 0.016f * Convert.ToSingle(9 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 16:
                            // 16 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.1f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.316f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.04f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.063f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) - 0.011f * Convert.ToSingle(7 * sineGain * Math.Sin(sinePhase)) + 0.011f * Convert.ToSingle(8 * sineGain * Math.Sin(sinePhase)) - 0.018f * Convert.ToSingle(9 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 17:
                            // 17 //
                                output[i] = Convert.ToSingle(1.585f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.126f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.316f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.056f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.071f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) + 0.014f * Convert.ToSingle(8 * sineGain * Math.Sin(sinePhase)) - 0.014f * Convert.ToSingle(9 * sineGain * Math.Sin(sinePhase)) + 0.011f * Convert.ToSingle(10 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 18:
                            // 18 //
                                output[i] = Convert.ToSingle(1.413f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.141f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.316f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.071f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.079f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) + 0.13f * Convert.ToSingle(6 * sineGain * Math.Sin(sinePhase)) + 0.014f * Convert.ToSingle(8 * sineGain * Math.Sin(sinePhase)) - 0.1f * Convert.ToSingle(9 * sineGain * Math.Sin(sinePhase)) + 0.014f * Convert.ToSingle(10 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 19:
                            // 19 //
                                output[i] = Convert.ToSingle(1.413f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.178f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.316f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.089f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.079f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) + 0.022f * Convert.ToSingle(6 * sineGain * Math.Sin(sinePhase)) + 0.013f * Convert.ToSingle(8 * sineGain * Math.Sin(sinePhase)) + 0.018f * Convert.ToSingle(10 * sineGain * Math.Sin(sinePhase)));
                            break;
                        case 20:
                            // 20 //
                                output[i] = Convert.ToSingle(1.413f * Convert.ToSingle(sineGain * Math.Sin(sinePhase)) + 0.178f * Convert.ToSingle(2 * sineGain * Math.Sin(sinePhase)) - 0.316f * Convert.ToSingle(3 * sineGain * Math.Sin(sinePhase)) + 0.089f * Convert.ToSingle(4 * sineGain * Math.Sin(sinePhase)) - 0.079f * Convert.ToSingle(5 * sineGain * Math.Sin(sinePhase)) + 0.022f * Convert.ToSingle(6 * sineGain * Math.Sin(sinePhase)) + 0.013f * Convert.ToSingle(8 * sineGain * Math.Sin(sinePhase)) + 0.018f * Convert.ToSingle(10 * sineGain * Math.Sin(sinePhase)));
                            break;
                        default:
                            break;
                    }
                }

                if (sinePhase < Convert.ToSingle(Math.PI * 2.0 * sineFrequencyHz))
                    sinePhase += f;
                else
                    sinePhase = 0;
            }
            
        }
        public void GenerateWhiteNoise(float[] output, int numberOfSamples)
        {
            /*for (int i = 0; i < numberOfSamples; i++)
            {
                if ((counter < 192000 && !isCounterHigh) || (counter == 0 && isCounterHigh))
                {
                    output[i] = 2.0f * (Convert.ToSingle(rand.NextDouble()) - 0.5f);
                    counter++;
                    isCounterHigh = false;
                }
                else if ((counter > 0 && isCounterHigh) || (counter >= 192000 && !isCounterHigh))
                {
                    output[i] = 0;
                    counter--;
                    isCounterHigh = true;
                }

            }*/

            

            /*for (int i = 0; i < numberOfSamples; i++)
            {
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                output[i] = (float)randStdNormal;
            }*/

            for (int i = 0; i < numberOfSamples; i++)
            {
                output[i] = GUI.transformCoordinateFloat(Convert.ToSingle(rand.NextDouble()), 0, 1, -0.5f, 0.5f);
            }
        }
        public void GenerateSineSweep(float[] output, int numberOfSamples, int sampleRate, int sweepDuration, float lowFreq, float highFreq, bool isLogarithmic)
        {
            float deltaTime = 1.0f / sampleRate;
            float freq = lowFreq;
            float sweepLogFrequencyFactor = lowFreq / highFreq;
            float expGrowth = Convert.ToSingle(Math.Pow((float)highFreq / (float)lowFreq, (float)(1 / (((float)sweepDuration * (float)sampleRate) - 1))));
            if (isLogarithmic)
            {

                for (int i = 0; i < numberOfSamples; i++)
                {
                    sweepFrequency = highFreq * sweepLogFrequencyFactor;

                    sweepCurrentCyclePosition += sweepFrequency / sampleRate;

                    output[i] = Convert.ToSingle(0.25f * Math.Sin(sweepCurrentCyclePosition * 2 * Math.PI));

                    if (sweepCurrentTime > sweepDuration)
                    {
                        sweepCurrentTime -= sweepDuration;
                        sweepCurrentTime += deltaTime;
                        sweepLogFrequencyFactor = lowFreq / highFreq;
                        //sweepCurrentCyclePosition = 1.0f;
                    }
                    else
                    {
                        sweepCurrentTime += deltaTime;
                        sweepLogFrequencyFactor = sweepCurrentTime / sweepDuration;
                    }
                }
            }
            else if(!isLogarithmic)
            {
                for (int i = 0; i < numberOfSamples; i++)
                {
                    sweepFrequency = lowFreq + ((highFreq - lowFreq) * sweepFrequencyFactor);

                    sweepCurrentCyclePosition += sweepFrequency / sampleRate;

                    output[i] = Convert.ToSingle(0.25f * Math.Sin(sweepCurrentCyclePosition * 2 * Math.PI));

                    if (sweepCurrentTime > sweepDuration)
                    {
                        sweepCurrentTime -= sweepDuration;
                        sweepCurrentTime += deltaTime;
                        sweepFrequencyFactor = 0.0f;
                        //sweepCurrentCyclePosition = 0.0f;
                    }
                    else
                    {
                        sweepCurrentTime += deltaTime;
                        sweepFrequencyFactor = sweepCurrentTime / sweepDuration; 
                    }

                }
            }
            

        }
    }
}
