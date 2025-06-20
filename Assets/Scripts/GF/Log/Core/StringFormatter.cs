using System;
using System.Text;
using System.Threading;

namespace GF.Log
{
    /// <summary>
    /// 高性能日志格式化工具类
    /// </summary>
    public static class StringFormatter
    {
        // 线程本地的StringBuilder池
        private static readonly ThreadLocal<StringBuilder> _stringBuilder = 
            new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

        private const int MaxCapacity = 1024; // 最大容量限制

        /// <summary>
        /// 使用StringBuilder进行高性能字符串格式化
        /// </summary>
        /// <param name="message">格式化字符串</param>
        /// <param name="args">参数数组</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(string message, params object[] args)
        {
            // 如果没有参数或消息为空，直接返回
            if (args == null || args.Length == 0 || string.IsNullOrEmpty(message))
                return message ?? string.Empty;

            var sb = _stringBuilder.Value;
            try
            {
                sb.Clear();
                return FormatInternal(sb, message, args);
            }
            finally
            {
                // 防止StringBuilder过度增长
                if (sb.Capacity > MaxCapacity)
                {
                    sb.Capacity = MaxCapacity;
                }
            }
        }

        /// <summary>
        /// 使用提供的StringBuilder进行格式化（避免线程本地存储的开销）
        /// </summary>
        /// <param name="sb">StringBuilder实例</param>
        /// <param name="message">格式化字符串</param>
        /// <param name="args">参数数组</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(StringBuilder sb, string message, params object[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrEmpty(message))
                return message ?? string.Empty;

            sb.Clear();
            return FormatInternal(sb, message, args);
        }

        /// <summary>
        /// 检查字符串是否需要格式化（包含占位符）
        /// </summary>
        /// <param name="message">要检查的字符串</param>
        /// <returns>如果包含占位符返回true</returns>
        public static bool NeedsFormatting(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;
            
            for (int i = 0; i < message.Length - 1; i++)
            {
                if (message[i] == '{' && message[i + 1] != '{')
                {
                    // 查找匹配的右括号
                    int endIndex = message.IndexOf('}', i + 1);
                    if (endIndex > i + 1)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 安全的格式化方法，失败时回退到标准格式化
        /// </summary>
        /// <param name="message">格式化字符串</param>
        /// <param name="args">参数数组</param>
        /// <returns>格式化后的字符串</returns>
        public static string SafeFormat(string message, params object[] args)
        {
            try
            {
                return Format(message, args);
            }
            catch (Exception)
            {
                // 格式化失败时回退到标准方式
                try
                {
                    return string.Format(message, args);
                }
                catch (Exception)
                {
                    // 如果标准格式化也失败，返回原始消息加参数信息
                    var fallback = new StringBuilder(message);
                    if (args != null && args.Length > 0)
                    {
                        fallback.Append(" [参数: ");
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (i > 0) fallback.Append(", ");
                            fallback.Append(args[i]?.ToString() ?? "null");
                        }
                        fallback.Append("]");
                    }
                    return fallback.ToString();
                }
            }
        }

        /// <summary>
        /// 内部格式化实现
        /// </summary>
        private static string FormatInternal(StringBuilder sb, string message, object[] args)
        {
            int argIndex = 0;
            bool inBrace = false;
            int braceStart = -1;

            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];

                if (c == '{')
                {
                    if (i + 1 < message.Length && message[i + 1] == '{')
                    {
                        // 转义的左括号 {{
                        sb.Append('{');
                        i++; // 跳过下一个字符
                    }
                    else if (!inBrace)
                    {
                        // 开始占位符
                        inBrace = true;
                        braceStart = i;
                    }
                    else
                    {
                        // 嵌套的左括号，当作普通字符处理
                        sb.Append(c);
                    }
                }
                else if (c == '}' && inBrace)
                {
                    // 结束占位符
                    inBrace = false;
                    
                    if (i > braceStart + 1)
                    {
                        // 解析占位符内容
                        var placeholder = message.Substring(braceStart + 1, i - braceStart - 1);
                        
                        if (int.TryParse(placeholder.Trim(), out int placeholderIndex))
                        {
                            // 数字占位符 {0}, {1} 等
                            if (placeholderIndex >= 0 && placeholderIndex < args.Length)
                            {
                                sb.Append(args[placeholderIndex]?.ToString() ?? "null");
                            }
                            else
                            {
                                // 索引超出范围，保留原始占位符
                                sb.Append('{').Append(placeholder).Append('}');
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(placeholder))
                        {
                            // 空占位符 {}，使用顺序参数
                            if (argIndex < args.Length)
                            {
                                sb.Append(args[argIndex++]?.ToString() ?? "null");
                            }
                            else
                            {
                                sb.Append("{}"); // 保留原始占位符
                            }
                        }
                        else
                        {
                            // 不支持的占位符格式，保留原始内容
                            sb.Append('{').Append(placeholder).Append('}');
                        }
                    }
                    else
                    {
                        // 空的大括号 {}
                        if (argIndex < args.Length)
                        {
                            sb.Append(args[argIndex++]?.ToString() ?? "null");
                        }
                        else
                        {
                            sb.Append("{}");
                        }
                    }
                }
                else if (c == '}' && !inBrace)
                {
                    if (i + 1 < message.Length && message[i + 1] == '}')
                    {
                        // 转义的右括号 }}
                        sb.Append('}');
                        i++; // 跳过下一个字符
                    }
                    else
                    {
                        // 单独的右括号，当作普通字符
                        sb.Append(c);
                    }
                }
                else if (!inBrace)
                {
                    // 普通字符
                    sb.Append(c);
                }
                // 如果在括号内，跳过字符（已经在占位符处理中包含）
            }

            // 如果有未闭合的占位符，添加剩余部分
            if (inBrace)
            {
                sb.Append(message.Substring(braceStart));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 释放线程本地资源
        /// </summary>
        public static void Dispose()
        {
            _stringBuilder?.Dispose();
        }
    }
} 