using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
namespace Roo.CompileAssembly
{
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
                    var allDirs = System.IO.Directory.GetDirectories(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                    if (allDirs != null && allDirs.Any())
                    {
                        foreach (var pathDir in allDirs)
                            try { System.IO.Directory.Delete(pathDir); } catch { }
                    }
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
        /// <summary>
        /// Tìm version File của tập tin (FileVersionInfo.GetVersionInfo(pathFile)), Exception trả về rỗng
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        public static string GetVersionStringFile(string pathFile)
        {
            try
            {
                if (System.IO.File.Exists(pathFile) == false) return "";
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(pathFile).FileVersion;
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// So sánh 2 Version dạng string
        /// 1.So sánh dạng chuỗi, nếu đúng trả về True
        /// 2.Kiểm tra nếu có chứa ký tự . thì kiểm tra tiếp, ngược lại trả về False
        /// 3.Cắt chuỗi bởi ký tự .
        /// 4.So sánh từng thành phần cắt ra, nếu so sánh chuỗi giống nhau thì được, ngược lại convert sang dạng số để so sánh, Exception convert số thì là False
        /// </summary>
        /// <param name="version1"></param>
        /// <param name="version2"></param>
        /// <returns></returns>
        public static bool CompareStringVersion(string version1, string version2)
        {
            var f_version1 = version1 + "";
            var f_version2 = version2 + "";
            if (f_version1 == f_version2)
                return true;
            if (f_version1.Contains(".") == false || f_version2.Contains(".") == false)
                return false;
            var split_version1 = f_version1.Split('.');
            var split_version2 = f_version2.Split('.');
            if (split_version1.Length != split_version2.Length)
                return false;
            bool compareItemSplit = true;
            for (int i = 0; i < split_version1.Length; i++)
            {
                if (split_version1[i] == split_version2[i])
                {
                    compareItemSplit = true;
                }
                else
                {
                    try { compareItemSplit = (Convert.ToInt16(split_version1[i]) == Convert.ToInt16(split_version2[i])); }
                    catch
                    {
                        compareItemSplit = false;
                    }
                }
                if (compareItemSplit == false)
                    break;
            }
            return compareItemSplit;
        }
    }
}
