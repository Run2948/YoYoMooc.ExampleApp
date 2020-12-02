using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace YoYoMooc.ExampleApp.Models
{
  /// <summary>
  /// 种子数据
  /// </summary>
  public static class SeedData
  {
    /// <summary>
    /// 初始化数据库和种子数据
    /// </summary>
    /// <param name="dbcontext"></param>
    public static IApplicationBuilder UseDataInitializer(this IApplicationBuilder builder)
    {
      using (var scope = builder.ApplicationServices.CreateScope())
      {
        var dbcontext = scope.ServiceProvider.GetService<ProductDbContext>();
        System.Console.WriteLine("开始执行迁移数据库...");
        dbcontext.Database.Migrate();
        System.Console.WriteLine("数据库迁移完成...");
        if (!dbcontext.Products.Any())
        {
          System.Console.WriteLine("开始创建种子数据中...");
          dbcontext.Products.AddRange(
          new Product("空调", "家用电器", 2750),
          new Product("电视机", "家用电器", 2448.95m),
          new Product("洗衣机 ", "家用电器", 1449.50m),
          new Product("油烟机 ", "家用电器", 3454.95m),
          new Product("冰箱", "家用电器", 9500),
          new Product("猪肉 ", "食品", 36),
          new Product("牛肉 ", "食品", 49.95m),
          new Product("鸡肉 ", "食品", 22),
          new Product("鸭肉", "食品", 18)
          );
          dbcontext.SaveChanges();
        }
        else
        {
          System.Console.WriteLine("无需创建种子数据...");
        }
      }
      return builder;
    }
  }

}