using UnityEngine;
using System;
using System.ComponentModel;

namespace Framework.UI
{
    using UnityEngine;

    public abstract class ViewBase<TViewModel> : MonoBehaviour where TViewModel : ViewModelBase
    {
        public TViewModel ViewModel { get; private set; }
        protected bool _isInitialized = false;

        /// <summary>
        /// 由UIManager调用，用于绑定ViewModel
        /// </summary>
        public void Bind(TViewModel viewModel)
        {
            ViewModel = viewModel;

            // 订阅ViewModel的PropertyChanged事件来触发绑定更新
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;

            // 执行初始化绑定
            InitializeBindings();

            ViewModel.OnCreate();
        }

        /// <summary>
        /// 由UIManager调用，用于解绑
        /// </summary>
        public void Unbind()
        {
            if (ViewModel != null)
            {
                ViewModel.Cleanup();
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ReleaseBindings();
                ViewModel = null;
            }
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        // 显示视图
        public virtual void OnShow(object data)
        {
        }

        // 隐藏视图
        public virtual void OnHide()
        {
        }

        // 初始化回调
        protected internal virtual void OnInitialize()
        {
        }

        // 更新回调
        protected internal virtual void OnUpdate(float deltaTime)
        {
        }

        // 固定更新回调
        protected internal virtual void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        // 延迟更新回调
        protected internal virtual void OnLateUpdate(float deltaTime)
        {
        }

        // 清理资源
        public virtual void Cleanup()
        {
            Unbind();

            if (ViewModel != null)
            {
                // _viewModel.Cleanup();
                ViewModel = null;
            }

            _isInitialized = false;
        }

        /// <summary>
        /// 当ViewModel属性变化时被调用
        /// </summary>
        protected abstract void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e);

        /// <summary>
        /// 在此方法中设置所有初始绑定
        /// </summary>
        protected abstract void InitializeBindings();

        /// <summary>
        /// 在此方法中释放所有绑定和事件监听
        /// </summary>
        protected abstract void ReleaseBindings();
    }
}