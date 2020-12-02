using System.Linq;

namespace YoYoMooc.ExampleApp.Models
{
  public class DataProductRepository : IProductRepository
  {
    private ProductDbContext context;

    public DataProductRepository(ProductDbContext ctx)
    {
      context = ctx;
    }

    public IQueryable<Product> Products => context.Products;
  }
}
