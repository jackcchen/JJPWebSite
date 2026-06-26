# Azure Static Web Apps CSV Reader

This repository contains a modern Angular frontend and a .NET 8 Azure Functions API for reading a CSV file from Azure Blob Storage with Microsoft Entra ID access control.

## Architecture

- Angular standalone app renders the CSV rows in a table.
- Azure Static Web Apps hosts the frontend.
- The included .NET isolated Azure Function exposes `GET /api/csv-items`.
- The Function reads a blob using `DefaultAzureCredential`.
- In Azure, `DefaultAzureCredential` uses the Static Web App managed identity.
- The managed identity needs `Storage Blob Data Reader` on the target storage account or container.

## Expected CSV Columns

The sample model expects these headers:

```csv
Id,Name,Category,Amount
1,Laptop,Hardware,1200.50
2,Keyboard,Hardware,75.00
3,Support Plan,Service,300.00
```

Update `api/CsvItem.cs` and the Angular table if your CSV has different columns.

## Local Development

Install frontend dependencies:

```bash
npm install
```

Copy Function settings:

```bash
cp api/local.settings.sample.json api/local.settings.json
```

Set these values in `api/local.settings.json`:

```json
{
  "STORAGE_ACCOUNT_NAME": "yourstorageaccount",
  "CSV_CONTAINER_NAME": "data",
  "CSV_BLOB_NAME": "items.csv"
}
```

For local Entra authentication, sign in with Azure CLI:

```bash
az login
```

Your signed-in user must have `Storage Blob Data Reader` access to the blob.

Run with the Static Web Apps CLI:

```bash
npm install -g @azure/static-web-apps-cli azure-functions-core-tools@4
swa start http://localhost:4200 --api-location api --run "npm start"
```

## Azure Deployment

1. Push this repository to GitHub.
2. Create an Azure Static Web App connected to the GitHub repository.
3. Set build details:
   - App location: `/`
   - API location: `api`
   - Output location: `dist/azure-swa-csv-reader/browser`
4. Add the repository secret `AZURE_STATIC_WEB_APPS_API_TOKEN` if Azure did not create it automatically.
5. Add Static Web App application settings:
   - `STORAGE_ACCOUNT_NAME`
   - `CSV_CONTAINER_NAME`
   - `CSV_BLOB_NAME`
6. Enable managed identity for the Static Web App.
7. Grant that identity `Storage Blob Data Reader` on the storage account or blob container.

## High Availability

Azure Static Web Apps provides globally distributed frontend hosting. For the data path, use zone-redundant or geo-redundant storage such as `ZRS` or `RA-GRS`, keep the API stateless, and consider caching when the CSV changes infrequently.
