using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Cubing
{
    internal class Cube3D
    {
        static int xxx;
        const int FACELETWIDTH = 100;
        const int ROUNDRADIUS = 10;
        const int COLORWIDTH = 100 - ROUNDRADIUS*2;
        const int U = 0, R = 1, F = 2, D = 3, L = 4, B = 5;
        Point3D p0, p1, p2, p3, p4, p5, p6, p7;
        Point3D t0, t1, t2, t3, t4, t5, t6, t7, ta, tb, tc, td;
        Vector3D[] faceNormals = new Vector3D[] {
            new Vector3D(0, 1, 0) ,
            new Vector3D(1, 0, 0) ,
            new Vector3D(0, 0, 1),
            new Vector3D(0, -1, 0) ,
            new Vector3D(-1, 0, 0) ,
            new Vector3D(0, 0, -1)};
        Int32Rect entireBitmapRect;
        Int32Rect partBitmapRect;

        public delegate void AnimationCompletedEventHandler(Rotation twist);
        public event AnimationCompletedEventHandler? AnimationCompleted;

        public Cube3D(Cube cube)
        {
            BaseCube = cube;
            Dimension = cube.Dimension;
            FaceBitmaps = new RenderTargetBitmap[6];
            for (int i = 0; i < 6; i++)
            {
                FaceBitmaps[i] = new RenderTargetBitmap(Dimension * FACELETWIDTH, Dimension * FACELETWIDTH, 96.0, 96.0, PixelFormats.Default);
            }
            entireBitmapRect = new Int32Rect(0, 0, Dimension * FACELETWIDTH, Dimension * FACELETWIDTH);
            CubeString = BaseCube.CubeString;
            p0 = new Point3D(0, Dimension, 0);
            p1 = new Point3D(Dimension, Dimension, 0);
            p2 = new Point3D(Dimension, Dimension, Dimension);
            p3 = new Point3D(0, Dimension, Dimension);
            p4 = new Point3D(0, 0, 0);
            p5 = new Point3D(Dimension, 0, 0);
            p6 = new Point3D(Dimension, 0, Dimension);
            p7 = new Point3D(0, 0, Dimension);

            InnerFaceBitmap = new RenderTargetBitmap(Dimension * FACELETWIDTH, Dimension * FACELETWIDTH, 96.0, 96.0, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            Brush innerbackgroundbrush = Brushes.Black;
            Pen innerbackgroundpen = new Pen(innerbackgroundbrush, 1.0);
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(innerbackgroundbrush, innerbackgroundpen, new Rect(0, 0, Dimension * FACELETWIDTH, Dimension * FACELETWIDTH));
            }
            InnerFaceBitmap.Render(dv);

            WholeCube = new GeometryModel3D[0];
            RotatablePart=new GeometryModel3D[0];
            FixedPart=new GeometryModel3D[0];   
        }

        private Cube BaseCube { get; set; }

        public int Dimension { get; }

        //Bitmap顺序为URFDLB
        public RenderTargetBitmap[] FaceBitmaps { get; set; }
        public RenderTargetBitmap InnerFaceBitmap { get; set; }

        private string CubeString { get; set; }

        //根据cubestring更新6面的bitmap
        private void Refresh()
        {
            CubeString = BaseCube.CubeString;

            Brush backgroundbrush = Brushes.White;
            Brush ubrush = Brushes.Yellow;
            Brush rbrush = Brushes.Red;
            Brush fbrush = Brushes.Blue;
            Brush dbrush = Brushes.WhiteSmoke;
            Brush lbrush = Brushes.Orange;
            Brush bbrush = Brushes.Green;
            Pen backgroundpen = new Pen(backgroundbrush, 1.0);
            Pen upen = new Pen(ubrush, 1.0);
            Pen rpen = new Pen(rbrush, 1.0);
            Pen fpen = new Pen(fbrush, 1.0);
            Pen dpen = new Pen(dbrush, 1.0);
            Pen lpen = new Pen(lbrush, 1.0);
            Pen bpen = new Pen(bbrush, 1.0);

            Brush currentbrush = backgroundbrush;
            Pen currentpen = backgroundpen;
            for (int i = 0; i < 6; i++)
            {
                string facestring = CubeString.Substring(i * Dimension * Dimension, Dimension * Dimension);
                int index = 0;
                char c;
                DrawingVisual dv = new DrawingVisual();
                Rect faceletRect = Rect.Empty;
                using (DrawingContext dc = dv.RenderOpen())
                {
                    dc.DrawRectangle(backgroundbrush, backgroundpen, new Rect(0, 0, Dimension * FACELETWIDTH, Dimension * FACELETWIDTH));
                    for (int y = 0; y < Dimension; y++)
                    {
                        for (int x = 0; x < Dimension; x++)
                        {
                            index = y * Dimension + x;
                            c = facestring[index];
                            switch (c)
                            {
                                case 'U':
                                    currentbrush = ubrush;
                                    currentpen = upen;
                                    break;
                                case 'R':
                                    currentbrush = rbrush;
                                    currentpen = rpen;
                                    break;
                                case 'F':
                                    currentbrush = fbrush;
                                    currentpen = fpen;
                                    break;
                                case 'D':
                                    currentbrush = dbrush;
                                    currentpen = dpen;
                                    break;
                                case 'L':
                                    currentbrush = lbrush;
                                    currentpen = lpen;
                                    break;
                                case 'B':
                                    currentbrush = bbrush;
                                    currentpen = bpen;
                                    break;
                            }
                            faceletRect = new Rect(x * FACELETWIDTH + ROUNDRADIUS, y * FACELETWIDTH + ROUNDRADIUS, COLORWIDTH, COLORWIDTH);
                            dc.DrawRoundedRectangle(currentbrush, currentpen, faceletRect, ROUNDRADIUS, ROUNDRADIUS);
                        }
                    }
                }
                FaceBitmaps[i].Render(dv);
            }
        }

        //每一个魔方的扭动，都可以把魔方分为一个可动部分，0至2个不动部分。
        public void Seperate(Seperation s)
        {
            Refresh();
            WholeCube = new GeometryModel3D[6];
            WholeCube[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, p2, p3 }, U, entireBitmapRect, true);
            WholeCube[R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, p5, p6 }, R, entireBitmapRect, true);
            WholeCube[F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, p6, p7 }, F, entireBitmapRect, true);
            WholeCube[D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, p5, p4 }, D, entireBitmapRect, true);
            WholeCube[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, p7, p4 }, L, entireBitmapRect, true);
            WholeCube[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, p4, p5 }, B, entireBitmapRect, true);
            //1.根据seperation确定分为几部分，重新定义WholeCube、FixedPart、RotatablePart数组
            //2.需要将块的start，end索引转换为坐标
            int x = 0, y = 0, width = 0, height = 0;
            switch (s.Direction)
            {
                case SeperationDircetion.None:
                    t0 = p0;
                    t1 = p1;
                    t2 = p2;
                    t3 = p3;
                    t4 = p4;
                    t5 = p5;
                    t6 = p6;
                    t7 = p7;
                    break;
                case SeperationDircetion.X:
                    if (s.StartX == 0)
                    {
                        if (s.EndX == Dimension - 1)
                        {
                            RotatablePart = WholeCube;
                            FixedPart = new GeometryModel3D[0];
                        }
                        else
                        {//L系列
                            ta = new Point3D(s.EndX + 1, Dimension, 0);
                            tb = new Point3D(s.EndX + 1, 0, 0);
                            tc = new Point3D(s.EndX + 1, 0, Dimension);
                            td = new Point3D(s.EndX + 1, Dimension, Dimension);
                            //可动部分
                            RotatablePart = new GeometryModel3D[6];
                            x = 0;
                            y = 0;
                            width = (s.EndX + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { p0, ta, td, p3 }, U, partBitmapRect, true);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { td, ta, tb, tc }, R, entireBitmapRect, false);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { p3, td, tc, p7 }, F, partBitmapRect, true);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { p7, tc, tb, p4 }, D, partBitmapRect, true);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, p7, p4 }, L, entireBitmapRect, true);
                            x = Dimension*FACELETWIDTH-(s.EndX+1)*FACELETWIDTH;
                            y = 0;
                            width = (s.EndX + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { ta, p0, p4, tb }, B, partBitmapRect, true);
                            //不动部分
                            FixedPart = new GeometryModel3D[6];
                            x = (s.EndX+1)*FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - x;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { ta, p1, p2, td }, U, partBitmapRect, true);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, p5, p6 }, R, entireBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { td, p2, p6, tc }, F, partBitmapRect, true);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { tc, p6, p5, tb }, D, partBitmapRect, true);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { ta, td, tc, tb }, L, entireBitmapRect, false);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH -(s.EndX+1)*FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { p1, ta, tb, p5 }, B, partBitmapRect, true);
                        }
                    }
                    else
                    {
                        if (s.EndX == Dimension - 1)
                        {//R系列
                            ta = new Point3D(s.StartX, Dimension, 0);
                            tb = new Point3D(s.StartX, 0, 0);
                            tc = new Point3D(s.StartX, 0, Dimension);
                            td = new Point3D(s.StartX, Dimension, Dimension);
                            //可动部分
                            RotatablePart = new GeometryModel3D[6];
                            x = s.StartX * FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - s.StartX * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { ta, p1, p2, td }, U, partBitmapRect, true);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, p5, p6 }, R, entireBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { td, p2, p6, tc }, F, partBitmapRect, true);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { tc, p6, p5, tb }, D, partBitmapRect, true);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { ta, td, tc, tb }, L, entireBitmapRect, false);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH - s.StartX * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { p1, ta, tb, p5 }, B, partBitmapRect, true);
                            //不动部分
                            FixedPart = new GeometryModel3D[6];
                            x = 0;
                            y = 0;
                            width = s.StartX*FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { p0, ta, td, p3 }, U, partBitmapRect, true);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { td, ta, tb, tc }, R, entireBitmapRect, false);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { p3, td, tc, p7 }, F, partBitmapRect, true);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { p7, tc, tb, p4 }, D, partBitmapRect, true);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, p7, p4 }, L, entireBitmapRect, true);
                            x = Dimension * FACELETWIDTH - s.StartX * FACELETWIDTH;
                            y = 0;
                            width = s.StartX * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { ta, p0, p4, tb }, B, partBitmapRect, true);
                        }
                        else
                        {//中间层
                            t0 = new Point3D(s.StartX, Dimension, 0);
                            t1 = new Point3D(s.EndX + 1, Dimension, 0);
                            t2 = new Point3D(s.EndX + 1, Dimension, Dimension);
                            t3 = new Point3D(s.StartX, Dimension, Dimension);
                            t4 = new Point3D(s.StartX, 0, 0);
                            t5 = new Point3D(s.EndX + 1, 0, 0);
                            t6 = new Point3D(s.EndX + 1, 0, Dimension);
                            t7 = new Point3D(s.StartX, 0, Dimension);
                            //可动部分
                            RotatablePart = new GeometryModel3D[6];
                            x = s.StartX * FACELETWIDTH;
                            y = 0;
                            width = (s.EndX - s.StartX + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { t0, t1, t2, t3 }, U, partBitmapRect, true);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { t2, t1, t5, t6 }, R, entireBitmapRect, false);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { t3, t2, t6, t7 }, F, partBitmapRect, true);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { t7, t6, t5, t4 }, D, partBitmapRect, true);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { t0, t3, t7, t4 }, L, entireBitmapRect, false);
                            x =Dimension*FACELETWIDTH - (s.EndX+1) * FACELETWIDTH;
                            y = 0;
                            width = (s.EndX - s.StartX + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { t1, t0, t4, t5 }, B, partBitmapRect, true);
                            //固定部分左
                            FixedPart = new GeometryModel3D[12];
                            x = 0;
                            y =0 ;
                            width = s.StartX * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { p0, t0, t3, p3 }, U, partBitmapRect, true);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { t3, t0, t4, t7 }, R, entireBitmapRect, false);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { p3, t3, t7, p7 }, F, partBitmapRect, true);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { p7, t7, t4, p4 }, D, partBitmapRect, true);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, p7, p4 }, L, entireBitmapRect, true);
                            x = Dimension*FACELETWIDTH-s.StartX*FACELETWIDTH;
                            y = 0;
                            width = s.StartX * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { t0, p0, p4, t4 }, B, partBitmapRect, true);
                            //固定部分右
                            x = (s.EndX + 1) * FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - x;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + U] = getCubeSideGeometryModel(new Point3D[] { t1, p1, p2, t2 }, U, partBitmapRect, true);
                            FixedPart[6 + R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, p5, p6 }, R, entireBitmapRect, true);
                            FixedPart[6 + F] = getCubeSideGeometryModel(new Point3D[] { t2, p2, p6, t6 }, F, partBitmapRect, true);
                            FixedPart[6 + D] = getCubeSideGeometryModel(new Point3D[] { t6, p6, p5, t5 }, D, partBitmapRect, true);
                            FixedPart[6 + L] = getCubeSideGeometryModel(new Point3D[] { t1, t2, t6, t5 }, L, entireBitmapRect, false);
                            x =0;
                            y = 0;
                            width = Dimension*FACELETWIDTH-(s.EndX+1)*FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + B] = getCubeSideGeometryModel(new Point3D[] { p1, t1, t5, p5 }, B, partBitmapRect, true);
                        }
                    }
                    break;
                case SeperationDircetion.Y:
                    if (s.StartY == 0)
                    {
                        if (s.EndY == Dimension - 1)
                        {
                            RotatablePart = WholeCube;
                            FixedPart = new GeometryModel3D[0];
                        }
                        else
                        {//U系列
                            ta = new Point3D(0, Dimension - (s.EndY + 1), 0);
                            tb = new Point3D(Dimension, Dimension - (s.EndY + 1), 0);
                            tc = new Point3D(Dimension, Dimension - (s.EndY + 1), Dimension);
                            td = new Point3D(0, Dimension - (s.EndY + 1), Dimension);
                            //旋转部分
                            RotatablePart = new GeometryModel3D[6];
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, p2, p3 }, U, entireBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = (s.EndY + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);//以下相同
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, tb, tc }, R, partBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, tc, td }, F, partBitmapRect, true);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { td, tc, tb, ta }, D, entireBitmapRect, false);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, td, ta }, L, partBitmapRect, true);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, ta, tb }, B, partBitmapRect, true);
                            //不动部分
                            FixedPart = new GeometryModel3D[6];
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { ta, tb, tc, td }, U, entireBitmapRect, false);
                            x = 0;
                            y = (s.EndY + 1) * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - y;
                            partBitmapRect = new Int32Rect(x, y, width, height);//以下相同
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { tc, tb, p5, p6 }, R, partBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { td, tc, p6, p7 }, F, partBitmapRect, true);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, p5, p4 }, D, entireBitmapRect, true);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { ta, td, p7, p4 }, L, partBitmapRect, true);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { tb, ta, p4, p5 }, B, partBitmapRect, true);

                        }
                    }
                    else
                    {
                        if (s.EndY == Dimension - 1)
                        {//D系列
                            ta = new Point3D(0, Dimension - s.StartY, 0);
                            tb = new Point3D(Dimension, Dimension - s.StartY, 0);
                            tc = new Point3D(Dimension, Dimension - s.StartY, Dimension);
                            td = new Point3D(0, Dimension - s.StartY, Dimension);
                            //旋转部分
                            RotatablePart = new GeometryModel3D[6];
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { ta, tb, tc, td }, U, entireBitmapRect, false);
                            x = 0;
                            y = s.StartY * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - y;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { tc, tb, p5, p6 }, R, partBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { td, tc, p6, p7 }, F, partBitmapRect, true);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, p5, p4 }, D, entireBitmapRect, true);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { ta, td, p7, p4 }, L, partBitmapRect, true);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { tb, ta, p4, p5 }, B, partBitmapRect, true);
                            //不动部分
                            FixedPart = new GeometryModel3D[6];
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = s.StartY * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, p2, p3 }, U, entireBitmapRect, true);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, tb, tc }, R, partBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, tc, td }, F, partBitmapRect, true);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { td, tc, tb, ta }, D, entireBitmapRect, false);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, td, ta }, L, partBitmapRect, true);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, ta, tb }, B, partBitmapRect, true);

                        }
                        else
                        {//中间层
                            t0 = new Point3D(0, Dimension - s.StartY, 0);
                            t1 = new Point3D(Dimension, Dimension - s.StartY, 0);
                            t2 = new Point3D(Dimension, Dimension - s.StartY, Dimension);
                            t3 = new Point3D(0, Dimension - s.StartY, Dimension);
                            t4 = new Point3D(0, Dimension - (s.EndY + 1), 0);
                            t5 = new Point3D(Dimension, Dimension - (s.EndY + 1), 0);
                            t6 = new Point3D(Dimension, Dimension - (s.EndY + 1), Dimension);
                            t7 = new Point3D(0, Dimension - (s.EndY + 1), Dimension);
                            //旋转部分
                            RotatablePart = new GeometryModel3D[6];
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { t0, t1, t2, t3 }, U, entireBitmapRect, false);
                            x = 0;
                            y = s.StartY * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = (s.EndY - s.StartY + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { t2, t1, t5, t6 }, R, partBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { t3, t2, t6, t7 }, F, partBitmapRect, true);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { t7, t6, t5, t4 }, D, entireBitmapRect, false);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { t0, t3, t7, t4 }, L, partBitmapRect, true);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { t1, t0, t4, t5 }, B, partBitmapRect, true);
                            //不动部分
                            FixedPart = new GeometryModel3D[12];
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = s.StartY * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, p2, p3 }, U, entireBitmapRect, true);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { p2, p1, t1, t2 }, R, partBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, t2, t3 }, F, partBitmapRect, true);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { t3, t2, t1, t0 }, D, entireBitmapRect, false);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { p0, p3, t3, t0 }, L, partBitmapRect, true);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, t0, t1 }, B, partBitmapRect, true);
                            x = 0;
                            y = (s.EndY + 1) * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - y;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + U] = getCubeSideGeometryModel(new Point3D[] { t4, t5, t6, t7 }, U, entireBitmapRect, false);
                            FixedPart[6 + R] = getCubeSideGeometryModel(new Point3D[] { t6, t5, p5, p6 }, R, partBitmapRect, true);
                            FixedPart[6 + F] = getCubeSideGeometryModel(new Point3D[] { t7, t6, p6, p7 }, F, partBitmapRect, true);
                            FixedPart[6 + D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, p5, p4 }, D, entireBitmapRect, true);
                            FixedPart[6 + L] = getCubeSideGeometryModel(new Point3D[] { t4, t7, p7, p4 }, L, partBitmapRect, true);
                            FixedPart[6 + B] = getCubeSideGeometryModel(new Point3D[] { t5, t4, p4, p5 }, B, partBitmapRect, true);

                        }
                    }
                    break;
                case SeperationDircetion.Z:
                    if (s.StartZ == 0)
                    {//B系列
                        if (s.EndZ == Dimension - 1)
                        {
                            RotatablePart = WholeCube;
                            FixedPart = new GeometryModel3D[0];
                        }
                        else
                        {
                            ta = new Point3D(0, Dimension, s.EndZ + 1);
                            tb = new Point3D(Dimension, Dimension, s.EndZ + 1);
                            tc = new Point3D(Dimension, 0, s.EndZ + 1);
                            td = new Point3D(0, 0, s.EndZ + 1);
                            //旋转部分
                            RotatablePart = new GeometryModel3D[6];
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = (s.EndZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, tb, ta }, U, partBitmapRect, true);
                            x = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            y = 0;
                            width = (s.EndZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { tb, p1, p5, tc }, R, partBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { ta, tb, tc, td }, F, entireBitmapRect, false);
                            x = 0;
                            y = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = (s.EndZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { td, tc, p5, p4 }, D, partBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = (s.EndZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { p0, ta, td, p4 }, L, partBitmapRect, true);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, p4, p5 }, B, entireBitmapRect, true);
                            //不动部分
                            FixedPart = new GeometryModel3D[6];
                            x = 0;
                            y = (s.EndZ + 1) * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - y;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { ta, tb, p2, p3 }, U, partBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { p2, tb, tc, p6 }, R, partBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, p6, p7 }, F, entireBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, tc, td }, D, partBitmapRect, true);
                            x = (s.EndZ + 1) * FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - x;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { ta, p3, p7, td }, L, partBitmapRect, true);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { tb, ta, td, tc }, B, entireBitmapRect, false);
                        }
                    }
                    else
                    {//F系列
                        if (s.EndZ == Dimension - 1)
                        {
                            ta = new Point3D(0, Dimension, s.StartZ);
                            tb = new Point3D(Dimension, Dimension, s.StartZ);
                            tc = new Point3D(Dimension, 0, s.StartZ);
                            td = new Point3D(0, 0, s.StartZ);
                            //
                            RotatablePart = new GeometryModel3D[6];
                            x = 0;
                            y = s.StartZ * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - y;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { ta, tb, p2, p3 }, U, partBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH - s.StartZ * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { p2, tb, tc, p6 }, R, partBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, p6, p7 }, F, entireBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - s.StartZ * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, tc, td }, D, partBitmapRect, true);
                            x = s.StartZ * FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - x;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { ta, p3, p7, td }, L, partBitmapRect, true);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { tb, ta, td, tc }, B, entireBitmapRect, false);
                            //
                            FixedPart = new GeometryModel3D[6];
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = s.StartZ * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, tb, ta }, U, partBitmapRect, true);
                            x = Dimension * FACELETWIDTH - s.StartZ * FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - x;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { tb, p1, p5, tc }, R, partBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { ta, tb, tc, td }, F, entireBitmapRect, false);
                            x = 0;
                            y = Dimension * FACELETWIDTH - s.StartZ * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - y;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { td, tc, p5, p4 }, D, partBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = s.StartZ * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { p0, ta, td, p4 }, L, partBitmapRect, true);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, p4, p5 }, B, entireBitmapRect, true);
                        }
                        else
                        {//中间层
                            t0 = new Point3D(0, Dimension, s.StartZ);
                            t1 = new Point3D(Dimension, Dimension, s.StartZ);
                            t2 = new Point3D(Dimension, Dimension, s.EndZ + 1);
                            t3 = new Point3D(0, Dimension, s.EndZ + 1);
                            t4 = new Point3D(0, 0, s.StartZ);
                            t5 = new Point3D(Dimension, 0, s.StartZ);
                            t6 = new Point3D(Dimension, 0, s.EndZ + 1);
                            t7 = new Point3D(0, 0, s.EndZ + 1);
                            //可动部分
                            RotatablePart = new GeometryModel3D[6];
                            x = 0;
                            y = s.StartZ * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = (s.EndZ - s.StartZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[U] = getCubeSideGeometryModel(new Point3D[] { t0, t1, t2, t3 }, U, partBitmapRect, true);
                            x = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            y = 0;
                            width = (s.EndZ - s.StartZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[R] = getCubeSideGeometryModel(new Point3D[] { t2, t1, t5, t6 }, R, partBitmapRect, true);
                            RotatablePart[F] = getCubeSideGeometryModel(new Point3D[] { t3, t2, t6, t7 }, F, entireBitmapRect, false);
                            x = 0;
                            y = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = (s.EndZ - s.StartZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[D] = getCubeSideGeometryModel(new Point3D[] { t7, t6, t5, t4 }, D, partBitmapRect, true);
                            x = s.StartZ * FACELETWIDTH;
                            y = 0;
                            width = (s.EndZ - s.StartZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            RotatablePart[L] = getCubeSideGeometryModel(new Point3D[] { t0, t3, t7, t4 }, L, partBitmapRect, true);
                            RotatablePart[B] = getCubeSideGeometryModel(new Point3D[] { t1, t0, t4, t5 }, B, entireBitmapRect, false);
                            //固定部分后
                            FixedPart = new GeometryModel3D[12];
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = s.StartZ * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[U] = getCubeSideGeometryModel(new Point3D[] { p0, p1, t1, t0 }, U, partBitmapRect, true);
                            x = Dimension * FACELETWIDTH - s.StartZ * FACELETWIDTH;
                            y = 0;
                            width = s.StartZ * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[R] = getCubeSideGeometryModel(new Point3D[] { t1, p1, p5, t5 }, R, partBitmapRect, true);
                            FixedPart[F] = getCubeSideGeometryModel(new Point3D[] { t0, t1, t5, t4 }, F, entireBitmapRect, false);
                            x = 0;
                            y = Dimension * FACELETWIDTH - s.StartZ * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = s.StartZ * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[D] = getCubeSideGeometryModel(new Point3D[] { t4, t5, p5, p4 }, D, partBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = s.StartZ * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[L] = getCubeSideGeometryModel(new Point3D[] { p0, t0, t4, p4 }, L, partBitmapRect, true);
                            FixedPart[B] = getCubeSideGeometryModel(new Point3D[] { p1, p0, p4, p5 }, B, entireBitmapRect, true);
                            //固定部分前
                            x = 0;
                            y = (s.EndZ + 1) * FACELETWIDTH;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + U] = getCubeSideGeometryModel(new Point3D[] { t3, t2, p2, p3 }, U, partBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + R] = getCubeSideGeometryModel(new Point3D[] { p2, t2, t6, p6 }, R, partBitmapRect, true);
                            FixedPart[6 + F] = getCubeSideGeometryModel(new Point3D[] { p3, p2, p6, p7 }, F, entireBitmapRect, true);
                            x = 0;
                            y = 0;
                            width = Dimension * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + D] = getCubeSideGeometryModel(new Point3D[] { p7, p6, t6, t7 }, D, partBitmapRect, true);
                            x = (s.EndZ + 1) * FACELETWIDTH;
                            y = 0;
                            width = Dimension * FACELETWIDTH - (s.EndZ + 1) * FACELETWIDTH;
                            height = Dimension * FACELETWIDTH;
                            partBitmapRect = new Int32Rect(x, y, width, height);
                            FixedPart[6 + L] = getCubeSideGeometryModel(new Point3D[] { t3, p3, p7, t7 }, L, partBitmapRect, true);
                            FixedPart[6 + B] = getCubeSideGeometryModel(new Point3D[] { t2, t3, t7, t6 }, B, entireBitmapRect, false);
                        }
                    }
                    break;
            }
        }
        public GeometryModel3D[] WholeCube { get; private set; }
        public GeometryModel3D[] FixedPart { get; private set; }
        public GeometryModel3D[] RotatablePart { get; private set; }

        //点在数组中的顺序是从左上角开始顺时针
        private GeometryModel3D getCubeSideGeometryModel(Point3D[] points, int faceIndex, Int32Rect imgRect, bool withtexture)
        {
            GeometryModel3D gm = new GeometryModel3D();

            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions.Add(points[0]);
            mg.Positions.Add(points[3]);
            mg.Positions.Add(points[2]);
            mg.Positions.Add(points[0]);
            mg.Positions.Add(points[2]);
            mg.Positions.Add(points[1]);
            mg.Normals.Add(faceNormals[faceIndex]);
            mg.Normals.Add(faceNormals[faceIndex]);
            mg.Normals.Add(faceNormals[faceIndex]);
            mg.Normals.Add(faceNormals[faceIndex]);
            mg.Normals.Add(faceNormals[faceIndex]);
            mg.Normals.Add(faceNormals[faceIndex]);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(4);
            mg.TriangleIndices.Add(5);

            ImageBrush sideImageBrush;
            if (withtexture)
            {
                CroppedBitmap sideBitmap;
                sideBitmap = new CroppedBitmap(FaceBitmaps[faceIndex], imgRect);
                sideImageBrush = new ImageBrush(sideBitmap);

                //FileStream stream = new FileStream($"{xxx}.bmp", FileMode.Create);
                //BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(sideBitmap));
                //encoder.Save(stream);
                //xxx++;
            }
            else
            {
                sideImageBrush = new ImageBrush(InnerFaceBitmap);
            }
            //必须设置图像刷的ViewportUnits，否则wpf会自动调整纹理坐标，从而得不到想要的贴图效果
            sideImageBrush.ViewportUnits = BrushMappingMode.Absolute;
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));
            DiffuseMaterial dm = new DiffuseMaterial(sideImageBrush);
            //dm = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            gm.Material = dm;

            gm.Geometry = mg;

            return gm;
        }

        public GeometryModel3D[] getWholeCube()
        {
            Refresh();
            WholeCube = new GeometryModel3D[6];
            WholeCube[0] = getCubeSideGeometryModel(new Point3D[] { p0, p1, p2, p3 }, 0, entireBitmapRect, true);
            WholeCube[1] = getCubeSideGeometryModel(new Point3D[] { p2, p1, p5, p6 }, 1, entireBitmapRect, true);
            WholeCube[2] = getCubeSideGeometryModel(new Point3D[] { p3, p2, p6, p7 }, 2, entireBitmapRect, true);
            WholeCube[3] = getCubeSideGeometryModel(new Point3D[] { p7, p6, p5, p4 }, 3, entireBitmapRect, true);
            WholeCube[4] = getCubeSideGeometryModel(new Point3D[] { p0, p3, p7, p4 }, 4, entireBitmapRect, true);
            WholeCube[5] = getCubeSideGeometryModel(new Point3D[] { p1, p0, p4, p5 }, 5, entireBitmapRect, true);

            return WholeCube;
        }

        public void RotateWithAnimation(Rotation r, int speed, int stoptime)
        {
            Vector3D axis = new Vector3D();
            double angle = 90 * r.Postfix;
            double i = r.Postfix;

            switch (r.Body)
            {
                case RotationType.Uw:
                case RotationType.U:
                case RotationType.u:
                case RotationType.y:
                    axis = new Vector3D(0, -1, 0);
                    break;
                case RotationType.d:
                case RotationType.Dw:
                case RotationType.D:
                case RotationType.e:
                case RotationType.E:
                    axis = new Vector3D(0, 1, 0);
                    break;
                case RotationType.R:
                case RotationType.Rw:
                case RotationType.r:
                case RotationType.x:
                    axis = new Vector3D(-1, 0, 0);
                    break;
                case RotationType.L:
                case RotationType.Lw:
                case RotationType.l:
                case RotationType.m:
                case RotationType.M:
                    axis = new Vector3D(1, 0, 0);
                    break;
                case RotationType.F:
                case RotationType.Fw:
                case RotationType.f:
                case RotationType.z:
                case RotationType.s:
                case RotationType.S:
                    axis = new Vector3D(0, 0, -1);
                    break;
                case RotationType.B:
                case RotationType.Bw:
                case RotationType.b:
                    axis = new Vector3D(0, 0, 1);
                    break;
            }
            if (angle == 270)
            {
                angle = -90;
                i = 1;
            }

            RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D();
            myAxisAngleRotation3d.Axis = axis;
            myAxisAngleRotation3d.Angle = 0;
            myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            myRotateTransform3D.CenterX = Dimension / 2.0;
            myRotateTransform3D.CenterY = Dimension / 2.0;
            myRotateTransform3D.CenterZ = Dimension / 2.0;


            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0;
            myDoubleAnimation.To = angle;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(stoptime + speed * i));
            myDoubleAnimation.BeginTime = TimeSpan.FromMilliseconds(stoptime);
            myDoubleAnimation.RepeatBehavior = new RepeatBehavior(1);

            AnimationClock myClock = myDoubleAnimation.CreateClock();

            myClock.Completed += (sender, e) =>
            {
                Debug.WriteLine("Completed");
                AnimationCompleted?.Invoke(r);
            };


            foreach (var item in RotatablePart)
            {
                item.Transform = myRotateTransform3D;
            }

            myAxisAngleRotation3d.ApplyAnimationClock(AxisAngleRotation3D.AngleProperty, myClock);
        }

    }
}
