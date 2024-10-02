using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecAnalysis.Utilities;

namespace SpecAnalysis
{
    class Filters
    {
        private static int samplingRate = 44100;
        private double[] m_QGainTable = new double[256];
        private double[] m_QResotable = new double[256];
        private double[] filterQTable;
        private double[] filterGainTable;
        private double[] m_combFeedbackTable;
        private int minResponseDB = -120;
        private int maxResponseDB = 20;

        /// <summary>
        /// Generates a frequency table based on the specified size.
        /// The frequency values are calculated using a geometric progression 
        /// between an initial and final frequency, scaled by the sampling rate.
        /// </summary>
        /// <param name="size">The number of frequency values to generate.</param>
        /// <returns>An array of frequency values.</returns>
        public double[] getFrequencyTable(int size)
        {
            double[] frequencyTable2 = new double[size];

            double finalFrequency2 = 0.99 * samplingRate * 0.5;
            double initialFrequency2 = finalFrequency2 / Math.Pow(2.0, 128.0 / 12.0);
            double multiplier2 = Math.Pow(finalFrequency2 / initialFrequency2, 1.0 / Convert.ToDouble(size - 1));
            double addend = (finalFrequency2 - initialFrequency2) / Convert.ToDouble(size - 1);

            for (int i = 0; i < size; i++)
            {
                frequencyTable2[i] = GUI.transformCoordinateFloat((float)initialFrequency2,
                    (float)finalFrequency2 / (float)Math.Pow(2.0, 128.0 / 12.0),
                    (float)finalFrequency2, 0, 1.0f);
                initialFrequency2 *= multiplier2;
            }

            return frequencyTable2;
        }

        /// <summary>
        /// Initializes the comb feedback table and the Q gain table.
        /// This method populates the Q gain table based on the resonant frequencies
        /// and calculates the feedback values for the comb filter.
        /// </summary>
        public void initializeCombFeedbackTable()
        {
            // Create a linear spaced table for Q resonances
            GUI.CreateLinearSpacedTable(m_QResotable, 0.0f, 5.0f, 128, true);

            // Calculate Q gain values based on the Q resonances
            for (int i = 0; i < 128; i++)
            {
                m_QGainTable[i] = 1.0 / (1.0 + m_QResotable[i]);
            }
            for (int i = 0; i < 128; i++)
            {
                m_QGainTable[i + 128] = (m_QGainTable[i + 1] - m_QGainTable[i]) / 128.0f;
            }
            m_QGainTable[255] = 0;

            // Initialize the comb feedback table
            double r = Math.Pow(0.001, 1.0 / 127.0);
            double temp = 1.0;

            m_combFeedbackTable = new double[256];

            for (int i = 0; i < 128; i++)
            {
                m_combFeedbackTable[i] = 0.997f * (1.0 - temp);
                temp *= r;
            }
            for (int i = 0; i < 128; i++)
            {
                m_combFeedbackTable[i + 128] = (1.0 / 128.0) * (m_combFeedbackTable[i + 1] - m_combFeedbackTable[i]);
            }
            m_combFeedbackTable[255] = 0;
        }

        /// <summary>
        /// Initializes the gain table for the filter.
        /// This method populates the filter gain table with values
        /// linearly spaced between an initial and a final value.
        /// </summary>
        public void initializeGainTable()
        {
            filterGainTable = new double[128];
            double final = 2.52;
            double initial = 0.62;
            double addend = (final - initial) / 127;

            for (int i = 0; i < 128; i++)
            {
                filterGainTable[i] = initial;
                initial += addend;
            }
        }

        /// <summary>
        /// Initializes the resonance table for the filter.
        /// This method populates the filter Q table with values
        /// exponentially spaced between an initial and a final value.
        /// </summary>
        public void initializeResonanceTable()
        {
            filterQTable = new double[128];
            double final = 100;
            double initial = 0.5;
            double multiplier = Math.Pow(final / initial, 1.0 / 127.0);

            for (int i = 0; i < 128; i++)
            {
                filterQTable[i] = Math.Sqrt(initial);
                initial *= multiplier;
            }
        }

