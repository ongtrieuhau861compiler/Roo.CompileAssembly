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
    public enum v_InfoAssembly
    {
        Title, Description, Configuration, Company, Product,
        Copyright, Trademark, Culture, Version, FileVersion
    }
    public enum v_KindBuildAssembly
    {
        ByCodeProvider, ByCommandCSC
    }
    public partial class Compiler
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
        /// <summary>
        /// Trả về chuỗi nằm giữa 2 giá trị start và end
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string GetBetweenStringsFirstForce(string inputString, string start, string end)
        {
            try
            {
                var inputStringUpper = (inputString + "").ToUpper();
                var startUpper = (start + "").ToUpper();
                var endUpper = (end + "").ToUpper();
                if (startUpper == "" || endUpper == "") return "";
                if (inputStringUpper.Contains(startUpper) == false || inputStringUpper.Contains(endUpper) == false) return "";
                int p1 = inputStringUpper.IndexOf(startUpper) + startUpper.Length;
                int p2 = inputStringUpper.IndexOf(endUpper, p1);

                if (end == "") return (inputString.Substring(p1));
                else return inputString.Substring(p1, p2 - p1);
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// Tìm giá trị Assembly theo loại truyền vào
        /// </summary>
        /// <param name="codeCs"></param>
        /// <param name="v_InfoAssembly"></param>
        /// <returns></returns>
        public static string GetInfoAssemblyFromCode(string codeCs, v_InfoAssembly v_InfoAssembly)
        {
            return Compiler.GetBetweenStringsFirstForce(codeCs, string.Format("[assembly: Assembly{0}(\"", v_InfoAssembly.ToString()), "\")]");
        }
        /// <summary>
        /// Tìm giá trị Assembly theo loại truyền vào, trả về dạng code C#
        /// Ví dụ: [assembly: AssemblyTitle("Roo.CompileAssembly")]
        /// </summary>
        /// <param name="codeCs"></param>
        /// <param name="v_InfoAssembly"></param>
        /// <returns></returns>
        public static string GetInfoAssemblyFromCodeReturnCs(string codeCs, v_InfoAssembly v_InfoAssembly)
        {
            var start = string.Format("[assembly: Assembly{0}(\"", v_InfoAssembly.ToString());
            var end = "\")]";
            var contentBetween = Compiler.GetBetweenStringsFirstForce(codeCs, start, end);
            if (contentBetween != "")
                return start + contentBetween + end;
            return "";
        }
        /// <summary>
        /// Delete thư mục dạng chạy ngầm, có sử dụng BackgroundWorker,
        /// 1. Xóa thư mục
        /// 2. Kiểm tra nếu thư mục tồn tại, thì xóa các file nằm trong thư mục đó kể cả thư mục con
        /// 3. Xóa lại thư mục lần nữa
        /// </summary>
        /// <param name="path"></param>
        public static void DirectoryDeleteForce(string path)
        {
            var backgroundWorker = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            backgroundWorker.DoWork += (sender, e) =>
            {
                try { System.IO.Directory.Delete(path, true); } catch { }
                if (System.IO.Directory.Exists(path))
                {
                    var allFiles = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
                    if (allFiles != null && allFiles.Any())
                    {
                        foreach (var pathFile in allFiles)
                            try { System.IO.File.Delete(pathFile); } catch { }
                    }
                }
                try { System.IO.Directory.Delete(path, true); } catch { }
            };
            backgroundWorker.RunWorkerAsync();
        }
        public Compiler() { }
        public string OutputAssembly { get; set; }
        public List<string> ReferencedAssemblies { get; set; }
        public List<string> SourceFiles { get; set; }
        public List<string> SourceFilesBuildTemp { get; set; }
        public string PathWin32Icon { get; set; }

        private string CreateOptionWin32Icon()
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
        private string CreateSourceFilesBuildTemp(DateTime dateTimeBuild, v_KindBuildAssembly v_KindBuildAssembly)
        {
            var pathDirSourceFilesBuildTemp = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                System.Reflection.Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace,
                v_KindBuildAssembly.ToString(),
                "Roo.CompileAssembly", dateTimeBuild.ToString("yyyy-MM-dd-HH-mm-ss-fff", System.Globalization.CultureInfo.InvariantCulture));
            try { System.IO.Directory.Delete(pathDirSourceFilesBuildTemp, true); } catch { }
            try { System.IO.Directory.CreateDirectory(pathDirSourceFilesBuildTemp); } catch { }
            this.SourceFilesBuildTemp = new List<string>();
            this.SourceFiles.ForEach(x =>
            {
                var destFileName = System.IO.Path.Combine(pathDirSourceFilesBuildTemp, System.IO.Path.GetRandomFileName() + "." + System.IO.Path.GetFileName(x));
                var contentSource = System.IO.File.ReadAllText(x);
                var codeAssemblyTitle = Compiler.GetInfoAssemblyFromCodeReturnCs(contentSource, v_InfoAssembly.Title);
                if (codeAssemblyTitle != "")
                {
                    var contentAssemblyTitle = Compiler.GetInfoAssemblyFromCode(codeAssemblyTitle, v_InfoAssembly.Title);
                    var codeAssemblyTitleAddBuild = codeAssemblyTitle.Replace(contentAssemblyTitle, contentAssemblyTitle + string.Format("[build.{0}]", v_KindBuildAssembly.ToString()));
                    contentSource = contentSource.Replace(codeAssemblyTitle, codeAssemblyTitleAddBuild);
                }
                System.IO.File.WriteAllText(destFileName, contentSource, System.Text.Encoding.Unicode);
                this.SourceFilesBuildTemp.Add(destFileName);
            });
            return pathDirSourceFilesBuildTemp;
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
            var pathDirTemp = this.CreateSourceFilesBuildTemp(DateTime.Now, v_KindBuildAssembly.ByCodeProvider);

            try
            {
                Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
                if (provider != null)
                {
                    System.CodeDom.Compiler.CompilerParameters compilerParameters = new System.CodeDom.Compiler.CompilerParameters();

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

                    // Generate an executable instead of 
                    // a class library.
                    if (this.OutputAssembly.ToUpper().EndsWith("EXE"))
                        compilerParameters.GenerateExecutable = true;
                    else
                        compilerParameters.GenerateExecutable = false;

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
                        this.SourceFilesBuildTemp.ToArray());
                    compileOk = !(this.CompilerResults.Errors.Count > 0);
                }
                return compileOk;
            }
            catch
            {
                return compileOk;
            }
            finally
            {
                Compiler.DirectoryDeleteForce(pathDirTemp);
            }
        }
    }
}
