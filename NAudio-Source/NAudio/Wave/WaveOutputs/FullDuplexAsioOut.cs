using System;
using NAudio.Wave.Asio;
using System.Threading;

namespace NAudio.Wave
{
    public delegate void AsioProcessBuffersDelegate(float[][] inputBuffers, float[][] outputBuffers, int numberOfSamples);
    public delegate void AsioConvertIntPtr2FloatDelegate(IntPtr[] inputData, float[][] outputData, int numberOfSamples);
    public delegate void AsioConvertFloat2IntPtrDelegate(float[][] inputData, IntPtr[] outputData, int numberOfSamples);
              
    /// <summary>
    /// ASIO Full Duplex Input/Output. New implementation using an internal C# binding.
    /// 
    /// This implementation is only supporting Short16Bit and Float32Bit formats and is optimized 
    /// for 2 outputs channels .
    /// SampleRate is supported only if ASIODriver is supporting it (TODO: Add a resampler otherwhise).
    ///     
    /// This implementation is probably the first ASIODriver binding fully implemented in C#!
    /// 
    /// Original Contributor: Mark Heath 
    /// New Contributor to C# binding : Alexandre Mutel - email: alexandre_mutel at yahoo.fr
    /// </summary>
    public class FullDuplexAsioOut
    {
        private ASIODriverExt driver;
        private PlaybackState playbackState;
        private int bufferSizeInSamples;
        private string driverName;
        private float[][] inputBuffers;
        private float[][] outputBuffers;
        private SynchronizationContext syncContext;
        private int outputChannelOffset = 0;
        private int numberOfOutputChannels = 2;
        private int inputChannelOffset = 0;
        private int numberOfInputChannels = 2;
        private AsioProcessBuffersDelegate processBuffersFunction;
        private AsioConvertIntPtr2FloatDelegate convertIntPtr2FloatFunction;
        private AsioConvertFloat2IntPtrDelegate convertFloat2IntPtrFunction;

        #region static convertor functions

