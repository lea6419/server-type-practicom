# ----------------------------------------------------------------------------------------------------
# שלב 1: בנייה (SDK)
# ----------------------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# העתקת קבצי הפרויקט
COPY Core/Core.csproj ./Core/
COPY Data/Data.csproj ./Data/
COPY Service/Service.csproj ./Service/
COPY WebApplication1/WebApplication1.csproj ./WebApplication1/

# שחזור תלויות (NuGet) עבור כל פרויקט בנפרד
RUN dotnet restore ./Core/Core.csproj
RUN dotnet restore ./Data/Data.csproj
RUN dotnet restore ./Service/Service.csproj
RUN dotnet restore ./WebApplication1/WebApplication1.csproj

# העתקת כל קוד הפרויקט
COPY Core ./Core/
COPY Data ./Data/
COPY Service ./Service/
COPY WebApplication1 ./WebApplication1/

# פרסום האפליקציה - מציין את הפרויקט במפורש
RUN dotnet publish ./WebApplication1/WebApplication1.csproj -c Release -o /app/out

# ----------------------------------------------------------------------------------------------------
# שלב 2: הפעלה (Runtime)
# ----------------------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "WebApplication1.dll"]