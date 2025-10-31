using System;

namespace GF.Option
{
    /// <summary>
    /// 基于字符串键的配置项标识
    /// </summary>
    public struct OptionKey : IEquatable<OptionKey>
    {
        public string Key { get; }
        public bool IsPlayerUnique { get; }

        public OptionKey(string key, bool isPlayerUnique = false)
        {
            Key = key;
            IsPlayerUnique = isPlayerUnique;
        }

        public bool Equals(OptionKey other)
        {
            return Key == other.Key && IsPlayerUnique == other.IsPlayerUnique;
        }

        public override bool Equals(object obj)
        {
            return obj is OptionKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, IsPlayerUnique);
        }

        public override string ToString()
        {
            return IsPlayerUnique ? $"Player:{Key}" : Key;
        }

        public static implicit operator string(OptionKey optionKey)
        {
            return optionKey.Key;
        }
    }
} 