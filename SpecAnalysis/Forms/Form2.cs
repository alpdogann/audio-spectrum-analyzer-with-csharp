using System;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using NAudio.Wave.Asio;
using SpecAnalysis.Utilities;
using System.Threading;

namespace SpecAnalysis
{
    public enum SineWaveType
    {
        Sine,
        HalfWaveSine,
        FullWaveSine,
        ClippedSine,
        Drive
    };
    public partial class Form2 : Form
    {
        // Define the FFT size for the analysis
        private static int fftSize = 2048;
        // Define the length of each FFT frame
        private static int fftFrameLength = 1024;
        // Define the sampling rate for audio processing
        private static int samplingRate = 44100;

        // Get the path to the user's desktop directory.
        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        // Define the relative path for storing recordings, set to a folder named "test".
        static string relativePathForDirectRecording = "..\\test";
        // Get the full path for recordings by combining the desktop path with the relative path.
        private string pathForDirectRecording = Path.GetFullPath(desktopPath + relativePathForDirectRecording);

        // Create an instance of the device selection form to choose an audio device.
        static FormSelectDevice formToSelectDevice = new FormSelectDevice();
        // Show the dialog to select the device and capture the dialog result.
        DialogResult result = formToSelectDevice.ShowDialog();
        // Initialize the ASIO output using the selected device index from the form.
        AsioOut asioOut = new AsioOut(formToSelectDevice.SelectedDeviceIndex);
        // Declare a WaveIn source for recording audio; initialized to null.
        private WaveIn waveSource = null;
        // Declare a WaveFileWriter for writing audio data to a file; initialized to null.
        private WaveFileWriter waveFile = null;
        // Create a signal generator instance for producing audio signals.
        SignalGenerator generator = new SignalGenerator();
        // Create an alternative signal generator from the NAudio library, configured with the sampling rate and one channel.
        NAudio.Wave.SampleProviders.SignalGenerator alternativeGenerator = new NAudio.Wave.SampleProviders.SignalGenerator(samplingRate, 1);

        // Define buffers for audio processing; initialized with a size of 32768 bytes.
        private byte[] inputBufferByte = new byte[32768];
        private byte[] outputBufferByte = new byte[32768];
        private float[] inputBufferFloat = new float[32768];
        private float[] outputBufferFloat = new float[32768];
        private float[] realTimeWaveform = new float[32768];

        // Define the folder path for recording wave files, set to a folder named "Envelope Test" on the desktop.
        private static string waveRecordFolder = Path.GetFullPath(desktopPath + "..\\Envelope Test");
        private string waveRecordFile = "TEST_NAME";
        private string waveRecordPath = "";
        private string waveform = "";

        // Define parameters for frequency sweep: low and high frequencies and duration.
        private int sweepLowFreq = 20; // Minimum frequency for sweep in Hz.
        private int sweepHighFreq = 20000; // Maximum frequency for sweep in Hz.
        private int sweepDuration = 10; // Duration of the sweep in seconds.

        // Define parameters for the sine wave generator.
        private float sineFrequencyHz = 200; // Frequency of the sine wave in Hz.
        private int signalIndex = 0;         // Index to track the current signal.


        private bool isPeriodic = false; // Flag to indicate if the peak detection is enabled.
        private int bitDepth = 16;       // Bit depth for audio recording.

        // Set the window type for the FFT processing.
        private WindowTypeEnum fftWindowType = WindowTypeEnum.eBlackmanHarrisWindow;

        private float[] shiftedIndexes = new float[fftSize / 2 +1];     // Define an array to hold shifted indexes for FFT analysis.
        private ComplexNumber[] fftOutput = new ComplexNumber[fftSize]; // Output of FFT processing.
        private ComplexNumber[] fftInput = new ComplexNumber[fftSize];  // Input to FFT processing.
        private ComplexNumber[] cFFTOutput = new ComplexNumber[fftSize];// Complex FFT output data.
        private FFTProcessor fftProcessor = new FFTProcessor(fftSize);  // Create an instance of the FFT processor for performing FFT analysis.
        private PointF[] outputPoints = new PointF[fftSize / 2 + 1];    // Array to hold output points for plotting the FFT results.
        private bool isRecording = false;
        private int driveIndex = 0;
        private bool driveEnabled = false;
        private float distortionGain = 0.0f;

        /// <summary>
        /// Initializes a new instance of the Form2 class and starts ASIO playback.
        /// </summary>
        public Form2()
        {         
            InitializeComponent();
            this.asioStartPlaying();
        }

