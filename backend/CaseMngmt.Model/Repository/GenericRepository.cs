using CaseMngmt.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CaseMngmt.Models.Repository
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        public GenericRepository(ApplicationDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException("An instance of DbContext is required to use this repository", "context");
            DbContext = dbContext;
            DbSet = DbContext.Set<T>();
        }

        protected DbContext DbContext { get; set; }

        protected DbSet<T> DbSet { get; set; }

        public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }

        public virtual IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public virtual T GetById(int id)
        {
            //return DbSet.FirstOrDefault(PredicateBuilder.GetByIdPredicate<T>(id));
            return DbSet.Find(id);
        }

        public virtual void Delete(T entity)
        {
            DbSet.Attach(entity);
            DbSet.Remove(entity);
        }

        public virtual void Delete(int id)
        {
            var entity = GetById(id);
            if (entity == null) return; // not found; assume already deleted.
            Delete(entity);
        }

        List<T> IRepository<T>.GetAll()
        {
            throw new NotImplementedException();
        }

        public List<T> Get(Func<T, bool> where)
        {
            throw new NotImplementedException();
        }

        public void Insert(T entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public void Remove(T entity)
        {
            throw new NotImplementedException();
        }

        public void Remove(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public void SyncDisconnected(T entity, bool forDeletion = false)
        {
            throw new NotImplementedException();
        }

        public void SyncDisconnected(IEnumerable<T> entities, bool forDeletion = false)
        {
            throw new NotImplementedException();
        }
    }
}
