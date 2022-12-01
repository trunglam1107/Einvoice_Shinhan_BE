using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace InvoiceServer.Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="System.Collections.Generic.IEnumerable{T}"/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Performs an action on each value of the enumerable
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enumerable">Sequence on which to perform action</param>
        /// <param name="action">Action to perform on every item</param>
        /// <exception cref="System.ArgumentNullException">Thrown when given null <paramref name="enumerable"/> or <paramref name="action"/></exception>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            Ensure.Argument.ArgumentNotNull(enumerable, "enumerable");
            Ensure.Argument.ArgumentNotNull(action, "action");

            foreach (T value in enumerable)
            {
                action(value);
            }
        }

        /// <summary>
        /// Convenience method for retrieving a specific page of items within a collection.
        /// </summary>
        /// <param name="pageIndex">The index of the page to get.</param>
        /// <param name="pageSize">The size of the pages.</param>
        public static IEnumerable<T> GetPage<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            Ensure.Argument.ArgumentNotNull(source, "source");
            Ensure.Argument.Is(pageIndex >= 0, "The page index cannot be negative.");
            Ensure.Argument.Is(pageSize > 0, "The page size must be greater than zero.");

            return source.Skip(pageIndex * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Converts an enumerable into a readonly collection
        /// </summary>
        public static IEnumerable<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList());
        }

        /// <summary>
        /// Validates that the <paramref name="enumerable"/> is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        /// <summary>
        /// Validates that the <paramref name="enumerable"/> is not null and contains items.
        /// </summary>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable != null && enumerable.Any();
        }

        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string. If values has no members, the method returns null.</returns>
        public static string JoinOrDefault<T>(this IEnumerable<T> values, string separator)
        {
            Ensure.Argument.ArgumentNotNullOrEmpty(separator, "separator");

            if (values == null)
                return default(string);

            return string.Join(separator, values);
        }

        private static IOrderedQueryable<T> OrderingHelper<T>(IQueryable<T> source, string propertyName, bool descending, bool isFirstOrderColumn)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var property = Expression.PropertyOrField(param, propertyName);
            var sort = Expression.Lambda(property, param);

            var call = Expression.Call(
                typeof(Queryable),
                (isFirstOrderColumn ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(T), property.Type },
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        private static IOrderedQueryable<T> OrderingHelperWithChildObject<T>(IQueryable<T> source, string propertyName, bool descending, bool isFirstOrderColumn)
        {
            string command = (isFirstOrderColumn ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty);
            var typeOfEntity = typeof(T);

            var orderByPropertyLevel1Name = propertyName.Substring(0, propertyName.IndexOf("."));

            var typeLevel1 = Assembly.Load(typeOfEntity.Assembly.ToString()).GetTypes().First(t => t.Name == orderByPropertyLevel1Name);
            if (typeLevel1 != null)
            {
                var propertiesL1 = typeOfEntity.GetProperty(orderByPropertyLevel1Name);
                var parameterL1 = Expression.Parameter(typeOfEntity, "p");
                var propertyAccessL1 = Expression.MakeMemberAccess(parameterL1, propertiesL1);


                var orderByPropertyLevel2Name = propertyName.Substring(propertyName.IndexOf(".") + 1);
                var propertyLevel2 = typeLevel1.GetProperty(orderByPropertyLevel2Name);
                var propertyAccessL2 = Expression.MakeMemberAccess(propertyAccessL1, propertyLevel2);
                var orderByExpression = Expression.Lambda(propertyAccessL2, parameterL1);

                var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeOfEntity, propertyLevel2.PropertyType },
                                              source.Expression, Expression.Quote(orderByExpression));

                return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExpression);
            }

            return null;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                return OrderingHelperWithChildObject(source, propertyName, false, true);
            }

            return OrderingHelper(source, propertyName, false, true);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool descending)
        {
            if (propertyName.Contains("."))
            {
                return OrderingHelperWithChildObject(source, propertyName, descending, true);
            }

            return OrderingHelper(source, propertyName, descending, true);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                return OrderingHelperWithChildObject(source, propertyName, false, true);
            }

            return OrderingHelper(source, propertyName, false, false);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName, bool descending)
        {
            if (propertyName.Contains("."))
            {
                return OrderingHelperWithChildObject(source, propertyName, descending, true);
            }

            return OrderingHelper(source, propertyName, descending, false);
        }

    }
}