        public void calculateLP12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eLowpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain * m_QGainTable[filterResonance]);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = coeffs1.ComplexResponseAt(frequencyTable[i]);
                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateLP24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eLowpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);
            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eLowpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain * m_QGainTable[filterResonance]);
            for (int i = 0; i < outputLength; i++)
            {
                ComplexNumber response1 = coeffs1.ComplexResponseAt(frequencyTable[i]);
                ComplexNumber response2 = coeffs2.ComplexResponseAt(frequencyTable[i]);

                response[i] = response1 * response2;

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateHP12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eHighpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain * m_QGainTable[filterResonance]);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = coeffs1.ComplexResponseAt(frequencyTable[i]);

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }

        public void calculateHP24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eHighpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);
            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eHighpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain * m_QGainTable[filterResonance]);

            for (int i = 0; i < outputLength; i++)
            {
                ComplexNumber response1 = coeffs1.ComplexResponseAt(frequencyTable[i]);
                ComplexNumber response2 = coeffs2.ComplexResponseAt(frequencyTable[i]);
                response[i] = response1 * response2;

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateBP12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eBandpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = coeffs1.ComplexResponseAt(frequencyTable[i]);

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateBP24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eBandpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);
            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eBandpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain);

            for (int i = 0; i < outputLength; i++)
            {
                ComplexNumber response1 = coeffs1.ComplexResponseAt(frequencyTable[i]);
                ComplexNumber response2 = coeffs2.ComplexResponseAt(frequencyTable[i]);
                response[i] = response1 * response2;

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }

        }
        public void calculateBS12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eBandstop,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = coeffs1.ComplexResponseAt(frequencyTable[i]);

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateBS24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eBandstop,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);
            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eBandstop,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            2.0f * filterGain);

            for (int i = 0; i < outputLength; i++)
            {
                ComplexNumber response1 = coeffs1.ComplexResponseAt(frequencyTable[i]);
                ComplexNumber response2 = coeffs2.ComplexResponseAt(frequencyTable[i]);

                response[i] = response1 * response2;

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateLS12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeGainTable();
            initializeCombFeedbackTable();

            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eLowpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            (2.0f * (filterGainTable[filterGain] - 1.0f)) * m_QGainTable[filterResonance]);
            ComplexNumber cOne = new ComplexNumber(1.0, 0.0);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = cOne + coeffs1.ComplexResponseAt(frequencyTable[i]);

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateLS24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeCombFeedbackTable();
            initializeGainTable();
            double[] frequencyTable = getFrequencyTable(outputLength);


            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eLowpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);
            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eLowpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            (2.0f * (filterGainTable[filterGain] - 1.0f)) * m_QGainTable[filterResonance]);
            ComplexNumber cOne = new ComplexNumber(1.0, 0.0);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = cOne + (coeffs1.ComplexResponseAt(frequencyTable[i]) * coeffs2.ComplexResponseAt(frequencyTable[i]));

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateHS12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeGainTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eHighpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            (2.0f * (filterGainTable[filterGain] - 1.0f)) * m_QGainTable[filterResonance]);
            ComplexNumber cOne = new ComplexNumber(1.0, 0.0);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = cOne + coeffs1.ComplexResponseAt(frequencyTable[i]);

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateHS24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeGainTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eHighpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);
            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eHighpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            (2.0f * (filterGainTable[filterGain] - 1.0f)) * m_QGainTable[filterResonance]);
            ComplexNumber cOne = new ComplexNumber(1.0, 0.0);

            for (int i = 0; i < outputLength; i++)
            {
                response[i] = cOne + (coeffs1.ComplexResponseAt(frequencyTable[i]) * coeffs2.ComplexResponseAt(frequencyTable[i]));

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculateCombResponse(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeCombFeedbackTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            double feedback = m_combFeedbackTable[filterResonance];
            double delay = 1.0 / frequencyTable[filterCutoff];
            double gain = filterGain * 2.0;

            for (int i = 0; i < outputLength; i++)
            {
                double angle = 2.0 * Math.PI * frequencyTable[i] * delay;
                response[i] = new ComplexNumber(1.0 - (feedback * Math.Cos(angle)), -feedback * Math.Sin(angle));

                double magnitude = gain / response[i].Norm;

                if (magnitude > 10.0)
                    magnitude = 10.0;
                else
                    if (magnitude < 0.00001)
                    magnitude = 0.00001;

                outputBuffer[i] = 20.0 * Math.Log10(magnitude);

                if (outputBuffer[i] < minResponseDB)
                    outputBuffer[i] = minResponseDB;
                else
                    if (outputBuffer[i] > maxResponseDB)
                    outputBuffer[i] = maxResponseDB;
            }

        }
        public void calculatePK12Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            initializeGainTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eBandpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            (2.0 * (filterGainTable[filterGain] - 1.0f)));

            for (int i = 0; i < outputLength; i++)
            {
                ComplexNumber cOne = new ComplexNumber(1.0, 0.0);
                response[i] = cOne + coeffs1.ComplexResponseAt(frequencyTable[i]);

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
        public void calculatePK24Response(double[] outputBuffer, int outputLength, int filterResonance, int filterCutoff, int filterGain, ComplexNumber[] response)
        {
            initializeResonanceTable();
            double[] frequencyTable = getFrequencyTable(outputLength);

            BiquadFilterCoefficients coeffs1 = new BiquadFilterCoefficients(FilterTypeEnum.eBandpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            1.0);

            BiquadFilterCoefficients coeffs2 = new BiquadFilterCoefficients(FilterTypeEnum.eBandpass,
                                                                            frequencyTable[filterCutoff],
                                                                            filterQTable[filterResonance],
                                                                            (2.0 * (filterGainTable[filterGain] - 1.0f)));

            for (int i = 0; i < outputLength; i++)
            {
                ComplexNumber cOne = new ComplexNumber(1.0, 0.0);
                response[i] = cOne + (coeffs1.ComplexResponseAt(frequencyTable[i]) * coeffs2.ComplexResponseAt(frequencyTable[i]));

                outputBuffer[i] = response[i].GetLogMagnitude(minResponseDB, maxResponseDB);
            }
        }
    }
}
