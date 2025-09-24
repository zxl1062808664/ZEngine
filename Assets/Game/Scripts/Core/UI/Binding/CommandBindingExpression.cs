using System;
using System.Reflection;
using Framework.Core;
using Framework.UI.MVVM.Commands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Framework.UI.MVVM.Binding
{
    public class CommandBindingExpression : BindingExpression
    {
        private readonly ICommand _command;
        private Delegate _eventHandler;
        private Delegate _unityEventListener;

        public CommandBindingExpression(object source, string commandProperty, object target, string eventName)
            : base(source, commandProperty, target, eventName)
        {
            // 获取命令属性
            var command = GetValue(Source, SourceProperty) as ICommand;
            if (command == null)
            {
                throw new InvalidOperationException($"Property {commandProperty} is not an ICommand");
            }
            _command = command;

            // 订阅命令的CanExecuteChanged事件
            _command.CanExecuteChanged += OnCommandCanExecuteChanged;

            // 绑定目标事件
            BindTargetEvent(target, eventName);

            // 初始更新命令状态
            UpdateCommandState();
        }

        private void BindTargetEvent(object target, string eventName)
        {
            var targetType = target.GetType();
            
            // 特殊处理Button的onClick事件
            if (target is Button button && eventName == "onClick")
            {
                BindButtonClickEvent(button);
                return;
            }

            // 处理其他UnityEvent字段
            var eventField = targetType.GetField(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (eventField != null && typeof(UnityEventBase).IsAssignableFrom(eventField.FieldType))
            {
                BindUnityEventField(target, eventField);
                return;
            }

            // 处理标准C#事件
            var eventInfo = targetType.GetEvent(eventName);
            if (eventInfo == null)
            {
                throw new ArgumentException($"Event {eventName} not found on target {targetType.Name}");
            }

            _eventHandler = CreateEventHandler(eventInfo.EventHandlerType);
            eventInfo.AddEventHandler(target, _eventHandler);
        }

        private void BindButtonClickEvent(Button button)
        {
            // 使用公开的UnityAction版本AddListener
            _unityEventListener = new UnityAction(OnEventRaised);
            button.onClick.AddListener((UnityAction)_unityEventListener);
            _eventHandler = _unityEventListener;
        }

        private void BindUnityEventField(object target, FieldInfo eventField)
        {
            var unityEvent = eventField.GetValue(target) as UnityEventBase;
            if (unityEvent == null)
                throw new InvalidOperationException($"Field {eventField.Name} is not a UnityEvent");

            // 处理非泛型UnityEvent
            if (unityEvent is UnityEvent nonGenericEvent)
            {
                _unityEventListener = new UnityAction(OnEventRaised);
                nonGenericEvent.AddListener((UnityAction)_unityEventListener);
            }
            // 处理泛型UnityEvent<T>
            else if (unityEvent.GetType().IsGenericType)
            {
                var genericType = unityEvent.GetType().GetGenericArguments()[0];
                var actionType = typeof(UnityAction<>).MakeGenericType(genericType);
                
                // 创建匹配的UnityAction<T>
                var methodInfo = GetType().GetMethod(nameof(OnEventRaisedWithParameter), 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var genericMethod = methodInfo.MakeGenericMethod(genericType);
                _unityEventListener = Delegate.CreateDelegate(actionType, this, genericMethod);

                // 使用公开的AddListener方法（关键修复）
                var addListenerMethod = unityEvent.GetType().GetMethod("AddListener");
                if (addListenerMethod != null)
                {
                    addListenerMethod.Invoke(unityEvent, new[] { _unityEventListener });
                }
                else
                {
                    throw new InvalidOperationException($"No public AddListener method found for {unityEvent.GetType().Name}");
                }
            }

            _eventHandler = _unityEventListener;
        }

        private Delegate CreateEventHandler(Type eventHandlerType)
        {
            if (eventHandlerType == typeof(EventHandler))
            {
                return new EventHandler((s, e) => OnEventRaised());
            }

            var invokeMethod = eventHandlerType.GetMethod("Invoke");
            if (invokeMethod != null)
            {
                var parameters = invokeMethod.GetParameters();
                if (parameters.Length == 2)
                {
                    return Delegate.CreateDelegate(eventHandlerType, this, 
                        GetType().GetMethod(nameof(OnStandardEventRaised)));
                }
            }

            throw new NotSupportedException($"Event handler type {eventHandlerType.Name} is not supported");
        }

        private void OnEventRaised()
        {
            if (_command.CanExecute(null))
            {
                _command.Execute(null);
            }
        }

        private void OnEventRaisedWithParameter<T>(T parameter)
        {
            if (_command.CanExecute(parameter))
            {
                _command.Execute(parameter);
            }
        }

        private void OnStandardEventRaised(object sender, EventArgs e)
        {
            if (_command.CanExecute(e))
            {
                _command.Execute(e);
            }
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCommandState();
        }

        private void UpdateCommandState()
        {
            try
            {
                if (Target is Button button)
                {
                    button.interactable = _command.CanExecute(null);
                    return;
                }

                var propertyInfo = Target.GetType().GetProperty("interactable");
                if (propertyInfo != null && propertyInfo.CanWrite && propertyInfo.PropertyType == typeof(bool))
                {
                    propertyInfo.SetValue(Target, _command.CanExecute(null));
                }
            }
            catch (Exception ex)
            {
                LogModule.Warning($"Failed to update command state: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            if (disposing)
            {
                _command.CanExecuteChanged -= OnCommandCanExecuteChanged;

                // 针对Button的解绑
                if (Target is Button button && TargetProperty == "onClick" && _unityEventListener is UnityAction action)
                {
                    button.onClick.RemoveListener(action);
                }
                // 针对其他UnityEvent的解绑
                else if (_unityEventListener != null)
                {
                    var eventField = Target.GetType().GetField(TargetProperty, BindingFlags.Public | BindingFlags.Instance);
                    if (eventField != null)
                    {
                        var unityEvent = eventField.GetValue(Target) as UnityEventBase;
                        if (unityEvent != null)
                        {
                            // 使用公开的RemoveListener方法
                            var removeMethod = unityEvent.GetType().GetMethod("RemoveListener");
                            if (removeMethod != null)
                            {
                                removeMethod.Invoke(unityEvent, new[] { _unityEventListener });
                            }
                        }
                    }
                }
                // 针对标准事件的解绑
                else if (_eventHandler != null)
                {
                    var eventInfo = Target.GetType().GetEvent(TargetProperty);
                    if (eventInfo != null)
                    {
                        eventInfo.RemoveEventHandler(Target, _eventHandler);
                    }
                }

                _unityEventListener = null;
                _eventHandler = null;
            }
        }
    }
}
    