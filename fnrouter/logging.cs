using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace fnrouter
{
    #region Типы сообщений логов
    /// <summary>
    /// Уровни логирования, типы сообщений
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Ничего не логировать
        /// </summary>
        Nothing=0, 
        /// <summary>
        /// Ошибка
        /// </summary>
        Error=10,
        /// <summary>
        /// Предупреждение
        /// </summary>
        Warning=20,
        /// <summary>
        /// Инфо
        /// </summary>
        Info=30
    }

    #endregion


    /// <summary>
    /// Класс ведения логов в файл
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// Каталог с логами
        /// </summary>
        public string LogPath;
        /// <summary>
        /// Имя подсистемы для логгирования
        /// </summary>
        public string LogSubsys;
        /// <summary>
        /// Уровень логирования
        /// </summary>
        public LogType LogLevel; 
        

        public Logging(string FolderLog, string Logsubsys, LogType Loglevel)
        {
            string RootFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]); // Каталог запуска программы
            //RootFolder = Path.Combine(RootFolder, "Log");
            this.LogPath =Path.Combine(RootFolder,FolderLog);
            this.LogSubsys = Logsubsys;
            this.LogLevel = Loglevel;
        }
        public Logging()
        {
            string RootFolder; // Каталог запуска программы
            RootFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            
            this.LogPath = Path.Combine(RootFolder, "Log");
            this.LogSubsys = "";
            this.LogLevel = LogType.Nothing;
        }
        /// <summary>
        /// Запись сообщения в лог с добавлением даты, без указания уровня
        /// </summary>
        /// <param name="Msg"></param>
        /// <returns>Msg с датой. null если уровень - не логировать</returns>
        public string LogMessage(string Msg)
        {
            if (LogLevel == LogType.Nothing) return null;
            return DoLog(Msg);

        }
        /// <summary>
        /// Запись сообщения в лог с добавлением даты, с указанием уровня. Логируется только заданный уровень
        /// </summary>
        /// <param name="logtype"></param>
        /// <param name="Msg"></param>
        /// <returns>Строка Msg с датой. Пусто если уровень логирования не соответсвует</returns>
        
        public string LogMessage(LogType logtype, string Msg)
        {
            if (LogLevel == LogType.Nothing) return null;
            if (LogLevel < logtype) return null;
            return DoLog(Msg);

        }
        string DoLog(string Msg)
        {
            lock (this)
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                DateTime dt = DateTime.Now;
                string LogFile = LogPath + Path.DirectorySeparatorChar + dt.ToString("yyMMdd-") + LogSubsys + ".txt";
                FileStream fs = new FileStream(LogFile, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding(1251));
                if (Msg == "-") Msg = "---------------------------------------------------------";
                else Msg = dt.ToString("dd.MM.yy HH:mm:ss") + " " + Msg;
                sw.WriteLine(Msg);
                sw.Close();
                fs.Close();
                Console.WriteLine(Msg);
                return Msg;
            }
        }
    }
}
