using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using FASTER.core;
using System.Threading.Tasks;
using System.Threading;

namespace Efflux.Stream
{
    public class BasicStream
    {
        internal FasterLog log;
        internal EffluxMessage PriorRecord { get; set; }

        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        IDevice device;
        protected readonly ILogger _logger;
        //Thread commitThread;
        string LogFilename;

        public BasicStream(ILogger logger)
        {
            _logger = logger;
        }

        public static async Task<BasicStream> OpenAsync(
                ILogger logger, string filename)
        {
            var stream = new BasicStream(logger);
            await stream.OpenLogAsync(filename);

            return stream;
        }

        protected virtual async Task OpenLogAsync(string logFilename)
        {
            if (!Directory.Exists(logFilename))
            {
                Directory.CreateDirectory(logFilename);
            }
            LogFilename = logFilename + "/stream";
            bool newLog = !File.Exists(LogFilename + ".0");
            device = Devices.CreateLogDevice(LogFilename);
            // FasterLog will recover and resume if there is a previous commit found
            log = new FasterLog(new FasterLogSettings { LogDevice = device });

            //commitThread = new Thread(new ThreadStart(CommitThread));
            //commitThread.Start();
            //commitThread.Join();

            if (newLog)
            {
                // Write root record
                PriorRecord = new EffluxMessage("Root");
                var buffer1 = PriorRecord.ToBytes().ToArray();
                log.Enqueue(buffer1);
                await log.CommitAsync();
            }
            else
            {
                var offset = log.CommittedUntilAddress;
                _logger.LogInformation($"[{LogFilename}] Resuming log at {offset}");

                // TODO: Fix reading of prior record
                var result = await log.ReadAsync(offset);
                PriorRecord = new EffluxMessage(result.Item1);
            }
        }

        public virtual async Task<long> WriteMessageAsync(EffluxMessage mssage)
        {
            long streamPosition;
            DateTime startTime = DateTime.Now;

            // Need a lock to make sure the prior record is linked to only
            // the next record in the chain.
            await semaphoreSlim.WaitAsync();

            _logger.LogInformation($"[{LogFilename}] Begin stream write {mssage.MetaData.Id}");
            mssage.MetaData.LinkedFingerprint = PriorRecord.Fingerprint;
            var buffer = mssage.ToBytes().ToArray();

            //streamPosition = log.EnqueueAndWaitForCommit(buffer);
            streamPosition = await log.EnqueueAsync(buffer);
            await log.CommitAsync();

            PriorRecord = mssage;
            _logger.LogInformation($"[{LogFilename}] End stream write {mssage.MetaData.Id}");
            semaphoreSlim.Release();

            var length = DateTime.Now - startTime;
            _logger.LogInformation($"[{LogFilename}] Wrote record {mssage.MetaData.Id} in {length.TotalMilliseconds}ms");
            return streamPosition;

        }

        public async Task<MessageReadResult> ReadMessageAsync(long offset)
        {
            _logger.LogInformation($"Begin stream read at {offset}");


            var result = await log.ReadAsync(offset);
            if (result.Item1 == null)
            {
                _logger.LogInformation($"[{LogFilename}] End of stream");
                return new MessageReadResult()
                {
                    Message = null,
                    EndOfStream = true,
                    NextMessageOffset = offset
                };
            }
            else
            {
                var record = EffluxMessage.FromStream(new MemoryStream(result.Item1));
                var streamPosition = offset + result.Item2 + 4;
                streamPosition = streamPosition + (streamPosition % 4);

                if (record.PayloadAsString() == "Root")
                {
                    _logger.LogInformation($"[{LogFilename}] Found 'Root' on read at {offset}: retrieved {record.MetaData.Id} now at {streamPosition} of {log.TailAddress} ");
                    var readResult = await ReadMessageAsync(streamPosition);
                    return readResult;
                }
                else
                {
                    _logger.LogInformation($"[{LogFilename}] End stream read at {offset}: retrieved {record.MetaData.Id} now at {streamPosition} of {log.TailAddress} ");

                    return new MessageReadResult()
                    {
                        Message = record,
                        EndOfStream = false,
                        NextMessageOffset = streamPosition
                    };
                }
            }
        }


        void CommitThread()
        {
            Console.WriteLine("Commit thread running");
            //Task<LinkedCommitInfo> prevCommitTask = null;
            while (true)
            {
                Thread.Sleep(5);
                log.Commit(true);

                // Async version
                // await log.CommitAsync();

                // Async version that catches all commit failures in between
                //try
                //{
                //    prevCommitTask = await log.CommitAsync(prevCommitTask);
                //}
                //catch (CommitFailureException e)
                //{
                //    Console.WriteLine(e);
                //    prevCommitTask = e.LinkedCommitInfo.nextTcs.Task;
                //}
            }
        }

    }

}
