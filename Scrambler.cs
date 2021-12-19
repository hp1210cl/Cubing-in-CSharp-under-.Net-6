using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubing
{
    internal class Scrambler
    {
        public Scrambler(int dimension, int depth)
        {
            Dimension = dimension;
            Depth = depth;
            Moves = new Rotation[depth];
            StupidSolution = new Rotation[depth];
            Random random = new Random();
            string[][] basic = { new string[] { "U", "D" }, new string[] { "F", "B" }, new string[] { "L", "R" } };
            string[] x = { "", "w" };
            string[] y = { "", "2", "'" };
            int body1 = 0, body2 = 0, prefix = 0, postfix = 0, w = 0;
            string twist = string.Empty;

            for (int i = 0; i < depth; i++)
            {
                body1 += random.Next(2) + 1;
                body1 = body1 % 3;

                body2 = random.Next(2);
                prefix = random.Next(Dimension / 2);
                if (prefix == 0) prefix = 1;
                postfix = random.Next(3);
                if (dimension > 3)
                {
                    w = random.Next(2);
                }
                else
                {
                    w = 0;
                }
                twist = $"{(prefix == 1 ? string.Empty : prefix.ToString())}{basic[body1][body2]}{x[w]}{y[postfix]}";
                Moves[i] = new Rotation(twist);
                StupidSolution[depth - 1 - i] = Moves[i].Reversed();
            }
        }

        public int Dimension { get; internal set; }
        public int Depth { get; internal set; }

        public Rotation[] Moves { get; }

        public string MovesString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Moves.Length; i++)
                {
                    sb.Append(Moves[i].Text);
                    sb.Append(" ");
                }
                return sb.ToString();
            }
        }
        public Rotation[] StupidSolution { get; }

        public string StupidSolutionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < StupidSolution.Length; i++)
                {
                    sb.Append(StupidSolution[i].Text);
                    sb.Append(" ");
                }
                return sb.ToString();
            }
        }
    }
}
