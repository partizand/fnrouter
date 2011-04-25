using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace fnrouter
{
    class Program
    {
        static string RuleName="test";
        static string ConfigFile="fnrouter.ini";
        
        static void Main(string[] args)
        {
            Console.WriteLine("fnrouter v. 0.1.0");
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
            GSettings.Param = new MParam("srv", "", "", "25", "sdfsd@sfsdf");
            FRouter router = new FRouter(ConfigFile, RuleName);
            router.DoRule();


        }

        static void ShowHelp()
        {
            Console.WriteLine("Using: fnrouter.exe [-cfile:имя_файла_с_правилами] -rule:rule_name");
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
