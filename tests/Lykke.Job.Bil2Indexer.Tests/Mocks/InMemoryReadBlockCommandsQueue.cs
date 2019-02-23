using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Commands;
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

            _task = new Task(
                () =>
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        while (!_cancellationTokenSource.IsCancellationRequested && _commands.TryDequeue(out var command))
                        {
                            var cmd = command;

                            OnCommandReceivedAsync(cmd)
                                .ConfigureAwait(false)
                                .GetAwaiter()
                                .GetResult();
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

        public void Wait()
        {
            _task.Wait(TimeSpan.FromSeconds(5));
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
