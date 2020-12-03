# 深入浅出`ASP.NET Core`与`Docker`三剑客

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

### 4.复制文件到正在运行的Docker容器中

为了便于演示效果，我们会通过运行两个容器来做对比，分别是映射到端口3000和4000。

* 运行我们之前创建的两个容器

```bash
docker start exampleApp3000 exampleApp4000
```

* 在样式文件中添加以下代码 (`site.css`)

```css
.text-white {
  color:red !important;
}

.bg-success {
  background-color: rgb(71, 71, 71) !important;
} 
```

* 项目根目录中执行以下命令

```bash
docker cp wwwroot/css/site.css exampleApp4000:/app/wwwroot/css/site.css
```

* 先尝试直接查看效果，然后输入以下命令再次查看效果（清除文件缓存的影响）

```bash
docker stop exampleApp4000
docker start exampleApp4000
```

* 检查对容器的修改

```bash
docker diff exampleApp4000
```

结果中的每个列，都有一个字母表示变化的类型，请查看以下注释说明：

- **A** 表示已将一个文件或文件夹添加到容器中。
- **C** 表示文件或文件夹已被修改。如果是文件夹，表示该文件夹内的文件已被添加或删除。
- **D** 表示文件或文件夹已从容器中删除。

我们可以看到除了`site.css`发生了变化，还创建了一些文件，这些文件均是和调试有关的内容。

### 5. 将正在运行的容器保存为本地Docker镜像

* 直接与容器进行交互，进入容器内部查看文件

```bash
docker exec exampleApp4000 cat /app/wwwroot/css/site.css 
```

- 进入容器内部，通过操作 css 样式文件来验证

```bash
// -it 参数是告诉 Docker 这是一个交互式命令,这需要终端支持
docker exec -it exampleApp4000 /bin/bash
// 这一步可能因为网络的问题，导致无法正常的安装VIM工具，可以选择跳过
apt-get update
apt-get install vim
// 进入Site.css文件中，然后执行i键，然后就可以正常的编辑代码了
vim /app/wwwroot/css/site.css
```

然后保存修改后的文件，刷新浏览器就可以看到修改后的结果。

* 将修改后的容器创建为镜像

```bash
// 这个命令会创建一个新的 yoyomooc/exampleapp 镜像的变体，标记为change
docker commit exampleApp4000 yoyomooc/exampleapp:changed
```

### 6. 发布Docker镜像到`Dockhub`容器仓库

**注意: ** 请自行前往`https://hub.docker.com/`注册自己的账户，为了对比方便，我们再制作一个未修改的镜像一共发布到Docker仓库中。

* 为需要发布的项目打上标记

```bash
docker tag yoyomooc/exampleapp:change kindyroo/exampleapp:change
docker tag yoyomooc/exampleapp:latest kindyroo/exampleapp:unchange
```

* 现在打开终端控制台，输入以下命令

```bash
docker login -u <用户名> -p <密码>
```

​		登录成功会返回 `Login Succeeded` 消息

* 推送镜像到仓库

```bash
docker push kindyroo/exampleapp:changed
docker push kindyroo/exampleapp:unchange
```

* 登录`https://hub.docker.com/repository/docker/kindyroo/exampleapp`地址进行验证

* 注销 docker 登录的用户

```bash
docker logout
```

### 7. [发布Docker镜像到阿里云容器仓库](https://www.yoyomooc.com/yoyomooc/11-Publish-the-image-to--Aliyun-Docker)

### 8. [发布Docker镜像到Azure容器仓库](https://www.yoyomooc.com/yoyomooc/12-Publish-the-image-to--Azure-Docker)

### 9. Docker中的数据卷(Volume)和网络(NetWork)介绍

* Docker数据卷的重要性

  使用容器的主要好处之一是它们很容易创造和摧毁，但当销毁容器时，其文件系统中的文件也会被删除，如果把数据文件一并删除了，那将是灾难级的，因为它们将永远丢失。所以Docker提供了**卷**的功能来管理应用程序数据。

* 验证Docker卷的存在

  通过实践来验证卷的存在是最好的方式， 我们在`YoYoMooc.ExampleApp`根目录中创建一个名为`Dockerfile.volumes`的文件。 添加以下代码:

```dockerfile
FROM alpine:3.9

WORKDIR /data

ENTRYPOINT (test -e message.txt && echo "文件已存在" \
    || (echo "创建文件中..." \
    && echo 你好, Docker 时间： $(date '+%X') > message.txt)) && cat message.txt
```

