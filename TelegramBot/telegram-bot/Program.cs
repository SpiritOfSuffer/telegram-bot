using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Collections.Specialized;
using System.Net.Http;
using System.IO;
using SimpleJSON;

namespace TelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot(); //Создаем объект класса Bot
            Thread thread = new Thread(bot.SendPost); //Создаем поток для постоянного обновления состояния стены сообщества для отправки последнего нового поста. В конструктор передаем метод SendPost

            thread.Start(0); //Запускаем поток, со значением по умолчанию (т.е. id поста) - 0
            Console.ReadKey();
        }
    }
}
