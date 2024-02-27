
targetScope='subscription'

param rgName string
param rgLocation string

@description('Domain name of the b2c (1st segment of xyz.onmicrosoft.com) ')
@minLength(6)
param b2cName string

@description('Display name ')
@minLength(6)
param b2cDisplayName string

// https://learn.microsoft.com/en-us/azure/active-directory-b2c/data-residency
@description('Geography (unitedstates,europe,asiapacific,australia,japan) ')
@allowed([
  'Global'
  'United States'
  'Europe'
  'Asia Pacific'
  'Australia'
])
param b2cGeo string
param b2cCountryCode string

resource b2cRG 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: rgLocation
}

var b2cDomain = '${b2cName}.onmicrosoft.com'
module b2c 'b2c.bicep' = {
  name: 'b2c'
  scope: resourceGroup(b2cRG.name)
  params: {
    b2cDomain: b2cDomain
    b2cDisplayName: b2cDisplayName
    b2cGeo: b2cGeo
    b2cCountryCode: b2cCountryCode
  }
}
