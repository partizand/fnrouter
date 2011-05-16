using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;

namespace fnrouter
{
    /// <summary>
    /// Одна строка правила
    /// </summary>
    class RuleLine
    {
        
        

        public LineDecoder LDecoder;

        public RuleRow Rule;

        Logging Log;
        /// <summary>
        /// Настройки для текущего правила
        /// </summary>
        MParam LocalParam;

        /// <summary>
        /// Правило пусто, выполнять нечего
        /// </summary>
        public bool IsEmpty
        {
            get { return _isEmpty; }
        }
        private bool _isEmpty;

        public RuleLine(string Linestr,string ruleName,Logging log)
        {
            LDecoder = new LineDecoder(Linestr); // Декодируем строку
            Rule = new RuleRow();
            _isEmpty = false;
            Log = log;
            // Разбор строки
            if (LDecoder.NumKeys == 0) // Строка пуста
            {
                SetVoid();
                return;
            }
            string sValue;
            // Имя правила
            sValue = LDecoder.GetValue("RULE");
            //sValue = sValue.ToUpper();
            if (String.IsNullOrEmpty(sValue)) // Имя правила пусто
            {
                SetVoid();
                return;
            }
            Rule.RuleName = sValue;
            GSettings.Param.UpdateGlobal(LDecoder); // Обновляем глобальные настройки (Если это правило settings)


            if (!Rule.RuleName.Equals(ruleName, StringComparison.CurrentCultureIgnoreCase)) // Имя правила не совпадает
            {
                SetVoid();
                return;
            }
            
            LocalParam = new MParam(LDecoder); // Читаем локальные настройки строки если есть
            LocalParam.CoverGlobal(GSettings.Param); // Перекрываем глобальными

            sValue = LDecoder.GetValue("RENDOS");
            sValue = sValue.ToUpper();
            Rule.RenDos = sValue;

            sValue = LDecoder.GetValue("ACT");
            sValue = sValue.ToUpper();
            switch (sValue)
            {
                case "COPY":
                    Rule.Action = TAction.Copy;
                    break;
                case "MOVE":
                    Rule.Action = TAction.Move;
                    break;
                case "SEND":
                    Rule.Action = TAction.Send;
                    break;
                case "SENDMSG":
                    Rule.Action = TAction.SendMsg;
                    break;
                case "RUNWAIT":
                    Rule.Action = TAction.RunWait;
                    break;
                case "RUNNOWAIT":
                    Rule.Action = TAction.RunNoWait;
                    break;
                case "UNRAR":
                    Rule.Action = TAction.UnRar;
                    break;
                case "MOVENALOGDIR":
                    Rule.Action = TAction.MoveNalogDir;
                    break;
                default:
                    SetVoid();
                    return;
                    //break;
            }
            FillFiles(); // Читаем список файлов
            
        }
        /// <summary>
        /// Выполнение действия строки правила
        /// </summary>
        public void DoAction()
        {
            if (_isEmpty) return;
            switch (Rule.Action)
            {
                case TAction.Copy:
                    ActCopy();
                    break;
                case TAction.Move:
                    ActCopy();
                    break;
                case TAction.Send:
                    ActSend();
                    break;
                case TAction.SendMsg:
                    ActSend();
                    break;
                case TAction.RunWait:
                    ActRun();
                    break;
                case TAction.RunNoWait:
                    ActRun();
                    break;
                case TAction.UnRar:
                    ActUnRar();
                    break;
                case TAction.MoveNalogDir:
                    ActMoveNalogDir();
                    break;

            }
        }

        void ActRun()
        {
            if (_isEmpty) return;
            if (Rule.SFiles.Count == 0) return;

            bool WaitForExit = true; // Ждать или не ждать
            if (Rule.Action == TAction.RunNoWait) WaitForExit = false;

            string cmd = LDecoder.GetValue("Cmd");
            if (String.IsNullOrEmpty(cmd)) return;
            string args = LDecoder.GetValue("Arg");
            args = ReplaceVar.ReplDate(args);

            //cmd = GetFullFileName(cmd);

            if (!File.Exists(cmd))
            {
                Log.LogMessage(LogType.Error, "Не найден файл для запуска "+cmd);
                return;
            }

            if (args.Contains("%")) // Запуск на каждый из файлов
            {
                string tArg;
                foreach (string sfile in Rule.SFiles)
                {
                    tArg = ReplaceVar.ReplFile(args, sfile,Rule.SFiles);
                    Exec(cmd, tArg, WaitForExit);
                }
            }
            else // Запуск один на все файлы
            {
                Exec(cmd, args, WaitForExit);
            }

            
        }

