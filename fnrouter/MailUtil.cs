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
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace fnrouter
{
    public class MailParams
    {
        public string MailSrv;
        

        public string MailUser;
        public string MailPass;
        public int MailPort
        {
            get { return _MailPort; }
            
        }
        private int _MailPort;
        public string MailFrom;

        public MailParams()
        {
            _MailPort = 25;
        }
        public MailParams(string mailSrv, string mailUser, string mailPass, string mailPort)
        {
            this.MailSrv = mailSrv;
            this.MailUser = mailUser;
            this.MailPass = mailPass;
            _MailPort = 25;
            SetPort(mailPort);
        }
        /// <summary>
        /// Задать порт подключения. Пусто или null - порт станет 25
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(string port)
        {
            if (String.IsNullOrEmpty(port)) _MailPort = 25;
            else
            {
                int tport;
                if (Int32.TryParse(port, out tport))
                {
                    _MailPort = tport;
                }
                
            }
        }
        
    }
    
    class MailUtil
    {

        /// <summary>
        /// Отправка письма с вложением
        /// </summary>
        /// <param name="mparam"></param>
        /// <param name="MailTo"></param>
        /// <param name="Subj"></param>
        /// <param name="Mess"></param>
        /// <param name="FileName"></param>
        public static bool SendMail(MailParams MailSrv, string MailTo, string Subj, string Msg, string FileName,Logging Log)
        {
            //int tport, port = 25;
            bool ret;
            //Формирование письма
            MailMessage Message = new MailMessage();
            Attachment att = null; // Вложение
            Message.From = new MailAddress(MailSrv.MailFrom);
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
            
            try
            {
                SmtpClient Smtp = new SmtpClient(MailSrv.MailSrv, MailSrv.MailPort);
                if (!String.IsNullOrEmpty(MailSrv.MailPass)) Smtp.Credentials = new NetworkCredential(MailSrv.MailUser, MailSrv.MailPass);
                Smtp.Send(Message);//отправка

                ret = true;
            }
            catch (SmtpFailedRecipientsException ESmtp)
            {
                //if (att != null) att.Dispose(); // Освобждение файла во вложении
                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка отправки на почту. SmtpFailedRecipientsException. " + ESmtp.Message);
                ret = false;
            }
            catch (SmtpException ESmtp)
            {
                //if (att != null) att.Dispose(); // Освобждение файла во вложении
                switch (ESmtp.StatusCode)
                {
                    case SmtpStatusCode.GeneralFailure:
                        if (Log != null) Log.LogMessage(LogType.Error, "Ошибка отправки на почту. SmtpException. Сервер недоступен " + MailSrv.MailSrv + ". " + ESmtp.Message);
                        break;
                    default:
                        if (Log != null) Log.LogMessage(LogType.Error, "Ошибка отправки на почту. SmtpException. StatusCode=" + ESmtp.StatusCode.ToString() + ". Smtp host=" + MailSrv.MailSrv + ". " + ESmtp.Message);
                        break;
                }

                ret = false;
            }

            catch (Exception E)
            {
                //if (att != null) att.Dispose(); // Освобждение файла во вложении
                if (Log != null) Log.LogMessage(LogType.Error, "Ошибка отправки на почту. " + E.Message);
                ret = false;
            }
            finally
            {
                if (att != null) att.Dispose(); // Освобждение файла во вложении
                Message.Dispose(); // Освобождение сообщения

            }
            return ret;
        }
        
    }
}
