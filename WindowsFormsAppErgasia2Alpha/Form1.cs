using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

//drawing application version 1
//note : will refer to 'pictureBox3' as 'canvas'

namespace WindowsFormsAppErgasia2Alpha
{
    public partial class Form1 : Form
    {

        Button[][] paletBoxes;

        string s = "";              //field that inidicates what drawing tool the user has choosen at any moment
        bool freehand;              //field that inidicates if the mouse is down and inside the panel at any moment
        int graphX1, graphY1;       //in these fields we keep the coordinates when we need to
        Graphics g, gh;
        Pen p;
        
        


        Bitmap buffer;                              //this is the Bitmap object. We will draw on the Bitmap-type object (the buffer) and not on the picture box of the form

        List<Bitmap> undo = new List<Bitmap>();     //when we draw anything on the buffer we save it in the undo list / this way we can go back to a previous state
        List<Bitmap> redo = new List<Bitmap>();     //when we go back to a previous state we save the current state in the redo list so we can revert to the original state
        //in the above lists we a clones of the object buffer so this way we do not refer to the same object

        bool mousedown;     //this field is used to indicate if the mouse is down when the user is drawing a square/elipse/rectangle/cirlce

        List<string> shapesDrawn;                                       //list that contains all currently drawn shapes (in words)
        List<List<string>> savedShapesUndo = new List<List<string>>();  //similar to the bitmap
        List<List<string>> savedShapesRedo = new List<List<string>>();



        //fields for fractal binary tree with timer
        Point O;
        List<Point> points = new List<Point>();
        List<double> angles = new List<double>();
        int layer = 1;
        double length = 100;
        double offset = Math.PI / 6;
        double angle = Math.PI / 2;
        //

        //fields for cycle shape
        Point prev;
        int k;
        double n, maxPoints;
        //

        //fields for the star
        Point[] starPoints;
        int pointTodraw;
        //

        //fields for the house
        Point[] houseAllpoints;
        Point[] outlinepoints;
        Point[] roof;
        Point[] window1;
        Point[] window2;
        Point[] door;
        int housePoints;
        int houseStep;
        //

        List<string> savedNumerics = new List<string>(); //in this list method 'saveNumerics' saves the values of all numericUpDown controls

        Form2 form2;
        public bool form2Active = false;    //bool if form2 is open or not
        public Form3 form3;
        public bool form3Active = false;    //bool if form3 is open or not


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); //set up saveDialog
            
            p = new Pen(Color.Black, 2.0f);   //initialize the pen

            buffer = new Bitmap(this.pictureBox3.Width, this.pictureBox3.Height); //initialize the buffer with the dimensions of the picturebox

            g = Graphics.FromImage(buffer);     //initialize a graphics object for the buffer

            gh = pictureBox3.CreateGraphics();  //initialize a graphics object for the picturebox

            undo.Add((Bitmap)buffer.Clone());   //add a clone of the buffer to the undo

            shapesDrawn = new List<string>();   //initialize the shapesDrawn list

            savedShapesUndo.Add(new List<string>(shapesDrawn)); //add a clone of the shapesDrawn to the undo

            //set up the UI color picker
            pictureBox1.BackColor = Color.FromArgb((int)numericUpDown1.Value, (int)numericUpDown2.Value, (int)numericUpDown3.Value);
            textBox1.Text = ColorTranslator.ToHtml(pictureBox1.BackColor);

            //add event functions to controls
            trackBar1.ValueChanged += new System.EventHandler(trackbars_ValueChanged);
            trackBar2.ValueChanged += new System.EventHandler(trackbars_ValueChanged);
            trackBar3.ValueChanged += new System.EventHandler(trackbars_ValueChanged);
            numericUpDown1.ValueChanged += new System.EventHandler(numericUpDowns_ValueChanged);
            numericUpDown2.ValueChanged += new System.EventHandler(numericUpDowns_ValueChanged);
            numericUpDown3.ValueChanged += new System.EventHandler(numericUpDowns_ValueChanged);
            button4.Click += new System.EventHandler(buttonToolsClick);
            button5.Click += new System.EventHandler(buttonToolsClick);
            button6.Click += new System.EventHandler(buttonToolsClick);
            button7.Click += new System.EventHandler(buttonToolsClick);
            button8.Click += new System.EventHandler(buttonToolsClick);
            button9.Click += new System.EventHandler(buttonToolsClick);

            //double array of colors
            Color[][] colors = new Color[4][];
            for(int i = 0; i < 4; i++)
            {
                colors[i] = new Color[4];
            }           
            colors[0][0] = Color.White;
            colors[0][1] = Color.Black;
            colors[0][2] = Color.Blue;
            colors[0][3] = Color.Brown;

            colors[1][0] = Color.Green;
            colors[1][1] = Color.Gray;
            colors[1][2] = Color.Purple;
            colors[1][3] = Color.Coral;

            colors[2][0] = Color.DarkCyan;
            colors[2][1] = Color.Crimson;
            colors[2][2] = Color.BurlyWood;
            colors[2][3] = Color.Pink;

            colors[3][0] = Color.Yellow;
            colors[3][1] = Color.DarkRed;
            colors[3][2] = Color.DarkGreen;
            colors[3][3] = Color.DarkOrange;

            //create 16 buttons, each with Backcolor from the above array
            for (int i = 0; i < 4; i++)
            {
                paletBoxes= new Button[4][];
                for (int j = 0; j < 4; j++)
                {
                    paletBoxes[i] = new Button[4];
                    paletBoxes[i][j] = new Button();
                    paletBoxes[i][j].Location =  new System.Drawing.Point(14 + i*30, 211+j*30);
                    paletBoxes[i][j].Name = "paletButton" + i.ToString();
                    paletBoxes[i][j].Size = new System.Drawing.Size(30, 30);
                    paletBoxes[i][j].Click += new System.EventHandler(paletBoxesClick);


                    this.Controls.Add(paletBoxes[i][j]);
                    paletBoxes[i][j].Parent = panel1;
                    paletBoxes[i][j].BackColor = colors[i][j];
                }                              
            }

            button4.PerformClick(); //select 'free style drawing' mode

            //apply the light theme
            applyLightTheme();
            lightToolStripMenuItem.Checked = true;
            darkToolStripMenuItem.Checked = false;
            orangeBlueToolStripMenuItem.Checked = false;

            //set the initial position of the canvas (pictureBox3)
            pictureBox3.Location = new Point((this.Width) / 2 - pictureBox3.Width / 2, this.Height / 2 - pictureBox3.Height / 2 -25);

            numericUpDown4.Value = 2;   //set up width value

            numericUpDown6.Maximum = pictureBox3.Width;     //set up numericUpDowns for fractal binary tree
            numericUpDown7.Maximum = pictureBox3.Height;

            numericUpDown6.Value = pictureBox3.Width / 2;
            numericUpDown7.Value = 500;

            numericUpDown10.Maximum = pictureBox3.Width;    //set up numericUpDowns for star
            numericUpDown9.Maximum = pictureBox3.Height;

            numericUpDown10.Value = pictureBox3.Width / 2;
            numericUpDown9.Value = pictureBox3.Height / 2;

