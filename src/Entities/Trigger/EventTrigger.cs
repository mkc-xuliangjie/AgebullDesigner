﻿using System;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// 事件触发器
    /// </summary>
    public abstract class EventTrigger
    {
        /// <summary>
        /// 目标类型
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// 当前配置对象
        /// </summary>
        protected NotificationObject Target;

        /// <summary>
        ///     发出属性修改前事件
        /// </summary>
        /// <param name="config"></param>
        /// <param name="property">属性</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        public void BeforePropertyChanged(NotificationObject config, string property, object oldValue, object newValue)
        {
            Target = config;
            BeforePropertyChanged(property, oldValue, newValue);
        }
        /// <summary>
        ///     发出属性修改前事件
        /// </summary>
        /// <param name="property">属性</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        protected virtual void BeforePropertyChanged(string property, object oldValue, object newValue)
        {
        }
        /// <summary>
        /// 属性事件处理
        /// </summary>
        /// <param name="config"></param>
        /// <param name="property"></param>
        internal void OnPropertyChanged(NotificationObject config, string property)
        {
            Target = config;
            OnPropertyChanged(property);
        }
        /// <summary>
        /// 属性事件处理
        /// </summary>
        /// <param name="property"></param>
        protected virtual void OnPropertyChanged(string property)
        {

        }

        /// <summary>
        /// 载入事件处理
        /// </summary>
        /// <param name="config"></param>
        public void OnCreate(NotificationObject config)
        {
            Target = config;
            OnCreate();
            OnLoad();
        }
        
        /// <summary>
        /// 载入事件处理
        /// </summary>
        protected virtual void OnCreate()
        {
        }

        /// <summary>
        /// 载入事件处理
        /// </summary>
        /// <param name="config"></param>
        public void OnLoad(NotificationObject config)
        {
            Target = config;
            OnLoad();
        }
        /// <summary>
        /// 载入事件处理
        /// </summary>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// 加入事件处理
        /// </summary>
        /// <param name="config"></param>
        /// <param name="parent"></param>
        public void OnAdded(NotificationObject parent, NotificationObject config)
        {
            Target = parent;
            OnAdded(config);
        }

        /// <summary>
        /// 加入事件处理
        /// </summary>
        /// <param name="config"></param>
        protected virtual void OnAdded(NotificationObject config)
        {
        }
        /// <summary>
        /// 删除事件处理
        /// </summary>
        /// <param name="config"></param>
        /// <param name="parent"></param>
        public void OnRemoved(NotificationObject parent, NotificationObject config)
        {
            Target = parent;
            OnRemoved(config);
        }
        /// <summary>
        /// 删除事件处理
        /// </summary>
        /// <param name="config"></param>
        protected virtual void OnRemoved(NotificationObject config)
        {
        }
    }
}
