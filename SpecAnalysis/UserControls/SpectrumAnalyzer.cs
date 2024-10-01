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

        private static int m_samplingRate = 44100;
        private static int m_fftSize = 2048;

        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string relativePathForDirectRecording = "..\\peaks";
        private string pathForDirectRecording = Path.GetFullPath(desktopPath + relativePathForDirectRecording);

        private double[] m_frequencyResponseOverall;
        private double[] m_filterAppliedSignal;
        private double[] m_frequencyTable;
        private double[] m_filterResonanceTable;
        private ComplexNumber[] cResponseArray;
        private double[] responseArray;
        private ComplexNumber cOne = new ComplexNumber(1.0f, 0.0f);
        private string waveType = "";

        private List<GraphicsPath> gPathList = new List<GraphicsPath>();
        private List<Pen> filterPenList = new List<Pen>();
        private List<bool> cutoffSelectionList = new List<bool>();
        private List<Rectangle> rectangleList = new List<Rectangle>();
        private List<float[]> m_transformedMagnitudesList = new List<float[]>();
        private List<ComplexNumber[]> cResponseList = new List<ComplexNumber[]>();
        private List<double[]> responseList = new List<double[]>();
        private List<FilterPanelUC> filterList= new List<FilterPanelUC>();

        private FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
        private OpenFileDialog fileDialog1 = new OpenFileDialog();
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
        private float[] tempTransformedMagnitudes;
        private float[] m_wavFileEnvelope;
        private bool m_isPeriodic = false;

        private float[] m_transformedFrequencies;
        private float[] m_envelopeArray = new float[m_fftSize / 2 + 1];

        private SpectrumDisplayType m_xAxisDisplay = SpectrumDisplayType.DisplayLog;
        private SpectrumDisplayType m_yAxisDisplay = SpectrumDisplayType.DisplayLog;

        private Pen zoomPen = new Pen(Color.Maroon, 1);
        private Pen gridPen = new Pen(Color.FromArgb(128, Color.Silver), 1);
        private Pen pointToPointPen = new Pen(Color.DarkOrange, 1);
        private Brush backBrush = new SolidBrush(Color.Silver);
        private Brush pointBackBrush = new SolidBrush(Color.White);
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
        private int cutoffCount = 0;
        private float currentCutoffFrequency = 0;
        private bool firstHarmonicLogged = false;
        private bool sineFrequencyChanged = false;

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
            GraphicsPath gPathEnvelope = new GraphicsPath();
            GraphicsPath gPathPeriod = new GraphicsPath();

            Rectangle frameRect = Rectangle.FromLTRB(this.Left + 10, this.Top + 150, this.Right - 10 + resizeX, this.Bottom - 200 + resizeY);
            Rectangle innerRect = Rectangle.FromLTRB(frameRect.Left + 50, frameRect.Top + 30, frameRect.Right - 35, frameRect.Bottom - 30);

            GUI.DrawRoundedRectangle(g, frameRect, framePen, 4, 4, 4, 4);
            g.DrawRectangle(innerPen, innerRect);
            g.DrawRectangle(zoomPen, zoomRect);           

            innerRectLeft = innerRect.Left;
            innerRectRight = innerRect.Right;
            innerRectBottom = innerRect.Bottom;
            innerRectTop = innerRect.Top;
            innerRectHeight = innerRect.Height;
            innerRectWidth = innerRect.Width;

            frameRectBottom = frameRect.Bottom;
            frameRectLeft = frameRect.Left;
            frameRectRight = frameRect.Right;
            frameRectTop = frameRect.Top;
            frameRectWidth = frameRect.Width;
            frameRectHeight = frameRect.Height;
            frameRectX = frameRect.X;
            frameRectY = frameRect.Y;
            frameRectSize = frameRect.Size;

            
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

            Rectangle signalRect = Rectangle.FromLTRB(btnCaptureWaveform.Right + 35, this.Top + 10, frameRect.Right, frameRect.Top - 10);

            signalRectBottom = signalRect.Bottom;
            signalRectHeight = signalRect.Bottom;
            signalRectLeft = signalRect.Left;
            signalRectRight = signalRect.Right;
            signalRectTop = signalRect.Top;
            signalRectWidth = signalRect.Width;

            lblWavefile.Location = new Point(
                signalRect.Left + 5,
                signalRect.Top
                );


            g.DrawRectangle(innerPen, signalRect);

            float displayScreenYMax = innerRect.Top;
            float displayScreenYMin = innerRect.Bottom;
            float displayScreenXMax = innerRect.Right;
            float displayScreenXMin = innerRect.Left;

            g.DrawString("X: " + coordinateToBin(cursorX).ToString("0") + " Hz", magFont, measureBrush, innerRectLeft, frameRectTop);
            g.DrawString("Y: " + coordinateToDB(cursorY).ToString() + " dB", magFont, measureBrush, innerRectLeft, frameRectTop + 12);

            indexList.Clear();
            magList.Clear();

            periodIndexList.Clear();
            periodMagList.Clear();

            peakIndexList.Clear();
            peakMagList.Clear();

            for (int i = 0; i < realTimeOutputs.Length; i++)
            {
                realTimeIndexes[i] = GUI.transformCoordinateFloat(i, 0, realTimeOutputs.Length, signalRect.Left, signalRect.Right);
                realTimeMagnitudes[i] = GUI.transformCoordinateFloat(realTimeOutputs[i], -1, 1, signalRect.Bottom, signalRect.Top);
            }

            for (int i = 0; i < realTimeOutputs.Length - 1; i++)
            {
                gPathSignal.AddLine(realTimeIndexes[i], realTimeMagnitudes[i], realTimeIndexes[i + 1], realTimeMagnitudes[i + 1]);
            }
        
            for (int i = -2; i <= 2; i++)
            {
                float m = i / 2.0f;
                float k = GUI.transformCoordinateFloat(m, -1.0f, 1.0f, signalRect.Bottom, signalRect.Top);
                g.DrawString(m.ToString(), magFont, measureBrush, signalRect.Left - 28, k);
            }
            if (filterList.Count > 0)
            {
                for (int i = 0; i < responseList.Count; i++)
                {
                    responseList[i] = filterList[i].FrequencyResponse;
                    cResponseList[i] = filterList[i].ComplexResponse;
                }

            }

            if (filterList.Count > 0)
            {
                for (int i = 0; i < filterList.Count; i++)
                {
                    if (filterList[i].ColorChanged)
                        filterPenList[i] = filterList[i].FilterPen;
                }
            }

            for (int i = 0; i < m_transformedLogIndexes.Length - 1; i++)
            {
                m_transformedLogIndexes[i + 1] = GUI.transformCoordinateFloat(m_shiftedIndexes[i + 1], Convert.ToSingle(Math.Log(1, 2)), Convert.ToSingle(Math.Log(m_fftSize / 2 + 1, 2)), displayScreenXMin, displayScreenXMax);
                m_transformedLinearIndexes[i + 1] = GUI.transformCoordinateFloat(m_outputIndexes[i + 1], 1, m_fftSize / 2 + 1, displayScreenXMin, displayScreenXMax);
            }

            for (int i = 0; i < m_fftSize / 2 + 1; i++)
            {
                for (int k = 0; k < cResponseList.Count; k++)
                {
                    cResponseArray[i] *= cResponseList[k][i];
                    responseArray[i] += responseList[k][i];

                    if (responseArray[i] > maxResponseDb)
                        responseArray[i] = maxResponseDb;
                    else
                        if (responseArray[i] < minResponseDb)
                        responseArray[i] = minResponseDb;
                }
                m_frequencyResponseOverall[i] = responseArray[i];
                //m_frequencyResponseOverall[i] = cResponseArray[i].GetLogMagnitude(minResponseDb, maxResponseDb);
                cResponseArray[i] *= complexOutputs[i]; // complexOutputs = fftOutput
                m_filterAppliedSignal[i] = cResponseArray[i].GetLogMagnitude(minResponseDb, maxResponseDb);
            }
            for (int i = 0; i < cResponseArray.Length; i++)
            {
                cResponseArray[i] = cOne;
                responseArray[i] = 0.0;
            }
            for (int i = 0; i < m_fftSize / 2 + 1; i++)
            {
                m_transformedMagnitudes[i] = filtersApplied ? GUI.transformCoordinateFloat((float)m_filterAppliedSignal[i], minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top) :
                    GUI.transformCoordinateFloat(m_envelopeArray[i], minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top);
                m_transformedMagnitudesOverall[i] = GUI.transformCoordinateFloat((float)m_frequencyResponseOverall[i], minResponseDb, maxResponseDb, innerRectBottom, innerRectTop);
            }
            if (responseList.Count > 0)
            {
                for (int j = 0; j < responseList.Count; j++)
                {
                    for (int i = 0; i < m_fftSize / 2 + 1; i++)
                    {
                        m_transformedMagnitudesList[j][i] = GUI.transformCoordinateFloat((float)responseList[j][i], minResponseDb, maxResponseDb, innerRectBottom, innerRectTop);
                    }
                }
            }


            // not zoomed
            if (!rectSelected)
            {
                // periodic signal peak detection, might need improvements!
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

                // logarithmic y axis grid
                for (int j = minResponseDb; j <= maxResponseDb; j += yAxisSpacingDb)
                {
                    int y = GUI.transformCoordinate(j, minResponseDb, maxResponseDb, innerRect.Bottom, innerRect.Top);
                    g.DrawLine(gridPen, innerRect.Left, y, innerRect.Right, y);
                    g.DrawString(j.ToString() + " dB", magFont, measureBrush, frameRect.Left, y - 5);
                }
                // logarithmic x axis grid
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
                // linear x axis grid
                if (m_xAxisDisplay == SpectrumDisplayType.DisplayLinear)
                {
                    for (int i = minFrequencyHz; i < maxFrequencyHz; i += linearFrequencySpacing)
                    {
                        int x = GUI.transformCoordinateInt(i, minFrequencyHz, maxFrequencyHz, innerRect.Left, innerRect.Right);
                        g.DrawLine(gridPen, x, innerRect.Bottom, x, innerRect.Top);
                        g.DrawString(i.ToString() + " Hz", freqFont, measureBrush, x - 12, frameRect.Bottom - 15);
                    }
                }

                // draw selected points
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
                // calculate peaks
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

                // draw the lines
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
            // zoomed
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
                // y axis grid
                for (int j = coordinateToDB(zoomRectBottom); j <= coordinateToDB(zoomRectTop); j += yAxisSpacingDb)
                {
                    int y = GUI.transformCoordinate(j, coordinateToDB(zoomRectBottom), coordinateToDB(zoomRectTop), innerRect.Bottom, innerRect.Top);
                    g.DrawLine(gridPen, innerRect.Left, y, innerRect.Right, y);
                    g.DrawString(j.ToString() + " dB", magFont, measureBrush, frameRect.Left, y - 5);
                }
                // x axis grid
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
                // draw selected points
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
                // calculate peaks
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

            g.DrawString(displayFrequency.ToString() + " Hz copied to the clipboard", freqFont, measureBrush, frameRect.Right - 220, frameRect.Top + 10);

            // draw peaks
            for (int i = 0; i < peakList.Count - 1; i++)
            {
                if (peakList.Count >= 2 && peakChecked == true && mousePressed == false && peakList[i] < innerRectBottom && peakList[i + 1] < innerRectBottom && freqList[i] < innerRectRight && freqList[i + 1] < innerRectRight && freqList[i] >= innerRectLeft && freqList[i + 1] > innerRectLeft)
                    gPathPeak.AddLine(freqList[i], peakList[i], freqList[i + 1], peakList[i + 1]);
            }
            g.DrawPath(Pens.SteelBlue, gPathPeak);
            g.DrawPath(drawPen, gPath);
            g.DrawPath(filterPenOverall, gPathOverall);
            g.DrawPath(drawPen, gPathSignal);
            g.DrawPath(Pens.Green, gPathEnvelope);
            //g.DrawPath(Pens.Red, gPathPeriod);

            if (gPathList.Count > 0)
            {
                for (int i = 0; i < gPathList.Count; i++)
                {
                    g.DrawPath(filterPenList[i], gPathList[i]);
                }
            }
            if (gPathList.Count > 0)
            {
                for (int i = 0; i < gPathList.Count; i++)
                {
                    gPathList[i].Reset();
                }
            }
            gPathOverall.Dispose();
            gPath.Dispose();
            gPathPeak.Dispose();
            gPathSignal.Dispose();
            gPathEnvelope.Dispose();
            gPathPeriod.Dispose();
        }        

        public void SpectrumFirstHarmonicUpdate()
        {
            firstHarmonicDetectEnabled = true;
        }
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
            tempTransformedMagnitudes = new float[fftSize / 2 + 1];
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.IsSelected == true && e.Node != null)
                button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Remove(treeView1.SelectedNode);
            if (treeView1.Nodes.Count == 0)
                btnSavePoints.Enabled = false;

            button2.Enabled = false;
        }

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
        private float binToCoordinate(int bin)
        {
            float coordinate = innerRectLeft + ((Convert.ToSingle(Math.Log((bin * m_fftSize) / m_samplingRate, 2) * ((innerRectRight - innerRectLeft) / (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)))))));
            return coordinate;
        }
        private float coordinateToBin(int coordinate)
        {
            float bin = ((float)m_samplingRate / (float)m_fftSize) * Convert.ToSingle(Math.Pow(2, (coordinate - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft))));
            return bin;
        }
        private float coordinateToBin(float coordinate)
        {
            float bin = ((float)m_samplingRate / (float)m_fftSize) * Convert.ToSingle(Math.Pow(2, (coordinate - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft))));
            return bin;
        }
        private int coordinateToDB(int coordinate)
        {
            int desibel = minResponseDb + ((coordinate - innerRectBottom) * (maxResponseDb - minResponseDb) / (innerRectTop - innerRectBottom));
            return desibel;
        }
        private float coordinateToDB(float coordinate)
        {
            float desibel = Convert.ToSingle(minResponseDb + ((coordinate - innerRectBottom) * (maxResponseDb - minResponseDb) / (innerRectTop - innerRectBottom)));
            return desibel;
        }
        private int coordinateToIndex(int coordinate)
        {
            float fIndex = Convert.ToSingle(Math.Pow(2, (coordinate - innerRectLeft) * (Convert.ToSingle(Math.Log(m_fftSize / 2, 2)) / (innerRectRight - innerRectLeft))));
            int index = Convert.ToInt32(fIndex);
            return index;
        }
        private float findDerivative(float x1, float y1, float x2, float y2)
        {
            float derivative = (y2 - y1) / (x2 - x1);
            return derivative;
        }

        private void checkBoxPeak_CheckedChanged(object sender, EventArgs e)
        {
            if (peakChecked == false)
                peakChecked = true;
            else
                peakChecked = false;
        }

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

        private void vScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            yAxisSpacingDb = vScrollBar2.Value;
            this.update();
            this.Invalidate();
        }

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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;

            panel1.Controls.Clear();
            panel1.Controls.Add(filterList[selectedIndex]);
        }

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
        public bool checkOdd(int number)
        {
            if (number % 2 == 0)
                return false;
            else
                return true;
        }
        public bool checkEven(int number)
        {
            if (number % 2 == 0)
                return true;
            else
                return false;
        }
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
            /*if (peakMagList.Count > 0)
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement xmlRoot = xmlDocument.CreateElement("Peaks");
                int numberOfNodes = peakMagList.Count;

                for (int i = 0; i < peakMagList.Count; i++)
                {
                    int mag = coordinateToDB((int)peakMagList[i]);
                    float freq = coordinateToBin(peakIndexList[i]);
                    float freqRatio = freq / coordinateToBin(peakMagList[0]);


                    xmlRoot.SetAttribute("frequency" + i, freq.ToString("0"));
                    xmlRoot.SetAttribute("magnitude" + i, mag.ToString());

                    xmlDocument.AppendChild(xmlRoot);
                }

                nameForPeaks = coordinateToDB((int)peakMagList[0]).ToString() + "_" + coordinateToBin(peakIndexList[0]).ToString("0");

                xmlDocument.Save(@"C:\Users\kv331audio\Desktop\" + @"\" + peakCount.ToString() + ".xml");
                peakCount++;
                //xmlDocument.Save(folderBrowserDialog1.SelectedPath + @"\" + nameForPeaks + ".xml");
            }*/
        }
        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0;
        }

        public float[] calculateRunningMean(List<float> input, int stepSize)
        {
            int outputLength = (int)(input.Count / stepSize) + (input.Count % stepSize);
            float[] output = new float[outputLength];
            float sum = 0.0f;
            int index = 0;

            for (int i = 0, j = 0; i < input.Count; i++)
            {
                if(j < stepSize)
                {
                    sum += input[i];
                    index = i;
                    j++;
                }
                else
                {
                    output[(int)(index / stepSize)] = sum / stepSize;
                    sum = 0.0f;
                    j = 0;
                    i--;
                }
                
            }
            return output;
        }
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

        private void button4_Click_1(object sender, EventArgs e)
        {
            firstHarmonicDetectEnabled = true;
        }

        private void checkBoxApplyFilters_CheckedChanged(object sender, EventArgs e)
        {
            if (filtersApplied == false)
                filtersApplied = true;
            else
                filtersApplied = false;
        }
    }
}