        /// <summary>
        /// Распаковка UnRar
        /// </summary>
        void ActUnRar()
        {
            string cmd = "rar.exe";
            int i;
            string RarFile,tDest,args;
            for (i=0;i<Rule.SFiles.Count;i++) // Перебираем исходные файлы
            {
                RarFile=Rule.SFiles[i]; // Имя архива
                tDest=Path.GetDirectoryName(Rule.DFiles[i]); // Каталог приемник
                tDest = tDest + Path.DirectorySeparatorChar;
                args=" e -y \""+RarFile+"\" \""+tDest+"\"";
                Exec(cmd,args,true);

            }
        }

        void ActMoveNalogDir()
        {
            string DirTo,ShortDir;
            // В исходном каталоге ищем подкаталоги по заданной маске и содержащие полный комплект ответов
            string[] FDirs = Directory.GetDirectories(Rule.SourceDir, Rule.SourceMask); // Каталоги для поиска полных комплектов
            foreach (string FDir in FDirs)
            {
                if (IsNalogDirFin(FDir)) // Каталог завершен, его нужно переместить в Dest
                {
                    ShortDir=Path.GetFileName(FDir); // Возможно неправильно! нужно имя каталога на конце строки
                    if (ShortDir.StartsWith("!")) ShortDir=ShortDir.Substring(1); // Убираем "!"
                    DirTo=Path.Combine(Rule.Dest,ShortDir);
                    // Создание каталога приемника если нужно
                    DirectoryCreateEx(Rule.Dest, Log);
                    if (DirMoveEx(FDir, DirTo, Log))
                    {
                        Log.LogMessage(LogType.Info, "Перемещен каталог " + FDir + " в " + DirTo);
                    }
                }
            }
        }
        /// <summary>
        /// Возвращает true если каталог Dir содержит полный комплект ответов из налоговой
        /// </summary>
        /// <param name="Dir"></param>
        /// <returns></returns>
        bool IsNalogDirFin(string Dir)
        {
            string[] SendFiles;
            bool IsAnsw;
            SendFiles = Directory.GetFiles(Dir, "sbc*.txt"); // Файлы отправляемые в налоговую
            if (SendFiles.Length < 1) return false; // Отправляемых файлов нет
            // К кажому sbc файлу должен быть sbf,sbp и sbr файл
            foreach (string SFile in SendFiles)
            {
                IsAnsw = IsNalogAnswerExists(SFile, "SBF");
                if (!IsAnsw) return false;
                IsAnsw = IsNalogAnswerExists(SFile, "SBP");
                if (!IsAnsw) return false;
                IsAnsw = IsNalogAnswerExists(SFile, "SBR");
                if (!IsAnsw) return false;
            }
            return true;
        }
        /// <summary>
        /// Возвращает существует ли ответ из налоговой на файл SourceFile с префиксом Prefix в том же каталоге
        /// </summary>
        /// <param name="SourceFile">Полный путь к файлу SBC</param>
        /// <param name="Prefix">Префикс ответа SBF,SBP,SBR</param>
        /// <returns></returns>
        bool IsNalogAnswerExists(string SourceFile, string Prefix)
        {
            string Dir=Path.GetDirectoryName(SourceFile);
            string shortFindFile = Path.GetFileName(SourceFile);
            string FindFile = Prefix + shortFindFile.Substring(3);
            FindFile = Path.Combine(Dir, FindFile);
            return File.Exists(FindFile);
        }

        /// <summary>
        /// Возвращает полное имя файла из короткого добавлением каталога программы или возращает неизменным если путь и так полный
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        string GetFullFileName(string FileName)
        {
            string FullName = FileName;
            if (!Path.IsPathRooted(FileName)) 
            {
                string RootFolder; // Каталог запуска программы
                RootFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                FullName = Path.Combine(RootFolder, FileName);

            }
            return FullName;
        }
        /// <summary>
        /// Запуск внешнего файла
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="Arg"></param>
        /// <param name="WaitForExit"></param>
        void Exec(string Cmd, string Arg, bool WaitForExit)
        {
            if (!File.Exists(Cmd))
            {
                Cmd = GetFullFileName(Cmd);
                if (!File.Exists(Cmd))
                {
                    Log.LogMessage(LogType.Error, "Не найден файл для запуска " + Cmd);
                    return;
                }
            }


            
            System.Diagnostics.Process pr = new System.Diagnostics.Process();

            pr.StartInfo.FileName = Cmd;
            pr.StartInfo.Arguments = Arg;

            Log.LogMessage(LogType.Error, "Запуск " + Cmd + " " + Arg);

            if (!pr.Start())  // Ошибка запуска
            {
                Log.LogMessage(LogType.Error, "Ошибка запуска " + Cmd + " " + pr.StartInfo.Arguments);
                return;

            }
            if (WaitForExit)
            {
                pr.WaitForExit(); // Ждем завершения до бесконечности
                pr.Dispose();
                pr.Close();
            }
        }

