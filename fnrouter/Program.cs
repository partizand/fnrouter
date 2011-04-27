using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
//using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;


namespace fnrouter
{
    class Program
    {
        static string RuleName;//="test";
        static string ConfigFile="fnrouter.ini";

        
        private static bool isNew;
        private static string guid;
        private static Mutex _mutex;

        static void Main(string[] args)
        {
            // Номер сборки
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            Version ver=curAssembly.GetName().Version;
            ver.ToString();
            Console.WriteLine("fnrouter v. "+ver.ToString());
            
            ReadArgs(args);

            if (!File.Exists(ConfigFile))
            {
                Console.WriteLine("Не найден файл с правилами: "+ConfigFile);
                return;
            }
            if (String.IsNullOrEmpty(RuleName)) // Не задано правило
            {
                ShowHelp();
                return;
            }

            using (Process currentProcess = Process.GetCurrentProcess())
            {
                guid = string.Format("[{0}][{1}]", currentProcess.ProcessName, RuleName);
            }

            if (_mutex == null)
                _mutex = new Mutex(true, guid, out isNew);

            if (!isNew)
            {
                Console.WriteLine("Копия программы с таким правилом уже запущена: " + RuleName);
                return;
            }

            //Console.WriteLine("Debug: Mutex guid:" + guid);

            Console.WriteLine("Запуск правила " + RuleName+" из файла "+ConfigFile);

            GSettings.Param = new MParam("srv", "", "", "25", "sdfsd@sfsdf");
            FRouter router = new FRouter(ConfigFile, RuleName);
            router.DoRule();


        }

        static void ShowHelp()
        {
            Console.WriteLine("Using: fnrouter.exe [-cfile:имя_файла_с_правилами] -rule:имя_правила");
            Console.WriteLine("Имя файла с правилами по умолчанию fnrouter.ini");
        }

        /// <summary>
        /// Чтение параметров командной строки
        /// </summary>
        /// <param name="args"></param>
        static void ReadArgs(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-cfile:", true, null)) // Указан файл с правилами
                {
                    ConfigFile = GetArgValue(arg);
                }
                if (arg.StartsWith("-rule:", true, null)) // Имя правила
                {
                    RuleName = GetArgValue(arg);
                }

            }
        }
        /// <summary>
        /// Возвращает значение праметра запуска программы, т.е. все после ":" в строке Arg (-cfile:d:\file.exe -> d:\file.exe)
        /// </summary>
        /// <param name="Arg"></param>
        /// <returns></returns>
        static string GetArgValue(string Arg)
        {
            int i = Arg.IndexOf(":");
            return Arg.Substring(i + 1);

        }
    }
}
