using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ExtractMessage
{
    class Program
    {
        static void Main(string[] args)
        {
            var ext = new List<string> { ".vb" };
            var methods = " Function | Sub ";

            var files = System.IO.Directory.GetFiles(@"D:\balboa-net\BemaNucleo\Business", "*.*", System.IO.SearchOption.AllDirectories)
                .Where(w => ext.Contains(Path.GetExtension(w)));

            foreach (string file in files)
            {
                StreamWriter sw = new StreamWriter(string.Concat(@"d:\other", @"\", Path.GetFileName(file).Replace(Path.GetExtension(file), ""), ".xml"));

                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<extension xmlns=\"urn:newrelic-extension\">\r\n\t<instrumentation>");

                Regex reg = new Regex(methods);

                var lines = File.ReadAllLines(file);
                var matches = lines.Where(w => reg.IsMatch(w));

                var spacename = lines.Where(w => w.IndexOf("namespace") > -1 || w.IndexOf("Namespace") > -1).FirstOrDefault();
                if (spacename != null)
                {
                    spacename = spacename.Replace("namespace", "")
                        .Replace("Namespace", "")
                        .Trim();
                }

                foreach (string line in matches)
                {
                    if(new string[] { "\\", "*", "-", "'" }.Contains(line.Trim().Substring(0, 1)))
                    {
                        continue;
                    }

                    var words = line.Replace("static", "").Replace("Static", "")
                        .Replace("public", "").Replace("Public", "")
                        .Replace("private", "").Replace("Private", "")
                        .Replace("Protected", "").Replace("protected", "")
                        .Replace("Override", "").Replace("override", "")
                        .Replace("Internal", "").Replace("internal", "")
                        .Replace("Friend", "").Replace("Friend", "")
                        .Replace(" Function ", "").Replace(" Function ", "")
                        .Replace(" Sub ", "").Replace(" Sub ", "")
                        .Replace("\t", "")
                        .Trim()
                        .Split(' ');                   

                    var method = words.FirstOrDefault();

                    method = method.IndexOf("(") > -1 ? method.Substring(0, method.IndexOf("(")).Trim() : method.Trim();

                    if (new string[] { "New", "Clone" }.Contains(method))
                    {
                        continue;
                    }

                    sw.WriteLine($"\t\t<!--{line}-->");
                    sw.WriteLine();

                    sw.WriteLine($"\t\t<tracerFactory >");
                    sw.WriteLine($"\t\t\t<match assemblyName=\"{ args.FirstOrDefault() }\" className=\"{ string.Concat(spacename, String.IsNullOrEmpty(spacename)?"":".", Path.GetFileName(file).Replace(Path.GetExtension(file), "")) }\">");
                    sw.WriteLine($"\t\t\t\t<exactMethodMatcher methodName=\"{ method }\" />");
                    sw.WriteLine($"\t\t\t</match>");
                    sw.WriteLine($"\t\t</tracerFactory>");
                    sw.WriteLine();

                    sw.Flush();
                }

                sw.WriteLine("\t</instrumentation>");
                sw.WriteLine("</extension>");
                sw.Dispose();
                sw = null;
            }
        }
    }
}
