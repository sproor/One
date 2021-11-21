using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram_Bot_1._0
{
    class BotCommand
    {
        public string Command { get; set; }
        public string Example { get; set; }
        public Action<CommandModel, Update> Execute { get; set; }
        public Action<CommandModel, Update> OnError { get; set; }
        public int CountArgs;
        public static CommandModel Parse(string txt)
        {

            if (txt.StartsWith("/"))
            {
                string secondLine = " ";
                string firstLine = " ";
                
                var spaceIndex = txt.IndexOf(" ");
                if (spaceIndex == -1) { firstLine = txt; }
                else { firstLine = txt.Substring(0, spaceIndex); }
                if (spaceIndex != -1) { secondLine = txt.Substring(spaceIndex + 1); }
                return new CommandModel
                {
                    Command = firstLine,
                    Args = secondLine,

                };
            }
            return null;

            //if (txt.StartsWith("/"))
            //{
            //    var splits = txt.Split(' ');
            //    var name = splits?.FirstOrDefault();
            //    var args = splits.Skip(1).Take(splits.Count()).ToArray();
            //    return new CommandModel
            //    {
            //        Command = name,
            //        Args = args,

            //    };
            //}
            //return null;
        }

    }
}
