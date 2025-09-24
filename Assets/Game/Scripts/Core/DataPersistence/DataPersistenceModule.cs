using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Framework.Core;

namespace Framework.DataPersistence
{
    public enum EncryptionMode
    {
        None,
        Simple,
        Advanced
    }

    public interface IDataSerializer
    {
        string Serialize<T>(T data) where T : class;
        T Deserialize<T>(string data) where T : class;
    }

    public class JsonDataSerializer : IDataSerializer
    {
        public string Serialize<T>(T data) where T : class
        {
            return JsonUtility.ToJson(data);
        }

        public T Deserialize<T>(string data) where T : class
        {
            return JsonUtility.FromJson<T>(data);
        }
    }

    public class DataPersistenceModule : IModule
    {
        private GameFramework m_gameFramework;
        private IDataSerializer m_dataSerializer;
        private string m_dataPath;
        private EncryptionMode m_encryptionMode = EncryptionMode.Simple;
        private string m_encryptionKey = "DefaultKey"; // 实际项目中应使用更安全的密钥管理

        private Dictionary<Type, object> m_cachedData = new Dictionary<Type, object>();

        public void SetGameFramework(GameFramework gameFramework)
        {
            m_gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            // 设置数据保存路径
            m_dataPath = Application.persistentDataPath + "/Data/";
            
            // 确保目录存在
            if (!Directory.Exists(m_dataPath))
            {
                Directory.CreateDirectory(m_dataPath);
            }

            // 默认使用JSON序列化器
            m_dataSerializer = new JsonDataSerializer();

            Framework.Core.LogModule.Log($"Data persistence module initialized. Data path: {m_dataPath}");
        }

        public void SetSerializer(IDataSerializer serializer)
        {
            m_dataSerializer = serializer ?? new JsonDataSerializer();
        }

        public void SetEncryptionMode(EncryptionMode mode, string key = null)
        {
            m_encryptionMode = mode;
            
            if (!string.IsNullOrEmpty(key))
            {
                m_encryptionKey = key;
            }
        }

        public async Task SaveDataAsync<T>(T data, string fileName = null) where T : class
        {
            if (data == null)
            {
                Framework.Core.LogModule.Warning("Trying to save null data");
                return;
            }

            try
            {
                // 获取文件名
                string file = GetFileName<T>(fileName);
                string path = Path.Combine(m_dataPath, file);

                // 序列化数据
                string dataString = m_dataSerializer.Serialize(data);

                // 加密数据
                if (m_encryptionMode != EncryptionMode.None)
                {
                    dataString = Encrypt(dataString);
                }

                // 保存数据
                await File.WriteAllTextAsync(path, dataString);

                // 更新缓存
                m_cachedData[typeof(T)] = data;

                Framework.Core.LogModule.Log($"Data saved successfully: {path}");
            }
            catch (Exception e)
            {
                Framework.Core.LogModule.Error($"Error saving data: {e.Message}");
            }
        }

        public async Task<T> LoadDataAsync<T>(string fileName = null) where T : class, new()
        {
            try
            {
                // 检查缓存
                if (m_cachedData.TryGetValue(typeof(T), out object cachedData))
                {
                    return cachedData as T;
                }

                // 获取文件名
                string file = GetFileName<T>(fileName);
                string path = Path.Combine(m_dataPath, file);

                // 检查文件是否存在
                if (!File.Exists(path))
                {
                    Framework.Core.LogModule.Log($"Data file not found, returning new instance: {path}");
                    T newData = new T();
                    m_cachedData[typeof(T)] = newData;
                    return newData;
                }

                // 读取数据
                string dataString = await File.ReadAllTextAsync(path);

                // 解密数据
                if (m_encryptionMode != EncryptionMode.None)
                {
                    dataString = Decrypt(dataString);
                }

                // 反序列化数据
                T data = m_dataSerializer.Deserialize<T>(dataString);
                if (data == null)
                {
                    data = new T();
                }

                // 更新缓存
                m_cachedData[typeof(T)] = data;

                Framework.Core.LogModule.Log($"Data loaded successfully: {path}");
                return data;
            }
            catch (Exception e)
            {
                Framework.Core.LogModule.Error($"Error loading data: {e.Message}");
                T fallbackData = new T();
                m_cachedData[typeof(T)] = fallbackData;
                return fallbackData;
            }
        }

        public bool DeleteData<T>(string fileName = null)
        {
            try
            {
                string file = GetFileName<T>(fileName);
                string path = Path.Combine(m_dataPath, file);

                if (File.Exists(path))
                {
                    File.Delete(path);
                    m_cachedData.Remove(typeof(T));
                    Framework.Core.LogModule.Log($"Data deleted: {path}");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Framework.Core.LogModule.Error($"Error deleting data: {e.Message}");
                return false;
            }
        }

        public T GetCachedData<T>() where T : class
        {
            if (m_cachedData.TryGetValue(typeof(T), out object data))
            {
                return data as T;
            }
            return null;
        }

        public void ClearCache()
        {
            m_cachedData.Clear();
            Framework.Core.LogModule.Log("Data cache cleared");
        }

        private string GetFileName<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return typeof(T).Name + ".dat";
            }
            return fileName.EndsWith(".dat") ? fileName : fileName + ".dat";
        }

        private string Encrypt(string plainText)
        {
            if (m_encryptionMode == EncryptionMode.Simple)
            {
                // 简单加密 - 实际项目中应使用更安全的加密方式
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                return Convert.ToBase64String(plainBytes);
            }
            else // Advanced
            {
                // 高级加密示例
                using (Aes aesAlg = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(m_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    aesAlg.Key = pdb.GetBytes(32);
                    aesAlg.IV = pdb.GetBytes(16);

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            if (m_encryptionMode == EncryptionMode.Simple)
            {
                // 简单解密
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                return Encoding.UTF8.GetString(cipherBytes);
            }
            else // Advanced
            {
                // 高级解密示例
                using (Aes aesAlg = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(m_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    aesAlg.Key = pdb.GetBytes(32);
                    aesAlg.IV = pdb.GetBytes(16);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        public void OnUpdate(float deltaTime) { }
        public void OnFixedUpdate(float fixedDeltaTime) { }
        public void OnLateUpdate(float deltaTime) { }

        public void OnShutdown()
        {
            ClearCache();
            m_dataSerializer = null;
        }
    }
}
