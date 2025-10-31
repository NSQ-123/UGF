using System;
using System.Collections.Generic;
using UnityEngine;

namespace GF.Option
{
    /// <summary>
    /// 增强版配置管理类，支持验证、回调等高级功能
    /// </summary>
    public class EnhancedOptions : IOptionService
    {
        const string LOCAL_PATH = "local_options.txt";

        private Dictionary<OptionKey, object> _options = new Dictionary<OptionKey, object>();
        private Dictionary<long, Dictionary<OptionKey, object>> _playerUniqueOptions = new Dictionary<long, Dictionary<OptionKey, object>>();
        private IConfigStorage _storage;
        
        public bool IsInited { get; private set; }

        public EnhancedOptions(IConfigStorage storage = null)
        {
            _storage = storage ?? new IniConfigStorage();
        }

        public void Initialize()
        {
            if (IsInited) return;
            
            Clear();
            _storage.Initialize(LOCAL_PATH);
            
            bool success = LoadLocalOptions();
            if (!success)
            {
                Debug.LogWarning("LoadLocalOptions Failed!");
            }
            
            ApplyAllCallBack();
            IsInited = true;
        }

        private void Clear()
        {
            _options.Clear();
            _playerUniqueOptions.Clear();
        }

        public bool Has(OptionKey key)
        {
            return _options.ContainsKey(key);
        }

        public void Delete(OptionKey key)
        {
            if (!_options.Remove(key)) return;
            
            string section = key.IsPlayerUnique ? "Player" : "Default";
            _storage.DeleteValue(section, key.Key);
        }

        public T Get<T>(OptionKey key)
        {
            if (!_options.TryGetValue(key, out var obj))
            {
                var defaultValue = GetDefaultValue<T>(key);
                Set(key, defaultValue);
                return defaultValue;
            }
            return (T)obj;
        }

        public void Set(OptionKey key, object value, bool needCallBack = false)
        {
            if (_storage == null) return;

            // 获取元数据进行验证
            var metadata = OptionRegistry.GetMetadata(key);
            if (metadata != null)
            {
                // 只读检查
                if (metadata.IsReadOnly)
                {
                    Debug.LogWarning($"尝试修改只读配置项: {key}");
                    return;
                }

                // 值验证
                if (metadata.Validator != null && !metadata.Validator(value))
                {
                    Debug.LogWarning($"配置项 {key} 的值 {value} 未通过验证");
                    return;
                }
            }

            // 检查值是否实际改变
            if (_options.TryGetValue(key, out var currentValue))
            {
                if (Equals(currentValue, value)) return;
            }

            // 设置新值
            _options[key] = value;
            
            // 保存到存储
            string section = key.IsPlayerUnique ? "Player" : "Default";
            Type valueType = metadata?.ValueType ?? value?.GetType() ?? typeof(object);
            SaveValue(section, key.Key, value, valueType);

            // 触发效果和回调
            if (needCallBack || metadata?.OnValueChanged != null)
            {
                ApplyOptionCallBack(key, value);
            }
        }

        public TValue GetPlayerUniqueConfig<TValue>(long playerId, OptionKey key)
        {
            if (_playerUniqueOptions.TryGetValue(playerId, out var playerOptions) && 
                playerOptions.TryGetValue(key, out var value))
            {
                return (TValue)value;
            }

            if (!LoadPlayerUniqueOptions(playerId, out playerOptions))
            {
                playerOptions = new Dictionary<OptionKey, object>();
                _playerUniqueOptions[playerId] = playerOptions;
            }

            if (playerOptions.TryGetValue(key, out value))
            {
                return (TValue)value;
            }

            var defaultValue = GetPlayerUniqueDefaultValue<TValue>(key);
            SetPlayerUniqueConfig(playerId, key, defaultValue);
            return defaultValue;
        }

        public void SetPlayerUniqueConfig(long playerId, OptionKey key, object value)
        {
            // 获取元数据进行验证
            var metadata = OptionRegistry.GetMetadata(key);
            if (metadata != null)
            {
                // 只读检查
                if (metadata.IsReadOnly)
                {
                    Debug.LogWarning($"尝试修改只读玩家配置项: {key}");
                    return;
                }

                // 值验证
                if (metadata.Validator != null && !metadata.Validator(value))
                {
                    Debug.LogWarning($"玩家配置项 {key} 的值 {value} 未通过验证");
                    return;
                }
            }

            if (!_playerUniqueOptions.TryGetValue(playerId, out var playerOptions))
            {
                playerOptions = new Dictionary<OptionKey, object>();
                _playerUniqueOptions[playerId] = playerOptions;
            }

            // 检查值是否实际改变
            if (playerOptions.TryGetValue(key, out var currentValue))
            {
                if (Equals(currentValue, value)) return;
            }

            playerOptions[key] = value;

            // 保存到存储
            var section = $"Player_{playerId}";
            Type valueType = metadata?.ValueType ?? value?.GetType() ?? typeof(object);
            SaveValue(section, key.Key, value, valueType);

            // 触发回调
            metadata?.OnValueChanged?.Invoke(value);
        }

