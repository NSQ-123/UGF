namespace GF.Option
{
    /// <summary>
    /// 配置系统主类，提供统一的访问入口
    /// </summary>
    public static class OptionSystem
    {
        private static IOptionService s_optionService;
        
        /// <summary>
        /// 获取配置服务实例
        /// </summary>
        public static IOptionService Service
        {
            get
            {
                if (s_optionService == null)
                {
                    s_optionService = new EnhancedOptions();
                    s_optionService.Initialize();
                }
                return s_optionService;
            }
        }
        
        /// <summary>
        /// 初始化配置系统
        /// </summary>
        /// <param name="customService">自定义配置服务实现</param>
        public static void Initialize(IOptionService customService = null)
        {
            if (customService != null)
            {
                s_optionService = customService;
            }
            else if (s_optionService == null)
            {
                s_optionService = new EnhancedOptions();
            }
            
            s_optionService.Initialize();
        }
        
        /// <summary>
        /// 重置配置系统
        /// </summary>
        public static void Reset()
        {
            s_optionService = null;
        }
    }
} 