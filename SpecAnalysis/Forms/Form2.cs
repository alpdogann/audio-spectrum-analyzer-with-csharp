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
        private static int fftSize = 2048;
        private static int fftFrameLength = 1024;
        private static int samplingRate = 44100;

        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string relativePathForDirectRecording = "..\\test";
        private string pathForDirectRecording = Path.GetFullPath(desktopPath + relativePathForDirectRecording);
        FileStream fs = null;

        // Select Audio Device

        static FormSelectDevice formToSelectDevice = new FormSelectDevice();
        DialogResult result = formToSelectDevice.ShowDialog();     
        AsioOut asioOut = new AsioOut(formToSelectDevice.SelectedDeviceIndex);
        private WaveIn waveSource = null;
        private WaveFileWriter waveFile = null;
        SignalGenerator generator = new SignalGenerator();
        NAudio.Wave.SampleProviders.SignalGenerator alternativeGenerator = new NAudio.Wave.SampleProviders.SignalGenerator(samplingRate, 1);
        

        private byte[] inputBufferByte = new byte[32768];
        private byte[] outputBufferByte = new byte[32768];
        private float[] inputBufferFloat = new float[32768]; // 32768
        private float[] outputBufferFloat = new float[32768];
        private float[] realTimeWaveform = new float[32768];

        private static string waveRecordFolder = Path.GetFullPath(desktopPath + "..\\Envelope Test");
        private string waveRecordFile = "TEST_NAME";
        private string waveRecordPath = "";
        private string waveform = "";
        private int sweepLowFreq = 20;
        private int sweepHighFreq = 20000;
        private int sweepDuration = 10;
        private float sineFrequencyHz = 200;
        private int signalIndex = 0;
        private bool isPeriodic = false;
        private int bitDepth = 16;

        private WindowTypeEnum fftWindowType = WindowTypeEnum.eBlackmanHarrisWindow;
        private float[] shiftedIndexes = new float[fftSize / 2 +1];
        private ComplexNumber[] fftOutput = new ComplexNumber[fftSize];
        private ComplexNumber[] fftInput = new ComplexNumber[fftSize];
        private ComplexNumber[] cFFTOutput = new ComplexNumber[fftSize];
        private FFTProcessor fftProcessor = new FFTProcessor(fftSize);
        private PointF[] outputPoints = new PointF[fftSize / 2 + 1];
        private bool isRecording = false;
        private int driveIndex = 0;
        private bool driveEnabled = false;
        private float distortionGain = 0.0f;

        public Form2()
        {         
            InitializeComponent();
            this.asioStartPlaying();
        }
        private void asioStartPlaying()
        {
            BufferedWaveProvider wavprov = new BufferedWaveProvider(new WaveFormat(samplingRate, bitDepth, 1));
            asioOut.AudioAvailable += new EventHandler<AsioAudioAvailableEventArgs>(asio_DataAvailable);
            asioOut.InitRecordAndPlayback(wavprov, 1, 0);
            asioOut.Play();
            
            fs = new FileStream(pathForDirectRecording, FileMode.Create, FileAccess.Write);
        }
        private void stopRecording()
        {
            waveSource.StopRecording();

            button1.Text = "Record";
            isRecording = false;
        }
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
        void asio_DataAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            try
            {
                AsioSampleType sampleType = e.AsioSampleType;
                int numberOfSamples = e.SamplesPerBuffer;
                
                System.Diagnostics.Debug.Print("wave");

                    for (int i = 0; i < e.InputBuffers.Length; i++)
                    {
                        Marshal.Copy(e.InputBuffers[i], inputBufferByte, 0, numberOfSamples * 4);
                        this.ConvertBytesToFloat(inputBufferByte, inputBufferFloat, numberOfSamples);
                        this.ConvertBytesToFloat(inputBufferByte, realTimeWaveform, numberOfSamples);
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
                        generator.GenerateSineSweep(outputBufferFloat, numberOfSamples, samplingRate, sweepDuration, sweepLowFreq, sweepHighFreq, true);
                        isPeriodic = false;
                        waveform = "Sine Sweep";
                        break;
                    case 4:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Triangle;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 0.99f;
                        var offset0 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset0.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Triangle";
                        break;
                    case 5:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Square;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 0.99f;
                        var offset1 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset1.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Square";
                        break;
                    case 6:
                        alternativeGenerator.Type = NAudio.Wave.SampleProviders.SignalGeneratorType.SawTooth;
                        alternativeGenerator.Frequency = sineFrequencyHz;
                        alternativeGenerator.Gain = 0.99f;
                        var offset2 = new NAudio.Wave.SampleProviders.OffsetSampleProvider(alternativeGenerator);
                        offset2.Read(outputBufferFloat, 0, numberOfSamples);
                        DistortWaveform(outputBufferFloat, driveIndex, driveEnabled, distortionGain);
                        isPeriodic = true;
                        waveform = "Sawtooth";
                        break;
                    case 7:
                        generator.GenerateSineWaveform(outputBufferFloat, numberOfSamples, sineFrequencyHz, samplingRate, SineWaveType.ClippedSine);
                        isPeriodic = false;
                        waveform = "Clipped Sine";
                        break;
                    case 8:
                        generator.GenerateSineWaveform(outputBufferFloat, numberOfSamples, sineFrequencyHz, samplingRate, SineWaveType.HalfWaveSine);
                        isPeriodic = false;
                        waveform = "Half-Wave Rectified Sine";
                        break;
                    case 9:
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
                        this.ConvertFloatToBytes(outputBufferFloat, outputBufferByte, numberOfSamples);
                        Marshal.Copy(outputBufferByte, 0, e.OutputBuffers[i], numberOfSamples * 4);
                    }
                fs.Write(inputBufferByte, 0, numberOfSamples * 4);

                if (isRecording)
                    waveFile.WriteSamples(inputBufferFloat, 0, numberOfSamples);

                fftProcessor.Initialize(fftSize, fftFrameLength, fftWindowType);
                fftProcessor.FFT(inputBufferFloat, 0, fftOutput);
                fftProcessor.FFT(outputBufferFloat, 0, fftInput);

                for (int i = 0; i < fftSize / 2 + 1; i++)
                {
                    cFFTOutput[i].real = fftOutput[i].x;
                    cFFTOutput[i].imag = fftOutput[i].y;
                    outputPoints[i].X = i;
                    outputPoints[i].Y = Convert.ToSingle(fftOutput[i].LogMagnitude());
                    shiftedIndexes[i] = (float)Math.Log(i, 2);
                }

                isPeriodic = false; // harmonics turned off, true for harmonics
                spectrumAnalyzer1.update(fftSize, samplingRate, outputPoints, shiftedIndexes, realTimeWaveform, isPeriodic, cFFTOutput, numberOfSamples);
                spectrumAnalyzer1.Invalidate();
                e.WrittenToOutputBuffers = true;
                fs.Flush();

                if(isRecording)
                    waveFile.Flush();
            }
            catch (Exception)
            {
                 throw;
            }
               
        }
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

        public float ChebyshevPolynomial(float input, int degree)
        {
            float output = 1;
            switch (degree)
            {
                case 0:
                    output = 1;
                    break;
                case 1:
                    output = input;
                    break;
                case 2:
                    output = Convert.ToSingle(2.0f * Math.Pow(input, 2)) - 1.0f;
                    break;
                case 3:
                    output = Convert.ToSingle(4.0f * Math.Pow(input, 3)) - (3.0f * input);
                    break;
                case 4:
                    output = Convert.ToSingle(8.0f * Math.Pow(input, 4)) - Convert.ToSingle(8.0f * Math.Pow(input, 2)) + 1;
                    break;
                case 5:
                    output = Convert.ToSingle(16.0f * Math.Pow(input, 5)) - Convert.ToSingle(20.0f * Math.Pow(input, 3)) + (5.0f * input);
                    break;
                case 6:
                    output = Convert.ToSingle(32.0f * Math.Pow(input, 6)) - Convert.ToSingle(48.0f * Math.Pow(input, 4)) + Convert.ToSingle(18.0f * Math.Pow(input, 2)) - 1;
                    break;
                case 7:
                    output = Convert.ToSingle(64.0f * Math.Pow(input, 7)) - Convert.ToSingle(112.0f * Math.Pow(input, 5)) + Convert.ToSingle(56.0f * Math.Pow(input, 3)) - (7.0f * input);
                    break;
                case 8:
                    output = Convert.ToSingle(128.0f * Math.Pow(input, 8)) - Convert.ToSingle(256.0f * Math.Pow(input, 6)) + Convert.ToSingle(160.0f * Math.Pow(input, 4)) - Convert.ToSingle(32.0f * Math.Pow(input, 2)) + 1;
                    break;
                case 9:
                    output = Convert.ToSingle(256.0f * Math.Pow(input, 9)) - Convert.ToSingle(576.0f * Math.Pow(input, 7)) + Convert.ToSingle(432.0f * Math.Pow(input, 5)) - Convert.ToSingle(120.0f * Math.Pow(input, 3)) + (9.0f * input);
                    break;
                case 10:
                    output = Convert.ToSingle(512.0f * Math.Pow(input, 10)) - Convert.ToSingle(1280.0f * Math.Pow(input, 8)) + Convert.ToSingle(1120.0f * Math.Pow(input, 6)) - Convert.ToSingle(400.0f * Math.Pow(input, 4)) + Convert.ToSingle(50.0f * Math.Pow(input, 2)) - 1;
                    break;
                case 11:
                    output = Convert.ToSingle(1024.0f * Math.Pow(input, 11)) - Convert.ToSingle(2816.0f * Math.Pow(input, 9)) + Convert.ToSingle(2816.0f * Math.Pow(input, 7)) - Convert.ToSingle(1232.0f * Math.Pow(input, 5)) + Convert.ToSingle(220.0f * Math.Pow(input, 3)) - (11.0f * input);
                    break;
                case 12:
                    output = Convert.ToSingle(2048.0f * Math.Pow(input, 12)) - Convert.ToSingle(6144.0f * Math.Pow(input, 10)) + Convert.ToSingle(6912.0f * Math.Pow(input, 8)) - Convert.ToSingle(3584.0f * Math.Pow(input, 6)) + Convert.ToSingle(840.0f * Math.Pow(input, 4)) - 
                        Convert.ToSingle(72.0f * Math.Pow(input, 2)) + 1;
                    break;
                case 13:
                    output = Convert.ToSingle(4096.0f * Math.Pow(input, 13)) - Convert.ToSingle(13312.0f * Math.Pow(input, 11)) + Convert.ToSingle(16640.0f * Math.Pow(input, 9)) - Convert.ToSingle(9984.0f * Math.Pow(input, 7)) + Convert.ToSingle(2912.0f * Math.Pow(input, 5)) - 
                        Convert.ToSingle(364.0f * Math.Pow(input, 3)) + (13.0f * input);
                    break;
                case 14:
                    output = Convert.ToSingle(8192.0f * Math.Pow(input, 14)) - Convert.ToSingle(28672.0f * Math.Pow(input, 12)) + Convert.ToSingle(39424.0f * Math.Pow(input, 10)) - Convert.ToSingle(26880.0f * Math.Pow(input, 8)) + Convert.ToSingle(9408.0f * Math.Pow(input, 6)) -
                        Convert.ToSingle(1568.0f * Math.Pow(input, 4)) + Convert.ToSingle(98.0f * Math.Pow(input, 2)) - 1;
                    break;
                case 15:
                    output = Convert.ToSingle(16384.0f * Math.Pow(input, 15)) - Convert.ToSingle(61440.0f * Math.Pow(input, 13)) + Convert.ToSingle(92160.0f * Math.Pow(input, 11)) - Convert.ToSingle(70400.0f * Math.Pow(input, 9)) + Convert.ToSingle(28800.0f * Math.Pow(input, 7)) -
                        Convert.ToSingle(6048.0f * Math.Pow(input, 5)) + Convert.ToSingle(560.0f * Math.Pow(input, 3)) - (15.0f * input);
                    break;
                case 16:
                    output = Convert.ToSingle(32768f * Math.Pow(input, 16)) - Convert.ToSingle(131072f * Math.Pow(input, 14)) + Convert.ToSingle(212992f * Math.Pow(input, 12)) - Convert.ToSingle(180224f * Math.Pow(input, 10)) + Convert.ToSingle(84480f * Math.Pow(input, 8)) - 
                        Convert.ToSingle(21504f * Math.Pow(input, 6)) + Convert.ToSingle(2688f * Math.Pow(input, 4)) - Convert.ToSingle(128.0f * Math.Pow(input, 2)) + 1;
                    break;
                case 17:
                    output = Convert.ToSingle(65536f * Math.Pow(input, 17)) - Convert.ToSingle(278528f * Math.Pow(input, 15)) + Convert.ToSingle(487424f * Math.Pow(input, 13)) - Convert.ToSingle(452608f * Math.Pow(input, 11)) + Convert.ToSingle(239360f * Math.Pow(input, 9)) -
                        Convert.ToSingle(71808f * Math.Pow(input, 7)) + Convert.ToSingle(11424f * Math.Pow(input, 5)) - Convert.ToSingle(816f * Math.Pow(input, 3)) + (17f * input);
                    break;
                case 18:
                    output = Convert.ToSingle(131072f * Math.Pow(input, 18)) - Convert.ToSingle(589824f * Math.Pow(input, 16)) + Convert.ToSingle(1105920f * Math.Pow(input, 14)) - Convert.ToSingle(1118208f * Math.Pow(input, 12)) + Convert.ToSingle(658944f * Math.Pow(input, 10)) - 
                        Convert.ToSingle(228096f * Math.Pow(input, 8)) + Convert.ToSingle(44352f * Math.Pow(input, 6)) - Convert.ToSingle(4320f * Math.Pow(input, 4)) + Convert.ToSingle(162f * Math.Pow(input, 2)) - 1;
                    break;
                case 19:
                    output = Convert.ToSingle(262144f * Math.Pow(input, 19)) - Convert.ToSingle(1245184f * Math.Pow(input, 17)) + Convert.ToSingle(2490368f * Math.Pow(input, 15)) - Convert.ToSingle(2723840f * Math.Pow(input, 13)) + Convert.ToSingle(1770496f * Math.Pow(input, 11)) - 
                        Convert.ToSingle(695552f * Math.Pow(input, 9)) + Convert.ToSingle(160512f * Math.Pow(input, 7)) - Convert.ToSingle(20064f * Math.Pow(input, 5)) + Convert.ToSingle(1140f * Math.Pow(input, 3)) - Convert.ToSingle(19f * input);
                    break;
                case 20:
                    output = Convert.ToSingle(524288f * Math.Pow(input, 20)) - Convert.ToSingle(2621440f * Math.Pow(input, 18)) + Convert.ToSingle(5570560f * Math.Pow(input, 16)) - Convert.ToSingle(6553600f * Math.Pow(input, 14)) + Convert.ToSingle(4659200f * Math.Pow(input, 12)) - 
                        Convert.ToSingle(205004f * Math.Pow(input, 10)) + Convert.ToSingle(549120f * Math.Pow(input, 8)) - Convert.ToSingle(84480f * Math.Pow(input, 6)) + Convert.ToSingle(6600f * Math.Pow(input, 4)) - Convert.ToSingle(200f * Math.Pow(input, 2)) + 1;
                    break;
                case 21:
                    output = Convert.ToSingle(1048576 * Math.Pow(input, 21)) - Convert.ToSingle(5505024 * Math.Pow(input, 19)) + Convert.ToSingle(12386304 * Math.Pow(input, 17)) - Convert.ToSingle(15597568 * Math.Pow(input, 15)) + Convert.ToSingle(12042240 * Math.Pow(input, 13)) - 
                        Convert.ToSingle(5870592 * Math.Pow(input, 11)) + Convert.ToSingle(1793792 * Math.Pow(input, 9)) - Convert.ToSingle(329472 * Math.Pow(input, 7)) + Convert.ToSingle(33264 * Math.Pow(input, 5)) - Convert.ToSingle(1540 * Math.Pow(input, 3)) + Convert.ToSingle(21 * input);
                    break;
                case 22:
                    output = Convert.ToSingle(2097152 * Math.Pow(input, 22)) - Convert.ToSingle(11534336 * Math.Pow(input, 20)) + Convert.ToSingle(27394048 * Math.Pow(input, 18)) - Convert.ToSingle(36765696 * Math.Pow(input, 16)) + Convert.ToSingle(30638080 * Math.Pow(input, 14)) - 
                        Convert.ToSingle(16400384 * Math.Pow(input, 12)) + Convert.ToSingle(5637632 * Math.Pow(input, 10)) - Convert.ToSingle(1208064 * Math.Pow(input, 8)) + Convert.ToSingle(151008 * Math.Pow(input, 6)) - Convert.ToSingle(9680 * Math.Pow(input, 4)) + Convert.ToSingle(242 * Math.Pow(input, 2)) - 1;
                    break;
                case 23:
                    output = Convert.ToSingle(4194304 * Math.Pow(input, 23)) - Convert.ToSingle(24117248 * Math.Pow(input, 21)) + Convert.ToSingle(60293120 * Math.Pow(input, 19)) - Convert.ToSingle(85917696 * Math.Pow(input, 17)) + Convert.ToSingle(76873728 * Math.Pow(input, 15)) - 
                        Convert.ToSingle(44843008 * Math.Pow(input, 13)) + Convert.ToSingle(17145856 * Math.Pow(input, 11)) - Convert.ToSingle(4209920 * Math.Pow(input, 9)) + Convert.ToSingle(631488 * Math.Pow(input, 7)) - Convert.ToSingle(52624 * Math.Pow(input, 5)) + 
                        Convert.ToSingle(2024 * Math.Pow(input, 3)) - Convert.ToSingle(23 * input);
                    break;
                case 24:
                    output = Convert.ToSingle(8388608 * Math.Pow(input, 24)) - Convert.ToSingle(50331648 * Math.Pow(input, 22)) + Convert.ToSingle(132120576 * Math.Pow(input, 20)) - Convert.ToSingle(199229440 * Math.Pow(input, 18)) + Convert.ToSingle(190513152 * Math.Pow(input, 16)) -
                        Convert.ToSingle(120324096 * Math.Pow(input, 14)) + Convert.ToSingle(50692096 * Math.Pow(input, 12)) - Convert.ToSingle(14057472 * Math.Pow(input, 10)) + Convert.ToSingle(2471040 * Math.Pow(input, 8)) - Convert.ToSingle(256256 * Math.Pow(input, 6)) +
                        Convert.ToSingle(13728 * Math.Pow(input, 4)) - Convert.ToSingle(288 * Math.Pow(input, 2)) + 1;
                    break;
                case 25:
                    output = Convert.ToSingle(16777216 * Math.Pow(input, 25)) - Convert.ToSingle(104857600 * Math.Pow(input, 23)) + Convert.ToSingle(288358400 * Math.Pow(input, 21)) - Convert.ToSingle(458752000 * Math.Pow(input, 19)) + Convert.ToSingle(466944000 * Math.Pow(input, 17)) -
                        Convert.ToSingle(317521920 * Math.Pow(input, 15)) + Convert.ToSingle(146227200 * Math.Pow(input, 13)) - Convert.ToSingle(45260800 * Math.Pow(input, 11)) + Convert.ToSingle(9152000 * Math.Pow(input, 9)) - Convert.ToSingle(1144000 * Math.Pow(input, 7)) +
                        Convert.ToSingle(80080 * Math.Pow(input, 5)) - Convert.ToSingle(2600 * Math.Pow(input, 3)) + Convert.ToSingle(25 * input);
                    break;
                case 26:
                    output = Convert.ToSingle(33554432 * Math.Pow(input, 26)) - Convert.ToSingle(218103808 * Math.Pow(input, 24)) + Convert.ToSingle(627048448 * Math.Pow(input, 22)) - Convert.ToSingle(1049624576 * Math.Pow(input, 20)) + Convert.ToSingle(1133117440 * Math.Pow(input, 18)) -
                        Convert.ToSingle(825226992 * Math.Pow(input, 16)) + Convert.ToSingle(412778496 * Math.Pow(input, 14)) - Convert.ToSingle(141213696 * Math.Pow(input, 12)) + Convert.ToSingle(32361472 * Math.Pow(input, 10)) - Convert.ToSingle(4759040 * Math.Pow(input, 8)) +
                        Convert.ToSingle(416416 * Math.Pow(input, 6)) - Convert.ToSingle(18928 * Math.Pow(input, 4)) + Convert.ToSingle(338 * Math.Pow(input, 2)) - 1;
                    break;
                case 27:
                    output = Convert.ToSingle(67108864 * Math.Pow(input, 27)) - Convert.ToSingle(452984832 * Math.Pow(input, 25)) + Convert.ToSingle(1358954496 * Math.Pow(input, 23)) - Convert.ToSingle(2387607552 * Math.Pow(input, 21)) + Convert.ToSingle(2724986880 * Math.Pow(input, 19)) -
                        Convert.ToSingle(2118057984 * Math.Pow(input, 17)) + Convert.ToSingle(1143078912 * Math.Pow(input, 15)) - Convert.ToSingle(428654592 * Math.Pow(input, 13)) + Convert.ToSingle(109983744 * Math.Pow(input, 11)) - Convert.ToSingle(18670080 * Math.Pow(input, 9)) +
                        Convert.ToSingle(1976832 * Math.Pow(input, 7)) - Convert.ToSingle(117935 * Math.Pow(input, 5)) + Convert.ToSingle(3276 * Math.Pow(input, 3)) - Convert.ToSingle(27 * input);
                    break;
                case 28:
                    output = Convert.ToSingle(134217728 * Math.Pow(input, 28)) - Convert.ToSingle(939524096 * Math.Pow(input, 26)) + Convert.ToSingle(2936012800 * Math.Pow(input, 24)) - Convert.ToSingle(5402263552 * Math.Pow(input, 22)) + Convert.ToSingle(6499598336 * Math.Pow(input, 20)) -
                        Convert.ToSingle(5369233408 * Math.Pow(input, 18)) + Convert.ToSingle(3111714816 * Math.Pow(input, 16)) - Convert.ToSingle(1270087680 * Math.Pow(input, 14)) + Convert.ToSingle(361181184 * Math.Pow(input, 12)) - Convert.ToSingle(69701632 * Math.Pow(input, 10)) +
                        Convert.ToSingle(8712704 * Math.Pow(input, 8)) - Convert.ToSingle(652286 * Math.Pow(input, 6)) + Convert.ToSingle(25480 * Math.Pow(input, 4)) - Convert.ToSingle(392 * Math.Pow(input, 2)) + 1;
                    break;
                case 29:
                    output = Convert.ToSingle(2684355456 * Math.Pow(input, 29)) - Convert.ToSingle(1946157056 * Math.Pow(input, 27)) + Convert.ToSingle(6325010432 * Math.Pow(input, 25)) - Convert.ToSingle(12163481600 * Math.Pow(input, 23)) + Convert.ToSingle(15386804224 * Math.Pow(input, 21)) -
                        Convert.ToSingle(13463453696 * Math.Pow(input, 19)) + Convert.ToSingle(8341487616 * Math.Pow(input, 17)) - Convert.ToSingle(3683254272 * Math.Pow(input, 15)) + Convert.ToSingle(1151016960 * Math.Pow(input, 13)) - Convert.ToSingle(249387008 * Math.Pow(input, 11)) +
                        Convert.ToSingle(36095488 * Math.Pow(input, 9)) - Convert.ToSingle(3281404 * Math.Pow(input, 7)) + Convert.ToSingle(168895 * Math.Pow(input, 5)) - Convert.ToSingle(4060 * Math.Pow(input, 3)) + Convert.ToSingle(29 * input);
                    break;
                case 30:
                    output = Convert.ToSingle(536870912 * Math.Pow(input, 30)) - Convert.ToSingle(4026531840 * Math.Pow(input, 28)) + Convert.ToSingle(13589544960 * Math.Pow(input, 26)) - Convert.ToSingle(27262976000 * Math.Pow(input, 24)) + Convert.ToSingle(36175872000 * Math.Pow(input, 22)) -
                        Convert.ToSingle(33426505728 * Math.Pow(input, 20)) + Convert.ToSingle(22052208640 * Math.Pow(input, 18)) - Convert.ToSingle(10478223360 * Math.Pow(input, 16)) + Convert.ToSingle(3572121600 * Math.Pow(input, 14)) - Convert.ToSingle(859952200 * Math.Pow(input, 12)) +
                        Convert.ToSingle(141892608 * Math.Pow(input, 10)) - Convert.ToSingle(12575512 * Math.Pow(input, 8)) + Convert.ToSingle(990076 * Math.Pow(input, 6)) - Convert.ToSingle(33600 * Math.Pow(input, 4)) + Convert.ToSingle(450 * Math.Pow(input, 2)) - 1;
                    break;
                case 31:
                    output = Convert.ToSingle(1073741824 * Math.Pow(input, 31)) - Convert.ToSingle(83214991136 * Math.Pow(input, 29)) + Convert.ToSingle(29125246976 * Math.Pow(input, 27)) - Convert.ToSingle(60850962432 * Math.Pow(input, 25)) + Convert.ToSingle(84515225600 * Math.Pow(input, 23)) -
                        Convert.ToSingle(82239815680 * Math.Pow(input, 21)) + Convert.ToSingle(57567870976 * Math.Pow(input, 19)) - Convert.ToSingle(29297934336 * Math.Pow(input, 17)) + Convert.ToSingle(10827497472 * Math.Pow(input, 15)) - Convert.ToSingle(2870921360 * Math.Pow(input, 13)) +
                        Convert.ToSingle(53317224 * Math.Pow(input, 11)) - Convert.ToSingle(61246512 * Math.Pow(input, 9)) + Convert.ToSingle(5261556 * Math.Pow(input, 7)) - Convert.ToSingle(236095 * Math.Pow(input, 5)) + Convert.ToSingle(4960 * Math.Pow(input, 3)) - Convert.ToSingle(31 * input);
                    break;
                default:
                    break;
            }

            return output;
        }
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            sineFrequencyHz = Convert.ToSingle(numericUpDown1.Value);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            sweepLowFreq = Convert.ToInt32(numericUpDown2.Value);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            sweepHighFreq =  Convert.ToInt32(numericUpDown3.Value);
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            sweepDuration = Convert.ToInt32(numericUpDown4.Value);
        }

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

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRecording)
                stopRecording();
            else
                startRecording();
        }

        private void numMultidrive_ValueChanged(object sender, EventArgs e)
        {
            driveIndex = Convert.ToInt32(numMultidrive.Value);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (driveEnabled == false)
                driveEnabled = true;
            else
                driveEnabled = false;
        }
    }
}
