using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Drawing.Imaging;
using SpecAnalysis.Utilities;
using System.Linq;
using NAudio.Wave;

namespace SpecAnalysis
{
    public enum SpectrumDisplayType
    {
        DisplayLinear,
        DisplayLog
    };
    public partial class SpectrumAnalyzer : UserControl
    {
        // Define the sampling rate for audio processing
        private static int m_samplingRate = 44100;
        // Define the FFT size for the analysis
        private static int m_fftSize = 2048;

        // Get the path to the user's desktop directory.
        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        // Define the relative path for storing peaks.
        static string relativePathForDirectRecording = "..\\peaks";
        // Get the full path for recording peaks by combining the desktop path with the relative path.
        private string pathForDirectRecording = Path.GetFullPath(desktopPath + relativePathForDirectRecording);

        private double[] m_frequencyResponseOverall;
        private double[] m_filterAppliedSignal;
        private double[] m_frequencyTable;
        private double[] m_filterResonanceTable;
        private ComplexNumber[] cResponseArray;
        private double[] responseArray;
        private ComplexNumber cOne = new ComplexNumber(1.0f, 0.0f);

        private List<GraphicsPath> gPathList = new List<GraphicsPath>();
        private List<Pen> filterPenList = new List<Pen>();
        private List<bool> cutoffSelectionList = new List<bool>();
        private List<Rectangle> rectangleList = new List<Rectangle>();
        private List<float[]> m_transformedMagnitudesList = new List<float[]>();
        private List<ComplexNumber[]> cResponseList = new List<ComplexNumber[]>();
        private List<double[]> responseList = new List<double[]>();
        private List<FilterPanelUC> filterList= new List<FilterPanelUC>();

        private FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
        private SaveFileDialog saveDialog = new SaveFileDialog();
        private int minResponseDb = -120;
        private int maxResponseDb = 20;
        private int minFrequencyHz = 10;
        private int maxFrequencyHz = 22000;
        private int linearFrequencySpacing = 2000;
        private int yAxisSpacingDb = 10;
        private int displayFrequency = 0;
        private int innerRectLeft = 0;
        private int innerRectRight = 0;
        private int innerRectTop = 0;
        private int innerRectBottom = 0;
        private int innerRectHeight = 0;
        private int innerRectWidth = 0;
        private int frameRectLeft = 0;
        private int frameRectRight = 0;
        private int frameRectTop = 0;
        private int frameRectBottom = 0;
        private int frameRectWidth = 0;
        private int frameRectHeight = 0;
        private int frameRectX = 0;
        private int frameRectY = 0;
        private Size frameRectSize = new Size(0, 0);
        private int lastNumOfPoints = 0;
        private int zoomRectLeft = 0;
        private int zoomRectRight = 0;
        private int zoomRectTop = 0;
        private int zoomRectBottom = 0;
        private int filterRect1Left = 0;
        private int filterRect1Top = 0;
        private int cutoffRectX = 15;
        private int cutoffRectY = 15;
        private float firstHarmonicMag = 0.0f;
        private float firstHarmonicFreq = 0.0f;

        private bool mousePressed = false;
        private bool peakChecked = false;
        private bool selectingRect = false;
        private bool rectSelected = false;
        private bool filtersApplied = false;

        private Rectangle zoomRect = new Rectangle();
        private int rectInitialX = 0;
        private int rectInitialY = 0;

        private List<int> pointCoordinateX = new List<int>();
        private List<int> pointCoordinateY = new List<int>();
        private List<int> zoomPointCoordinateX = new List<int>();
        private List<int> zoomPointCoordinateY = new List<int>();
        private string nameForSelectedPoints = "";
        private string nameForFilter = "";
        private string nameForPeaks = "";
        private List<float> peakList = new List<float>(new float[m_fftSize / 2 - 1]);
        private List<float> envList = new List<float>(new float[m_fftSize / 2 - 1]);
        private List<float> freqList = new List<float>(new float[m_fftSize / 2 - 1]);
        private List<float> peakList2 = new List<float>(new float[m_fftSize / 2 - 1]);
        private List<float> envList2 = new List<float>(new float[m_fftSize / 2 - 1]);
        private List<float> freqList2 = new List<float>(new float[m_fftSize / 2 - 1]);

        private List<float> indexList = new List<float>();
        private List<float> magList = new List<float>();
        private List<float> periodIndexList = new List<float>();
        private List<float> periodMagList = new List<float>();
        private List<float> peakIndexList = new List<float>();
        private List<float> peakMagList = new List<float>();



        private int numberOfFilters = 0;

        private int cursorX = 0;
        private int cursorY = 0;
        private int resizeX = 0;
        private int resizeY = 0;
      
        private float releaseCoefficient = Convert.ToSingle(Math.Exp(-6.9078 * m_fftSize / (5000 * m_samplingRate * 0.001)));
        private float attackCoefficient = Convert.ToSingle(Math.Exp(-6.9078 * m_fftSize / (50 * m_samplingRate * 0.001)));

        private float attackCoefficient2 = 0f;
        private float releaseCoefficient2 = 0f;

        private ComplexNumber[] complexOutputs;
        private float[] realTimeOutputs;
        private float[] realTimeIndexes;
        private float[] realTimeMagnitudes;

        private float[] m_outputIndexes;
        private float[] m_outputMagnitudes;
        private float[] m_shiftedIndexes;
        private float[] m_transformedLinearIndexes;
        private float[] m_transformedLogIndexes;
        private float[] m_transformedMagnitudes;
        private float[] m_transformedMagnitudesOverall;
        private bool m_isPeriodic = false;

        private float[] m_transformedFrequencies;
        private float[] m_envelopeArray = new float[m_fftSize / 2 + 1];

        private SpectrumDisplayType m_xAxisDisplay = SpectrumDisplayType.DisplayLog;
        private SpectrumDisplayType m_yAxisDisplay = SpectrumDisplayType.DisplayLog;

        private Pen zoomPen = new Pen(Color.Maroon, 1);
        private Pen gridPen = new Pen(Color.FromArgb(128, Color.Silver), 1);
        private Pen pointToPointPen = new Pen(Color.DarkOrange, 1);
        private Pen periodicPeakPen = new Pen(Color.Navy, 6);
        private Pen pointPen = new Pen(Color.Red, 6);
        private Pen framePen = new Pen(Color.Silver, 2);
        private Pen innerPen = new Pen(Color.Silver, 2);
        private Pen drawPen = new Pen(Color.LightGoldenrodYellow);
        private Pen filterPenOverall = new Pen(Color.Indigo, 3);
        private Font freqFont = new Font(DefaultFont, FontStyle.Regular);
        private Font magFont = new Font(FontFamily.GenericSansSerif, 8);
        private Font slopeFont = new Font(FontFamily.GenericMonospace, 9);
        private Brush measureBrush = new SolidBrush(Color.Silver);
        private Brush pointToPointBrush = new SolidBrush(Color.OrangeRed);
        private Random rand = new Random();
        private int peakCount = 0;
        private float currentCutoffFrequency = 0;

        private int signalRectTop = 0;
        private int signalRectBottom = 0;
        private int signalRectLeft = 0;
        private int signalRectRight = 0;
        private int signalRectWidth = 0;
        private int signalRectHeight = 0;
        private Size signalRectSize = new Size(0, 0);
        private float maxValue = 0;
        private float minValue = 0;
        private bool firstHarmonicDetectEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumAnalyzer"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the spectrum analyzer by initializing its components,
        /// enabling double buffering to reduce flicker, and calling the <c>update</c> method
        /// to prepare necessary parameters. It also initializes various lists and arrays
        /// for the envelope, peak, and frequency values to default states.
        /// </remarks>
        public SpectrumAnalyzer()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.update();

            for (int i = 0; i < envList.Count; i++)
            {
                envList[i] = -120.0f;
                peakList[i] = innerRectBottom;
                freqList[i] = innerRectRight;
                envList2[i] = -1.0f;
                peakList2[i] = 0;
                freqList2[i] = 0;
            }

            Array.Clear(m_envelopeArray, 0, m_envelopeArray.Length);

