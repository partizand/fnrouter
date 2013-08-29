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
using System.Text.RegularExpressions;

namespace fnrouter
{
    /// <summary>
    /// Декодирование одной строки правила
    /// </summary>
    class LineDecoder
    {

        Params Par;
        /// <summary>
        /// Декодируемая строка
        /// </summary>
        string LineStr;
        /// <summary>
        /// Количество ключей в строке
        /// </summary>
        public int NumKeys
        {
            //get { return Keys.Count; }
            get { return Words.Count; }
            
        }

        /// <summary>
        /// Список ключей
        /// </summary>
        //List<string> Keys;
        /// <summary>
        /// Список значений ключей
        /// </summary>
        //List<string> Values;


        /// <summary>
        /// Список ключей и параметров
        /// </summary>
        public Dictionary<string, string> Words;

        public LineDecoder(string Line, Params param)
        {
            Par = param;
            Words = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // Регистронезависимое сравнение
            LineStr = Line;
            DecodeLine();
        }
        /// <summary>
        /// Возвращает значение ключа по его имени (регистр имени ключа не важен)
        /// </summary>
        /// <param name="KeyName"></param>
        /// <returns></returns>
        public string GetValue(string KeyName)
        {
            if (Words.ContainsKey(KeyName)) return Words[KeyName];
            return "";
            
            //int i=Keys.FindIndex (Key=>Key.Equals(KeyName,StringComparison.CurrentCultureIgnoreCase)); // Индекс ключа
            //if (i > -1) return Values[i];
            //else return "";
           
        }

        public bool ContainsKey(string KeyName)
        {
            return Words.ContainsKey(KeyName);
        }

        /// <summary>
        /// Проверка строки на подстановку переменных. true- все ок, false - переменные не раскрыты
        /// </summary>
        public bool IsVarExpanded(out string Val)
        {
            string val;
            
            string testFile = "c:\\test\\test.txt";
            List<string> tFiles=new List<string>{"c:\\test\\test.txt","c:\\test\\test.zip","c:\\test\\test.doc"};
            foreach (string value in Words.Values)
            {
                val = Par.ReplFile(value, testFile, tFiles);
                if (Par.ContainVar(val))
                {
                    Val = val;
                    return false;
                }
            }
            Val = "";
            return true;
        }
        

        /// <summary>
        /// Заполнение массивов ключ-значение по строке, убирание комментария
        /// </summary>
        void DecodeLine()
        {
            // Обрезка комментария
            int i=LineStr.IndexOf("#");
            if (i > -1) LineStr=LineStr.Substring(0, i);
            LineStr=LineStr.Trim(); // Убираем пробелы

            

            Words.Clear();


            // Ищем строку с секцией, вроде [abc]
            Regex regexsection = new Regex("^[\\s]*\\[[\\s]*([^\\[\\s].*[^\\s\\]])[\\s]*\\][\\s]*$", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
            Match m = null;
            if (regexsection.Match(LineStr).Success)
            {
                m = regexsection.Match(LineStr);
                Words.Add("Section", m.Groups[1].Value);
                //Trace.WriteLine(string.Format("Adding section [{0}]", m.Groups[1].Value));
                //tempsection = AddSection(m.Groups[1].Value);
                return;
            }

            if (String.IsNullOrEmpty(LineStr)) return;

            int PosE, PosC;
            string KeyName, Value;
            int CurPos = 0;

            while (CurPos < LineStr.Length-1)
            {
                PosE = LineStr.IndexOf("=", CurPos); // Позиция равно
                if (PosE > -1)
                {
                    PosC = LineStr.IndexOf(";", PosE); // Позиция ';'
                    if (PosC == -1) PosC = LineStr.Length - 1; // Если нет ; то берем конец строки
                    KeyName = LineStr.Substring(CurPos, PosE-CurPos );
                    Value = LineStr.Substring(PosE + 1, PosC - PosE - 1);
                    //KeyName = KeyName.ToUpper();
                    KeyName=KeyName.Trim();
                    //Keys.Add(KeyName); //Имя ключа
                    Value = Value.Trim();
                    Value = Par.ReplStdOptions(Value); // Замена переменных %..% кроме имен файлов
                    //Values.Add(Value); // Значение ключа
                    Words[KeyName] = Value;
                    CurPos = PosC + 1;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
