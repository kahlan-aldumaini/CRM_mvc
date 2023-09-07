using CRM_mvc.Models.Entities;
using CRM_mvc.Utilities.Enumerations;
using System.Linq.Expressions;

namespace CRM_mvc.Models.Repositories;

public interface ICallRepository
{
    List<Call> Get(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false);
    Call First(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false);
    Call Last(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false);
    Task<Call?> GetByID(int Id, bool withTrashed = false);
    Task<Call> Insert(Call model);
    bool Any(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false);
    Task<DbStatus> Delete(int Id);
    Task<DbStatus> Restore(int Id, bool withTrashed = false);
    Call Find(int Id, bool withTrashed = false);
    DbStatus Update(Call model, bool withTrashed = false);
    IQueryable<Call> WithTrashed(bool withTrashed = false);
    Task<DbStatus> Save();
}