using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace os_ass2
{
    public partial class SchedulerSettingsForm : Form
    {
        public SchedulerSettingsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1 && textBox2.TextLength != 0 && (textBox3.TextLength != 0 || (comboBox1.SelectedIndex != 1 && comboBox1.SelectedIndex!=3)))
            {
                if (textBox3.TextLength == 0) //no quanta entered.
                    textBox3.Text = "0";
                Scheduler s = new Scheduler(textBox1.Text, comboBox1.SelectedIndex, Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox3.Text));
                try //try reading input file.
                {
                    s.ReadInput();
                }
                catch(Exception E)
                {
                    MessageBox.Show(E.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                GraphForm gf=new GraphForm(s.StartSimulation());
                gf.Show();
            }
            else
            {
                MessageBox.Show("Fill all the required data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
    }
}
