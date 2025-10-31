using System;
using System.Linq;

namespace GF.Option
{
    /// <summary>
    /// 扩展方法，提供更便捷的API
    /// </summary>
    public static class OptionServiceExtensions
    {
        /// <summary>
        /// 获取布尔配置
        /// </summary>
        public static bool GetBool(this IOptionService service, OptionKey key)
        {
            return service.Get<bool>(key);
        }

        /// <summary>
        /// 获取整数配置
        /// </summary>
        public static int GetInt(this IOptionService service, OptionKey key)
        {
            return service.Get<int>(key);
        }

        /// <summary>
        /// 获取浮点配置
        /// </summary>
        public static float GetFloat(this IOptionService service, OptionKey key)
        {
            return service.Get<float>(key);
        }

        /// <summary>
        /// 获取字符串配置
        /// </summary>
        public static string GetString(this IOptionService service, OptionKey key)
        {
            return service.Get<string>(key);
        }

        /// <summary>
        /// 设置布尔配置
        /// </summary>
        public static void SetBool(this IOptionService service, OptionKey key, bool value, bool needCallBack = false)
        {
            service.Set(key, value, needCallBack);
        }

        /// <summary>
        /// 设置整数配置
        /// </summary>
        public static void SetInt(this IOptionService service, OptionKey key, int value, bool needCallBack = false)
        {
            service.Set(key, value, needCallBack);
        }

        /// <summary>
        /// 设置浮点配置
        /// </summary>
        public static void SetFloat(this IOptionService service, OptionKey key, float value, bool needCallBack = false)
        {
            service.Set(key, value, needCallBack);
        }

        /// <summary>
        /// 设置字符串配置
        /// </summary>
        public static void SetString(this IOptionService service, OptionKey key, string value, bool needCallBack = false)
        {
            service.Set(key, value, needCallBack);
        }

        // 便捷方法：直接通过字符串键访问
        public static T Get<T>(this IOptionService service, string key)
        {
            return service.Get<T>(new OptionKey(key));
        }

        public static void Set(this IOptionService service, string key, object value, bool needCallBack = false)
        {
            service.Set(new OptionKey(key), value, needCallBack);
        }

        #region 组合字符串扩展方法

        /// <summary>
        /// 设置组合字符串配置（使用推荐的分隔符）
        /// </summary>
        public static void SetCompositeString(this IOptionService service, OptionKey key, params string[] values)
        {
            char separator = CompositeStringHelper.SelectBestSeparator(values);
            string compositeValue = CompositeStringHelper.Combine(separator, values);
            service.SetString(key, compositeValue);
        }

        /// <summary>
        /// 设置组合字符串配置（指定分隔符）
        /// </summary>
        public static void SetCompositeString(this IOptionService service, OptionKey key, char separator, params string[] values)
        {
            string compositeValue = CompositeStringHelper.Combine(separator, values);
            service.SetString(key, compositeValue);
        }

        /// <summary>
        /// 设置组合值配置（自动转换为字符串，使用推荐的分隔符）
        /// </summary>
        public static void SetCompositeValues(this IOptionService service, OptionKey key, params object[] values)
        {
            var stringValues = values?.Select(v => v?.ToString() ?? string.Empty).ToArray() ?? new string[0];
            SetCompositeString(service, key, stringValues);
        }

        /// <summary>
        /// 设置组合值配置（自动转换为字符串，指定分隔符）
        /// </summary>
        public static void SetCompositeValues(this IOptionService service, OptionKey key, char separator, params object[] values)
        {
            var stringValues = values?.Select(v => v?.ToString() ?? string.Empty).ToArray() ?? new string[0];
            SetCompositeString(service, key, separator, stringValues);
        }

        /// <summary>
        /// 获取组合字符串配置（自动检测分隔符）
        /// </summary>
        public static string[] GetCompositeString(this IOptionService service, OptionKey key)
        {
            string compositeValue = service.GetString(key);
            if (string.IsNullOrEmpty(compositeValue))
                return new string[0];

            // 尝试用推荐的分隔符解析
            foreach (char separator in CompositeStringHelper.RecommendedSeparators)
            {
                if (compositeValue.Contains(separator))
                {
                    return CompositeStringHelper.Parse(compositeValue, separator);
                }
            }

            // 如果没有找到分隔符，返回单个元素数组
            return new string[] { compositeValue };
        }

        /// <summary>
        /// 获取组合字符串配置（指定分隔符）
        /// </summary>
        public static string[] GetCompositeString(this IOptionService service, OptionKey key, char separator)
        {
            string compositeValue = service.GetString(key);
            return CompositeStringHelper.Parse(compositeValue, separator);
        }

        /// <summary>
        /// 获取组合值配置（指定类型和分隔符）
        /// </summary>
        public static T[] GetCompositeValues<T>(this IOptionService service, OptionKey key, char separator) where T : IConvertible
        {
            string compositeValue = service.GetString(key);
            return CompositeStringHelper.Parse<T>(compositeValue, separator);
        }

        /// <summary>
        /// 尝试获取组合值配置（指定类型和分隔符）
        /// </summary>
        public static bool TryGetCompositeValues<T>(this IOptionService service, OptionKey key, char separator, out T[] result) where T : IConvertible
        {
            string compositeValue = service.GetString(key);
            return CompositeStringHelper.TryParse<T>(compositeValue, separator, out result);
        }

        // 便捷方法：直接通过字符串键访问组合字符串
        public static void SetCompositeString(this IOptionService service, string key, params string[] values)
        {
            service.SetCompositeString(new OptionKey(key), values);
        }

        public static void SetCompositeString(this IOptionService service, string key, char separator, params string[] values)
        {
            service.SetCompositeString(new OptionKey(key), separator, values);
        }

        public static string[] GetCompositeString(this IOptionService service, string key)
        {
            return service.GetCompositeString(new OptionKey(key));
        }

        public static string[] GetCompositeString(this IOptionService service, string key, char separator)
        {
            return service.GetCompositeString(new OptionKey(key), separator);
        }

        #endregion
    }
} 