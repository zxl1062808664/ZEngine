namespace Framework.UI
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名 (由编译器自动填充)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值，如果值有变化则触发通知
        /// </summary>
        /// <returns>如果值被改变则返回 true</returns>
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // 视图模型生命周期方法
        public virtual void OnCreate() { } // 当ViewModel被创建时
        // public virtual void OnEnable() { } // 当绑定的View被显示时
        // public virtual void OnDisable() { } // 当绑定的View被隐藏时
        // public virtual void OnDestroy() { } // 当ViewModel被销毁时
        
        // 显示视图
        public virtual void OnShow(object data) { }

        // 隐藏视图
        public virtual void OnHide() { }

        // 初始化回调
        protected virtual void OnInitialize() { }

        // 清理资源
        public virtual void Cleanup() { }
    }
}