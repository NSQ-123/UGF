using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GF.Option
{
    /// <summary>
    /// 组合字符串帮助类，支持多种分隔符和安全的字符串组合/解析
    /// </summary>
    public static class CompositeStringHelper
    {
        /// <summary>
        /// 常用的分隔符定义（避免INI文件格式冲突）
        /// </summary>
        public static class Separators
        {
            /// <summary>管道符 |</summary>
            public const char Pipe = '|';
            
            /// <summary>分号 ; （注意：可能与INI注释冲突，谨慎使用）</summary>
            public const char Semicolon = ';';
            
            /// <summary>逗号 ,</summary>
            public const char Comma = ',';
            
            /// <summary>冒号 :</summary>
            public const char Colon = ':';
            
            /// <summary>波浪号 ~</summary>
            public const char Tilde = '~';
            
            /// <summary>at符号 @</summary>
            public const char At = '@';
            
            /// <summary>美元符号 $</summary>
            public const char Dollar = '$';
            
            /// <summary>百分号 %</summary>
            public const char Percent = '%';
            
            /// <summary>插入符 ^</summary>
            public const char Caret = '^';
            
            /// <summary>与符号 &</summary>
            public const char Ampersand = '&';
            
            /// <summary>下划线 _</summary>
            public const char Underscore = '_';
            
            /// <summary>连字符 -</summary>
            public const char Hyphen = '-';
            
            /// <summary>加号 +</summary>
            public const char Plus = '+';
            
            /// <summary>星号 *</summary>
            public const char Asterisk = '*';
            
            /// <summary>问号 ?</summary>
            public const char Question = '?';
            
            /// <summary>制表符 \t</summary>
            public const char Tab = '\t';
            
            /// <summary>垂直制表符 \v</summary>
            public const char VerticalTab = '\v';
            
            /// <summary>单元分隔符 \x1F (ASCII 31) - 最安全</summary>
            public const char UnitSeparator = '\x1F';
            
            /// <summary>记录分隔符 \x1E (ASCII 30) - 最安全</summary>
            public const char RecordSeparator = '\x1E';
            
            /// <summary>组分隔符 \x1D (ASCII 29) - 最安全</summary>
            public const char GroupSeparator = '\x1D';
            
            /// <summary>文件分隔符 \x1C (ASCII 28) - 最安全</summary>
            public const char FileSeparator = '\x1C';
            
            /// <summary>信息分隔符1 \x1B (ASCII 27) - 最安全</summary>
            public const char InfoSeparator1 = '\x1B';
            
            /// <summary>信息分隔符2 \x1A (ASCII 26) - 最安全</summary>
            public const char InfoSeparator2 = '\x1A';
        }
        
        /// <summary>
        /// INI文件中的特殊字符，不应用作分隔符
        /// </summary>
        public static class IniReservedChars
        {
            /// <summary>等号 = （用于键值分隔）</summary>
            public const char Equal = '=';
            
            /// <summary>方括号 [ ] （用于节标记）</summary>
            public const char LeftBracket = '[';
            public const char RightBracket = ']';
            
            /// <summary>井号 # （用于注释）</summary>
            public const char Hash = '#';
            
            /// <summary>双引号 " （用于字符串值包装）</summary>
            public const char Quote = '"';
            
            /// <summary>换行符 \n \r （用于行分隔）</summary>
            public const char LineFeed = '\n';
            public const char CarriageReturn = '\r';
            
            /// <summary>反斜杠 \ （可能用于转义）</summary>
            public const char Backslash = '\\';
        }

        /// <summary>
        /// 推荐的分隔符优先级（从最安全到较安全，避免INI文件冲突）
        /// </summary>
        public static readonly char[] RecommendedSeparators = new char[]
        {
            Separators.UnitSeparator,    // 最安全，专门用于分隔
            Separators.RecordSeparator,  // 最安全
            Separators.GroupSeparator,   // 最安全
            Separators.FileSeparator,    // 最安全
            Separators.InfoSeparator1,   // 最安全
            Separators.InfoSeparator2,   // 最安全
            Separators.Pipe,             // 常用，安全
            Separators.Tilde,            // 常用，安全
            Separators.At,               // 常用，安全
            Separators.Caret,            // 较安全
            Separators.Dollar,           // 较安全
            Separators.Percent,          // 较安全
            Separators.Ampersand,        // 较安全
            Separators.Asterisk,         // 较安全
            Separators.Plus,             // 较安全
            Separators.Question,         // 较安全
            Separators.Underscore,       // 较安全
            Separators.Hyphen,           // 较安全
            Separators.Comma,            // 常用但可能冲突
            Separators.Colon,            // 常用但可能冲突
            Separators.Tab,              // 可能影响可读性
            Separators.VerticalTab,      // 可能影响可读性
            // 注意：不包含 Semicolon，因为可能与INI注释冲突
        };

        /// <summary>
        /// 组合多个字符串为一个复合字符串
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <param name="values">要组合的值</param>
        /// <returns>组合后的字符串</returns>
        public static string Combine(char separator, params string[] values)
        {
            if (values == null || values.Length == 0)
                return string.Empty;

            // 转义包含分隔符的字符串
            var escapedValues = values.Select(v => EscapeString(v ?? string.Empty, separator));
            return string.Join(separator.ToString(), escapedValues);
        }

        /// <summary>
        /// 组合多个值为一个复合字符串（自动转换为字符串）
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <param name="values">要组合的值</param>
        /// <returns>组合后的字符串</returns>
        public static string Combine(char separator, params object[] values)
        {
            if (values == null || values.Length == 0)
                return string.Empty;

            var stringValues = values.Select(v => v?.ToString() ?? string.Empty).ToArray();
            return Combine(separator, stringValues);
        }

        /// <summary>
        /// 解析复合字符串为字符串数组
        /// </summary>
        /// <param name="compositeString">复合字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>解析后的字符串数组</returns>
        public static string[] Parse(string compositeString, char separator)
        {
            if (string.IsNullOrEmpty(compositeString))
                return new string[0];

            var parts = compositeString.Split(separator);
            return parts.Select(part => UnescapeString(part, separator)).ToArray();
        }

        /// <summary>
        /// 解析复合字符串为指定类型的数组
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="compositeString">复合字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>解析后的类型化数组</returns>
        public static T[] Parse<T>(string compositeString, char separator) where T : IConvertible
        {
            var stringArray = Parse(compositeString, separator);
            var result = new T[stringArray.Length];
            
            for (int i = 0; i < stringArray.Length; i++)
            {
                try
                {
                    result[i] = (T)Convert.ChangeType(stringArray[i], typeof(T));
                }
                catch (Exception)
                {
                    result[i] = default(T);
                }
            }
            
            return result;
        }

        /// <summary>
        /// 尝试解析复合字符串为指定类型的数组
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="compositeString">复合字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="result">解析结果</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse<T>(string compositeString, char separator, out T[] result) where T : IConvertible
        {
            result = null;
            try
            {
                result = Parse<T>(compositeString, separator);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查字符串是否包含指定的分隔符
        /// </summary>
        /// <param name="text">要检查的字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>是否包含分隔符</returns>
        public static bool ContainsSeparator(string text, char separator)
        {
            return !string.IsNullOrEmpty(text) && text.Contains(separator);
        }

        /// <summary>
        /// 为字符串选择最佳的分隔符（避免INI文件冲突）
        /// </summary>
        /// <param name="values">要组合的字符串数组</param>
        /// <returns>推荐的分隔符</returns>
        public static char SelectBestSeparator(params string[] values)
        {
            if (values == null || values.Length == 0)
                return RecommendedSeparators[0];

            foreach (char separator in RecommendedSeparators)
            {
                // 跳过INI保留字符
                if (IsIniReservedChar(separator))
                    continue;
                
                bool isConflict = false;
                foreach (string value in values)
                {
                    if (ContainsSeparator(value, separator))
                    {
                        isConflict = true;
                        break;
                    }
                }
                
                if (!isConflict)
                    return separator;
            }

            // 如果所有推荐分隔符都有冲突，返回最安全的一个（会进行转义）
            return RecommendedSeparators[0];
        }
        
        /// <summary>
        /// 为字符串选择最佳的分隔符（包含安全性检查）
        /// </summary>
        /// <param name="values">要组合的字符串数组</param>
        /// <param name="avoidIniConflict">是否避免INI文件冲突字符</param>
        /// <returns>推荐的分隔符</returns>
        public static char SelectBestSeparator(string[] values, bool avoidIniConflict = true)
        {
            if (values == null || values.Length == 0)
                return RecommendedSeparators[0];

            foreach (char separator in RecommendedSeparators)
            {
                // 如果需要避免INI冲突，跳过保留字符
                if (avoidIniConflict && IsIniReservedChar(separator))
                    continue;
                
                bool isConflict = false;
                foreach (string value in values)
                {
                    if (ContainsSeparator(value, separator))
                    {
                        isConflict = true;
                        break;
                    }
                }
                
                if (!isConflict)
                    return separator;
            }

            // 如果所有推荐分隔符都有冲突，返回最安全的一个（会进行转义）
            return RecommendedSeparators[0];
        }

        /// <summary>
        /// 转义字符串中的分隔符
        /// </summary>
        /// <param name="text">原始字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>转义后的字符串</returns>
        private static string EscapeString(string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 使用反斜杠转义
            return text.Replace("\\", "\\\\").Replace(separator.ToString(), "\\" + separator);
        }

        /// <summary>
        /// 反转义字符串中的分隔符
        /// </summary>
        /// <param name="text">转义后的字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>原始字符串</returns>
        private static string UnescapeString(string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 反转义
            return text.Replace("\\" + separator, separator.ToString()).Replace("\\\\", "\\");
        }

        /// <summary>
        /// 获取分隔符的描述信息
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <returns>分隔符描述</returns>
        public static string GetSeparatorDescription(char separator)
        {
            switch (separator)
            {
                case Separators.Pipe: return "管道符 | (安全)";
                case Separators.Semicolon: return "分号 ; (注意：可能与INI注释冲突)";
                case Separators.Comma: return "逗号 ,";
                case Separators.Colon: return "冒号 :";
                case Separators.Tilde: return "波浪号 ~ (安全)";
                case Separators.At: return "at符号 @ (安全)";
                case Separators.Dollar: return "美元符号 $";
                case Separators.Percent: return "百分号 %";
                case Separators.Caret: return "插入符 ^ (安全)";
                case Separators.Ampersand: return "与符号 &";
                case Separators.Underscore: return "下划线 _";
                case Separators.Hyphen: return "连字符 -";
                case Separators.Plus: return "加号 +";
                case Separators.Asterisk: return "星号 *";
                case Separators.Question: return "问号 ?";
                case Separators.Tab: return "制表符 \\t";
                case Separators.VerticalTab: return "垂直制表符 \\v";
                case Separators.UnitSeparator: return "单元分隔符 (ASCII 31) - 最安全";
                case Separators.RecordSeparator: return "记录分隔符 (ASCII 30) - 最安全";
                case Separators.GroupSeparator: return "组分隔符 (ASCII 29) - 最安全";
                case Separators.FileSeparator: return "文件分隔符 (ASCII 28) - 最安全";
                case Separators.InfoSeparator1: return "信息分隔符1 (ASCII 27) - 最安全";
                case Separators.InfoSeparator2: return "信息分隔符2 (ASCII 26) - 最安全";
                
                // INI保留字符警告
                case IniReservedChars.Equal: return "等号 = (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.Hash: return "井号 # (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.LeftBracket: return "左方括号 [ (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.RightBracket: return "右方括号 ] (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.Quote: return "双引号 \" (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.Backslash: return "反斜杠 \\ (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.LineFeed: return "换行符 \\n (警告：INI文件保留字符，不建议使用)";
                case IniReservedChars.CarriageReturn: return "回车符 \\r (警告：INI文件保留字符，不建议使用)";
                
                default: return $"自定义分隔符 '{separator}' (ASCII {(int)separator})";
            }
        }
        
        /// <summary>
        /// 检查分隔符是否与INI文件格式冲突
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <returns>是否冲突</returns>
        public static bool IsIniReservedChar(char separator)
        {
            return separator == IniReservedChars.Equal ||
                   separator == IniReservedChars.Hash ||
                   separator == IniReservedChars.LeftBracket ||
                   separator == IniReservedChars.RightBracket ||
                   separator == IniReservedChars.Quote ||
                   separator == IniReservedChars.Backslash ||
                   separator == IniReservedChars.LineFeed ||
                   separator == IniReservedChars.CarriageReturn;
        }
    }
} 