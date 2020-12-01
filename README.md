### 创建`RazorPage`项目

```shell
mkdir YoYoMooc.ExampleApp
dotnet new razor --language C# --auth None --framework netcoreapp3.1
```

* 创建数据模型和存储库

```c#
using System.ComponentModel.DataAnnotations.Schema;

namespace YoYoMooc.ExampleApp.Models
{
    public class Product
    {
        public Product() { }
        public Product(string name = null,
        string category = null,
        decimal price = 0)
        {
            Name = name;
            Category = category;
            Price = price;
        }
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
    }
}
```

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoYoMooc.ExampleApp.Models
{
    public interface IProductRepository
    {
        IQueryable<Product> Products { get; }

    }
}
```

```c#
    public class MockProductRepository : IProductRepository
    {

       private static readonly Product[] DummyData = new Product[] {
new Product { Name = "产品1", Category = "分类1", Price = 100 },
new Product { Name = "产品2", Category = "分类1", Price = 100 },
new Product { Name = "产品3", Category = "分类2", Price = 100 },
new Product { Name = "产品4", Category = "分类2", Price = 100 },
};
        public IQueryable<Product> Products => DummyData.AsQueryable();

     }
```

* 传递数据到视图

```c#
  public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IProductRepository _repository;
        private readonly IConfiguration _config;
        public string Message { get; set; }
        public List<Product> Products { get; set; }
        public IndexModel(ILogger<IndexModel> logger, IProductRepository repository, IConfiguration config)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
        }
        public void OnGet()
        {
            Message = _config["MESSAGE"] ?? "深入浅出 ASP.NET Core 与Docker";
            Products = _repository.Products.ToList();
        }
    }
```

* 修改`Index.cshtml`文件

```c#
<div class="text-center">

    	<div class="m-1 p-1">

	<h3 class="display-4">欢迎</h3>
		<p>创建年轻人的 <a href="https://www.yoyomooc.com">第一个 ASP.NET Core项目</a>.</p>
			<h4 class="bg-success text-xs-center p-1 text-white"> @Model.Message</h4>
<table class="table table-sm table-striped">
			<thead>
				<tr><th>ID</th><th>名称</th><th>分类</th><th>价格</th></tr>
			</thead>
			<tbody>
				@foreach (var p in Model.Products)
				{
					<tr>
						<td>@p.ProductID</td>
						<td>@p.Name</td>
						<td>@p.Category</td>
						<td>￥@p.Price.ToString("F2")</td>
					</tr>
				}
			</tbody>
		</table>
        </div>
   </div>
```

* 注册到容器中

```c#
 public void ConfigureServices(IServiceCollection services)
 {
     services.AddTransient<IProductRepository, MockProductRepository>();
     services.AddRazorPages();
 }
```