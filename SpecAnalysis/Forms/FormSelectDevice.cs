using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace SpecAnalysis
{
    public partial class FormSelectDevice : Form
    {
        int selectedDeviceIndex = 0;

        public int SelectedDeviceIndex
        {
            get { return selectedDeviceIndex; }
            set { selectedDeviceIndex = value; }
        }
        public FormSelectDevice()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void LoadDevices()
        {
            /*for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                WaveInCapabilities caps = WaveIn.GetCapabilities(i);
                cmbAsioDevices.Items.Add(caps.ProductName);
            }
            cmbAsioDevices.SelectedIndex = 0;*/

            var asioDriverNames = AsioOut.GetDriverNames();

            foreach (var driverName in asioDriverNames)
            {
                cmbAsioDevices.Items.Add(driverName);
            }
            cmbAsioDevices.SelectedIndex = 0;


        }

        private void cmbAsioDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedDeviceIndex = cmbAsioDevices.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
