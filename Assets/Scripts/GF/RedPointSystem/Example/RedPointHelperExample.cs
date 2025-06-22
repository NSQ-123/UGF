using UnityEngine;
using GF.RedPoint;

namespace GF.RedPoint.Example
{
    /// <summary>
    /// RedPointHelper使用示例
    /// 展示如何使用简化的静态API进行红点管理
    /// </summary>
    public class RedPointHelperExample : MonoBehaviour
    {
        [Header("UI引用")]
        public GameObject mailIcon;
        public GameObject taskIcon;
        public GameObject shopIcon;
        public GameObject mainMenuIcon;

        private void Start()
        {
            // 静态API无需初始化，会自动初始化
            SetupRedPoints();
            SetupListeners();
        }

        /// <summary>
        /// 设置红点配置
        /// </summary>
        private void SetupRedPoints()
        {
            // 1. 创建动态红点（自动检查数据）
            RedPointHelper.CreateRedPoint("mail", () => GetMailCount());
            RedPointHelper.CreateRedPoint("task", () => GetTaskCount());
            RedPointHelper.CreateRedPoint("shop", () => GetShopCount());

            // 2. 创建层级红点（主菜单汇总所有子红点）
            RedPointHelper.CreateRedPoint("main_menu", () => 0, "mail", "task", "shop");

            // 3. 创建静态红点（手动控制）
            RedPointHelper.CreateStaticRedPoint("newbie_guide", 1);
        }

        /// <summary>
        /// 设置监听器
        /// </summary>
        private void SetupListeners()
        {
            // 监听红点状态变化（是否显示红点）
            RedPointHelper.Watch("mail", hasRedPoint => {
                if (mailIcon != null)
                    mailIcon.SetActive(hasRedPoint>0);
                Debug.Log($"邮件红点: {hasRedPoint}");
            });

            RedPointHelper.Watch("task", hasRedPoint => {
                if (taskIcon != null)
                    taskIcon.SetActive(hasRedPoint>0);
                Debug.Log($"任务红点: {hasRedPoint}");
            });

            RedPointHelper.Watch("shop", hasRedPoint => {
                if (shopIcon != null)
                    shopIcon.SetActive(hasRedPoint>0);
                Debug.Log($"商店红点: {hasRedPoint}");
            });

            RedPointHelper.Watch("main_menu", hasRedPoint => {
                if (mainMenuIcon != null)
                    mainMenuIcon.SetActive(hasRedPoint>0);
                Debug.Log($"主菜单红点: {hasRedPoint}");
            });

            // 监听红点数量变化
            RedPointHelper.Watch("mail", count => {
                Debug.Log($"邮件数量: {count}");
            });
        }

        #region 模拟数据源

        private int GetMailCount()
        {
            // 模拟邮件数量
            return Random.Range(0, 5);
        }

        private int GetTaskCount()
        {
            // 模拟任务数量
            return Random.Range(0, 3);
        }

        private int GetShopCount()
        {
            // 模拟商店新物品数量
            return Random.Range(0, 2);
        }

        #endregion

        #region 测试按钮（可在Inspector中调用）

        [ContextMenu("刷新所有红点")]
        public void RefreshAllRedPoints()
        {
            RedPointHelper.RefreshRedPoints("mail", "task", "shop");
        }

        [ContextMenu("设置邮件红点数量为5")]
        public void SetMailCount()
        {
            // 注意：这里只是演示API，实际上mail是动态红点，应该通过修改数据源来改变
            Debug.Log("邮件是动态红点，数量由GetMailCount()函数决定");
            RedPointHelper.RefreshRedPoint("mail");
        }

        [ContextMenu("设置新手引导红点")]
        public void SetNewbieGuide()
        {
            RedPointHelper.SetRedPointCount("newbie_guide", 1);
        }

        [ContextMenu("清除新手引导红点")]
        public void ClearNewbieGuide()
        {
            RedPointHelper.SetRedPointCount("newbie_guide", 0);
        }

        [ContextMenu("检查红点状态")]
        public void CheckRedPointStatus()
        {
            Debug.Log($"邮件红点: {RedPointHelper.HasRedPoint("mail")} (数量: {RedPointHelper.GetRedPointCount("mail")})");
            Debug.Log($"任务红点: {RedPointHelper.HasRedPoint("task")} (数量: {RedPointHelper.GetRedPointCount("task")})");
            Debug.Log($"商店红点: {RedPointHelper.HasRedPoint("shop")} (数量: {RedPointHelper.GetRedPointCount("shop")})");
            Debug.Log($"主菜单红点: {RedPointHelper.HasRedPoint("main_menu")} (数量: {RedPointHelper.GetRedPointCount("main_menu")})");
        }

        [ContextMenu("打印红点图")]
        public void PrintRedPointGraph()
        {
            RedPointHelper.PrintGraph();
        }

        #endregion

        private void OnDestroy()
        {
            // 清理资源（可选，静态类会自动管理）
            // RedPointHelper.Dispose();
        }
    }
}

/*
RedPointHelper 使用指南总结：

1. 【简单场景】使用快速创建方法：
   - CreateStateRedPoint() - 状态型红点（有/无）
   - CreateCountRedPoint() - 计数型红点（显示数量）
   - CreateStaticRedPoint() - 静态红点（手动设置）

2. 【复杂场景】使用Builder模式：
   - Create(key).WithCheckFunc().AsStateType().ConnectTo().InGroup().Register()

3. 【监听变化】多种方式：
   - Watch() - 监听是否有红点
   - WatchCount() - 监听红点数量
   - OnRedPointChanged事件 - 全局监听
   - Module.BindRefreshAct() - 传统方式（通过Module访问）

4. 【常用操作】：
   - HasRedPoint() - 检查是否有红点
   - GetRedPointCount() - 获取红点数量
   - SetRedPointCount() - 手动设置数量
   - RefreshRedPoints() - 批量刷新

5. 【高级操作】通过Module属性访问：
   - Module.AddRedPointRelation() - 添加关系
   - Module.PrintGraph() - 打印红点图
   - Module.HasCircularDependency() - 检查循环依赖

6. 【最佳实践】：
   - 优先使用RedPointHelper的简化API
   - 需要高级功能时通过Module属性访问原始API
   - 使用有意义的key命名
   - 合理设置红点层级关系
   - 在Editor中检查循环依赖
*/ 