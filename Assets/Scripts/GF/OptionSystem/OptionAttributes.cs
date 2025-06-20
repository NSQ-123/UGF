using System;

namespace GF.Option
{
    /// <summary>
    /// 配置项属性，用于标记配置项
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionAttribute : Attribute
    {
        public string Key { get; }
        public string Description { get; set; }
        public bool IsPlayerUnique { get; set; }
        public bool IsReadOnly { get; set; }

        public OptionAttribute(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// 配置验证器属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionValidatorAttribute : Attribute
    {
        public string ValidatorMethodName { get; }

        public OptionValidatorAttribute(string validatorMethodName)
        {
            ValidatorMethodName = validatorMethodName;
        }
    }

    /// <summary>
    /// 配置变更回调属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionCallbackAttribute : Attribute
    {
        public string CallbackMethodName { get; }

        public OptionCallbackAttribute(string callbackMethodName)
        {
            CallbackMethodName = callbackMethodName;
        }
    }
} 