        private void SaveValue(string section, string key, object value, Type valueType)
        {
            if (valueType == typeof(bool))
                _storage.SetValue(section, key, (bool)value);
            else if (valueType == typeof(int))
                _storage.SetValue(section, key, (int)value);
            else if (valueType == typeof(float))
                _storage.SetValue(section, key, (float)value);
            else if (valueType == typeof(long))
                _storage.SetValue(section, key, (long)value);
            else
                _storage.SetValue(section, key, value?.ToString() ?? "");
        }

        private T GetDefaultValue<T>(OptionKey key)
        {
            var metadata = OptionRegistry.GetMetadata(key);
            if (metadata?.DefaultValue != null)
                return (T)metadata.DefaultValue;

            return default(T);
        }

        private T GetPlayerUniqueDefaultValue<T>(OptionKey key)
        {
            var metadata = OptionRegistry.GetMetadata(key);
            if (metadata?.DefaultValue != null)
                return (T)metadata.DefaultValue;

            return default(T);
        }

        private bool LoadLocalOptions()
        {
            var lines = _storage.GetSectionLines("Default");
            return LoadAllLines(lines, _options, GetOptionTypeMap());
        }

        private bool LoadPlayerUniqueOptions(long playerId, out Dictionary<OptionKey, object> playerOptions)
        {
            playerOptions = new Dictionary<OptionKey, object>();
            _playerUniqueOptions[playerId] = playerOptions;
            
            var section = $"Player_{playerId}";
            var lines = _storage.GetSectionLines(section);
            return LoadAllLines(lines, playerOptions, GetOptionTypeMap());
        }

        private void ApplyAllCallBack()
        {
            foreach (var kvp in _options)
            {
                ApplyOptionCallBack(kvp.Key, kvp.Value);
            }
        }

        private void ApplyOptionCallBack(OptionKey optionKey, object value)
        {
            var metadata = OptionRegistry.GetMetadata(optionKey);
            metadata?.OnValueChanged?.Invoke(value);
        }

        private static Dictionary<OptionKey, Type> GetOptionTypeMap()
        {
            var typeMap = new Dictionary<OptionKey, Type>();
            foreach (var optionKey in OptionRegistry.GetAllOptionKeys())
            {
                var metadata = OptionRegistry.GetMetadata(optionKey);
                if (metadata?.ValueType != null)
                {
                    typeMap[optionKey] = metadata.ValueType;
                }
            }
            return typeMap;
        }

        private static bool LoadAllLines<TKey>(string[] lines, Dictionary<TKey, object> targetDic, Dictionary<TKey, Type> typeDic)
        {
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (LoadLine(trimmedLine, typeDic, out TKey key, out object value))
                {
                    targetDic[key] = value;
                }
                else
                {
                    Debug.LogWarning($"Failed to load option line: {line}");
                }
            }
            return true;
        }

        private static bool LoadLine<TKey>(string line, Dictionary<TKey, Type> typeDic, out TKey key, out object value)
        {
            key = default(TKey);
            value = null;

            int equalPos = line.IndexOf('=');
            if (equalPos == -1) return false;

            string keyStr = line.Substring(0, equalPos).Trim();
            string valueStr = line.Substring(equalPos + 1).Trim();

            // 如果TKey是OptionKey，直接创建
            if (typeof(TKey) == typeof(OptionKey))
            {
                var optionKey = new OptionKey(keyStr);
                key = (TKey)(object)optionKey;
            }
            else
            {
                try
                {
                    key = (TKey)Enum.Parse(typeof(TKey), keyStr, true);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            if (!typeDic.TryGetValue(key, out Type type)) 
            {
                // 如果类型字典中没有找到，尝试从值推断类型
                if (typeof(TKey) == typeof(OptionKey))
                {
                    type = InferValueType(valueStr);
                }
                else
                {
                    return true;
                }
            }

            try
            {
                if (type == typeof(bool))
                    value = bool.Parse(valueStr);
                else if (type == typeof(int))
                    value = int.Parse(valueStr);
                else if (type == typeof(float))
                    value = float.Parse(valueStr);
                else if (type == typeof(long))
                    value = long.Parse(valueStr);
                else if (type == typeof(string))
                    value = valueStr.Trim('"');
                else
                    return false;

                return true;
            }
            catch (Exception)
            {
                Debug.LogError($"Failed to parse value '{valueStr}' for key '{keyStr}' as type {type}");
                return false;
            }
        }

        private static Type InferValueType(string valueStr)
        {
            // 字符串值（带引号）
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                return typeof(string);

            // 布尔值
            if (valueStr.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                valueStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                return typeof(bool);

            // 整数
            if (int.TryParse(valueStr, out _))
                return typeof(int);

            // 长整数
            if (long.TryParse(valueStr, out _))
                return typeof(long);

            // 浮点数
            if (float.TryParse(valueStr, out _))
                return typeof(float);

            // 默认作为字符串
            return typeof(string);
        }
    }
} 