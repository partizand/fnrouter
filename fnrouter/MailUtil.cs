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