        /// <summary>
        /// Starts playing audio using the ASIO driver, initializes recording and playback,
        /// and sets up a file stream for direct recording.
        /// </summary>
        private void asioStartPlaying()
        {
            BufferedWaveProvider wavprov = new BufferedWaveProvider(new WaveFormat(samplingRate, bitDepth, 1));
            asioOut.AudioAvailable += new EventHandler<AsioAudioAvailableEventArgs>(asio_DataAvailable);
            asioOut.InitRecordAndPlayback(wavprov, 1, 0);
            asioOut.Play();
        }

        /// <summary>
        /// Stops the audio recording and resets the recording button.
        /// </summary>
        private void stopRecording()
        {
            waveSource.StopRecording();

            button1.Text = "Record";
            isRecording = false;
        }

        /// <summary>
        /// Starts recording audio from the selected input device and saves it as a WAV file.
        /// </summary>
        private void startRecording()
        {
            waveSource = new WaveIn();
            waveSource.DeviceNumber = formToSelectDevice.SelectedDeviceIndex;
            waveSource.WaveFormat = new WaveFormat(samplingRate, bitDepth, 1);
            waveSource.BufferMilliseconds = 100;
            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);

            waveRecordFile = txtFileName.Text;

            System.IO.Directory.CreateDirectory(waveRecordFolder);

            waveRecordPath = waveRecordFolder + @"\" + waveRecordFile + ".wav";

            waveFile = new WaveFileWriter(waveRecordPath, waveSource.WaveFormat);

            waveSource.StartRecording();

            button1.Text =  "Stop";
            isRecording = true;
        }
        void waveSource_DataAvailable(object sender, WaveInEventArgs e){}

