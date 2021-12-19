using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubing
{
    internal class Rotation
    {
        public Rotation(string twist)
        {
            Text = twist.Trim();
            int bodyStartIndex = -1;
            int bodyEndIndex = twist.Length;

            if (twist != String.Empty)
            {
                for (int i = 0; i < twist.Length; i++)
                {
                    char c = twist[i];
                    if (char.IsLetter(c))
                    {
                        if (bodyStartIndex == -1)
                        {
                            bodyStartIndex = i;
                        }
                    }
                    else
                    {
                        if (bodyStartIndex != -1)
                        {
                            bodyEndIndex = i;
                        }
                    }
                }
                string temp = twist.Substring(0, bodyStartIndex);
                if (temp == string.Empty)
                {
                    Profix = 0;
                }
                else
                {
                    Profix = Convert.ToInt32(temp);
                }
                temp = twist.Substring(bodyEndIndex);
                if (temp == string.Empty)
                {
                    Postfix = 0;
                }
                else
                {

                    if (temp == "'")
                    {
                        Postfix = 3;
                    }
                    else
                    {
                        Postfix = Convert.ToInt32(temp);
                    }
                }
                temp = twist.Substring(bodyStartIndex, bodyEndIndex - bodyStartIndex);
                Body = Enum.Parse<RotationType>(temp);
                //0Xw=2Xw
                switch (Body)
                {
                    case RotationType.Uw:
                    case RotationType.u:
                    case RotationType.Rw:
                    case RotationType.r:
                    case RotationType.Fw:
                    case RotationType.f:
                    case RotationType.Dw:
                    case RotationType.d:
                    case RotationType.Lw:
                    case RotationType.l:
                    case RotationType.Bw:
                    case RotationType.b:
                        if (Profix == 0)
                        {
                            Profix = 2;
                        }
                        break;
                }
                if (Profix == 0)
                {
                    Profix = 1;
                }
                if (Postfix == 0)
                {
                    Postfix = 1;
                }
            }
            else
            {
                Body = RotationType.None;
                Profix = 0;
                Postfix = 0;
            }

        }
        public string Text { get; internal set; }
        public RotationType Body { get; internal set; }
        public int Profix { get; internal set; }
        public int Postfix { get; internal set; }

        public Rotation Reversed()
        {
            string text = Text;
            int i = text.IndexOf('\'');
            if (i > 0)
            {
                text = text.Remove(i);
            }
            else
            {
                if (text.Substring(text.Length - 1) != "2")
                {
                    text = text + "'";
                }
            }
            Rotation result = new Rotation(text);
            return result;
        }

    }
}
