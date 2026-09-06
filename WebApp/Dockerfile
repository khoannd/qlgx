# Một image duy nhất phục vụ cả API và tệp tĩnh của web — quyết định GHI RÕ Ở ĐÂY (Task 13):
# cho pilot vài giáo xứ, một container/một health check/một phiên bản không thể lệch nhau giữa
# API và web đơn giản hơn cho vận hành so với hai image + một reverse proxy nội bộ chỉ để nối
# chúng lại. Cái giá phải trả: build lại toàn bộ khi chỉ sửa web, và không scale API/web độc
# lập — chấp nhận được ở quy mô pilot. Xem TRIEN-KHAI.md để tách lại thành hai image nếu sau
# này cần scale riêng.
#
# Build từ thư mục WebApp: `docker build -f Dockerfile ..` KHÔNG đúng — build context phải LÀ
# thư mục WebApp (chứa cả src/Qlgx.Api và src/web). Dùng docker-compose.yml (context: .) là
# cách chạy được kiểm chứng.

# ---- Giai đoạn 1: build web (React/Vite) ----
FROM node:22-alpine AS build-web
WORKDIR /web
COPY src/web/package.json src/web/package-lock.json ./
RUN npm ci
COPY src/web/ ./
RUN npm run build

# ---- Giai đoạn 2: build API (.NET) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-api
WORKDIR /src
COPY Qlgx.sln ./
COPY src/Qlgx.Domain/Qlgx.Domain.csproj src/Qlgx.Domain/
COPY src/Qlgx.Data/Qlgx.Data.csproj src/Qlgx.Data/
COPY src/Qlgx.Api/Qlgx.Api.csproj src/Qlgx.Api/
COPY src/Qlgx.Migration.Core/Qlgx.Migration.Core.csproj src/Qlgx.Migration.Core/
COPY src/Qlgx.Migration/Qlgx.Migration.csproj src/Qlgx.Migration/
COPY tests/Qlgx.Api.Tests/Qlgx.Api.Tests.csproj tests/Qlgx.Api.Tests/
COPY tests/Qlgx.Data.Tests/Qlgx.Data.Tests.csproj tests/Qlgx.Data.Tests/
COPY tests/Qlgx.Migration.Tests/Qlgx.Migration.Tests.csproj tests/Qlgx.Migration.Tests/
RUN dotnet restore src/Qlgx.Api/Qlgx.Api.csproj
COPY src/ src/
RUN dotnet publish src/Qlgx.Api/Qlgx.Api.csproj -c Release -o /app/publish --no-restore

# ---- Giai đoạn 3: image chạy — chỉ ASP.NET runtime, không SDK ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
# curl chỉ để docker-compose healthcheck gọi /api/suc-khoe từ BÊN TRONG container — ảnh nền
# chính thức không có sẵn công cụ gọi HTTP nào.
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build-api /app/publish .
COPY --from=build-web /web/dist ./wwwroot
# Không chạy bằng root trong container — ảnh nền chính thức đã có sẵn user "app" không đặc
# quyền. Đặt USER SAU khi COPY xong để các tệp copy vào không bị thiếu quyền đọc/ghi cần thiết.
USER app
EXPOSE 8080
# ASPNETCORE_URLS đặt qua biến môi trường trong docker-compose.yml, KHÔNG cố định ở đây, để
# không phải build lại image khi đổi cổng.
ENTRYPOINT ["dotnet", "Qlgx.Api.dll"]
