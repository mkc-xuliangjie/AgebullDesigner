// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     扩展属性字典
    /// </summary>
    /// <remarks>
    ///     扩展字典对象都为DataMember属性,在需要网络序列化时,应该保证字典的Value的类型已知并可以网络序列化,否则在WCF传输时会出错
    /// </remarks>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public sealed class AttributeDictionary
    {
        [DataMember]
        private Dictionary<string, object> _dictionary;

        /// <summary>
        ///     扩展属性字典
        /// </summary>
        public Dictionary<string, object> Dictionary
        {
            get
            {
                return _dictionary ?? (_dictionary = new Dictionary<string, object>() );
            }
            set
            {
                _dictionary = value;
            }
        }

        /// <summary>
        ///     取得或设置扩展属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }
                object value;
                return Dictionary.TryGetValue(name, out value) ? value : null;
            }
            set
            {
                if (Dictionary.ContainsKey(name))
                {
                    if (value == null)
                    {
                        Dictionary.Remove(name);
                    }
                    else
                    {
                        Dictionary[name] = value;
                    }
                }
                else if (value != null)
                {
                    Dictionary.Add(name, value);
                }
            }
        }

        /// <summary>
        ///     取得或设置扩展属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return default( T );
            }
            object value;
            return Dictionary.TryGetValue(name, out value) ? (T) value : default( T );
        }

        /// <summary>
        ///     取得或设置扩展属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>如果设置成功返回True</returns>
        public bool Set<T>(string name, T value)
        {
            object value1;
            if (Dictionary.TryGetValue(name, out value1))
            {
                if (Equals(value, default( T )))
                {
                    Dictionary.Remove(name);
                    return true;
                }
                if (Equals(value1, value))
                {
                    return false;
                }
                Dictionary[name] = value;
                return true;
            }
            if (Equals(value, default( T )))
            {
                return false;
            }
            Dictionary.Add(name, value);
            return true;
        }
    }
}