            this.InitializeFrequencyTable(m_fftSize / 2 + 1);
            this.InitializeResonanceTable();
        }
        
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {            
            this.update();
            this.Invalidate();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {          
            this.update();
            this.Invalidate();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.update();
            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            GraphicsPath gPath = new GraphicsPath();
            GraphicsPath gPathOverall = new GraphicsPath();
            GraphicsPath gPathPeak = new GraphicsPath();
            GraphicsPath gPathSignal = new GraphicsPath();
            GraphicsPath gPathPeriod = new GraphicsPath();


            // Create a rectangular frame for the main display area, offsetting from the window edges
            Rectangle frameRect = Rectangle.FromLTRB(this.Left + 10, this.Top + 150, this.Right - 10 + resizeX, this.Bottom - 200 + resizeY);
            // Create an inner rectangle for the content area within the frame
            Rectangle innerRect = Rectangle.FromLTRB(frameRect.Left + 50, frameRect.Top + 30, frameRect.Right - 35, frameRect.Bottom - 30);

            GUI.DrawRoundedRectangle(g, frameRect, framePen, 4, 4, 4, 4);
            g.DrawRectangle(innerPen, innerRect);
            g.DrawRectangle(zoomPen, zoomRect);

            // Store the boundaries and dimensions of the inner rectangle for later use
            innerRectLeft = innerRect.Left;
            innerRectRight = innerRect.Right;
            innerRectBottom = innerRect.Bottom;
            innerRectTop = innerRect.Top;
            innerRectHeight = innerRect.Height;
            innerRectWidth = innerRect.Width;

            // Store the boundaries and dimensions of the frame rectangle for later use
            frameRectBottom = frameRect.Bottom;
            frameRectLeft = frameRect.Left;
            frameRectRight = frameRect.Right;
            frameRectTop = frameRect.Top;
            frameRectWidth = frameRect.Width;
            frameRectHeight = frameRect.Height;
            frameRectX = frameRect.X;
            frameRectY = frameRect.Y;
            frameRectSize = frameRect.Size;

            // Position the controls and set their width and height values to match the frame
            groupBoxEnvelope.Location = new Point(
                frameRectLeft,
                btnPeakReset.Bottom + 3
                );
            groupBoxEnvelope.Width = frameRectWidth;

            groupBoxFilters.Location = new Point(
                groupBoxEnvelope.Left,
                groupBoxEnvelope.Bottom 
                );
            groupBoxFilters.Width = frameRectWidth;

            groupBoxPoints.Location = new Point(
                frameRectLeft,
                frameRectTop - groupBoxPoints.Height
                );
            vScrollBar1.Location = new Point(
                frameRectLeft - 10,
                frameRectTop
                );
            vScrollBar1.Height = frameRectHeight;

            vScrollBar2.Location = new Point(
                innerRectRight + 10,
                innerRectTop
                );
            vScrollBar2.Height = innerRectHeight;

            checkBoxPeak.Location = new Point(
                frameRectWidth / 2,
                frameRectBottom + 10
                );
            btnPeakReset.Location = new Point(
                checkBoxPeak.Location.X + checkBoxPeak.Width + 10,
                checkBoxPeak.Location.Y - 5
                );
            btnSavePeaks.Location = new Point(
                checkBoxPeak.Location.X - btnSavePeaks.Width - 10,
                checkBoxPeak.Location.Y - 5
                );
            button4.Location = new Point(
                btnSavePeaks.Location.X - btnSavePeaks.Width - button4.Width - 10,
                btnSavePeaks.Location.Y
                );
            btnCaptureWaveform.Location = new Point(
                groupBoxPoints.Right + 5,
                groupBoxPoints.Top + btnCaptureWaveform.Height - 20
                );
            btnCaptureSpectrum.Location = new Point(
                btnPeakReset.Location.X + btnCaptureSpectrum.Width + 10,
                btnPeakReset.Location.Y
                );
            comboBox1.Location = new Point(
                frameRectLeft + 200,
                frameRectTop + 5
                );
            comboBox1.Height = innerRectTop - frameRectTop;

            txtSlope.Location = new Point(
                frameRectLeft + 400,
                frameRectTop + 8
                );
            txtSlope.Height = innerRectTop - frameRectTop;

            // Create a rectangular area for waveform display
            Rectangle signalRect = Rectangle.FromLTRB(btnCaptureWaveform.Right + 35, this.Top + 10, frameRect.Right, frameRect.Top - 10);

            // Store the boundaries and dimensions of the waveform rectangle for later use
            signalRectBottom = signalRect.Bottom;
            signalRectHeight = signalRect.Bottom;
            signalRectLeft = signalRect.Left;
            signalRectRight = signalRect.Right;
            signalRectTop = signalRect.Top;
            signalRectWidth = signalRect.Width;

            // Position the label for the waveform display within the waveform rectangle
            lblWavefile.Location = new Point(
                signalRect.Left + 5,
                signalRect.Top
                );

            // Draw the waveform rectangle for the signal display
            g.DrawRectangle(innerPen, signalRect);

            // Set display limits for the Y-axis based on the inner rectangle
            float displayScreenYMax = innerRect.Top;
            float displayScreenYMin = innerRect.Bottom;
            float displayScreenXMax = innerRect.Right;
            float displayScreenXMin = innerRect.Left;

            // Display the current X and Y coordinates in the upper-left corner of the inner rectangle
            g.DrawString("X: " + coordinateToBin(cursorX).ToString("0") + " Hz", magFont, measureBrush, innerRectLeft, frameRectTop);
            g.DrawString("Y: " + coordinateToDB(cursorY).ToString() + " dB", magFont, measureBrush, innerRectLeft, frameRectTop + 12);

            // Clear lists used for storing index and magnitude data
            indexList.Clear();
            magList.Clear();
            periodIndexList.Clear();
            periodMagList.Clear();
            peakIndexList.Clear();
            peakMagList.Clear();

            // Loop through each output in the real-time output array
            for (int i = 0; i < realTimeOutputs.Length; i++)
            {
                // Transform the index to screen coordinates
                realTimeIndexes[i] = GUI.transformCoordinateFloat(i, 0, realTimeOutputs.Length, signalRect.Left, signalRect.Right);
                // Transform the magnitude to screen coordinates, mapping -1 to 1 to the vertical axis
                realTimeMagnitudes[i] = GUI.transformCoordinateFloat(realTimeOutputs[i], -1, 1, signalRect.Bottom, signalRect.Top);
            }

            // Loop through the real-time output indexes to add line segments to gPathSignal for plotting the input waveform
            for (int i = 0; i < realTimeOutputs.Length - 1; i++)
            {
                gPathSignal.AddLine(realTimeIndexes[i], realTimeMagnitudes[i], realTimeIndexes[i + 1], realTimeMagnitudes[i + 1]);
            }

            // Loop through a range of values to display the magnitude ticks on the vertical axis of the input waveform
            for (int i = -2; i <= 2; i++)
            {
                float m = i / 2.0f;
                float k = GUI.transformCoordinateFloat(m, -1.0f, 1.0f, signalRect.Bottom, signalRect.Top);
                g.DrawString(m.ToString(), magFont, measureBrush, signalRect.Left - 28, k);
            }

            // Check if there are any filters to process
            if (filterList.Count > 0)
            {
                // Loop through the response list to retrieve frequency and complex responses from filters
                for (int i = 0; i < responseList.Count; i++)
                {
                    responseList[i] = filterList[i].FrequencyResponse;
                    cResponseList[i] = filterList[i].ComplexResponse;
                }

            }

            // Check again if there are filters to update their color
            if (filterList.Count > 0)
            {
                // Loop through each filter in the filter list
                for (int i = 0; i < filterList.Count; i++)
                {
                    // If the filter's color has changed, update the corresponding pen in the filter pen list
                    if (filterList[i].ColorChanged)
                        filterPenList[i] = filterList[i].FilterPen;
                }
            }

            // Loop through transformed log indexes to transform them to screen coordinates
            for (int i = 0; i < m_transformedLogIndexes.Length - 1; i++)
            {
                // Transform the shifted indexes to logarithmic screen coordinates
                m_transformedLogIndexes[i + 1] = GUI.transformCoordinateFloat(m_shiftedIndexes[i + 1], Convert.ToSingle(Math.Log(1, 2)), Convert.ToSingle(Math.Log(m_fftSize / 2 + 1, 2)), displayScreenXMin, displayScreenXMax);
                // Transform output indexes to linear screen coordinates
                m_transformedLinearIndexes[i + 1] = GUI.transformCoordinateFloat(m_outputIndexes[i + 1], 1, m_fftSize / 2 + 1, displayScreenXMin, displayScreenXMax);
            }

            // Loop through half the FFT size to calculate frequency response
            for (int i = 0; i < m_fftSize / 2 + 1; i++)
            {
                // Loop through the complex response list to aggregate responses
                for (int k = 0; k < cResponseList.Count; k++)
                {
                    // Multiply the complex response array for frequency response
                    cResponseArray[i] *= cResponseList[k][i];
                    // Accumulate the overall frequency response
                    responseArray[i] += responseList[k][i];

                    // Clamp the response to the maximum and minimum dB values
                    if (responseArray[i] > maxResponseDb)
                        responseArray[i] = maxResponseDb;
                    else
                        if (responseArray[i] < minResponseDb)
                        responseArray[i] = minResponseDb;
                }

                // Store the overall frequency response
                m_frequencyResponseOverall[i] = responseArray[i];
                // Apply the complex outputs to the complex response array
                cResponseArray[i] *= complexOutputs[i];
                // Calculate the logarithmic magnitude for the applied filter signal
                m_filterAppliedSignal[i] = cResponseArray[i].GetLogMagnitude(minResponseDb, maxResponseDb);
            }

            // Loop through the complex response array to initialize values
            for (int i = 0; i < cResponseArray.Length; i++)
            {
                cResponseArray[i] = cOne;
                responseArray[i] = 0.0;
            }

            // Loop through half the FFT size to transform magnitudes for display
            for (int i = 0; i < m_fftSize / 2 + 1; i++)
            {
                // Determine the magnitude to display based on whether filters are applied
                m_transformedMagnitudes[i] = filtersApplied ? GUI.transformCoordinateFloat((float)m_filterAppliedSignal[i], minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top) :
                    GUI.transformCoordinateFloat(m_envelopeArray[i], minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top);
                // Transform the overall frequency response for display
                m_transformedMagnitudesOverall[i] = GUI.transformCoordinateFloat((float)m_frequencyResponseOverall[i], minResponseDb, maxResponseDb, innerRectBottom, innerRectTop);
            }

            // Check if there are response lists to process
            if (responseList.Count > 0)
            {
                // Loop through each response list
                for (int j = 0; j < responseList.Count; j++)
                {
                    // Loop through the transformed magnitudes for the current response list
                    for (int i = 0; i < m_fftSize / 2 + 1; i++)
                    {
                        // Transform the response values to screen coordinates
                        m_transformedMagnitudesList[j][i] = GUI.transformCoordinateFloat((float)responseList[j][i], minResponseDb, maxResponseDb, innerRectBottom, innerRectTop);
                    }
                }
            }


            // Not zoomed
            if (!rectSelected)
            {
                // Periodic signal peak detection, might need improvements!
                if (m_isPeriodic)
                {
                    for (int i = 1; i < m_fftSize / 2; i++)
                    {
                        if (m_envelopeArray[i] > m_envelopeArray[i - 1] && m_envelopeArray[i] > m_envelopeArray[i + 1] && m_envelopeArray[i] > -99)
                        {
                            float k = GUI.transformCoordinateFloat(m_envelopeArray[i], minResponseDb, maxResponseDb, innerRectBottom, innerRectTop);
                            float m = GUI.transformCoordinateFloat(m_shiftedIndexes[i], Convert.ToSingle(Math.Log(1, 2)), Convert.ToSingle(Math.Log(m_fftSize / 2 + 1, 2)), innerRectLeft, innerRectRight);
                            peakMagList.Add(k);
                            peakIndexList.Add(m);
                        }
                    }
                    if (peakMagList.Count != 0)
                    {
                        if (firstHarmonicDetectEnabled == true)
                        {
                            firstHarmonicMag = peakMagList[0];
                            firstHarmonicFreq = peakIndexList[0];
                            firstHarmonicDetectEnabled = false;
                        }                   

                        for (int i = 0; i < peakMagList.Count; i++)
                        {
                            if (coordinateToDB((int)peakMagList[i]) >= coordinateToDB((int)firstHarmonicMag) - 40 || coordinateToDB((int)peakMagList[i]) == coordinateToDB((int)firstHarmonicMag))
                            {
                                int mag = coordinateToDB((int)peakMagList[i]);
                                float freq = coordinateToBin(peakIndexList[i]);
                                float magRatio = coordinateToDB(firstHarmonicMag) / (float)mag;
                                float freqRatio = freq / coordinateToBin(firstHarmonicFreq);
                                g.DrawEllipse(periodicPeakPen, peakIndexList[i], peakMagList[i], 1, 1);
                                g.DrawString(mag.ToString() + "dB, " + freq.ToString("0") + "Hz", magFont, measureBrush, peakIndexList[i] - 60, peakMagList[i] - 20);
                                g.DrawString("Ratio: " + magRatio.ToString("0.00"), magFont, measureBrush, peakIndexList[i] - 60, peakMagList[i] - 40);
                                g.DrawString("Harmonic " + freqRatio.ToString("0"), magFont, measureBrush, peakIndexList[i] - 60, peakMagList[i] - 60);
                            }
                            
                        }
                    }

                }

                // Logarithmic y axis grid
                for (int j = minResponseDb; j <= maxResponseDb; j += yAxisSpacingDb)
                {
                    int y = GUI.transformCoordinate(j, minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top);
                    g.DrawLine(gridPen, innerRect.Left, y, innerRect.Right, y);
                    g.DrawString(j.ToString() + " dB", magFont, measureBrush, frameRect.Left, y - 5);
                }

                // Logarithmic x axis grid
                if (m_xAxisDisplay == SpectrumDisplayType.DisplayLog)
                {
                    for (int i = Convert.ToInt32(coordinateToBin(innerRectLeft)); i < Convert.ToInt32(coordinateToBin(innerRectRight)); i++)
                    {
                        if (i % 10 == 0 && i < 100)
                        {
                            float k = binToCoordinate(i);
                            if (i != 40 && i != 50 && i != 70 && i != 90)
                                g.DrawString(i.ToString(), freqFont, measureBrush, k - 10, frameRect.Bottom - 15);
                            g.DrawLine(gridPen, k, innerRect.Bottom, k, innerRect.Top);
                        }
                        else
                            if (i % 100 == 0 && i < 1000)
                        {
                            float k = binToCoordinate(i);
                            if (i != 500 && i != 700 && i != 900)
                                g.DrawString(i.ToString(), freqFont, measureBrush, k - 10, frameRect.Bottom - 15);
                            g.DrawLine(gridPen, k, innerRect.Bottom, k, innerRect.Top);
                        }
                        else
                            if (i % 1000 == 0 && i <= 10000)
                        {
                            float k = binToCoordinate(i);
                            if (i != 5000 && i != 7000 && i != 9000)
                                g.DrawString((i / 1000).ToString() + "K", freqFont, measureBrush, k - 10, frameRect.Bottom - 15);
                            g.DrawLine(gridPen, k, innerRect.Bottom, k, innerRect.Top);
                        }
                        else
                            if (i % 10000 == 0 && i <= 20000)
                        {
                            float k = binToCoordinate(i);
                            g.DrawString((i / 1000).ToString() + "K", freqFont, measureBrush, k - 10, frameRect.Bottom - 15);
                            g.DrawLine(gridPen, k, innerRect.Bottom, k, innerRect.Top);
                        }
                    }
                }

                // Linear x axis grid
                if (m_xAxisDisplay == SpectrumDisplayType.DisplayLinear)
                {
                    for (int i = minFrequencyHz; i < maxFrequencyHz; i += linearFrequencySpacing)
                    {
                        int x = GUI.transformCoordinateInt(i, minFrequencyHz, maxFrequencyHz, innerRect.Left, innerRect.Right);
                        g.DrawLine(gridPen, x, innerRect.Bottom, x, innerRect.Top);
                        g.DrawString(i.ToString() + " Hz", freqFont, measureBrush, x - 12, frameRect.Bottom - 15);
                    }
                }

                // Draw selected points
                for (int i = 0; i < pointCoordinateX.Count; i++)
                {
                    g.DrawEllipse(pointPen, pointCoordinateX[i], pointCoordinateY[i], 1, 1);
                    g.DrawString("Point" + i, magFont, pointToPointBrush, pointCoordinateX[i], pointCoordinateY[i]);
                }
                if (pointCoordinateX.Count >= 2 && pointCoordinateX != null && pointCoordinateY != null && pointCoordinateX.Count != 0 && pointCoordinateY.Count != 0)
                {
                    float magChange = (float)coordinateToDB(pointCoordinateY[pointCoordinateY.Count - 1]) - (float)coordinateToDB(pointCoordinateY[pointCoordinateY.Count - 2]);
                    float frequencyChange = Convert.ToSingle(Math.Log((float)coordinateToBin(pointCoordinateX[pointCoordinateX.Count - 1]) / (float)coordinateToBin(pointCoordinateX[pointCoordinateX.Count - 2]), 2));
                    float slope = magChange / frequencyChange;
                    g.DrawLine(pointToPointPen, pointCoordinateX[pointCoordinateX.Count - 2], pointCoordinateY[pointCoordinateY.Count - 2], pointCoordinateX[pointCoordinateX.Count - 1], pointCoordinateY[pointCoordinateY.Count - 1]);
                    txtSlope.Text = "Slope: " + magChange.ToString() + "/" + frequencyChange.ToString();
                }

                // Calculate peaks
                for (int i = 0; i < m_transformedMagnitudes.Length - 2; i++)
                {
                    float derivative1 = findDerivative(m_transformedLogIndexes[i], m_transformedMagnitudes[i], m_transformedLogIndexes[i + 1], m_transformedMagnitudes[i + 1]);
                    float derivative2 = findDerivative(m_transformedLogIndexes[i + 1], m_transformedMagnitudes[i + 1], m_transformedLogIndexes[i + 2], m_transformedMagnitudes[i + 2]);
                    if ((derivative1 >= 0 && derivative2 < 0 && envList[i] <= m_envelopeArray[i + 1] && m_xAxisDisplay == SpectrumDisplayType.DisplayLog) || (derivative1 < 0 && derivative2 >= 0 && envList[i] <= m_envelopeArray[i + 1] && m_xAxisDisplay == SpectrumDisplayType.DisplayLog))
                    {
                        envList[i] = m_envelopeArray[i + 1];
                        peakList[i] = GUI.transformCoordinateFloat(envList[i], minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top);
                        freqList[i] = m_transformedLogIndexes[i + 1];
                    }
                }

                // Draw the lines
                for (int i = 0; i < m_transformedMagnitudes.Length - 2; i++)
                {
                    if (m_xAxisDisplay == SpectrumDisplayType.DisplayLog)
                    {
                        if (filterList.Count > 0)
                        {
                            for (int j = 0; j < filterList.Count; j++)
                            {
                                gPathList[j].AddLine(m_transformedLogIndexes[i + 1], m_transformedMagnitudesList[j][i + 1], m_transformedLogIndexes[i + 2], m_transformedMagnitudesList[j][i + 2]);
                            }

                        }
                        if (rectangleList.Count > 0)
                        {
                            for (int k = 0; k < rectangleList.Count; k++)
                            {
                                if (filterList[k].FilterSelected)
                                {
                                    rectangleList[k] = new Rectangle((int)m_transformedLogIndexes[filterList[k].FilterCutoff] - 5, (int)m_transformedMagnitudesList[k][filterList[k].FilterCutoff] - 5, cutoffRectX, cutoffRectY);
                                    filterRect1Left = rectangleList[k].Left;
                                    filterRect1Top = rectangleList[k].Top;
                                    g.DrawRectangle(filterPenList[k], rectangleList[k]);
                                }
                            }
                        }
                        gPath.AddLine(m_transformedLogIndexes[i + 1], m_transformedMagnitudes[i + 1], m_transformedLogIndexes[i + 2], m_transformedMagnitudes[i + 2]);
                        gPathOverall.AddLine(m_transformedLogIndexes[i + 1], m_transformedMagnitudesOverall[i + 1], m_transformedLogIndexes[i + 2], m_transformedMagnitudesOverall[i + 2]);
                    }
                    else if (m_xAxisDisplay == SpectrumDisplayType.DisplayLinear)
                    {
                        if (filterList.Count > 0)
                        {
                            for (int j = 0; j < filterList.Count; j++)
                            {
                                gPathList[j].AddLine(m_transformedLinearIndexes[i + 1], m_transformedMagnitudesList[j][i + 1], m_transformedLinearIndexes[i + 2], m_transformedMagnitudesList[j][i + 2]);
                            }

                        }
                        if (rectangleList.Count > 0)
                        {
                            for (int k = 0; k < rectangleList.Count; k++)
                            {
                                if (filterList[k].FilterSelected)
                                {
                                    rectangleList[k] = new Rectangle((int)m_transformedLinearIndexes[filterList[k].FilterCutoff] - 5, (int)m_transformedMagnitudesList[k][filterList[k].FilterCutoff] - 5, cutoffRectX, cutoffRectY);
                                    filterRect1Left = rectangleList[k].Left;
                                    filterRect1Top = rectangleList[k].Top;
                                    g.DrawRectangle(filterPenList[k], rectangleList[k]);
                                }
                            }
                        }
                        gPathOverall.AddLine(m_transformedLinearIndexes[i + 1], m_transformedMagnitudesOverall[i + 1], m_transformedLinearIndexes[i + 2], m_transformedMagnitudesOverall[i + 2]);
                        gPath.AddLine(m_transformedLinearIndexes[i + 1], m_transformedMagnitudes[i + 1], m_transformedLinearIndexes[i + 2], m_transformedMagnitudes[i + 2]);
                    }

                }
            }

            // Zoomed in
            if (rectSelected == true)
            {
                if (m_isPeriodic)
                {
                    for (int i = coordinateToIndex(zoomRectLeft); i < coordinateToIndex(zoomRectRight); i++)
                    {
                        if (m_envelopeArray[i] > m_envelopeArray[i - 1] && m_envelopeArray[i] > m_envelopeArray[i + 1] && m_envelopeArray[i] > -99 && m_envelopeArray[i] > coordinateToDB(zoomRectBottom) && m_envelopeArray[i] < coordinateToDB(zoomRectTop))
                        {
                            float k = GUI.transformCoordinateFloat(m_envelopeArray[i], coordinateToDB(zoomRectBottom), coordinateToDB(zoomRectTop), displayScreenYMin, displayScreenYMax);
                            float m = GUI.transformCoordinateFloat(m_shiftedIndexes[i], Convert.ToSingle(Math.Log(coordinateToIndex(zoomRectLeft), 2)), Convert.ToSingle(Math.Log(coordinateToIndex(zoomRectRight), 2)), displayScreenXMin, displayScreenXMax);
                            peakMagList.Add(k);
                            peakIndexList.Add(m);
                        }
                    }
                    //firstHarmonicMag = GUI.transformCoordinateFloat(peakMagList[0], innerRectTop, innerRectBottom, zoomRectTop, zoomRectBottom); 
                    //firstHarmonicFreq = GUI.transformCoordinateFloat(peakIndexList[0], innerRectLeft, innerRectRight, zoomRectLeft, zoomRectRight);

                    for (int i = 0; i < peakMagList.Count; i++)
                    {
                        float mag = coordinateToDB(GUI.transformCoordinateFloat(peakMagList[i], innerRectTop, innerRectBottom, zoomRectTop, zoomRectBottom));
                        float freq = coordinateToBin(GUI.transformCoordinateFloat(peakIndexList[i], innerRectLeft, innerRectRight, zoomRectLeft, zoomRectRight));
                        float magRatio = coordinateToDB(firstHarmonicMag) / mag;
                        float freqRatio = freq / coordinateToBin(firstHarmonicFreq);
                        g.DrawEllipse(periodicPeakPen, peakIndexList[i], peakMagList[i], 1, 1);
                        g.DrawString(mag.ToString("0") + "dB, " + freq.ToString("0") + "Hz", magFont, measureBrush, peakIndexList[i] - 70, peakMagList[i] - 20);
                        g.DrawString("Ratio: " + magRatio.ToString("0.00"), magFont, measureBrush, peakIndexList[i] - 70, peakMagList[i] - 40);
                        g.DrawString("Harmonic " + freqRatio.ToString("0"), magFont, measureBrush, peakIndexList[i] - 70, peakMagList[i] - 60);
                    }
                }
                for (int i = coordinateToIndex(zoomRectLeft); i < coordinateToIndex(zoomRectRight); i++)
                {
                    indexList.Add(GUI.transformCoordinateFloat(Convert.ToSingle(Math.Log(i, 2)), Convert.ToSingle(Math.Log(coordinateToIndex(zoomRectLeft), 2)), Convert.ToSingle(Math.Log(coordinateToIndex(zoomRectRight), 2)), displayScreenXMin, displayScreenXMax));

                    if (m_envelopeArray[i] < coordinateToDB(zoomRectTop) && m_envelopeArray[i] > coordinateToDB(zoomRectBottom))
                        magList.Add(GUI.transformCoordinateFloat(m_envelopeArray[i], coordinateToDB(zoomRectBottom), coordinateToDB(zoomRectTop), displayScreenYMin, displayScreenYMax));
                    else
                        if (m_envelopeArray[i] < coordinateToDB(zoomRectBottom))
                        magList.Add(displayScreenYMin);
                    else
                        if (m_envelopeArray[i] > coordinateToDB(zoomRectTop))
                        magList.Add(displayScreenYMax);
                }

                for (int j = 0; j < indexList.Count - 1; j++)
                {
                    gPath.AddLine(indexList[j], magList[j], indexList[j + 1], magList[j + 1]);
                }

                // Y axis grid
                for (int j = coordinateToDB(zoomRectBottom); j <= coordinateToDB(zoomRectTop); j += yAxisSpacingDb)
                {
                    int y = GUI.transformCoordinate(j, coordinateToDB(zoomRectBottom), coordinateToDB(zoomRectTop), innerRect.Bottom, innerRect.Top);
                    g.DrawLine(gridPen, innerRect.Left, y, innerRect.Right, y);
                    g.DrawString(j.ToString() + " dB", magFont, measureBrush, frameRect.Left, y - 5);
                }

                // X axis grid
                for (int i = Convert.ToInt32(coordinateToBin(zoomRectLeft)); i < coordinateToBin(zoomRectRight); i += (Convert.ToInt32(coordinateToBin(zoomRectRight)) - Convert.ToInt32(coordinateToBin(zoomRectLeft))) / 6)
                {
                    int k = GUI.transformCoordinate(Math.Log(i, 2), Math.Log(coordinateToBin(zoomRectLeft), 2), Math.Log(coordinateToBin(zoomRectRight), 2), innerRect.Left, innerRect.Right);
                    g.DrawLine(gridPen, k, innerRectBottom, k, innerRectTop);

                    if (i < 1000)
                        g.DrawString(i.ToString(), freqFont, measureBrush, k - 10, frameRect.Bottom - 15);
                    else
                        if (i >= 1000)
                        g.DrawString((i / 1000).ToString() + "." + (i & 1000).ToString("0") + "K", freqFont, measureBrush, k - 10, frameRect.Bottom - 15);

                }

                // Draw selected points and the line connects them
                for (int i = 0; i < zoomPointCoordinateX.Count; i++)
                {
                    g.DrawEllipse(pointPen, zoomPointCoordinateX[i], zoomPointCoordinateY[i], 1, 1);
                    g.DrawString("Point" + i, magFont, pointToPointBrush, zoomPointCoordinateX[i], zoomPointCoordinateY[i]);
                }
                if (pointCoordinateX.Count >= 2 && pointCoordinateX != null && pointCoordinateY != null && pointCoordinateX.Count != 0 && pointCoordinateY.Count != 0)
                {
                    float magChange = (float)coordinateToDB(pointCoordinateY[pointCoordinateY.Count - 1]) - (float)coordinateToDB(pointCoordinateY[pointCoordinateY.Count - 2]);
                    float frequencyChange = Convert.ToSingle(Math.Log((float)coordinateToBin(pointCoordinateX[pointCoordinateX.Count - 1]) / (float)coordinateToBin(pointCoordinateX[pointCoordinateX.Count - 2]), 2));
                    float slope = magChange / frequencyChange;
                    int x1 = GUI.transformCoordinateInt(pointCoordinateX[pointCoordinateX.Count - 2], zoomRectLeft, zoomRectRight, innerRectLeft, innerRectRight);
                    int y1 = GUI.transformCoordinateInt(pointCoordinateY[pointCoordinateY.Count - 2], zoomRectBottom, zoomRectTop, innerRectBottom, innerRectTop);
                    int x2 = GUI.transformCoordinateInt(pointCoordinateX[pointCoordinateX.Count - 1], zoomRectLeft, zoomRectRight, innerRectLeft, innerRectRight);
                    int y2 = GUI.transformCoordinateInt(pointCoordinateY[pointCoordinateY.Count - 1], zoomRectBottom, zoomRectTop, innerRectBottom, innerRectTop);

                    if (x1 < innerRectLeft)
                        x1 = innerRectLeft;
                    else
                    if (x1 > innerRectRight)
                        x1 = innerRectRight;

                    if (x2 < innerRectLeft)
                        x2 = innerRectLeft;
                    else
                    if (x2 > innerRectRight)
                        x2 = innerRectRight;

                    if (y1 < innerRectTop)
                        y1 = innerRectTop;
                    else
                    if (y1 > innerRectBottom)
                        y1 = innerRectBottom;

                    if (y2 < innerRectTop)
                        y2 = innerRectTop;
                    else
                    if (y2 > innerRectBottom)
                        y2 = innerRectBottom;

                    g.DrawLine(pointToPointPen, x1, y1, x2, y2);
                    txtSlope.Text = "Slope: " + magChange.ToString() + "/" + frequencyChange.ToString();
                }

                // Calculate peaks
                for (int i = coordinateToIndex(zoomRectLeft); i < coordinateToIndex(zoomRectRight); i++)
                {
                    float derivative1 = findDerivative(m_transformedLogIndexes[i], m_transformedMagnitudes[i], m_transformedLogIndexes[i + 1], m_transformedMagnitudes[i + 1]);
                    float derivative2 = findDerivative(m_transformedLogIndexes[i + 1], m_transformedMagnitudes[i + 1], m_transformedLogIndexes[i + 2], m_transformedMagnitudes[i + 2]);
                    if ((derivative1 >= 0 && derivative2 < 0 && envList[i] <= m_envelopeArray[i + 1] && m_xAxisDisplay == SpectrumDisplayType.DisplayLog && m_envelopeArray[i + 1] < coordinateToDB(zoomRectTop) && m_envelopeArray[i + 1] > coordinateToDB(zoomRectBottom)) || (derivative1 < 0 && derivative2 >= 0 && envList[i] <= m_envelopeArray[i + 1] && m_xAxisDisplay == SpectrumDisplayType.DisplayLog && m_envelopeArray[i + 1] < coordinateToDB(zoomRectTop) && m_envelopeArray[i + 1] > coordinateToDB(zoomRectBottom)))
                    {
                        envList[i] = m_envelopeArray[i + 1];
                        peakList[i] = GUI.transformCoordinateFloat(envList[i], coordinateToDB(zoomRectBottom), coordinateToDB(zoomRectTop), displayScreenYMin, displayScreenYMax);
                        freqList[i] = GUI.transformCoordinateFloat(Convert.ToSingle(Math.Log(i + 1, 2)), Convert.ToSingle(Math.Log(coordinateToIndex(zoomRectLeft), 2)), Convert.ToSingle(Math.Log(coordinateToIndex(zoomRectRight), 2)), displayScreenXMin, displayScreenXMax);
                    }

                }
            }

            // Draw the copied frequency message at the top right corner of the frame
            g.DrawString(displayFrequency.ToString() + " Hz copied to the clipboard", freqFont, measureBrush, frameRect.Right - 220, frameRect.Top + 10);

            // Draw peaks
            for (int i = 0; i < peakList.Count - 1; i++)
            {
                if (peakList.Count >= 2 && peakChecked == true && mousePressed == false && peakList[i] < innerRectBottom && peakList[i + 1] < innerRectBottom && freqList[i] < innerRectRight && freqList[i + 1] < innerRectRight && freqList[i] >= innerRectLeft && freqList[i + 1] > innerRectLeft)
                    gPathPeak.AddLine(freqList[i], peakList[i], freqList[i + 1], peakList[i + 1]);
            }
            g.DrawPath(Pens.SteelBlue, gPathPeak);      // Draw Peaks 
            g.DrawPath(drawPen, gPath);                 // Draw frequency spectrum
            g.DrawPath(filterPenOverall, gPathOverall); // Draw overall filter responses
            g.DrawPath(drawPen, gPathSignal);           // Draw real-time time domain signal
            //g.DrawPath(Pens.Red, gPathPeriod);        // Uncomment the following line to draw the periodic path if needed


            // Check if there are any filter paths to draw
            if (gPathList.Count > 0)
            {
                for (int i = 0; i < gPathList.Count; i++)
                {
                    g.DrawPath(filterPenList[i], gPathList[i]);
                }
            }

            // Reset paths for the next paint cycle to clear previous drawings
            if (gPathList.Count > 0)
            {
                for (int i = 0; i < gPathList.Count; i++)
                {
                    gPathList[i].Reset();
                }
            }

            // Dispose of the paths to free up resources and prevent memory leaks
            gPathOverall.Dispose();
            gPath.Dispose();
            gPathPeak.Dispose();
            gPathSignal.Dispose();
            gPathPeriod.Dispose();
        }

        /// <summary>
        /// Enables the detection of the first harmonic in the spectrum analysis.
        /// </summary>
        /// <remarks>
        /// This method sets the <c>firstHarmonicDetectEnabled</c> flag to true, allowing the spectrum analyzer
        /// to include the first harmonic in its calculations and visualizations.
        /// </remarks>
        public void SpectrumFirstHarmonicUpdate()
        {
            firstHarmonicDetectEnabled = true;
        }

        /// <summary>
        /// Updates the internal coefficients for the spectrum analyzer based on the current FFT size, 
        /// sampling rate, and selected display settings.
        /// </summary>
        /// <remarks>
        /// This method calculates the attack and release coefficients used for envelope following in the
        /// spectrum analysis. It also sets the display types for the X and Y axes based on the user's 
        /// selection in the combo box. Finally, it resets the envelope, peak, and frequency lists to their
        /// initial values.
        /// </remarks>
        public void update()
        {
            releaseCoefficient = Convert.ToSingle(Math.Exp(-6.9078 * m_fftSize / (Convert.ToSingle(numericUpDown3.Value) * m_samplingRate * 0.001)));
            releaseCoefficient2 = 1.0f - releaseCoefficient;
            attackCoefficient = Convert.ToSingle(Math.Exp(-6.9078 * m_fftSize / (Convert.ToSingle(numericUpDown2.Value) * m_samplingRate * 0.001)));
            attackCoefficient2 = 1.0f - attackCoefficient;

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    m_yAxisDisplay = SpectrumDisplayType.DisplayLog;
                    m_xAxisDisplay = SpectrumDisplayType.DisplayLog;
                    break;

                case 1:
                    m_yAxisDisplay = SpectrumDisplayType.DisplayLog;
                    m_xAxisDisplay = SpectrumDisplayType.DisplayLinear;
                    break;

                default:
                    break;
            }

            for (int i = 0; i < envList2.Count; i++)
            {
                envList2[i] = -1.0f;
                peakList2[i] = 0;
                freqList2[i] = 0;
            }
        }

