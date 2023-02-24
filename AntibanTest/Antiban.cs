using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AntibanTest
{
    public class Antiban
    {
        private readonly List<EventMessage> _messages = new();

        public List<AntibanResult> GetResult()
        {
            foreach (EventMessage item in _messages)
            {
                item.Reset();
            }

            Dictionary<string, DateTime> lastMessages = new();
            Dictionary<string, DateTime> lastPriorityMessages = new();

            PriorityQueue<EventMessage, (DateTime, int)> queue = new(_messages.Select(m => (m, (m.DateTime, m.Id))));

            EventMessage first = queue.Dequeue();
            DateTime lastSentAt = first.DateTime;
            lastMessages[first.Phone] = lastSentAt;
            if (first.Priority == 1)
            {
                lastPriorityMessages[first.Phone] = lastSentAt;
            }

            while (queue.Count > 0)
            {
                EventMessage next = queue.Dequeue();

                DateTime tempLastSentAt = next.DateTime;

                if ((next.DateTime - lastSentAt).TotalSeconds < 10)
                {
                    tempLastSentAt = lastSentAt.AddSeconds(10);
                }

                if (lastMessages.TryGetValue(next.Phone, out DateTime time))
                {
                    if ((next.DateTime - time).TotalMinutes < 1)
                    {
                        tempLastSentAt = time.AddMinutes(1);
                    }
                }

                if (next.Priority == 1)
                {
                    if (lastPriorityMessages.TryGetValue(next.Phone, out DateTime time2))
                    {
                        if ((next.DateTime - time2).TotalHours < 24)
                        {
                            tempLastSentAt = time2.AddHours(24);
                        }
                    }
                }

                if (tempLastSentAt != next.DateTime)
                {
                    next.DateTime = tempLastSentAt;
                    queue.Enqueue(next, (next.DateTime, next.Id));
                }
                else
                {
                    lastSentAt = tempLastSentAt;
                    lastMessages[next.Phone] = lastSentAt;

                    if (next.Priority == 1)
                    {
                        lastPriorityMessages[next.Phone] = lastSentAt;
                    }
                }
            }

            List<AntibanResult> result = _messages
                .OrderBy(m => m.DateTime)
                .Select(m => new AntibanResult { EventMessageId = m.Id, SentDateTime = m.DateTime })
                .ToList();

            return result;
        }

        public void PushEventMessage(EventMessage eventMessage) => _messages.Add(eventMessage);
    }
}
