using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GF.HotUpdateSystem.New
{
    // /version/policy.json
    // {
    // "MinSupportedVersion": "v1.0.0",
    // "ForceUpdateVersion": "v2.0.0",
    // "AllowDowngrade": false,
    // "CompatibleVersions": ["v1.0.0", "v1.0.1", "v1.0.2"],
    // "UpdateMessage": "发现新版本，建议更新以获得更好的体验"
    // }
    
    
    /// <summary>
    /// 版本策略管理器
    /// </summary>
    public static class VersionPolicyHelper
    {
        private static ServerVersionPolicy _cachedPolicy;
        private static float _lastRequestTime;
        private const float CACHE_DURATION = 300f; // 5分钟缓存

        /// <summary>
        /// 获取版本策略（带缓存）
        /// </summary>
        public static IEnumerator GetVersionPolicy(string serverURL, System.Action<ServerVersionPolicy> onSuccess, System.Action<string> onError)
        {
            // 检查缓存是否有效
            if (_cachedPolicy != null && Time.time - _lastRequestTime < CACHE_DURATION)
            {
                onSuccess?.Invoke(_cachedPolicy);
                yield break;
            }

            yield return RequestVersionPolicyInternal(serverURL, onSuccess, onError);
        }

        /// <summary>
        /// 强制刷新策略
        /// </summary>
        public static IEnumerator ForceRefreshPolicy(string serverURL, System.Action<ServerVersionPolicy> onSuccess, System.Action<string> onError)
        {
            ClearCache();
            yield return RequestVersionPolicyInternal(serverURL, onSuccess, onError);
        }
        
        /// <summary>
        /// 请求版本策略（带重试）
        /// </summary>
        public static IEnumerator RequestVersionPolicyWithRetry(string serverURL, int maxRetries = 2, System.Action<ServerVersionPolicy> onSuccess = null, System.Action<string> onError = null)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                bool success = false;
                ServerVersionPolicy policy = null;
                string error = null;
                
                yield return RequestVersionPolicyInternal(serverURL, 
                    (result) => { success = true; policy = result; }, 
                    (err) => { error = err; });
                
                if (success)
                {
                    onSuccess?.Invoke(policy);
                    yield break;
                }
                
                if (i < maxRetries - 1)
                {
                    Debug.LogWarning($"版本策略请求失败，重试 {i + 1}/{maxRetries}: {error}");
                    yield return new WaitForSeconds(0.5f); // 等待0.5秒后重试
                }
            }
            
            onError?.Invoke($"版本策略请求失败，已重试{maxRetries}次");
        }

        /// <summary>
        /// 内部请求方法
        /// </summary>
        private static IEnumerator RequestVersionPolicyInternal(string serverURL, System.Action<ServerVersionPolicy> onSuccess, System.Action<string> onError)
        {
            var url = $"{serverURL}/version/policy.json";
            using var request = UnityWebRequest.Get(url);
            request.timeout = 5; // 5秒超时
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var json = request.downloadHandler.text;
                    var policy = JsonUtility.FromJson<ServerVersionPolicy>(json);
                    _cachedPolicy = policy;
                    _lastRequestTime = Time.time;
                    onSuccess?.Invoke(policy);
                }
                catch (System.Exception ex)
                {
                    onError?.Invoke($"解析版本策略失败: {ex.Message}");
                }
            }
            else
            {
                onError?.Invoke($"请求版本策略失败: {request.error}");
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            _cachedPolicy = null;
            _lastRequestTime = 0f;
        }
        
        /// <summary>
        /// 检查策略服务是否可用
        /// </summary>
        public static IEnumerator CheckPolicyServiceAvailable(string serverURL, System.Action<bool> onResult)
        {
            var url = $"{serverURL}/version/policy.json";
            using var request = UnityWebRequest.Head(url);
            request.timeout = 3;
            yield return request.SendWebRequest();
            
            bool isAvailable = request.result == UnityWebRequest.Result.Success;
            onResult?.Invoke(isAvailable);
        }
    }
}