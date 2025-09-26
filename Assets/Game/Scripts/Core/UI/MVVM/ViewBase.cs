using UnityEngine;
using System.ComponentModel;

namespace Framework.UI
{
    // 非泛型基类，UIModule用它来存储和调用通用生命周期方法
    public abstract class ViewBase : MonoBehaviour
    {
        public ViewModelBase ViewModel { get; protected set; }   // ✅ 给基类加上通用 ViewModel 属性

        public abstract void OnShow(object data = null);
        public abstract void OnHide();
        protected internal abstract void OnInitialize();
        protected internal abstract void OnUpdate(float deltaTime);
        protected internal abstract void OnFixedUpdate(float fixedDeltaTime);
        protected internal abstract void OnLateUpdate(float deltaTime);
        public abstract void Cleanup();

        // 通用绑定接口（接受 ViewModelBase）
        public abstract void Bind(ViewModelBase vm);
    }

    // 泛型子类，具体持有 TViewModel
    public abstract class ViewBase<TViewModel> : ViewBase where TViewModel : ViewModelBase
    {
        public new TViewModel ViewModel { get; private set; }   // ✅ 隐藏基类属性，返回强类型

        protected bool _isInitialized = false;

        public override void Bind(ViewModelBase vm)
        {
            Bind(vm as TViewModel);
        }

        public void Bind(TViewModel viewModel)
        {
            ViewModel = viewModel;
            base.ViewModel = viewModel;   // ✅ 同步到基类属性，UIModule 用的时候不会报错

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            InitializeBindings();
            ViewModel.OnCreate();
        }

        public void Unbind()
        {
            if (ViewModel != null)
            {
                ViewModel.Cleanup();
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ReleaseBindings();
                ViewModel = null;
                base.ViewModel = null;
            }
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        public override void OnShow(object data) { }
        public override void OnHide() { }

        protected internal override void OnInitialize() { }
        protected internal override void OnUpdate(float deltaTime) { }
        protected internal override void OnFixedUpdate(float fixedDeltaTime) { }
        protected internal override void OnLateUpdate(float deltaTime) { }

        public override void Cleanup()
        {
            Unbind();
            _isInitialized = false;
        }

        protected abstract void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e);
        protected abstract void InitializeBindings();
        protected abstract void ReleaseBindings();
    }
}
