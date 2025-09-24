using System;
using System.Reflection;

namespace Framework.UI.MVVM.Binding
{
    public abstract class BindingExpression : IDisposable
    {
        protected object Source { get; }
        protected string SourceProperty { get; }
        public object Target { get; }
        protected string TargetProperty { get; }

        protected bool IsDisposed { get; private set; }

        public BindingExpression(object source, string sourceProperty, object target, string targetProperty)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            SourceProperty = sourceProperty ?? throw new ArgumentNullException(nameof(sourceProperty));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));

            // 验证源属性是否存在
            var sourceType = source.GetType();
            if (sourceType.GetProperty(sourceProperty) == null && sourceType.GetField(sourceProperty) == null)
            {
                throw new ArgumentException($"Source property {sourceProperty} not found on type {sourceType.Name}");
            }

            // 验证目标属性是否存在
            var targetType = target.GetType();
            if (targetType.GetProperty(targetProperty) == null && targetType.GetField(targetProperty) == null)
            {
                throw new ArgumentException($"Target property {targetProperty} not found on type {targetType.Name}");
            }
        }

        protected object GetValue(object obj, string propertyName)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(obj);
            }

            var field = type.GetField(propertyName);
            if (field != null)
            {
                return field.GetValue(obj);
            }

            return null;
        }

        protected void SetValue(object obj, string propertyName, object value)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var property = type.GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                // 尝试转换值类型以匹配属性类型
                var targetType = property.PropertyType;
                value = Convert.ChangeType(value, targetType);
                property.SetValue(obj, value);
                return;
            }

            var field = type.GetField(propertyName);
            if (field != null && !field.IsInitOnly)
            {
                // 尝试转换值类型以匹配字段类型
                var targetType = field.FieldType;
                value = Convert.ChangeType(value, targetType);
                field.SetValue(obj, value);
            }
        }

        public abstract void Dispose();

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            IsDisposed = true;
        }
    }
}
