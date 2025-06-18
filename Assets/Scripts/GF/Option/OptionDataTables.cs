using System;
using System.Collections.Generic;

namespace pff.Homestead
{
     // 全局配置
    public enum Option
    {
        Invalid = 0,
        MAX_COUNT,

    }

 // 根据玩家ID不同区别保存的配置
    public enum PlayerUniqueOption 
    {
        Invalid = 0,
        MAX_COUNT,

    }

    public class OptionDataTables
    {
        public static readonly Dictionary<Option, Type> s_typeMap = new Dictionary<Option, Type>
        {


        };
        public static readonly Dictionary<Option, object> s_defaultsMap = new Dictionary<Option, object>
        {
    

          
        };
        
        public static object GetDefaultOptionValue(Option optType)
        {
            return s_defaultsMap[optType];
        }
    

        
        
        public static readonly Dictionary<PlayerUniqueOption, Type> s_PlayerUniqueTypeMap = new Dictionary<PlayerUniqueOption, Type>
        {
            

        };
        public static readonly Dictionary<PlayerUniqueOption, object> s_PlayerUniqueDefaultsMap = new Dictionary<PlayerUniqueOption, object>
        {

            

        };
    }
}