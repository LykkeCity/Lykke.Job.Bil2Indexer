using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using RabbitMQ.Client.Events;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class InMemoryBlocksQueue : IDisposable
    {
        public class BlockReceivedEventArgs : EventArgs
        {
            public BlockHeader Block { get; }

            public BlockReceivedEventArgs(BlockHeader block)
            {
                Block = block;
            }
        }

        public event AsyncEventHandler<BlockReceivedEventArgs> BlockReceived;

        public Exception BackgroundException => _task.Exception;

        private readonly ConcurrentQueue<BlockHeader> _messages;
        private readonly Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        public InMemoryBlocksQueue()
        {
            _messages = new ConcurrentQueue<BlockHeader>();
            _cancellationTokenSource = new CancellationTokenSource();

            _task = new Task(() =>
            {
                var exceptions = new List<Exception>();

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    while (!_cancellationTokenSource.IsCancellationRequested && _messages.TryDequeue(out var block))
                    {
                        try
                        {
                            OnBlockReceivedAsync(block)
                                .ConfigureAwait(false)
                                .GetAwaiter()
                                .GetResult();

                            exceptions.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Processing error: {ex.Message}");
                            
                            exceptions.Add(ex);

                            // Enqueue the message again
                            Publish(block);
                        }
                    }
                   
                    Task.Delay(TimeSpan.FromMilliseconds(20))
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
            },
            _cancellationTokenSource.Token);

            _task.Start();
        }

        public void Publish(BlockHeader header)
        {
            _messages.Enqueue(header);
        }

        public void Stop()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public bool Wait()
        {
            _task.Wait(Waiting.Timeout);

            return true;
        }

        public void Dispose()
        {
            Stop();
            Wait();
            _task.Dispose();
        }

        private Task OnBlockReceivedAsync(BlockHeader block)
        {
            return BlockReceived?.Invoke(this, new BlockReceivedEventArgs(block)) ?? 
                   Task.CompletedTask;
        }
    }
}
