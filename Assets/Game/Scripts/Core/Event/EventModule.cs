using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Core.Events
{
    /// <summary>
    /// 游戏事件参数基类（关联发送体+外部传入eventId）
    /// </summary>
    public abstract class GameEventArgs : EventArgs
    {
        /// <summary>
        /// 事件发送体（发起事件的对象，不可为null）
        /// </summary>
        public object Sender { get; private set; }

        /// <summary>
        /// 事件ID（外部传入，需全局唯一）
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// 事件是否已被处理（标记为true后停止后续分发）
        /// </summary>
        public bool IsHandled { get; set; } = false;

        /// <summary>
        /// 构造函数（强制传入发送体和eventId）
        /// </summary>
        protected GameEventArgs(object sender, int eventId)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender), "事件发送体（sender）不能为null");
            if (eventId <= 0)
                throw new ArgumentException("EventId必须大于0，确保全局唯一性", nameof(eventId));
            
            ID = eventId;
        }
    }

    /// <summary>
    /// 泛型游戏事件参数（带数据载体）
    /// </summary>
    public class GameEventArgs<TData> : GameEventArgs
    {
        /// <summary>
        /// 事件携带的数据
        /// </summary>
        public TData Data { get; set; }

        /// <summary>
        /// 构造函数（发送体+eventId+数据）
        /// </summary>
        public GameEventArgs(object sender, int eventId, TData data) : base(sender, eventId)
        {
            Data = data;
        }

        /// <summary>
        /// 构造函数（发送体+eventId，无数据场景）
        /// </summary>
        public GameEventArgs(object sender, int eventId) : base(sender, eventId) { }
    }

    /// <summary>
    /// 非泛型自定义事件基类（无数据）
    /// </summary>
    public abstract class CustomGameEventArgs : GameEventArgs
    {
        protected CustomGameEventArgs(object sender, int eventId) : base(sender, eventId) { }
    }

    /// <summary>
    /// 事件处理函数包装类
    /// </summary>
    internal class EventHandlerWrapper
    {
        public EventHandler<GameEventArgs> Handler { get; }
        public bool IsOnce { get; }
        public object Owner { get; }

        public EventHandlerWrapper(EventHandler<GameEventArgs> handler, bool isOnce, object owner)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            IsOnce = isOnce;
            Owner = owner;
        }
    }

    /// <summary>
    /// 事件模块接口（发送事件需传入发送体）
    /// </summary>
    public interface IEventModule : IModule
    {
        int TotalHandlerCount { get; }
        int EventTypeCount { get; }
        int GetHandlerCount(int eventId);
        bool HasHandler(int eventId, EventHandler<GameEventArgs> handler);

        void Subscribe(int eventId, EventHandler<GameEventArgs> handler, bool isOnce = false, object owner = null);
        void Subscribe<TData>(int eventId, Action<object, GameEventArgs<TData>> handler, bool isOnce = false, object owner = null);
        
        void Unsubscribe(int eventId, EventHandler<GameEventArgs> handler);
        void UnsubscribeByOwner(object owner);
        void UnsubscribeAll(int eventId);
        
        void SetDefaultHandler(EventHandler<GameEventArgs> handler);
        
        void Fire(object sender, GameEventArgs e);
        void FireNow(object sender, GameEventArgs e);
        void Fire<TData>(object sender, int eventId, TData data);
        void FireNow<TData>(object sender, int eventId, TData data);
    }

    /// <summary>
    /// 事件模块实现（发送体优先+外部eventId）
    /// </summary>
    public class EventModule : IEventModule
    {
        private GameFramework _gameFramework;
        private readonly Dictionary<int, List<EventHandlerWrapper>> _eventPool = new Dictionary<int, List<EventHandlerWrapper>>();
        private readonly Dictionary<object, List<Tuple<int, EventHandler<GameEventArgs>>>> _ownerMap = new Dictionary<object, List<Tuple<int, EventHandler<GameEventArgs>>>>();
        private EventHandler<GameEventArgs> _defaultHandler;
        private readonly Queue<GameEventArgs> _eventQueue = new Queue<GameEventArgs>();
        private readonly Queue<EventHandlerWrapper> _handlerCache = new Queue<EventHandlerWrapper>();
        private readonly object _queueLock = new object();
        private int _totalHandlerCount;

        public static EventModule Instance => GameFramework.Instance?.GetModule<EventModule>();

        public int TotalHandlerCount => _totalHandlerCount;
        public int EventTypeCount => _eventPool.Count;

        public void SetGameFramework(GameFramework gameFramework)
        {
            _gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            LogModule.Log("EventModule initialized (sender-first mode)");
        }

        /// <summary>
        /// 验证eventId有效性
        /// </summary>
        private void ValidateEventId(int eventId)
        {
            if (eventId <= 0)
                throw new ArgumentOutOfRangeException(nameof(eventId), "EventId必须大于0");
        }

        /// <summary>
        /// 获取指定eventId的处理函数数量
        /// </summary>
        public int GetHandlerCount(int eventId)
        {
            ValidateEventId(eventId);
            return _eventPool.TryGetValue(eventId, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// 检查指定eventId是否已订阅处理函数
        /// </summary>
        public bool HasHandler(int eventId, EventHandler<GameEventArgs> handler)
        {
            if (handler == null) return false;
            ValidateEventId(eventId);

            if (_eventPool.TryGetValue(eventId, out var list))
            {
                foreach (var wrapper in list)
                {
                    if (wrapper.Handler == handler)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 订阅事件（通用版本）
        /// </summary>
        public void Subscribe(int eventId, EventHandler<GameEventArgs> handler, bool isOnce = false, object owner = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            ValidateEventId(eventId);

            if (HasHandler(eventId, handler))
            {
                LogModule.Warning($"EventId[{eventId}]已订阅处理函数[{handler.Method.Name}]，无需重复订阅");
                return;
            }

            if (!_eventPool.TryGetValue(eventId, out var list))
            {
                list = new List<EventHandlerWrapper>();
                _eventPool[eventId] = list;
            }

            var wrapper = new EventHandlerWrapper(handler, isOnce, owner);
            list.Add(wrapper);
            _totalHandlerCount++;

            if (owner != null)
            {
                if (!_ownerMap.TryGetValue(owner, out var ownerHandlers))
                {
                    ownerHandlers = new List<Tuple<int, EventHandler<GameEventArgs>>>();
                    _ownerMap[owner] = ownerHandlers;
                }
                ownerHandlers.Add(Tuple.Create(eventId, handler));
            }
        }

        /// <summary>
        /// 订阅泛型事件（带数据）
        /// </summary>
        public void Subscribe<TData>(int eventId, Action<object, GameEventArgs<TData>> handler, bool isOnce = false, object owner = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            ValidateEventId(eventId);

            EventHandler<GameEventArgs> genericHandler = (sender, e) =>
            {
                if (e is GameEventArgs<TData> dataArgs)
                    handler(sender, dataArgs);
            };

            Subscribe(eventId, genericHandler, isOnce, owner);
        }

        /// <summary>
        /// 取消订阅指定eventId的处理函数
        /// </summary>
        public void Unsubscribe(int eventId, EventHandler<GameEventArgs> handler)
        {
            if (handler == null)
            {
                LogModule.Warning("取消订阅的处理函数为null，操作忽略");
                return;
            }
            ValidateEventId(eventId);

            if (!_eventPool.TryGetValue(eventId, out var list))
            {
                LogModule.Warning($"EventId[{eventId}]无订阅记录，取消失败");
                return;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Handler == handler)
                {
                    var wrapper = list[i];
                    list.RemoveAt(i);
                    _totalHandlerCount--;

                    if (wrapper.Owner != null && _ownerMap.TryGetValue(wrapper.Owner, out var ownerHandlers))
                    {
                        ownerHandlers.RemoveAll(t => t.Item1 == eventId && t.Item2 == handler);
                        if (ownerHandlers.Count == 0)
                            _ownerMap.Remove(wrapper.Owner);
                    }

                    if (list.Count == 0)
                        _eventPool.Remove(eventId);

                    return;
                }
            }

            LogModule.Warning($"EventId[{eventId}]未找到指定处理函数，取消失败");
        }

        /// <summary>
        /// 按所有者批量取消订阅
        /// </summary>
        public void UnsubscribeByOwner(object owner)
        {
            if (owner == null) return;

            if (_ownerMap.TryGetValue(owner, out var handlers))
            {
                var handlersCopy = new List<Tuple<int, EventHandler<GameEventArgs>>>(handlers);
                foreach (var (id, handler) in handlersCopy)
                {
                    UnsubscribeInternal(id, handler);
                }
            }
        }

        /// <summary>
        /// 取消指定eventId的所有订阅
        /// </summary>
        public void UnsubscribeAll(int eventId)
        {
            ValidateEventId(eventId);
            if (!_eventPool.TryGetValue(eventId, out var list))
            {
                LogModule.Warning($"EventId[{eventId}]无订阅记录，无需取消");
                return;
            }

            var listCopy = new List<EventHandlerWrapper>(list);
            foreach (var wrapper in listCopy)
            {
                UnsubscribeInternal(eventId, wrapper.Handler);
            }
        }

        /// <summary>
        /// 内部取消订阅逻辑（复用代码）
        /// </summary>
        private void UnsubscribeInternal(int eventId, EventHandler<GameEventArgs> handler)
        {
            if (_eventPool.TryGetValue(eventId, out var list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Handler == handler)
                    {
                        var wrapper = list[i];
                        list.RemoveAt(i);
                        _totalHandlerCount--;

                        if (wrapper.Owner != null && _ownerMap.TryGetValue(wrapper.Owner, out var ownerHandlers))
                        {
                            ownerHandlers.RemoveAll(t => t.Item1 == eventId && t.Item2 == handler);
                            if (ownerHandlers.Count == 0)
                                _ownerMap.Remove(wrapper.Owner);
                        }

                        if (list.Count == 0)
                            _eventPool.Remove(eventId);

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 设置默认处理函数（无订阅者时执行）
        /// </summary>
        public void SetDefaultHandler(EventHandler<GameEventArgs> handler)
        {
            _defaultHandler = handler;
        }

        /// <summary>
        /// 延迟发布事件（下一帧处理，线程安全）
        /// </summary>
        public void Fire(object sender, GameEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender), "发送体（sender）不能为null");
            if (e == null)
                throw new ArgumentNullException(nameof(e), "事件参数不能为null");
            if (e.Sender != sender)
                throw new InvalidOperationException("事件参数中的Sender与传入的Sender不一致");

            lock (_queueLock)
            {
                _eventQueue.Enqueue(e);
            }
        }

        /// <summary>
        /// 简化版延迟发布（自动包装泛型事件）
        /// </summary>
        public void Fire<TData>(object sender, int eventId, TData data)
        {
            ValidateEventId(eventId);
            var eventArgs = new GameEventArgs<TData>(sender, eventId, data);
            Fire(sender, eventArgs);
        }

        /// <summary>
        /// 立即发布事件（同步执行）
        /// </summary>
        public void FireNow(object sender, GameEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender), "发送体（sender）不能为null");
            if (e == null)
                throw new ArgumentNullException(nameof(e), "事件参数不能为null");
            if (e.Sender != sender)
                throw new InvalidOperationException("事件参数中的Sender与传入的Sender不一致");

            HandleEvent(e);
        }

        /// <summary>
        /// 简化版立即发布（自动包装泛型事件）
        /// </summary>
        public void FireNow<TData>(object sender, int eventId, TData data)
        {
            ValidateEventId(eventId);
            var eventArgs = new GameEventArgs<TData>(sender, eventId, data);
            FireNow(sender, eventArgs);
        }

        /// <summary>
        /// 事件分发核心逻辑
        /// </summary>
        private void HandleEvent(GameEventArgs e)
        {
            List<EventHandlerWrapper> handlersToRemove = null;
            int eventId = e.ID;

            if (_eventPool.TryGetValue(eventId, out var handlers) && handlers.Count > 0)
            {
                // 缓存处理函数，避免遍历中修改原列表
                _handlerCache.Clear();
                foreach (var handler in handlers)
                    _handlerCache.Enqueue(handler);

                // 执行所有处理函数
                while (_handlerCache.Count > 0)
                {
                    var wrapper = _handlerCache.Dequeue();
                    try
                    {
                        // 传入事件发送体（e.Sender）和事件参数
                        wrapper.Handler.Invoke(e.Sender, e);

                        // 一次性事件标记移除
                        if (wrapper.IsOnce)
                        {
                            handlersToRemove ??= new List<EventHandlerWrapper>();
                            handlersToRemove.Add(wrapper);
                        }

                        // 事件已处理则停止分发
                        if (e.IsHandled)
                            break;
                    }
                    catch (Exception ex)
                    {
                        LogModule.Error($"处理EventId[{eventId}]时出错（发送体：{e.Sender.GetType().Name}）：{ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            // 无订阅者时执行默认处理函数
            else if (_defaultHandler != null)
            {
                try
                {
                    _defaultHandler.Invoke(e.Sender, e);
                }
                catch (Exception ex)
                {
                    LogModule.Error($"默认处理函数处理EventId[{eventId}]出错（发送体：{e.Sender.GetType().Name}）：{ex.Message}");
                }
            }
            else
            {
                LogModule.Warning($"EventId[{eventId}]无订阅者且无默认处理函数（发送体：{e.Sender.GetType().Name}）");
            }

            // 移除一次性事件的处理函数
            if (handlersToRemove != null)
            {
                foreach (var wrapper in handlersToRemove)
                {
                    UnsubscribeInternal(eventId, wrapper.Handler);
                }
            }
        }

        /// <summary>
        /// 每帧更新：处理延迟事件队列
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            while (true)
            {
                GameEventArgs eventArgs = null;
                lock (_queueLock)
                {
                    if (_eventQueue.Count > 0)
                        eventArgs = _eventQueue.Dequeue();
                    else
                        break;
                }

                if (eventArgs != null)
                    HandleEvent(eventArgs);
            }
        }

        public void OnFixedUpdate(float fixedDeltaTime) { }
        public void OnLateUpdate(float deltaTime) { }

        /// <summary>
        /// 模块关闭：清理所有资源
        /// </summary>
        public void OnShutdown()
        {
            _eventPool.Clear();
            _ownerMap.Clear();
            lock (_queueLock)
            {
                _eventQueue.Clear();
            }
            _handlerCache.Clear();
            _defaultHandler = null;
            _totalHandlerCount = 0;
            LogModule.Log("EventModule shutdown successfully");
        }
    }
}