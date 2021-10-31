using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsAppErgasia2Alpha
{
    class Star
    {

        public static Point[] starPoints(Point center,int size)
        {
            Point a1, a2, a3, a4, a5, a6 ,a7,a8,a9,a10;
            a1 = new Point(center.X, center.Y - size);

            a2 = new Point((int)((double)center.X + ((double)size) * Math.Sin(2 * Math.PI / 5)), (int)((double)center.Y - ((double)size) * Math.Cos(2 * Math.PI / 5)));
            a5 = new Point((int)((double)center.X - ((double)size) * Math.Sin(2 * Math.PI / 5)), (int)((double)center.Y - ((double)size) * Math.Cos(2 * Math.PI / 5)));
            a3 = new Point((int)((double)center.X + ((double)size) * Math.Sin(2 * Math.PI / 10)), (int)((double)center.Y + ((double)size) * Math.Cos(2 * Math.PI / 10)));
            a4 = new Point((int)((double)center.X - ((double)size) * Math.Sin(2 * Math.PI / 10)), (int)((double)center.Y + ((double)size) * Math.Cos(2 * Math.PI / 10)));

            
            float size22 = center.Y - a2.Y;
            int size2 = (int)(size22 / Math.Cos(2 * Math.PI / 10));
            a6 = new Point((int)((double)center.X + ((double)size2) * Math.Sin(2 * Math.PI / 10)), (int)((double)center.Y - ((double)size2) * Math.Cos(2 * Math.PI / 10)));
            a10 = new Point((int)((double)center.X - ((double)size2) * Math.Sin(2 * Math.PI / 10)), (int)((double)center.Y - ((double)size2) * Math.Cos(2 * Math.PI / 10)));

            a7 = new Point((int)((double)center.X + ((double)size2) * Math.Sin(2 * Math.PI / 5)), (int)((double)center.Y + ((double)size2) * Math.Cos(2 * Math.PI / 5)));
            a8 = new Point(center.X, center.Y + size2);
            a9 = new Point((int)((double)center.X - ((double)size2) * Math.Sin(2 * Math.PI / 5)), (int)((double)center.Y + ((double)size2) * Math.Cos(2 * Math.PI / 5)));

            //Point[] centers = { a1, a3, a5, a2, a4, a1 };
            //Point[] centers = { a1, a2, a3, a4, a5, a1 };
            Point[] points = { a5, a10, a1, a6, a2, a7, a3, a8, a4, a9, a5};
            //Point[] centers = {a10,a6,a7,a8,a9,a10 };
            return points;
        }

        public static Point[] housePoints(Point corner,int height,int width)
        {
            Point a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17;

            //outer shape of the house:
            a1 = new Point(corner.X, corner.Y);
            a2 = new Point(corner.X, corner.Y - (2 * height) / 3);
            a3 = new Point(a2.X + width / 2, a2.Y - height / 3);
            a4 = new Point(a2.X + width, a2.Y);
            a5 = new Point(a1.X + width, a1.Y);
            //shape of the door:
            a6 = new Point(a5.X - (45 * width) / 100, a5.Y);
            a7 = new Point(a6.X, a5.Y - (10 * height)/100);
            a8 = new Point(a6.X - (10 * width)/100, a7.Y);
            a9 = new Point(a8.X, a8.Y + (10 * height) / 100);
            //points of windows:
            //right window:
            a10 = new Point(a1.X + width/2 + (20*width)/100,a1.Y - (((2 * height) / 3) *70)/100);
            a11 = new Point(a10.X, a1.Y - (((2 * height) / 3) * 80) / 100);
            a12 = new Point(a11.X+ (10 * width) / 100, a11.Y );
            a13 = new Point(a12.X, a10.Y);
            //left window:
            a14 = new Point(a1.X + (20 * width) / 100, a1.Y - (((2 * height) / 3) * 70) / 100);
            a15 = new Point(a14.X, a1.Y - (((2 * height) / 3) * 80) / 100);
            a16 = new Point(a15.X + (10 * width) / 100, a15.Y);
            a17 = new Point(a16.X, a14.Y);

            Point[] points = { a1, a2, a3, a4, a5, a1, a9, a8, a7, a6, a10, a11, a12, a13, a14, a15, a16, a17 };

            return points;
        }
    }
}
