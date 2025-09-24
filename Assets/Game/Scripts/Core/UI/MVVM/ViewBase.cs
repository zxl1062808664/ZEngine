using UnityEngine;
using System;

namespace Framework.UI.MVVM
{
    public abstract class ViewBase : MonoBehaviour
    {
        [SerializeField] protected string m_viewModelType;
        
        protected ViewModelBase m_viewModel;
        private bool m_isBinding = false;

        protected virtual void Awake()
        {
            FindViewModel();
        }

        protected virtual void OnEnable()
        {
            if (m_viewModel != null && !m_isBinding)
            {
                BindViewModel();
                m_isBinding = true;
            }
        }

        protected virtual void OnDisable()
        {
            if (m_viewModel != null && m_isBinding)
            {
                UnbindViewModel();
                m_isBinding = false;
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_viewModel != null)
            {
                UnbindViewModel();
                m_viewModel.Cleanup();
                m_viewModel = null;
            }
        }

        private void FindViewModel()
        {
            if (string.IsNullOrEmpty(m_viewModelType))
                return;

            Type type = Type.GetType(m_viewModelType);
            if (type == null)
            {
                Framework.Core.LogModule.Error($"ViewModel type {m_viewModelType} not found");
                return;
            }

            m_viewModel = Activator.CreateInstance(type) as ViewModelBase;
            if (m_viewModel != null)
            {
                m_viewModel.Initialize();
            }
            else
            {
                Framework.Core.LogModule.Error($"Failed to create ViewModel instance for {m_viewModelType}");
            }
        }

        public void SetViewModel(ViewModelBase viewModel)
        {
            if (m_viewModel != null && m_isBinding)
            {
                UnbindViewModel();
                m_viewModel.Cleanup();
            }

            m_viewModel = viewModel;
            m_viewModel?.Initialize();

            if (m_viewModel != null && isActiveAndEnabled)
            {
                BindViewModel();
                m_isBinding = true;
            }
        }

        protected abstract void BindViewModel();
        protected abstract void UnbindViewModel();
    }
}
