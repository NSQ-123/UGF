using System;
using System.Collections.Generic;

namespace pff.Homestead
{
    public enum Option
    {
        Invalid = 0,
        GFX_Quality,                // 画面显示品质等级
        GFX_TargetFrameRate,        // 帧率
        Resolution,                 // 分辨率
        IsFullScreen,               // 是否全屏
        SoundValue_Global,          // 总音量
        SoundValue_Background,       // 背景音量
        SoundValue_Sound,           // 音效音量
        Language,                   // 语言类型

    }

    public enum PlayerUniqueOption  // 根据玩家ID不同区别保存的配置
    {
        Invalid = 0,
        Activity_Pop,
        MAX_COUNT,

    }

    public class OptionDataTables
    {
        public static readonly Dictionary<Option, Type> s_typeMap = new Dictionary<Option, Type>
        {
            {Option.GFX_Quality,typeof(int)},
            {Option.GFX_TargetFrameRate,typeof(int)},
            {Option.SoundValue_Global,typeof(float)},
            {Option.SoundValue_Background,typeof(float)},
            {Option.SoundValue_Sound,typeof(float)},
            {Option.Language,typeof(string)},
            {Option.Resolution,typeof(string)},
            {Option.IsFullScreen,typeof(bool)},

        };
        public static readonly Dictionary<Option, object> s_defaultsMap = new Dictionary<Option, object>
        {
            {Option.GFX_Quality, 3},
            {Option.GFX_TargetFrameRate,30},
            {Option.SoundValue_Global,0.67f},
            {Option.SoundValue_Background,0.67f},
            {Option.SoundValue_Sound,0.67f},
            {Option.Language,""}, // 需要匹配设备系统的语言，这里不再使用默认的"EN"
#if DEBUG
            {Option.IsFullScreen,false},
#else
            {Option.IsFullScreen,true},
#endif
            {Option.Resolution,"1920x1080"},
          
        };
        
        public static object GetDefaultOptionValue(Option optType)
        {
            if (optType == Option.Language)
            {
                // 获取默认语言
                s_defaultsMap[optType] = "en"; //LocalizeMgr.Inst.GetDefaultLanguageType().GetCode();
            }
            return s_defaultsMap[optType];
        }
        private static Dictionary<string, string> _s_Lang_SelectMap;
        public static Dictionary<string, string> s_Lang_SelectMap
        {
            get
            {
                if (_s_Lang_SelectMap == null)
                {
                    _s_Lang_SelectMap = new Dictionary<string, string>();
                    // TODO 从配置文件获取
                    _s_Lang_SelectMap.Add("En", "ENGLISH");
                }
                return _s_Lang_SelectMap;
            }
        }

        
        public static readonly Dictionary<int, string> s_fps_SelectMap = new Dictionary<int, string>
        {
            {30, "30fps"},
            {60, "60fps"},
        };

        public static readonly Dictionary<int, string> s_quality_SelectMap = new Dictionary<int, string>
        {
            {0, "graphics_quality_01"},
            {1, "graphics_quality_02"},
            {2, "graphics_quality_03"},
            {3, "graphics_quality_04"},
            {4, "graphics_quality_05"},
            {5, "graphics_quality_06"},
        }; 

        public static readonly Dictionary<string, string> s_resolution_SelectMap = new Dictionary<string, string>
        {
            {"3840x2160", "3840x2160"},
            {"1920x1080", "1920x1080"},
            {"1366x768", "1366x768"},
            {"1280x720", "1280x720"},
        };
        public static string GetTypeValue(int _typeNum)
        {
            string _value = "1280x720";
            switch (_typeNum)
            {
                case 2160:
                    _value = s_resolution_SelectMap["3840x2160"];
                    break;
                case 1080:
                    _value = s_resolution_SelectMap["1920x1080"];
                    break;
                case 768:
                    _value = s_resolution_SelectMap["1366x768"];
                    break;
                case 720:
                    _value = s_resolution_SelectMap["1280x720"];
                    break;
            }

            return _value;
        }
        
        public static readonly Dictionary<PlayerUniqueOption, Type> s_PlayerUniqueTypeMap = new Dictionary<PlayerUniqueOption, Type>
        {
            {PlayerUniqueOption.Activity_Pop,typeof(string)},

        };
        public static readonly Dictionary<PlayerUniqueOption, object> s_PlayerUniqueDefaultsMap = new Dictionary<PlayerUniqueOption, object>
        {

            { PlayerUniqueOption.Activity_Pop, "" },

        };
    }
}