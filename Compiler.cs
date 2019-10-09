using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
[assembly: AssemblyTitle("Roo.CompileAssembly")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Roo.CompileAssembly")]
[assembly: AssemblyCopyright("Copyright © 2019 by ongtrieuhau861@gmail.com")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("04c0a613-35ae-412a-92cd-e81c6ac90dde")]
[assembly: AssemblyVersion("1.19.10.10")]
[assembly: AssemblyFileVersion("1.19.10.10")]
namespace Roo.CompileAssembly
{
    public class Compiler
    {
        /// <summary>
        /// Trả về chuỗi có dấu " ở đầu và cuối chuỗi (Có kiểm tra không có mới thêm)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AddDoubleQuotesForce(string value)
        {
            if ((value + "").StartsWith("\"") == false)
                value = "\"" + value;
            if ((value + "").EndsWith("\"") == false)
                value = value + "\"";
            return value;
        }
        public Compiler() { }
        public string OutputAssembly { get; set; } = "";
        public List<string> ReferencedAssemblies { get; set; }
        public List<string> SourceFiles { get; set; }
        public string PathWin32Icon { get; set; }
        public string CreateOptionWin32Icon()
        {
            if (this.PathWin32Icon + "" == "") return "";
            try
            {
                if (System.IO.File.Exists(this.PathWin32Icon) == false) return "";
                if (System.IO.File.Exists(this.PathWin32Icon) == true)
                    return string.Format("-win32icon:{0}", Compiler.AddDoubleQuotesForce(this.PathWin32Icon));
            }
            catch { }
            return "";
        }

        private List<string> f_ReferencedAssemblyDirs = null;
        private List<string> ReferencedAssemblyDirs
        {
            get
            {
                if (this.f_ReferencedAssemblyDirs == null)
                {
                    this.f_ReferencedAssemblyDirs = new List<string>()
                    {
                        System.IO.Directory.GetCurrentDirectory(),
                        System.IO.Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Reference Assemblies","Microsoft","Framework",".NETFramework","v4.0"),
                        System.IO.Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Reference Assemblies","Microsoft","Framework",".NETFramework","v4.0"),
                        RuntimeEnvironment.GetRuntimeDirectory(),
                    };
                }
                return this.f_ReferencedAssemblyDirs;
            }
        }
        private List<string> f_ReferencedAssemblyNoPaths = null;
        private List<string> ReferencedAssemblyNoPaths
        {
            get
            {
                if (this.f_ReferencedAssemblyNoPaths == null)
                {
                    this.f_ReferencedAssemblyNoPaths = new List<string>() { "System.dll", "System.Core.dll", "mscorlib.dll" };
                }
                return this.f_ReferencedAssemblyNoPaths;
            }
        }

        public System.CodeDom.Compiler.CompilerResults CompilerResults { get; set; }

        public bool CompileAssembly()
        {
            bool compileOk = false;
            if (this.SourceFiles.Any() == false)
                return compileOk;
            this.CompilerResults = null;
            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            if (provider != null)
            {
                System.CodeDom.Compiler.CompilerParameters compilerParameters = new System.CodeDom.Compiler.CompilerParameters();

                // Generate an executable instead of 
                // a class library.
                compilerParameters.GenerateExecutable = true;
                // Save the assembly as a physical file.
                compilerParameters.GenerateInMemory = false;
                // Set whether to treat all warnings as errors.
                compilerParameters.TreatWarningsAsErrors = false;
                // Specify the assembly file name to generate.
                if (this.OutputAssembly + "" != "")
                    compilerParameters.OutputAssembly = this.OutputAssembly;
                else
                {
                    var sourceFirst = new System.IO.FileInfo(this.SourceFiles.First());
                    compilerParameters.OutputAssembly = String.Format(@"{0}\{1}.exe",
                    System.Environment.CurrentDirectory,
                    sourceFirst.Name.Replace(".", "_"));
                }
                compilerParameters.CompilerOptions = string.Format("/optimize {0}", this.CreateOptionWin32Icon());

                // Set reference dll
                if (this.ReferencedAssemblies != null &&
                    this.ReferencedAssemblies.Any())
                {
                    foreach (string itemValue in this.ReferencedAssemblies)
                    {
                        var item = itemValue + "";
                        if (item.ToUpper().EndsWith(".dll".ToUpper()) == false && item.ToUpper().EndsWith(".exe".ToUpper()) == false)
                            item = item + ".dll";
                        if (System.IO.File.Exists(item))
                            compilerParameters.ReferencedAssemblies.Add(item);
                        else if (this.ReferencedAssemblyNoPaths.Where(x => x.ToUpper().Contains(item.ToUpper())).Any())
                        {
                            compilerParameters.ReferencedAssemblies.Add(item);
                        }
                        else
                        {
                            foreach (var pathDir in this.ReferencedAssemblyDirs)
                            {
                                var pathFile = System.IO.Path.Combine(pathDir, item);
                                if (System.IO.File.Exists(pathFile))
                                {
                                    compilerParameters.ReferencedAssemblies.Add(pathFile);
                                    break;
                                }
                            }
                        }
                    }
                }
                this.ReferencedAssemblyNoPaths.ForEach(x =>
                {
                    if (compilerParameters.ReferencedAssemblies.Contains(x) == false)
                        compilerParameters.ReferencedAssemblies.Add(x);
                });
                // Invoke compilation of the source file.
                this.CompilerResults = provider.CompileAssemblyFromFile(compilerParameters,
                    this.SourceFiles.ToArray());
                compileOk = !(this.CompilerResults.Errors.Count > 0);
            }
            return compileOk;
        }

        private string GetRuntimeCSCexe() { return System.IO.Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "csc.exe"); }
        public bool CompileAssemblyByCSC()
        {
            bool compileOk = false;
            if (this.SourceFiles.Any() == false)
                return compileOk;
            this.SourceFiles.ForEach(x =>
            {
                x = Compiler.AddDoubleQuotesForce(x);
            });
            var pathFileCodeCs = string.Join(" ", this.SourceFiles.ToArray());

            System.Diagnostics.Process proc;
            System.Diagnostics.ProcessStartInfo psiUser = new System.Diagnostics.ProcessStartInfo(this.GetRuntimeCSCexe())
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
