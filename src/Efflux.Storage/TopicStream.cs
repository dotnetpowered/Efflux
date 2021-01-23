using System;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Efflux.Stream
{
    public class TopicStream : BasicStream, ITopic
    {
        internal ITopicIndex topicIndex;

        public string TopicName { get; private set; }

        public TopicStream(ILogger logger, string topicName, ITopicIndex topicIndex) : base(logger)
        {
            this.topicIndex = topicIndex;
            this.TopicName = topicName;
        }

        public static async Task<TopicStream> OpenAsync(
                ILogger logger, string topicName, ITopicIndex topicIndex)
        {
            if (!Directory.Exists(topicName))
            {
                logger.LogInformation($"Open new topic: {topicName}");
                Directory.CreateDirectory(topicName);
            }
            else
            {
                logger.LogInformation($"Open topic: {topicName}");
            }

            var topic = new TopicStream(logger, topicName, topicIndex);
            await topic.OpenLogAsync("data-stream");

            return topic;
        }

        protected override async Task OpenLogAsync(string filename)
        {
            topicIndex.Open(TopicName);

            var logFilename = TopicName + "/" + filename;
            await base.OpenLogAsync(logFilename);
        }

        public async Task<EffluxMessage> FindByIdAsync(string id, bool consumeMessage = true)
        {
            var doc = topicIndex.FindMessage(id);

            if (doc == null)
                return null;

            var offset = doc.Offset;

            if (consumeMessage)
            {
                var read = await ReadMessageAsync(offset);

                if (!read.EndOfStream)
                    return read.Message;
                else
                    throw new InvalidDataException($"Document missing from stream at offset: {offset}");
            }
            else
            {
                var m = new EffluxMessage() { };
                m.MetaData.Id = id;
                m.MetaData.Timestamp = doc.Timestamp;
                // TODO: return _offset;
                return m;
            }
        }

        public override async Task<long> WriteMessageAsync(EffluxMessage message)
        {
            var streamPosition = await base.WriteMessageAsync(message);

            _logger.LogInformation($"Store document {message.MetaData.Id}");

            topicIndex.InsertMessageMetaData(new IndexedMessageMetaData(message.MetaData, streamPosition, message.DataBlockSize));


            return streamPosition;
        }

        public async Task<ITopicConsumer> CreateConsumerAsync(string ConsumerName, long startOffset = 0)
        {
            var consumerTracker = topicIndex.GetConsumer(ConsumerName);

            if (consumerTracker == null)
            {

                var offset = startOffset;

                if (startOffset == 0)
                    offset = log.BeginAddress;

                //// Read from offset to verify position in stream
                //var result = await ReadMessageAsync(startOffset);

                //if (startOffset == 0)
                //{
                //    // Skip past header record to first data record
                //    offset = result.NextMessageOffset;
                //}
                //else
                //{
                //    offset = startOffset;
                //}

                consumerTracker = new TopicConsumerTracker()
                {
                    Id = Guid.NewGuid(),
                    Name = ConsumerName,
                    CurrentOffset = offset,
                    StartOffset = startOffset,
                };
                topicIndex.AddConsumer(consumerTracker);

            }

            if (log.RecoveredIterators != null && log.RecoveredIterators.TryGetValue(ConsumerName, out long recoveredOffset))
            {
                startOffset = recoveredOffset;
                _logger.LogInformation($"[{TopicName}.{ConsumerName}] Recovered consumer with offset = {recoveredOffset}");
            }
            var iter = log.Scan(startOffset, long.MaxValue, name: ConsumerName);
            consumerTracker.CurrentOffset = iter.NextAddress;

            var consumer = new TopicStreamConsumer(this, consumerTracker, topicIndex, iter);

            return consumer;
        }



    }

}
