using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoYoMooc.ExampleApp.Models
{
  public class MockProductRepository : IProductRepository
  {
    private static readonly Product[] DummyData = new[]
    {
      new Product { ProductID = 1, Name = "产品1", Category = "分类1", Price = 100 },
      new Product { ProductID = 2, Name = "产品2", Category = "分类1", Price = 200 },
      new Product { ProductID = 3, Name = "产品3", Category = "分类2", Price = 300 },
      new Product { ProductID = 4, Name = "产品4", Category = "分类2", Price = 400 },
    };
    public IQueryable<Product> Products => DummyData.AsQueryable();

  }
}
