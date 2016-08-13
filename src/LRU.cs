using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volte.Data.Dapper
{

    [Serializable]
    public class LRU<TKey, TValue> where TValue : class {
        const string      ZFILE_NAME   = "LRU";
        private readonly Dictionary<TKey, LRUNode> _NodesDict    = new Dictionary<TKey, LRUNode>();
        private readonly LinkedList<LRUNode>       LRULinkedList = new LinkedList<LRUNode>();
        private readonly int maxSize;
        private readonly TimeSpan timeOut;
        private static object _PENDING = new object();

        private Timer cleanupTimer;

        public LRU(int maxCacheSize = 100)
        {
            this.maxSize = maxCacheSize;
        }

        public LRU(TimeSpan itemExpiryTimeout, int maxCacheSize = 100, int memoryRefreshInterval = 1000)
        {
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            this.maxSize             = maxCacheSize;
            this.timeOut             = itemExpiryTimeout;
            TimerCallback tcb        = this.RemoveExpiredElements;
            this.cleanupTimer        = new Timer(tcb, autoEvent, 0, memoryRefreshInterval);
        }

        public void SetValue(TKey key, TValue cacheObject)
        {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            lock (_PENDING) {
                LRUNode node;

                if (this._NodesDict.TryGetValue(key, out node)) {
                    this.Delete(node);
                }

                this.ShrinkToSize(this.maxSize - 1);
                this.CreateNodeandAddtoList(key, cacheObject);
            }
        }

        public TValue GetValue(TKey key)
        {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            TValue data = null;
            LRUNode node;

            lock (_PENDING) {
                if (this._NodesDict.TryGetValue(key, out node)) {
                    if (node != null) {
                        node.HitTimes++;
                        data = node.Value;

                        if (node.HitTimes > 20) {
                            ThreadPool.QueueUserWorkItem(this.AddBeforeFirstNode, key);
                        }
                    }
                }

                return data;
            }
        }

        private void CreateNodeandAddtoList(TKey userKey, TValue cacheObject)
        {
            LRUNode node = new LRUNode(userKey, cacheObject, (this.timeOut > DateTime.MaxValue.Subtract(DateTime.UtcNow) ? DateTime.MaxValue : DateTime.UtcNow.Add(this.timeOut)));
            node.LLNode = this.LRULinkedList.AddFirst(node);
            this._NodesDict[userKey] = node;
        }

        private void AddBeforeFirstNode(object stateinfo)
        {
            lock (_PENDING) {
                TKey key = (TKey) stateinfo;
                LRUNode nodeInfo;

                if (this._NodesDict.TryGetValue(key, out nodeInfo)) {
                    if (nodeInfo != null && nodeInfo.HitTimes > 20) {
                        if (nodeInfo.LLNode != this.LRULinkedList.First) {
                            this.LRULinkedList.Remove(nodeInfo.LLNode);
                            nodeInfo.LLNode = this.LRULinkedList.AddBefore(this.LRULinkedList.First, nodeInfo);
                            nodeInfo.HitTimes = 0;
                        }
                    }
                }
            }
        }

        private void ShrinkToSize(int desiredSize)
        {
            while (this._NodesDict.Count > desiredSize) {
                this.RemoveLeastValuableNode();
            }
        }

        private void RemoveLeastValuableNode()
        {
            if (this.LRULinkedList.Last != null) {
                LRUNode node = this.LRULinkedList.Last.Value;
                this.Delete(node);
            }
        }

        private void Delete(LRUNode node)
        {
            this.LRULinkedList.Remove(node.LLNode);
            this._NodesDict.Remove(node.Key);
        }

        private class LRUNode {
            internal readonly DateTime Expiry_DateTime;

            internal LRUNode(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
                this.Expiry_DateTime = DateTime.MaxValue ;
            }

            internal LRUNode(TKey key, TValue value, DateTime _Expiry_DateTime)
            {
                this.Key = key;
                this.Value = value;
                this.Expiry_DateTime = _Expiry_DateTime;
            }

            internal TKey Key
            {
                get;
                private set;
            }
            internal TValue Value
            {
                get;
                private set;
            }
            internal int HitTimes
            {
                get;
                set;
            }

            internal LinkedListNode<LRUNode> LLNode
            {
                get;
                set;
            }
        }
    }
}