        /// <summary>
        /// Копирование и перемещение файлов
        /// </summary>
        void ActCopy()
        {
            if (_isEmpty) return;
            if (Rule.SFiles.Count != Rule.DFiles.Count) return; // количество файлов не совпадает
            int i;
            for (i = 0; i < Rule.SFiles.Count; i++)
            {
                // Создание каталога приемника если нужно

                DirectoryCreateEx(Path.GetDirectoryName(Rule.DFiles[i]), Log);

                switch (Rule.Action)
                {
                    case TAction.Copy:
                        if (FileCopyEx(Rule.SFiles[i], Rule.DFiles[i], Log))
                        {
                            Log.LogMessage(LogType.Info, "Скопирован файл " + Rule.SFiles[i] + " в " + Rule.DFiles[i]);
                        }
                        break;
                    case TAction.Move:
                        if (FileMoveEx(Rule.SFiles[i], Rule.DFiles[i], Log))
                        {
                            Log.LogMessage(LogType.Info, "Перемещен файл " + Rule.SFiles[i] + " в " + Rule.DFiles[i]);
                        }
                        break;
                }
                
            }
        }
        /// <summary>
        /// Отправка файлов и уведомлений файлов на почту
        /// </summary>
        void ActSend()
        {
            if (_isEmpty) return;
            if (Rule.SFiles.Count == 0) return; // Файлов нет
            string MailTo, Subj, Msg,tSubj,tMsg;
            MailTo = LDecoder.GetValue("to");
            if (String.IsNullOrEmpty(MailTo)) return;
            Subj = LDecoder.GetValue("Subj");// Надо заменить имена файлов
            Msg = LDecoder.GetValue("Text"); // Надо заменить имена файлов
            if (Rule.Action == TAction.Send) // Отправка файлов
            {
                foreach (string sfile in Rule.SFiles)
                {
                    tSubj = ReplaceVar.ReplFile(Subj, sfile, Rule.SFiles);
                    tMsg = ReplaceVar.ReplFile(Msg, sfile, Rule.SFiles);
                    if (SendMail(MailTo, tSubj, tMsg, sfile))
                    {
                        Log.LogMessage(LogType.Info, "Файл отправлен по почте " + sfile + " для " + MailTo);
                    }
                }
            }
            if (Rule.Action == TAction.SendMsg) // Отправка сообщения о файлах
            {
                Subj = ReplaceVar.ReplFile(Subj, "", Rule.SFiles);
                Msg = ReplaceVar.ReplFile(Msg, "", Rule.SFiles);
                if (SendMail(MailTo, Subj, Msg, ""))
                {
                    Log.LogMessage(LogType.Info, "Сообщение о файле(ах) отправлено по почте " + ReplaceVar.GetFileListStr(Rule.SFiles, true) + " для " + MailTo);
                }
            }

        }
        

        

