# Stage 1: Build - Sử dụng SDK để khôi phục và biên dịch
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép các tệp .csproj và file solution (.sln) nếu có
# Việc này giúp restore toàn bộ dependencies cho tất cả các dự án
COPY ElecWasteCollection.API/*.csproj ./ElecWasteCollection.API/
COPY ElecWasteCollection.Application/*.csproj ./ElecWasteCollection.Application/
COPY ElecWasteCollection.Domain/*.csproj ./ElecWasteCollection.Domain/
COPY ElecWasteCollection.Infrastructure/*.csproj ./ElecWasteCollection.Infrastructure/
# Giả định file solution (.sln) nằm ở thư mục gốc của context
# COPY ElecWasteCollection.sln . 

# Chạy restore (sử dụng solution hoặc dự án API)
# Sử dụng dự án API để restore các dependencies bên ngoài
RUN dotnet restore ElecWasteCollection.API/ElecWasteCollection.API.csproj

# Sao chép toàn bộ mã nguồn
COPY . .

# Chuyển context sang dự án API
WORKDIR /src/ElecWasteCollection.API

# Chạy lệnh PUBLISH. Xuất bản tệp vào thư mục /app/publish
# Đây là cách chuẩn để tách biệt tệp build và tệp runtime
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# 📢 SỬA LỖI: COPY TỪ /app/publish (của stage build) ĐẾN /app (của stage runtime)
COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "ElecWasteCollection.API.dll"]