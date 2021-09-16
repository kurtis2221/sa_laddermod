using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Media;
using ReadMemory;
using Common;

namespace sa_ladder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Variables
        GlobalKeyboardHook gkh = new GlobalKeyboardHook();
        Memory mem;
        SoundPlayer snd = new SoundPlayer();
        FileStream fs;
        StreamWriter sw;
        KeysConverter kconvert = new KeysConverter();

        bool isclimbing = false;
        bool iskeydown = false;
        int estage = 0;
        float tmpz = 0f;
        int maxladders = 0;
        int ladderid = -1;
        float[] pX, pY, pZ, pZl, pZt, pA, pX2, pY2, pZ2, pA2;

        float radius = 1f;
        float ladder_step = 0.2f;
        float editor_step = 0.2f;

        //Ladder keys
        Keys upkey = Keys.W, downkey = Keys.S, usekey = Keys.F;

        //--------------------------------FUNCS--------------------------------

        private void LoadGame()
        {
            if (!IsProcessOpen("gta_sa") && File.Exists("gta_sa.exe"))
                System.Diagnostics.Process.Start("gta_sa.exe");

            if (IsProcessOpen("gta_sa"))
            {
                button1.Enabled = false;
                mem = new Memory("gta_sa", 0x001F0FFF);
                gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);
                gkh.KeyUp += new KeyEventHandler(gkh_KeyUp);
                gkh.Hook();
                label1.Show();
                timer1.Start();
                LoadFile();
            }
            else
            {
                MessageBox.Show("San Andreas NOT found.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int CountLadders()
        {
            int tmp = 0;
            int i = 1;
            while (true)
            {
                    if (GetLine("sa_ladder.txt", i) == null) break;
                    try
                    {
                        if (GetLine("sa_ladder.txt", i).StartsWith("ladder")) tmp += 1;
                    }
                    catch (Exception)
                    {

                    }
                    i++;
            }
            return tmp;
        }

        //Some Cheap Coding in here
        private void LoadFile()
        {
            maxladders = CountLadders();
            pX = new float[maxladders];
            pY = new float[maxladders];
            pZ = new float[maxladders];
            pZl = new float[maxladders];
            pZt = new float[maxladders];
            pA = new float[maxladders];
            pX2 = new float[maxladders];
            pY2 = new float[maxladders];
            pZ2 = new float[maxladders];
            pA2 = new float[maxladders];

            int[] dpos = new int[3];
            int i = 1;
            int count = 0;
            while (true)
            {
                if (GetLine("sa_ladder.txt", i) == null) break;
                try
                {
                    if (GetLine("sa_ladder.txt", i).StartsWith("ladder"))
                    {
                        i += 1;
                        pX[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".",","));
                        i += 1;
                        pY[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pZ[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pA[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pZl[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pZt[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pX2[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pY2[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pZ2[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        pA2[count] = Convert.ToSingle(GetLine("sa_ladder.txt", i).Replace(".", ","));
                        i += 1;
                        count++;
                    }
                }
                catch (Exception)
                {

                }
                i++;
            }
        }

        string GetLine(string fileName, int line)
        {
            using (var sr = new StreamReader(fileName))
            {
                for (int i = 1; i < line; i++)
                    sr.ReadLine();
                return sr.ReadLine();
            }
        }

        public bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetPlayerZ()
        {
            float pz = mem.ReadFloat(0x008CCC44);

            return pz.ToString().Replace(",", ".");
        }

        private float GetPlayerZ2()
        {
            return mem.ReadFloat(0x008CCC44);
        }

        private bool IsPlayerAtPoint(float x, float y, float z, float radius)
        {
            float px = mem.ReadFloat(0x008CCC3C);
            float py = mem.ReadFloat(0x008CCC40);
            float pz = mem.ReadFloat(0x008CCC44);

            if (px + radius >= x && px - radius <= x && py + radius >= y && py - radius <= y
                && pz + radius >= z && pz - radius <= z)
            {
                return true;
            }
            return false;
        }

        private void SetPlayerZ(float z)
        {
            int test = mem.ReadPointer(0xB6F3B8);
            test += 0x14;
            byte[] Buffer = BitConverter.GetBytes((float)z);
            mem.WritePointer((uint)test, 0x38, Buffer);
        }

        private void SetPlayerPos(float x, float y, float z, float ang)
        {
            int test = mem.ReadPointer(0xB6F3B8);
            test += 0x14;
            byte[] Buffer = BitConverter.GetBytes((float)x);
            mem.WritePointer((uint)test, 0x30, Buffer);
            Buffer = BitConverter.GetBytes((float)y);
            mem.WritePointer((uint)test, 0x34, Buffer);
            Buffer = BitConverter.GetBytes((float)z);
            mem.WritePointer((uint)test, 0x38, Buffer);
            Buffer = BitConverter.GetBytes((float)ang);
            mem.WritePointer(0xB6F5F0, 0x558, Buffer);
            mem.WritePointer(0xB6F5F0, 0x55C, Buffer);
        }

        private void FreezePlayer(bool freeze)
        {
            if (freeze)
            {
                byte[] Buffer = BitConverter.GetBytes(2);
                mem.WritePointer(0xB6F5F0, 0x42, Buffer);
            }
            else
            {
                byte[] Buffer = BitConverter.GetBytes(0);
                mem.WritePointer(0xB6F5F0, 0x42, Buffer);
            }
        }

        private void FreezePlayer2(bool freeze)
        {
            if (freeze)
            {
                byte[] Buffer = BitConverter.GetBytes(1);
                mem.WritePointer(0xB6F5F0, 0x598, Buffer);
                Buffer = BitConverter.GetBytes(2);
                mem.WritePointer(0xB6F5F0, 0x42, Buffer);
            }
            else
            {
                byte[] Buffer = BitConverter.GetBytes(0);
                mem.WritePointer(0xB6F5F0, 0x598, Buffer);
                Buffer = BitConverter.GetBytes(0);
                mem.WritePointer(0xB6F5F0, 0x42, Buffer);
            }
        }

        private void FreezeFacing(float angle)
        {
            byte[] Buffer = BitConverter.GetBytes(angle);
            mem.WritePointer(0xB6F5F0, 0x558, Buffer);
            mem.WritePointer(0xB6F5F0, 0x55C, Buffer);
            Buffer = BitConverter.GetBytes(0);
            mem.WritePointer(0xB6F5F0, 0x560, Buffer);
        }

        private void UnFreezeFacing()
        {
            byte[] Buffer = BitConverter.GetBytes(9f);
            mem.WritePointer(0xB6F5F0, 0x560, Buffer);
        }

        //--------------------------------EVENTS--------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
            LoadGame();
        }

        private void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            iskeydown = false;
        }

        private void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            if (mem.ReadPointer(0xBA67A4) == 0 && !this.Focused)
            {
                //Ladder Script
                if (!label4.Visible)
                {
                    if (e.KeyCode == usekey && !iskeydown)
                    {
                        if (isclimbing)
                        {
                            FreezePlayer(false);
                            isclimbing = false;
                            ladderid = -1;
                            UnFreezeFacing();
                            e.Handled = true;
                        }
                        else
                        {
                            for (int i = 0; i < maxladders; i++)
                            {
                                if (IsPlayerAtPoint(pX[i], pY[i], pZ[i], radius))
                                {
                                    isclimbing = true;
                                    FreezePlayer(true);
                                    SetPlayerPos(pX[i], pY[i], pZl[i] + ladder_step, pA[i]);
                                    tmpz = pZl[i] + ladder_step;
                                    ladderid = i;
                                    break;
                                }
                                else if (IsPlayerAtPoint(pX2[i], pY2[i], pZ2[i], radius))
                                {
                                    isclimbing = true;
                                    FreezePlayer(true);
                                    SetPlayerPos(pX[i], pY[i], pZt[i] - ladder_step, pA[i]);
                                    tmpz = pZt[i] - ladder_step;
                                    ladderid = i;
                                    break;
                                }
                            }
                            e.Handled = true;
                        }
                        iskeydown = true;
                    }
                    if (isclimbing)
                    {
                        if (e.KeyCode == upkey)
                        {
                            tmpz += ladder_step;
                            SetPlayerZ(tmpz);
                            FreezeFacing(pA[ladderid]);
                            if (Math.Abs(tmpz - pZt[ladderid]) < radius)
                            {
                                SetPlayerPos(pX2[ladderid], pY2[ladderid], pZ2[ladderid], pA2[ladderid]);
                                FreezePlayer(false);
                                isclimbing = false;
                                UnFreezeFacing();
                                ladderid = -1;
                            }
                        }
                        else if (e.KeyCode == downkey)
                        {
                            tmpz -= ladder_step;
                            SetPlayerZ(tmpz);
                            FreezeFacing(pA[ladderid]);
                            if (Math.Abs(tmpz - pZl[ladderid]) < radius)
                            {
                                SetPlayerPos(pX[ladderid], pY[ladderid], pZ[ladderid], pA[ladderid]);
                                FreezePlayer(false);
                                isclimbing = false;
                                UnFreezeFacing();
                                ladderid = -1;
                            }
                        }
                        else if (e.KeyCode != Keys.Escape)
                        {
                            e.Handled = true;
                        }
                    }
                }
                else
                {
                    //Editor Script
                    if (e.KeyCode == Keys.L && estage == 0)
                    {
                        snd.Stream = Properties.Resources.work;
                        snd.Play();
                        estage = 1;
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Enter && !iskeydown && estage == 1)
                    {
                        if (!File.Exists("sa_ladder.txt")) File.Create("sa_ladder.txt");
                        fs = new FileStream("sa_ladder.txt", FileMode.Append, FileAccess.Write);
                        sw = new StreamWriter(fs);
                        sw.WriteLine("ladder");
                        sw.WriteLine(mem.ReadFloat(0x008CCC3C).ToString().Replace(",", "."));
                        sw.WriteLine(mem.ReadFloat(0x008CCC40).ToString().Replace(",", "."));
                        sw.WriteLine(mem.ReadFloat(0x008CCC44).ToString().Replace(",", "."));
                        sw.WriteLine(mem.ReadFloatOffset(0xB6F5F0, 0x558).ToString().Replace(",", "."));
                        sw.WriteLine(GetPlayerZ());
                        snd.Play();
                        FreezePlayer2(true);
                        estage = 2;
                        tmpz = GetPlayerZ2();
                        iskeydown = true;
                        e.Handled = true;
                    }
                    //NAVIGATION
                    else if (e.KeyCode == Keys.Up && !iskeydown && estage == 2)
                    {
                        int test = mem.ReadPointer(0xB6F3B8);
                        test += 0x14;
                        byte[] Buffer = BitConverter.GetBytes(mem.ReadFloat(0x008CCC40) + editor_step);
                        mem.WritePointer((uint)test, 0x34, Buffer);
                    }
                    else if (e.KeyCode == Keys.Down && !iskeydown && estage == 2)
                    {
                        int test = mem.ReadPointer(0xB6F3B8);
                        test += 0x14;
                        byte[] Buffer = BitConverter.GetBytes(mem.ReadFloat(0x008CCC40) - editor_step);
                        mem.WritePointer((uint)test, 0x34, Buffer);
                    }
                    else if (e.KeyCode == Keys.Left && !iskeydown && estage == 2)
                    {
                        int test = mem.ReadPointer(0xB6F3B8);
                        test += 0x14;
                        byte[] Buffer = BitConverter.GetBytes(mem.ReadFloat(0x008CCC3C) - editor_step);
                        mem.WritePointer((uint)test, 0x30, Buffer);
                    }
                    else if (e.KeyCode == Keys.Right && !iskeydown && estage == 2)
                    {
                        int test = mem.ReadPointer(0xB6F3B8);
                        test += 0x14;
                        byte[] Buffer = BitConverter.GetBytes(mem.ReadFloat(0x008CCC3C) + editor_step);
                        mem.WritePointer((uint)test, 0x30, Buffer);
                    }
                    else if (e.KeyCode == Keys.Space && !iskeydown && estage == 2)
                    {
                        tmpz += editor_step;
                        SetPlayerZ(tmpz);
                    }
                    else if (e.KeyCode == Keys.C && !iskeydown && estage == 2)
                    {
                        tmpz -= editor_step;
                        SetPlayerZ(tmpz);
                    }
                    //NAVIGATION END
                    else if (e.KeyCode == Keys.Enter && !iskeydown && estage == 2)
                    {
                        sw.WriteLine(GetPlayerZ());
                        snd.Play();
                        estage = 3;
                        e.Handled = true;
                        sw.WriteLine(mem.ReadFloat(0x008CCC3C).ToString().Replace(",", "."));
                        sw.WriteLine(mem.ReadFloat(0x008CCC40).ToString().Replace(",", "."));
                        sw.WriteLine(mem.ReadFloat(0x008CCC44).ToString().Replace(",", "."));
                        sw.WriteLine(mem.ReadFloatOffset(0xB6F5F0, 0x558).ToString().Replace(",", "."));
                        sw.WriteLine("end");
                        snd.Stream = Properties.Resources.complete;
                        snd.Play();
                        FreezePlayer2(false);
                        estage = 0;
                        tmpz = 0;
                        sw.Close();
                        fs.Close();
                        iskeydown = true;
                        e.Handled = true;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!IsProcessOpen("gta_sa")) Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists("sa_ladder.txt"))
            {
                MessageBox.Show("The file sa_ladder.txt is missing," +
                "created a new file with that name.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                File.Create("sa_ladder.txt");
            }
            if (!File.Exists("sa_ladder.ini"))
                MessageBox.Show("The file sa_ladder.ini is missing," +
                "using default global options.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                radius = Convert.ToSingle(GetLine("sa_ladder.ini", 1).Replace(".",","));
                ladder_step = Convert.ToSingle(GetLine("sa_ladder.ini", 2).Replace(".", ","));
                editor_step = Convert.ToSingle(GetLine("sa_ladder.ini", 3).Replace(".", ","));
                upkey = (Keys)Enum.Parse(typeof(Keys), GetLine("sa_ladder.ini", 4));
                downkey = (Keys)Enum.Parse(typeof(Keys), GetLine("sa_ladder.ini", 5));
                usekey = (Keys)Enum.Parse(typeof(Keys), GetLine("sa_ladder.ini", 6));
            }
            this.Size = new Size(440,300);
            if(IsProcessOpen("gta_sa")) LoadGame();
        }

        //-----------------------------EDITOR------------------------------

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Text = (button2.Text == "Enable Ladder Editor")
                ? "Disable Ladder Editor" : "Enable Ladder Editor";
            label4.Visible = (label4.Visible) ? false : true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Show Editor")
            {
                this.Size = new Size(440, 470);
                button3.Text = "Hide Editor";
                button2.Visible = true;
            }
            else
            {
                this.Size = new Size(440, 300);
                button3.Text = "Show Editor";
                button2.Visible = false;
                Process.GetCurrentProcess();
            }
        }
    }
}