* 在`YoYoMooc.ExampleApp`根目录下，运行如下命令

```bash
docker build . -t yoyomooc/vtest -f Dockerfile.volumes
docker run --name vtest yoyomooc/vtest
```

* 执行后，可以会看到如下结果

```bash
创建文件中...
你好, Docker 时间: 05:38:35
```

​		以上信息是容器创建后，继续创建`message.txt`文件，然后读取`data/message.txt`的中的数据，然后显示出来。

​		因为我没有为这个数据文件指定卷，所以它成为了容器内系统文件的一部分。而容器的文件系统是持久化的。

* 我们可以通过命令来进行验证：

```bash
docker start -a vtest
```

​		可以看到输出的消息提示为`/data/message.txt`已经存在，并且时间戳都是相同的。

* 现在删除容器，重新创建并启动一个新的容器

```bash
docker run --name vtest apress/vtest
```

* 得到的输出内容为：

```bash
创建文件中...
你好, Docker 时间: 05:58:00
```

  Docker 会删除容器，同时 `/data/message.txt` 文件也会被删除。 所以容器删除后，数据文件也就丢失了。而在实际生产环境中，删除数据文件会造成严重的后果，所以需要避免。

### 10. 如何使用Docker Volume管理机密数据

* 更新Dockerfile文件

  现在我们需要更新我们的Dockerfile文件，让它支持数据卷，以下是更新后的代码：

```dockerfile
FROM alpine:3.9

VOLUME /data

WORKDIR /data

ENTRYPOINT (test -e message.txt && echo "文件已存在" \
    || (echo "创建文件中..." \
    && echo 你好, Docker 时间: $(date '+%X') > message.txt)) && cat message.txt

```

* 重新创建镜像文件

```bash
docker build . -t yoyomooc/vtest -f Dockerfile.volumes
```

* 创建一个卷

```bash
docker volume create --name testdata
```

* 创建容器

```bash
// 参数 -v 会告诉docker，将容器内部/data目录中创建的任何数据均保存到卷testdata中
docker run --name vtest2 -v testdata:/data yoyomooc/vtest
```

* 输出结果如下

```bash
创建文件中...
你好, Docker 时间: 06:25:41
```

* 尝试删除现有的容器，并创建和运行一个新容器替换现在的

```bash
docker rm -f vtest2
docker run --name vtest2 -v testdata:/data yoyomooc/vtest
```

* 输出的结果如下

```bash
文件已存在
你好, Docker 时间: 06:25:41
```

* 查看一个镜像是否使用了卷

```bash
docker inspect yoyomooc/vtest
```

* 查看是否包括以下内容：

```bash
 "Volumes": {
    "/data": {}
 },
```

### 11. 为`ASP.NET Core RazorPage` 使用`EF Core` 连接`MySQL`数据库

* 拉取并检查数据库镜像

```bash
docker pull mysql:8.0

docker inspect mysql:8.0
```

* 通过检查返回的信息

```bash
 "Volumes": {
     "/var/lib/mysql": {}
 },
```

这是告知我们，`mysql:8.0`的镜像可以将目录`/var/lib/mysql`作为数据卷，这个卷就是`MySQL`存储数据文件的地方。

* 创建一个数据库容器及Docker卷

```bash
docker volume create --name productdata
```

* 创建一个`MySQL`容器，关联到我们创建的卷上

```bash
docker run -d --name asp-mysql8 -v productdata:/var/lib/mysql -e MYSQL_ROOT_PASSWORD=123456 -e bind-address=0.0.0.0  mysql:8.0.0
```

* 为`RazorPage`项目添加`MySQL`支持

```xml
  <ItemGroup>   
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="3.1.3">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
     <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="3.1.1" />
  </ItemGroup>
```

* 创建一个连接数据库的仓储类

```c#
using Microsoft.EntityFrameworkCore;

namespace YoYoMooc.ExampleApp.Models
{
  public class ProductDbContext : DbContext
  {
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
    : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
  }
}
```

```c#
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
```

```c#
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
```

* 配置服务和种子数据

```c#
    public void ConfigureServices(IServiceCollection services)
    {
      // services.AddTransient<IProductRepository, MockProductRepository>();
      services.AddTransient<IProductRepository, DataProductRepository>();
      services.AddRazorPages();

      var host = Configuration["DBHOST"] ?? "localhost";
      var port = Configuration["DBPORT"] ?? "3306";
      var password = Configuration["DBPASSWORD"] ?? "123456";


      var connectionStr = $"server={host};userid=root;pwd={password};"
      + $"port={port};database=products";

      services.AddDbContextPool<ProductDbContext>(options => options.UseMySql(connectionStr));

    }
```

