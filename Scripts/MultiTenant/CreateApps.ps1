# once only: Install-Module -Name MSAL.PS 

$conf = ".\appsettings.json"
$settings = Get-Content -Path $conf -ErrorAction Stop | Out-String | ConvertFrom-Json
# Add check whether the app names are stil available

$token = Get-MsalToken -clientId $settings.script.clientId -redirectUri $settings.script.redirectUri -Tenant $settings.b2cTenant -Scopes $settings.script.scopes

$headers = @{
   'Content-Type' = 'application/json';
   'Authorization' = ("Bearer {0}" -f $token.AccessToken);
}

############### Register apps in B2C ###########################
$url = "https://graph.microsoft.com/beta/applications"
$body = @{
    "displayName" = $settings.webApp.name;
    "isFallbackPublicClient" = $false;
    "web" = @{
        "redirectUris" = foreach($p in @("mtsusi-firsttenant", "mtsusi2", "redeem", "mtsusint")){ "https://{0}.azurewebsites.net/signin-{1}" -f $settings.webApp.name, $p };
        "implicitGrantSettings" = @{
          "enableAccessTokenIssuance" = $false;
          "enableIdTokenIssuance" = $false
        }
    };
    "identifierUris" = @(("https://{0}/{1}" -f $settings.b2cTenant, $settings.webApp.name));
}
$webAppReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)
$body.displayName = $settings.webAPI.name
$body.web.redirectUris = @(("https://{0}.com" -f $settings.webAPI.name))
$body.identifierUris = @(("https://{0}/b2crestapi" -f $settings.b2cTenant))
$webAPIReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

################## Register ServicePrincipals SP #########################
$body = @{
  "appId" = $webAppReg.appId;
  "displayName" = $webAppReg.displayName
}
$webAppSP = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/serviceprincipals"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
$body = @{
  "appId" = $webAPIReg.appId;
  "displayName" = $webAPIReg.displayName
}
$webAPISP = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/serviceprincipals"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)

$url = "https://graph.microsoft.com/beta/servicePrincipals?$filter=publisherName eq 'Microsoft Graph' or (displayName eq 'Microsoft Graph' or startswith(displayName,'Microsoft Graph'))"
$graphSP = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
$today = Get-Date
$body = @{
  "clientId" = $webAppSP.id;
  "consentType" = "AllPrincipals";
  "expiryTime" = (Get-Date -Year ($today.Year + 3) -Format o);
  "principalId" = $null;
  "resourceId" = ($graphSP.value[0].id)
  "scope" = "openid offline_access"
}
Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/oauth2PermissionGrants"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)

#################### Add scopes #########################
$body = @{
    "api" = @{
        "requestedAccessTokenVersion" = 2;
        "oauth2PermissionScopes" = @(
            @{
                "adminConsentDescription" = "Allow reading all members";
                "adminConsentDisplayName" = "Read members";
                "id" = (New-Guid);
                "isEnabled" = $true;
                "type" = "User";
                "userConsentDescription" = "Allow reading all members";
                "userConsentDisplayName" = "Read members";
                "value" = "Members.ReadAll"
            });
    }
}
$url = "https://graph.microsoft.com/beta/applications/{0}" -f $webAPIReg.id
$settings.script.scopes = Invoke-RestMethod -Uri $url  -Method Patch -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

##################### Grant permission to web app #########################
$body = @{
  "clientId" = $webAppSP.id;
  "consentType" = "AllPrincipals";
  "expiryTime" = (Get-Date -Year ($today.Year + 3) -Format o);
  "principalId" = $null;
  "resourceId" = $webAPISP.id
  "scope" = "Members.ReadAll"
}
Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/oauth2PermissionGrants"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)

################### Add secret key ###################################
Add-Type -AssemblyName System.Security
[Reflection.Assembly]::LoadWithPartialName("System.Security")
$rijndael = new-Object System.Security.Cryptography.RijndaelManaged
$rijndael.GenerateKey()
$secret = ([Convert]::ToBase64String($rijndael.Key))
$rijndael.Dispose()
$body = @{
    "passwordCredentials" = @(
    @{
      "customKeyIdentifier" = "ScriptRequest";
      "endDateTime" = "2299-12-31T00:00:00";
    })
}
$url = "https://graph.microsoft.com/beta/applications/{0}/addPassword" -f $webAppReg.id
$webAppCreds = Invoke-RestMethod -Uri $url  -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

