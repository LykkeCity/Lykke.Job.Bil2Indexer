using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Polly;
using RabbitMQ.Client.Events;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class InMemoryReadBlockCommandsQueue : IDisposable
    {
        public class CommandReceivedEventArgs : EventArgs
        {
            public ReadBlockCommand Command { get; }

            public CommandReceivedEventArgs(ReadBlockCommand command)
            {
                Command = command;
            }
        }

        public event AsyncEventHandler<CommandReceivedEventArgs> CommandReceived;

        public Exception BackgroundException => _task.Exception;

        private readonly ConcurrentQueue<ReadBlockCommand> _commands;
        private readonly Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        public InMemoryReadBlockCommandsQueue()
        {
            _commands = new ConcurrentQueue<ReadBlockCommand>();
            _cancellationTokenSource = new CancellationTokenSource();

            _task = new Task(() =>
            {
                var exceptions = new List<Exception>();

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    while (!_cancellationTokenSource.IsCancellationRequested && _commands.TryDequeue(out var command))
                    {
                        var cmd = command;

                        Policy.Handle<Exception>()
                            .WaitAndRetryForeverAsync(
                                i => TimeSpan.FromMilliseconds(500), 
                                (ex, delay) => Console.WriteLine($"Retrying block {cmd.BlockNumber} in {delay}"))
                            .ExecuteAsync(
                                async () =>
                                {
                                    if (_cancellationTokenSource.IsCancellationRequested)
                                    {
                                        return;
                                    }

                                    try
                                    {
                                        await OnCommandReceivedAsync(cmd);
                                        exceptions.Clear();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Processing error: {ex.Message}");
                                        exceptions.Add(ex);
                                        throw;
                                    }
                                })
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();

                        if (exceptions.Any())
                        {
                            throw new AggregateException(exceptions);
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

        public void Send(ReadBlockCommand command)
        {
            _commands.Enqueue(command);
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
            if (!Debugger.IsAttached)
            {
                return _task.Wait(TimeSpan.FromSeconds(1));
            }

            _task.Wait();

            return true;
        }

        public void Dispose()
        {
            Stop();
            Wait();
            _task.Dispose();
        }

        private Task OnCommandReceivedAsync(ReadBlockCommand command)
        {
            return CommandReceived?.Invoke(this, new CommandReceivedEventArgs(command)) ?? 
                   Task.CompletedTask;
        }
    }
}
