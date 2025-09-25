using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Core
{
    public class UIModule : IModule
    {
        private GameFramework _gameFramework;
        private readonly Dictionary<string, ViewBase<ViewModelBase>> _views = new Dictionary<string, ViewBase<ViewModelBase>>();
        private readonly Stack<ViewBase<ViewModelBase>> _viewStack = new Stack<ViewBase<ViewModelBase>>();
        private Transform _uiRoot;

        public void SetGameFramework(GameFramework gameFramework)
        {
            _gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            // 创建UI根节点
            _uiRoot = _gameFramework.transform.Find("UI/UIRoot");
            LogModule.Log("UI module initialized");
        }

        // 加载并显示UI视图
        public async Task<T> ShowViewAsync<T>(string assetPath,ViewModelBase vmBase, bool pushToStack = true, object data = null)
            where T : ViewBase<ViewModelBase>
        {
            // 检查视图是否已存在
            if (_views.TryGetValue(assetPath, out var existingView))
            {
                existingView.gameObject.SetActive(true);
                existingView.Bind(vmBase);
                existingView.OnShow(data);

                if (pushToStack && _viewStack.Count == 0 || _viewStack.Peek() != existingView)
                {
                    _viewStack.Push(existingView);
                }

                return existingView as T;
            }

            // 加载UI预制体
            var viewObj = await _gameFramework.ResourceModule.InstantiatePrefabAsync(assetPath, _uiRoot);
            if (viewObj == null)
            {
                LogModule.Error($"Failed to load UI view: {assetPath}");
                return null;
            }

            // 获取ViewBase<ViewModelBase>组件
            var view = viewObj.GetComponent<T>();
            if (view == null)
            {
                LogModule.Error($"UI view {assetPath} does not have a ViewBase<ViewModelBase> component");
                GameObject.Destroy(viewObj);
                return null;
            }

            // 初始化视图
            view.OnInitialize();
            view.OnShow(data);

            // 存储视图引用
            _views.Add(assetPath, view);

            // 推入栈
            if (pushToStack)
            {
                _viewStack.Push(view);
            }

            LogModule.Log($"UI view shown: {assetPath}");
            return view;
        }

        // 隐藏UI视图
        public void HideView(string assetPath, bool removeFromStack = true)
        {
            if (_views.TryGetValue(assetPath, out var view))
            {
                view.OnHide();
                view.gameObject.SetActive(false);

                if (removeFromStack && _viewStack.Contains(view))
                {
                    // 从栈中移除
                    var tempStack = new Stack<ViewBase<ViewModelBase>>();
                    while (_viewStack.Count > 0)
                    {
                        var topView = _viewStack.Pop();
                        if (topView == view)
                        {
                            break;
                        }

                        tempStack.Push(topView);
                    }

                    // 恢复其他视图
                    while (tempStack.Count > 0)
                    {
                        _viewStack.Push(tempStack.Pop());
                    }
                }

                LogModule.Log($"UI view hidden: {assetPath}");
            }
            else
            {
                LogModule.Warning($"UI view not found: {assetPath}");
            }
        }

        // 关闭并销毁UI视图
        public void CloseView(string assetPath)
        {
            if (_views.TryGetValue(assetPath, out var view))
            {
                HideView(assetPath);
                view.Cleanup();
                GameObject.Destroy(view.gameObject);
                _views.Remove(assetPath);

                LogModule.Log($"UI view closed: {assetPath}");
            }
            else
            {
                LogModule.Warning($"UI view not found: {assetPath}");
            }
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

        // 获取视图对应的资源路径
        private string GetAssetPathForView(ViewBase<ViewModelBase> view)
        {
            foreach (var pair in _views)
            {
                if (pair.Value == view)
                {
                    return pair.Key;
                }
            }

            return null;
        }

        // 获取指定类型的视图
        public T GetView<T>() where T : ViewBase<ViewModelBase>
        {
            foreach (var view in _views.Values)
            {
                if (view is T tView)
                {
                    return tView;
                }
            }

            return null;
        }

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
            // 关闭所有视图
            var assetPaths = new List<string>(_views.Keys);
            foreach (var path in assetPaths)
            {
                CloseView(path);
            }

            _views.Clear();
            _viewStack.Clear();

            if (_uiRoot != null)
            {
                GameObject.Destroy(_uiRoot.gameObject);
                _uiRoot = null;
            }

            LogModule.Log("UI module shutdown");
        }
    }
}