        static void AsioConvertFloat2Int16LSB(float[][] inputData, IntPtr[] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertFloat2Int16MSB(float[][] inputData, IntPtr[] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertFloat2Int24LSB(float[][] inputData, IntPtr[] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertFloat2Int24MSB(float[][] inputData, IntPtr[] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertFloat2Int32LSB(float[][] inputData, IntPtr[] outputData, int numberOfSamples)
        {
            int channels = inputData.Length;
            for (int i = 0; i < channels; i++)
            {
                float[] input = inputData[i];
                IntPtr output = outputData[i];

                unsafe
                {
                    fixed(float *inputP = input)
                    {
                        int samples = numberOfSamples;
                        const float k=8388607.0f;
	                    long temp;
	                    sbyte *outputPtr = (sbyte*)output.ToPointer();
	                    sbyte *p = (sbyte*)&temp;
                    
                        for (int j = 0; j < samples; j++)
                        {
                            temp=(int)(k*inputP[j]);
		                    if (temp>8388607)
			                    temp = 8388607;
		                    else
		                    if (temp<-8388607)
			                    temp = -8388607;
		                    temp<<=8;
		                    outputPtr[0] = p[0];
		                    outputPtr[1] = p[1];
		                    outputPtr[2] = p[2];
		                    outputPtr[3] = p[3];
		                    outputPtr += 4;
                        }
                    }
                }
            }
	    }

        static void AsioConvertFloat2Int32MSB(float[][] inputData, IntPtr[] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertInt16LSB2Float(IntPtr[] inputData, float[][] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertInt16MSB2Float(IntPtr[] inputData, float[][] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertInt24LSB2Float(IntPtr[] inputData, float[][] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertInt24MSB2Float(IntPtr[] inputData, float[][] outputData, int numberOfSamples)
        {
        }

        static void AsioConvertInt32LSB2Float(IntPtr[] inputData, float[][] outputData, int numberOfSamples)
        {
            int channels = inputData.Length;
            for (int i = 0; i < channels; i++)
            {
                IntPtr input = inputData[i];
                float[] output = outputData[i];

                unsafe
                {
                    fixed (float* outputP = output)
                    {
                        int samples = numberOfSamples;
                        void* inputP = input.ToPointer();
                        const float k = 1.0f / 8388608.0f;
                        long temp;
                        sbyte* inputPtr = (sbyte*)inputP;
                        sbyte* p = (sbyte*)&temp;

                        for(int j=0; j<samples; j++)
                        {
                            p[0] = inputPtr[0];
                            p[1] = inputPtr[1];
                            p[2] = inputPtr[2];
                            p[3] = inputPtr[3];
                            temp >>= 8;	// this will sign extend the 32 bit value
                            inputPtr += 4;
                            outputP[j] = k * ((float)temp);
                        }
          
                    }
                }
            }
        }

        static void AsioConvertInt32MSB2Float(IntPtr[] inputData, float[][] outputData, int numberOfSamples)
        {
        }

        #endregion

        /// <summary>
        /// Playback Stopped
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsioOut"/> class with the first 
        /// available ASIO Driver.
        /// </summary>
        public FullDuplexAsioOut()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsioOut"/> class with the driver name.
        /// </summary>
        /// <param name="driverName">Name of the device.</param>
        public FullDuplexAsioOut(String driverName)
        {
            this.syncContext = SynchronizationContext.Current;
            InitFromName(driverName);
        }

        /// <summary>
        /// Opens an ASIO output device
        /// </summary>
        /// <param name="driverIndex">Device number (zero based)</param>
        public FullDuplexAsioOut(int driverIndex)
        {
            this.syncContext = SynchronizationContext.Current; 
            String[] names = GetDriverNames();
            if (names.Length == 0)
            {
                throw new ArgumentException("There is no ASIO Driver installed on your system");
            }
            if (driverIndex < 0 || driverIndex > names.Length)
            {
                throw new ArgumentException(String.Format("Invalid device number. Must be in the range [0,{0}]", names.Length));
            }
            this.driverName = names[driverIndex];
            InitFromName(this.driverName);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="AsioOut"/> is reclaimed by garbage collection.
        /// </summary>
        ~FullDuplexAsioOut()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (driver != null)
            {
                if (playbackState != PlaybackState.Stopped)
                {
                    driver.Stop();
                }
                driver.ReleaseDriver();
                driver = null;
            }
        }

        /// <summary>
        /// Gets the names of the installed ASIO Driver.
        /// </summary>
        /// <returns>an array of driver names</returns>
        public static String[] GetDriverNames()
        {
            return ASIODriver.GetASIODriverNames();
        }

        /// <summary>
        /// Determines whether ASIO is supported.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if ASIO is supported; otherwise, <c>false</c>.
        /// </returns>
        public static bool isSupported()
        {
            return GetDriverNames().Length > 0;
        }

        /// <summary>
        /// Inits the driver from the asio driver name.
        /// </summary>
        /// <param name="driverName">Name of the driver.</param>
        private void InitFromName(String driverName)
        {
            // Get the basic driver
            ASIODriver basicDriver = ASIODriver.GetASIODriverByName(driverName);

            // Instantiate the extended driver
            driver = new ASIODriverExt(basicDriver);
        }

        /// <summary>
        /// Shows the control panel
        /// </summary>
        public void ShowControlPanel()
        {
            driver.ShowControlPanel();
        }

        /// <summary>
        /// Starts playback
        /// </summary>
        public void Play()
        {
            if (playbackState != PlaybackState.Playing)
            {
                playbackState = PlaybackState.Playing;
                driver.Start();
            }
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public void Stop()
        {
            playbackState = PlaybackState.Stopped;
            driver.Stop();
            //TODO: RaisePlaybackStopped(null);
        }

        /// <summary>
        /// Pauses playback
        /// </summary>
        public void Pause()
        {
            playbackState = PlaybackState.Paused;
            driver.Stop();
        }

        /// <summary>
        /// Initialises full duplex operation
        /// </summary>
        /// <param name="numberOfChannels">Number of channels to record and playback</param>
        /// <param name="sampleRate">Specify sample rate here to record and playback</param>
        public void Init(int numberOfChannels, int sampleRate, AsioProcessBuffersDelegate processFunction)
        {
            processBuffersFunction = processFunction;
            numberOfInputChannels = numberOfChannels;
            numberOfOutputChannels = numberOfChannels;

            AsioSampleType inputSampleType = driver.Capabilities.InputChannelInfos[0].type;
            AsioSampleType outputSampleType = driver.Capabilities.OutputChannelInfos[0].type;

            switch (inputSampleType)
            {
                case AsioSampleType.Int16LSB:
                    convertIntPtr2FloatFunction = AsioConvertInt16LSB2Float;
                    break;
                case AsioSampleType.Int16MSB:
                    convertIntPtr2FloatFunction = AsioConvertInt16MSB2Float;
                    break;
                case AsioSampleType.Int24LSB:
                    convertIntPtr2FloatFunction = AsioConvertInt24LSB2Float;
                    break;
                case AsioSampleType.Int24MSB:
                    convertIntPtr2FloatFunction = AsioConvertInt24MSB2Float;
                    break;
                case AsioSampleType.Int32LSB:
                    convertIntPtr2FloatFunction = AsioConvertInt32LSB2Float;
                    break;
                case AsioSampleType.Int32MSB:
                    convertIntPtr2FloatFunction = AsioConvertInt32MSB2Float;
                    break;
            }

            switch (outputSampleType)
            {
                case AsioSampleType.Int16LSB:
                    convertFloat2IntPtrFunction = AsioConvertFloat2Int16LSB;
                    break;
                case AsioSampleType.Int16MSB:
                    convertFloat2IntPtrFunction = AsioConvertFloat2Int16MSB;
                    break;
                case AsioSampleType.Int24LSB:
                    convertFloat2IntPtrFunction = AsioConvertFloat2Int24LSB;
                    break;
                case AsioSampleType.Int24MSB:
                    convertFloat2IntPtrFunction = AsioConvertFloat2Int24MSB;
                    break;
                case AsioSampleType.Int32LSB:
                    convertFloat2IntPtrFunction = AsioConvertFloat2Int32LSB;
                    break;
                case AsioSampleType.Int32MSB:
                    convertFloat2IntPtrFunction = AsioConvertFloat2Int32MSB;
                    break;
            }

            if (!driver.IsSampleRateSupported(sampleRate))
            {
                throw new ArgumentException("SampleRate is not supported");
            }

            if (driver.Capabilities.SampleRate != sampleRate)
            {
                driver.SetSampleRate(sampleRate);
            }

            // Plug the callback
            driver.FillBufferCallback = driver_BufferUpdate;

            // Used Prefered size of ASIO Buffer
            bufferSizeInSamples = driver.CreateBuffers(numberOfInputChannels, numberOfOutputChannels, false);

            // create float input/output buffers
            switch (numberOfInputChannels)
            {
                case 2:
                    {
                        float[] buffer1 = new float[bufferSizeInSamples];
                        float[] buffer2 = new float[bufferSizeInSamples];

                        inputBuffers = new float[][] { buffer1, buffer2 };
                    }
                    break;
            }

            switch (numberOfOutputChannels)
            {
                case 2:
                    {
                        float[] buffer1 = new float[bufferSizeInSamples];
                        float[] buffer2 = new float[bufferSizeInSamples];

                        outputBuffers = new float[][] { buffer1, buffer2 };
                    }
                    break;
            }

            driver.SetChannelOffset(outputChannelOffset, inputChannelOffset); // will throw an exception if channel offset is too high

        }

        /// <summary>
        /// driver buffer update callback to fill the wave buffer.
        /// </summary>
        /// <param name="inputChannels">The input channels.</param>
        /// <param name="outputChannels">The output channels.</param>
        public void driver_BufferUpdate(IntPtr[] inputChannels, IntPtr[] outputChannels)
        {
            convertIntPtr2FloatFunction(inputChannels, inputBuffers, bufferSizeInSamples);
            processBuffersFunction(inputBuffers, outputBuffers, bufferSizeInSamples);
            convertFloat2IntPtrFunction(outputBuffers, outputChannels, bufferSizeInSamples);
        }

        /// <summary>
        /// Playback State
        /// </summary>
        public PlaybackState PlaybackState
        {
            get { return playbackState; }
        }

        /// <summary>
        /// Driver Name
        /// </summary>
        public string DriverName
        {
            get { return this.driverName; }
        }

        /// <summary>
        /// The number of output channels we are currently using for playback
        /// (Must be less than or equal to DriverOutputChannelCount)
        /// </summary>
        public int NumberOfOutputChannels { get; private set; }

        /// <summary>
        /// The number of input channels we are currently recording from
        /// (Must be less than or equal to DriverInputChannelCount)
        /// </summary>
        public int NumberOfInputChannels { get; private set; }

        /// <summary>
        /// The maximum number of input channels this ASIO driver supports
        /// </summary>
        public int DriverInputChannelCount { get { return driver.Capabilities.NbInputChannels; } }
        
        /// <summary>
        /// The maximum number of output channels this ASIO driver supports
        /// </summary>
        public int DriverOutputChannelCount { get { return driver.Capabilities.NbOutputChannels; } }

        /// <summary>
        /// By default the first channel on the input WaveProvider is sent to the first ASIO output.
        /// This option sends it to the specified channel number.
        /// Warning: make sure you don't set it higher than the number of available output channels - 
        /// the number of source channels.
        /// n.b. Future NAudio may modify this
        /// </summary>
        public int ChannelOffset { get; set; }

        /// <summary>
        /// Input channel offset (used when recording), allowing you to choose to record from just one
        /// specific input rather than them all
        /// </summary>
        public int InputChannelOffset { get; set; }

        /// <summary>
        /// Sets the volume (1.0 is unity gain)
        /// Not supported for ASIO Out. Set the volume on the input stream instead
        /// </summary>
        [Obsolete("this function will be removed in a future NAudio as ASIO does not support setting the volume on the device")]
        public float Volume
        {
            get
            {
                return 1.0f;
            }
            set
            {
                if (value != 1.0f)
                {
                    throw new InvalidOperationException("AsioOut does not support setting the device volume");
                }
            }
        }

        private void RaisePlaybackStopped(Exception e)
        {
            var handler = PlaybackStopped;
            if (handler != null)
            {
                if (this.syncContext == null)
                {
                    handler(this, new StoppedEventArgs(e));
                }
                else
                {
                    this.syncContext.Post(state => handler(this, new StoppedEventArgs(e)), null);
                }
            }
        }
    }

}
