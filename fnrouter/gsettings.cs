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
            sValue = LDecoder.GetValue("MAILUSER");
            if (ReadEmpty) this.MailUser = sValue;
            else
            {
                if (!String.IsNullOrEmpty(sValue))
                    this.MailUser = sValue;
            }
            sValue = LDecoder.GetValue("MAILPASS");
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
            if (String.IsNullOrEmpty(MailPass)) MailPass = GlobalParam.MailPass;
            if (String.IsNullOrEmpty(MailPort)) MailPort = GlobalParam.MailPort;
            if (String.IsNullOrEmpty(MailFrom)) MailFrom = GlobalParam.MailFrom;
        }
    }
}
