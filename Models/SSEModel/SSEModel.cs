
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dotnetSSE.Models.SSEModel
{
    public class SSEModel
    {
        internal static readonly ConcurrentDictionary<string, List<StreamWriter>> MessagesMembers =
            new ConcurrentDictionary<string, List<StreamWriter>>();

        public async Task ListenToMessagesModel(string id, Stream body, CancellationToken token)
        {
            using (var message = new StreamWriter(body))
            {
                var messages = MessagesMembers.GetOrAdd(id, _ => new List<StreamWriter>(5));

                lock (messages) 
                    messages.Add(message);
                
                await message.WriteAsync("event: connected\ndata:\n\n");
                await message.FlushAsync();

                try
                {
                    // to keep alive
                    await Task.Delay(Timeout.Infinite, token);
                }
                catch (TaskCanceledException) 
                {
                    lock (messages)
                    messages.Remove(message);
                }
            }
        }

        // SendMessages 
        public async Task SendMessagesModel(string id, string message)
        {
            if (!MessagesMembers.TryGetValue(id, out var messages))
                return;

            lock (messages) 
                messages.ToList(); 
                
            async Task Send(StreamWriter mess)
            {
                try
                {
                    await mess.WriteAsync("data: " + message + "\n\n");
                    await mess.FlushAsync();
                }
                catch (ObjectDisposedException)
                {
                    lock (messages)
                    messages.Remove(mess);
                }
            }

            await Task.WhenAll(messages.Select(Send));
        }
    }
}