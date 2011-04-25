using System;
using System.Collections.Generic;
using System.Linq;
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

        public FRouter(string FileName, string ruleName)
        {
            ConfigFile = FileName;
            RuleName = ruleName;


            Log = new Logging("Log", RuleName, LogType.Info);

            
  
        }

        public void DoRule()
        {
            RuleLine CurRule;
            string[] Lines;



            Lines = File.ReadAllLines(ConfigFile,Encoding.GetEncoding(1251)); // Читаем файл с правилами в строки

            foreach (string sLine in Lines)
            {
                CurRule = new RuleLine(sLine,RuleName,Log);
                if (!CurRule.IsEmpty)
                {
                    CurRule.DoAction();
                }
            }
        }
        

    }
}
