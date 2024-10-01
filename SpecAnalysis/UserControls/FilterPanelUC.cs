using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpecAnalysis.Utilities;

namespace SpecAnalysis
{
    public enum FilterModeEnum
    {
        d12 = 0,
        d24
    };
    public partial class FilterPanelUC : UserControl
    {
        private static int fftSize = 2048;
        private static int samplingRate = 44100;
        private Pen filterPen;
        private bool filterOn = false;
        private int filterCutoff = 128;
        private int filterResonance = 0;
        private int filterGain = 1;
        private bool filterSelected = false;
        private bool colorChanged = false;
        private FilterTypeEnum filterType;
        private FilterModeEnum filterMode;
        private Filters filter = new Filters();
        private ComplexNumber[] cResponse = new ComplexNumber[fftSize / 2 + 1];
        private double[] frequencyResponse = new double[fftSize / 2 + 1];
        private ColorDialog dialog1 = new ColorDialog();

        public bool ColorChanged
        {
            get { return colorChanged; }
            set { colorChanged = value; }
        }
        public Pen FilterPen
        {
            get { return filterPen; }
            set { filterPen = value; }
        }
        public bool FilterSelected
        {
            get { return filterSelected; }
            set { filterSelected = value; }
        }
        public FilterModeEnum FilterMode
        {
            get { return filterMode; }
            set { filterMode = value; }
        }
        public ComplexNumber[] ComplexResponse
        {
            get { return cResponse; }
            set { cResponse = value; }
        }
        public double[] FrequencyResponse
        {
            get { return frequencyResponse; }
            set { frequencyResponse = value; }
        }
        public bool FilterOn
        {
            get { return filterOn; }
            set { filterOn = value; }
        }
        public int FilterCutoff
        {
            get { return filterCutoff; }
            set { filterCutoff = value; }
        }
        public int FilterResonance
        {
            get { return filterResonance; }
            set { filterResonance = value; }
        }
        public int FilterGain
        {
            get { return filterGain; }
            set { filterGain = value; }
        }
        public FilterTypeEnum FilterType
        {
            get { return filterType; }
            set { filterType = value; }
        }
        public FilterPanelUC()
        {
            for (int i = 0; i < fftSize / 2 + 1; i++)
            {
                cResponse[i] = new ComplexNumber(1.0f, 0.0f);
            }
            InitializeComponent();
        }
        public void update()
        {
            if (filterGain == 0 || FilterGain == 0)
                filterGain = 1;

            ColorChanged = colorChanged;
            FilterPen = filterPen;
            FilterCutoff = filterCutoff;
            FilterGain = filterGain;
            FilterResonance = filterResonance;
            FilterOn = filterOn;
            FilterType = filterType;
            FilterMode = filterMode;
            FilterSelected = filterSelected;   

            filter.initializeResonanceTable();
            filter.getFrequencyTable(fftSize / 2 + 1);

            switch (FilterType)
            {
                case FilterTypeEnum.eLowpass:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculateLP12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculateLP24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;

                case FilterTypeEnum.eHighpass:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculateHP12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculateHP24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;

                case FilterTypeEnum.eBandpass:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculateBP12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculateBP24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;

                case FilterTypeEnum.eBandstop:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculateBS12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculateBS24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;

                case FilterTypeEnum.eLowShelve:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculateLS12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculateLS24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;

                case FilterTypeEnum.eHighShelve:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculateHS12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculateHS24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;
                case FilterTypeEnum.ePeaking:
                    switch (FilterMode)
                    {
                        case FilterModeEnum.d12:
                            filter.calculatePK12Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        case FilterModeEnum.d24:
                            filter.calculatePK24Response(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                            break;
                        default:
                            break;
                    }
                    break;
                case FilterTypeEnum.eComb:
                    filter.calculateCombResponse(FrequencyResponse, fftSize / 2 + 1, FilterResonance, FilterCutoff, FilterGain, cResponse);
                    break;
            }
            for (int i = 0; i < fftSize / 2 + 1; i++)
            {
                ComplexResponse[i] = cResponse[i];
                FrequencyResponse[i] = frequencyResponse[i];
            }
            numCutoff.Value = Convert.ToDecimal((FilterCutoff) * (samplingRate / fftSize));
            numRes.Value = Convert.ToDecimal(FilterResonance);

            if (FilterGain < 1)
                FilterSelected = false;
            else
                if (FilterGain > 0)
                FilterSelected = true;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            filterLabel.Text = comboBox1.Text;
            comboBox2.SelectedIndex = -1;
            comboBox2.Enabled = true;

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    filterType = FilterTypeEnum.eLowpass;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Gain";
                    filterGain = 1;
                    numGain.Value = 1;
                    break;
                case 1:
                    filterType = FilterTypeEnum.eHighpass;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Gain";
                    filterGain = 1;
                    numGain.Value = 1;
                    break;
                case 2:
                    filterType = FilterTypeEnum.eBandpass;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Gain";
                    filterGain = 1;
                    numGain.Value = 1;
                    break;
                case 3:
                    filterType = FilterTypeEnum.eBandstop;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Gain";
                    filterGain = 1;
                    numGain.Value = 1;
                    break;
                case 4:
                    filterType = FilterTypeEnum.eLowShelve;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Boost/Cut";
                    filterGain = 25;
                    numGain.Value = 25;
                    break;
                case 5:
                    filterType = FilterTypeEnum.eHighShelve;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Boost/Cut";
                    filterGain = 25;
                    numGain.Value = 25;
                    break;
                case 6:
                    filterType = FilterTypeEnum.eComb;
                    filterPanel.Enabled = true;
                    filterSelected = true;
                    comboBox2.Enabled = false;
                    lblRes.Text = "Feedback";
                    lblGain.Text = "Volume";
                    filterGain = 1;
                    numGain.Value = 1;
                    break;
                case 7:
                    filterType = FilterTypeEnum.ePeaking;
                    lblRes.Text = "Resonance";
                    lblGain.Text = "Boost/Cut";
                    filterGain = 25;
                    numGain.Value = 25;
                    break;
                default:
                    break;
            }          
            //this.update();
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            filterPanel.Enabled = true;
            filterSelected = true;

            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    filterMode = FilterModeEnum.d12;
                    break;
                case 1:
                    filterMode = FilterModeEnum.d24;
                    break;
                default:
                    break;
            }          

