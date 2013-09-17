﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace fnrouter
{
    /// <summary>
    /// Работа с файлами. Поиск нужных файлов в каталоге
    /// </summary>

    class DirInfo
    {


        //const char[] DEFAULT_SEPARATOR =  // Разделитель по умолчанию



        string IncPattern; // Список включаемых файлов - паттерн
        string ExcPattern; // Список исключаемых файлов - паттерн

        string SourceDir; // Каталог

        public char[] Separator; // Разделитель масок

        /// <summary>
        /// Файл должен содержать любую из этих строк
        /// </summary>
        string[] _Contain; // Файл содержит эту строку

        /// <summary>
        /// Файл не должен содержать ни одной из этих строк
        /// </summary>
        string[] _NOTContain;


        /// <summary>
        /// Создание объекта класса. Для одной маски вклчения допустимо ее укзать в sPath. -> DirInfo("c:\test\*","","")
        /// Если масок включения несколько то sPath только каталог -> DirInfo("c:\test","*.txt,*.csv","")
        /// Маски исключения можно задавать в любом случае. Если не заданы то не обрабатываются.
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="Include"></param>
        /// <param name="Exclude"></param>

        public DirInfo(string sPath, string Include, string Exclude)
        {
            Separator = new char[3] { ',', '|', ';' };;
            if (String.IsNullOrEmpty(Include)) // Включаемая маска одна и укзана в Path
            {
                SourceDir = Path.GetDirectoryName(sPath);
                Include = Path.GetFileName(sPath);

            }
            else
            {
                SourceDir = sPath;

            }

            SetMask(Include, Exclude);
            

        }

        public DirInfo(string sPath, string Include, string Exclude, string contain):
            this(sPath,Include,Exclude)
        {
            this._Contain = contain.Split('|');
            
        }

        /// <summary>
        /// Установить список включаемых и исключаемых масок
        /// </summary>
        /// <param name="Include"></param>
        public void SetMask(string Include, string Exclude)
        {
            //Inc = GetArray(Include);
            //Exc = GetArray(Exclude);
            IncPattern = makePattern(Include);
            ExcPattern = makePattern(Exclude);
        }


        /// <summary>
        /// Возвращает список файлов по заданным условиям
        /// </summary>
        /// <returns></returns>
        public List<string> GetFiles()
        {
            List<string> LFiles = new List<string>(); 
            if (String.IsNullOrEmpty(SourceDir)) return LFiles;
             if (!Directory.Exists(SourceDir)) return LFiles;

            

            DirectoryInfo di = new DirectoryInfo(SourceDir);
            
            
            FileInfo[] Files = di.GetFiles();

            // Перебираем все файлы в каталоге
            foreach (FileInfo fi in Files)
            {
                if (checkRange(fi.Name))
                {
                    if (IsContain(fi.FullName)) LFiles.Add(fi.FullName);
                }
            }

            return LFiles;

        }

        /// <summary>
        /// Проверяет входит ли файл в заданный диапазон include exclude
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool checkRange(string fileName)
        {
            // проверка что в Include
            bool inc = checkMask(fileName, IncPattern);
            if (!inc) return false;
            // Проверка что в Exclude
            bool exc = checkMask(fileName, ExcPattern);
            if (exc) return false;
            return true;

        }
        /// <summary>
        /// возвращает регулярное выражение для поиска RegEx из строки типа "STUD-*.xml|*.csv|*.7z|???.???"
        /// Исходные dos-овские маски переводятся в регулярные выражения. 

        /// </summary>
        /// <param name="delimS"></param>
        /// <returns></returns>
        /*
         Например, в нашем примере:

        STUD-*.xml переведется в ^STUD-.*\.xml$
        *.csv переведется в ^.*\.csv$
        *.7z переведется в ^.*\.7z$
        ???.??? переведется в ^...\....$ 
         */
        string makePattern(string delimS)
        {
            string[] exts = delimS.Split(Separator);
            string pattern = string.Empty;
            foreach (string ext in exts)
            {
                pattern += @"^";//признак начала строки
                foreach (char symbol in ext)
                    switch (symbol)
                    {
                        case '.': pattern += @"\."; break;
                        case '?': pattern += @"."; break;
                        case '*': pattern += @".*"; break;
                        default: pattern += symbol; break;
                    }
                pattern += @"$|";//признак окончания строки
            }
            if (pattern.Length == 0) return pattern;
            pattern = pattern.Remove(pattern.Length - 1);
            return pattern;
        }


        /// <summary>
        /// Проверка соответствия имени файла маске
        /// </summary>
        /// <param name="fileName">Имя проверяемого файла</param>
        /// <param name="pattern">Паттерн</param>
        /// <returns>true - файл удовлетворяет маске, иначе false</returns>
        static public bool checkMask(string fileName, string pattern)
        {

            if (String.IsNullOrEmpty(pattern)) return false;

            Regex mask = new Regex(pattern, RegexOptions.IgnoreCase);
            return mask.IsMatch(System.IO.Path.GetFileName(fileName));

        }
        /// <summary>
        /// Содержится ли файле fi строка contain, если contain="" то всегда возвращается true
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="contain"></param>
        /// <returns></returns>
        bool IsContain(string FileName)
        {
            if (this._Contain==null) return true; // Список строк пуст
            if (this._Contain.Length==0) return true;
            string Content;
            try
            {
                Content = File.ReadAllText(FileName, Encoding.GetEncoding(1251));
                foreach (string cont in this._Contain) // Ищем вхождение строк в содержимом файла
                {
                    if (Content.Contains(cont)) return true;
                }
            }
            catch
            {

            }
            
            
            
            return false;
        }
    }
}
