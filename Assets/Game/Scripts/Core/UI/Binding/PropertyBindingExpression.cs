using System;
using System.ComponentModel;
using System.Reflection;
using Framework.Core;

namespace Framework.UI.MVVM.Binding
{
    public class PropertyBindingExpression : BindingExpression
    {
        private readonly BindingMode _mode;
        private readonly PropertyChangedEventHandler _sourcePropertyChanged;
        private readonly EventHandler _targetPropertyChanged;

        public PropertyBindingExpression(object source, string sourceProperty,
            object target, string targetProperty,
            BindingMode mode)
            : base(source, sourceProperty, target, targetProperty)
        {
            _mode = mode;

            // 源属性变更事件处理
            _sourcePropertyChanged = OnSourcePropertyChanged;

            // 如果源实现了INotifyPropertyChanged，则订阅其事件
            if (Source is INotifyPropertyChanged notifySource)
            {
                notifySource.PropertyChanged += _sourcePropertyChanged;
            }
            // 否则尝试订阅特定属性的变更事件（如"PropertyNameChanged"模式）
            else
            {
                SubscribeToPropertyChangeEvent(Source, SourceProperty, _targetPropertyChanged);
            }

            // 双向绑定或目标到源模式下，订阅目标的属性变更事件
            if (_mode == BindingMode.TwoWay || _mode == BindingMode.OneWayToSource)
            {
                _targetPropertyChanged = OnTargetPropertyChanged;

                if (Target is INotifyPropertyChanged notifyTarget)
                {
                    notifyTarget.PropertyChanged += _sourcePropertyChanged;
                }
                else
                {
                    SubscribeToPropertyChangeEvent(Target, TargetProperty, _targetPropertyChanged);
                }
            }

            // 初始同步
            if (_mode == BindingMode.OneWay || _mode == BindingMode.TwoWay)
            {
                SyncSourceToTarget();
            }
            else if (_mode == BindingMode.OneWayToSource)
            {
                SyncTargetToSource();
            }
        }

        // 源属性变更时同步到目标
        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SourceProperty || string.IsNullOrEmpty(e.PropertyName))
            {
                SyncSourceToTarget();
            }
        }

        // 目标属性变更时同步到源（双向绑定）
        private void OnTargetPropertyChanged(object sender, EventArgs e)
        {
            if (_mode == BindingMode.TwoWay || _mode == BindingMode.OneWayToSource)
            {
                SyncTargetToSource();
            }
        }

        // 将源属性值同步到目标
        public void SyncSourceToTarget()
        {
            if (IsDisposed) return;

            try
            {
                var value = GetValue(Source, SourceProperty);
                SetValue(Target, TargetProperty, value);
            }
            catch (Exception ex)
            {
                LogModule.Error($"Failed to sync source to target: {ex.Message}");
            }
        }

        // 将目标属性值同步到源
        public void SyncTargetToSource()
        {
            if (IsDisposed) return;

            try
            {
                var value = GetValue(Target, TargetProperty);
                SetValue(Source, SourceProperty, value);
            }
            catch (Exception ex)
            {
                LogModule.Error($"Failed to sync target to source: {ex.Message}");
            }
        }

        // 订阅特定属性的变更事件（如"PropertyNameChanged"模式）
        private void SubscribeToPropertyChangeEvent(object obj, string propertyName, EventHandler handler)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName) || handler == null)
                return;

            var eventName = $"{propertyName}Changed";
            var eventInfo = obj.GetType().GetEvent(eventName);

            if (eventInfo != null)
            {
                eventInfo.AddEventHandler(obj, handler);
            }
        }

        // 取消订阅特定属性的变更事件
        private void UnsubscribeFromPropertyChangeEvent(object obj, string propertyName, EventHandler handler)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName) || handler == null)
                return;

            var eventName = $"{propertyName}Changed";
            var eventInfo = obj.GetType().GetEvent(eventName);

            if (eventInfo != null)
            {
                eventInfo.RemoveEventHandler(obj, handler);
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
                // 取消订阅源事件
                if (Source is INotifyPropertyChanged notifySource)
                {
                    notifySource.PropertyChanged -= _sourcePropertyChanged;
                }
                else
                {
                    UnsubscribeFromPropertyChangeEvent(Source, SourceProperty, _targetPropertyChanged);
                }

                // 取消订阅目标事件
                if (_targetPropertyChanged != null)
                {
                    if (Target is INotifyPropertyChanged notifyTarget)
                    {
                        notifyTarget.PropertyChanged -= _sourcePropertyChanged;
                    }
                    else
                    {
                        UnsubscribeFromPropertyChangeEvent(Target, TargetProperty, _targetPropertyChanged);
                    }
                }
            }
        }
    }
}