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

namespace fnrouter
{
    /// <summary>
    /// Декодирование одной строки правила
    /// </summary>
    class LineDecoder
    {

        Params Options;
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

        public LineDecoder(string Line, Params options)
        {
            Options = options;
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
            //Keys = new List<string>();
            //Values = new List<string>();

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
                    KeyName.Trim();
                    //Keys.Add(KeyName); //Имя ключа
                    Value = Value.Trim();
                    Value = Options.ReplStdOptions(Value); // Замена переменных %..% кроме имен файлов
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
