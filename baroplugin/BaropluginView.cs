using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using MissionPlanner;
using MissionPlanner.Utilities;
using UAVCAN;
using System.IO;
using System.Text.RegularExpressions;


namespace baroplugin
{
    public partial class BaropluginView : Form
    {
        private MissionPlanner.Utilities.Settings config;
        private BaropluginCode plugin;
        public string nowpath;
        
       
        public BaropluginView(BaropluginCode baropluginCode)

        {
            InitializeComponent();

            this.FormClosing += Form_FormClosing;
            this.plugin = baropluginCode;
       }

       


        private void button1_Click(object sender, EventArgs e)
        {
            
          
        }
      
        public void setvars(BaropluginCode.Vars s)
        {
            label4.Text = Math.Round(s.pressure * 0.750062, 2).ToString();
            label3.Text = Math.Round(plugin.Kalman((s.temperature/100)),2).ToString(); 
            label8.Text = Math.Round(s.altitude, 2).ToString();
            label9.Text = Math.Round(s.windSpeed,3).ToString();
            label10.Text = Math.Round(s.windDir,3).ToString();
            label12.Text = (Math.Round(s.alt_sea, 2) / 1000).ToString();
            label14.Text = Math.Round(s.temperature / 100,3).ToString();
        }

        public void upd(int u)
        {
            label11.Text = u.ToString();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void BaropluginView_Load(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            string path = @"C:\meteo-" + date.ToShortDateString() + "-" + date.Hour.ToString() + "-" + date.Minute.ToString() + ".ini";
            plugin.FileWrite(path);
            nowpath = path;
            button1.Enabled = false;
            plugin.rec = true;
            plugin.flag = false;
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            plugin.rec = false;
            button1.Enabled = true;

            StreamReader sr = new StreamReader(nowpath);
                string cont = sr.ReadToEnd();
                sr.Close();
            string patter = @"^datetime\w*";
            DateTime date = DateTime.Now;
            string targets = "datetime = " + date.ToShortDateString() + "-" + date.ToShortTimeString() + "test";
            Regex regex = new Regex(patter);
            cont = Regex.Replace(cont, @"^datetime\w*", targets);

            StreamWriter wr = new StreamWriter(nowpath);
            wr.Write(cont);
            wr.Close();
            
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }
}
