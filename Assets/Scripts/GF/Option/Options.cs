
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace pff.Homestead
{
    /// <summary>
    /// 选项配置功能，支持本地需要保存的信息，选项内容请配置OptionDataTables
    /// </summary>
    public  class Options
    {

        const string LOCAL_PATH = "local_options.txt";

        private Dictionary<Option, object> _options = new Dictionary<Option, object>();
        private Dictionary<long, Dictionary<PlayerUniqueOption, object>> _playerUniqueOptions = new Dictionary<long, Dictionary<PlayerUniqueOption, object>>();

        private ConfigUtlis _localCfg = null;
        public bool IsInited;

        public void Initialize(bool applyEffect = true)
        {
            if (IsInited)
                return;
            this.Clear();
             bool local = LoadLocalOption();
            if (!local)
            {
                //Log.Game.Info("LoadLocalOption Failed!");
            }
            if(applyEffect)
            {
                InitAllOptionEffect();
            }

            IsInited = true;
        }

        private void Clear()
        {
            this._options.Clear();
        }
        

        public bool Has(Option key)
        {
            return this._options.ContainsKey(key);
        }

        public void Delete(Option key)
        {
            if (!this._options.Remove(key))
            {
                return;
            }
            _localCfg.Delete(key.ToString());
        }

        public T Get<T>(Option key)
        {
            object obj;
            if (!this._options.TryGetValue(key, out obj))
            {
                var val = (T)OptionDataTables.GetDefaultOptionValue(key);
                Set(key, val);

                return val;
                //return default(T);
            }
            return (T)((object)obj);
        }

        public bool GetBool(Option key)
        {
            return this.Get<bool>(key);
        }
        public int GetInt(Option key)
        {
            return this.Get<int>(key);
        }
        public float GetFloat(Option key)
        {
            return this.Get<float>(key);
        }
        public long GetLong(Option key)
        {
            return this.Get<long>(key);
        }
        public string GetString(Option key)
        {
            return this.Get<string>(key);
        }

        // needSetEffect:是否要改变游戏内的效果，默认不改变
        public void Set(Option key, object val, bool needSetEffect = false)
        {
            if (_localCfg == null)
                return;
            
            object obj;
            if (this._options.TryGetValue(key, out obj))
            {
                if (obj == val)
                {
                    return;
                }
                if (obj != null && obj.Equals(val))
                {
                    return;
                }
            }

            //Log.Game.Info("Set Option {0} to {1}", key, val);
            this._options[key] = val;

            Type type = OptionDataTables.s_typeMap[key];
            if (type == typeof(bool))
            {
                _localCfg.SetBool(key.ToString(), (bool)val);
            }
            else if (type == typeof(int))
            {
                _localCfg.SetInt(key.ToString(), (int)val);
            }
            else if (type == typeof(float))
            {
                _localCfg.SetFloat(key.ToString(), (float)val);
            }
            else if (type == typeof(long))
            {
                _localCfg.SetLong(key.ToString(), (long)val);
            }
            else
            {
                _localCfg.SetString(key.ToString(), (string)val);
            }

            if (needSetEffect)
            {
                SetOptionEffect(key);
            }
        }

        /// <summary>
        /// 设置项改变时，根据设置类型改变游戏内对应的行为
        /// </summary>
        private void SetOptionEffect(Option optType)
        {
            if (!IsOptionVaild(optType))
                return;
        }


        // 某个设置项在当前平台是否生效
        public bool IsOptionVaild(Option optType)
        {

            return true;
        }
        

        /// <summary>
        /// 获得玩家配置信息
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="playerId">若为0则使用当前玩家id</param>
        /// <returns></returns>
        public TValue GetPlayerUniqueConfig<TValue>(long playerId,PlayerUniqueOption key)
        {
            Dictionary<PlayerUniqueOption, object> playerDic;
            object oldValue;
            if ((_playerUniqueOptions.TryGetValue(playerId, out playerDic) || LoadPlayerUniqueOption(playerId, out playerDic)) && playerDic.TryGetValue(key, out oldValue))
            {
                return (TValue)(oldValue);
            }
            var val = (TValue)OptionDataTables.s_PlayerUniqueDefaultsMap[key];
            SetPlayerUniqueConfig(playerId,key, val);

            return val;
        }

        public void SetPlayerUniqueConfig(long playerId,PlayerUniqueOption key, object value)
        {
            Dictionary<PlayerUniqueOption, object> playerDic;
            if (!_playerUniqueOptions.TryGetValue(playerId, out playerDic))
            {
                playerDic = new Dictionary<PlayerUniqueOption, object>();
                _playerUniqueOptions[playerId] = playerDic;
            }

            object oldValue;
            if (playerDic.TryGetValue(key, out oldValue))
            {
                if (oldValue == value)
                {
                    return;
                }
                if (oldValue != null && oldValue.Equals(value))
                {
                    return;
                }
            }
            playerDic[key] = value;


            Type type = OptionDataTables.s_PlayerUniqueTypeMap[key];
            var sectionStr = string.Format("Player_{0}", playerId);
            if (type == typeof(bool))
            {
                _localCfg.SetBool(sectionStr, key.ToString(), (bool)value);
            }
            else if (type == typeof(int))
            {
                _localCfg.SetInt(sectionStr, key.ToString(), (int)value);
            }
            else if (type == typeof(float))
            {
                _localCfg.SetFloat(sectionStr, key.ToString(), (float)value);
            }
            else if (type == typeof(long))
            {
                _localCfg.SetLong(sectionStr, key.ToString(), (long)value);
            }
            else
            {
                _localCfg.SetString(sectionStr, key.ToString(), (string)value);
            }
        }

        private bool LoadPlayerUniqueOption(long playerId, out Dictionary<PlayerUniqueOption, object> playerDic)
        {
            playerDic = null;
            if (null == _localCfg)
                return false;
            var sectionStr = string.Format("Player_{0}", playerId);

            if (!_playerUniqueOptions.TryGetValue(playerId, out playerDic))
            {
                playerDic = new Dictionary<PlayerUniqueOption, object>();
                _playerUniqueOptions[playerId] = playerDic;
            }
            return LoadAllLines(_localCfg.GetSectionLines(sectionStr), playerDic, OptionDataTables.s_PlayerUniqueTypeMap);
        }


        public bool LoadLocalOption()
        {
            var path = ConfigUtlis.ConfigPath + LOCAL_PATH;
#if UNITY_EDITOR
            var path2 = $"{Application.dataPath}/{LOCAL_PATH}";
            if (File.Exists(path2) && !File.Exists(path))
            {
                File.Copy(path2, path, true);
            }

            if (File.Exists(path))
            {
                File.Copy(path, path2, true);
            }
#endif
            //Log.Game.Info($"LoadLocalOption path={path}");
            if (!File.Exists(path))
            {
                _localCfg = new ConfigUtlis(LOCAL_PATH);
                return true;
            }
            else
            {
                _localCfg = new ConfigUtlis(LOCAL_PATH);
                var lines = _localCfg.GetSectionLines("Default");

                if (!LoadAllLines(lines, _options, OptionDataTables.s_typeMap))
                {
                    return false;
                }
                return true;
            }
        }

        private void InitAllOptionEffect()
        {
            //Log.Info("[Options] InitAllOptionEffect");
            foreach (var kv in OptionDataTables.s_typeMap)
            {
                SetOptionEffect(kv.Key);
            }
        }
        
        private static bool LoadAllLines<TKey>(string[] lines, Dictionary<TKey, object> targetDic, Dictionary<TKey, Type> typeDic)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string text = lines[i];
                text = text.Trim();

                if (text.Length != 0)
                {
                    if (!text.StartsWith("#"))
                    {
                        TKey key;
                        object value;
                        if (!LoadLine(text, typeDic, out key, out value))
                        {
                            //Log.Game.Error("Options.LoadAllLines() - Failed to load line {0}.", i + 1);
                            return false;
                        }
                        else
                        {
                            targetDic[key] = value;
                        }
                    }
                }
            }

            return true;
        }


        private static bool LoadLine<TKey>(string line, Dictionary<TKey, Type> typeDic, out TKey key, out object val)
        {
            key = default(TKey);
            val = null;
            string text = null;
            string text2 = null;
            var equalPos = line.IndexOf('=');
            if (equalPos == -1)
                return false;
            text = line.Substring(0,equalPos).Trim();
            text2 = line.Substring(equalPos + 1, line.Length - equalPos - 1).Trim();
            TKey option = default(TKey);
            try
            {
                option = EnumUtils.GetEnum<TKey>(text, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                key = default(TKey);
                val = text2;
                return true;
            }
            if (!typeDic.ContainsKey(option)) return true;
            try
            {
                Type type = typeDic[option];
                if (type == typeof(bool))
                {
                    val = bool.Parse(text2);
                }
                else if (type == typeof(int))
                {
                    val = int.Parse(text2);
                }
                else if (type == typeof(float))
                {
                    val = float.Parse(text2);
                }
                else if (type == typeof(long))
                {
                    val = long.Parse(text2);
                }
                else
                {
                    if (type != typeof(string))
                    {
                        return false;
                    }
                    val = text2.Trim('"');
                }

                key = option;
            }
            catch (Exception e)
            {
                //Log.Error($"LoadLine Failed: [{text2}]");
                return false;
            }
            return true;
        }
        
    }
}