        /// <summary>
        /// Updates the spectrum analyzer with the provided FFT size, sampling rate, and output data.
        /// </summary>
        /// <param name="fftSize">The size of the FFT to be used for processing.</param>
        /// <param name="samplingRate">The sampling rate of the audio signal.</param>
        /// <param name="outputPoints">An array of points representing the output magnitudes and frequencies.</param>
        /// <param name="shiftedIndexes">An array of shifted frequency indexes for the FFT output.</param>
        /// <param name="realTimeOutput">An array of real-time values for the waveform display.</param>
        /// <param name="isPeriodic">Indicates whether the harmonics turned on/off.</param>
        /// <param name="cOutputPoints">An array of complex numbers representing the FFT output.</param>
        /// <param name="numOfSamples">The number of samples to process.</param>
        /// <remarks>
        /// This method initializes various internal structures based on the FFT size and sampling rate.
        /// It processes the output data and calculates the current cutoff frequency based on the maximum 
        /// envelope magnitude detected in the output points.
        /// </remarks>
        public void update(int fftSize, int samplingRate, PointF[] outputPoints, float[] shiftedIndexes, float[] realTimeOutput, bool isPeriodic, ComplexNumber[] cOutputPoints, int numOfSamples) 
        {
            float maxValue = -1000f;
            int maxIndex = 0;

            if (fftSize != m_fftSize || samplingRate != m_samplingRate)
            {
                m_fftSize = fftSize;
                m_samplingRate = samplingRate;

                this.InitializeFrequencyTable(m_fftSize / 2 + 1);
                this.InitializeResonanceTable();
                this.update();

                peakList = new List<float>(new float[fftSize / 2 - 1]);
                envList = new List<float>(new float[fftSize / 2 - 1]);
                freqList = new List<float>(new float[fftSize / 2 - 1]);
                m_envelopeArray = new float[fftSize / 2 + 1];
            }

            m_isPeriodic = isPeriodic;
            complexOutputs = new ComplexNumber[fftSize / 2 + 1];
            realTimeOutputs = new float[numOfSamples];
            realTimeIndexes = new float[numOfSamples];
            realTimeMagnitudes = new float[numOfSamples];
            m_outputMagnitudes = new float[fftSize / 2 + 1];
            m_outputIndexes = new float[fftSize / 2 + 1];
            m_shiftedIndexes = new float[fftSize / 2 + 1];
            m_transformedFrequencies = new float[fftSize / 2 + 1];
            m_transformedMagnitudes = new float[fftSize / 2 + 1];
            m_transformedMagnitudesOverall = new float[fftSize / 2 + 1];
            m_transformedLogIndexes = new float[fftSize / 2 + 1];
            m_transformedLinearIndexes = new float[fftSize / 2 + 1];

            for (int i = 0; i < numOfSamples; i++)
            {
                realTimeOutputs[i] = realTimeOutput[i];
            }

            for (int i = 0; i < fftSize / 2 + 1; i++)
            {
                complexOutputs[i].real = cOutputPoints[i].real;
                complexOutputs[i].imag = cOutputPoints[i].imag;
                m_shiftedIndexes[i] = shiftedIndexes[i];
                m_outputMagnitudes[i] = outputPoints[i].Y;
                m_outputIndexes[i] = outputPoints[i].X;


                if (m_outputMagnitudes[i] < minResponseDb)
                    m_outputMagnitudes[i] = minResponseDb;
                else if (m_outputMagnitudes[i] > maxResponseDb)
                    m_outputMagnitudes[i] = maxResponseDb;

                if (m_outputMagnitudes[i] > m_envelopeArray[i])
                    m_envelopeArray[i] = attackCoefficient * m_envelopeArray[i] + attackCoefficient2 * m_outputMagnitudes[i];
                else
                    m_envelopeArray[i] = releaseCoefficient * m_envelopeArray[i] + releaseCoefficient2 * m_outputMagnitudes[i];

                if(m_envelopeArray[i] > maxValue)
                {
                    maxValue = m_envelopeArray[i];
                    maxIndex = i;
                }                
            }
            currentCutoffFrequency = ((float)samplingRate / (float)fftSize) * maxIndex;
        }

