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
    public class Bot
    {
        private const string VKApiUrl = @"https://api.vk.com/method/wall.get?domain=habr&count=10&access_token=1111111111111111111111111111111111111111111111111111111111111111111111111111111111111&filter=owner"; //Запрос, в котором мы получаем 10 последних записей со стены какого-то сообщества (например хабра), в котором передаем id паблика, кол-во записей, и токен
        private const string VKPostUrl = @"https://vk.com/habr?w=wall-20629724_"; //Неполная ссылка на конкретный пост. После символа "_" будет подставляться id поста
        private const string TelegramApiUrl = @"https://api.telegram.org/bot";  //Ссылка на Telegram API
        private const string Token = @"111111111:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";   //Токен телеграмм-бота
        public TelegramResponse response = new TelegramResponse();
        public void GetUpdates()    //Метод будет работать в потоке и постоянно получать обновление состояния бота
        {
            while (true)    //Запускаем бесконечный цикл
            {
                using (WebClient client = new WebClient())  //Создаем объект класса WebClient для работы с сетью
                {
                    string Response = client.DownloadString(TelegramApiUrl + Token + "/getupdates?offset=" + (response.LastUpdateId + 1));    //Строка, в которую мы будем получать данные с запроса

                    if (Response.Length <= 23)  //Если в строке нет данных (шаблон строки занимает 23 символа) - продолжаем цикл
                    {
                        continue;
                    }
                    var data = JSON.Parse(Response);    //Парсим данные

                    foreach (JSONNode node in data["result"].AsArray)   //Циклом ищем нужные нам поля и сохраняем в свойства их значения
                    {
                        response.LastUpdateId = node["update_id"].AsInt;
                        response.Name = node["message"]["chat"]["username"];
                        response.Message = node["message"]["text"];
                        response.MessageId = node["message"]["message_id"].AsInt;
                        response.ChatId = node["message"]["chat"]["id"].AsInt;

                        if (response.Message == "Hello")
                        {
                            SendMessage(response.ChatId, "Hi! :)");
                            ForwardMessage(response.ChatId, response.ChatId, response.MessageId);
                        }

                        if (response.Message == "Show meme")
                        {
                            SendPhoto(response.ChatId, @"C:\Users\your_path");
                            SendPhotoLink(response.ChatId, "YOUR_LINK");
                        }

                        if (response.Message == "Music, please!")
                        {
                            SendAudio(response.ChatId, @"C:\Users\your_path");
                            SendAudioLink(response.ChatId, "YOUR_LINK");
                        }

                        if (response.Message == "I wanna see some video")
                        {
                            SendVideo(response.ChatId, @"C:\Users\your_path");
                            SendVideoLink(response.ChatId, "YOUR_LINK");
                        }
                    }
                }
                OutputData(response.Name, response.Message);    //Выводим лог сообщений
            }
        }

        //Отправка сообщения
        public void SendMessage(int chatId, string message)
        {
            using (WebClient client = new WebClient())
            {
                NameValueCollection collection = new NameValueCollection();
                collection.Add("chat_id", chatId.ToString());
                collection.Add("text", message);
                client.UploadValues(TelegramApiUrl + Token + "/sendMessage", collection);
            }
        }

        //Пересыл сообщения
        public void ForwardMessage(int chatId, int fromchatId, int messageId)
        {
            using (WebClient client = new WebClient())
            {
                NameValueCollection collection = new NameValueCollection();
                collection.Add("chat_id", chatId.ToString());
                collection.Add("from_chat_id", fromchatId.ToString());
                collection.Add("message_id", messageId.ToString());
                client.UploadValues(TelegramApiUrl + Token + "/forwardMessage", collection);
            }
        }

        //Отправка фото по ссылке 
        public void SendPhotoLink(int chatId, string photoLink)
        {
            using (WebClient client = new WebClient())
            {
                NameValueCollection collection = new NameValueCollection();
                collection.Add("chat_id", chatId.ToString());
                collection.Add("photo", photoLink);
                client.UploadValues(TelegramApiUrl + Token + "/sendPhoto", collection);
            }
        }

        //Отправка аудио по ссылке
        public void SendAudioLink(int chatId, string audioLink)
        {
            using (WebClient client = new WebClient())
            {
                NameValueCollection collection = new NameValueCollection();
                collection.Add("chat_id", chatId.ToString());
                collection.Add("audio", audioLink);
                client.UploadValues(TelegramApiUrl + Token + "/sendAudio", collection);
            }
        }

        //Отправка видео по ссылке
        public void SendVideoLink(int chatId, string videoLink)
        {
            using (WebClient client = new WebClient())
            {
                NameValueCollection collection = new NameValueCollection();
                collection.Add("chat_id", chatId.ToString());
                collection.Add("video", videoLink);
                client.UploadValues(TelegramApiUrl + Token + "/sendVideo", collection);
            }
        }

        //Отправка фото с компьютера
        async public void SendPhoto(int chatId, string photoPath)
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string fileName = photoPath.Split('\\').Last();

                form.Add(new StringContent(chatId.ToString(), Encoding.UTF8), "chat_id");
                using (FileStream fileStream = new FileStream(photoPath, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "photo", fileName);
                    using (HttpClient client = new HttpClient())
                    {
                        await client.PostAsync(TelegramApiUrl + Token + "/sendPhoto", form);
                    }
                }
            }
        }

        //Отправка аудио с компьютера
        async public void SendAudio(int chatId, string audioPath)
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string fileName = audioPath.Split('\\').Last();

                form.Add(new StringContent(chatId.ToString(), Encoding.UTF8), "chat_id");
                using (FileStream fileStream = new FileStream(audioPath, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "audio", fileName);
                    using (HttpClient client = new HttpClient())
                    {
                        await client.PostAsync(TelegramApiUrl + Token + "/sendAudio", form);
                    }
                }
            }
        }

        //Отправка видео с компьютера
        async public void SendVideo(int chatId, string videoPath)
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string fileName = videoPath.Split('\\').Last();

                form.Add(new StringContent(chatId.ToString(), Encoding.UTF8), "chat_id");
                using (FileStream fileStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "video", fileName);
                    using (HttpClient client = new HttpClient())
                    {
                        await client.PostAsync(TelegramApiUrl + Token + "/sendVideo", form);
                    }
                }
            }
        }

        //Получение данных запроса со стены сообщества
        public string GetData()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(VKApiUrl);
                request.Method = "GET";
                request.Accept = "application/json";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    StringBuilder output = new StringBuilder();
                    output.Append(reader.ReadToEnd());
                    response.Close();

                    return output.ToString();
                }
            }
            catch (TimeoutException t)
            {
                Console.WriteLine(t);
                return null;
            }
        }

        //Отправка определенного количества последних постов из сообщества
        public void SendPosts(string items, int numberOfPosts)
        {
            var data = JSON.Parse(items);

            for (int i = 2; i <= numberOfPosts; i++)
            {
                string postLink = VKPostUrl + data["response"][i]["id"];
                SendMessage(79924905, postLink);
                Thread.Sleep(1000);
            }
        }

        //Отправка последнего поста из сообщества
        public void SendPost(object lastUpdateId)
        {
            while (true)
            {
                string items = GetData();
                var data = JSON.Parse(items);
                int id = data["response"][2]["id"].AsInt; // При значении 1 - будет браться первый пост, включая закрепленный. Если в сообществе есть закрепленный пост, и вы не хотите, чтобы он вам приходил - ставьте значение 2.

                if (id != Convert.ToInt32(lastUpdateId) && id != 0)
                {
                    string postLink = VKPostUrl + id.ToString();
                    SendMessage(79924905, postLink);
                    lastUpdateId = id;
                    Thread.Sleep(1000);
                }
            }
        }

        //Вывод логов в консоль
        private static void OutputData(string Name, string Message)
        {
            Console.WriteLine("{0}: {1}", Name, Message);
        }
    }
}
