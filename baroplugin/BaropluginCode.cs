﻿namespace baroplugin
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using baroplugin;
    using MissionPlanner;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public class BaropluginCode : MissionPlanner.Plugin.Plugin
    {
        public override string Name => "Baroplugin";
        public override string Version => "1.0";
        public override string Author => "pix";


        private TabPage pa;
        private ToolStripMenuItem configMenuItem;
        public BaropluginView gmbControlWindow;
        public UserTab tabl;
        public int lay = 0;
        public bool rec = false;
        public bool flag;
        double tempkef = 0;
        int[,] delt = new int[9,14] { {-1,-2,-3,-4,-5,-6,-7,-8,-8,-9,-20,-29,-39,-49},
            {-1,-2,-3,-4,-5,-6,-6,-7,-8,-9,-19,-29,-38,-48}, {-1,-2,-3,-4,-5,-6,-6,-7,-7,-8,-18,-28,-37,-46},
            {-1,-2,-3,-4,-4,-5,-5,-6,-7,-8,-17,-26,-35,-44}, {-1,-2,-3,-3,-4,-4,-5,-6,-7,-7,-17,-25,-34,-42},
            {-1,-2,-3,-3,-4,-4,-5,-6,-6,-7,-16,-24,-32,-40}, {-1,-2,-2,-3,-4,-4,-5,-5,-6,-7,-15,-23,-31,-38},
            {-1,-2,-2,-3,-4,-4,-4,-5,-5,-6,-15,-22,-30,-37}, {-1,-2,-2,-3,-4,-4,-4,-4,-5,-6,-14,-20,-27,-34} };

        int[,] deltz = new int[,] { {1,2,3,4,5,6,7,8,9,10,20,30,0,0},
            {1,2,3,4,5,6,7,8,9,10,20,30,0,0}, {1,2,3,4,5,6,7,8,9,10,20,30,0,0},
            {1,2,3,4,5,6,7,8,9,10,20,30,0,0}, {1,2,3,4,5,6,7,8,9,10,20,30,0,0}, 
            {1,2,3,4,5,6,7,8,9,10,20,30,0,0}, {1,2,3,4,5,6,7,8,9,10,20,30,0,0},
            {1,2,3,4,5,6,7,8,9,10,20,30,0,0}, {1,2,3,4,5,6,7,8,9,10,20,30,0,0} };
        int[] dt = new int[14] {1,2,3,4,5,6,7,8,9,10,20,30,40,50};
        int[] altz = new int[9] {200,400,800,1200,1600,2000,2400,3000,4000};

        public struct Vars
        {
            public double pressure { get; set; }
            public double temperature { get; set; }
            public double altitude { get; set; }
            public double windSpeed { get; set; }
            public double windDir { get; set; }
            public double alt_sea { get; set; }
        }
        public struct layer
        {
            public double pressure { get; set; }
            public double temperature { get; set; }
            public double altitude { get; set; }
            public double windSpeed { get; set; }
            public double windDir { get; set; }
        }


        public override bool Init()
        {
            
            tabl = new UserTab();
            tabl.Dock = DockStyle.Fill;
            pa = new TabPage("LEDs");
            pa.Controls.Add(tabl);
            MissionPlanner.GCSViews.FlightData.instance.tabControlactions.TabPages.Add(pa);
            return true;
           
        }

        public override bool Loaded()
        {
            Host.comPort.OnPacketReceived += MavOnOnPacketReceivedHandler;
           
            
            this.configMenuItem = new ToolStripMenuItem("Barometer");
            configMenuItem.Click += ConfigMenuItem_Click;
            this.Host.FDMenuMap.Items.Add(configMenuItem);
            gmbControlWindow = new BaropluginView(this);
            return true;
           

        }

        private void ConfigMenuItem_Click(object sender, EventArgs e)
        {
            if (gmbControlWindow != null)
            {
                if (!gmbControlWindow.Visible)
                {
                    gmbControlWindow.TopMost = true;
                    gmbControlWindow.Show();
                }
            }
        }
        Vars s = new Vars();
        
        public void MavOnOnPacketReceivedHandler(object o, MAVLink.MAVLinkMessage linkMessage)
        {
                        
            
            if ((MAVLink.MAVLINK_MSG_ID)linkMessage.msgid == MAVLink.MAVLINK_MSG_ID.SCALED_PRESSURE)
            {
                
                var pres = linkMessage.ToStructure<MAVLink.mavlink_scaled_pressure_t>();
                s.pressure = pres.press_abs;
                s.temperature = pres.temperature;
            }

            if ((MAVLink.MAVLINK_MSG_ID)linkMessage.msgid == MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT)
            {
                var alt_sea = linkMessage.ToStructure<MAVLink.mavlink_global_position_int_t>();
                s.alt_sea = alt_sea.alt;
            }

                if ((MAVLink.MAVLINK_MSG_ID)linkMessage.msgid == MAVLink.MAVLINK_MSG_ID.LOCAL_POSITION_NED)
            {
                var alt = linkMessage.ToStructure<MAVLink.mavlink_local_position_ned_t>();
                s.altitude = alt.z * (-1);
                if ((s.altitude >= 0) && (s.altitude <200)) lay = 0;
                if ((s.altitude >= 200) && (s.altitude < 400)) lay = 200;
                if ((s.altitude >= 400) && (s.altitude < 800)) lay = 400;
                if ((s.altitude >= 800) && (s.altitude < 1200)) lay = 800;
                if ((s.altitude >= 1200) && (s.altitude < 1600)) lay = 1200;
                if ((s.altitude >= 1600) && (s.altitude < 2000)) lay = 1600;
                if ((s.altitude >= 2000) && (s.altitude < 2400)) lay = 2000;
                if ((s.altitude >= 2400) && (s.altitude < 3000)) lay = 2400;
                if ((s.altitude >= 3000) && (s.altitude < 3500)) lay = 3000;
                if (s.altitude >= 3500)  lay = 3500;
            }
            if ((MAVLink.MAVLINK_MSG_ID)linkMessage.msgid == MAVLink.MAVLINK_MSG_ID.WIND)
            {
                var wind = linkMessage.ToStructure<MAVLink.mavlink_wind_t>();
                s.windDir = wind.direction;
                s.windSpeed = wind.speed;
            }
           
            gmbControlWindow.setvars(s);
        }

        //запись наземных переменных
        public void FileWrite(string path) 
        {
            DateTime date = DateTime.Now;
            //string path = @"C:\" + date.ToShortTimeString.ToString("HH-mm")+ ".ini";
            

            if (s.temperature < 0) tempkef = 0;
            if ((s.temperature >= 0) && (s.temperature < 5)) tempkef = 0.5;
            if ((s.temperature >= 5) && (s.temperature < 15)) tempkef = 1;
            if ((s.temperature >= 15) && (s.temperature <= 20)) tempkef = 1.5;
            if ((s.temperature > 20) && (s.temperature <= 25)) tempkef =2;
            if ((s.temperature > 25) && (s.temperature <= 30)) tempkef = 3.5;
            if ((s.temperature > 30) && (s.temperature <= 40)) tempkef = 4.5;

            //string writePath = @"C:\meteo.ini";
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine("[Ground]");
                sw.WriteLine("datetime = " + date.ToShortDateString() + "-" + date.ToShortTimeString());
                sw.WriteLine("altitude = " + Math.Round(s.alt_sea / 1000, 2));
                sw.WriteLine("deltah = " + (750-Math.Round(s.pressure * 0.750062, 2)).ToString());
                sw.WriteLine("deltavt = " + ((s.temperature/100) + tempkef - 15.9).ToString());
                sw.WriteLine(System.Environment.NewLine);

                sw.WriteLine("[0]");
                sw.WriteLine("altitude = 0");
                sw.WriteLine("deltat = " + (750 - Math.Round(s.pressure * 0.750062, 2)).ToString());
                sw.WriteLine("winddir = " + (s.temperature / 100).ToString());
                sw.WriteLine("windspeed = " + (s.temperature / 100).ToString());
                sw.WriteLine(System.Environment.NewLine);

            } 
                
        }


        public override bool Exit()
        {
            return true;
        }   

        public override bool Loop()
        {
            if (!MissionPlanner.GCSViews.FlightData.instance.tabControlactions.TabPages.Contains(pa))
                MissionPlanner.GCSViews.FlightData.instance.tabControlactions.TabPages.Add(pa);
            gmbControlWindow.upd(lay);
            
            if (rec == true)
            {
                                    
                        if ((lay == 200) && (flag==false))
                        {
                    
                            using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                            {

                                sw.WriteLine("[200]");
                                sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                                sw.WriteLine("deltat = " + (s.temperature/100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = true;
                            }
                            
                        }

                if ((lay == 400) && (flag == true))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("[400]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = false;
                    }
                }


                if ((lay == 800) && (flag == false))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[800]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir,3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed,3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = true;
                    }

                }

                if ((lay == 1200) && (flag == true))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[1200]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = false;
                    }

                }

                if ((lay == 1600) && (flag == false))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[1600]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = true;
                    }


                }

                if ((lay == 2000) && (flag == true))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[2000]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = false;
                    }

                }

                if ((lay == 2400) && (flag == false))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[2400]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = true;
                    }
                }

                if ((lay == 3000) && (flag == true))
                {
                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[3000]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = false;
                    }
                }

                if ((lay == 3500) && (flag == false))
                {

                    using (StreamWriter sw = new StreamWriter(gmbControlWindow.nowpath, true, System.Text.Encoding.Default))
                    {

                        sw.WriteLine("[3500]");
                        sw.WriteLine("altitude = " + Math.Round(s.altitude, 2));
                        sw.WriteLine("deltat = " + (s.temperature / 100).ToString());
                        sw.WriteLine("winddir = " + Math.Round(s.windDir, 3).ToString());
                        sw.WriteLine("windspeed = " + Math.Round(s.windSpeed, 3).ToString());
                        sw.WriteLine(System.Environment.NewLine);
                        flag = true;
                    }
                }



            }

            if (rec == false)
            {


            }

            return true;
        }

        int avtemp(double nulltemp, int layer)
        {
            int avtemp = 0;
            int tempx = 0, tempy = 0, tempa = 0;
            int temp = Math.Abs(Convert.ToInt32(Math.Round(nulltemp)));
            if (nulltemp>0)
            {
                  for (int i =0;i<dt.Length;i++)
                {
                    if (temp/10 == dt[i]) { tempx = i; }
                    if (temp%10 == dt[i]) { tempy = i; }
                }
                  for (int i = 0;i<altz.Length;i++)
                {
                    if (layer == altz[i]) { tempa = i; }
                }

                avtemp = deltz[tempa, tempx] + deltz[tempa, tempy];
            }

            if (nulltemp<0)
            {
                for (int i = 0; i < dt.Length; i++)
                {
                    if (temp / 10 == dt[i]) { tempx = i; }
                    if (temp % 10 == dt[i]) { tempy = i; }
                }
                for (int i = 0; i < altz.Length; i++)
                {
                    if (layer == altz[i]) { tempa = i; }
                }

               avtemp = delt[tempa, tempx] + delt[tempa, tempy];
            }



            return avtemp;
        }
    }

}