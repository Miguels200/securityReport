Azure Web App deployment - parameters and steps

Required GitHub Secrets

- ACR_LOGIN_SERVER: <your-acr-name>.azurecr.io
- ACR_USERNAME: service principal or ACR username
- ACR_PASSWORD: service principal password or ACR password
- AZURE_WEBAPP_NAME: name of the Azure Web App (or container app)
- SERVICEBUS_CONNECTION: connection string for Azure Service Bus (used by worker)
- AZURE_OPENAI_ENDPOINT
- AZURE_OPENAI_API_KEY
- AZURE_OPENAI_DEPLOYMENT
- BLOB_CONNECTION: Azure Blob Storage connection string
- DB_CONNECTION: SQL Server connection string for production
- JWT_SECRET
- APPINSIGHTS_INSTRUMENTATIONKEY (optional)
- AZURE_KEYVAULT_URI (optional)

Recommended Azure resources

- Azure Container Registry (ACR) to store Docker images
- Azure App Service (Linux) configured to pull from ACR, or Azure Web App for Containers
- Azure SQL Database (or managed instance) for production
- Azure Service Bus namespace with a queue named as in SERVICEBUS_QUEUE_ANALYSIS
- Azure Blob Storage for evidences
- Azure Key Vault for secrets
- Application Insights for telemetry

Manual deployment steps (CI will push image to ACR and deploy automatically if secrets set):

1. Create ACR and push credentials to GitHub Secrets.
2. Create Azure App Service (Linux) and configure it to use container from ACR, or allow CI to deploy via `azure/webapps-deploy` action.
3. Configure App Settings (from Key Vault or GitHub Secrets): DB_CONNECTION, SERVICEBUS_CONNECTION, BLOB_CONNECTION, AZURE_OPENAI_* variables, JWT_*.
4. Ensure Managed Identity or Service Principal has access to Key Vault and resources.

Local testing

- Build docker image locally: `docker build -t securityreport:local .`
- Run with env vars: `docker run -e DB_CONNECTION="Server=..." -e JWT_SECRET="..." -p 5000:80 securityreport:local`

Notes

- Do not store secrets in the repository. Use Key Vault and GitHub Secrets.
- Ensure network rules allow the app to reach Service Bus, SQL and Blob Storage.
