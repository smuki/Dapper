namespace Volte.Data.Dapper
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.IO;
    using System.Threading;

    public class DapperCompiler {
        const string ZFILE_NAME = "DapperCompiler";
        public static object _PENDING = new object();
        public static string APP_PATH = "";

        public static CompilerResults Compile(string code, CompilerParameters cp)
        {

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");


            return provider.CompileAssemblyFromSource(cp, new string[] { code });
        }

        public static Assembly StartCompile(string code, StringCollection referencedAssemblies, string entitykey)
        {
            CompilerResults result = Compile(code, referencedAssemblies, entitykey, "");

            string error = null;

            if (result.Errors.Count > 0) {
                for (int i = 0; i < result.Errors.Count; i++) {
                    error = error + "\r\n" + result.Errors[i];
                }

                ZZLogger.Debug(ZFILE_NAME , code);
                ZZLogger.Debug(ZFILE_NAME , error);

                throw new DataException(error + "\r\n如数据库中有字段的增删改变动,请重新生成实体集:)");
            }

            return result.CompiledAssembly;
        }

        public static CompilerResults Compile(string code, StringCollection referencedAssemblies, string entitykey, string fileName)
        {

            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

            string Url = path;

            if (path.IndexOf("file:\\") >= 0) {
                Url = Url.Replace("file:\\", "");
            }

            CompilerParameters cp = new CompilerParameters {
                GenerateExecutable = false,
                GenerateInMemory = true,
                TreatWarningsAsErrors = false //,
            };

            if (fileName != "") {
                cp.OutputAssembly = fileName;
            }

            foreach (string referencedAssemblie in referencedAssemblies) {
                ZZLogger.Debug(ZFILE_NAME, referencedAssemblie);
                cp.ReferencedAssemblies.Add(referencedAssemblie);
            }

            CompilerResults result = Compile(code, cp);

            return result;
        }

    }
}
