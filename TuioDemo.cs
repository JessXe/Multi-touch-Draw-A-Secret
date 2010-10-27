using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;   //Added because it was used in TUIO demo - Jesse
using System.Threading;     //Added because it was used in TUIO demo - Jesse
using TUIO;

namespace Thesis_Security_App_1._1
{
    public class TuioDemo : Form, TuioListener
    {
        /// <summary>
        /// Primary logic class for DrawASecret Manipulation
        /// </summary>
        private TuioClient client;
        private Dictionary<long, TuioDemoObject> objectList;
        private Dictionary<long, TuioCursor> cursorList;
        private object cursorSync = new object();
        private object objectSync = new object();

        public static int width;
        public static int height;
        private int window_width = 640;
        private int window_height = 480;
        private int window_left = 0;
        private int window_top = 0;
        private int screen_width = Screen.PrimaryScreen.Bounds.Width;
        private int screen_height = Screen.PrimaryScreen.Bounds.Height;

        private bool fullscreen;
        private bool verbose;

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox1;

        SolidBrush blackBrush = new SolidBrush(Color.Black);
        SolidBrush whiteBrush = new SolidBrush(Color.White);

        SolidBrush grayBrush = new SolidBrush(Color.Gray);
        Pen fingerPen = new Pen(new SolidBrush(Color.Blue), 1);

        Grid mainGrid;
        Queue<int>[] code = new Queue<int>[10];
        Queue<int>[] prev_code = new Queue<int>[10];

        public TuioDemo(int port)
        {
          //  InitializeComponent();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            InitializeQueueArray(code);
            InitializeQueueArray(prev_code);
            int x = 5;
            int y = 5;
            lastnode = 999;
            mainGrid = new Grid(x, y, this);

            //Button 1
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Register";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //Button 2
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Finish";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            //Button 3
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Authenticate";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            //Textbox
            this.textBox1.ForeColor = System.Drawing.Color.Red;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(286, 20);
            this.textBox1.TabIndex = 3;

            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);

            verbose = false;
            fullscreen = false;
            width = mainGrid.getWidth();       //Used important, used for positioning cursors and such
            height = mainGrid.getHeight();     //Used important, used for positioning cursors and such

            this.ClientSize = new System.Drawing.Size(width, height);
            this.Name = "TuioDemo";     //cut
            this.Text = "TuioDemo";     //cut

            this.Closing += new CancelEventHandler(Form_Closing);//keep
            this.KeyDown += new KeyEventHandler(Form_KeyDown);//keep

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |//keep
                            ControlStyles.UserPaint |
                            ControlStyles.DoubleBuffer, true);

            objectList = new Dictionary<long, TuioDemoObject>(128);//keep
            cursorList = new Dictionary<long, TuioCursor>(128);//keep

