using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubing
{
    internal class CubeSolver
    {
        public delegate void SolutionFoundHandler(CubeSolver sender, string sol);
        public event SolutionFoundHandler? SolutionFound;
        Process p = new Process();

        public void BeginFind(string cubestring)
        {
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.WorkingDirectory = @"F:\rubiks-cube-NxNxN-solver\";
            p.StartInfo.WorkingDirectory = @"E:\MS Samples\rubiks-cube-NxNxN-solver-master";
            p.OutputDataReceived += P_OutputDataReceived;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.BeginOutputReadLine();
            StreamWriter writer = p.StandardInput;

            writer.WriteLine($"python rubiks-cube-solver.py --state {cubestring}");

        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string result = e.Data.ToString();
                if (result.IndexOf("Solution:") >= 0)
                {
                    SolutionFound?.Invoke(this,result.Substring("Solution:".Length));
                    p.Kill();
                }
            }
        }

    }
}
