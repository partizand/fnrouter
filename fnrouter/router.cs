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

namespace fnrouter
{
    class FRouter
    {
        
        //RuleRow CurRule;
        string RuleName;
        string ConfigFile; // Имя файла с конфигурацией
        Logging Log;
        Params Par;

        public FRouter(string FileName, string ruleName,ref Params Param)
        {
            ConfigFile = FileName;
            RuleName = ruleName;
            Par = Param;

            Log = new Logging("Log", RuleName, LogType.Info);

            
  
        }

        public void DoRule()
        {
            RuleLine CurRule;
            string[] Lines;



            Lines = File.ReadAllLines(ConfigFile,Encoding.GetEncoding(1251)); // Читаем файл с правилами в строки
            int i = 1;
            foreach (string sLine in Lines)
            {
                Par.CurLineNum = i;
                CurRule = new RuleLine(sLine,RuleName,Log,ref Par);
                if (!CurRule.IsEmpty)
                {
                    CurRule.DoAction();
                }
                i++;
            }
        }
        

    }
}
