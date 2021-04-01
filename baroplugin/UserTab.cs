using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MissionPlanner;
using System.Threading;
using System.Threading.Tasks;

namespace baroplugin
{
    public partial class UserTab : UserControl
    {

        public UserTab()
        {
            InitializeComponent();
        }

        private void UserTab_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            MainV2.comPort.doCommand(MAVLink.MAV_CMD.DO_SET_SERVO, 6, 19000,
                0, 0, 0, 0, 0, false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button1.Enabled = true;
            MainV2.comPort.doCommand(MAVLink.MAV_CMD.DO_SET_SERVO, 6, 0,
                0, 0, 0, 0, 0, false);
        }


        async private void button3_Click(object sender, EventArgs e)
        {
            int x = 0;
            int num = 0;
            TimerCallback tm = new TimerCallback(cnt);
            System.Threading.Timer tmr = new System.Threading.Timer(tm, num, 0, 1000);
            //        MainV2.comPort.doCommand(MAVLink.MAV_CMD.DO_SET_SERVO, 6, 0,
            //            0, 0, 0, 0, 0, false);
            //        await Task.Delay(1000);

            //        MainV2.comPort.doCommand(MAVLink.MAV_CMD.DO_SET_SERVO, 6, 19000,
            //            0, 0, 0, 0, 0, false);

        }
        public static int its = 0;
        public static void cnt(object obj)
        {
            its++;


            MainV2.comPort.doCommand(MAVLink.MAV_CMD.DO_SET_SERVO, 6, 19000,
                0, 0, 0, 0, 0, false);

        }
    }
}