        /// <summary>
        /// Handles the mouse down event within the spectrum analyzer.
        /// </summary>
        /// <remarks>
        /// This method processes right-click actions for frequency display, 
        /// initiates rectangle selection, and detects cutoff selection for filters.
        /// It also enables the resizing of the analyzer frame when the mouse is
        /// near the bottom right corner.
        /// </remarks>
        private void SpectrumAnalyzer_MouseDown(object sender, MouseEventArgs e)
        {
            
            if (e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && e.Button == MouseButtons.Right && m_xAxisDisplay == SpectrumDisplayType.DisplayLinear)
            {
                displayFrequency = Convert.ToInt32((m_samplingRate / m_fftSize) * Convert.ToSingle((e.Location.X - innerRectLeft) * (Convert.ToSingle(m_fftSize / 2) / (innerRectRight - innerRectLeft))));
                System.Diagnostics.Debug.Print("Frequency: " + displayFrequency);
                Clipboard.SetText(displayFrequency.ToString());
            }
            else if (e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && e.Button == MouseButtons.Right)
            {                
                rectInitialX = e.X;
                rectInitialY = e.Y;
                selectingRect = true;
            }
            
            for (int i = 0; i < rectangleList.Count; i++)
            {
                if (e.Location.X > rectangleList[i].Left && e.Location.X < rectangleList[i].Left + cutoffRectX && e.Location.Y > rectangleList[i].Top && e.Location.Y < rectangleList[i].Top + cutoffRectY)
                {
                    cutoffSelectionList[i] = true;
                    listBox1.SelectedIndex = i;
                }
            }            
            
            if ((e.Location.X < (frameRectRight + 10)) && (e.Location.X > (frameRectRight - 10)) && (e.Location.Y < (frameRectBottom + 10)) && (e.Location.Y > (frameRectBottom - 10)) && e.Button == MouseButtons.Left && rectSelected == false)
                mousePressed = true;

            this.Invalidate();
        }

