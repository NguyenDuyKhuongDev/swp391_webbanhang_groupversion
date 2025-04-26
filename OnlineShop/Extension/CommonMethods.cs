using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Identity.Client;
using System.Linq.Expressions;

namespace OnlineShop.ExtensionMethods
{
    public static class CommonMethods
    {
        public static List<T> Paging<T>(List<T> source, int page, int pageSize)
        {
            if (source == null || source.Count == 0) return new List<T>();
            if (page < 0 || pageSize < 0) return new List<T>();
            return source.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
        }

        public static List<T> Paging<T>(this IQueryable<T> source, int page, int pageSize)
        {
            if (source == null || !source.Any()) return new List<T>();
            if (page < 0 || pageSize < 0) return new List<T>();
            return source.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
        }

        //su dung reflection
        //khong dung duoc neu source khong nam trong ram
        public static List<T> Search<T>(this IQueryable<T> source, List<string> atributes, string searchTearm)
        {
            if (searchTearm == null) return source.ToList();
            if (source == null || !source.Any()) return new List<T>();
            var properties = typeof(T)
                .GetProperties()
                .Where(p => atributes.Contains(p.Name))
                .ToList()
                ;
            var lowerSearchTearm = searchTearm.ToLower();
            return source.Where(item =>
           properties.Any(p =>
           p.GetValue(item) != null ? p.GetValue(item).ToString().ToLower().Contains(lowerSearchTearm) : false
               )).ToList();
        }
        //su dung expression tree de truy van xuong db  thay vi load toan bo len ram
        public static IQueryable<T> SearchEF<T>(this IQueryable<T> source, List<string> properties, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return source;

            var parameter = Expression.Parameter(typeof(T), "x");
            var searchLower = Expression.Constant(searchTerm.ToLower());    
            Expression predicate = null;

            foreach (var propName in properties)
            {
                var prop = Expression.Property(parameter, propName);
                var toStringCall = Expression.Call(prop, "ToString", Type.EmptyTypes);
                var toLowerCall = Expression.Call(toStringCall, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                var containsCall = Expression.Call(toLowerCall, typeof(string).GetMethod("Contains", new[] { typeof(string) }), searchLower);

                predicate = predicate == null
                    ? (Expression)containsCall
                    : Expression.OrElse(predicate, containsCall);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
            return source.Where(lambda);
        }



    }
}
