using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GF.Option
{
    /// <summary>
    /// 自定义INI格式配置文件存储
    /// </summary>
    public class IniConfigStorage : IConfigStorage
    {
        private string _filePath;
        private Dictionary<string, Dictionary<string, object>> _data;
        private bool _isDirty;

#if UNITY_EDITOR
        private static string ConfigPath => Application.dataPath + "/../";
#else
        private static string ConfigPath => Application.persistentDataPath + "/";
#endif

        public IniConfigStorage()
        {
            _data = new Dictionary<string, Dictionary<string, object>>();
            _isDirty = false;
        }

        public void Initialize(string path)
        {
            _filePath = System.IO.Path.Combine(ConfigPath, path);
            _data.Clear();
            
            LoadFromFile();
        }

        public T GetValue<T>(string section, string key, T defaultValue)
        {
            if (_data.TryGetValue(section, out var sectionData) && 
                sectionData.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public void SetValue<T>(string section, string key, T value)
        {
            if (!_data.TryGetValue(section, out var sectionData))
            {
                sectionData = new Dictionary<string, object>();
                _data[section] = sectionData;
            }

            object oldValue = sectionData.TryGetValue(key, out var existing) ? existing : null;
            if (!Equals(oldValue, value))
            {
                sectionData[key] = value;
                _isDirty = true;
                AutoSave();
            }
        }

        public void DeleteValue(string section, string key)
        {
            if (_data.TryGetValue(section, out var sectionData) && 
                sectionData.Remove(key))
            {
                if (sectionData.Count == 0)
                {
                    _data.Remove(section);
                }
                _isDirty = true;
                AutoSave();
            }
        }

        public void Save()
        {
            if (_isDirty)
            {
                SaveToFile();
                _isDirty = false;
            }
        }

        public string[] GetSectionLines(string section)
        {
            var lines = new List<string>();
            
            if (_data.TryGetValue(section, out var sectionData))
            {
                foreach (var kvp in sectionData)
                {
                    string valueStr = FormatValue(kvp.Value);
                    lines.Add($"{kvp.Key}={valueStr}");
                }
            }
            
            return lines.ToArray();
        }

        private void LoadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                // 创建空文件
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                File.WriteAllText(_filePath, "");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(_filePath);
                ParseContent(lines);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load config file {_filePath}: {ex.Message}");
            }
        }

        private void ParseContent(string[] lines)
        {
            string currentSection = "Default";
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // 跳过空行和注释
                if (string.IsNullOrEmpty(trimmedLine) || 
                    trimmedLine.StartsWith("#") || 
                    trimmedLine.StartsWith("//"))
                    continue;

                // 处理section
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    continue;
                }

                // 处理key=value
                int equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex > 0)
                {
                    string key = trimmedLine.Substring(0, equalIndex).Trim();
                    string valueStr = trimmedLine.Substring(equalIndex + 1).Trim();
                    
                    object value = ParseValue(valueStr);
                    
                    if (!_data.TryGetValue(currentSection, out var sectionData))
                    {
                        sectionData = new Dictionary<string, object>();
                        _data[currentSection] = sectionData;
                    }
                    
                    sectionData[key] = value;
                }
            }
        }

        private object ParseValue(string valueStr)
        {
            // 字符串值（带引号）
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\"") && valueStr.Length >= 2)
            {
                return valueStr.Substring(1, valueStr.Length - 2);
            }

            // 布尔值
            if (valueStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (valueStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            // 整数
            if (int.TryParse(valueStr, out int intValue))
                return intValue;

            // 长整数
            if (long.TryParse(valueStr, out long longValue))
                return longValue;

            // 浮点数
            if (float.TryParse(valueStr, out float floatValue))
                return floatValue;

            // 默认作为字符串
            return valueStr;
        }

        private string FormatValue(object value)
        {
            if (value == null)
                return "\"\"";

            Type type = value.GetType();
            
            if (type == typeof(string))
                return $"\"{value}\"";
            else if (type == typeof(bool))
                return value.ToString().ToLower();
            else if (type == typeof(float))
                return ((float)value).ToString("F6");
            else
                return value.ToString();
        }

        private void SaveToFile()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                
                using (var writer = new StreamWriter(_filePath, false))
                {
                    foreach (var section in _data)
                    {
                        // 写入section header
                        writer.WriteLine($"[{section.Key}]");
                        
                        // 写入key-value pairs
                        foreach (var kvp in section.Value)
                        {
                            string valueStr = FormatValue(kvp.Value);
                            writer.WriteLine($"{kvp.Key}={valueStr}");
                        }
                        
                        // section之间空一行
                        writer.WriteLine();
                    }
                }
                
#if UNITY_EDITOR
                // 在编辑器中同步到Assets目录
                string editorPath = Path.Combine(Application.dataPath, Path.GetFileName(_filePath));
                if (File.Exists(_filePath))
                {
                    File.Copy(_filePath, editorPath, true);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save config file {_filePath}: {ex.Message}");
            }
        }

        private void AutoSave()
        {
            // 自动保存，避免频繁的文件IO
            Save();
        }
    }
} 