### 1.通过`.NET Core CLI`创建`RazorPage`程序

```bash
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

### 2.创建一个自定义`ASP.NET Core RazorPage Docker`镜像

提示：`从ASP.NET Core 3.x 开始，微软镜像就不再由 hub.docker.com 托管。而是由微软官方进行独立维护，所以镜像域名地址为mcr.microsoft.com`

```dockerfile
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
# 复制应用程序文件
COPY dist /app
# 设置工作目录
WORKDIR /app
# 公开HTTP端口
EXPOSE 80/tcp
# 运行应用程序
ENTRYPOINT ["dotnet","Example.dll"]
```

- 复制应用程序文件

当您将`ASP.NET Core`应用程序容器化时，所有已编译的类文件，`NuGet`包，配置文件，并将Razor视图添加到镜像中。 COPY命令复制文件或文件夹放入容器。

```bash
COPY dist /app
```

此命令是将`dist`的文件夹中的文件复制到容器`/app`的文件夹中。 目前dist文件夹不存在，我们会在后面准备它。

- 设置工作目录

`WORKDIR`命令便是设置容器的工作目录，这是在运行时非常有用命令，当你需要指定某个路径或者文件的时，不必指定完整路径。 `Dockerfile`文件中的命令会将COPY 命令创建的`/app`文件夹的路径，包含到容器的应用程序中。

- 公开HTTP端口

容器中的进程无需任何特殊措施即可打开网络端口，但Docker不允许外部世界访问它们，除非`Dockerfile`包含一个指定端口的`EXPOSE`命令，如下所示：

```bash
EXPOSE 80/tcp
```

这个命令告诉Docker，它可以使容器外的TCP请求可用端口80。 对我们的示例应用程序，也需要这样做，这样`ASP.NET Core Kestrel`服务器才能接收到HTTP请求。

> 提示:在容器中处理端口是一个两步走的过程。在后面 "使用容器的工作 "部分，了解更多关于 关于如何完成配置，使服务器能够接收请求的详细信息。

- 运行应用程序

Docker文件的最后一步是`ENTRYPOINT`命令，它告诉Docker此为容器的起点。

```bash
ENTRYPOINT ["dotnet", "YoYoMooc.ExampleApp.dll"]
```

该命令告诉Docker运行`dotnet cli`命令行工具来执行`YoYoMooc.ExampleApp`文件，我将在下一节中创建。 不必指定`YoYoMooc.ExampleApp`文件的路径,因为它假定位于`WORKDIR`命令指定的目录中，而目录将包含所有的应用程序文件。

- 预备的应用程序镜像

  我们知道只有 `registory.cn-hangzhou.aliyuncs.com/yoyosoft/dotnet/core/sdk`才能运行`dotnet cli`命令。

输入以下命令：

```bash
dotnet publish --framework netcoreapp3.1 --configuration Release --output dist
```

 `dotnet publish`命令可以编译应用程序，然后将其转换为转换成一个独立的文件集，其中包含了应用程序所需的所有内容。输出参数指定了编译后的项目应该被写到一个名为 `dist` 的文件夹中，这个文件夹对应`Dockerfile`中的 `COPY`命令。

* 创建一个自定义镜像

```bash
docker build . -t yoyomooc/exampleapp -f Dockerfile
```

### 3.`Docker`镜像创建容器的几种方法

* 创建容器

```c#
docker create -p 3000:80 --name exampleApp3000 yoyomooc/exampleapp
```

* 启动所有容器

```bash
docker start exampleApp3000

docker start $(docker ps -aq)
```

* 停止所有正在运行的容器

```bash
docker stop exampleApp3000

docker stop $(docker ps -q)
```

* 获取容器输出日志

```bash
显示容器的最新输出(即使是容器已被停止)
docker start exampleApp3000

使用 -f 参数来监控输出
docker logs -f exampleApp3000
```

* 命令创建和启动容器

```bash
docker run -p 5000:80 --name exampleApp5000 yoyomooc/exampleapp
```

* 自动删除容器(当容器停止时自动删除)

```bash
docker run -p 6000:80 --rm --name exampleApp6000 yoyomooc/exampleapp
```