        /// <summary>
        /// Handles the mouse movement event within the spectrum analyzer.
        /// </summary>
        /// <remarks>
        /// This method updates the cursor position based on the mouse location and handles
        /// adjustments to filter cutoffs if a cutoff selection is active. It also manages
        /// the zoom rectangle during selection.
        /// </remarks>
        private void SpectrumAnalyzer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X < innerRectRight && e.X > innerRectLeft && e.Y < innerRectBottom && e.Y > innerRectTop && !rectSelected)
            {
                cursorX = e.X;
                cursorY = e.Y;
            }
            if (e.X < innerRectRight && e.X > innerRectLeft && e.Y < innerRectBottom && e.Y > innerRectTop && rectSelected)
            {
                cursorX = GUI.transformCoordinate(e.X, innerRectLeft, innerRectRight, zoomRectLeft, zoomRectRight);
                cursorY = GUI.transformCoordinate(e.Y, innerRectTop, innerRectBottom, zoomRectTop, zoomRectBottom);
            }
            /*if (mousePressed)
            {
                resizeX = e.X - frameRectRight;
                resizeY = e.Y - frameRectBottom;
            }*/
            for (int i = 0; i < cutoffSelectionList.Count; i++)
            {
                if (cutoffSelectionList[i] && e.X < innerRectRight && e.X > innerRectLeft && e.Y < innerRectBottom && e.Y > innerRectTop)
                {
                    filterList[i].FilterCutoff = Convert.ToInt32(coordinateToBin(e.X)) / (m_samplingRate / m_fftSize);

                    if (filterList[i].FilterCutoff < 0)
                        filterList[i].FilterCutoff = 0;
                    else
                        if (filterList[i].FilterCutoff > m_fftSize / 2)
                        filterList[i].FilterCutoff = m_fftSize / 2;

                    filterList[i].FilterResonance = GUI.transformCoordinateInt(e.Y, innerRectBottom, innerRectTop, 0, 128);
                    filterList[i].update();
                }
            }

            if (selectingRect && e.X > rectInitialX && e.Y < rectInitialY && e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && !rectSelected)
                    zoomRect = Rectangle.FromLTRB(rectInitialX, e.Y, e.X, rectInitialY);
            else 
                if (selectingRect && e.X > rectInitialX && e.Y > rectInitialY && e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && !rectSelected)
                    zoomRect = Rectangle.FromLTRB(rectInitialX, rectInitialY, e.X, e.Y);
            else 
                if (selectingRect && e.X < rectInitialX && e.Y < rectInitialY && e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && !rectSelected)
                    zoomRect = Rectangle.FromLTRB(e.X, e.Y, rectInitialX, rectInitialY);
            else 
                if (selectingRect && e.X < rectInitialX && e.Y > rectInitialY && e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && !rectSelected)
                zoomRect = Rectangle.FromLTRB(e.X, rectInitialY, rectInitialX, e.Y);
        }

        /// <remarks>
        /// This method performs several actions upon releasing the mouse button:
        /// <list type="bullet">
        /// <item>
        /// <description>Sets <c>mousePressed</c> and <c>selectingRect</c> to false.</description>
        /// </item>
        /// <item>
        /// <description>Resets all entries in the <c>cutoffSelectionList</c> to false.</description>
        /// </item>
        /// <item>
        /// <description>If the selected zoom rectangle's dimensions are greater than the specified thresholds:</description>
        /// <list type="bullet">
        /// <item>
        /// <description>Sets <c>rectSelected</c> to true.</description>
        /// </item>
        /// <item>
        /// <description>Adds points from <c>pointCoordinateX</c> and <c>pointCoordinateY</c> to <c>zoomPointCoordinateX</c> and <c>zoomPointCoordinateY</c> if they lie within the zoom rectangle.</description>
        /// </item>
        /// <item>
        /// <description>Resets the <c>envList</c>, <c>peakList</c>, and <c>freqList</c> to their initial values.</description>
        /// </item>
        /// <item>
        /// <description>Stores the dimensions of the zoom rectangle for future reference.</description>
        /// </item>
        /// <item>
        /// <description>Resets the <c>zoomRect</c> to a zero-sized rectangle.</description>
        /// </item>
        /// </list>
        /// </item>
        /// </list>
        /// </remarks>
        private void SpectrumAnalyzer_MouseUp(object sender, MouseEventArgs e)
        {
            mousePressed = false;
            selectingRect = false;
            for (int i = 0; i < cutoffSelectionList.Count; i++)
            {
                cutoffSelectionList[i] = false;
            }

            if(zoomRect.Width > 50 && zoomRect.Height > 20)
            {
                rectSelected = true;
                for (int i = 0; i < pointCoordinateX.Count; i++)
                {
                    if (pointCoordinateX[i] > zoomRect.Left && pointCoordinateX[i] < zoomRect.Right && pointCoordinateY[i] < zoomRect.Bottom && pointCoordinateY[i] > zoomRect.Top)
                    {
                        zoomPointCoordinateX.Add(GUI.transformCoordinate(pointCoordinateX[i], zoomRect.Left, zoomRect.Right, innerRectLeft, innerRectRight));
                        zoomPointCoordinateY.Add(GUI.transformCoordinate(pointCoordinateY[i], zoomRect.Bottom, zoomRect.Top, innerRectBottom, innerRectTop));
                    }

                }
                for (int i = 0; i < envList.Count; i++)
                {
                    envList[i] = minResponseDb;
                    peakList[i] = innerRectBottom;
                    freqList[i] = innerRectRight;
                }
                zoomRectLeft = zoomRect.Left;
                zoomRectRight = zoomRect.Right;
                zoomRectTop = zoomRect.Top;
                zoomRectBottom = zoomRect.Bottom;
                zoomRect = Rectangle.FromLTRB(0, 0, 0, 0);
            }            
        }

        /// <summary>
        /// Handles the mouse double-click event on the spectrum analyzer control.
        /// </summary>
        /// <remarks>
        /// This method checks the location of the mouse click and performs various actions based on the mouse button pressed:
        /// <list type="bullet">
        /// <item>
        /// <description><c>Middle Button</c>: Resets the resizing factors for zoom.</description>
        /// </item>
        /// <item>
        /// <description><c>Left Button</c>: Clears the zoom points and resets the envelope, peak, and frequency lists to their minimum values.</description>
        /// </item>
        /// <item>
        /// <description><c>Right Button</c>: 
        /// If the X-axis display is logarithmic and no rectangle is selected, it calculates the frequency corresponding to the clicked position,
        /// copies it to the clipboard, and stores the coordinates.
        /// If a rectangle is selected, it calculates the frequency based on the zoomed coordinates and stores them accordingly.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        private void SpectrumAnalyzer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && e.Button == MouseButtons.Middle)
            {
                resizeX = 0;
                resizeY = 0;
            }
            if (e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && e.Button == MouseButtons.Left)
            {
                rectSelected = false;
                zoomPointCoordinateX.Clear();
                zoomPointCoordinateY.Clear();
                for (int i = 0; i < envList.Count; i++)
                {
                    envList[i] = minResponseDb;
                    peakList[i] = innerRectBottom;
                    freqList[i] = innerRectRight;
                }
            }
            if (e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && e.Button == MouseButtons.Right && m_xAxisDisplay == SpectrumDisplayType.DisplayLog && !rectSelected)
            {
                //displayFrequency = Convert.ToInt32((m_samplingRate / m_fftSize) * Convert.ToSingle(Math.Pow(2, (e.Location.X - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft)))));
                displayFrequency = Convert.ToInt32(coordinateToBin(e.X));
                System.Diagnostics.Debug.Print("Frequency: " + displayFrequency);
                Clipboard.SetText(displayFrequency.ToString());
                int x = GUI.transformCoordinateInt(e.X, innerRectLeft, innerRectRight, innerRectLeft, innerRectRight);
                int y = GUI.transformCoordinateInt(e.Y, innerRectBottom, innerRectTop, innerRectBottom, innerRectTop);
                pointCoordinateX.Add(x);
                pointCoordinateY.Add(y);
                button1.Enabled = true;
            }
            else if (e.Location.X < innerRectRight && e.Location.X > innerRectLeft && e.Location.Y < innerRectBottom && e.Location.Y > innerRectTop && e.Button == MouseButtons.Right && m_xAxisDisplay == SpectrumDisplayType.DisplayLog && rectSelected)
            {
                displayFrequency = Convert.ToInt32((m_samplingRate / m_fftSize) * Convert.ToSingle(Math.Pow(2, (GUI.transformCoordinateInt(e.Location.X, innerRectLeft, innerRectRight, zoomRectLeft, zoomRectRight) - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft)))));
                System.Diagnostics.Debug.Print("Frequency: " + displayFrequency);
                Clipboard.SetText(displayFrequency.ToString());
                int x = GUI.transformCoordinateInt(e.Location.X, innerRectLeft, innerRectRight, zoomRectLeft, zoomRectRight);
                int y = GUI.transformCoordinateInt(e.Location.Y, innerRectBottom, innerRectTop, zoomRectBottom, zoomRectTop);
                pointCoordinateX.Add(x);
                pointCoordinateY.Add(y);
                zoomPointCoordinateX.Add(e.Location.X);
                zoomPointCoordinateY.Add(e.Location.Y);
                button1.Enabled = true;
            }

        }

        /// <summary>
        /// Handles the click event of button1, adding points to the tree view from the coordinate lists.
        /// </summary>
        /// <remarks>
        /// This method checks if the <c>pointCoordinateY</c> list has more points than the last recorded count. 
        /// If so, it iterates through the new points, calculates their magnitude and frequency, 
        /// and adds them as nodes in <c>treeView1</c>. 
        /// After updating the tree view, it enables the <c>btnSavePoints</c> button 
        /// and disables <c>button1</c> to prevent duplicate entries.
        /// </remarks>
        private void button1_Click(object sender, EventArgs e)
        {
            if (pointCoordinateY.Count > lastNumOfPoints)
            {
                for (int i = lastNumOfPoints; i < pointCoordinateY.Count; i++)
            {
                int index = lastNumOfPoints + i;
                int magnitude = Convert.ToInt32(minResponseDb + ((pointCoordinateY[i] - innerRectBottom) * (maxResponseDb - minResponseDb) / (innerRectTop - innerRectBottom)));
                int frequency = Convert.ToInt32(coordinateToBin(pointCoordinateX[i]));
                TreeNode n = treeView1.Nodes.Add("Point" + i);
                n.Nodes.Add(frequency.ToString() + " Hz");
                n.Nodes.Add(magnitude.ToString() + " dB");
            }
            lastNumOfPoints = pointCoordinateX.Count;
            }

            btnSavePoints.Enabled = true;
            button1.Enabled = false;
            
            this.Invalidate();
        }

        /// <summary>
        /// Handles the event that occurs after a node(point) in the tree view(marked points) is selected.
        /// </summary>
        /// <remarks>
        /// This method enables the <c>button2</c> control(Delete Point) when a node in the <c>treeView1</c> is selected.
        /// The button is enabled only if the selected node is not null and is currently selected.
        /// </remarks>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.IsSelected == true && e.Node != null)
                button2.Enabled = true;
        }

        /// <summary>
        /// Handles the click event for the button that removes the selected node from the tree view that marked points are stored.
        /// </summary>
        /// <remarks>
        /// This method removes the currently selected node from the <c>treeView1</c> control. 
        /// If there are no remaining nodes in the tree view after the removal, the <c>btnSavePoints</c> button is disabled. 
        /// The <c>button2</c> itself is also disabled after the action to prevent further attempts to remove nodes.
        /// </remarks>
        private void button2_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Remove(treeView1.SelectedNode);
            if (treeView1.Nodes.Count == 0)
                btnSavePoints.Enabled = false;

            button2.Enabled = false;
        }

        /// <summary>
        /// Handles the click event for the button that saves marked points' data to an XML file.
        /// </summary>
        /// <remarks>
        /// This method prompts the user to select a folder where the points data will be saved as an XML file. 
        /// If the <c>treeView1</c> has nodes, it creates an XML document with the root element "Points". 
        /// For each node in the tree view, it adds attributes for slope, magnitude, frequency, and name, 
        /// and appends them to the XML document. The document is then saved in the selected folder.
        /// </remarks>
        private void btnSavePoints_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (treeView1.TopNode != null && treeView1.Nodes != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    XmlElement xmlRoot = xmlDocument.CreateElement("Points");
                    int numOfNodes = treeView1.Nodes.Count;

                    foreach (TreeNode node in treeView1.Nodes)
                    {
                        xmlRoot.SetAttribute("slope", txtSlope.Text);
                        xmlRoot.SetAttribute("magnitude" + node.Index, node.LastNode.Text);
                        xmlRoot.SetAttribute("frequency" + node.Index, node.FirstNode.Text);
                        xmlRoot.SetAttribute("name" + node.Index, node.Text);
                        xmlDocument.AppendChild(xmlRoot);
                        nameForSelectedPoints += node.FirstNode.Text + "_";
                    }
                    xmlDocument.Save(folderBrowserDialog1.SelectedPath + @"\" + nameForSelectedPoints + ".xml");
                }
            }           

        }

        /// <summary>
        /// Handles the click event for the button that removes the last points from the coordinate lists and spectrum.
        /// </summary>
        /// <remarks>
        /// This method checks if there are any points in the <c>pointCoordinateX</c> 
        /// and <c>pointCoordinateY</c> lists and removes the last point if they are not empty. 
        /// It also removes the last point from <c>zoomPointCoordinateX</c> and <c>zoomPointCoordinateY</c> 
        /// if they contain points. The <c>lastNumOfPoints</c> variable is decremented to reflect the 
        /// removal, and is reset to zero if it goes below zero.
        /// </remarks>
        private void button3_Click(object sender, EventArgs e)
        {
            if (pointCoordinateX.Count != 0 && pointCoordinateY.Count != 0)
            {
                pointCoordinateX.RemoveAt(pointCoordinateX.Count - 1);
                pointCoordinateY.RemoveAt(pointCoordinateY.Count - 1);
                lastNumOfPoints--;
            }
            if (zoomPointCoordinateX.Count != 0 && zoomPointCoordinateY.Count != 0)
            {
                zoomPointCoordinateX.RemoveAt(zoomPointCoordinateX.Count - 1);
                zoomPointCoordinateY.RemoveAt(zoomPointCoordinateY.Count - 1);
            }
            if (lastNumOfPoints < 0)
                lastNumOfPoints = 0;
        }

        /// <summary>
        /// Converts a frequency bin index to a horizontal coordinate for spectrum display.
        /// </summary>
        /// <param name="bin">The index of the frequency bin to be converted.</param>
        /// <returns>The horizontal coordinate corresponding to the frequency bin.</returns>
        private float binToCoordinate(int bin)
        {
            float coordinate = innerRectLeft + ((Convert.ToSingle(Math.Log((bin * m_fftSize) / m_samplingRate, 2) * ((innerRectRight - innerRectLeft) / (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)))))));
            return coordinate;
        }

        /// <summary>
        /// Converts a horizontal coordinate from the spectrum display back to a frequency bin index.
        /// </summary>
        /// <param name="coordinate">The horizontal coordinate to be converted.</param>
        /// <returns>The corresponding frequency bin index.</returns>
        private float coordinateToBin(int coordinate)
        {
            float bin = ((float)m_samplingRate / (float)m_fftSize) * Convert.ToSingle(Math.Pow(2, (coordinate - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft))));
            return bin;
        }

        /// <summary>
        /// Converts a horizontal coordinate from the spectrum display back to a frequency bin index.
        /// </summary>
        /// <param name="coordinate">The horizontal coordinate as a float to be converted.</param>
        /// <returns>The corresponding frequency bin index.</returns>
        private float coordinateToBin(float coordinate)
        {
            float bin = ((float)m_samplingRate / (float)m_fftSize) * Convert.ToSingle(Math.Pow(2, (coordinate - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft))));
            return bin;
        }

        /// <summary>
        /// Converts a vertical coordinate from the spectrum display to a decibel (dB) value.
        /// </summary>
        /// <param name="coordinate">The vertical coordinate to be converted.</param>
        /// <returns>The corresponding decibel (dB) value.</returns>
        private int coordinateToDB(int coordinate)
        {
            int desibel = minResponseDb + ((coordinate - innerRectBottom) * (maxResponseDb - minResponseDb) / (innerRectTop - innerRectBottom));
            return desibel;
        }

        /// <summary>
        /// Converts a vertical coordinate from the spectrum display to a decibel (dB) value.
        /// </summary>
        /// <param name="coordinate">The vertical coordinate as a float to be converted.</param>
        /// <returns>The corresponding decibel (dB) value as a float.</returns>
        private float coordinateToDB(float coordinate)
        {
            float desibel = Convert.ToSingle(minResponseDb + ((coordinate - innerRectBottom) * (maxResponseDb - minResponseDb) / (innerRectTop - innerRectBottom)));
            return desibel;
        }

        /// <summary>
        /// Converts a horizontal coordinate from the spectrum display to a corresponding index value.
        /// </summary>
        /// <param name="coordinate">The horizontal coordinate to be converted.</param>
        /// <returns>The corresponding index value as an integer.</returns>
        private int coordinateToIndex(int coordinate)
        {
            float fIndex = Convert.ToSingle(Math.Pow(2, (coordinate - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft))));
            int index = Convert.ToInt32(fIndex);
            return index;
        }

        /// <summary>
        /// Calculates the derivative (slope) between two points in a 2D space.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <returns>The derivative (slope) between the two points as a float.</returns>
        private float findDerivative(float x1, float y1, float x2, float y2)
        {
            float derivative = (y2 - y1) / (x2 - x1);
            return derivative;
        }

        /// <summary>
        /// Toggles the peak detection feature when the associated checkbox state changes.
        /// </summary>
        /// <remarks>
        /// This method updates the <c>peakChecked</c> boolean flag based on the current 
        /// state of the checkbox. If the checkbox is checked, peak detection will be enabled; 
        /// if unchecked, it will be disabled.
        /// </remarks>
        private void checkBoxPeak_CheckedChanged(object sender, EventArgs e)
        {
            if (peakChecked == false)
                peakChecked = true;
            else
                peakChecked = false;
        }

        /// <summary>
        /// Handles updating the minimum response dB value.
        /// </summary>
        /// <remarks>
        /// This method sets the <c>minResponseDb</c> to the current value of the scrollbar, 
        /// and updates the <c>envList</c>, <c>peakList</c>, and <c>freqList</c> accordingly. 
        /// It then refreshes the display to reflect these changes.
        /// </remarks>
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            minResponseDb = vScrollBar1.Value;
            for (int i = 0; i < envList.Count; i++)
            {
                envList[i] = minResponseDb;
                peakList[i] = innerRectBottom;
                freqList[i] = innerRectRight;
            }
                
            this.update();
            this.Invalidate();
        }

        /// <summary>
        /// Handles the scroll event for the second vertical scrollbar, updating the Y-axis spacing in decibels.
        /// </summary>
        /// <remarks>
        /// This method sets the <c>yAxisSpacingDb</c> to the current value of the scrollbar and refreshes the display 
        /// to reflect the updated spacing.
        /// </remarks>
        private void vScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            yAxisSpacingDb = vScrollBar2.Value;
            this.update();
            this.Invalidate();
        }

        /// <summary>
        /// Resets the peak values and environmental data for the spectrum display.
        /// </summary>
        /// <remarks>
        /// This method sets all values in the <c>envList</c> to the minimum response dB, 
        /// resets the peak and frequency lists to their initial positions, 
        /// and updates the display accordingly.
        /// </remarks>
        private void btnPeakReset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < envList.Count; i++)
            {
                envList[i] = minResponseDb;
                peakList[i] = innerRectBottom;
                freqList[i] = innerRectRight;
            }
            peakCount = 0;
            this.update();
            this.Invalidate();
        }

        /// <summary>
        /// Captures the current spectrum and saves it as a JPG image file.
        /// </summary>
        /// <remarks>
        /// This method creates a bitmap of the current spectrum, captures its screen representation, 
        /// and prompts the user to select a location and filename to save the image as a JPG file.
        /// The default filename is set to "test" and the file extension is ensured to be ".jpg".
        /// </remarks>
        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap bm = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bm))
            {
                g.CopyFromScreen(this.PointToScreen(Point.Empty).X,
                    this.PointToScreen(Point.Empty).Y,
                    0,
                    0,
                    this.Size,
                    CopyPixelOperation.SourceCopy);
            }

            saveDialog.FileName = "test";
            saveDialog.DefaultExt = "jpg";
            saveDialog.Filter = "JPG images (*.jpg)|*.jpg";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = saveDialog.FileName;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != "jpg")
                    fileName = fileName + ".jpg";

                bm.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                //Clipboard.SetImage((Image)bm);
            }
        }

        /// <summary>
        /// Initializes the frequency table and related arrays based on the specified size.
        /// </summary>
        /// <param name="size">The size of the frequency table and related arrays to be initialized.</param>
        /// <remarks>
        /// This method calculates the frequency values and initializes the frequency response and filter applied signal arrays.
        /// It also prepares a complex response array with default values.
        /// </remarks>
        public void InitializeFrequencyTable(int size)
        {
            m_frequencyTable = new double[size];
            m_frequencyResponseOverall = new double[size];
            m_filterAppliedSignal = new double[size];
            cResponseArray = new ComplexNumber[size];
            responseArray = new double[size];

            double finalFrequency = 0.49;
            double initialFrequency = finalFrequency / Math.Pow(2, 128.0 / 12.0);
            double multiplier = Math.Pow(finalFrequency / initialFrequency, 1.0 / Convert.ToDouble(size - 1));

            for (int i = 0; i < size; i++)
            {
                m_frequencyTable[i] = initialFrequency;
                initialFrequency *= multiplier;
            }
            for (int i = 0; i < size; i++)
            {
                cResponseArray[i] = new ComplexNumber(1.0f, 0.0f);
            }
        }

        /// <summary>
        /// Initializes the filter resonance table with calculated resonance values.
        /// </summary>
        /// <remarks>
        /// This method fills the resonance table with values ranging from a specified initial 
        /// resonance to a final resonance, spaced logarithmically over 128 entries.
        /// </remarks>
        public void InitializeResonanceTable()
        {
            m_filterResonanceTable = new double[128];
            double final = 100;
            double initial = 0.5;
            double multiplier = Math.Pow(final / initial, 1.0 / 127.0);

            for (int i = 0; i < 128; i++)
            {
                m_filterResonanceTable[i] = Math.Sqrt(initial);
                initial *= multiplier;
            }

        }

        /// <summary>
        /// Creates a new filter panel and initializes associated data structures for filter response processing.
        /// </summary>
        /// <remarks>
        /// This method initializes arrays and objects necessary for filter processing, including
        /// magnitude arrays, response arrays, and graphical representations. It also adds the new 
        /// filter panel to the list of filters and updates the UI to reflect the addition.
        /// </remarks>
        private void button5_Click(object sender, EventArgs e)
        {
            FilterPanelUC filter = new FilterPanelUC();
            float[] magnitudeArray = new float[m_fftSize / 2 + 1];
            double[] responseArray = new double[m_fftSize / 2 + 1];
            ComplexNumber[] cResponseAray = new ComplexNumber[m_fftSize / 2 + 1];
            Rectangle rect = new Rectangle();
            bool cutoffSelection = false;
            Pen pen = new Pen(Color.FromArgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)));
            GraphicsPath gPath = new GraphicsPath();

            gPathList.Add(gPath);
            filterPenList.Add(pen);
            cutoffSelectionList.Add(cutoffSelection);
            rectangleList.Add(rect);
            cResponseList.Add(cResponseAray);
            responseList.Add(responseArray);
            m_transformedMagnitudesList.Add(magnitudeArray);
            filterList.Add(filter);

            listBox1.Items.Add("Filter" + numberOfFilters);
            numberOfFilters++;
            listBox1.Enabled = true;
        }

        /// <summary>
        /// Handles the event when a new filter is selected from the filter list.
        /// </summary>
        /// <remarks>
        /// This method clears the current controls from <c>panel1</c> and adds the selected filter panel 
        /// from the <c>filterList</c> to the panel, updating the UI to display the selected filter's settings.
        /// </remarks>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;

            panel1.Controls.Clear();
            panel1.Controls.Add(filterList[selectedIndex]);
        }

        /// <summary>
        /// Opens a folder browser dialog to select a directory and saves the current filter settings as an XML file.
        /// </summary>
        /// <remarks>
        /// This method displays a folder browser dialog and, if the user selects a directory and there are filters present,
        /// it creates an XML document containing the settings for each filter in the <c>filterList</c>. The XML document
        /// includes attributes for filter mode, type, cutoff frequency, gain, and resonance for each filter. The resulting 
        /// XML file is saved in the selected directory with a name derived from the filter types and modes.
        /// </remarks>
        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && filterList.Count > 0)
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement xmlRoot = xmlDocument.CreateElement("Filters");
                int numberOfNodes = filterList.Count;

                    for (int i = 0; i < filterList.Count; i++)
                    {
                        float cutoff = ((filterList[i].FilterCutoff + 13) * (m_samplingRate / m_fftSize)) - 300;
                        xmlRoot.SetAttribute("FilterMode" + i, filterList[i].FilterMode.ToString());
                        xmlRoot.SetAttribute("FilterType" + i, filterList[i].FilterType.ToString());
                        xmlRoot.SetAttribute("Cutoff" + i, cutoff.ToString());
                        xmlRoot.SetAttribute("Gain" + i, filterList[i].FilterGain.ToString());
                        xmlRoot.SetAttribute("Resonance" + i, filterList[i].FilterResonance.ToString());

                        nameForFilter += filterList[i].FilterType.ToString() + filterList[i].FilterMode.ToString() + "_";

                        xmlDocument.AppendChild(xmlRoot);
                    }
                                    
                xmlDocument.Save(folderBrowserDialog1.SelectedPath + @"\" + nameForFilter + ".xml");
            }
        }

        /// <summary>
        /// Determines whether a given number is odd.
        /// </summary>
        /// <param name="number">The integer number to check.</param>
        /// <returns>
        /// <c>true</c> if the number is odd; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method uses the modulo operator to check if the number is divisible by 2.
        /// If it is not 
        public bool checkOdd(int number)
        {
            if (number % 2 == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Determines whether a given number is even.
        /// </summary>
        /// <param name="number">The integer number to check.</param>
        /// <returns>
        /// <c>true</c> if the number is even; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method uses the modulo operator to check if the number is divisible by 2.
        /// If it is divisible, the number is considered even.
        /// </remarks>
        public bool checkEven(int number)
        {
            if (number % 2 == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Handles the click event of the "Save Peaks" button.
        /// Saves the magnitudes of the peaks and their corresponding frequency ratios to a file.
        /// </summary>
        /// <remarks>
        /// This method checks if there are any peak magnitudes in <c>peakMagList</c>. If so, it opens a text writer to 
        /// save the information about harmonics, including frequency ratios and magnitudes. The method distinguishes 
        /// between odd and even frequency ratios and applies appropriate calculations for coefficients that are appended
        /// to a polynomial string. The resulting data is written to a specified file path. Each entry for harmonics 
        /// is formatted and logged into the file, followed by the polynomial representation of the magnitudes.
        /// </remarks>
        private void btnSavePeaks_Click(object sender, EventArgs e)
        {
            int countOdd = 0;
            int countEven = 0;
            //DialogResult result = folderBrowserDialog1.ShowDialog();
            string polynomial = "";
            if (/*result == DialogResult.OK && */peakMagList.Count > 0)
            {
                using (TextWriter writer = new StreamWriter(pathForDirectRecording, true))
                {
                    writer.WriteLine("///" + peakCount.ToString() + "///");
                    for (int i = 0; i < peakMagList.Count; i++)
                    {
                        if(coordinateToDB((int)peakMagList[i]) >= coordinateToDB((int)firstHarmonicMag) - 40 || coordinateToDB((int)peakMagList[i]) == coordinateToDB((int)firstHarmonicMag))
                        {
                            int mag = coordinateToDB((int)peakMagList[i]);
                            float freq = coordinateToBin(peakIndexList[i]);
                            float freqRatio = freq / coordinateToBin(peakIndexList[0]);

                            if(checkOdd(Convert.ToInt32(freqRatio)))
                            {
                                writer.WriteLine("harmonic" + freqRatio.ToString("0") + ": " + freq.ToString("0") + " magnitude" + i.ToString() + ": " + mag.ToString());

                                float coefficient = checkEven(countOdd) ? Convert.ToSingle(Math.Pow(10, (coordinateToDB((int)peakMagList[i]) - coordinateToDB((int)firstHarmonicMag)) / 20.0f)) : -Convert.ToSingle(Math.Pow(10, (coordinateToDB((int)peakMagList[i]) - coordinateToDB((int)firstHarmonicMag)) / 20.0f));

                                polynomial += " " + coefficient.ToString("0.000") + " * x^" + freqRatio.ToString("0") + " + ";

                                countOdd++;
                            }
                            if(checkEven(Convert.ToInt32(freqRatio)))
                            {
                                writer.WriteLine("harmonic" + freqRatio.ToString("0") + ": " + freq.ToString("0") + " magnitude" + i.ToString() + ": " + mag.ToString());

                                float coefficient = checkEven(countEven) ? Convert.ToSingle(Math.Pow(10, (coordinateToDB((int)peakMagList[i]) - coordinateToDB((int)firstHarmonicMag)) / 20.0f)) : -Convert.ToSingle(Math.Pow(10, (coordinateToDB((int)peakMagList[i]) - coordinateToDB((int)firstHarmonicMag)) / 20.0f));

                                polynomial += " " + coefficient.ToString("0.000") + " * x^" + freqRatio.ToString("0") + " + ";

                                countEven++;
                            }

                            /*writer.WriteLine("harmonic" + freqRatio.ToString("0") + ": " + freq.ToString("0") + " magnitude" + i.ToString() + ": " + mag.ToString());

                            float coefficient = Convert.ToSingle(Math.Pow(10, (coordinateToDB((int)peakMagList[i]) - coordinateToDB((int)firstHarmonicMag)) / 20.0f));

                            polynomial += " " + coefficient.ToString("0.000") + " * x^" + freqRatio.ToString("0") + " + ";*/
                        }
                        
                    }
                    writer.WriteLine(polynomial);
                    writer.WriteLine("///");
                    peakCount++;

                }
            }
        }

        /// <summary>
        /// Converts two bytes into a double-precision floating-point number
        /// representing a value in the range of -1 to just below 1.
        /// </summary>
        /// <param name="firstByte">The first byte (least significant byte) in the conversion.</param>
        /// <param name="secondByte">The second byte (most significant byte) in the conversion.</param>
        /// <returns>A double representing the combined value of the two bytes, normalized to the range -1 to just below 1.</returns>
        /// <remarks>
        /// This method combines the two input bytes into a 16-bit signed short value using little-endian format.
        /// The resulting short is then divided by 32768.0 to convert the value to a double in the specified range.
        /// </remarks>
        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0;
        }

        /// <summary>
        /// Captures the current waveform displayed in the designated rectangle area of the screen
        /// and saves it as a JPEG image file.
        /// </summary>
        /// <remarks>
        /// This method creates a bitmap image of the specified area of the input waveform. It uses the <c>CopyFromScreen</c>
        /// method to capture the pixel data and allows the user to specify the file name and 
        /// location for saving the image in JPEG format. If the user does not provide a .jpg extension,
        /// it appends the extension automatically.
        /// </remarks>
        private void btnCaptureWaveform_Click_1(object sender, EventArgs e)
        {
            Bitmap bm = new Bitmap(signalRectWidth, signalRectHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bm))
            {
                g.CopyFromScreen(this.PointToScreen(Point.Empty).X + signalRectLeft,
                    this.PointToScreen(Point.Empty).Y + signalRectTop,
                    0,
                    0,
                    new Size(signalRectWidth, signalRectHeight),
                    CopyPixelOperation.SourceCopy);
            }

            saveDialog.FileName = "test";
            saveDialog.DefaultExt = "jpg";
            saveDialog.Filter = "JPG images (*.jpg)|*.jpg";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = saveDialog.FileName;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != "jpg")
                    fileName = fileName + ".jpg";

                bm.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                //Clipboard.SetImage((Image)bm);
            }
        }

        /// <summary>
        /// Enables the detection of the first harmonic when the associated button is clicked.
        /// </summary>
        /// <remarks>
        /// This method sets the <c>firstHarmonicDetectEnabled</c> flag to <c>true</c>,
        /// allowing the application to begin processing and detecting the first harmonic
        /// in the audio signal. This should be connected to the appropriate audio analysis 
        /// functionality in the application.
        /// </remarks>
        private void button4_Click_1(object sender, EventArgs e)
        {
            firstHarmonicDetectEnabled = true;
        }

        /// <summary>
        /// Toggles the application of filters when the associated checkbox state changes.
        /// </summary>
        /// <remarks>
        /// This method updates the <c>filtersApplied</c> boolean flag based on the current 
        /// state of the checkbox. If the checkbox is checked, filters will be applied 
        /// to the audio processing. If unchecked, filters will be removed.
        /// </remarks>
        private void checkBoxApplyFilters_CheckedChanged(object sender, EventArgs e)
        {
            if (filtersApplied == false)
                filtersApplied = true;
            else
                filtersApplied = false;
        }

        /// <summary>
        /// Handles the resize event of the SpectrumAnalyzer control
        /// </summary>
        /// <remarks>
        /// This method clears the coordinate lists for point marking and zooming when the 
        /// SpectrumAnalyzer is resized, ensuring that the points and zoom area are reset 
        /// after the control's size changes.
        /// </remarks>
        private void SpectrumAnalyzer_Resize(object sender, EventArgs e)
        {
            pointCoordinateX.Clear();
            pointCoordinateY.Clear();
            zoomPointCoordinateX.Clear();
            zoomPointCoordinateY.Clear();
        }
    }
}