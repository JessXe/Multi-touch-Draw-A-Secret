using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Thesis_Security_App_1._1
{
    class Grid
    {
        private int x;
        private int y;
        private static int x_mult = 100;
        private static int y_mult = 100;
        private static int x_off = -50;
        private static int y_off = -50;
        private static int img_width = 50;
        private static int img_height = 50;
        private int form_width;
        private int form_height;
        MyPictureBox[,] node;

        public Grid()
        {
            node = new MyPictureBox[3, 3];
        }
        public Grid(int x, int y, TuioDemo f)
        {    
            node = new MyPictureBox[x, y];
            drawGrid(f);
        }

        public bool drawGrid(TuioDemo f)
        {
            for (x = 0; x < node.GetLength(0); x++) //x is the column indicator
            {
                for (y = 0; y < node.GetLength(1); y++) //y is the row indicator
                {
                    node[x, y] = new MyPictureBox(); //So, node[2][0] is the first row, 3rd element
                    ((System.ComponentModel.ISupportInitialize)(node[x, y])).BeginInit();
                    node[x, y].Image = global::Thesis_Security_App_1._1.Properties.Resources.Untouched;
                    node[x, y].Location = new System.Drawing.Point(((x + 1) * x_mult) + x_off, ((y + 1) * y_mult) + y_off);
                    node[x, y].Name = "node:{" + x + "," + y + "}";
                    node[x, y].Size = new System.Drawing.Size(img_width, img_height);
                    node[x, y].TabIndex = (node.GetLength(1) * y) + x + 4; //The algorithm we are using is (RowNumber*MaxNumColumns)+currentColumn( + offset so that the buttons are first)
                    node[x, y].TabStop = false;
                    node[x, y].passes = 0;
                    node[x, y].nodenum = (node.GetLength(1) * y) + x;
                    f.Controls.Add(node[x, y]);
                    ((System.ComponentModel.ISupportInitialize)(node[x, y])).EndInit();
                }
            }
            System.Drawing.Point edge = node[node.GetLength(0)-1, node.GetLength(1)-1].Location;    //Getting bottom righthand node location
            form_height = edge.Y + 100;
            form_width = edge.X + 100;
            f.ClientSize = new System.Drawing.Size(form_width, form_height);                     //and using it to readjust the window size
            f.setButtonloc(1, 13, edge.Y + 75);
            f.setButtonloc(2, 105, edge.Y + 75);
            f.setButtonloc(3, 199, edge.Y + 75);
            f.setTextBoxLoc(289, edge.Y + 75);
            return true;
        }

        public void checkCursor(float cursorX, float cursorY, int cursorID, Queue<int>[] code)
        {
            float x_val = (cursorX -(float)x_off) / (float)x_mult;
            float y_val = (cursorY -(float)y_off) / (float)y_mult;
            float wid_max = (float)img_width / (float)x_mult;
            float hth_max = (float)img_height / (float)y_mult;
            int x_id = (int)Math.Floor((double)x_val);
            int y_id = (int)Math.Floor((double)y_val);
            float x_dec = x_val - x_id;
            float y_dec = y_val - y_id;
            MyPictureBox cur_node;
            if (x_dec <= wid_max)
                if (y_dec <= hth_max)
                {
                    cur_node = node[x_id - 1, y_id -1];
                    cur_node.passes++;
                        if (cur_node.passes < 2)
                        {
                            code[cursorID].Enqueue(cur_node.nodenum);
                            updateImage(cur_node);
//                            lastnode = cur_node.nodenum;
                        }
                }
        }

        public void updateImage(MyPictureBox pb)
        {
            switch (pb.passes)
            {
                case 0:
                    pb.Image = global::Thesis_Security_App_1._1.Properties.Resources.Untouched;
                    break;
                case 1:
                    pb.Image = global::Thesis_Security_App_1._1.Properties.Resources.Touched1;
                    break;
                case 2:
                    pb.Image = global::Thesis_Security_App_1._1.Properties.Resources.Touched2;
                    break;
                case 3:
                    pb.Image = global::Thesis_Security_App_1._1.Properties.Resources.Touched3;
                    break;
                default:
                    pb.Image = global::Thesis_Security_App_1._1.Properties.Resources.Bad;
                    break;
            }
        }

        public int getWidth()
        {
            return form_width;
        }

        public int getHeight()
        {
            return form_height;
        }

        public MyPictureBox[,] getPBArray()
        {
            return this.node;
        }
    }
}