```c#
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    { 
        ...
		app.UseDataInitializer();
    }
```

* 创建数据迁移

```bash
// 从 net core 3.0 起，EF Core 命令列工具 (dotnet ef) 不在 .NET Core SDK 里面，需另装。命令如下：
dotnet tool install --global dotnet-ef

dotnet ef migrations add Initial

// 使用Visual Studio包管理器控制台
Add-migration Initial
```

* 如果你想手动生成数据库

```bash
dotnet ef database update

// 使用Visual Studio包管理器控制台
Update-Database
```

注意：目前我们没有映射端口到的宿主主机，所以无法访问。如果要本地访问请执行以下命令

```bash
docker run -d -p 3253:3306 --name asp-mysql8 -v productdata:/var/lib/mysql -e MYSQL_ROOT_PASSWORD=123456  -e bind-address=0.0.0.0 mysql:8.0
```

* 修改视图内容

```c#
        /// <summary>
        /// 服务器的名称
        /// </summary>
        public string Hostname { get; set; }
        public string DBHOST { get; set; }
        public string DBPORT { get; set; }
        public string DBPASSWORD { get; set; }

		public void OnGet()
        {
            Message = _config["MESSAGE"] ?? "深入浅出 ASP.NET Core 与Docker";

            Products = _repository.Products.ToList();


            Hostname = _config["HOSTNAME"];
            DBHOST = _config["DBHOST"] ?? "localhost";
            DBPORT = _config["DBPORT"] ?? "3306";
            DBPASSWORD = _config["DBPASSWORD"] ?? "bb123456";
        }
```

```c#
<div class="text-center">
  <div class="m-1 p-1">
    <h3 class="display-4">欢迎</h3>
    <p>创建年轻人的 <a href="https://www.yoyomooc.com">第一个 ASP.NET Core项目</a>.</p>
    <h4 class="bg-success text-xs-center p-1 text-white"> @Model.Message</h4>
    <h4 class="bg-success text-xs-center p-1 text-white">服务器Id: @Model.Hostname</h4>
    <h1 class="bg-primary text-xs-center p-1 text-white">@Model.DBHOST:@Model.DBPORT</h1>
    <table class="table table-sm table-striped">
      <thead>
        <tr>
          <th>ID</th>
          <th>名称</th>
          <th>分类</th>
          <th>价格</th>
        </tr>
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

* 重新创建应用镜像

```bash
dotnet publish --framework netcoreapp3.1 --configuration Release --output dist

docker build . -t yoyomooc/exampleapp -f Dockerfile
```

* 检查Docker分配给`MySQL`容器的`IP`地址

```c#
docker network inspect bridge
```

```bash
"Containers": {
    "5aea9ac5557d172117ed56baba66fd070acc9421198344dfda0becae00f5ebb0": {
    "Name": "asp-mysql8",
    "EndpointID": "a01a73de5b19359ed7a0043667305fbfe3277678bef588807ed0b5fb51608503",
    "MacAddress": "02:42:ac:11:00:02",
    "IPv4Address": "172.17.0.2/16",
    "IPv6Address": ""
    },
}
```

* 创建和启动容器，并监视它的输出

```bash
docker run -d --name productapp -p 3000:80 -e DBHOST=172.17.0.2 -e DBPASSWORD=123456 yoyomooc/exampleapp

docker logs -f productapp
```

```bash
warn: Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository[60]
      Storing keys in a directory '/root/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. Protected data will be unavailable when container is destroyed.
warn: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[35]
      No XML encryptor configured. Key {4d902682-d44c-47d0-aad9-18f1547803d0} may be persisted to storage in unencrypted form.
开始执行迁移数据库...
数据库迁移完成...
开始创建种子数据中...
启动 ASP.NET Core 深入浅出Docker...
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://[::]:80
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
      Failed to determine the https port for redirect.
