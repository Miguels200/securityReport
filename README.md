# securityReport

SecurityReport backend - ASP.NET Core

See `src/` for projects: Api, Application, Infrastructure, Domain

Security Report - Backend

Estructura:
- src/Api - API REST (ASP.NET Core)
- src/Application - Lógica de aplicación, CQRS, DTOs
- src/Domain - Entidades de dominio
- src/Infrastructure - Persistencia, Azure OpenAI, Logging, Background Jobs
- tests - Unitarias e integración

Variables de entorno (mínimas):
- ASPNETCORE_ENVIRONMENT
- DB_CONNECTION
- JWT_ISSUER
- JWT_AUDIENCE
- JWT_SECRET
- AZURE_OPENAI_ENDPOINT
- AZURE_OPENAI_API_KEY
- AZURE_OPENAI_DEPLOYMENT

Advertencia legal (mostrar en respuestas IA):
“Este análisis es un apoyo a la toma de decisiones del responsable del SG?SST. La IA no toma decisiones ni ejecuta acciones.”

Docker y CI listos. Sigue el código en `src`.

Uso local

- Restaurar y compilar:
  - `dotnet restore src/SecurityReport.sln`
  - `dotnet build src/SecurityReport.sln -c Release`

- Ejecutar tests (unitarios + integración con Testcontainers):
  - `dotnet test tests/Tests.csproj`

- Ejecutar API localmente (configura variables de entorno antes):
  - `dotnet run --project src/Api/Api.csproj`

- Docker local:
  - `docker build -t securityreport:local .`
  - `docker run -e DB_CONNECTION="<cadena>" -e JWT_SECRET="<secret>" -p 5000:80 securityreport:local`

CI/CD

- GitHub Actions incluido en `.github/workflows/ci.yml`:
  - construye, ejecuta tests (Testcontainers), crea imagen y opcionalmente publica en ACR y despliega a Azure Web App.

Secrets recomendados para CI/Producción (usar GitHub Secrets o Key Vault)
- ACR_LOGIN_SERVER, ACR_USERNAME, ACR_PASSWORD
- AZURE_WEBAPP_NAME
- DB_CONNECTION
- JWT_SECRET, JWT_ISSUER, JWT_AUDIENCE
- SERVICEBUS_CONNECTION, SERVICEBUS_QUEUE_ANALYSIS
- AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT
- BLOB_CONNECTION
- APPINSIGHTS_INSTRUMENTATIONKEY (opcional)
- AZURE_KEYVAULT_URI (opcional)

Despliegue y seguridad

- No almacenar secretos en el repositorio. Usar Key Vault y GitHub Secrets.
- En producción usar Azure Service Bus y ACR; habilitar Application Insights y monitorización.

Notas

- Las migraciones EF están disponibles en `src/Infrastructure/Migrations`. Si prefieres aplicar migraciones automáticamente, ejecuta `dotnet ef database update` apuntando a la DB.
- Para ejecutar integraciones en CI asegúrate de configurar los secrets y permisos necesarios.