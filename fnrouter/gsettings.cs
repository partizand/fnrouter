using System;
using System.Collections.Generic;
using System.Text;

namespace fnrouter
{
    /// <summary>
    /// Общие настройки
    /// </summary>
    class GSettings
    {
        public static MParam Param;
    }
    /// <summary>
    /// Настройки, пока только почты
    /// </summary>
    class MParam
    {
        /*
        public static string MailSrv;
        public static string MailUser;
        public static string MailPort = "25";
        public static string MailFrom;
        */
        public string MailSrv;
        public string MailUser;
        public string MailPass;
        public string MailPort = "25";
        public string MailFrom;

        public MParam(string mailSrv, string mailUser, string mailPass, string mailPort, string mailFrom)
        {
            this.MailSrv = mailSrv;
            this.MailUser = mailUser;
            this.MailPass = mailPass;
            this.MailPort = mailPort;
            this.MailFrom = mailFrom;
        }
        /// <summary>
        /// Создание настроек из строки. Отстутвующие параметры становятся = ""
        /// </summary>
        /// <param name="LDecoder"></param>
        public MParam(LineDecoder LDecoder)
        {
            Read(LDecoder, true);
            
        }
        /// <summary>
        /// Обновляет глобальные настройки. Пустые параметры не обновляются. Обновляется только если rule=settings
        /// </summary>
        /// <param name="LDecoder"></param>
        public void UpdateGlobal(LineDecoder LDecoder)
        {
            string sValue;
            sValue = LDecoder.GetValue("Rule");
            if (!sValue.Equals("Settings", StringComparison.CurrentCultureIgnoreCase)) return;
            Read(LDecoder, false);
            
        }
        /// <summary>
        /// Заполняет настройки из строки
        /// </summary>
        /// <param name="LDecoder"></param>
        /// <param name="ReadEmpty"></param>
        public void Read(LineDecoder LDecoder,bool ReadEmpty)
        {
            string sValue;
            sValue = LDecoder.GetValue("MAILSRV");
            if (ReadEmpty) this.MailSrv = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailSrv = sValue;
            }
            sValue = LDecoder.GetValue("MAILFROM");
            if (ReadEmpty) this.MailFrom = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailFrom = sValue;
            }
            
            
            sValue = LDecoder.GetValue("MAILPORT");
            if (ReadEmpty) this.MailPort = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailPort = sValue;
            }
            sValue = LDecoder.GetValue("MailUser");
            if (ReadEmpty) this.MailUser = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailUser = sValue;
            }
            sValue = LDecoder.GetValue("MailPass");
            if (ReadEmpty) this.MailPass = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailPass = sValue;
            }
            
        }
        /// <summary>
        /// Перекрытие текущих настроек глобальными. Все пустые строки заменяются на глобоальные
        /// </summary>
        /// <param name="GlobalParam"></param>
        public void CoverGlobal(MParam GlobalParam)
        {
            if (String.IsNullOrEmpty(MailSrv)) MailSrv = GlobalParam.MailSrv;
            if (String.IsNullOrEmpty(MailUser)) MailUser = GlobalParam.MailUser;
            if (String.IsNullOrEmpty(MailPort)) MailPort = GlobalParam.MailPort;
            if (String.IsNullOrEmpty(MailFrom)) MailFrom = GlobalParam.MailFrom;
        }
    }
}
