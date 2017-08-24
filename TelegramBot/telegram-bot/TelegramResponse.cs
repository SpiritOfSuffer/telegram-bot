using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class TelegramResponse
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public int ChatId { get; set; }
        public int MessageId { get; set; }
        public int LastUpdateId { get; set; } = 0;
    }
}