```

* 请打开浏览器请求URL:  http://localhost:3000

### 12.  Docker中的虚拟网络是如何工作的

![image-20201203090815980](https://gitee.com/zhujinrun/image/raw/master/qing/2020/image-20201203090815980.png)

**核心问题**：

​		当我们的`MySQL`容器重启或重新创建时，它的`IP`地址很有可能会发生变化，这就导致和我们`MySQL`容器相关联的其他正在运行的容器崩溃。我们能不能以不直接指定`IP`地址的方式来进行工作，而是通过指定其他的一种形式来完成与`MySQL` 容器的关联及相关动作。

### 13. 在Docker中创建自定义虚拟网路

* 创建自定义网络命令：

```bash
// frontend网络用于接收来自MVC容器的Http请求
docker network create frontend

// backend网络将被用于在MVC容器和MySQL容器之间进行数据查询
docker network create backend

// 查询创建的自定义网络
docker network ls
```

* 将容器连接到自定义网络
  * 创建一个新的数据库容器，然后将它链接到后端网络中

```bash
docker run -d --name mysql -v productdata:/var/lib/mysql --network backend -e MYSQL_ROOT_PASSWORD=123456 -e bind-address=0.0.0.0 mysql:latest
```
  * 创建创建多个`MVC`应用程序

```bash
docker create --name productapp1 -e DBHOST=mysql -e MESSAGE="第1台服务器" --network backend yoyomooc/exampleapp

docker create --name productapp2 -e DBHOST=mysql -e MESSAGE="第2台服务器" --network backend yoyomooc/exampleapp

docker create --name productapp3 -e DBHOST=mysql -e MESSAGE="第3台服务器" --network backend yoyomooc/exampleapp
```

`docker run`和`docker create`命令只能将容器连接到一个网络。当前的命令中我们指定容器链接了`backend`网络。但是我们同时也需要将`mvc`容器连接到前端网络`frontend`中。

* 容器继续连接网络命令如下:

```bash
docker network connect frontend productapp1

docker network connect frontend productapp2

docker network connect frontend productapp3
```

* 现在我们启动这些容器

```bash
docker start productapp1 productapp2 productapp3
```

当前我们的`MVC` 容器被创建了，但是都没有映射端口，这意味着它们只能通过Docker的虚拟网络进行访问，我们无法通过主机的操作系统访问。

* 使用 `haproxy` 实现多个Docker容器组网实现负载均衡

  * 添加 `haproxy` 配置文件 `haproxy.cfg` 

```bash
defaults
    timeout connect 5000
    timeout client 50000
    timeout server 50000

frontend localnodes
    bind *:80
    mode http
    default_backend mvc

backend mvc
    mode http
    balance roundrobin
    server mvc1 produuctapp1:80
    server mvc2 produuctapp2:80
    server mvc3 produuctapp3:80
```

* 创建负载均衡容器

```bash
docker run -d --name loadbalancer --network frontend -v "$(pwd)/haproxy.cfg:/usr/local/etc/haproxy/haproxy.cfg" -p 3000:80 haproxy:1.7.0
```

创建成功后，我们就可以在浏览器中输入:  http://localhost:3000 进行访问。

**5个容器之间的相互调用流程**

![image-20201203102018905](https://gitee.com/zhujinrun/image/raw/master/qing/2020/image-20201203102018905.png)

### 14. Docker-compose

* `docker-compose.mysql.yml`

```yml
version: "3.8"

volumes: 
  productdata

networks: 
  frontend:
  backend:

services: 
  mysql:
    image: "mysql:8.0.0"
    volumes: 
      - productdata:/var/lib/mysql
    networks: 
      - backend
    environment: 
      - MYSQL_ROOT_PASSWORD=123456
      - bind-address=0.0.0.0

dbinit:
  image: "registry.cn-hangzhou.aliyuncs.com/yoyosoft/exampleapp:mysql"
  networks: 
    - backend
  environment: 
    - INITDB=true
    - DBHOST=mysql
  depends_on: 
    - mysql
  
mvc:
  image: "registry.cn-hangzhou.aliyuncs.com/yoyosoft/exampleapp:mysql"
  networks: 
    - backend
    - frontend
  environment: 
    - DBHOST=mysql
  depends_on: 
    - mysql 
loadbalancer:
  image: "dockercloud/haproxy:1.2.1"
  ports: 
    - 3000:80
  links: 
    - mvc
  volumes: 
    - /var/run/docker.sock:/var/run/dock.sock
  networks: 
    - frontend

```

* 运行 docker-compose 文件

```bash
docker-compose -f docker-compose.mysql.yml up
```

* 快速创建4个mvc容器

```bash
docker-compose -f docker-compose.mysql.yml scale mvc=4
```

