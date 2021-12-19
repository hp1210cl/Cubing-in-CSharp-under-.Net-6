using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Cubing
{
    public enum SideColor
    {
        X,
        U,
        R,
        F,
        D,
        L,
        B
    }
    public enum BlockSide
    {
        U,
        R,
        F,
        D,
        L,
        B
    }
    public enum RotationType
    {
        None,
        U,
        R,
        F,
        D,
        L,
        B,
        Uw,
        Rw,
        Fw,
        Dw,
        Lw,
        Bw,
        u,
        r,
        f,
        d,
        l,
        b,
        M,
        E,
        S,
        m,
        e,
        s,
        x,
        y,
        z
    }

    internal class CubeBlock
    {
        public CubeBlock(SideColor[] colors)
        {
            SideColors = colors;
        }

        public int Index { get; set; }
        public int OldIndex { get; set; }
        //所在魔方的维数
        public int Dimension { get; set; }
        //颜色顺序为URFDLB面
        public SideColor[] SideColors { get; internal set; }
        //在块坐标系统的坐标，与3D坐标轴方向一致，原点为左上角的块
        //X由L面到R面方向,Y由U面到D面方向,Z由B面到F面方向
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override string ToString()
        {
            return $"Index={Index.ToString("D4")} {OldIndex.ToString("D4")} ({X},{Y},{Z})  [{SideColors[0].ToString()},{SideColors[1].ToString()},{SideColors[2].ToString()},{SideColors[3].ToString()},{SideColors[4].ToString()},{SideColors[5].ToString()},]";
        }

        //每次旋转都是90度的倍数
        //重新计算在块坐标系中的坐标，从而得出新的块索引
        public int CalculateNewIndex(RotationType rotation, int count)
        {
            //RotateTransform rt = new RotateTransform();
            //Matrix m = new Matrix();
            //Point p;
            //Debug.Write($"{Index} @ ({X},{Z },{Y} )  --  ");
            int newIndex = 0;
            SideColor sc;
            const int U = 0, R = 1, F = 2, D = 3, L = 4, B = 5;
            int temp;
            switch (rotation)
            {
                case RotationType.U:
                case RotationType.Uw:
                case RotationType.u:
                case RotationType.y:
                    //rt.Angle = 90 * count;
                    //rt.CenterX = (Dimension - 1) / 2.0;
                    //rt.CenterY = (Dimension - 1) / 2.0;
                    //m = rt.Value;
                    //p = new Point(X, Z);
                    //p = m.Transform(p);
                    //p = new Point(Math.Round(p.X), Math.Round(p.Y));
                    //Debug.Write($"{p.ToString()}  -  ");

                    //Y值不变,旋转变化XZ
                    //颜色面会发生调整，UD不变，FLBR依次递进
                    switch (count)
                    {
                        case 1:
                            temp = X;
                            X = Dimension - 1 - Z;
                            Z = temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[R];
                            SideColors[R] = SideColors[B];
                            SideColors[B] = SideColors[L];
                            SideColors[L] = sc;
                            break;
                        case 2:
                            X = Dimension - 1 - X;
                            Z = Dimension - 1 - Z;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[B];
                            SideColors[B] = sc;
                            sc = SideColors[R];
                            SideColors[R] = SideColors[L];
                            SideColors[L] = sc;
                            break;
                        case 3:
                            temp = X;
                            X = Z;
                            Z = Dimension - 1 - temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[L];
                            SideColors[L] = SideColors[B];
                            SideColors[B] = SideColors[R];
                            SideColors[R] = sc;
                            break;
                    }
                    //Debug.WriteLine($"{X},{Z}");
                    //this.X = (int)p.X;
                    //this.Z = (int)p.Y;
                    break;
                case RotationType.D:
                case RotationType.Dw:
                case RotationType.d:
                case RotationType.E:
                case RotationType.e:
                    //rt.Angle = 90 * (4 - count);
                    //rt.CenterX = (Dimension - 1) / 2.0;
                    //rt.CenterY = (Dimension - 1) / 2.0;
                    //m = rt.Value;
                    //p = new Point(X, Z);
                    //p = m.Transform(p);
                    //p = new Point(Math.Round(p.X), Math.Round(p.Y));
                    //Y值不变,旋转变化XZ
                    //颜色面会发生调整，UD不变，FRBL依次递进
                    switch (count)
                    {
                        case 1:
                            temp = X;
                            X = Z;
                            Z = Dimension - 1 - temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[L];
                            SideColors[L] = SideColors[B];
                            SideColors[B] = SideColors[R];
                            SideColors[R] = sc;
                            break;
                        case 2:
                            X = Dimension - 1 - X;
                            Z = Dimension - 1 - Z;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[B];
                            SideColors[B] = sc;
                            sc = SideColors[R];
                            SideColors[R] = SideColors[L];
                            SideColors[L] = sc;
                            break;
                        case 3:
                            temp = X;
                            X = Dimension - 1 - Z;
                            Z = temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[R];
                            SideColors[R] = SideColors[B];
                            SideColors[B] = SideColors[L];
                            SideColors[L] = sc;
                            break;
                    }
                    //Debug.WriteLine($"{p.ToString()}  -  {X},{Z}");
                    //this.X = (int)p.X;
                    //this.Z = (int)p.Y;
                    break;
                case RotationType.R:
                case RotationType.Rw:
                case RotationType.r:
                case RotationType.x:
                    //rt.Angle = 90 * count;
                    //rt.CenterX = (Dimension - 1) / 2.0;
                    //rt.CenterY = (Dimension - 1) / 2.0;
                    //m = rt.Value;
                    //p = new Point(Y, Z);
                    //p = m.Transform(p);
                    //p = new Point(Math.Round(p.X), Math.Round(p.Y));
                    //X值不变,旋转变化YZ
                    //颜色面会发生调整，RL不变，FUBD依次递进
                    switch (count)
                    {
                        case 1:
                            temp = Y;
                            Y = Dimension - 1 - Z;
                            Z = temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[D];
                            SideColors[D] = SideColors[B];
                            SideColors[B] = SideColors[U];
                            SideColors[U] = sc;
                            break;
                        case 2:
                            Y = Dimension - 1 - Y;
                            Z = Dimension - 1 - Z;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[B];
                            SideColors[B] = sc;
                            sc = SideColors[D];
                            SideColors[D] = SideColors[U];
                            SideColors[U] = sc;
                            break;
                        case 3:
                            temp = Y;
                            Y = Z;
                            Z = Dimension - 1 - temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[U];
                            SideColors[U] = SideColors[B];
                            SideColors[B] = SideColors[D];
                            SideColors[D] = sc;
                            break;
                    }
                    //Debug.WriteLine($"{p.ToString()}  -  {Y},{Z}");
                    //this.Y = (int)p.X;
                    //this.Z = (int)p.Y;
                    break;
                case RotationType.L:
                case RotationType.Lw:
                case RotationType.l:
                case RotationType.M:
                case RotationType.m:
                    //rt.Angle = 90 * (4 - count);
                    //rt.CenterX = (Dimension - 1) / 2.0;
                    //rt.CenterY = (Dimension - 1) / 2.0;
                    //m = rt.Value;
                    //p = new Point(Y, Z);
                    //p = m.Transform(p);
                    //p = new Point(Math.Round(p.X), Math.Round(p.Y));
                    //X值不变,旋转变化YZ
                    //颜色面会发生调整，RL不变，FDBU依次递进
                    switch (count)
                    {
                        case 1:
                            temp = Y;
                            Y = Z;
                            Z = Dimension - 1 - temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[U];
                            SideColors[U] = SideColors[B];
                            SideColors[B] = SideColors[D];
                            SideColors[D] = sc;
                            break;
                        case 2:
                            Y = Dimension - 1 - Y;
                            Z = Dimension - 1 - Z;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[B];
                            SideColors[B] = sc;
                            sc = SideColors[D];
                            SideColors[D] = SideColors[U];
                            SideColors[U] = sc;
                            break;
                        case 3:
                            temp = Y;
                            Y = Dimension - 1 - Z;
                            Z = temp;
                            sc = SideColors[F];
                            SideColors[F] = SideColors[D];
                            SideColors[D] = SideColors[B];
                            SideColors[B] = SideColors[U];
                            SideColors[U] = sc;
                            break;
                    }
                    //Debug.WriteLine($"{p.ToString()}  -  {Y},{Z}");
                    //this.Y = (int)p.X;
                    //this.Z = (int)p.Y;
                    break;
                case RotationType.F:
                case RotationType.Fw:
                case RotationType.f:
                case RotationType.z:
                case RotationType.S:
                case RotationType.s:
                    //rt.Angle = 90 * count;
                    //rt.CenterX = (Dimension - 1) / 2.0;
                    //rt.CenterY = (Dimension - 1) / 2.0;
                    //m = rt.Value;
                    //p = new Point(X, Y);
                    //p = m.Transform(p);
                    //p = new Point(Math.Round(p.X), Math.Round(p.Y));
                    //Z值不变,旋转变化XY
                    //颜色面会发生调整，FB不变，URDL依次递进
                    switch (count)
                    {
                        case 1:
                            temp = X;
                            X = Dimension - 1 - Y;
                            Y = temp;
                            sc = SideColors[U];
                            SideColors[U] = SideColors[L];
                            SideColors[L] = SideColors[D];
                            SideColors[D] = SideColors[R];
                            SideColors[R] = sc;
                            break;
                        case 2:
                            X = Dimension - 1 - X;
                            Y = Dimension - 1 - Y;
                            sc = SideColors[U];
                            SideColors[U] = SideColors[D];
                            SideColors[D] = sc;
                            sc = SideColors[L];
                            SideColors[L] = SideColors[R];
                            SideColors[R] = sc;
                            break;
                        case 3:
                            temp = X;
                            X = Y;
                            Y = Dimension - 1 - temp;
                            sc = SideColors[U];
                            SideColors[U] = SideColors[R];
                            SideColors[R] = SideColors[D];
                            SideColors[D] = SideColors[L];
                            SideColors[L] = sc;
                            break;
                    }
                    //Debug.WriteLine($"{p.ToString()}  -  {X},{Y}");
                    //this.X = (int)p.X;
                    //this.Y = (int)p.Y;
                    break;
                case RotationType.B:
                case RotationType.Bw:
                case RotationType.b:
                    //rt.Angle = 90 * (4 - count);
                    //rt.CenterX = (Dimension - 1) / 2.0;
                    //rt.CenterY = (Dimension - 1) / 2.0;
                    //m = rt.Value;
                    //p = new Point(X, Y);
                    //p = m.Transform(p);
                    //p = new Point(Math.Round(p.X), Math.Round(p.Y));
                    //Z值不变,旋转变化XY
                    //颜色面会发生调整，FB不变，ULDR依次递进
                    switch (count)
                    {
                        case 1:
                            temp = X;
                            X = Y;
                            Y = Dimension - 1 - temp;
                            sc = SideColors[U];
                            SideColors[U] = SideColors[R];
                            SideColors[R] = SideColors[D];
                            SideColors[D] = SideColors[L];
                            SideColors[L] = sc;
                            break;
                        case 2:
                            X = Dimension - 1 - X;
                            Y = Dimension - 1 - Y;
                            sc = SideColors[U];
                            SideColors[U] = SideColors[D];
                            SideColors[D] = sc;
                            sc = SideColors[L];
                            SideColors[L] = SideColors[R];
                            SideColors[R] = sc;
                            break;
                        case 3:
                            temp = X;
                            X = Dimension - 1 - Y;
                            Y = temp;
                            sc = SideColors[U];
                            SideColors[U] = SideColors[L];
                            SideColors[L] = SideColors[D];
                            SideColors[D] = SideColors[R];
                            SideColors[R] = sc;
                            break;
                    }
                    //Debug.WriteLine($"{p.ToString()}  -  {X},{Y}");
                    //this.X = (int)p.X;
                    //this.Y = (int)p.Y;
                    break;
            }
            this.Index = Y * Dimension * Dimension + Z * Dimension + X;

            return newIndex;
        }

    }
}
