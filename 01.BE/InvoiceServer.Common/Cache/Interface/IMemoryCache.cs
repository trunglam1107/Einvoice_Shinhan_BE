
namespace InvoiceServer.Common.Cache
{
    public interface IMemoryCache<T>
    {
        T Get(string key);
        void Set(string key, T data);
        void Remove(string key);
        void RemoveIfKeyStartsWith(string partOfKey);
        void Clear();
        bool Contains(string key);
    }
}
