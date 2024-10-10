using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Quillry.Server.Domain;

namespace Quillry.Server.DataAccess
{
    public class UserRepo : IRepository<AppUser>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UserRepo(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> DeleteManyAsync(List<AppUser> data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.RemoveRange(data);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<bool> DeleteOneAsync(AppUser obj)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.Remove(obj);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<List<AppUser>> GetManyAsync(Expression<Func<AppUser, bool>> filter = null, int? take = null, string? includedProperties = null)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            IQueryable<AppUser> query = ctx.Users.AsNoTracking();

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
                return await query.Take(take.Value).OrderByDescending(u => u.JoinedOn).ToListAsync();
            }

            return await query.OrderByDescending(u => u.JoinedOn).ToListAsync();
        }

        public async Task<AppUser> GetOneAsync(Expression<Func<AppUser, bool>> filter, string? includedProperties = null)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            IQueryable<AppUser> query = ctx.Users.AsNoTracking();

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

        public async Task<bool> InsertManyAsync(List<AppUser> data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            await ctx.Users.AddRangeAsync(data);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<AppUser> InsertOneAsync(AppUser data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            await ctx.Users.AddAsync(data);
            return data;
        }

        public Task<bool> UpdateManyAsync(List<AppUser> data)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateOneAsync(AppUser data)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.Users.Attach(data);
            ctx.Entry(data).State = EntityState.Modified;
            return await ctx.SaveChangesAsync() >= 1;
        }
    }
}
