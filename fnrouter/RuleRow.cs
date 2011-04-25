using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fnrouter
{
    public enum TAction { Copy, Move, Send, SendMsg,RunWait,RunNoWait,UnRar };

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
        public string SourceDir;
        /// <summary>
        /// Маска файлов источника
        /// </summary>
        public string SourceMask;
        /// <summary>
        /// Исходный файл должен содержать строку
        /// </summary>
        public string Contain;
        /// <summary>
        /// Каталог приемник
        /// </summary>
        //public string Dest;
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
        public string RenDos;

        public RuleRow()
        {
            SFiles = new List<string>();
            DFiles = new List<string>();
            //Action = TAction.Nothing;
        }
    }
}
