using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

namespace Framework.Localization
{
    /// <summary>
    /// 本地化模块
    /// </summary>
    public class LocalizationModule : ModuleBase
    {
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private string _localizationDataPath = "Localization/";

        private string _currentLanguage;
        private Dictionary<string, string> _localizedStrings = new Dictionary<string, string>();
        private ResourceModule _resourceModule;

        public string CurrentLanguage => _currentLanguage;
        public event Action LanguageChanged;

        public override void OnInitialize()
        {
            base.OnInitialize();
            _resourceModule = GameFramework.GetModule<ResourceModule>();
            _currentLanguage = _defaultLanguage;

            // 尝试从PlayerPrefs加载保存的语言设置
            string savedLang = PlayerPrefs.GetString("CurrentLanguage", _defaultLanguage);
            SetLanguage(savedLang);
        }

        /// <summary>
        /// 设置当前语言
        /// </summary>
        public async void SetLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogError("Language code cannot be null or empty.");
                return;
            }

            // 如果语言没有变化，不做处理
            if (_currentLanguage == languageCode)
                return;

            // 加载语言数据
            string dataPath = $"{_localizationDataPath}{languageCode}";
            var loadResult = await _resourceModule.LoadAssetAsync(dataPath);

            if (loadResult == null)
            {
                Debug.LogWarning($"Failed to load localization data for {languageCode}, falling back to default.");

                // 加载默认语言
                if (languageCode != _defaultLanguage)
                {
                    SetLanguage(_defaultLanguage);
                }

                return;
            }

            // 解析语言数据
            var textAsset = loadResult.AssetObject as TextAsset;
            ParseLocalizationData(textAsset.text);
            _resourceModule.UnloadAsset(loadResult.GetAssetInfo());

            // 保存当前语言设置
            _currentLanguage = languageCode;
            PlayerPrefs.SetString("CurrentLanguage", languageCode);
            PlayerPrefs.Save();

            // 通知语言已更改
            LanguageChanged?.Invoke();
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        public string GetLocalizedString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (_localizedStrings.TryGetValue(key, out string value))
            {
                return value;
            }

            // 如果找不到对应翻译，返回原始键并添加警告
            Debug.LogWarning($"Localization key not found: {key} for language {_currentLanguage}");
            return key;
        }

        /// <summary>
        /// 解析本地化数据
        /// </summary>
        private void ParseLocalizationData(string data)
        {
            _localizedStrings.Clear();

            if (string.IsNullOrEmpty(data))
                return;

            // 简单的CSV格式解析
            string[] lines = data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                // 跳过注释行
                if (line.StartsWith("//"))
                    continue;

                string[] parts = line.Split(new[] { ',' }, 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim().Replace("\\\"", "\""); // 处理引号转义
                    _localizedStrings[key] = value;
                }
            }
        }

        public override void OnShutdown()
        {
            base.OnShutdown();
            _localizedStrings.Clear();
            LanguageChanged = null;
        }
    }
}