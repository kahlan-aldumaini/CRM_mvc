using CRM_mvc.Utilities.Enumerations;
using System.Linq.Expressions;

namespace CRM_mvc.Models.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        List<T> Get(Expression<Func<T, bool>>? predicate = null, bool withTrashed = false);
        T First(Expression<Func<T, bool>>? predicate = null, bool withTrashed = false);
        T Last(Expression<Func<T, bool>>? predicate = null, bool withTrashed = false);
        Task<T?> GetByID(int Id, bool withTrashed = false);
        Task<T> Insert(T model);
        bool Any(Expression<Func<T, bool>>? predicate = null, bool withTrashed = false);
        Task<DbStatus> Delete(int Id);
        Task<DbStatus> Restore(int Id, bool withTrashed = false);
        T Find(int Id, bool withTrashed = false);
        DbStatus Update(T model, bool withTrashed = false);
        IQueryable<T> WithTrashed(bool withTrashed = false);
        Task<DbStatus> Save();
    }
}