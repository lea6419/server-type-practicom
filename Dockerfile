# ��� ������
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

WORKDIR /source


COPY . .

WORKDIR /source/WebApplication1.Api

ARG TARGETARCH

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

# ��� �����
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final

WORKDIR /app

COPY --from=build /app .

# ����� ����� ��-����
RUN adduser -D appuser


USER appuser

# ����� ����� ������ ASPNETCORE_URLS �-HTTP �� ���� 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# ����� ������ �����
ENTRYPOINT ["dotnet", "WebApplication1.Api.dll"]

# ����� ���� ������� ������ ������
COPY *.csproj ./
RUN dotnet restore

# ����� ��� ������ ������ ���������
COPY . ./
RUN dotnet publish -c Release -o /out

