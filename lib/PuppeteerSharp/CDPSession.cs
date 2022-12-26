using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PuppeteerSharp.Helpers.Json;
using PuppeteerSharp.Messaging;

namespace PuppeteerSharp
{
    /// <inheritdoc/>
    public class CDPSession : ICDPSession
    {
        private readonly ConcurrentDictionary<int, MessageTask> _callbacks;

        internal CDPSession(Connection connection, TargetType targetType, string sessionId)
        {
            Connection = connection;
            TargetType = targetType;
            Id = sessionId;

            _callbacks = new ConcurrentDictionary<int, MessageTask>();
        }

        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <inheritdoc/>
        public event EventHandler Disconnected;

        internal event EventHandler<SessionAttachedEventArgs> SessionAttached;

        /// <inheritdoc/>
        public TargetType TargetType { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public bool IsClosed { get; internal set; }

        /// <inheritdoc/>
        public string CloseReason { get; private set; }

        /// <inheritdoc/>
        public ILoggerFactory LoggerFactory => Connection.LoggerFactory;

        /// <inheritdoc/>
        internal Connection Connection { get; private set; }

        /// <inheritdoc/>
        public async Task<T> SendAsync<T>(string method, object args = null)
        {
            var content = await SendAsync(method, args).ConfigureAwait(false);
            return content.ToObject<T>(true);
        }

        /// <inheritdoc/>
        public async Task<JObject> SendAsync(string method, object args = null, bool waitForCallback = true)
        {
            if (Connection == null)
            {
                throw new PuppeteerException(
                    $"Protocol error ({method}): Session closed. " +
                    $"Most likely the {TargetType} has been closed." +
                    $"Close reason: {CloseReason}");
            }

            var id = Connection.GetMessageID();
            MessageTask callback = null;
            if (waitForCallback)
            {
                callback = new MessageTask
                {
                    TaskWrapper = new TaskCompletionSource<JObject>(TaskCreationOptions.RunContinuationsAsynchronously),
                    Method = method,
                };
                _callbacks[id] = callback;
            }

            try
            {
                await Connection.RawSendASync(id, method, args, Id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (waitForCallback && _callbacks.TryRemove(id, out _))
                {
                    callback.TaskWrapper.TrySetException(new MessageException(ex.Message, ex));
                }
            }

            return waitForCallback ? await callback.TaskWrapper.Task.ConfigureAwait(false) : null;
        }

        /// <inheritdoc/>
        public Task DetachAsync()
        {
            if (Connection == null)
            {
                throw new PuppeteerException($"Session already detached.Most likely the {TargetType} has been closed.");
            }

            return Connection.SendAsync("Target.detachFromTarget", new TargetDetachFromTargetRequest
            {
                SessionId = Id,
            });
        }

        internal void Send(string method, object args = null)
            => _ = SendAsync(method, args, false);

        internal bool HasPendingCallbacks() => _callbacks.Count != 0;

        internal void OnMessage(ConnectionResponse obj)
        {
            var id = obj.Id;

            if (id.HasValue && _callbacks.TryRemove(id.Value, out var callback))
            {
                Connection.MessageQueue.Enqueue(callback, obj);
            }
            else
            {
                var method = obj.Method;
                MessageReceived?.Invoke(this, new MessageEventArgs
                {
                    MessageID = method,
                    MessageData = obj.Params,
                });
            }
        }

        internal void Close(string closeReason)
        {
            if (IsClosed)
            {
                return;
            }

            CloseReason = closeReason;
            IsClosed = true;

            foreach (var callback in _callbacks.Values.ToArray())
            {
                callback.TaskWrapper.TrySetException(new TargetClosedException(
                    $"Protocol error({callback.Method}): Target closed.",
                    closeReason));
            }

            _callbacks.Clear();
            Disconnected?.Invoke(this, EventArgs.Empty);
            Connection = null;
        }

        internal void OnSessionAttached(CDPSession session)
            => SessionAttached?.Invoke(this, new SessionAttachedEventArgs { Session = session });
    }
}
