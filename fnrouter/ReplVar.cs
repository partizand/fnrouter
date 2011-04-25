using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace fnrouter
{
    /// <summary>
    /// Замена переменных %% в строке.
    /// </summary>
    class ReplaceVar
    {
        /// <summary>
        /// Тип замены
        /// </summary>
        public enum ReplType { All, CurDate, FileName };

        public static string ReplAll(string S, string FileName, List<string> SFiles)
        {
            string tStr=S;
            tStr = ReplDate(tStr);
            tStr = ReplFile(tStr, FileName, SFiles);
            return tStr;
        }

        /// <summary>
        /// Заменяет параметры типа %yymmdd% в строке S
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string ReplDate(string S)
        {
            string param, newS, strDate;
            DateTime dtNow = DateTime.Now;
            newS = S;
            param = GetStrVar(newS,ReplType.CurDate);
            while (!String.IsNullOrEmpty(param))
            {
                
                
                    strDate = dtNow.ToString(param);
                    newS = newS.Replace("%" + param + "%", strDate);
                
                param = GetStrVar(newS,ReplType.CurDate);
            }
            return newS;
        }
        /// <summary>
        /// Находит в строке S первое вхождение параметра в %% с заданным типом и возвращает этот параметр без %
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        static string GetStrVar(string S,ReplType replType)
        {
            int beg=0, end;
            string var="";
            beg = S.IndexOf("%");

            while (beg<S.Length && beg>-1)
            {
            
            if (beg == -1) return "";
            end = S.IndexOf("%", beg + 1);
            if (end == -1) return "";

            var = S.Substring(beg + 1, end - beg - 1);
            if (replType==ReplType.All || GetReplType(var) == replType) return var;
            beg = S.IndexOf("%", end + 1);


            
            }
            return "";
            

        }

        /*
        Было:
         * %File% - для UnRar имя файла без расширения
        %filename% - короткое имя файла или списка файлов
        %file% - длинное имя файла или списка файлов
         * 
         * Стало:
         * 
         * %ListFileName% - список коротких имен файлов
         * %ListFullFileName% - список длинных имен файлов
         * %FullFileName% - полное имя файла
         * %FileName% - короткое имя файла
         * %FileWithoutExt% - только имя файла без расширения
         * %ExtFile% - расширение имени файла
         */
        /// <summary>
        /// Заменяет праметры типа %file% в строке на их значения
        /// </summary>
        /// <param name="S"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string ReplFile(string S, string FileName,List<string> SFiles)
        {
            string str, var, sValue;
            str = S;
            var = GetStrVar(str,ReplType.FileName);
            while (!String.IsNullOrEmpty(var))
            {
                if (var.Equals("ListFileName", StringComparison.CurrentCultureIgnoreCase)) // список коротких имен файлов
                {
                    sValue = GetFileListStr(SFiles,true);
                    str = str.Replace("%" + var + "%", sValue);
                }
                if (var.Equals("ListFullFileName", StringComparison.CurrentCultureIgnoreCase)) // список длинных имен файлов
                {
                    sValue = GetFileListStr(SFiles,false);
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


                var = GetStrVar(str,ReplType.FileName);
            }
            return str;
        }
        /// <summary>
        /// Является ли заданное имя переменной (без %%) именем для обработки имен файлов
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        static ReplType GetReplType(string var)
        {

            if (var.Equals("ListFileName", StringComparison.CurrentCultureIgnoreCase)) // список коротких имен файлов
            {
                return ReplType.FileName;
            }
            if (var.Equals("ListFullFileName", StringComparison.CurrentCultureIgnoreCase)) // список длинных имен файлов
            {
                return ReplType.FileName;
            }
            if (var.Equals("FullFileName", StringComparison.CurrentCultureIgnoreCase)) // длинное имя файла
            {
                return ReplType.FileName;
            }
            if (var.Equals("FileName", StringComparison.CurrentCultureIgnoreCase)) // короткое имя файла
            {
                return ReplType.FileName;
            }
            if (var.Equals("FileWithoutExt", StringComparison.CurrentCultureIgnoreCase)) // имя файла без раширения
            {
                return ReplType.FileName;
            }
            if (var.Equals("ExtFile", StringComparison.CurrentCultureIgnoreCase)) // расширение
            {
                return ReplType.FileName;
            }
            return ReplType.CurDate;

        }
        /// <summary>
        /// Возвращает строку содержащую список файлов для обработки разделенных запятой. 
        /// </summary>
        /// <param name="ShortName"></param>
        /// <returns></returns>
        public static string GetFileListStr(List<string> SFiles, bool ShortName)
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
    }
}
