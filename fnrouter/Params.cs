using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace fnrouter
{
    /// <summary>
    /// Чтение, предоставление настроек приложения
    /// </summary>
    class Params
    {
        /// <summary>
        /// Тип замены
        /// </summary>
        enum ReplType { All, CurDate, FileName, Option,Undefined };

        public string MailSrv;
        public string MailUser;
        public string MailPass;
        public string MailPort = "25";
        public string MailFrom;
        /// <summary>
        /// Переменные задаваемые пользователем
        /// </summary>
        public Dictionary<string, string> Options;
        /// <summary>
        /// Имена параметров для даты/времени
        /// </summary>
        public  List<string> DateOptions;
        /// <summary>
        /// Имена параметров для имен файлов
        /// </summary>
        public List<string> FileOptions;

        public Params(string iniFile)
        {
            
            FillStdOptions();
            ReadIni(iniFile);
        }

        #region Public functions

        /// <summary>
        /// Заменяет все переменные в строке кроме имен файлов
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public string ReplStdOptions(string S)
        {
            string newS;
            newS = ReplDate(S);
            newS = ReplUserOptions(newS);
            return newS;
            /*
            string param, newS, strDate;
            DateTime dtNow = DateTime.Now;
            ReplType repType;
            newS = S;
            param = GetStrVar(newS, ReplType.All);
            while (!String.IsNullOrEmpty(param))
            {

                repType = GetReplType(param);
                switch (repType)
                {
                    case ReplType.CurDate:
                        strDate = dtNow.ToString(param);
                        newS = ReplaceParam(newS, param, strDate);
                        break;
                    case ReplType.Option:
                        newS = ReplaceParam(newS, param, Options[param]);
                        break;
                }


                param = GetStrVar(newS, ReplType.CurDate);
            }
            return newS;
             * */
        }

        

        /// <summary>
        /// Заменяет параметры даты (типа %yymmdd%) в строке S
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public string ReplDate(string S)
        {
            string param, newS, strDate;
            DateTime dtNow = DateTime.Now;
            newS = S;
            param = GetStrVar(newS, ReplType.CurDate);
            while (!String.IsNullOrEmpty(param))
            {
                strDate = dtNow.ToString(param);
                //newS =  newS.Replace("%" + param + "%", strDate);
                newS = ReplaceParam(newS, param, strDate);

                param = GetStrVar(newS, ReplType.CurDate);
            }
            return newS;
        }
        /// <summary>
        /// Заменяет переменнные заданные пользователем
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public string ReplUserOptions(string S)
        {
            string param, newS;
            
            newS = S;
            param = GetStrVar(newS, ReplType.Option);
            while (!String.IsNullOrEmpty(param))
            {
                
                newS = ReplaceParam(newS, param, Options[param]);
                param = GetStrVar(newS, ReplType.Option);
            }
            return newS;
        }

        

        /// <summary>
        /// Заменяет праметры типа %file% в строке на их значения
        /// </summary>
        /// <param name="S"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public string ReplFile(string S, string FileName, List<string> SFiles)
        {
            string str, var, sValue;
            str = S;
            var = GetStrVar(str, ReplType.FileName);
            while (!String.IsNullOrEmpty(var))
            {
                if (var.Equals("ListFileName", StringComparison.CurrentCultureIgnoreCase)) // список коротких имен файлов
                {
                    sValue = GetFileListStr(SFiles, true);
                    str = str.Replace("%" + var + "%", sValue);
                }
                if (var.Equals("ListFullFileName", StringComparison.CurrentCultureIgnoreCase)) // список длинных имен файлов
                {
                    sValue = GetFileListStr(SFiles, false);
                    str = str.Replace("%" + var + "%", sValue);
                }
                if (var.Equals("FullFileName", StringComparison.CurrentCultureIgnoreCase)) // длинное имя файла
                {
                    str = str.Replace("%" + var + "%", FileName);
                }
                if (var.Equals("FileName", StringComparison.CurrentCultureIgnoreCase)) // короткое имя файла
                {
                    str = str.Replace("%" + var + "%", Path.GetFileName(FileName));
                }
                if (var.Equals("FileWithoutExt", StringComparison.CurrentCultureIgnoreCase)) // имя файла без раширения
                {
                    str = str.Replace("%" + var + "%", Path.GetFileNameWithoutExtension(FileName));
                }
                if (var.Equals("ExtFile", StringComparison.CurrentCultureIgnoreCase)) // расширение
                {
                    sValue = Path.GetExtension(FileName);
                    sValue = sValue.Replace(".", ""); // Убираем точку из раширения
                    str = str.Replace("%" + var + "%", sValue);
                }
                if (var.Equals("Nalog", StringComparison.CurrentCultureIgnoreCase)) // Налоговая, должна быть последняя в строке!
                {
                    str = str.Replace("%" + var + "%", ""); // Убираем %nalog% из пути
                    sValue = Path.GetExtension(FileName); // Расширение файла
                    if (sValue.Equals(".txt", StringComparison.CurrentCultureIgnoreCase)) // Это налоговая
                    {
                        str = GetNalogDir(str, FileName); // Получаем каталог, где лежит файл налоговой
                    }
                    if (sValue.Equals(".xml", StringComparison.CurrentCultureIgnoreCase)) // Это ФТС
                    {
                        str = GetFTSDir(str, FileName); // Получаем каталог, где лежит файл налоговой
                    }

                }
                /*
                if (var.Equals("FTS", StringComparison.CurrentCultureIgnoreCase)) // ФТС, должна быть последняя в строке!
                {
                    str = str.Replace("%" + var + "%", ""); // Убираем %fts% из пути
                    str = GetFTSDir(str, FileName); // Получаем каталог, где лежит файл ФТС
                }
                 */
                var = GetStrVar(str, ReplType.FileName);
            }
            return str;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Подстановка занчения параметра в строку. Имя параметра без %
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamValue"></param>
        /// <returns></returns>
        string ReplaceParam(string str, string ParamName, string ParamValue)
        {
           return str.Replace("%" + ParamName + "%", ParamValue);
        }
        
        private void ReadIni(string iniFile)
        {
            if (String.IsNullOrEmpty(iniFile))
            {
                iniFile = "frouter.ini";
            }
            
            
            if (!File.Exists(iniFile)) return;
            IniFile ini = new IniFile();
            ini.Load(iniFile);
            // Чтение настроек почты
            MailSrv = ini.GetKeyValue("Mail", "MailSrv");
            MailUser = ini.GetKeyValue("Mail", "MailUser");
            MailPass = ini.GetKeyValue("Mail", "MailPass");
            MailPort = ini.GetKeyValue("Mail", "MailPort");
            if (String.IsNullOrEmpty(MailPort)) MailPort = "25";
            MailFrom = ini.GetKeyValue("Mail", "MailFrom");

            // Чтение переменных
            IniFile.IniSection OptSect = ini.GetSection("Options");
            
            if (OptSect == null) return;
            foreach (IniFile.IniSection.IniKey k in OptSect.Keys)
            {
                Options.Add(k.Name, k.Value);
            }
            
            ExpandOptions();
        }
        /// <summary>
        /// Заполнение именами стандартных переменных
        /// </summary>
        void FillStdOptions()
        {
            DateOptions = new List<string> { "d", "dd" ,"ddd","dddd",
                "f","ff","fff","ffff","fffff","ffffff",
            "F","FF","FFF","FFFF","FFFFF","FFFFFF","FFFFFFF","g", "gg",
            "h", "hh",
            "H","HH",
            "K", "m","mm",
            "M","MM","MMM","MMMM",
            "s","ss","t","tt",
            "y","yy","yyy","yyyy","yyyyy",
            "z","zz","zzz",
            "yyMMdd","yyyyMM"};
            
            FileOptions=new List<string>{"ListFileName","ListFullFileName","FullFileName",
                "FileName","FileWithoutExt","ExtFile","Nalog"};
            
            Options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Options.Add("ComputerName", System.Environment.MachineName);
            Options.Add("MachineName", System.Environment.MachineName);
            Options.Add("UserName", System.Environment.UserName);

        }
        /// <summary>
        /// Возвращает тип переменной по имени (имя должно быть без %%)
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        ReplType GetReplType(string var)
        {

            if (DateOptions.Exists(obj => String.Compare(obj, var, false) == 0))
                return ReplType.CurDate;
            if (FileOptions.Exists(obj => String.Compare(obj, var, true) == 0))
                return ReplType.FileName;
            if (Options.ContainsKey(var))
                return ReplType.Option;
            return ReplType.Undefined;

        }
        /// <summary>
        /// Находит в строке S первое вхождение параметра в %% с заданным типом и возвращает этот параметр без %
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        string GetStrVar(string S, ReplType replType)
        {
            int beg = 0, end;
            string var = "";
            beg = S.IndexOf("%");

            while (beg < S.Length && beg > -1)
            {

                if (beg == -1) return "";
                end = S.IndexOf("%", beg + 1);
                if (end == -1) return "";

                var = S.Substring(beg + 1, end - beg - 1);
                if (replType == ReplType.All || GetReplType(var) == replType) return var;
                beg = S.IndexOf("%", end + 1);



            }
            return "";


        }
        
        /// <summary>
        /// Поиск каталога содержащего файл налоговой, по имени ответного файла из налоговой
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        static string GetNalogDir(string RootDir, string FileName)
        {
            // Нужно найти каталог в которм есть файл SBСxxx..xxxxxx.txt, от имени FileName
            // Т.е. берем FileName, меняем 3-й симовол на C и ищем такой файл
            string FileToFind = Path.GetFileName(FileName);
            if (FileToFind.Length != 50) return RootDir; // Какой-то не такой файл
            if (!Directory.Exists(RootDir)) return RootDir; // Каталога нет

            FileToFind = "SBC" + FileToFind.Substring(3); // Имя файла для поиска

            // Ищем файлы в каталоге RootDir и подкаталогах
            string[] files = Directory.GetFiles(RootDir, FileToFind, SearchOption.AllDirectories); // Список всех файлов 
            if (files.Length > 0) // Берем первый попавшийся
            {
                return Path.GetDirectoryName(files[0]);
            }
            return RootDir;

        }
        /// <summary>
        /// Поиск каталога содержащего файл ФТС, по имени ответного файла из ФТС
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        static string GetFTSDir(string RootDir, string FileName)
        {

            string FileToFind = Path.GetFileName(FileName);
            if (!FileToFind.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase)) return RootDir; // Какой-то не такой файл
            if (!Directory.Exists(RootDir)) return RootDir; // Каталога нет

            FileToFind = "P" + FileToFind.Substring(1); // Имя файла для поиска

            // Ищем файлы в каталоге RootDir и подкаталогах
            string[] files = Directory.GetFiles(RootDir, FileToFind, SearchOption.AllDirectories); // Список всех файлов 
            if (files.Length > 0) // Берем первый попавшийся
            {
                return Path.GetDirectoryName(files[0]);
            }
            return RootDir;

        }
        /// <summary>
        /// Возвращает строку содержащую список файлов для обработки разделенных запятой. 
        /// </summary>
        /// <param name="ShortName"></param>
        /// <returns></returns>
        public string GetFileListStr(List<string> SFiles, bool ShortName)
        {
            string sList = "";
            string sFile, delim = "";
            int i;
            for (i = 0; i < SFiles.Count; i++)
            {
                sFile = SFiles[i];
                if (ShortName) sFile = Path.GetFileName(sFile);
                if (i > 0) delim = ", ";
                sList = sList + delim + sFile;
            }
            return sList;
        }

        /*
        /// <summary>
        /// Обновляет глобальные настройки. Пустые параметры не обновляются. Обновляется только если rule=settings
        /// </summary>
        /// <param name="LDecoder"></param>
        public void UpdateGlobal(LineDecoder LDecoder)
        {
            string sValue;
            sValue = LDecoder.GetValue("Rule");
            if (!sValue.Equals("Settings", StringComparison.CurrentCultureIgnoreCase)) return;
            ReadMailSet(LDecoder, false);

        }
        */
        /// <summary>
        /// Читает из строки настройки почты и переменные пользователя
        /// </summary>
        /// <param name="LDecoder"></param>
        public void ReadLine(LineDecoder LDecoder)
        {
            ReadMailSet(LDecoder, false);
            ReadParam(LDecoder);
        }

        /// <summary>
        /// Чтение переменных пользователя в строке
        /// </summary>
        /// <param name="LDecoder"></param>
        private void ReadParam(LineDecoder LDecoder)
        {
            string[] sValue;
            foreach (KeyValuePair<string, string> kvp in LDecoder.Words)
            {
                if (kvp.Key.StartsWith("SetVar", StringComparison.CurrentCultureIgnoreCase))
                {
                    sValue = kvp.Value.Split('|');
                    if (sValue.Length > 1)
                    {
                        Options.Add(sValue[0], sValue[1]);
                    }
                }
            }
            ExpandOptions();
            
        }
        /// <summary>
        /// Заменяет в переменных на значения других переменных
        /// </summary>
        void ExpandOptions()
        {
            bool contains;
            int i=0;
            do
            {
                contains = false;
                foreach (string key in Options.Keys)
                {
                    if (Options[key].Contains("%"))
                    {
                        Options[key] = ReplUserOptions(Options[key]);
                        contains = true;
                    }
                }
                /*
                foreach (KeyValuePair<string, string> kvp in Options)
                {
                    if (kvp.Value.Contains("%"))
                    {
                        Options[kvp.Key] = ReplUserOptions(Options[kvp.Key]);
                        contains = true;
                    }
                }
                */
                i++;
            }
            while (contains && i < 20);
        }
        
        /// <summary>
        /// Чтение настроек почты
        /// </summary>
        /// <param name="LDecoder"></param>
        /// <param name="ReadEmpty"></param>
        private void ReadMailSet(LineDecoder LDecoder, bool ReadEmpty)
        {
            string sValue;
            sValue = LDecoder.GetValue("MAILSRV");
            if (ReadEmpty) this.MailSrv = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailSrv = sValue;
            }
            sValue = LDecoder.GetValue("MAILFROM");
            if (ReadEmpty) this.MailFrom = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailFrom = sValue;
            }


            sValue = LDecoder.GetValue("MAILPORT");
            if (ReadEmpty) this.MailPort = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailPort = sValue;
            }
            sValue = LDecoder.GetValue("MAILUSER");
            if (ReadEmpty) this.MailUser = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailUser = sValue;
            }
            sValue = LDecoder.GetValue("MAILPASS");
            if (ReadEmpty) this.MailPass = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailPass = sValue;
            }

        }
        

        #endregion


    }
}