            this.update();
        }

        private void numGain_ValueChanged(object sender, EventArgs e)
        {
            filterGain = Convert.ToInt32(numGain.Value);
            this.update();
        }

        private void numCutoff_ValueChanged(object sender, EventArgs e)
        {
            filterCutoff = ((Convert.ToInt32(numCutoff.Value) / (samplingRate / fftSize)));

            if(((Convert.ToInt32(numCutoff.Value) / (samplingRate / fftSize))) <= 0)
                filterCutoff = 1;
            else
                if(((Convert.ToInt32(numCutoff.Value) / (samplingRate / fftSize))) >= fftSize / 2)
                filterCutoff = fftSize / 2 - 1;

            this.update();
        }

        private void numRes_ValueChanged(object sender, EventArgs e)
        {
            filterResonance = Convert.ToInt32(numRes.Value);
            this.update();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            filterCutoff = 1;
            filterResonance = 0;
            filterGain = 0;
            filterPanel.Enabled = false;
            filterType = FilterTypeEnum.eNoFilter;
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;

            for (int i = 0; i < fftSize / 2 + 1; i++)
            {
                cResponse[i] = new ComplexNumber(1.0f, 0.0f);
                frequencyResponse[i] = 0;
            }
            this.update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                filterPen = new Pen(dialog1.Color, 1);
                colorChanged = true;
            }
            this.update();
        }
    }
}
