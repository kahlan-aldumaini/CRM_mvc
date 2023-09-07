using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Utilities.Enumerations;
using System.Linq.Expressions;

namespace CRM_mvc.Models.Repositories.Interfaces;

public class CallRepository : ICallRepository
{
    private readonly ApplicationDbContext _context;

    public CallRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DbStatus> Delete(int Id)
    {
        Call? call = WithTrashed().First(v => v.Id == Id);
        if (call != null)
        {
            call.DeletedAt = DateTime.Now;
            return await Save();
        }
        return DbStatus.FAILS;
    }

    public async Task<DbStatus> Restore(int Id, bool withTrashed = false)
    {
        Call? call = WithTrashed(withTrashed).First(v => v.Id == Id);
        if (call != null && call.DeletedAt.Equals(null))
        {
            call.DeletedAt = null;
            return await Save();
        }
        return DbStatus.FAILS;
    }

    public async Task<Call?> GetByID(int Id, bool withTrashed = false)
    {
        if (WithTrashed(withTrashed).Any(v => v.Id == Id))
        {
            return null;
        }
        return await _context.Calls.FindAsync(Id);
    }

    public List<Call> Get(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false)
    {
        if (predicate == null)
        {
            return WithTrashed(withTrashed).ToList();
        }
        return WithTrashed(withTrashed).Where(predicate).ToList();
    }

    public async Task<Call> Insert(Call model)
    {
        await _context.AddAsync(model);
        return WithTrashed().Last();
    }

    public async Task<DbStatus> Save()
    {
        int save = await _context.SaveChangesAsync();
        if (save == 0)
        {
            return DbStatus.FAILS;
        }
        return DbStatus.SUCCESSED;
    }

    public DbStatus Update(Call model, bool withTrashed = false)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Call> WithTrashed(bool withTrashed = false)
    {
        if (withTrashed)
            return _context.Calls;
        return _context.Calls.Where(v => !v.DeletedAt.Equals(null));
    }

    public Call First(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false)
    {
        if (predicate == null)
        {
            return WithTrashed(withTrashed).First();
        }
        return WithTrashed(withTrashed).First(predicate);
    }

    public Call Last(Expression<Func<Call, bool>>? predicate = null, bool withTrashed = false)
    {
        if (predicate == null)
        {
            return WithTrashed(withTrashed).Last();
        }
        return WithTrashed(withTrashed).Last(predicate);
    }

    public bool Any(Expression<Func<Call, bool>> predicate, bool withTrashed = false)
    {
        return WithTrashed(withTrashed).Any(predicate);
    }

    public Call Find(int Id, bool withTrashed = false)
    {
        throw new NotImplementedException();
    }
}