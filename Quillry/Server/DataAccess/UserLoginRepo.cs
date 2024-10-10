using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Quillry.Server.Domain;

namespace  Quillry.Server.DataAccess
{
    public class UserLoginRepo : IRepository<AppUserLogin>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UserLoginRepo(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> DeleteManyAsync(List<AppUserLogin> data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.RemoveRange(data);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<bool> DeleteOneAsync(AppUserLogin obj)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.Remove(obj);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<List<AppUserLogin>> GetManyAsync(Expression<Func<AppUserLogin, bool>> filter = null, int? take = null, string? includedProperties = null)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            IQueryable<AppUserLogin> query = ctx.AppUserLogins.AsNoTracking();

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includedProperties))
            {
                foreach (var includedProperty in includedProperties.Split(
                    new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includedProperty.Trim());
                }
            }

            if (take is not null)
            {
                return await query.Take(take.Value).ToListAsync();
            }

            return await query.OrderByDescending(x => x.LoggedInOn).ToListAsync();
        }

        public async Task<AppUserLogin> GetOneAsync(Expression<Func<AppUserLogin, bool>> filter, string? includedProperties = null)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            IQueryable<AppUserLogin> query = ctx.AppUserLogins.AsNoTracking();

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includedProperties))
            {
                foreach (var includedProperty in includedProperties.Split(
                    new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includedProperty.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> InsertManyAsync(List<AppUserLogin> data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            await ctx.AppUserLogins.AddRangeAsync(data);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<AppUserLogin> InsertOneAsync(AppUserLogin data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            await ctx.AppUserLogins.AddAsync(data);
            await ctx.SaveChangesAsync();
            return data;
        }

        public Task<bool> UpdateManyAsync(List<AppUserLogin> data)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateOneAsync(AppUserLogin data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.AppUserLogins.Attach(data);
            ctx.Entry(data).State = EntityState.Modified;
            return await ctx.SaveChangesAsync() >= 1;
        }
    }
}
