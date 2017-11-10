using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Libs;

namespace Wenli.Live.RtmpLib.Models
{
    [Serializable]
    [TypeConverter(typeof(AsObjectConverter))]
    public class AsObject : DynamicObject, IDictionary<string, object>
    {
        readonly Dictionary<string, object> underlying;

        public string TypeName { get; set; }
        public bool IsTyped => !string.IsNullOrEmpty(TypeName);

        public AsObject()
        {
            underlying = new Dictionary<string, object>();
        }

        public AsObject(string typeName) : this()
        {
            this.TypeName = typeName;
        }

        public AsObject(Dictionary<string, object> dictionary) : this()
        {
            underlying = new Dictionary<string, object>(dictionary);
        }

        public override IEnumerable<string> GetDynamicMemberNames() => underlying.Keys;
        public override bool TryGetMember(GetMemberBinder binder, out object result) => underlying.TryGetValue(binder.Name, out result);
        public override bool TryDeleteMember(DeleteMemberBinder binder) => underlying.Remove(binder.Name);
        public override bool TrySetMember(SetMemberBinder binder, object value) { underlying[binder.Name] = value; return true; }

        #region IDictionary<> members

        public int Count => underlying.Count;
        public bool IsReadOnly => ((IDictionary<string, object>)underlying).IsReadOnly;
        public ICollection<string> Keys => underlying.Keys;
        public ICollection<object> Values => underlying.Values;

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)underlying).Add(item);
        }

        public void Clear()
        {
            underlying.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return underlying.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)underlying).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)underlying).Remove(item);
        }

        public void Add(string key, object value)
        {
            underlying.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return underlying.ContainsKey(key);
        }

        public object this[string key]
        {
            get { return underlying[key]; }
            set { underlying[key] = value; }
        }

        public bool Remove(string key)
        {
            return underlying.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return underlying.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return underlying.GetEnumerator();
        }

        #endregion
    }
}
