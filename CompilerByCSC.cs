using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roo.CompileAssembly
{
    public partial class Compiler
    {
        private string GetRuntimeCSCexe() { return System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "csc.exe"); }
        public bool CompileAssemblyByCSC()
        {
            bool compileOk = false;
            if (this.SourceFiles.Any() == false)
                return compileOk;
            var pathCSCExe = this.GetRuntimeCSCexe();
            if (System.IO.File.Exists(pathCSCExe) == false)
                return compileOk;
            this.SourceFiles.ForEach(x =>
            {
                x = Compiler.AddDoubleQuotesForce(x);
            });
            var pathFileCodeCs = string.Join(" ", this.SourceFiles.ToArray());

            System.Diagnostics.Process proc;
            System.Diagnostics.ProcessStartInfo psiUser = new System.Diagnostics.ProcessStartInfo(pathCSCExe)
            {
                Arguments = string.Format("-out:{0} {1} {2}", Compiler.AddDoubleQuotesForce(this.OutputAssembly), this.CreateOptionWin32Icon(), pathFileCodeCs),
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            proc = new System.Diagnostics.Process() { StartInfo = psiUser };
            proc.Start();
            compileOk = true;
            return compileOk;
        }
    }
}