        /// <summary>
        /// Отправка письма с вложением
        /// </summary>
        /// <param name="mparam"></param>
        /// <param name="MailTo"></param>
        /// <param name="Subj"></param>
        /// <param name="Mess"></param>
        /// <param name="FileName"></param>
        bool SendMail(string MailTo, string Subj, string Msg, string FileName)
        {
            int tport,port=25;
            //Формирование письма
            MailMessage Message = new MailMessage();
            Attachment att=null; // Вложение
            Message.From = new MailAddress(LocalParam.MailFrom);
            Message.To.Add(MailTo);
            Message.Subject = Subj;
            Message.IsBodyHtml = false;
            Message.Body = Msg;
            // Вложение если есть
            if (!String.IsNullOrEmpty(FileName))
            {
                if (File.Exists(FileName))
                {
                    att = new Attachment(FileName);
                    Message.Attachments.Add(att);
                }
            }
            //Авторизация на SMTP сервере
            if (Int32.TryParse(LocalParam.MailPort, out tport))
            {
                port = tport;
            }
            try
            {
                SmtpClient Smtp = new SmtpClient(LocalParam.MailSrv, port);
                if (!String.IsNullOrEmpty(LocalParam.MailPass)) Smtp.Credentials = new NetworkCredential(LocalParam.MailUser, LocalParam.MailPass);
                Smtp.Send(Message);//отправка
                if (att != null) att.Dispose(); // Освобждение файла во вложении
                Message.Dispose(); // Освобождение сообщения
                return true;
            }
            catch (Exception E)
            {
                Log.LogMessage(LogType.Error, "Ошибка отправки на почту. " + E.Message);
                return false;
            }
        }
        
        
        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// Создание каталога с отловом исключения и записью в лог, если ошибка. Log может быть null, тогда лога нет
        /// </summary>
        /// <param name="DirName"></param>
        /// <param name="Log"></param>
        /// <returns></returns>
        public static bool DirectoryCreateEx(string DirName, Logging Log)
        {
            if (Directory.Exists(DirName)) return true;
            try
            {
                Directory.CreateDirectory(DirName);
                return true;
            }
            catch (Exception E)
            {

                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка создания каталога " + DirName + ". " + E.Message);
                return false;
            }

        }
        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// Копирование файла с отловом исключения и записью в лог, если ошибка. Log может быть null, тогда лога нет
        /// </summary>
        /// <param name="FileNameFrom"></param>
        /// <param name="FileNameTo"></param>
        /// <param name="Log">Может быть null</param>
        /// <returns></returns>
        public static bool FileCopyEx(string FileNameFrom, string FileNameTo, Logging Log)
        {
            try
            {
                File.Copy(FileNameFrom, FileNameTo,true);
                return true;
            }
            catch (Exception E)
            {

                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка копирования файла " + FileNameFrom + " в " + FileNameTo + ". " + E.Message);
                return false;
            }

        }
        /// <summary>
        /// Перемещение файла с отловом исключения и записью в лог, если ошибка. Log может быть null, тогда лога нет
        /// </summary>
        /// <param name="FileNameFrom"></param>
        /// <param name="FileNameTo"></param>
        /// <param name="Log"></param>
        /// <returns></returns>
        public static bool FileMoveEx(string FileNameFrom, string FileNameTo, Logging Log)
        {
            string NewFileNameTo = FileNameTo;
            try
            {
                NewFileNameTo = UniqFileName(FileNameTo);
                File.Move(FileNameFrom, NewFileNameTo);
                return true;
            }
            catch (Exception E)
            {
                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка перемещения файла " + FileNameFrom + " в " + FileNameTo + ". " + E.Message);
                return false;
            }

        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Получение не существующего имени файла для каталога
        /// </summary>
        /// <param name="FileName">Путь и имя файла</param>
        /// <returns>Имя файла, которое не занято</returns>

        public static string UniqFileName(string FileName)
        {
            string UFileName = FileName;
            string Ext = Path.GetExtension(FileName); // Расширение файла
            if (String.IsNullOrEmpty(Ext))// Расширения нет
            {
                while (File.Exists(UFileName))
                {
                    UFileName += ".1";
                }
                return UFileName;
            }
            else // Расширение есть
            {
                string FNameWithoutExt = Path.GetFileNameWithoutExtension(FileName);
                string FileDir = Path.GetDirectoryName(FileName);
                // Получение уникального имени для файла в каталоге

                while (File.Exists(UFileName))
                {
                    FNameWithoutExt += ".1";
                    UFileName = Path.Combine(FileDir, FNameWithoutExt + Ext);
                    //UFileName += ".1";
                }
                return UFileName;
            }

        }

        /// <summary>
        /// Перемещение каталога с отловом исключения и записью в лог, если ошибка. Log может быть null, тогда лога нет
        /// </summary>
        /// <param name="FileNameFrom"></param>
        /// <param name="FileNameTo"></param>
        /// <param name="Log"></param>
        /// <returns></returns>
        public static bool DirMoveEx(string DirNameFrom, string DirNameTo, Logging Log)
        {
            try
            {
                Directory.Move(DirNameFrom, DirNameTo);
                return true;
            }
            catch (Exception E)
            {
                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка перемещения каталога " + DirNameFrom + " в " + DirNameTo + ". " + E.Message);
                return false;
            }

        }
        /// <summary>
        /// Удаление файла с отловом исключения и записью в лог, если ошибка. Log может быть null, тогда лога нет
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Log"></param>
        /// <returns></returns>
        public static bool FileDeleteEx(string FileName, Logging Log)
        {
            try
            {
                File.Delete(FileName);
                return true;
            }
            catch (Exception E)
            {
                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка удаления файла " + FileName + ". " + E.Message);
                return false;
            }

        }

        /// <summary>
        /// Устанавливает пустое действие
        /// </summary>
        void SetVoid()
        {
            _isEmpty = true;

            //Rule.Action = TAction.Nothing;
        }
        
        /// <summary>
        /// Заполнение списков файлов для обработки
        /// </summary>
        void FillFiles()
        {
            if (!_isEmpty)
            {
                FillSFiles();
                FillDFiles();
            }
        }
        
        /// <summary>
        /// Заполнение списка ИСХОДНЫХ файлов для обработки
        /// </summary>
        void FillSFiles()
        {
            
            Rule.Source = LDecoder.GetValue("S");
            Rule.Source = ReplaceVar.ReplDate(Rule.Source); // Подстановка текущих даты времени
            Rule.Contain = LDecoder.GetValue("CONTAIN");
            if (String.IsNullOrEmpty(Rule.Source)) return;
            Rule.SourceDir = Path.GetDirectoryName(Rule.Source);
            if (!Directory.Exists(Rule.SourceDir)) return;
            Rule.SourceMask = Path.GetFileName(Rule.Source);

            if (Rule.Action == TAction.MoveNalogDir) return; // Перемещение каталога налоговой, файлов нет

            DirectoryInfo di = new DirectoryInfo(Rule.SourceDir);
            FileInfo[] Files = di.GetFiles(Rule.SourceMask);
            
            foreach (FileInfo fi in Files) // Проходим по всем файлам, выбираем те которые содержат строку contain
                {
                    if (IsContain(fi.FullName, Rule.Contain))
                    {
                        Rule.SFiles.Add(fi.FullName);
                    }
                }
            
            
        }
        /// <summary>
        /// Заполнение списка конечных файлов по исходным и ключу D
        /// </summary>
        void FillDFiles()
        {
            string tDest;

            Rule.Dest = LDecoder.GetValue("D");
            Rule.Dest = ReplaceVar.ReplDate(Rule.Dest); // Замена даты
            if (String.IsNullOrEmpty(Rule.Dest)) return; // не указан приемник
            if (Rule.Action == TAction.MoveNalogDir) return; // Перемещение каталога налоговой, файлов нет
            string shortFileName;
            foreach (string FullSFile in Rule.SFiles) // перебираем все исходные файлы
            {
                shortFileName = GetRenFileName(FullSFile); // Переименование если нужно
                tDest = ReplaceVar.ReplFile(Rule.Dest, FullSFile, Rule.SFiles); // Замена %file% в имени каталога приемника
                Rule.DFiles.Add(Path.Combine(tDest,shortFileName));
            }

            
        }
        /// <summary>
        /// Возвращает переимнованное имя файла согласно заданным правилам, если правил нет возвращает имя без имзенений
        /// Возвращается всегда короткое имя файла
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        string GetRenFileName(string FileName)
        {
            string RenFile = Path.GetFileName(FileName);
            if (Rule.RenDos.Equals("RightLeft", StringComparison.CurrentCultureIgnoreCase))
            {
                string Ext, onlyName;
                int len;
                Ext = Path.GetExtension(FileName);
                onlyName = Path.GetFileNameWithoutExtension(FileName);
                if (Ext.Length > 4) // Обрезаем расширение, если оно длиннее 3 символов
                {
                    Ext = Ext.Substring(0, 4);
                }
                len=onlyName.Length;
                if (len>8)
                {
                    onlyName = onlyName.Substring(len - 8, 8);
                }
                RenFile = onlyName + Ext;
            }
            return RenFile;
        }

        /// <summary>
        /// Содержится ли файле fi строка contain, если contain="" то всегда возвращается true
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="contain"></param>
        /// <returns></returns>
        bool IsContain(string FileName, string contain)
        {
            string Content="";
            try
            {
                Content = File.ReadAllText(FileName,Encoding.GetEncoding(1251));
            }
            catch
            {
                
            }
            return Content.Contains(contain);
        }
        
        


    }
}