        /// <summary>
        /// Handles the audio processing for input/output audio buffers.
        /// Generates different waveforms based on the selected signal type,
        /// processes the audio data, performs FFT analysis, and updates the spectrum analyzer.
        /// </summary>
        void asio_DataAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            try
            {
                // Get the sample type and the number of samples from the event args.
                AsioSampleType sampleType = e.AsioSampleType;
                int numberOfSamples = e.SamplesPerBuffer;
                
                System.Diagnostics.Debug.Print("wave");

                // Process each input buffer.
                for (int i = 0; i < e.InputBuffers.Length; i++)
                    {
                    // Copy input buffer data to a byte array and convert to float.
                    Marshal.Copy(e.InputBuffers[i], inputBufferByte, 0, numberOfSamples * 4);
                    this.ConvertBytesToFloat(inputBufferByte, inputBufferFloat, numberOfSamples);
                    this.ConvertBytesToFloat(inputBufferByte, realTimeWaveform, numberOfSamples);

                    // Generate the appropriate waveform based on the selected index.
                    switch (signalIndex)
                {
                    case 0:
                        generator.GenerateWhiteNoise(outputBufferFloat, numberOfSamples);
                        isPeriodic = false;
                        waveform = "White Noise";
                        break;
                    case 1:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Sin;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 1;
                        var offset3 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset3.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Sine Wave";
                        break;
                    case 2:
                        generator.GenerateSineSweep(outputBufferFloat, numberOfSamples, samplingRate, sweepDuration, sweepLowFreq, sweepHighFreq, false);
                        isPeriodic = false;
                        waveform = "Sine Sweep";
                        break;
                    case 3:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Triangle;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 0.99f;
                        var offset0 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset0.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Triangle";
                        break;
                    case 4:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Square;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 0.99f;
                        var offset1 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset1.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Square";
                        break;
                    case 5:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.SawTooth;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 0.99f;
                        var offset2 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset2.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Sawtooth";
                        break;
                    case 6:
                        generator.GenerateSineWaveform(outputBufferFloat, numberOfSamples, sineFrequencyHz, samplingRate, SineWaveType.ClippedSine);
                        isPeriodic = false;
                        waveform = "Clipped Sine";
                        break;
                    case 7:
                        generator.GenerateSineWaveform(outputBufferFloat, numberOfSamples, sineFrequencyHz, samplingRate, SineWaveType.HalfWaveSine);
                        isPeriodic = false;
                        waveform = "Half-Wave Rectified Sine";
                        break;
                    case 8:
                        generator.GenerateSineWaveform(outputBufferFloat, numberOfSamples, sineFrequencyHz, samplingRate, SineWaveType.FullWaveSine);
                        isPeriodic = false;
                        waveform = "Full-Wave Rectified Sine";
                        break;
                    default:
                        generator.GenerateWhiteNoise(outputBufferFloat, numberOfSamples);
                        isPeriodic = false;
                        waveform = "White Noise";
                        break;
                }
                    // Convert the output buffer from float to byte.
                    this.ConvertFloatToBytes(outputBufferFloat, outputBufferByte, numberOfSamples);
                    // Copy the output buffer data back to the output buffers.
                    Marshal.Copy(outputBufferByte, 0, e.OutputBuffers[i], numberOfSamples * 4);
                    }
                // Write samples to the wave file if recording.
                if (isRecording)
                    waveFile.WriteSamples(inputBufferFloat, 0, numberOfSamples);

                // Initialize the FFT processor and perform FFT on input and output buffers.
                fftProcessor.Initialize(fftSize, fftFrameLength, fftWindowType);
                fftProcessor.FFT(inputBufferFloat, 0, fftOutput);
                fftProcessor.FFT(outputBufferFloat, 0, fftInput);

                // Prepare the FFT output data for visualization.
                for (int i = 0; i < fftSize / 2 + 1; i++)
                {
                    cFFTOutput[i].real = fftOutput[i].x;
                    cFFTOutput[i].imag = fftOutput[i].y;
                    outputPoints[i].X = i;
                    outputPoints[i].Y = Convert.ToSingle(fftOutput[i].LogMagnitude());
                    shiftedIndexes[i] = (float)Math.Log(i, 2);
                }

                // Update spectrum analyzer and invalidate to redraw.
                isPeriodic = false; // Set harmonics turned off
                spectrumAnalyzer1.update(fftSize, samplingRate, outputPoints, shiftedIndexes, realTimeWaveform, isPeriodic, cFFTOutput, numberOfSamples);
                spectrumAnalyzer1.Invalidate();
                e.WrittenToOutputBuffers = true;

                // Flush the wave file if recording.
                if (isRecording)
                    waveFile.Flush();
            }
            catch (Exception)
            {
                // Optionally log the exception or handle it as needed.
                throw; // Re-throw the exception for further handling
            }
               
        }

        /// <summary>
        /// Applies distortion to the audio waveform using a combination of Chebyshev polynomials.
        /// The distortion is applied only when the isDistorted flag is true.
        /// </summary>
        /// <param name="buffer">The audio buffer to be distorted.</param>
        /// <param name="index">The index determining the distortion profile.</param>
        /// <param name="isDistorted">A flag indicating whether distortion is applied.</param>
        /// <param name="gain">The gain applied to the distorted signal.</param>
        public void DistortWaveform(float[] buffer, int index, bool isDistorted, float gain)
        {
            // CENTER - HOT WIRED - TWEED // 
            if (isDistorted)
            {
                switch (index)
                {
                    case 0:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.141f * ChebyshevPolynomial(buffer[i], 1) + 0.05f * ChebyshevPolynomial(buffer[i], 2) + -0.079f * ChebyshevPolynomial(buffer[i], 3) + -0.013f * ChebyshevPolynomial(buffer[i], 4) + 0.028f * ChebyshevPolynomial(buffer[i], 5) + -0.013f * ChebyshevPolynomial(buffer[i], 7) ;
                        }
                        break;
                    case 1:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.2f * ChebyshevPolynomial(buffer[i], 1) + -0.028f * ChebyshevPolynomial(buffer[i], 3);
                        }
                        break;
                    case 2:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.224f * ChebyshevPolynomial(buffer[i], 1) + -0.032f * ChebyshevPolynomial(buffer[i], 3) + 0.011f * ChebyshevPolynomial(buffer[i], 5);
                        }
                        break;
                    case 3:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + -0.035f * ChebyshevPolynomial(buffer[i], 3) + 0.013f * ChebyshevPolynomial(buffer[i], 5);
                        }
                        break;
                    case 4:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.032f * ChebyshevPolynomial(buffer[i], 2) + -0.035f * ChebyshevPolynomial(buffer[i], 3) + -0.014f * ChebyshevPolynomial(buffer[i], 4) + 0.011f * ChebyshevPolynomial(buffer[i], 5) + 0.01f * ChebyshevPolynomial(buffer[i], 6);
                        }
                        break;
                    case 5:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.035f * ChebyshevPolynomial(buffer[i], 2) + -0.04f * ChebyshevPolynomial(buffer[i], 3) + -0.018f * ChebyshevPolynomial(buffer[i], 4) + 0.011f * ChebyshevPolynomial(buffer[i], 5) + 0.013f * ChebyshevPolynomial(buffer[i], 6) + -0.01f * ChebyshevPolynomial(buffer[i], 8);
                        }
                        break;
                    case 6:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.032f * ChebyshevPolynomial(buffer[i], 2) + -0.04f * ChebyshevPolynomial(buffer[i], 3) + -0.018f * ChebyshevPolynomial(buffer[i], 4) + 0.013f * ChebyshevPolynomial(buffer[i], 5) + 0.013f * ChebyshevPolynomial(buffer[i], 6) + -0.011f * ChebyshevPolynomial(buffer[i], 8)
                                 + 0.01f * ChebyshevPolynomial(buffer[i], 10);
                        }
                        break;
                    case 7:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.028f * ChebyshevPolynomial(buffer[i], 2) + -0.045f * ChebyshevPolynomial(buffer[i], 3) + -0.016f * ChebyshevPolynomial(buffer[i], 4) + 0.014f * ChebyshevPolynomial(buffer[i], 5) + 0.011f * ChebyshevPolynomial(buffer[i], 6) + -0.011f * ChebyshevPolynomial(buffer[i], 8)
                                 + 0.01f * ChebyshevPolynomial(buffer[i], 10);
                        }
                        break;
                    case 8:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.025f * ChebyshevPolynomial(buffer[i], 2) + -0.045f * ChebyshevPolynomial(buffer[i], 3) + -0.014f * ChebyshevPolynomial(buffer[i], 4) + 0.016f * ChebyshevPolynomial(buffer[i], 5) + 0.011f * ChebyshevPolynomial(buffer[i], 6) + -0.01f * ChebyshevPolynomial(buffer[i], 8)
                                 + 0.01f * ChebyshevPolynomial(buffer[i], 10);
                        }
                        break;
                    case 9:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.022f * ChebyshevPolynomial(buffer[i], 2) + -0.045f * ChebyshevPolynomial(buffer[i], 3) + -0.013f * ChebyshevPolynomial(buffer[i], 4) + 0.016f * ChebyshevPolynomial(buffer[i], 5) + 0.01f * ChebyshevPolynomial(buffer[i], 6) + -0.01f * ChebyshevPolynomial(buffer[i], 8)
                                 + 0.01f * ChebyshevPolynomial(buffer[i], 10);
                        }
                        break;
                    case 10:
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0.251f * ChebyshevPolynomial(buffer[i], 1) + 0.022f * ChebyshevPolynomial(buffer[i], 2) + -0.045f * ChebyshevPolynomial(buffer[i], 3) + -0.013f * ChebyshevPolynomial(buffer[i], 4) + 0.016f * ChebyshevPolynomial(buffer[i], 5) + 0.01f * ChebyshevPolynomial(buffer[i], 6) + -0.01f * ChebyshevPolynomial(buffer[i], 8)
                                 + 0.01f * ChebyshevPolynomial(buffer[i], 10);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
                return;

        }

        /// <summary>
        /// Recursively computes the value of the Chebyshev polynomial of the first kind for a given input and degree.
        /// </summary>
        /// <param name="input">The input value for which the Chebyshev polynomial is evaluated.</param>
        /// <param name="degree">The degree of the Chebyshev polynomial.</param>
        /// <returns>The computed value of the Chebyshev polynomial for the given input and degree.</returns>
        public float ChebyshevPolynomial(float input, int degree)
        {
            if (degree == 0)
            {
                return 1;
            }
            if (degree == 1)
            {
                return input;
            }

            // Recursive formula: Tn(x) = 2x * Tn-1(x) - Tn-2(x)
            return 2 * input * ChebyshevPolynomial(input, degree - 1) - ChebyshevPolynomial(input, degree - 2);
        }

        /// <summary>
        /// Converts an array of bytes to an array of floating-point values.
        /// </summary>
        /// <param name="input">The input byte array containing the sample data.</param>
        /// <param name="output">The output float array where the converted values will be stored.</param>
        /// <param name="numberOfSamples">The number of samples to convert from the input byte array.</param>
        void ConvertBytesToFloat(byte[] input, float[] output, int numberOfSamples)
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
        /// Converts an array of floating-point values to an array of bytes.
        /// </summary>
        /// <param name="input">The input float array containing the sample data to convert.</param>
        /// <param name="output">The output byte array where the converted byte values will be stored.</param>
        /// <param name="numberOfSamples">The number of samples to convert from the input float array.</param>
        void ConvertFloatToBytes(float[] input, byte[] output, int numberOfSamples)
        {
            float k = 2147483647.0f;

            for (int i = 0, j = 0; i < numberOfSamples; i++)
            {
                int tempInt;

                if (k * input[i] >= 2147483647)
                    tempInt = 2147483647;
                else
                if (k * input[i] < -2147483647)
                    tempInt = -2147483647;

                else
                    tempInt = Convert.ToInt32(k * input[i]);

                byte[] byteArray = BitConverter.GetBytes(tempInt);

                output[j++] = byteArray[0];
                output[j++] = byteArray[1];
                output[j++] = byteArray[2];
                output[j++] = byteArray[3];
            }
        }

        /// <summary>
        /// Handles the event when the selected waveform in the combo box changes.
        /// This method updates the enabled state of various controls based on the selected waveform type.
        /// </summary>
        private void cmbWaveforms_SelectedIndexChanged(object sender, EventArgs e)
        {
            signalIndex = cmbWaveforms.SelectedIndex;

            switch (signalIndex)
            {
                case 0:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = false;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
                case 1:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = true;
                    checkBox1.Enabled = true;
                    break;
                case 2:
                    numericUpDown4.Enabled = true;
                    numericUpDown3.Enabled = true;
                    numericUpDown2.Enabled = true;
                    numericUpDown1.Enabled = false;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
                case 3:
                    numericUpDown4.Enabled = true;
                    numericUpDown3.Enabled = true;
                    numericUpDown2.Enabled = true;
                    numericUpDown1.Enabled = false;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
                case 4:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = true;
                    checkBox1.Enabled = true;
                    break;
                case 5:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = true;
                    checkBox1.Enabled = true;
                    break;
                case 6:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = true;
                    checkBox1.Enabled = true;
                    break;
                case 7:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
                case 8:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
                case 9:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = true;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
                default:
                    numericUpDown4.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = false;
                    numMultidrive.Enabled = false;
                    checkBox1.Enabled = false;
                    break;
            }
            spectrumAnalyzer1.SpectrumFirstHarmonicUpdate();
            spectrumAnalyzer1.update();
        }

        /// <summary>
        /// Handles the event when the value of the frequency numeric up-down control changes.
        /// Updates the frequency of the sine wave based on the new value.
        /// </summary>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            sineFrequencyHz = Convert.ToSingle(numericUpDown1.Value);
        }

        /// <summary>
        /// Handles the event when the value of the low frequency numeric up-down control changes.
        /// Updates the lower frequency limit for the sine sweep based on the new value.
        /// </summary>
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            sweepLowFreq = Convert.ToInt32(numericUpDown2.Value);
        }

        /// <summary>
        /// Handles the event when the value of the high frequency numeric up-down control changes.
        /// Updates the upper frequency limit for the sine sweep based on the new value.
        /// </summary>
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            sweepHighFreq = Convert.ToInt32(numericUpDown3.Value);
        }

        /// <summary>
        /// Handles the event when the value of the duration numeric up-down control changes.
        /// Updates the duration of the sine sweep based on the new value.
        /// </summary>
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            sweepDuration = Convert.ToInt32(numericUpDown4.Value);
        }

        /// <summary>
        /// Handles the event when the selected window type in the combo box changes.
        /// Updates the FFT window type based on the selected item.
        /// </summary>
        private void cmbWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbWindow.SelectedIndex)
            {
                case 0:
                    fftWindowType = WindowTypeEnum.eHammingWindow;
                    break;
                case 1:
                    fftWindowType = WindowTypeEnum.eHanningWindow;
                    break;
                case 2:
                    fftWindowType = WindowTypeEnum.eBlackmanWindow;
                    break;
                case 3:
                    fftWindowType = WindowTypeEnum.eBlackmanHarrisWindow;
                    break;
                case 4:
                    fftWindowType = WindowTypeEnum.eRectangular;
                    break;
                default:
                    fftWindowType = WindowTypeEnum.eBlackmanHarrisWindow;
                    break;
            }
        }

        /// <summary>
        /// Handles the click event for button1. 
        /// Starts or stops recording audio based on the current recording state.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (isRecording)
                stopRecording();
            else
                startRecording();
        }

        /// <summary>
        /// Handles the value change event for the multidrive control.
        /// Updates the drive index based on the new value from the control.
        /// </summary>
        private void numMultidrive_ValueChanged(object sender, EventArgs e)
        {
            driveIndex = Convert.ToInt32(numMultidrive.Value);
        }

        /// <summary>
        /// Handles the checked change event for the drive enable checkbox.
        /// Toggles the drive enabled state based on the checkbox's current state.
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (driveEnabled == false)
                driveEnabled = true;
            else
                driveEnabled = false;
        }
    }
}
