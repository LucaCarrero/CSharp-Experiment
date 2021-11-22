using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp2
{
    public static class Program
    {
        static void Main(string[] args)
        {
            test t1 = new test() {Id =1 , Hint ="", Name="" };

            List<string> f = new List<string>()
            {
              
                "Hint"
            };
            
            var type = LinqRuntimeTypeBuilder.genereteNewType(t1, f);

            using (var db = new EagerLoadingDbContext())
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // the code that you want to measure comes here                  


                var p = SelectDynamic(db.demo.AsQueryable(), type.Item1, type.Item2).ToList();

                //var p = db.demo.Select(el => new {hit =  el.Hint}).ToList();

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
            }
        }


        public static IQueryable<dynamic> SelectDynamic<T>(IQueryable<T> source, Type dynamicNewType, Dictionary<string, PropertyInfo> sourceProperties)
        {

            ParameterExpression sourceItem = Expression.Parameter(source.ElementType, "t");
            IEnumerable<MemberBinding> bindings = dynamicNewType.GetFields().Select(p => 
                    Expression.Bind(
                        p, 
                        Expression.Property(sourceItem, sourceProperties[p.Name])
                    )).OfType<MemberBinding>();

            var selector = Expression.Lambda<Func<T, dynamic>>(Expression.MemberInit(
                Expression.New(dynamicNewType.GetConstructor(Type.EmptyTypes)), bindings), sourceItem).Compile();

            return source.Select(selector).AsQueryable();
        }
    }

    class EagerLoadingDbContext : DbContext
    {

        private const string connectionString = "Data Source=c:\\sqlite.db";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        public DbSet<test> demo { get; set; }

    }

    public class test
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Hint { get; set; }

        public string c;
    }
}
