using System.Linq.Expressions;

namespace Quillry.Server.DataAccess
{
    public interface IRepository<T> where T : class
    {
        Task<bool> DeleteManyAsync(List<T> data);
        Task<bool> DeleteOneAsync(T obj);
        Task<List<T>> GetManyAsync(Expression<Func<T, bool>> filter = null, int? take = null, string? includedProperties = null);
        Task<T> GetOneAsync(Expression<Func<T, bool>> filter, string? includedProperties = null);
        Task<bool> InsertManyAsync(List<T> data);
        Task<T> InsertOneAsync(T data);
        Task<bool> UpdateManyAsync(List<T> data);
        Task<bool> UpdateOneAsync(T data);
    }
}
