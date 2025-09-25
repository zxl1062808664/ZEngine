// using Framework.Core;
// using Framework.UI;
// using UnityEngine;
//
// namespace Framework.Core
// {
//     public abstract class UIView : MonoBehaviour
//     {
//         [SerializeField] protected string _viewModelTypeName;
//         
//         protected ViewModelBase _viewModel;
//         protected bool _isInitialized = false;
//
//         public ViewModelBase ViewModel => _viewModel;
//
//         public void Initialize()
//         {
//             if (_isInitialized) return;
//
//             // 创建ViewModel实例
//             CreateViewModel();
//             
//             // 绑定ViewModel
//             if (_viewModel != null)
//             {
//                 // _viewModel.Initialize();
//                 BindViewModel();
//             }
//             else
//             {
//                 LogModule.Warning($"ViewModel not set for view: {gameObject.name}");
//             }
//
//             OnInitialize();
//             _isInitialized = true;
//         }
//
//         // 创建ViewModel实例
//         protected virtual void CreateViewModel()
//         {
//             if (string.IsNullOrEmpty(_viewModelTypeName)) return;
//
//             var type = System.Type.GetType(_viewModelTypeName);
//             if (type != null && typeof(ViewModelBase).IsAssignableFrom(type))
//             {
//                 _viewModel = System.Activator.CreateInstance(type) as ViewModelBase;
//                 _viewModel?.OnCreate();
//             }
//             else
//             {
//                 LogModule.Error($"Could not create ViewModel: {_viewModelTypeName}");
//             }
//         }
//
//         // 绑定ViewModel到View
//         protected abstract void BindViewModel();
//
//         // 解除绑定
//         protected virtual void UnbindViewModel()
//         {
//             if (this != null)
//             {
//                 // BindingManager.Instance.RemoveBindingsForTarget(this);
//             }
//         }
//
//         // 显示视图
//         public virtual void OnShow(object data) { }
//
//         // 隐藏视图
//         public virtual void OnHide() { }
//
//         // 初始化回调
//         protected virtual void OnInitialize() { }
//
//         // 更新回调
//         protected internal virtual void OnUpdate(float deltaTime) { }
//
//         // 固定更新回调
//         protected internal virtual void OnFixedUpdate(float fixedDeltaTime) { }
//
//         // 延迟更新回调
//         protected internal virtual void OnLateUpdate(float deltaTime) { }
//
//         // 清理资源
//         public virtual void Cleanup()
//         {
//             UnbindViewModel();
//             
//             if (_viewModel != null)
//             {
//                 // _viewModel.Cleanup();
//                 _viewModel = null;
//             }
//
//             _isInitialized = false;
//         }
//
//         protected virtual void OnDestroy()
//         {
//             Cleanup();
//         }
//     }
// }
