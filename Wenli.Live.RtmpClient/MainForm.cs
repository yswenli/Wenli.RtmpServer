using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wenli.Live.RtmpClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接到服务器时发生异常，error:" + ex.Message);

                this.textBox1.Enabled = true;

                this.button1.Enabled = true;
            }
        }

        private void _rClient_Disconnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _rClient_CallbackException(object sender, Exception e)
        {
            throw new NotImplementedException();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //await _rClient.LoginAsync("wenli", "wenli");
        }
    }
}
