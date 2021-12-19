using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Cubing
{
    internal class Cube
    {
        Dictionary<int, CubeBlock> allBlocks = new Dictionary<int, CubeBlock>();
        Dictionary<int, CubeBlock> changedBlocks = new Dictionary<int, CubeBlock>();

        public Cube(int dimension)
        {
            this.Dimension = dimension;
            this.SolvedCubeString = getSolvedCubeString(dimension);
            createCubeByString(this.SolvedCubeString);
            this.OriginalCubeString = "";
        }

        public Cube(string cubestring)
        {
            this.OriginalCubeString = cubestring;
            if (cubestring.Length % 6 == 0)
            {
                int dimension = cubestring.Length / 6;

                this.Dimension = (int)Math.Sqrt(dimension);
                this.SolvedCubeString = getSolvedCubeString(dimension);

                createCubeByString(cubestring);

                return;
            }
            throw new Exception("Cube string error!");
        }


        public int Dimension { get; internal set; }
        public int InternalDimension { get; internal set; }
        public int AllBlockCount { get; internal set; }
        public int VisibleBlockCount { get; internal set; }

        public string OriginalCubeString { get; internal set; }
        public string SolvedCubeString { get; internal set; }

        public string CubeString
        {
            get
            {
                const int U = 0, R = 1, F = 2, D = 3, L = 4, B = 5;

                StringBuilder sb = new StringBuilder();
                sb.Append('X', Dimension * Dimension * 6);
                for (int i = 0; i < Dimension * Dimension * 6; i++)
                {
                    int x = 0, y = 0, z = 0;
                    char c = 'X';
                    int faceIndex = i / (Dimension * Dimension);
                    int indexOnFace = i % (this.Dimension * this.Dimension);
                    int blockIndex = 0;
                    switch (faceIndex)
                    {
                        case U:
                            y = 0;
                            x = indexOnFace % this.Dimension;
                            z = indexOnFace / this.Dimension;
                            break;
                        case R:
                            x = Dimension - 1;
                            z = Dimension - 1 - indexOnFace % Dimension;
                            y = indexOnFace / Dimension;
                            break;
                        case F:
                            z = Dimension - 1;
                            x = indexOnFace % Dimension;
                            y = indexOnFace / Dimension;
                            break;
                        case D:
                            y = Dimension - 1;
                            x = indexOnFace % Dimension;
                            z = Dimension - 1 - indexOnFace / Dimension;
                            break;
                        case L:
                            x = 0;
                            y = indexOnFace / Dimension;
                            z = indexOnFace % Dimension;
                            break;
                        case B:
                            z = 0;
                            x = Dimension - 1 - indexOnFace % Dimension;
                            y = indexOnFace / Dimension;
                            break;
                        default:
                            break;
                    }
                    blockIndex = y * Dimension * Dimension + z * Dimension + x;
                    CubeBlock cb;
                    cb = allBlocks[blockIndex];
                    c = Convert.ToChar(cb.SideColors[faceIndex].ToString());
                    sb.Replace('X', c, i, 1);
                }
                return sb.ToString();
            }
        }

        public override string ToString()
        {
            string cs = this.CubeString;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Dimension; i++)
            {
                sb.Append(new String(' ', Dimension+1));
                sb.Append(cs.Substring(i * Dimension, Dimension));
                sb.Append('\r');
            }
            for (int i = 0; i < Dimension; i++)
            {
                sb.Append(cs.Substring(Dimension * Dimension * 4 + i * Dimension, Dimension));
                sb.Append(' ');
                sb.Append(cs.Substring(Dimension * Dimension * 2 + i * Dimension, Dimension));
                sb.Append(' ');
                sb.Append(cs.Substring(Dimension * Dimension * 1 + i * Dimension, Dimension));
                sb.Append(' ');
                sb.Append(cs.Substring(Dimension * Dimension * 5 + i * Dimension, Dimension));
                sb.Append('\r');
            }
            for (int i = 0; i < Dimension; i++)
            {
                sb.Append(new String(' ', Dimension+1));
                sb.Append(cs.Substring(Dimension * Dimension * 3 + i * Dimension, Dimension));
                sb.Append('\r');
            }

            return sb.ToString();
        }
        public void Move(Rotation twist)
        {
            Select(twist);
            AdjustBlockIndex( twist);
        }

        //选择某个扭动涉及的全部魔方块在allBlocks中的索引
        public void Select(Rotation twist)
        {
            Seperation s = new Seperation(Dimension, twist);
            int startx = s.StartX;
            int endx = s.EndX;
            int starty = s.StartY;
            int endy = s.EndY;
            int startz = s.StartZ;
            int endz = s.EndZ;

            //Rotation t = twist;
            //int startx = 0, endx = 0, starty = 0, endy = 0, startz = 0, endz = 0;//范围从0到Dimension - 1
            //int invisiblecount = 0;
            //switch (t.Body)
            //{
            //    case RotationType.U:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = t.Profix - 1;
            //        endy = starty;
            //        if (starty > 0 && endy < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.Uw:
            //    case RotationType.u:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = t.Profix - 1;
            //        if (endy < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * (endy - starty);
            //        }
            //        else
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.R:
            //        startx = Dimension - t.Profix;
            //        endx = startx;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (startx > 0 && endx < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.Rw:
            //    case RotationType.r:
            //        startx = Dimension - t.Profix;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (startx > 0)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * (endx - startx);
            //        }
            //        else
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.F:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = Dimension - t.Profix;
            //        endz = startz;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (startz > 0 && endz < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.Fw:
            //    case RotationType.f:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = Dimension - t.Profix;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (startz > 0)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension*(endz-startz);
            //        }
            //        else
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.D:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = Dimension - t.Profix;
            //        endy = starty;
            //        if (starty > 0 && endy < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.Dw:
            //    case RotationType.d:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = Dimension - t.Profix;
            //        endy = Dimension - 1;
            //        if (starty > 0)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * (endy - starty);
            //        }
            //        else
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.L:
            //        startx = t.Profix - 1;
            //        endx = startx;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (startx > 0 && endx < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.Lw:
            //    case RotationType.l:
            //        startx = 0;
            //        endx = t.Profix - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (endx < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * (endx - startx);
            //        }
            //        else
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.B:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = t.Profix - 1;
            //        endz = startz;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (startz > 0 && endz < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.Bw:
            //    case RotationType.b:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = t.Profix - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        if (endz < Dimension - 1)
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * (endz - startz);
            //        }
            //        else
            //        {
            //            invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.E:
            //        //奇数阶有效
            //        if (Dimension % 2 == 1)
            //        {
            //            startx = 0;
            //            endx = Dimension - 1;
            //            startz = 0;
            //            endz = Dimension - 1;
            //            starty = Dimension / 2;
            //            endy = starty;
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.M:
            //        //奇数阶有效
            //        if (Dimension % 2 == 1)
            //        {
            //            startx = Dimension / 2;
            //            endx = startx;
            //            startz = 0;
            //            endz = Dimension - 1;
            //            starty = 0;
            //            endy = Dimension - 1;
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.S:
            //        //奇数阶有效
            //        if (Dimension % 2 == 1)
            //        {
            //            startx = 0;
            //            endx = Dimension - 1;
            //            startz = Dimension / 2;
            //            endz = startz;
            //            starty = 0;
            //            endy = Dimension - 1;
            //            invisiblecount = InternalDimension * InternalDimension;
            //        }
            //        break;
            //    case RotationType.e:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 1;
            //        endy = Dimension - 1 - 1;
            //        invisiblecount = InternalDimension * InternalDimension;
            //        break;
            //    case RotationType.m:
            //        startx = 1;
            //        endx = Dimension - 1 - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        invisiblecount = InternalDimension * InternalDimension;
            //        break;
            //    case RotationType.s:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 1;
            //        endz = Dimension - 1 - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        invisiblecount = InternalDimension * InternalDimension;
            //        break;
            //    case RotationType.x:
            //    case RotationType.y:
            //    case RotationType.z:
            //        startx = 0;
            //        endx = Dimension - 1;
            //        startz = 0;
            //        endz = Dimension - 1;
            //        starty = 0;
            //        endy = Dimension - 1;
            //        invisiblecount = InternalDimension * InternalDimension * InternalDimension;
            //        break;
            //}
            //int resultcount = (endx - startx + 1) * (endy - starty + 1) * (endz - startz + 1);

            //int[] result = new int[resultcount - invisiblecount];
            changedBlocks.Clear();
            int i = 0, index = 0;
            for (int y = starty; y <= endy; y++)
            {
                for (int z = startz; z <= endz; z++)
                {
                    for (int x = startx; x <= endx; x++)
                    {
                        if (x == 0 || x == Dimension - 1 || z == 0 || z == Dimension - 1 || y == 0 || y == Dimension - 1)
                        {
                            index = y * Dimension * Dimension + z * Dimension + x;
                            //result[i] = index;
                            changedBlocks.Add(index, allBlocks[index]);
                            //i++;
                        }
                    }
                }
            }
            //return result;
        }

        public void AdjustBlockIndex( Rotation twist)
        {

            foreach (var cb in changedBlocks.Values)
            {
                cb.CalculateNewIndex(twist.Body, twist.Postfix);
                allBlocks[cb.Index] = cb;
            }

        }

        private void createCubeByString(string cubestring)
        {

            AllBlockCount = Dimension * Dimension * Dimension;
            InternalDimension = Dimension - 2;
            if (InternalDimension < 0) InternalDimension = 0;
            VisibleBlockCount = AllBlockCount - InternalDimension * InternalDimension * InternalDimension;

            allBlocks = new Dictionary<int, CubeBlock>();

            SideColor[][] sidecolors = getBlockSideColor(cubestring);
            int index = 0;
            int blockCount = 0;
            for (int y = 0; y < this.Dimension; y++)
            {
                for (int z = 0; z < this.Dimension; z++)
                {
                    for (int x = 0; x < this.Dimension; x++)
                    {
                        if (x == 0 || x == Dimension - 1 || z == 0 || z == Dimension - 1 || y == 0 || y == Dimension - 1)
                        {
                            index = x + z * this.Dimension + y * this.Dimension * this.Dimension;
                            CubeBlock cb = new CubeBlock(sidecolors[index]);
                            cb.X = x;
                            cb.Y = y;
                            cb.Z = z;
                            cb.Index = index;
                            cb.OldIndex = index;
                            cb.Dimension = Dimension;
                            allBlocks.Add(cb.Index, cb);
                            blockCount++;
                        }
                    }
                }
            }
        }

        private SideColor[][] getBlockSideColor(string cubestring)
        {

            const int U = 0, R = 1, F = 2, D = 3, L = 4, B = 5;
            SideColor[][] result = new SideColor[this.AllBlockCount][];
            for (int i = 0; i < this.AllBlockCount; i++)
            {
                result[i] = new SideColor[6];
            }

            for (int i = 0; i < cubestring.Length; i++)
            {
                int x = 0, y = 0, z = 0;
                char c = cubestring[i];
                int faceIndex = i / (Dimension * Dimension);
                int indexOnFace = i % (this.Dimension * this.Dimension);
                int blockIndex = 0;
                switch (faceIndex)
                {
                    case U:
                        y = 0;
                        x = indexOnFace % this.Dimension;
                        z = indexOnFace / this.Dimension;
                        break;
                    case R:
                        x = Dimension - 1;
                        z = Dimension - 1 - indexOnFace % Dimension;
                        y = indexOnFace / Dimension;
                        break;
                    case F:
                        z = Dimension - 1;
                        x = indexOnFace % Dimension;
                        y = indexOnFace / Dimension;
                        break;
                    case D:
                        y = Dimension - 1;
                        x = indexOnFace % Dimension;
                        z = Dimension - 1 - indexOnFace / Dimension;
                        break;
                    case L:
                        x = 0;
                        y = indexOnFace / Dimension;
                        z = indexOnFace % Dimension;
                        break;
                    case B:
                        z = 0;
                        x = Dimension - 1 - indexOnFace % Dimension;
                        y = indexOnFace / Dimension;
                        break;
                    default:
                        break;
                }
                blockIndex = y * Dimension * Dimension + z * Dimension + x;
                result[blockIndex][faceIndex] = Enum.Parse<SideColor>(c.ToString());
            }
            return result;
        }

        private string getSolvedCubeString(int dimension)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('U', dimension * dimension);
            sb.Append('R', dimension * dimension);
            sb.Append('F', dimension * dimension);
            sb.Append('D', dimension * dimension);
            sb.Append('L', dimension * dimension);
            sb.Append('B', dimension * dimension);

            return sb.ToString();
        }

    }
}
