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
        /// <summary>
        /// Декодируемая строка
        /// </summary>
        public string LineStr;
        /// <summary>
        /// Количество ключей в строке
        /// </summary>
        public int NumKeys
        {
            get { return Keys.Count; }
            
        }

        /// <summary>
        /// Список ключей
        /// </summary>
        public List<string> Keys;
        /// <summary>
        /// Список значений ключей
        /// </summary>
        public List<string> Values;

        public LineDecoder(string Line)
        {
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
            int i=Keys.FindIndex (Key=>Key.Equals(KeyName,StringComparison.CurrentCultureIgnoreCase)); // Индекс ключа
            if (i > -1) return Values[i];
            else return "";

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

            Keys = new List<string>();
            Values = new List<string>();

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
                    Keys.Add(KeyName.Trim()); //Имя ключа
                    Values.Add(Value.Trim()); // Значение ключа
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
