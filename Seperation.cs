using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubing
{
    public enum SeperationDircetion
    {
        None,
        X,
        Y,
        Z
    }
    internal class Seperation
    {
        public Seperation(int dim,Rotation r)
        {
            Dimension = dim;
            Rotation = r;
            int startx = 0, endx = 0, starty = 0, endy = 0, startz = 0, endz = 0;//范围从0到dim - 1
            Direction = SeperationDircetion.None;
            switch (r.Body)
            {
                case RotationType.U:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = r.Profix - 1;
                    endy = starty;
                    this.Direction = SeperationDircetion.Y;
                    break;
                case RotationType.Uw:
                case RotationType.u:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = r.Profix - 1;
                    this.Direction = SeperationDircetion.Y;
                    break;
                case RotationType.R:
                    startx = dim - r.Profix;
                    endx = startx;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.X;
                    break;
                case RotationType.Rw:
                case RotationType.r:
                    startx = dim - r.Profix;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.X;
                    break;
                case RotationType.F:
                    startx = 0;
                    endx = dim - 1;
                    startz = dim - r.Profix;
                    endz = startz;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Z;
                    break;
                case RotationType.Fw:
                case RotationType.f:
                    startx = 0;
                    endx = dim - 1;
                    startz = dim - r.Profix;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Z;
                    break;
                case RotationType.D:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = dim - r.Profix;
                    endy = starty;
                    this.Direction = SeperationDircetion.Y;
                    break;
                case RotationType.Dw:
                case RotationType.d:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = dim - r.Profix;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Y;
                    break;
                case RotationType.L:
                    startx = r.Profix - 1;
                    endx = startx;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.X;
                    break;
                case RotationType.Lw:
                case RotationType.l:
                    startx = 0;
                    endx = r.Profix - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.X;
                    break;
                case RotationType.B:
                    startx = 0;
                    endx = dim - 1;
                    startz = r.Profix - 1;
                    endz = startz;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Z;
                    break;
                case RotationType.Bw:
                case RotationType.b:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = r.Profix - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Z;
                    break;
                case RotationType.E:
                    //奇数阶有效
                    if (dim % 2 == 1)
                    {
                        startx = 0;
                        endx = dim - 1;
                        startz = 0;
                        endz = dim - 1;
                        starty = dim / 2;
                        endy = starty;
                        this.Direction = SeperationDircetion.Y;
                    }
                    break;
                case RotationType.M:
                    //奇数阶有效
                    if (dim % 2 == 1)
                    {
                        startx = dim / 2;
                        endx = startx;
                        startz = 0;
                        endz = dim - 1;
                        starty = 0;
                        endy = dim - 1;
                        this.Direction = SeperationDircetion.X;
                    }
                    break;
                case RotationType.S:
                    //奇数阶有效
                    if (dim % 2 == 1)
                    {
                        startx = 0;
                        endx = dim - 1;
                        startz = dim / 2;
                        endz = startz;
                        starty = 0;
                        endy = dim - 1;
                        this.Direction = SeperationDircetion.Z;
                    }
                    break;
                case RotationType.e:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 1;
                    endy = dim - 1 - 1;
                    this.Direction = SeperationDircetion.Y;
                    break;
                case RotationType.m:
                    startx = 1;
                    endx = dim - 1 - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.X;
                    break;
                case RotationType.s:
                    startx = 0;
                    endx = dim - 1;
                    startz = 1;
                    endz = dim - 1 - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Z;
                    break;
                case RotationType.x:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.X;
                    break;
                case RotationType.y:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Y;
                    break;
                case RotationType.z:
                    startx = 0;
                    endx = dim - 1;
                    startz = 0;
                    endz = dim - 1;
                    starty = 0;
                    endy = dim - 1;
                    this.Direction = SeperationDircetion.Z;
                    break;
            }
            this.StartX = startx;
            this.EndX = endx;
            this.StartY = starty;
            this.EndY = endy;
            this.StartZ = startz;
            this.EndZ = endz;
        }
        //start,end为块的索引顺序
        public int StartX { get; }
        public int EndX { get; }
        public int StartY { get; }
        public int EndY { get; }
        public int StartZ { get; }
        public int EndZ { get; }

        public SeperationDircetion Direction { get; }
        public int Dimension { get; }
        public Rotation Rotation { get; }
    }
}
