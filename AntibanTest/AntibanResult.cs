using System;

namespace AntibanTest
{
    public class AntibanResult
    {
        /// <summary>
        /// Предполагаемое время отправки сообщения
        /// </summary>
        public DateTime SentDateTime { get; set; }
        public int EventMessageId { get; set; }
    }
}
