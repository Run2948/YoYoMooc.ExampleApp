# dotnet restore
# dotnet publish --framework netcoreapp3.1 --configuration Release --output dist
# docker build . -t yoyomooc/exampleapp -f Dockerfile

# mcr.microsoft.com/dotnet/core/aspnet:3.1  registry.cn-hangzhou.aliyuncs.com/yoyosoft/dotnet/core/aspnet:3.1
# mcr.microsoft.com/dotnet/core/sdk:3.1     registry.cn-hangzhou.aliyuncs.com/yoyosoft/dotnet/core/sdk:3.1
# mcr.microsoft.com/dotnet/core/runtime:3.1 registry.cn-hangzhou.aliyuncs.com/yoyosoft/dotnet/core/runtime:3.1

# FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
FROM registry.cn-hangzhou.aliyuncs.com/yoyosoft/dotnet/core/aspnet:3.1
COPY dist /app
WORKDIR /app
EXPOSE 80/tcp
ENTRYPOINT ["dotnet","YoYoMooc.ExampleApp.dll"]
