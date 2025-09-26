using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Framework.UI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public virtual void OnCreate() { }

        public virtual void OnShow(object data) { }

        public virtual void OnHide() { }

        protected virtual void OnInitialize() { }

        public virtual void Cleanup() { }
    }
}