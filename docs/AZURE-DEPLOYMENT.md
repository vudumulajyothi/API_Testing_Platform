# TestPilot – Microsoft Azure Deployment Guide

This guide walks through deploying the complete TestPilot full-stack platform to Microsoft Azure using **Azure App Service**, **Azure Static Web Apps**, and **Azure SQL Database**.

---

## 1. Cloud Architecture Topology

```
[ User Browser ]
       |
       +---> [ Azure Static Web Apps ] (Angular 18 Frontend)
                     |
                     | HTTPS REST /api/v1 + JWT
                     v
             [ Azure App Service (Linux .NET 8) ] (ASP.NET Core Web API)
                     |
                     | Encrypted Connection (TDS / port 1433)
                     v
             [ Azure SQL Database ] (Standard/Basic Serverless)
```

---

## 2. Prerequisites & Azure CLI Login

```bash
# Login to Azure
az login

# Set preferred subscription
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"

# Define variables
RESOURCE_GROUP="rg-testpilot-prod"
LOCATION="eastus"
SQL_SERVER="sql-testpilot-prod"
SQL_DB="sqldb-testpilot"
SQL_ADMIN="testpilotadmin"
SQL_PASSWORD="P@ssw0rdSecure2026!"
APP_SERVICE_PLAN="asp-testpilot-prod"
API_APP_NAME="app-testpilot-api"
STATIC_WEB_APP="swa-testpilot-ui"
```

---

## 3. Provisioning Infrastructure Step-by-Step

### Step 3.1: Create Resource Group
```bash
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### Step 3.2: Provision Azure SQL Database
```bash
# Create Logical SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

# Configure Firewall to Allow Azure Services
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create Database (Standard S0 or Basic tier for cost savings)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --service-objective "Basic"
```

### Step 3.3: Deploy ASP.NET Core Web API to Azure App Service
```bash
# Create Linux App Service Plan (.NET 8)
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --is-linux \
  --sku B1

# Create Web App with .NET 8 Runtime
az webapp create \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --name $API_APP_NAME \
  --runtime "DOTNETCORE:8.0"

# Build Connection String
SQL_CONN_STR="Server=tcp:$SQL_SERVER.database.windows.net,1433;Initial Catalog=$SQL_DB;Persist Security Info=False;User ID=$SQL_ADMIN;Password=$SQL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configure App Settings and Secrets
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $API_APP_NAME \
  --settings \
    ConnectionStrings__DefaultConnection="$SQL_CONN_STR" \
    JwtSettings__Secret="A_Very_Long_Production_Signing_Key_256_Bits_Secure_For_Azure!" \
    JwtSettings__Issuer="https://$API_APP_NAME.azurewebsites.net" \
    JwtSettings__Audience="https://$API_APP_NAME.azurewebsites.net" \
    Security__AllowLocalhostInDevelopment="false" \
    ASPNETCORE_ENVIRONMENT="Production"
```

### Step 3.4: Deploy Angular to Azure Static Web Apps
```bash
# Provision Azure Static Web App
az staticwebapp create \
  --name $STATIC_WEB_APP \
  --resource-group $RESOURCE_GROUP \
  --location "centralus" \
  --sku Free

# Get Frontend Host URL
FRONTEND_URL=$(az staticwebapp show --name $STATIC_WEB_APP --query "defaultHostname" -o tsv)

# Configure CORS on the Backend Web App
az webapp cors add \
  --resource-group $RESOURCE_GROUP \
  --name $API_APP_NAME \
  --allowed-origins "https://$FRONTEND_URL"
```

---

## 4. Production CI/CD Deployment with GitHub Actions
Create secret variables in your GitHub Repository (`Settings > Secrets and variables > Actions`):
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Downloaded publish profile from Azure App Service.
- `AZURE_STATIC_WEB_APPS_API_TOKEN`: Deployment token generated for Azure Static Web Apps.

Database migrations run automatically via EF Core database bundle or in the CI/CD pipeline step:
```bash
dotnet ef database update --connection "$SQL_CONN_STR"
```
