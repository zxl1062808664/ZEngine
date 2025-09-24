using System;
using System.Collections.Generic;
using System.Reflection;
using Framework.Core;

namespace Framework.UI.MVVM.Binding
{
    public enum BindingMode
    {
        OneWay,       // 源到目标
        TwoWay,       // 双向绑定
        OneWayToSource // 目标到源
    }

    public class BindingManager : IDisposable
    {
        private readonly List<BindingExpression> _bindings = new List<BindingExpression>();
        private bool _isDisposed;

        public static BindingManager Instance { get; } = new BindingManager();

        private BindingManager() { }

        // 创建属性绑定
        public PropertyBindingExpression BindProperty(object source, string sourceProperty,
                                                     object target, string targetProperty,
                                                     BindingMode mode = BindingMode.OneWay)
        {
            try
            {
                var binding = new PropertyBindingExpression(source, sourceProperty, target, targetProperty, mode);
                _bindings.Add(binding);
                return binding;
            }
            catch (Exception ex)
            {
                LogModule.Error($"Failed to create property binding: {ex.Message}");
                return null;
            }
        }

        // 创建命令绑定
        public CommandBindingExpression BindCommand(object source, string commandProperty,
                                                  object target, string eventName)
        {
            try
            {
                var binding = new CommandBindingExpression(source, commandProperty, target, eventName);
                _bindings.Add(binding);
                return binding;
            }
            catch (Exception ex)
            {
                LogModule.Error($"Failed to create command binding: {ex.Message}");
                return null;
            }
        }

        // 移除绑定
        public void RemoveBinding(BindingExpression binding)
        {
            if (binding == null) return;

            binding.Dispose();
            _bindings.Remove(binding);
        }

        // 移除与目标对象相关的所有绑定
        public void RemoveBindingsForTarget(object target)
        {
            if (target == null) return;

            for (int i = _bindings.Count - 1; i >= 0; i--)
            {
                var binding = _bindings[i];
                if (binding.Target == target)
                {
                    RemoveBinding(binding);
                }
            }
        }

        // 清理所有绑定
        public void Clear()
        {
            foreach (var binding in _bindings)
            {
                binding.Dispose();
            }
            _bindings.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Clear();
            }

            _isDisposed = true;
        }

        ~BindingManager()
        {
            Dispose(false);
        }
    }
}
