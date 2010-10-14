using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thesis_Security_App_1._1
{
    class Grid //Test
    {
        private int x;
        private int y;
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
            drawGrid(-50, -50, f);
        }

        public bool drawGrid(int x_off, int y_off, TuioDemo f)
        {
            for (x = 0; x < node.GetLength(0); x++) //x is the column indicator
            {
                for (y = 0; y < node.GetLength(1); y++) //y is the row indicator
                {
                    node[x, y] = new MyPictureBox(); //So, node[2][0] is the first row, 3rd element
                    ((System.ComponentModel.ISupportInitialize)(node[x, y])).BeginInit();
                    node[x, y].Image = global::Thesis_Security_App_1._1.Properties.Resources.Untouched;
                    node[x, y].Location = new System.Drawing.Point(((x + 1) * 100) + x_off, ((y + 1) * 100) + y_off);
                    node[x, y].Name = "node:{" + x + "," + y + "}";
                    node[x, y].Size = new System.Drawing.Size(50, 50);
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