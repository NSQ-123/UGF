using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace pff.Homestead
{
    /// <summary>
    /// 配置工具类，可以用于生成本地配置数
    /// </summary>
    public class ConfigUtlis
    {
#if UNITY_EDITOR
        public static string ConfigPath = Application.dataPath + "/../";
#else
        public static string ConfigPath = Application.persistentDataPath + "/";
#endif



        private enum ValueType
        {
            Invalid,
            String,
            Int,
            Bool,
            Float
        }

        private string _path = null;
        private Dictionary<string, Dictionary<string, object>> _content = new Dictionary<string, Dictionary<string, object>>();
        private bool _isDirty = false;
        private string _defaultSection = "Default";
        private string[] _allLines = null;

        /// <summary>
        /// 配置名称：根据不同平台，此文件生成在不同的目录
        /// </summary>
        public ConfigUtlis(string configName, bool absolute = false)
        {
            if (!absolute)
            {
                _path = ConfigPath + configName;
            }
            else
            {
                _path = configName;
            }

            if (!File.Exists(_path))
            {
                File.Create(_path).Close();
            }

            _content.Clear();
            _allLines = File.ReadAllLines(_path);
            ParseContent(_allLines);
        }

        public ConfigUtlis(byte[] content)
        {
            _content.Clear();
            string text = System.Text.Encoding.UTF8.GetString(content);
            _allLines = text.Split(new char[] { '\n', '\r' });
            ParseContent(_allLines);
        }

        private void ParseContent(string[] lines)
        {
            string curSection = _defaultSection;
            for (int i = 0; i < lines.Length; i++)
            {
                string text = lines[i].Trim();

                if (text.Length != 0
                    && !text.StartsWith("#")
                    && !text.StartsWith("//")
                    && !string.IsNullOrEmpty(text))
                {
                    //if (text.Contains("//"))
                    //{
                    //    int desIndex = text.IndexOf("//");
                    //    text = text.Remove(desIndex);
                    //    text = text.Trim();
                    //}


                    // 3.15版本临时修改，针对德国地区会把float设为逗号导致进入游戏失败的问题，读配置时处理逗号
                    if (text.Contains("SoundValue"))
                    {
                        text = text.Replace(",", ".");
                    }

                    if (text.Contains("[") || text.Contains("]"))
                    {
                        curSection = text.Replace("[", "");
                        curSection = curSection.Replace("]", "");
                    }
                    else
                    {
                        int splitIndex = text.IndexOf('=');
                        if (splitIndex < 0)
                            continue;

                        string[] strs = new string[2];

                        strs[0] = text.Substring(0, splitIndex);
                        strs[1] = text.Substring(splitIndex + 1);

                        string key = strs[0].Trim();
                        object val = null;

                        string value = strs[1];

                        ValueType type = GetValueType(value);

                        int result = 0;
                        try
                        {
                            switch (type)
                            {
                                case ValueType.Int:
                                    val = int.Parse(value);
                                    break;
                                case ValueType.Bool:
                                    val = bool.Parse(value);
                                    break;
                                case ValueType.Float:
                                    val = float.Parse(value);
                                    break;
                                case ValueType.String:
                                    value = value.Remove(0, 1);
                                    val = value.Remove(value.Length - 1, 1);
                                    break;
                                default:
                                    Debug.LogError($"parse error1: [{text}].");
                                    result = -1;
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"parse error2: [{text}].");
                            result = -2;
                        }

                        if (result == 0)
                        {
                            if (!_content.ContainsKey(curSection))
                            {
                                _content[curSection] = new Dictionary<string, object>();
                            }
                            _content[curSection][key] = val;
                        }
                    }
                }
            }
        }

        public static int StringCount(string str, char c)
        {
            int ret = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    ret++;
                }
            }

            return ret;
        }

        private ValueType GetValueType(string str)
        {
            if (2 == StringCount(str, '"'))
            {
                return ValueType.String;
            }
            else if (0 == StringCount(str, '"'))
            {
                if (int.TryParse(str, out var _) == true)
                {
                    return ValueType.Int;
                }
                else if (float.TryParse(str, out var _) == true)
                {
                    return ValueType.Float;
                }
                else if (str.ToLower().Equals("true") || str.ToLower().Equals("false"))
                {
                    return ValueType.Bool;
                }
                else
                {
                    return ValueType.Invalid;
                }
            }
            else
            {
                return ValueType.Invalid;
            }
        }

        private void Save()
        {
            if (_isDirty)
            {
                _isDirty = false;

                if (string.IsNullOrEmpty(_path))
                {
                    return;
                }

                using (StreamWriter sw = new StreamWriter(_path, false))
                {
                    Dictionary<string, Dictionary<string, object>>.Enumerator _enumer = _content.GetEnumerator();
                    while (_enumer.MoveNext())
                    {
                        KeyValuePair<string, Dictionary<string, object>> section = _enumer.Current;

                        sw.WriteLine("[{0}]", section.Key);

                        Dictionary<string, object>.Enumerator _sectionContent = section.Value.GetEnumerator();
                        while (_sectionContent.MoveNext())
                        {
                            KeyValuePair<string, object> content = _sectionContent.Current;
                            
                            Type tp = content.Value.GetType();
                            if (tp == typeof(System.Int32) || tp == typeof(System.Boolean) || tp == typeof(System.Single))
                            {
                                sw.WriteLine("{0}={1}", content.Key, content.Value);
                            }
                            else if (tp == typeof(System.String))
                            {
                                sw.WriteLine("{0}=\"{1}\"", content.Key, content.Value);
                            }
                        }

                        sw.WriteLine();
                    }

                    sw.Flush();
                    //sw.Close();
                    //sw.Dispose();
                }
            }
        }

        private T Get<T>(string section, string key, T defaultValue = default(T))
        {
            Dictionary<string, object> sectionContents = null;
            if (_content.TryGetValue(section, out sectionContents))
            {
                object val = null;
                if (sectionContents.TryGetValue(key, out val))
                {
                    return (T)val;
                }
            }

            return defaultValue;
        }

        public bool Has(string section, string key)
        {
            Dictionary<string, object> sectionContents = null;
            if (_content.TryGetValue(section, out sectionContents))
            {
                object val = null;
                if (sectionContents.TryGetValue(key, out val))
                {
                    return true;
                }
            }

            return false;
        }

        public float GetFloat(string section, string key, float defaultValue = 0.0f)
        {
            return Get<float>(section, key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue = 0.0f)
        {
            return Get<float>(_defaultSection, key, defaultValue);
        }

        public int GetInt(string section, string key, int defaultValue = 0)
        {
            return Get<int>(section, key, defaultValue);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return Get<int>(_defaultSection, key, defaultValue);
        }
        public long GetLong(string section, string key, long defaultValue = 0)
        {
            return Get<long>(section, key, defaultValue);
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            return Get<long>(_defaultSection, key, defaultValue);
        }

        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            return Get<bool>(section, key, defaultValue);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return Get<bool>(_defaultSection, key, defaultValue);
        }

        public string GetString(string section, string key, string defaultValue = "")
        {
            return Get<string>(section, key, defaultValue);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return Get<string>(_defaultSection, key, defaultValue);
        }

        private void Set<T>(string section, string key, T value)
        {
            if (!_content.ContainsKey(section))
            {
                _content[section] = new Dictionary<string, object>();
            }

            if (!_content[section].ContainsKey(key) 
                    || (_content[section].ContainsKey(key) && !_IsEqual<T>(_content[section][key], value)))
            {
                _content[section][key] = value;
                _isDirty = true;
                Save();
            }
        }

        bool _IsEqual<T>(object s, T v)
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            {
                return float.Equals(s, v);
            }
            else
                return v.Equals((T)s);
        }

        public void SetFloat(string section, string key, float value)
        {
            Set<float>(section, key, value);
        }

        public void SetFloat(string key, float value)
        {
            Set<float>(_defaultSection, key, value);
        }

        public void SetInt(string section, string key, int value)
        {
            Set<int>(section, key, value);
        }

        public void SetInt(string key, int value)
        {
            Set<int>(_defaultSection, key, value);
        }

        public void SetLong(string section, string key, long value)
        {
            Set<long>(section, key, value);
        }

        public void SetLong(string key, long value)
        {
            Set<long>(_defaultSection, key, value);
        }
        public void SetBool(string section, string key, bool value)
        {
            Set<bool>(section, key, value);
        }

        public void SetBool(string key, bool value)
        {
            Set<bool>(_defaultSection, key, value);
        }

        public void SetString(string section, string key, string value)
        {
            if (null != value)
            {
                Set<string>(section, key, value);
            }
        }

        public void SetString(string key, string value)
        {
            Set<string>(_defaultSection, key, value);
        }

        public void Delete(string key)
        {
            Delete(_defaultSection, key);
        }

        public void Delete(string section, string key)
        {
            if (!_content.ContainsKey(section) || !_content[section].ContainsKey(key))
            {
                return;
            }

            _content[section].Remove(key);
            if (_content[section].Count == 0)
            {
                _content.Remove(section);
            }
            Save();
        }

        public Dictionary<string, object> GetSection(string section)
        {
            Dictionary<string, object> ret = null;

            _content.TryGetValue(section, out ret);

            return ret;
        }

        public string[] GetAllLines()
        {
            return _allLines;
        }

        public string[] GetSectionLines(string section)
        {
            List<string> tmp = new List<string>();
            bool find = false;
            string sectionStr = "[" + section + "]";
            for (int i = 0; i < _allLines.Length; i++)
            {
                if (find)
                {
                    if (_allLines[i].Contains("[") || _allLines[i].Contains("]"))
                        break;

                    tmp.Add(_allLines[i].Trim());
                }

                if (_allLines[i].Contains(sectionStr))
                {
                    find = true;
                }
            }

            return tmp.ToArray();
        }
    }
}