            numericUpDown13.Maximum = pictureBox3.Width;    //set up numericUpDowns for House
            numericUpDown11.Maximum = pictureBox3.Height;

            numericUpDown19.Maximum = pictureBox3.Width;    //set up numericUpDowns for cycle shape
            numericUpDown18.Maximum = pictureBox3.Height;

            numericUpDown19.Value = pictureBox3.Width / 2;
            numericUpDown18.Value = pictureBox3.Height / 2;

            //set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 500;
            //force the toolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.button1, "Redo (Ctrl + Y)");
            toolTip1.SetToolTip(this.button2, "Undo (Ctrl + X)");
            toolTip1.SetToolTip(this.button14, "Export to .png");
            toolTip1.SetToolTip(this.button3, "Clear Canvas");
            toolTip1.SetToolTip(this.button10, "Draw Fractal Binary Tree Instantly");
            toolTip1.SetToolTip(this.button12, "Draw Fractal Binary Tree with a Timer");
            toolTip1.SetToolTip(this.button19, "Draw Star Instantly");
            toolTip1.SetToolTip(this.button20, "Draw Star with a Timer");
            toolTip1.SetToolTip(this.button11, "Draw House Instantly");
            toolTip1.SetToolTip(this.button17, "Draw House with a Timer");
            toolTip1.SetToolTip(this.button18, "Draw Cycle Shape Instantly");
            toolTip1.SetToolTip(this.button13, "Draw Cycle Shape with a Timer");
            toolTip1.SetToolTip(this.button4, "Free-style drawing");
            toolTip1.SetToolTip(this.button6, "Draw Rectangle");
            toolTip1.SetToolTip(this.button8, "Draw line");
            toolTip1.SetToolTip(this.button7, "Draw Elipse");
            toolTip1.SetToolTip(this.button5, "Draw Cycle");
            toolTip1.SetToolTip(this.button9, "Draw Square");
        }




        //click event for the 'palet buttons' that were created in Form load
        private void paletBoxesClick(object sender, EventArgs e)
        {
            Color c = ((Button)sender).BackColor;
            numericUpDown1.Value = c.R;
            numericUpDown2.Value = c.G;
            numericUpDown3.Value = c.B;
        }

        //click event for the 'drawing tools buttons' 
        private void buttonToolsClick(object sender, EventArgs e)
        {
            s = (String)((Button)sender).Tag;   //string s gets the value of the buttons tag
            button4.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;
            button7.BackColor = Color.Transparent;
            button8.BackColor = Color.Transparent;
            button9.BackColor = Color.Transparent;
            ((Button)sender).BackColor = Color.Green;
        }


        // --> Pen color adjustment <-- //
        private void updateColor()
        {
            pictureBox1.BackColor = Color.FromArgb((int)numericUpDown1.Value, (int)numericUpDown2.Value, (int)numericUpDown3.Value);
            textBox1.Text = ColorTranslator.ToHtml(pictureBox1.BackColor);
        }

        private void trackbars_ValueChanged(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;

            string s = (String)tb.Tag;

            switch (s)
            {
                case "1":
                    numericUpDown1.Value = tb.Value;
                    break;
                case "2":
                    numericUpDown2.Value = tb.Value;
                    break;
                case "3":
                    numericUpDown3.Value = tb.Value;
                    break;
            }

            updateColor();

        }

        private void numericUpDowns_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nup = (NumericUpDown)sender;

            string s = (String)nup.Tag;

            switch (s)
            {
                case "1":
                    trackBar1.Value = (int)nup.Value;
                    break;
                case "2":
                    trackBar2.Value = (int)nup.Value;
                    break;
                case "3":
                    trackBar3.Value = (int)nup.Value;
                    break;
            }
            updateColor();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //MessageBox.Show("ok");
                string s = textBox1.Text;
                if (!s.StartsWith("#"))
                    s = "#" + s;
                try
                {
                    System.Drawing.Color c = System.Drawing.ColorTranslator.FromHtml(s);
                    numericUpDown1.Value = c.R;
                    numericUpDown2.Value = c.G;
                    numericUpDown3.Value = c.B;
                }
                catch (System.Exception)
                {
                    updateColor();
                }
            }
        }

        private void pictureBox1_BackColorChanged(object sender, EventArgs e)
        {
            p.Color = pictureBox1.BackColor;
        }

        //button 'windows color picker'
        private void button16_Click(object sender, EventArgs e)
        {
            //set the color of the pen as the color selected by the user in the color picker
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color c = colorDialog1.Color;
                numericUpDown1.Value = c.R;
                numericUpDown2.Value = c.G;
                numericUpDown3.Value = c.B;
            }
        }
        // -- -- //


        // --> Pen width adjustment <-- //

        //numericUpDown4 -> values range : (0.1 - 50.0)
        //trackBar4 -> values range : (1 - 500)

        //helping method that sets the width of the pen to the value of numericUpDown4
        private void setPenWidth()
        {
            p.Width = (float)numericUpDown4.Value;
        }

        //when trackBar4 is scrolled numericUpDown4 gets its value/10 and calls 'setPenWidth'
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            float f = (float)(trackBar4.Value) / 10;
            numericUpDown4.Value = new decimal(f);
            setPenWidth();
        }

        //when numericUpDown4 gets a new value, trackBar4 is updated and calls 'setPenWidth'
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            int i = (int)(numericUpDown4.Value * 10);
            trackBar4.Value = i;
            setPenWidth();
        }
        // -- //


        // --> theme functions  <-- //
        //simply change some colors of the form
        private void applyLightTheme()
        {
            panel1.BackColor = SystemColors.ScrollBar;
            panel2.BackColor = SystemColors.ScrollBar;
            this.ForeColor = Color.Black;
            this.BackColor = SystemColors.Control;
            button1.BackColor = Color.White;
            button2.BackColor = Color.White;
            groupBox1.ForeColor = Color.Black;
            groupBox2.ForeColor = Color.Black;
            groupBox3.ForeColor = Color.Black;
            groupBox4.ForeColor = Color.Black;
        }

        private void applyDarktTheme()
        {
            panel1.BackColor = Color.FromArgb(56, 59, 56);
            panel2.BackColor = Color.FromArgb(56, 59, 56);
            this.ForeColor = Color.White;
            this.BackColor = SystemColors.ControlDark;
            button1.BackColor = Color.White;
            button2.BackColor = Color.White;
            groupBox1.ForeColor = Color.White;
            groupBox2.ForeColor = Color.White;
            groupBox3.ForeColor = Color.White;
            groupBox4.ForeColor = Color.White;
        }

        private void applyOrangeBlueTheme()
        {
            panel1.BackColor = Color.FromArgb(66, 135, 245);
            panel2.BackColor = Color.FromArgb(66, 135, 245);
            this.ForeColor = Color.White;
            this.BackColor = Color.Orange;
            button1.BackColor = Color.White;
            button2.BackColor = Color.White;
            groupBox1.ForeColor = Color.White;
            groupBox2.ForeColor = Color.White;
            groupBox3.ForeColor = Color.White;
            groupBox4.ForeColor = Color.White;
        }
        // -- -- //


        //  --> mouse related events <-- //

        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            //if the user has choosen  a tool
            if (!s.Equals(""))
            {
                mousedown = true;//mouse is down
                if (s.Equals("freepen"))
                    freehand = true;    //this is for the free style drawing
                graphX1 = e.X;          //take coordinates
                graphY1 = e.Y;
            }


        }

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            //when the user releaces the mouse the shape is drawn
            //draw the correct shape in he g Graphics object and add the corresponding word (string) that describes the drawn shape to the list 'shapesDrawn'
            //shapesDrawn will 'store' every shape drawn so this information can be stored in the DB and also can be read by the method 'parser' to re-draw the shapes.
            freehand = false;
            if (s.Equals("line"))
            {
                g.DrawLine(p, graphX1, graphY1, e.X, e.Y);
                shapesDrawn.Add("LIN*"+ graphX1.ToString() +"*"+ graphY1.ToString() + "*" + e.X.ToString() + "*" + e.Y.ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

            }
            //for the rest of the symbols we take 4 cases for the 4 quartes (tetartimoria)
            else if (s.Equals("rectangle"))
            {
                if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                {
                    //label2.Text = " 1o tetartimorio " + graphX1.ToString() + "-" + graphY1.ToString() + "  " + e.X.ToString() + "-" + e.Y.ToString();
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, graphX1, e.Y, e.X - graphX1, graphY1 - e.Y);
                        shapesDrawn.Add("REF*" + graphX1.ToString() + "*" + e.Y.ToString() + "*" + (e.X - graphX1).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawRectangle(p, graphX1, e.Y, e.X - graphX1, graphY1 - e.Y);
                        shapesDrawn.Add("REN*" + graphX1.ToString() + "*" + e.Y.ToString() + "*" + (e.X - graphX1).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }

                }
                else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                {
                    //label2.Text = " 2o tetartimorio " + graphX1.ToString() + "-" + graphY1.ToString() + "  " + e.X.ToString() + "-" + e.Y.ToString();
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, e.X, e.Y, graphX1 - e.X, graphY1 - e.Y);
                        shapesDrawn.Add("REF*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    else
                    {
                        g.DrawRectangle(p, e.X, e.Y, graphX1 - e.X, graphY1 - e.Y);
                        shapesDrawn.Add("REN*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    
                }
                else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                {
                    //label2.Text = " 3o tetartimorio " + graphX1.ToString() + "-" + graphY1.ToString() + "  " + e.X.ToString() + "-" + e.Y.ToString();
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, e.X, graphY1, graphX1 - e.X, e.Y - graphY1);
                        shapesDrawn.Add("REF*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    else
                    {
                        g.DrawRectangle(p, e.X, graphY1, graphX1 - e.X, e.Y - graphY1);
                        shapesDrawn.Add("REN*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    
                }
                else
                {
                    //label2.Text = " 4o tetartimorio " + graphX1.ToString() + "-" + graphY1.ToString() + "  " + e.X.ToString() + "-" + e.Y.ToString();
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, graphX1, graphY1, e.X - graphX1, e.Y - graphY1);
                        shapesDrawn.Add("REF*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.X - graphX1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    else
                    {
                        g.DrawRectangle(p, graphX1, graphY1, e.X - graphX1, e.Y - graphY1);
                        shapesDrawn.Add("REN*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.X - graphX1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }

                }
            }
            else if (s.Equals("elipse"))
            {
                if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, graphX1, e.Y, e.X - graphX1, graphY1 - e.Y);
                        shapesDrawn.Add("ELF*" + (graphX1).ToString() + "*" + (e.Y).ToString() + "*" + (e.X - graphX1).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    else
                    {
                        g.DrawEllipse(p, graphX1, e.Y, e.X - graphX1, graphY1 - e.Y);
                        shapesDrawn.Add("ELN*" + (graphX1).ToString() + "*" + (e.Y).ToString() + "*" + (e.X - graphX1).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, e.X, e.Y, graphX1 - e.X, graphY1 - e.Y);
                        shapesDrawn.Add("ELF*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawEllipse(p, e.X, e.Y, graphX1 - e.X, graphY1 - e.Y);
                        shapesDrawn.Add("ELN*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }

                }
                else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, e.X, graphY1, graphX1 - e.X, e.Y - graphY1);
                        shapesDrawn.Add("ELF*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawEllipse(p, e.X, graphY1, graphX1 - e.X, e.Y - graphY1);
                        shapesDrawn.Add("ELN*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (graphX1 - e.X).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, graphX1, graphY1, e.X - graphX1, e.Y - graphY1);
                        shapesDrawn.Add("ELF*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.X - graphX1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawEllipse(p, graphX1, graphY1, e.X - graphX1, e.Y - graphY1);
                        shapesDrawn.Add("ELN*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.X - graphX1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
            }
            else if (s.Equals("square"))
            {
                if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, graphX1, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("SQF*" + (graphX1).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    else
                    {
                        g.DrawRectangle(p, graphX1, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("SQN*" + (graphX1).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, e.X, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("SQF*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawRectangle(p, e.X, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("SQN*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, e.X, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("SQF*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawRectangle(p, e.X, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("SQN*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillRectangle(myBrush, graphX1, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("SQF*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawRectangle(p, graphX1, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("SQN*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
            }
            else if (s.Equals("circle"))
            {
                if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, graphX1, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("CIF*" + (graphX1).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                    }
                    else
                    {
                        g.DrawEllipse(p, graphX1, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("CIN*" + (graphX1).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, e.X, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("CIF*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawEllipse(p, e.X, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                        shapesDrawn.Add("CIN*" + (e.X).ToString() + "*" + (e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + (graphY1 - e.Y).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }

                }
                else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, e.X, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("CIF*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawEllipse(p, e.X, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("CIN*" + (e.X).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
                else
                {
                    if (checkBox2.Checked)
                    {
                        SolidBrush myBrush = new SolidBrush(p.Color);
                        g.FillEllipse(myBrush, graphX1, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("CIF*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                    else
                    {
                        g.DrawEllipse(p, graphX1, graphY1, e.Y - graphY1, e.Y - graphY1);
                        shapesDrawn.Add("CIN*" + (graphX1).ToString() + "*" + (graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + (e.Y - graphY1).ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());

                    }
                }
            }

            //when the shape is drawn
            mousedown = false;  //mouse is now up
            g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
            undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
            pictureBox3.Image = buffer;         //picturebox displays the buffer

            savedShapesUndo.Add(new List<string>(shapesDrawn));
        }

        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
            //when the mouse movesinside the picturebox display the coordinates
            label5.Text = e.X.ToString() + " , " + e.Y.ToString();

            //code for free hand
            if (freehand)
            {
                g.DrawLine(p, graphX1, graphY1, e.X, e.Y);
                graphX1 = e.X;
                graphY1 = e.Y;
                g = Graphics.FromImage(buffer);
                pictureBox3.Image = buffer;

            }

            /*

                this code is for displaying the shape as the user is moving the button before he lifts the mouse
                this worls by drawing the shape on the current mouse position and clearing the canvas every time the mouse moves
                in order not to interfier with the graphics of the buffer we are drawing on the picturebox instead by using the gh Graphics object
                and we clear with the Invalidate() mathod

            */

            pictureBox3.Invalidate();   //clear the picturebox

            if (mousedown)
            {
                //draw shape to the current position
                //notice that we use gh instead of g so we are now drawing on the picturebox and not on the buffer
                //all shapes are drawn with as before
                if (s.Equals("line"))
                {
                    gh.DrawLine(p, graphX1, graphY1, e.X, e.Y);
                }
                else if (s.Equals("rectangle"))
                {
                    if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawRectangle(p, graphX1, e.Y, e.X - graphX1, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawRectangle(p, e.X, e.Y, graphX1 - e.X, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                    {
                        gh.DrawRectangle(p, e.X, graphY1, graphX1 - e.X, e.Y - graphY1);
                    }
                    else
                    {
                        gh.DrawRectangle(p, graphX1, graphY1, e.X - graphX1, e.Y - graphY1);
                    }
                }
                else if (s.Equals("elipse"))
                {
                    if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawEllipse(p, graphX1, e.Y, e.X - graphX1, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawEllipse(p, e.X, e.Y, graphX1 - e.X, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                    {
                        gh.DrawEllipse(p, e.X, graphY1, graphX1 - e.X, e.Y - graphY1);
                    }
                    else
                    {
                        gh.DrawEllipse(p, graphX1, graphY1, e.X - graphX1, e.Y - graphY1);
                    }
                }
                else if (s.Equals("square"))
                {
                    if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawRectangle(p, graphX1, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawRectangle(p, e.X, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                    {
                        gh.DrawRectangle(p, e.X, graphY1, e.Y - graphY1, e.Y - graphY1);
                    }
                    else
                    {
                        gh.DrawRectangle(p, graphX1, graphY1, e.Y - graphY1, e.Y - graphY1);
                    }
                }
                else if (s.Equals("circle"))
                {
                    if (e.X - graphX1 > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawEllipse(p, graphX1, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && graphY1 - e.Y > 0)
                    {
                        gh.DrawEllipse(p, e.X, e.Y, graphY1 - e.Y, graphY1 - e.Y);
                    }
                    else if (graphX1 - e.X > 0 && e.Y - graphY1 > 0)
                    {
                        gh.DrawEllipse(p, e.X, graphY1, e.Y - graphY1, e.Y - graphY1);
                    }
                    else
                    {
                        gh.DrawEllipse(p, graphX1, graphY1, e.Y - graphY1, e.Y - graphY1);
                    }
                }
            }

        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            label5.Text = "Mouse Outside of canvas"; //when the mouse leaves, update the label
        }

        //  --  -- //


        //button clear
        private void button3_Click(object sender, EventArgs e)
        {
            //works by going to the first object of list undo
            try
            {
                buffer = (Bitmap)undo[0].Clone();   //buffer becomes first element of undo list
                g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                pictureBox3.Image = buffer;         //picturebox displays the buffer
                undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo list

                shapesDrawn = new List<string>(savedShapesUndo[0]); //shapesDrawn works in the same way
                savedShapesUndo.Add(new List<string>(shapesDrawn));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Canvas has already been cleared", "My Drawing Application",0,MessageBoxIcon.Asterisk);
                undo.Add((Bitmap)buffer.Clone());   //incase the undo list is empty
                return;
            }
        }

        //button undo
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap temp = (Bitmap)buffer.Clone();       //temp is clone of current buffer object
                redo.Add(temp);                             //temp is appended to the redo list

                buffer = (Bitmap)undo[undo.Count - 2].Clone();  //buffer becomes next-to-last(proteleutaio) object of undo list

                undo.RemoveAt(undo.Count - 1);                  //remove the last object of undo (because the last obj of undo is always the current buffer)
                g = Graphics.FromImage(buffer);
                pictureBox3.Image = buffer;                     //picturebox displays the buffer


                List<string> tempS = new List<string>(shapesDrawn); //shapesDrawn works in the same way
                savedShapesRedo.Add(tempS);

                shapesDrawn = new List<string>(savedShapesUndo[savedShapesUndo.Count - 2]); 
                savedShapesUndo.RemoveAt(savedShapesUndo.Count - 1);

            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Cannot undo anymore", "My Drawing Application",0,MessageBoxIcon.Warning);   //incase the undo list becomes empty
                return;
            }
        }

        //button redo
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                buffer = redo[redo.Count - 1];      //buffer becomes last object of redo list
                pictureBox3.Image = buffer;         //picturebox displays the buffer
                redo.RemoveAt(redo.Count - 1);      //remove last object of redo list  
                undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo list
                g = Graphics.FromImage(buffer);


                shapesDrawn = savedShapesRedo[savedShapesRedo.Count - 1]; //shapesDrawn works in the same way
                savedShapesRedo.RemoveAt(savedShapesRedo.Count - 1);
                savedShapesUndo.Add(new List<string>(shapesDrawn));

            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Cannot redo anymore", "My Drawing Application",0,MessageBoxIcon.Warning);   //incase the redo list becomes empty
                return;
            }
        }

        //button save (export to png)
        private void button14_Click(object sender, EventArgs e)
        {
            //user saveFileDialog to save the current buffer object as a png image
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                buffer.Save(saveFileDialog1.FileName);
            }
        }



        //////////////////   Fractal Binary Tree   ////////////////////////

        //button draw fractal binary tree instant
        private void button10_Click(object sender, EventArgs e)
        {
            //fractal binary tree is drawn using a recursive method
            fractalTree(new Point((int)numericUpDown6.Value, (int)numericUpDown7.Value), Math.PI / 2, (int)numericUpDown5.Value, ((float)numericUpDown8.Value / 180) * Math.PI);
            //when the shape is drawn
            //mousedown = false;  //mouse is now up
            g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
            undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
            pictureBox3.Image = buffer;         //picturebox displays the buffer
            
            //add to shape history
            shapesDrawn.Add("FBT"+"*"+ numericUpDown5.Value.ToString()+"*"+ numericUpDown6.Value.ToString()+"*"+ numericUpDown7.Value.ToString()+"*"+ numericUpDown8.Value.ToString()+"*"+p.Color.R.ToString()+"*"+p.Color.G.ToString() +"*"+ p.Color.B.ToString()  + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
            savedShapesUndo.Add(new List<string>(shapesDrawn));
        }

        //fractal binary tree recursive function
        private void fractalTree(Point root,double angle,double lenght ,double offset)
        {
            if (lenght < 10) return;
            int x1 = root.X + (int)(Math.Cos(angle) * lenght);
            int y1 = root.Y - (int)(Math.Sin(angle) * lenght);

            Point next = new Point(x1, y1);

            g.DrawLine(p, root, next);

            fractalTree(next, angle + offset, lenght*0.8, offset);
            fractalTree(next, angle - offset, lenght * 0.8, offset);
        }

        //button draw fractal binary tree animated
        private void button12_Click(object sender, EventArgs e)
        {
            if (timer2.Enabled)
            {
                MessageBox.Show("Please wait for the shape to be drawn","My Drawing Application",0,MessageBoxIcon.Error);
                return;
            }
            //set up the parameters to the fields
            layer = 1;
            length = (int)numericUpDown5.Value;
            points.Clear();
            angles.Clear();
            O = new Point((int)numericUpDown6.Value, (int)numericUpDown7.Value);
            points.Add(O);
            offset = ((float)numericUpDown8.Value / 180) * Math.PI;
            //enable the timer
            timer2.Enabled = true;
        }

        //timer for binary tree animated
        private void timer2_Tick(object sender, EventArgs e)
        {
            //tree stops when length < 10
            if (length < 10)
            {
                g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                pictureBox3.Image = buffer;         //picturebox displays the buffer
                //add to shape history
                shapesDrawn.Add("FBT" + "*" + numericUpDown5.Value.ToString() + "*" + numericUpDown6.Value.ToString() + "*" + numericUpDown7.Value.ToString() + "*" + numericUpDown8.Value.ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                savedShapesUndo.Add(new List<string>(shapesDrawn));

                timer2.Enabled = false;
            }
            //the tree starts with a single straight line
            //to draw the fractal we utilize lists for the points and the angles of the branches
            else if (layer == 1)
            {
                //Console.WriteLine("layer==1");
                Point p1 = new Point(points[0].X + (int)(length * Math.Cos(angle)), points[0].Y - (int)(length * Math.Sin(angle)));
                g.DrawLine(p, points[0], p1);
                g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                pictureBox3.Image = buffer;         //picturebox displays the buffer
                points.RemoveAt(0);
                points.Add(p1);
                angles.Add(angle);
                angles.Add(angle);
                length = length * 0.8;
                layer += 1;
            }          
            else
            {
                long initialsize = points.Count;
                int anglessize = angles.Count; 

                for (int i = 0; i < initialsize; i++)
                {
                    Point p1 = new Point(points[i].X + (int)(length * Math.Cos(angles[i]  + offset)), points[i].Y - (int)(length * Math.Sin(angles[i] + offset)));
                    Point p2 = new Point(points[i].X + (int)(length * Math.Cos(angles[i]  - offset)), points[i].Y - (int)(length * Math.Sin(angles[i] - offset)));
                    Pen newpen1 = new Pen(Color.Blue, 2.0f);
                    Pen newpen2 = new Pen(Color.Black, 2.0f);
                    g.DrawLine(p, points[i], p1);
                    g.DrawLine(p, points[i], p2);
                    g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                    //undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                    pictureBox3.Image = buffer;         //picturebox displays the buffer
                    points.Add(p1);
                    points.Add(p2);
                    angles.Add(angles[i] + offset);
                    angles.Add(angles[i] - offset);
                }
                length = length * 0.8;
                layer += 1;

                for (int i = 0; i < initialsize; i++)
                {
                    points.RemoveAt(0);
                }

                for (int i = 0; i < anglessize; i++)
                {
                    angles.RemoveAt(0);
                }
            }
        }

        //////////////////////////////////////////




        //////////////////   Cycle Shape   ////////////////////////

        //the cycle shape is a cycle consisting of other cycles and is generated by a mathematical formula

        //button draw cycle shape instant
        private void button18_Click_1(object sender, EventArgs e)
        {
            //to draw the shape instantly, we find all the points of the shape and then draw them
            
            n = 0.01;   //oso mikrotero to n toso megaliteri akriveia (gia n=0.02 sxediazei 14gwna)
            maxPoints = 8 / n;
            Point[] points = new Point[(int)maxPoints + 10];
            for (int k=0;k< (int)maxPoints + 10; k++)
            {
                double x1 = (double)numericUpDown19.Value + 200 * Math.Cos((Math.PI / 4) * k * n) + (double)numericUpDown17.Value * Math.Cos(((Math.PI / 4) * (double)(numericUpDown16.Value)) * k * n);
                double y1 = (double)numericUpDown18.Value + 200 * Math.Sin((Math.PI / 4) * k * n) + (double)numericUpDown17.Value * Math.Sin(((Math.PI / 4) * (double)(numericUpDown16.Value)) * k * n);
                points[k] = new Point((int)x1, (int)y1);               
            }
            g.DrawLines(p, points);            
            undo.Add((Bitmap)buffer.Clone());
            pictureBox3.Image = buffer;

            shapesDrawn.Add("CYS" + "*" + numericUpDown17.Value.ToString() + "*" + numericUpDown16.Value.ToString() + "*" + numericUpDown19.Value.ToString() + "*" + numericUpDown18.Value.ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
            savedShapesUndo.Add(new List<string>(shapesDrawn));

        }

        //button draw cycle shape animated
        private void button13_Click(object sender, EventArgs e)
        {
            if (timer3.Enabled)
            {
                MessageBox.Show("Please wait for the shape to be drawn", "My Drawing Application", 0, MessageBoxIcon.Error);
                return;
            }

            n = 0.01;//oso mikrotero to n toso megaliteri akriveia (gia n=0.02 sxediazei 14gwna)
            k = 0;
            //find the first point 
            double x1 = (double)numericUpDown19.Value + 200 * Math.Cos((Math.PI / 4) * k * n) + (double)numericUpDown17.Value * Math.Cos(((Math.PI / 4) * (double)(numericUpDown16.Value)) * k * n);
            double y1 = (double)numericUpDown18.Value + 200 * Math.Sin((Math.PI / 4) * k * n) + (double)numericUpDown17.Value * Math.Sin(((Math.PI / 4) * (double)(numericUpDown16.Value)) * k * n);
            prev = new Point((int)x1, (int)y1);
            k = 1;
            maxPoints = 8 / n;

            shapesDrawn.Add("CYS" + "*" + numericUpDown17.Value.ToString() + "*" + numericUpDown16.Value.ToString() + "*" + numericUpDown19.Value.ToString() + "*" + numericUpDown18.Value.ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
            savedShapesUndo.Add(new List<string>(shapesDrawn));

            timer3.Enabled = true;
        }

        //timer for cycle shape animated
        private void timer3_Tick(object sender, EventArgs e)
        {
            //find the next point and draw a line between the previous and the first point

            double x1 = (double)numericUpDown19.Value + 200 * Math.Cos((Math.PI / 4) * k *n) + (double)numericUpDown17.Value * Math.Cos(((Math.PI / 4) * (double)(numericUpDown16.Value)) * k * n);
            double y1 = (double)numericUpDown18.Value + 200 * Math.Sin((Math.PI / 4) * k *n) + (double)numericUpDown17.Value * Math.Sin(((Math.PI / 4) * (double)(numericUpDown16.Value)) * k * n);         
            Point next = new Point((int)x1, (int)y1);

            //Console.WriteLine(next.ToString());

            g.DrawLine(p, prev, next);

            pictureBox3.Image = buffer;

            prev = next;

            k += 1;
            if (k == (int)maxPoints + 10)
            {
                undo.Add((Bitmap)buffer.Clone());
                timer3.Enabled = false; 
            }
                
        }

        //////////////////////////////////////////



        ///////////////////   Star   ////////////////////////

        //button draw star instant
        private void button19_Click(object sender, EventArgs e)
        {
            //we get the points of the star from a method of the class Star
            Point[] centers = Star.starPoints(new Point((int)numericUpDown10.Value, (int)numericUpDown9.Value), (int)numericUpDown12.Value);

            if (!checkBox1.Checked)
            {
                g.DrawLines(p, centers);
                shapesDrawn.Add("STN" + "*" + numericUpDown10.Value.ToString() + "*"+ numericUpDown9.Value.ToString() + "*" + numericUpDown12.Value.ToString() + "*"+ p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                savedShapesUndo.Add(new List<string>(shapesDrawn));
            }              
            else
            {
                SolidBrush myBrush = new SolidBrush(p.Color);
                g.FillPolygon(myBrush, centers);
                shapesDrawn.Add("STF" + "*" + numericUpDown10.Value.ToString() + "*" + numericUpDown9.Value.ToString() + "*" + numericUpDown12.Value.ToString() + "*" + p.Color.R.ToString() + "*"  + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                savedShapesUndo.Add(new List<string>(shapesDrawn));
            }

            g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
            undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
            pictureBox3.Image = buffer;         //picturebox displays the buffer


        }

        //button draw star animated
        private void button20_Click(object sender, EventArgs e)
        {

            if (timer4.Enabled)
            {
                MessageBox.Show("Please wait for the shape to be drawn", "My Drawing Application", 0, MessageBoxIcon.Error);
                return;
            }
            //get the points for the star
            starPoints = Star.starPoints(new Point((int)numericUpDown10.Value, (int)numericUpDown9.Value), (int)numericUpDown12.Value);
            //set up the field
            pointTodraw = 0;

            if (checkBox1.Checked)
            {
                shapesDrawn.Add("STF" + "*" + numericUpDown10.Value.ToString() + "*" + numericUpDown9.Value.ToString() + "*" + numericUpDown12.Value.ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                savedShapesUndo.Add(new List<string>(shapesDrawn));
            }
            else
            {
                shapesDrawn.Add("STN" + "*" + numericUpDown10.Value.ToString() + "*" + numericUpDown9.Value.ToString() + "*" + numericUpDown12.Value.ToString() + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
                savedShapesUndo.Add(new List<string>(shapesDrawn));
            }

            timer4.Enabled = true;
        }

        //timer for star animated
        private void timer4_Tick(object sender, EventArgs e)
        {
            //drawing finished
            if (pointTodraw + 2 > starPoints.Length)
            {
                if (checkBox1.Checked)
                {
                    SolidBrush myBrush = new SolidBrush(p.Color);
                    g.FillPolygon(myBrush, starPoints);
                }
                g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                pictureBox3.Image = buffer;         //picturebox displays the buffer
                timer4.Enabled = false;
            }
            //draw line one by one
            else
            {
                g.DrawLine(p, starPoints[pointTodraw], starPoints[pointTodraw + 1]);
                pointTodraw += 1;
                g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                pictureBox3.Image = buffer;         //picturebox displays the buffer
            }
        }

        //////////////////////////////////////////////////////////



        /////////////////////   House   ////////////////////////

        //button draw house instant
        private void button11_Click(object sender, EventArgs e)
        {
            //when the user chooses the house to be filled, get the colors from the 2 buttons in the groupbox

            //get points from method housePoints of class Star
            Point[] pointsAll = Star.housePoints(new Point((int)numericUpDown13.Value, (int)numericUpDown11.Value), (int)numericUpDown15.Value, (int)numericUpDown14.Value);
            //create lists of points for the parts of the house
            Point[] outlinePoints = { pointsAll[0], pointsAll[1], pointsAll[2], pointsAll[3], pointsAll[4], pointsAll[0] };
            Point[] window1 = { pointsAll[10], pointsAll[11], pointsAll[12], pointsAll[13], pointsAll[10] };
            Point[] window2 = { pointsAll[14], pointsAll[15], pointsAll[16], pointsAll[17], pointsAll[14] };
            Point[] door = { pointsAll[6], pointsAll[7], pointsAll[8], pointsAll[9], pointsAll[6] };
            Point[] roof = { pointsAll[1], pointsAll[2], pointsAll[3], pointsAll[1] };

            //draw or fill
            if (!checkBox3.Checked)
            {
                g.DrawLines(p, outlinePoints);
                g.DrawLine(p, pointsAll[1], pointsAll[3]);
                g.DrawLines(p, window1);
                g.DrawLines(p, window2);
                g.DrawLines(p, door);
                //add shape to shapesDrawn
                shapesDrawn.Add("HON" + "*" + numericUpDown13.Value.ToString() + "*" + numericUpDown11.Value.ToString() + "*" + numericUpDown15.Value.ToString() + "*" + numericUpDown14.Value + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
            }
            else
            {
                SolidBrush myBrush1 = new SolidBrush(button21.BackColor);
                SolidBrush myBrush2 = new SolidBrush(button22.BackColor);
                g.FillPolygon(myBrush1, outlinePoints);
                g.FillPolygon(myBrush2, roof);
                g.FillPolygon(myBrush2, window1);
                g.FillPolygon(myBrush2, window2);
                g.FillPolygon(myBrush2, door);
                //add shape to shapesDrawn
                shapesDrawn.Add("HOF" + "*" + numericUpDown13.Value.ToString() + "*" + numericUpDown11.Value.ToString() + "*" + numericUpDown15.Value.ToString() + "*" + numericUpDown14.Value + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() +"*" + button21.BackColor.R.ToString() + "*" + button21.BackColor.G.ToString() + "*" + button21.BackColor.B.ToString() + "*" + button22.BackColor.R.ToString() + "*" + button22.BackColor.G.ToString() + "*" + button22.BackColor.B.ToString() + "*" + DateTime.Now.ToString());

            }


            g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
            undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
            pictureBox3.Image = buffer;         //picturebox displays the buffer
            //update the history
            savedShapesUndo.Add(new List<string>(shapesDrawn));
        }

        //button draw house animated
        private void button17_Click_1(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                MessageBox.Show("Please wait for the shape to be drawn", "My Drawing Application", 0, MessageBoxIcon.Error);
                return;
            }

            //set up the fields
            houseAllpoints = Star.housePoints(new Point((int)numericUpDown13.Value, (int)numericUpDown11.Value), (int)numericUpDown15.Value, (int)numericUpDown14.Value);
            outlinepoints = new Point[] { houseAllpoints[0], houseAllpoints[1], houseAllpoints[2], houseAllpoints[3], houseAllpoints[4], houseAllpoints[0] };
            window1 = new Point[] { houseAllpoints[10], houseAllpoints[11], houseAllpoints[12], houseAllpoints[13], houseAllpoints[10] };
            window2 = new Point[] { houseAllpoints[14], houseAllpoints[15], houseAllpoints[16], houseAllpoints[17], houseAllpoints[14] };
            door = new Point[] { houseAllpoints[6], houseAllpoints[7], houseAllpoints[8], houseAllpoints[9], houseAllpoints[6] };
            roof = new Point[] { houseAllpoints[1], houseAllpoints[2], houseAllpoints[3], houseAllpoints[1] };

            housePoints = 0;
            houseStep = 0;

            if (!checkBox3.Checked)
            {
                shapesDrawn.Add("HON" + "*" + numericUpDown13.Value.ToString() + "*" + numericUpDown11.Value.ToString() + "*" + numericUpDown15.Value.ToString() + "*" + numericUpDown14.Value + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + DateTime.Now.ToString());
            }
            else
            {
                shapesDrawn.Add("HOF" + "*" + numericUpDown13.Value.ToString() + "*" + numericUpDown11.Value.ToString() + "*" + numericUpDown15.Value.ToString() + "*" + numericUpDown14.Value + "*" + p.Color.R.ToString() + "*" + p.Color.G.ToString() + "*" + p.Color.B.ToString() + "*" + p.Width.ToString() + "*" + button21.BackColor.R.ToString() + "*" + button21.BackColor.G.ToString() + "*" + button21.BackColor.B.ToString() + "*" + button22.BackColor.R.ToString() + "*" + button22.BackColor.G.ToString() + "*" + button22.BackColor.B.ToString() + "*" + DateTime.Now.ToString());
            }
            savedShapesUndo.Add(new List<string>(shapesDrawn));
            timer1.Enabled = true;
        }

        //timer for house animated
        private void timer1_Tick(object sender, EventArgs e)
        {
            //draw the parts of the house
            switch (houseStep)
            {
                case 0:
                    if (housePoints + 2 > outlinepoints.Length)
                    {
                        houseStep += 1;
                        housePoints = 0;
                    }
                    else
                    {
                        g.DrawLine(p, outlinepoints[housePoints], outlinepoints[housePoints + 1]);
                        housePoints += 1;
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        pictureBox3.Image = buffer;         //picturebox displays the buffer
                    }
                    break;

                case 1:
                    g.DrawLine(p, houseAllpoints[1], houseAllpoints[3]);
                    houseStep += 1;
                    g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                    pictureBox3.Image = buffer;         //picturebox displays the buffer
                    break;

                case 2:
                    if (housePoints + 2 > window1.Length)
                    {
                        houseStep += 1;
                        housePoints = 0;
                    }
                    else
                    {
                        g.DrawLine(p, window1[housePoints], window1[housePoints + 1]);
                        housePoints += 1;
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        pictureBox3.Image = buffer;         //picturebox displays the buffer
                    }
                    break;
                case 3:
                    if (housePoints + 2 > window2.Length)
                    {
                        houseStep += 1;
                        housePoints = 0;
                    }
                    else
                    {
                        g.DrawLine(p, window2[housePoints], window2[housePoints + 1]);
                        housePoints += 1;
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        pictureBox3.Image = buffer;         //picturebox displays the buffer
                    }
                    break;
                case 4:
                    if (housePoints + 2 > door.Length)
                    {
                        houseStep += 1;
                        housePoints = 0;
                    }
                    else
                    {
                        g.DrawLine(p, door[housePoints], door[housePoints + 1]);
                        housePoints += 1;
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        pictureBox3.Image = buffer;         //picturebox displays the buffer
                    }
                    break;

                //finally fill if the user wants
                case 5:
                    if (checkBox3.Checked)
                    {
                        SolidBrush myBrush1 = new SolidBrush(button21.BackColor);
                        SolidBrush myBrush2 = new SolidBrush(button22.BackColor);
                        Pen temp1 = new Pen(button21.BackColor, p.Width);
                        Pen temp2 = new Pen(button22.BackColor, p.Width);

                        g.FillPolygon(myBrush1, outlinepoints);
                        g.FillPolygon(myBrush2, roof);
                        g.FillPolygon(myBrush2, window1);
                        g.FillPolygon(myBrush2, window2);
                        g.FillPolygon(myBrush2, door);

                        g.DrawLines(temp1, outlinepoints);
                        g.DrawLines(temp2, roof);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer
                        timer1.Enabled = false;

                    }
                    else
                    {
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer
                        timer1.Enabled = false;
                    }
                    break;
            }




        }

        //button color 1 fill for house
        private void button21_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).BackColor = colorDialog1.Color;
            }
        }

        //button color 2 fill for house
        private void button22_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).BackColor = colorDialog1.Color;
            }
        }

        //////////////////////////////////////////////////////////







        //function that saves the values of all numericUpdown controls to the list 'savedNumerics'
        private void saveNumerics()
        {
            savedNumerics.Clear();
            foreach(Control control in this.panel1.Controls)
            {
                if (control.GetType() == typeof(NumericUpDown))
                    savedNumerics.Add(((NumericUpDown)control).Value.ToString());
            }

            foreach (Control control in this.panel2.Controls)
            {
                if (control.GetType() == typeof(GroupBox))
                {
                    foreach (Control inner in ((GroupBox)control).Controls)
                    {

                        if (inner.GetType() == typeof(NumericUpDown))
                            savedNumerics.Add(((NumericUpDown)inner).Value.ToString());
                    }
                }

            }
        }

        private void loadNumerics()
        {
            foreach (Control control in this.panel1.Controls)
            {
                if (control.GetType() == typeof(NumericUpDown))
                {
                    NumericUpDown nud = (NumericUpDown)control;
                    try
                    {                      
                        nud.Value = int.Parse(savedNumerics[0]);
                        savedNumerics.RemoveAt(0);
                    }
                    catch (System.FormatException)
                    {
                        float f = float.Parse(savedNumerics[0]);
                        nud.Value = new decimal(f);
                        savedNumerics.RemoveAt(0);
                    }

                }

            }

            foreach (Control control in this.panel2.Controls)
            {
                if (control.GetType() == typeof(GroupBox))
                {
                    foreach (Control inner in ((GroupBox)control).Controls)
                    {

                        if (inner.GetType() == typeof(NumericUpDown))
                        {
                            NumericUpDown nud = (NumericUpDown)inner;
                            nud.Value = int.Parse(savedNumerics[0]);
                            savedNumerics.RemoveAt(0);
                        }

                    }
                }

            }
        }

        //this functions returns a string that contains information about all the currently drawn shapes
        public string shapesDrawnToData()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string s in shapesDrawn)
            {
                sb.Append(s);
                sb.Append("~");
            }

            return sb.ToString();
        }

        //function that takes the string and draws the shapes
        //string is from the database
        public void parser(string input)
        {
            button3.PerformClick(); //clear the canvas
            saveNumerics();         //save current values of all numericUpDown controls

            String[] shapes = input.Split('~');
            foreach(string shape in shapes)
            {
                String[] data = shape.Split('*');
                float f;
                bool backup;
                SolidBrush tempBrush;
                DateTime dt;
                //for the special shapes, we pass the parameters to the numericUpDowns and click the 'instant draw' button
                //we also have the timestamp of each shape in the database in the end of each word

                switch (data[0])
                {
                    case "FBT":
                        //Console.WriteLine("fractal binary tree");
                        numericUpDown5.Value = int.Parse(data[1]);
                        numericUpDown6.Value = int.Parse(data[2]);
                        numericUpDown7.Value = int.Parse(data[3]);
                        numericUpDown8.Value = int.Parse(data[4]);

                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        button10.PerformClick();

                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "STN":
                        //Console.WriteLine("star not filled");

                        numericUpDown10.Value = int.Parse(data[1]);
                        numericUpDown9.Value = int.Parse(data[2]);
                        numericUpDown12.Value = int.Parse(data[3]);

                        backup = checkBox1.Checked;
                        checkBox1.Checked = false;

                        numericUpDown1.Value = int.Parse(data[4]);
                        numericUpDown2.Value = int.Parse(data[5]);
                        numericUpDown3.Value = int.Parse(data[6]);

                        f = (float.Parse(data[7]));
                        numericUpDown4.Value = new decimal(f);
                        button19.PerformClick();
                        checkBox1.Checked = backup;
                        dt = DateTime.Parse(data[8]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "STF":
                        //Console.WriteLine("star filled");
                        numericUpDown10.Value = int.Parse(data[1]);
                        numericUpDown9.Value = int.Parse(data[2]);
                        numericUpDown12.Value = int.Parse(data[3]);
                        backup = checkBox1.Checked;
                        checkBox1.Checked = true;

                        numericUpDown1.Value = int.Parse(data[4]);
                        numericUpDown2.Value = int.Parse(data[5]);
                        numericUpDown3.Value = int.Parse(data[6]);

                        f = (float.Parse(data[7]));
                        numericUpDown4.Value = new decimal(f);
                        button19.PerformClick();
                        checkBox1.Checked = backup;
                        dt = DateTime.Parse(data[8]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "HON":
                        //Console.WriteLine("house not filled");
                        numericUpDown13.Value = int.Parse(data[1]);
                        numericUpDown11.Value = int.Parse(data[2]);
                        numericUpDown15.Value = int.Parse(data[3]);
                        numericUpDown14.Value = int.Parse(data[4]);
                        backup = checkBox3.Checked; //save initial value of checkbox
                        checkBox3.Checked = false;


                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);
                        button11.PerformClick();
                        checkBox3.Checked = backup; //restore initial value of checkbox
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "HOF":
                        //Console.WriteLine("house filled");

                        numericUpDown13.Value = int.Parse(data[1]);
                        numericUpDown11.Value = int.Parse(data[2]);
                        numericUpDown15.Value = int.Parse(data[3]);
                        numericUpDown14.Value = int.Parse(data[4]);
                        backup = checkBox3.Checked;
                        checkBox3.Checked = true;

                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        Color backup1, backup2;

                        backup1 = button21.BackColor;
                        backup2 = button22.BackColor;

                        button21.BackColor = Color.FromArgb(int.Parse(data[9]), int.Parse(data[10]), int.Parse(data[11]));
                        button22.BackColor = Color.FromArgb(int.Parse(data[12]), int.Parse(data[13]), int.Parse(data[14]));

                        button11.PerformClick();
                        checkBox3.Checked = backup;

                        button21.BackColor = backup1;
                        button22.BackColor = backup2;
                        dt = DateTime.Parse(data[15]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "CYS":
                        //Console.WriteLine("cycle shape");
                        numericUpDown17.Value = int.Parse(data[1]);
                        numericUpDown16.Value = int.Parse(data[2]);
                        numericUpDown19.Value = int.Parse(data[3]);
                        numericUpDown18.Value = int.Parse(data[4]);

                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);
                        button18.PerformClick();
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "REF":
                        //Console.WriteLine("rectangle filled");
                        

                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        tempBrush = new SolidBrush(p.Color);
                        g.FillRectangle(tempBrush, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());

                        break;
                    case "REN":
                        //Console.WriteLine("rectangle not filled");
                        //Console.WriteLine(shape);
                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        g.DrawRectangle(p, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        //Console.WriteLine(data[9]);
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "LIN":
                        //Console.WriteLine("line");
                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        g.DrawLine(p, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));

                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());

                        break;
                    case "ELF":
                        //Console.WriteLine("elipse filled");


                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        tempBrush = new SolidBrush(p.Color);
                        g.FillEllipse(tempBrush, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));

                        dt = DateTime.Parse(data[9]);
                       // Console.WriteLine(dt.ToString());

                        break;
                    case "ELN":
                        //Console.WriteLine("elipse not filled");
                        //Console.WriteLine(shape);
                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        g.DrawEllipse(p, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "SQF":
                        //Console.WriteLine("square filled");


                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        tempBrush = new SolidBrush(p.Color);
                        g.FillRectangle(tempBrush, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));

                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());

                        break;
                    case "SQN":
                        //Console.WriteLine("square not filled");


                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        g.DrawRectangle(p, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                    case "CIF":
                        //Console.WriteLine("cycle filled");


                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        tempBrush = new SolidBrush(p.Color);
                        g.FillEllipse(tempBrush, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());


                        break;
                    case "CIN":
                        //Console.WriteLine("cycle not filled");


                        numericUpDown1.Value = int.Parse(data[5]);
                        numericUpDown2.Value = int.Parse(data[6]);
                        numericUpDown3.Value = int.Parse(data[7]);

                        f = (float.Parse(data[8]));
                        numericUpDown4.Value = new decimal(f);

                        g.DrawEllipse(p, int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                        //Console.WriteLine(shape);

                        shapesDrawn.Add(shape);
                        g = Graphics.FromImage(buffer);     //g takes the graphics of the buffer
                        undo.Add((Bitmap)buffer.Clone());   //add the current buffer to the undo
                        pictureBox3.Image = buffer;         //picturebox displays the buffer

                        savedShapesUndo.Add(new List<string>(shapesDrawn));
                        dt = DateTime.Parse(data[9]);
                        //Console.WriteLine(dt.ToString());
                        break;
                }
            }
            loadNumerics();//restore numericUpDowns
        }



        ///////////////////////  menustrip    ///////////////////////////////

        //menustrip edit/undo
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2.PerformClick();
        }

        //menustrip edit/redo
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1.PerformClick();

        }

        //menustrip edit/clear
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3.PerformClick();
        }

        //menustrip : file/exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //menustrip export to png
        private void exportTopngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button14.PerformClick();
        }

        //menu strip themes : 
        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applyLightTheme();
            lightToolStripMenuItem.Checked = true;
            darkToolStripMenuItem.Checked = false;
            orangeBlueToolStripMenuItem.Checked = false;
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applyDarktTheme();
            lightToolStripMenuItem.Checked = false;
            darkToolStripMenuItem.Checked = true;
            orangeBlueToolStripMenuItem.Checked = false;
        }

        private void orangeBlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applyOrangeBlueTheme();
            lightToolStripMenuItem.Checked = false;
            darkToolStripMenuItem.Checked = false;
            orangeBlueToolStripMenuItem.Checked = true;
        }

        //menustrip upload to database
        private void uploadToDataBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!form2Active)
            {
                form2 = new Form2(this);
                form2.Show();
                form2Active = true;
            }
            else
            {
                form2.Focus();
            }
        }

        //menustrip browse database
        private void browseDataBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!form3Active)
            {
                form3 = new Form3(this);
                form3.Show();
                form3Active = true;
            }
            else
            {
                form3.Focus();
            }
        }

        //////////////////////////////////////////


        //prompt before the form closes
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit the application?", "My Drawing Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }



        //when form1 gets resize , move canvas to the center
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            pictureBox3.Location = new Point((this.Width) / 2 - pictureBox3.Width / 2, this.Height / 2 - pictureBox3.Height / 2 - 25);
        }

        //form1 keybindings/shortcuts
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                button2.PerformClick();
            }

            if (e.Control && e.KeyCode == Keys.Y)
            {
                button1.PerformClick();
            }
        }

        //when the form has closed, close everything 
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

    }


}