            client = new TuioClient(port);//keep
            client.addTuioListener(this);//keep
            client.connect();//keep
        }

        public void InitializeQueueArray(Queue<int>[] toInit)
        {
            for (int x = 0; x < toInit.GetLength(0); x++)
            {
                toInit[x] = new Queue<int>();
            }
        }

        private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            if (e.KeyData == Keys.F1)
            {
                if (fullscreen == false)
                {

                    width = screen_width;
                    height = screen_height;

                    window_left = this.Left;
                    window_top = this.Top;

                    this.FormBorderStyle = FormBorderStyle.None;
                    this.Left = 0;
                    this.Top = 0;
                    this.Width = screen_width;
                    this.Height = screen_height;

                    fullscreen = true;
                }
                else
                {

                    width = window_width;
                    height = window_height;

                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.Left = window_left;
                    this.Top = window_top;
                    this.Width = window_width;
                    this.Height = window_height;

                    fullscreen = false;
                }
            }
            else if (e.KeyData == Keys.Escape)
            {
                this.Close();

            }
            else if (e.KeyData == Keys.V)
            {
                verbose = !verbose;
            }

        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.removeTuioListener(this);

            client.disconnect();
            System.Environment.Exit(0);
        }

        public void addTuioObject(TuioObject o)
        {
            lock (objectSync)
            {
                objectList.Add(o.getSessionID(), new TuioDemoObject(o));
            } if (verbose) Console.WriteLine("add obj " + o.getSymbolID() + " (" + o.getSessionID() + ") " + o.getX() + " " + o.getY() + " " + o.getAngle());
        }

        public void updateTuioObject(TuioObject o)
        {
            lock (objectSync)
            {
                objectList[o.getSessionID()].update(o);
            }
            if (verbose) Console.WriteLine("set obj " + o.getSymbolID() + " " + o.getSessionID() + " " + o.getX() + " " + o.getY() + " " + o.getAngle() + " " + o.getMotionSpeed() + " " + o.getRotationSpeed() + " " + o.getMotionAccel() + " " + o.getRotationAccel());
        }

        public void removeTuioObject(TuioObject o)
        {
            lock (objectSync)
            {
                objectList.Remove(o.getSessionID());
            }
            if (verbose) Console.WriteLine("del obj " + o.getSymbolID() + " (" + o.getSessionID() + ")");
        }

        public void addTuioCursor(TuioCursor c)
        {
            lock (cursorSync)
            {
                cursorList.Add(c.getSessionID(), c);
            }
            if (verbose) Console.WriteLine("add cur " + c.getCursorID() + " (" + c.getSessionID() + ") " + c.getX() + " " + c.getY());
        }

        public void updateTuioCursor(TuioCursor c)
        {
            if (verbose) Console.WriteLine("set cur " + c.getCursorID() + " (" + c.getSessionID() + ") " + c.getX() + " " + c.getY() + " " + c.getMotionSpeed() + " " + c.getMotionAccel());
            mainGrid.checkCursor((c.getX() * TuioDemo.width), (c.getY() * TuioDemo.height), c.getCursorID(), code);            
        }

        public void SortCode() //Consider upgrading to level 2 or level 3, also currently won't deal with two patterns starting at the same node.
        {
        int cur = 0;
        int nxt = 0;
        bool flg = true;
        Queue<int> tmp;
        while (flg)
        {
            flg = false;
            for (int x = 0; x < code.GetLength(0) - 1; x++)
            {
                if (code[x].Count < 1 || code[x + 1].Count < 1) break;
                cur = code[x].Peek();
                nxt = code[x + 1].Peek();
                if (cur > nxt)
                {
                    tmp = code[x];
                    code[x] = code[x + 1];
                    code[x + 1] = tmp;
                    flg = true;
                }
            }
        }
        }

        public void WriteOutCode(string filepath)
        {
            /// <summary>
            /// Creates the file to store the authentication information in
            /// and encrypts it.
            /// </summary>
            /// <param name="filepath">
            /// The full file path to write out to.
            /// </param>
            FileStream fs = new FileStream(filepath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            SortCode();
            for (int x = 0; x < code.GetLength(0); x++)
            {
                while(code[x].Count>0)
                {
                    sb.Append(code[x].Dequeue()+",");
                }
                sb.Append("\n,");
            }
            Debug.Write(sb.ToString());
            sw.Write(Encryption.enc(sb.ToString()));
            sw.Close();
        }

        public bool ReadInCode(string filepath)
        {
            /// <summary>
            /// Reads from the file of previously recorded authentication
            /// information and decrypts it.
            /// </summary>
            /// <param name="filepath">
            /// The full file path to read in from.
            /// </param>
            FileStream fs = new FileStream(filepath, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            if (!fs.CanRead)
            {
                return false;
            }
            else
            {
                string[] comp = Encryption.dec(sr.ReadLine()).Split(',');
                int x = 0; //Used to keep track of which 'cursor' we're reading in.
                foreach (string value in comp)
                {
                    if (value.CompareTo("\n")==0)
                    {
                        x++;
                    }
                    else if (value.CompareTo("") == 0)
                    {
                        break;
                    }
                    else
                    {
                        prev_code[x].Enqueue(Convert.ToInt16(value));
                    }
                }
                return true;
            }
        }

        public void removeTuioCursor(TuioCursor c)
        {
            lock (cursorSync)
            {
                cursorList.Remove(c.getSessionID());
            }
            if (verbose) Console.WriteLine("del cur " + c.getCursorID() + " (" + c.getSessionID() + ")");
        }

        public void refresh(TuioTime frameTime)
        {
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Getting the graphics object
            Graphics g = pevent.Graphics;
            g.FillRectangle(whiteBrush, new Rectangle(0, 0, width, height));

            // draw the cursor path
            if (cursorList.Count > 0)
            {
                lock (cursorSync)
                {
                    foreach (TuioCursor tcur in cursorList.Values)
                    {
                        List<TuioPoint> path = tcur.getPath();
                        TuioPoint current_point = path[0];

                        for (int i = 0; i < path.Count; i++)
                        {
                            TuioPoint next_point = path[i];
                            g.DrawLine(fingerPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
                            current_point = next_point;
                        }
                        g.FillEllipse(grayBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
                        Font font = new Font("Arial", 10.0f);
                        g.DrawString(tcur.getCursorID() + "", font, blackBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
                    }
                }
            }

            // draw the objects
            if (objectList.Count > 0)
            {
                lock (objectSync)
                {
                    foreach (TuioDemoObject tobject in objectList.Values)
                    {
                        tobject.paint(g);
                    }
                }
            }
        }

        public static void Main(String[] argv)
        {
            int port = 0;
            switch (argv.Length)
            {
                case 1:
                    port = int.Parse(argv[0], null);
                    if (port == 0) goto default;
                    break;
                case 0:
                    port = 3333;
                    break;
                default:
                    Console.WriteLine("usage: java TuioDemo [port]");
                    System.Environment.Exit(0);
                    break;
            }

            TuioDemo app = new TuioDemo(port);
            Application.Run(app);

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Name = "TuioDemo";
            this.ResumeLayout(false);
        }

        public void setButtonloc(int buttID, int x_loc, int y_loc)
        {
            switch (buttID)
            {
                case 1:
                    {
                        this.button1.Location = new System.Drawing.Point(x_loc, y_loc);
                        break;
                    }
                case 2:
                    {
                        this.button2.Location = new System.Drawing.Point(x_loc, y_loc);
                        break;
                    }
                case 3:
                    {
                        this.button3.Location = new System.Drawing.Point(x_loc, y_loc);
                        break;
                    }
            }
        }

        public void setTextBoxLoc(int x_loc, int y_loc)
        {
            this.textBox1.Location = new System.Drawing.Point(x_loc, y_loc);
        }

        int m = 0;
        int s = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.ForeColor = System.Drawing.Color.Aqua;
            Random r = new Random();
            int t = (int)(r.Next(0, 100));
            if(t<50)
            {
                textBox1.Text = "Registering Multi-Touch. Please draw at least two patterns.";
                m = 1;
            } else
            {
                textBox1.Text = "Registering Single-Touch. Please draw only one patterns.";
                s = 1;
            }
            ResetGrid();
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            #region Multi-Touch
            if (m == 1)
            {
                //Force multi-touch enrollment only
                bool oneTouch = false;
                bool isMulti = false;
                for (int x = 0; x < 10; x++)
                {
                    if (code[x].Count() > 0 && !oneTouch)
                    {
                        oneTouch = true;
                    }
                    else if (code[x].Count() > 0 && oneTouch)
                    {
                        isMulti = true;
                    }
                }
                if (isMulti)
                {
                    WriteOutCode(@"C:\temp\code.txt");
                    button2.Enabled = false;
                    button1.Enabled = true;
                    button3.Enabled = true;
                    textBox1.ForeColor = System.Drawing.Color.Green;
                    textBox1.Text = "Your pattern has been registered.";
                    ResetGrid();
                    InitializeQueueArray(code);
                    m = 0;
                    s = 0;
                }
                else
                {
                    textBox1.ForeColor = System.Drawing.Color.Red;
                    textBox1.Text = "Error. Must have more than one pattern registered.";
                    button2.Enabled = false;
                    button1.Enabled = true;
                    button3.Enabled = true;
                    m = 0;
                    s = 0;
                }
            }
            #endregion
            #region Single-Touch
            if (s == 1)
            {
                //Force single-touch enrollment only
                bool oneTouch = false;
                bool isSingle = true;
                for (int x = 0; x < 10; x++)
                {
                    if (code[x].Count() > 0 && !oneTouch)
                    {
                        oneTouch = true;
                    }
                    else if (code[x].Count() > 0 && oneTouch)
                    {
                        isSingle = false;
                    }
                }
                if (isSingle)
                {
                    WriteOutCode(@"C:\temp\code.txt");
                    button2.Enabled = false;
                    button1.Enabled = true;
                    button3.Enabled = true;
                    textBox1.ForeColor = System.Drawing.Color.Green;
                    textBox1.Text = "Your pattern has been registered.";
                    ResetGrid();
                    InitializeQueueArray(code);
                    m = 0;
                    s = 0;
                }
                else
                {
                    textBox1.ForeColor = System.Drawing.Color.Red;
                    textBox1.Text = "Error. Must not utilize more than one pattern.";
                    button2.Enabled = false;
                    button1.Enabled = true;
                    button3.Enabled = true;
                    m = 0;
                    s = 0;
                }
            }
            #endregion
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button1.Enabled == true) //If the Set button is enabled, we know we are not in the middle of authentication
            {
                ResetGrid();
                InitializeQueueArray(prev_code);
                bool AuthExist = ReadInCode(@"C:\temp\code.txt");
                if (!AuthExist)
                {
                    textBox1.ForeColor = System.Drawing.Color.Red;
                    textBox1.Text = "Error. No pattern set.";
                }
                else
                {
                    button1.Enabled = false;
                    textBox1.ForeColor = System.Drawing.Color.Aqua;
                    textBox1.Text = "Authenticating...";
                }
            }
            else //If the set button is disabled, the Authenticate button has already been pressed once, and now we need to check against our saved value.
            {
                bool valid = true;
                button1.Enabled = true;
                button2.Enabled = false;


                SortCode();
                for (int x = 0; x < code.GetLength(0); x++)
                {
                    if (code[x].Count != prev_code[x].Count)
                    {
                        valid = false;
                        textBox1.ForeColor = System.Drawing.Color.Red;
                        textBox1.Text = "Non-matching Pattern";
                    }
                    else
                    {
                        while (code[x].Count > 0)
                        {
                            if (code[x].Dequeue() != prev_code[x].Dequeue())
                            {
                                valid = false;
                                textBox1.ForeColor = System.Drawing.Color.Red;
                                textBox1.Text = "Non-matching Pattern";
                                break;
                            }
                        }
                    }
                    if (!valid) break;
                }
                if (valid)
                {
                    textBox1.ForeColor = System.Drawing.Color.Green;
                    textBox1.Text = "Authenticated!";
                }
                InitializeQueueArray(code);
            }
        }

        public void ResetGrid()
        {
            IEnumerable pbEnum = mainGrid.getPBArray() as IEnumerable;
            if (pbEnum != null)
            {
                foreach (MyPictureBox pb in pbEnum)
                {
                    pb.Image = global::Thesis_Security_App_1._1.Properties.Resources.Untouched;
                    pb.passes = 0;
                }
            }
            lastnode = 999;
        }
    }
}