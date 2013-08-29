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
        //MParam Options;

        Params Par;

        /// <summary>
        /// Правило пусто, выполнять нечего
        /// </summary>
        public bool IsEmpty
        {
            get { return _isEmpty; }
        }
        private bool _isEmpty;

        public RuleLine(string Linestr,string ruleName,Logging log, ref Params param)
        {
            Par = param;
            LDecoder = new LineDecoder(Linestr,Par); // Декодируем строку
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

            
            if (LDecoder.ContainsKey("Section")) // Это начало новой секции
            {
                Par.Section = LDecoder.GetValue("Section");
                SetVoid();
                return;
            }

            // Секции могут быть:
            // Mail, Vars и имена правил

            if (Par.Section.Equals("Mail", StringComparison.CurrentCultureIgnoreCase)
                || Par.Section.Equals("Vars", StringComparison.CurrentCultureIgnoreCase))
            {
                SetVoid();
                return;
            }

            Par.ReadLine(LDecoder);
            
            if (Par.Debug)
            {
                string val;
                if (!LDecoder.IsVarExpanded(out val))
                {
                    Log.LogMessage(LogType.Error, "Не найдена переменная в строке [" + Par.CurLineNum.ToString()+"] "+val);
                    return;
                }
                else
                    return;
            }

            
            
            // Rule не задано явно, копируем из section
            if (!LDecoder.ContainsKey("RULE") && !String.IsNullOrEmpty(Par.Section))
            {
                LDecoder.Words.Add("RULE", Par.Section);
                
            }

            sValue = LDecoder.GetValue("RULE");
            //sValue = sValue.ToUpper();
            if (String.IsNullOrEmpty(sValue)) // Имя правила пусто
            {
                SetVoid();
                return;
            }
            Rule.RuleName = sValue;
            //Options.UpdateGlobal(LDecoder); // Обновляем глобальные настройки (Если это правило settings)
            

            if (!Rule.RuleName.Equals(ruleName, StringComparison.CurrentCultureIgnoreCase)) // Имя правила не совпадает
            {
                SetVoid();
                return;
            }

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
                case "UNARJ":
                    Rule.Action = TAction.UnArj;
                    break;
                case "UNCAB":
                    Rule.Action = TAction.UnCab;
                    break;
                case "DELETE":
                    Rule.Action = TAction.Delete;
                    break;
                case "MOVENALOGDIR":
                    Rule.Action = TAction.MoveNalogDir;
                    break;
                case "MOVEFTSDIR":
                    Rule.Action = TAction.MoveFTSDir;
                    break;
                case "PBGEN":
                    Rule.Action = TAction.PbGen;
                    break;
                case "MERGENALOGFILE":
                    Rule.Action = TAction.MergeNalogFile;
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
                case TAction.UnArj:
                    ActUnArj();
                    break;
                case TAction.UnCab:
                    ActUnCab();
                    break;
                case TAction.Delete:
                    ActDelete();
                    break;
                case TAction.MoveNalogDir:
                    ActMoveNalogDir();
                    break;
                case TAction.MoveFTSDir:
                    ActMoveNalogDir();
                    break;
                case TAction.PbGen:
                    ActPbGen();
                    break;
                case TAction.MergeNalogFile:
                    ActMergeNalogFile();
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
            //args = ReplaceVar.ReplDate(args);

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
                    tArg = Par.ReplFile(args, sfile,Rule.SFiles);
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

        /// <summary>
        /// Объединение файлов налоговой в один с убиранием подписи для распечатки
        /// </summary>
        void ActMergeNalogFile()
        {
            if (_isEmpty) return;
            if (Rule.SFiles.Count == 0) return; // Файлов нет

            string line;
            string pages = LDecoder.GetValue("pages"); // Разбивать на страницы (если pages=yes)
            bool split = false;
            bool splitnext=false;
            if (pages.ToLower() == "yes") split = true;
            FileStream fs = FileCreateEx(Rule.Dest, Log);
            StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding(866));
            foreach (string sfile in Rule.SFiles) // Перебор всех файлов
            {
                if (split && splitnext) // Вставка разрыва страницы
                {
                    sw.WriteLine("\f");
                } 
                // Читаем файл построчно до строки "==="
                StreamReader sr = new StreamReader(sfile, Encoding.GetEncoding(866));
                while (!sr.EndOfStream) // Перебор всех строк в файле
                {
                    
                    line=sr.ReadLine();
                    sw.WriteLine(line);
                    if (line == "===")
                    {
                        
                        break;
                    }
                    
                }

                sr.Close();
                if (!splitnext) splitnext = true;

            }
            sw.Close();
            fs.Close();


        }

        /// <summary>
        /// Распаковка UnArj
        /// </summary>
        void ActUnArj()
        {
            string cmd = "arj32.exe";
            int i;
            string ArjFile, tDest, args;
            for (i = 0; i < Rule.SFiles.Count; i++) // Перебираем исходные файлы
            {
                ArjFile = Rule.SFiles[i]; // Имя архива
                tDest = Path.GetDirectoryName(Rule.DFiles[i]); // Каталог приемник
                tDest = tDest + Path.DirectorySeparatorChar;
                DirectoryCreateEx(tDest, Log);
                args = "e -y " + ArjFile + " " + tDest; // arj не понимает кавычки
                Exec(cmd, args, true);

            }
        }
        /// <summary>
        /// Распаковка UnCab
        /// </summary>
        void ActUnCab()
        {
            string cmd = "expand.exe";
            int i;
            string CabFile, tDest, args;
            for (i = 0; i < Rule.SFiles.Count; i++) // Перебираем исходные файлы
            {
                CabFile = Rule.SFiles[i]; // Имя архива
                tDest = Path.GetDirectoryName(Rule.DFiles[i]); // Каталог приемник
                tDest = tDest + Path.DirectorySeparatorChar;
                DirectoryCreateEx(tDest, Log);
                //args = "-R \"" + CabFile + "\" \"" + tDest + "\"";
                args = "-R " + CabFile + " " + tDest;
                Exec(cmd, args, true);

            }
        }

        void ActMoveNalogDir()
        {
            string DirTo,ShortDir;
            string SourceDir, SourceMask;
            SourceDir = Path.GetDirectoryName(Rule.Source);
            SourceMask = Path.GetFileName(Rule.Source);
            // В исходном каталоге ищем подкаталоги по заданной маске и содержащие полный комплект ответов
            string[] FDirs = Directory.GetDirectories(SourceDir, SourceMask); // Каталоги для поиска полных комплектов
            foreach (string FDir in FDirs)
            {
                if (IsDirFin(FDir)) // Каталог завершен, его нужно переместить в Dest
                {

                    ShortDir = Path.GetFileName(FDir); // Возможно неправильно! нужно имя каталога на конце строки
                    if (ShortDir.StartsWith("!")) ShortDir = ShortDir.Substring(1); // Убираем "!"
                    DirTo = Path.Combine(Rule.Dest, ShortDir);
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
        /// Возвращает true если каталог Dir содержит полный комплект ответов из налоговой или ФТС
        /// </summary>
        /// <param name="Dir"></param>
        /// <returns></returns>
        bool IsDirFin(string Dir)
        {
            switch (Rule.Action)
            {
                case TAction.MoveNalogDir:
                    return IsNalogDirFin(Dir);
                    
                case TAction.MoveFTSDir:
                    return IsFTSDirFin(Dir);
                    
            }
            return false;

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
            string FindFile = Prefix + shortFindFile.Substring(Prefix.Length);
            FindFile = Path.Combine(Dir, FindFile);
            return File.Exists(FindFile);
        }
        /// <summary>
        /// Возвращает true если каталог Dir содержит полный комплект ответов из ФТС
        /// </summary>
        /// <param name="Dir"></param>
        /// <returns></returns>
        bool IsFTSDirFin(string Dir)
        {
            string[] SendFiles;
            bool IsAnsw;
            SendFiles = Directory.GetFiles(Dir, "ps*.xml"); // Файлы отправляемые в ФТС
            if (SendFiles.Length < 1) return false; // Отправляемых файлов нет
            // К кажому ps файлу должен быть fs файл
            foreach (string SFile in SendFiles)
            {
                IsAnsw = IsNalogAnswerExists(SFile, "F");
                if (!IsAnsw) return false;
                
            }
            return true;
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
        /// Expands environment variables and, if unqualified, locates the exe in the working directory
        /// or the evironment's path.
        /// </summary>
        /// <param name="exe">The name of the executable file</param>
        /// <returns>The fully-qualified path to the file</returns>
        /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
        string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == String.Empty) // Поиск в Path
                {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
                // Поиск в текущем каталоге с программой
                string fullPath = GetFullFileName(exe);
                return fullPath;
            }
            return Path.GetFullPath(exe);
        }
        /// <summary>
        /// Запуск внешнего файла
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="Arg"></param>
        /// <param name="WaitForExit"></param>
        void Exec(string Cmd, string Arg, bool WaitForExit)
        {
            
            // Ищем файл в path
            string FullCmd = FindExePath(Cmd);
            if (!File.Exists(FullCmd))
            {
               Log.LogMessage(LogType.Error, "Не найден файл для запуска " + Cmd);
               return;
            }

            System.Diagnostics.Process pr = new System.Diagnostics.Process();

            pr.StartInfo.FileName = FullCmd;
            pr.StartInfo.Arguments = Arg;

            Log.LogMessage(LogType.Error, "Запуск " + FullCmd + " " + Arg);

            if (!pr.Start())  // Ошибка запуска
            {
                Log.LogMessage(LogType.Error, "Ошибка запуска " + FullCmd + " " + pr.StartInfo.Arguments);
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
        /// Удаление файлов
        /// </summary>
        void ActDelete()
        {
            if (_isEmpty) return;
            
            int i;
            for (i = 0; i < Rule.SFiles.Count; i++)
            {
                

                
                        if (FileDeleteEx(Rule.SFiles[i], Log))
                        {
                            Log.LogMessage(LogType.Info, "Удален файл " + Rule.SFiles[i] );
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
                    tSubj = Par.ReplFile(Subj, sfile, Rule.SFiles);
                    tMsg = Par.ReplFile(Msg, sfile, Rule.SFiles);
                    if (SendMail(MailTo, tSubj, tMsg, sfile))
                    {
                        Log.LogMessage(LogType.Info, "Файл отправлен по почте " + sfile + " для " + MailTo);
                    }
                }
            }
            if (Rule.Action == TAction.SendMsg) // Отправка сообщения о файлах
            {
                Subj = Par.ReplFile(Subj, "", Rule.SFiles);
                Msg = Par.ReplFile(Msg, "", Rule.SFiles);
                if (SendMail(MailTo, Subj, Msg, ""))
                {
                    Log.LogMessage(LogType.Info, "Сообщение о файле(ах) отправлено по почте " + Par.GetFileListStr(Rule.SFiles, true) + " для " + MailTo);
                }
            }

        }

        /// <summary>
        /// Генерация квитков PB1 на файлы R* налоговой 
        /// </summary>
        /// <param name="inFolder"></param>
        /// <param name="outFolder"></param>
        /// <returns></returns>
        void ActPbGen()
        {
            if (_isEmpty) return;
            if (Rule.SFiles.Count == 0) return; // Файлов нет

            bool res;

            foreach (string sfile in Rule.SFiles)
            {
                res = KvitGen(sfile, Rule.Dest);
                if (res)
                {
                    Log.LogMessage(LogType.Info, "Сформирован квиток PB1 для " + sfile + " в каталог " + Rule.Dest);
                }
                else
                {
                    Log.LogMessage(LogType.Error, "Возникло исключение при формировании квитка PB1 для " + sfile + " в каталог " + Rule.Dest);
                }
            }


                
        }

        /// <summary>
        /// Генерация квитка PB1 на файл fileName в каталог outFolder
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="outFolder"></param>
        /// <returns></returns>
        bool KvitGen(string fileName, string outFolder)
        {
            DirectoryCreateEx(outFolder, Log);
            string newFileName = "PB1_" + Path.GetFileNameWithoutExtension(fileName) + ".txt";
            newFileName = Path.Combine(outFolder, newFileName);
            try
            {
                StreamWriter sw = File.CreateText(newFileName);
                sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + "###");
                sw.WriteLine("10@@@");
                DateTime.Today.ToString("YYYY-mm-dd");
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd") + "@@@");
                sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "@@@");
                sw.WriteLine("===");
                sw.Close();
            }
            catch
            {
                return false;
            }
            return true;
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
            bool ret;
            //Формирование письма
            MailMessage Message = new MailMessage();
            Attachment att=null; // Вложение
            Message.From = new MailAddress(Par.MailFrom);
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
            if (Int32.TryParse(Par.MailPort, out tport))
            {
                port = tport;
            }
            try
            {
                SmtpClient Smtp = new SmtpClient(Par.MailSrv, port);
                if (!String.IsNullOrEmpty(Par.MailPass)) Smtp.Credentials = new NetworkCredential(Par.MailUser, Par.MailPass);
                Smtp.Send(Message);//отправка
                
                ret=true;
            }
            catch (SmtpFailedRecipientsException ESmtp)
            {
                //if (att != null) att.Dispose(); // Освобждение файла во вложении
                Log.LogMessage(LogType.Error, "Ошибка отправки на почту. SmtpFailedRecipientsException. " + ESmtp.Message);
                ret = false;
            }
            catch (SmtpException ESmtp)
            {
                //if (att != null) att.Dispose(); // Освобждение файла во вложении
                switch (ESmtp.StatusCode)
                {
                    case SmtpStatusCode.GeneralFailure:
                        Log.LogMessage(LogType.Error, "Ошибка отправки на почту. SmtpException. Сервер недоступен " + Par.MailSrv + ". " + ESmtp.Message);
                        break;
                    default:
                        Log.LogMessage(LogType.Error, "Ошибка отправки на почту. SmtpException. StatusCode=" + ESmtp.StatusCode.ToString() + ". Smtp host=" + Par.MailSrv +". "+ ESmtp.Message);
                        break;
                }

                ret = false;
            }

            catch (Exception E)
            {
                //if (att != null) att.Dispose(); // Освобждение файла во вложении
                Log.LogMessage(LogType.Error, "Ошибка отправки на почту. " + E.Message);
                ret = false;
            }
            finally
            {
                if (att != null) att.Dispose(); // Освобждение файла во вложении
                Message.Dispose(); // Освобождение сообщения
                
            }
            return ret;
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
        static FileStream FileCreateEx(string FileName, Logging Log)
        {
            string Dir = Path.GetDirectoryName(FileName);
            DirectoryCreateEx(Dir, Log);
            try
            {
                FileStream fs=File.Create(FileName);
                return fs;
            }
            catch (Exception E)
            {

                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка создания файла " + FileName + ". " + E.Message);
                return null;
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

        //-------------------------------------------------------------------------------------------
        /// <summary>
        /// Проверка не открыт ли файл кем либо еще
        /// </summary>
        /// <param name="FileName">Путь к файлу</param>
        /// <returns></returns>
        public static bool IsFileBlocked(string FileName)
        {
            try
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Write))
                {
                    fs.Close();
                }
                return false;
            }
            catch
            {

                return true;
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
            //Rule.Source = ReplaceVar.ReplDate(Rule.Source); // Подстановка текущих даты времени
            Rule.Contain = LDecoder.GetValue("CONTAIN");
            if (String.IsNullOrEmpty(Rule.Source)) return;
            if (Rule.Action == TAction.MoveNalogDir) return; // Перемещение каталога налоговой, файлов нет

            //Rule.SourceDir = Path.GetDirectoryName(Rule.Source);
            //if (!Directory.Exists(Rule.SourceDir)) return;
            //Rule.SourceMask = Path.GetFileName(Rule.Source);

            
            string Exclude = LDecoder.GetValue("EXCLUDE"); // Исключаемые маски
            string Include = LDecoder.GetValue("INC"); // Включаемые маски

            DirInfo di = new DirInfo(Rule.Source, Include, Exclude, Rule.Contain);
            Rule.SFiles = di.GetFiles();
            
        }
        /// <summary>
        /// Заполнение списка конечных файлов по исходным и ключу D
        /// </summary>
        void FillDFiles()
        {
            string tDest;

            Rule.Dest = LDecoder.GetValue("D");
            //Rule.Dest = ReplaceVar.ReplDate(Rule.Dest); // Замена даты
            if (String.IsNullOrEmpty(Rule.Dest)) return; // не указан приемник
            if (Rule.Action == TAction.MoveNalogDir) return; // Перемещение каталога налоговой, файлов нет
            string shortFileName;
            foreach (string FullSFile in Rule.SFiles) // перебираем все исходные файлы
            {
                shortFileName = GetRenFileName(FullSFile); // Переименование если нужно
                tDest = Par.ReplFile(Rule.Dest, FullSFile, Rule.SFiles); // Замена %file% в имени каталога приемника
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
