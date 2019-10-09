using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roo.CompileAssembly
{
    public partial class Compiler
    {
        private string GetRuntimeCSCexe() { return System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "csc.exe"); }
        private string CreateTargetCommand()
        {
            if (this.OutputAssembly.ToUpper().EndsWith("EXE"))
                return "-target:exe";
            return "-target:library";
        }

        /// <summary>
        /// Build code bằng commandpromt: gọi file Csc.exe trong thư mục mặc định khi cài Framework 4.0
        /// </summary>
        /// <returns></returns>
        public bool CompileAssemblyByCSC()
        {
            bool compileOk = false;
            if (this.SourceFiles.Any() == false)
                return compileOk;
            var pathCSCExe = this.GetRuntimeCSCexe();
            if (System.IO.File.Exists(pathCSCExe) == false)
                return compileOk;
            var pathDirTemp = this.CreateSourceFilesBuildTemp(DateTime.Now, v_KindBuildAssembly.ByCommandCSC);
            try
            {
                this.SourceFilesBuildTemp.ForEach(x =>
                {
                    x = Compiler.AddDoubleQuotesForce(x);
                });
                var pathFileCodeCs = string.Join(" ", this.SourceFilesBuildTemp.ToArray());

                System.Diagnostics.Process proc;
                System.Diagnostics.ProcessStartInfo psiUser = new System.Diagnostics.ProcessStartInfo(pathCSCExe)
                {
                    Arguments = string.Format("{0} -out:{1} {2} {3}", this.CreateTargetCommand(), Compiler.AddDoubleQuotesForce(this.OutputAssembly), this.CreateOptionWin32Icon(), pathFileCodeCs),
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                proc = new System.Diagnostics.Process() { StartInfo = psiUser };
                proc.Start();
                proc.WaitForExit();
                compileOk = true;
                if (compileOk && System.IO.File.Exists(this.OutputAssembly))
                {
                    if (this.AssemblyFileVersionCode == "")
                        compileOk = true;
                    else
                        compileOk = Compiler.CompareStringVersion(this.AssemblyFileVersionCode, Compiler.GetVersionStringFile(this.OutputAssembly));
                }
                else
                {
                    compileOk = false;
                }
                return compileOk;
            }
            catch
            {
                return compileOk;
            }
            finally
            {
                try { Compiler.DirectoryDeleteForce(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(pathDirTemp))); } catch { }
            }
        }
    }
}
