targetScope='resourceGroup'

// https://learn.microsoft.com/en-us/azure/templates/microsoft.azureactivedirectory/b2cdirectories?pivots=deployment-language-bicep


param b2cDomain string
param b2cDisplayName string
param b2cGeo string

// https://learn.microsoft.com/en-us/azure/active-directory-b2c/data-residency
param b2cCountryCode string

resource b2c 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' = {
  name: b2cDomain
  location: b2cGeo
  tags: {
    tagName1: 'tagValue1'
    tagName2: 'tagValue2'
  }
  sku: {
    name: 'PremiumP1'
    tier: 'A0'
  }
  properties: {
    createTenantProperties: {
      countryCode: b2cCountryCode
      displayName: b2cDisplayName
    }
  }
}
