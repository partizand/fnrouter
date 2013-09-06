/*

 * Copyright 2011 Kapustin Andrey

 * This file is part of Fnrouter.

 * Fnrouter is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.


 * Fnrouter is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.


 * You should have received a copy of the GNU General Public License
 * along with Fnrouter.  If not, see <http://www.gnu.org/licenses/>.


 * Код распространяется по лицензии GPL 3
 * Автор: Капустин Андрей, 2011 г.
*/


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

        static bool debug=false;

        static Params Par;

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
            if (String.IsNullOrEmpty(RuleName) && !debug) // Не задано правило
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

            if (!debug) Console.WriteLine("Запуск правила " + RuleName+" из файла "+ConfigFile);
            else Console.WriteLine("Проверка файла " + ConfigFile);

            string ProgDir=Path.GetDirectoryName(Environment.CommandLine); // По умолчанию в каталоге с программой
            ConfigFile = Path.Combine(ProgDir, ConfigFile);

            Par = new Params(ConfigFile);
            Par.Debug = debug;
            //GSettings.Param = new MParam("srv", "", "", "25", "sdfsd@sfsdf");
            FRouter router = new FRouter(ConfigFile, RuleName,ref Par);
            router.DoRule();


        }

        static void ShowHelp()
        {
            Console.WriteLine("Using: fnrouter.exe [-cfile:имя_файла_с_правилами] -rule:имя_правила [-check]");
            Console.WriteLine("Имя файла с правилами по умолчанию fnrouter.ini");
            Console.WriteLine("-check Проверка подстановки переменных %% в файле");
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
                if (arg.Equals("-check", StringComparison.CurrentCultureIgnoreCase))
                {
                    debug = true; 
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
