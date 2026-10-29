using System.Management;

namespace TEmpCPU
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            try
            {
                Double CPUtprt = 0;
                ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\WMI",
                        "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject Mo in MOS.Get())
                {
                    CPUtprt = Convert.ToDouble(Convert.ToDouble(Mo.GetPropertyValue("CurrentTemperature".ToString())) - 2732) / 10.0;
                    listBox1.Items.Add(" CPU: " + CPUtprt.ToString() + " ° C");
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show("Ошибка получения данных " + ex.Message);
            }
        }
    }
}
