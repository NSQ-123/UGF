using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GF.Option
{
    /// <summary>
    /// 配置注册器，支持反射自动注册和手动注册
    /// </summary>
    public class OptionRegistry
    {
        private static readonly Dictionary<OptionKey, OptionMetadata> s_optionMetadata = new Dictionary<OptionKey, OptionMetadata>();
        private static readonly Dictionary<Type, object> s_configInstances = new Dictionary<Type, object>();

        /// <summary>
        /// 手动注册配置项
        /// </summary>
        public static OptionBuilder<T> Register<T>(OptionKey optionKey, T defaultValue)
        {
            return new OptionBuilder<T>(optionKey, defaultValue);
        }

        /// <summary>
        /// 自动注册配置类中的所有配置项
        /// </summary>
        public static void RegisterFromType<T>() where T : new()
        {
            RegisterFromType(typeof(T));
        }

        /// <summary>
        /// 自动注册配置类中的所有配置项
        /// </summary>
        public static void RegisterFromType(Type configType)
        {
            var instance = Activator.CreateInstance(configType);
            s_configInstances[configType] = instance;

            var members = configType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var member in members)
            {
                var optionAttr = member.GetCustomAttribute<OptionAttribute>();
                if (optionAttr == null) continue;

                Type memberType;
                object defaultValue;

                // 获取成员类型和默认值
                if (member is PropertyInfo property)
                {
                    memberType = property.PropertyType;
                    defaultValue = property.GetValue(instance);
                }
                else if (member is FieldInfo field)
                {
                    memberType = field.FieldType;
                    defaultValue = field.GetValue(instance);
                }
                else
                {
                    continue;
                }

                var optionKey = new OptionKey(optionAttr.Key, optionAttr.IsPlayerUnique);
                
                // 创建元数据
                var metadata = new OptionMetadata
                {
                    ValueType = memberType,
                    DefaultValue = defaultValue,
                    Description = optionAttr.Description,
                    IsReadOnly = optionAttr.IsReadOnly
                };

                // 设置验证器
                var validatorAttr = member.GetCustomAttribute<OptionValidatorAttribute>();
                if (validatorAttr != null)
                {
                    var validatorMethod = configType.GetMethod(validatorAttr.ValidatorMethodName, 
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (validatorMethod != null)
                    {
                        metadata.Validator = value => (bool)validatorMethod.Invoke(
                            validatorMethod.IsStatic ? null : instance, new[] { value });
                    }
                }

                // 设置回调
                var callbackAttr = member.GetCustomAttribute<OptionCallbackAttribute>();
                if (callbackAttr != null)
                {
                    var callbackMethod = configType.GetMethod(callbackAttr.CallbackMethodName,
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (callbackMethod != null)
                    {
                        metadata.OnValueChanged = value => callbackMethod.Invoke(
                            callbackMethod.IsStatic ? null : instance, new[] { value });
                    }
                }

                s_optionMetadata[optionKey] = metadata;
                Debug.Log($"Auto-registered option: {optionKey} with type {memberType.Name}");
            }
        }

        /// <summary>
        /// 获取配置元数据
        /// </summary>
        public static OptionMetadata GetMetadata(OptionKey optionKey)
        {
            return s_optionMetadata.TryGetValue(optionKey, out var metadata) ? metadata : null;
        }

        /// <summary>
        /// 获取所有已注册的配置项
        /// </summary>
        public static IEnumerable<OptionKey> GetAllOptionKeys()
        {
            return s_optionMetadata.Keys;
        }

        /// <summary>
        /// 获取配置实例
        /// </summary>
        public static T GetConfigInstance<T>() where T : class
        {
            return s_configInstances.TryGetValue(typeof(T), out var instance) ? instance as T : null;
        }

        internal static void AddMetadata(OptionKey optionKey, OptionMetadata metadata)
        {
            s_optionMetadata[optionKey] = metadata;
        }
    }

    /// <summary>
    /// 配置元数据
    /// </summary>
    public class OptionMetadata
    {
        public Type ValueType { get; set; }
        public object DefaultValue { get; set; }
        public Func<object, bool> Validator { get; set; }
        public Action<object> OnValueChanged { get; set; }
        public string Description { get; set; }
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// 配置构建器
    /// </summary>
    public class OptionBuilder<T>
    {
        private readonly OptionKey _optionKey;
        private readonly OptionMetadata _metadata;

        internal OptionBuilder(OptionKey optionKey, T defaultValue)
        {
            _optionKey = optionKey;
            _metadata = new OptionMetadata
            {
                ValueType = typeof(T),
                DefaultValue = defaultValue
            };
        }

        public OptionBuilder<T> WithDescription(string description)
        {
            _metadata.Description = description;
            return this;
        }

        public OptionBuilder<T> WithValidator(Func<T, bool> validator)
        {
            _metadata.Validator = obj => validator((T)obj);
            return this;
        }

        public OptionBuilder<T> WithOnValueChanged(Action<T> onValueChanged)
        {
            _metadata.OnValueChanged = obj => onValueChanged((T)obj);
            return this;
        }

        public OptionBuilder<T> AsReadOnly()
        {
            _metadata.IsReadOnly = true;
            return this;
        }

        public void Build()
        {
            OptionRegistry.AddMetadata(_optionKey, _metadata);
        }
    }
} 