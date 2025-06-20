namespace GF.Option
{
    /// <summary>
    /// 配置服务接口，支持不同的存储后端实现
    /// </summary>
    public interface IOptionService
    {
        /// <summary>
        /// 初始化配置服务
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 获取配置值
        /// </summary>
        T Get<T>(OptionKey key);
        
        /// <summary>
        /// 设置配置值
        /// </summary>
        void Set(OptionKey key, object value, bool needCallBack = false);
        
        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        bool Has(OptionKey key);
        
        /// <summary>
        /// 删除配置
        /// </summary>
        void Delete(OptionKey key);
        
        /// <summary>
        /// 获取玩家特定配置
        /// </summary>
        TValue GetPlayerUniqueConfig<TValue>(long playerId, OptionKey key);
        
        /// <summary>
        /// 设置玩家特定配置
        /// </summary>
        void SetPlayerUniqueConfig(long playerId, OptionKey key, object value);
    }
    
    /// <summary>
    /// 配置存储后端接口
    /// </summary>
    public interface IConfigStorage
    {
        void Initialize(string path);
        T GetValue<T>(string section, string key, T defaultValue);
        void SetValue<T>(string section, string key, T value);
        void DeleteValue(string section, string key);
        void Save();
        string[] GetSectionLines(string section);
    }
} 