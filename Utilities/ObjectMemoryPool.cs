using System;
using System.Collections.Concurrent;

namespace EnhancedStreamChat.Utilities
{
    /// <summary>
    /// A dynamic pool of unity components of type T, that recycles old objects when possible, and allocates new objects when required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectMemoryPool<T> : IDisposable where T : new()
    {
        private readonly ConcurrentStack<T> _freeObjects;
        private readonly Action<T>? _firstAlloc;
        private readonly Action<T>? _onAlloc;
        private readonly Action<T>? _onFree;
        private readonly Func<T>? _constructor;
        private bool disposedValue;

        /// <summary>
        /// ObjectPool constructor function, used to setup the initial pool size and callbacks.
        /// </summary>
        /// <param name="initialCount">The number of components of type T to allocate right away.</param>
        /// <param name="constructor">The constructor function used to create new objects in the pool.</param>
        /// <param name="firstAlloc">The callback function you want to occur only the first time when a new component of type T is allocated.</param>
        /// <param name="onAlloc">The callback function to be called everytime ObjectPool.Alloc() is called.</param>
        /// <param name="onFree">The callback function to be called everytime ObjectPool.Free() is called</param>
        public ObjectMemoryPool(int initialCount = 0, Func<T>? constructor = null, Action<T>? firstAlloc = null, Action<T>? onAlloc = null, Action<T>? onFree = null)
        {
            this._constructor = constructor;
            this._firstAlloc = firstAlloc;
            this._onAlloc = onAlloc;
            this._onFree = onFree;
            this._freeObjects = new ConcurrentStack<T>();

            for (var i = 0; i < initialCount; i++) {
                this._freeObjects.Push(this.InternalAlloc());
            }
        }

        private T InternalAlloc()
        {
            var newObj = this._constructor is null ? new T() : this._constructor.Invoke();

            this._firstAlloc?.Invoke(newObj);
            return newObj;
        }

        /// <summary>
        /// Allocates a component of type T from a pre-allocated pool, or instantiates a new one if required.
        /// </summary>
        /// <returns></returns>
        public T Alloc()
        {
            if (!this._freeObjects.TryPop(out var obj) || obj == null) {
                obj = this.InternalAlloc();
                Logger.Debug($"InternalAlloc() in Alloc! : {obj}");
            }
            this._onAlloc?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// Inserts a component of type T into the stack of free objects. Note: the component does *not* need to be allocated using ObjectPool.Alloc() to be freed with this function!
        /// </summary>
        /// <param name="obj"></param>
        public void Free(T obj)
        {
            if (obj == null) {
                Logger.Warn($"{obj} is Destroyed.");
                return;
            }
            this._onFree?.Invoke(obj);
            this._freeObjects.Push(obj);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    while (this._freeObjects.TryPop(out _)) {
                    }
                    this._freeObjects.Clear();
                }
                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                this.disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}