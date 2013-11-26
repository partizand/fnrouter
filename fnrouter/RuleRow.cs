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
    public enum TAction { Copy, Move, Send, SendMsg, RunWait, RunNoWait, UnRar, MoveNalogDir, MoveFTSDir, UnArj, UnCab, Delete, PbGen, MergeNalogFile, Nothing};

    class RuleRow
    {
        /// <summary>
        /// Имя правила
        /// </summary>
        public string RuleName;
        /// <summary>
        /// Каталог источник вместе с маской
        /// </summary>
        public string Source;
        /// <summary>
        /// Каталог источника, без маски
        /// </summary>
        //public string SourceDir;
        /// <summary>
        /// Маска файлов источника
        /// </summary>
        //public string SourceMask;
        /// <summary>
        /// Исходный файл должен содержать строки, разделенные |
        /// </summary>
        public string Contain;
        /// <summary>
        /// Исходный файл не должен содержать строки, разделенные |
        /// </summary>
        public string NOTContain;
        /// <summary>
        /// Каталог приемник
        /// </summary>
        public string Dest;
        /// <summary>
        /// Список исходных файлов вместе с путями
        /// </summary>
        //public FileInfo[] SFiles;
        public List<string> SFiles;
        /// <summary>
        /// Список конечных файлов вместе с путями
        /// </summary>
        public List<string> DFiles;
        /// <summary>
        /// Действие
        /// </summary>
        public TAction Action;
        /// <summary>
        /// Правило переименования файлов
        /// </summary>
        //public string RenDos;
        /// <summary>
        /// Правило переименования файлов
        /// </summary>
        public string Ren;

        public RuleRow()
        {
            SFiles = new List<string>();
            DFiles = new List<string>();
            //Action = TAction.Nothing;
        }
    }
}
