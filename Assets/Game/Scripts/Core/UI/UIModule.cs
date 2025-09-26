using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.UI;
using UnityEngine;

namespace Framework.Core
{
    public class UIModule : IModule
    {
        private GameFramework _gameFramework;

        // 用非泛型 ViewBase 存储，避免泛型协变问题
        private readonly Dictionary<string, ViewBase> _views = new Dictionary<string, ViewBase>();
        private readonly Stack<ViewBase> _viewStack = new Stack<ViewBase>();

        private Transform _uiRoot;

        public void SetGameFramework(GameFramework gameFramework)
        {
            _gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            _uiRoot = _gameFramework.transform.Find("UI/UIRoot");
            LogModule.Log("UI module initialized");
        }

        // 加载并显示UI视图
        public async Task<TView> ShowViewAsync<TView, TViewModel>(string assetPath, TViewModel vmBase,
            bool pushToStack = true, object data = null)
            where TView : ViewBase<TViewModel>
            where TViewModel : ViewModelBase
        {
            if (_views.TryGetValue(assetPath, out var existingViewObj))
            {
                var existingView = existingViewObj as TView;
                if (existingView == null)
                {
                    LogModule.Error($"Cached view type mismatch for {assetPath}");
                    return null;
                }

                existingView.gameObject.SetActive(true);
                existingView.Bind(vmBase);
                existingView.OnShow(data);

                if (pushToStack && (_viewStack.Count == 0 || _viewStack.Peek() != existingView))
                {
                    _viewStack.Push(existingViewObj);
                }

                return existingView;
            }

            var viewObj = await _gameFramework.ResourceModule.InstantiatePrefabAsync(assetPath, _uiRoot);
            if (viewObj == null)
            {
                LogModule.Error($"Failed to load UI view: {assetPath}");
                return null;
            }

            var view = viewObj.GetComponent<TView>();
            if (view == null)
            {
                LogModule.Error($"UI view {assetPath} does not have a {typeof(TView)} component");
                GameObject.Destroy(viewObj);
                return null;
            }

            view.OnInitialize();
            view.Bind(vmBase);
            view.OnShow(data);

            _views.Add(assetPath, view);

            if (pushToStack)
            {
                _viewStack.Push(view);
            }

            LogModule.Log($"UI view shown: {assetPath}");
            return view;
        }

        // --------------------------
        // 通过 assetPath 操作的原版
        // --------------------------
        public void HideView(string assetPath, bool removeFromStack = true)
        {
            if (_views.TryGetValue(assetPath, out var view))
            {
                InternalHideView(view, removeFromStack);
                LogModule.Log($"UI view hidden: {assetPath}");
            }
            else
            {
                LogModule.Warning($"UI view not found: {assetPath}");
            }
        }

        public void CloseView(string assetPath)
        {
            if (_views.TryGetValue(assetPath, out var view))
            {
                InternalCloseView(assetPath, view);
            }
            else
            {
                LogModule.Warning($"UI view not found: {assetPath}");
            }
        }

        // --------------------------
        // 新增：通过 ViewModel 操作
        // --------------------------
        public void HideView(ViewModelBase viewModel, bool removeFromStack = true)
        {
            var (key, view) = FindViewByViewModel(viewModel);
            if (view == null)
            {
                LogModule.Warning($"HideView failed: View not found for {viewModel?.GetType().Name}");
                return;
            }

            InternalHideView(view, removeFromStack);
            LogModule.Log($"UI view hidden by ViewModel: {viewModel.GetType().Name}");
        }

        public void CloseView(ViewModelBase viewModel)
        {
            var (key, view) = FindViewByViewModel(viewModel);
            if (view == null)
            {
                LogModule.Warning($"CloseView failed: View not found for {viewModel?.GetType().Name}");
                return;
            }

            InternalCloseView(key, view);
            LogModule.Log($"UI view closed by ViewModel: {viewModel.GetType().Name}");
        }

        // --------------------------
        // 内部封装
        // --------------------------
        private void InternalHideView(ViewBase view, bool removeFromStack)
        {
            view.OnHide();
            view.gameObject.SetActive(false);

            if (removeFromStack && _viewStack.Contains(view))
            {
                var tempStack = new Stack<ViewBase>();
                while (_viewStack.Count > 0)
                {
                    var topView = _viewStack.Pop();
                    if (topView == view) break;
                    tempStack.Push(topView);
                }

                while (tempStack.Count > 0)
                {
                    _viewStack.Push(tempStack.Pop());
                }
            }
        }

        private void InternalCloseView(string key, ViewBase view)
        {
            InternalHideView(view, true);
            view.Cleanup();
            GameObject.Destroy(view.gameObject);
            _views.Remove(key);
        }

        private (string key, ViewBase view) FindViewByViewModel(ViewModelBase viewModel)
        {
            foreach (var pair in _views)
            {
                if (pair.Value.ViewModel == viewModel)
                {
                    return (pair.Key, pair.Value);
                }
            }

            return (null, null);
        }

        // 关闭栈顶视图
        public void CloseTopView()
        {
            if (_viewStack.Count > 0)
            {
                var topView = _viewStack.Pop();
                var assetPath = GetAssetPathForView(topView);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    CloseView(assetPath);
                }
            }
            else
            {
                LogModule.Warning("No views in stack to close");
            }
        }

        private string GetAssetPathForView(ViewBase view)
        {
            foreach (var pair in _views)
            {
                if (pair.Value == view) return pair.Key;
            }

            return null;
        }

        // --------------------------
        // 获取 View
        // --------------------------
        public T GetView<T>() where T : ViewBase
        {
            foreach (var view in _views.Values)
            {
                if (view is T tView) return tView;
            }

            return null;
        }

        public ViewBase GetView(ViewModelBase viewModel)
        {
            foreach (var view in _views.Values)
            {
                if (view.ViewModel == viewModel) return view;
            }

            return null;
        }

        // --------------------------
        // 生命周期
        // --------------------------
        public void OnUpdate(float deltaTime)
        {
            foreach (var view in _views.Values)
            {
                if (view.gameObject.activeSelf)
                {
                    view.OnUpdate(deltaTime);
                }
            }
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            foreach (var view in _views.Values)
            {
                if (view.gameObject.activeSelf)
                {
                    view.OnFixedUpdate(fixedDeltaTime);
                }
            }
        }

        public void OnLateUpdate(float deltaTime)
        {
            foreach (var view in _views.Values)
            {
                if (view.gameObject.activeSelf)
                {
                    view.OnLateUpdate(deltaTime);
                }
            }
        }

        public void OnShutdown()
        {
            var assetPaths = new List<string>(_views.Keys);
            foreach (var path in assetPaths)
            {
                CloseView(path);
            }

            _views.Clear();
            _viewStack.Clear();
            _uiRoot = null;

            LogModule.Log("UI module shutdown");
        }
    }
}