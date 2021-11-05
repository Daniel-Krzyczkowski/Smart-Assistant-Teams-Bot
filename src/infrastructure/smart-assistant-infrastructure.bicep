param location string = resourceGroup().location
param environmentTypeSuffix string = 'dev'
param tenantId string = ''
param solutionName string = 'smart-assistant'
param funcAppsstorageAccountNamePrefix string = 'stsmartaccounting'
param msaAppIdForAzureBot string = ''

resource keyVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: 'kv-${solutionName}-${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: tenantId
    accessPolicies: []
  }
}

resource applicationInsights 'Microsoft.Insights/components@2015-05-01' = {
  name: 'appi-${solutionName}-${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    IngestionMode: 'ApplicationInsights'
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts@2021-04-15' = {
  name: 'cosmos-${solutionName}-${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  kind: 'GlobalDocumentDB'
  properties: {
    publicNetworkAccess: 'Enabled'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

resource appServicePlanForBotWebApp 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: 'plan-${solutionName}-${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  sku: {
    name: 'S1'
    capacity: 1
  }
}

resource azureWebAppForBot 'Microsoft.Web/sites@2021-01-15' = {
  name: 'app-${solutionName}-web-portal-${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  properties: {
    serverFarmId: appServicePlanForBotWebApp.id
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource functionAppsStorageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: '${funcAppsstorageAccountNamePrefix}${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  kind: 'StorageV2'
  sku: {
    name: 'Standard_ZRS'
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
  }
}

resource functionAppsHostingPlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: 'plan-func-apps-${solutionName}-${environmentTypeSuffix}'
  location: location
  tags: {
    Environment: environmentTypeSuffix
  }
  sku: {
    name: 'Y1' 
    tier: 'Dynamic'
  }
}

resource functionapp 'Microsoft.Web/sites@2020-06-01' = {
  name: 'func-${solutionName}-${environmentTypeSuffix}'
  location: location
  kind: 'functionapp'
  properties: {
    httpsOnly: true
    serverFarmId: functionAppsHostingPlan.id
    clientAffinityEnabled: true
    siteConfig: {
      appSettings: [
        {
          'name': 'APPINSIGHTS_INSTRUMENTATIONKEY'
          'value': applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppsStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(functionAppsStorageAccount.id, functionAppsStorageAccount.apiVersion).keys[0].value}'
        }
        {
          'name': 'FUNCTIONS_EXTENSION_VERSION'
          'value': '~3'
        }
        {
          'name': 'FUNCTIONS_WORKER_RUNTIME'
          'value': 'dotnet'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppsStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(functionAppsStorageAccount.id, functionAppsStorageAccount.apiVersion).keys[0].value}'
        }
      ]
    }
  }
}

resource azureBotService 'Microsoft.BotService/botServices@2021-05-01-preview' = {
  name: 'bot-${solutionName}-${environmentTypeSuffix}'
  location: 'global'
  tags: {
    Environment: environmentTypeSuffix
  }
  sku: {
    name: 'S1'
  }
  kind: 'azurebot'
  properties: {
    displayName: 'bot-${solutionName}-${environmentTypeSuffix}'
    msaAppId: msaAppIdForAzureBot
    msaAppType: 'MultiTenant'
    endpoint: 'https://${azureWebAppForBot.properties.defaultHostName}/api/messages'
  }
}
