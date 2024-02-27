$LOCATION = "eastus"
$BICEP_FILE="main.bicep"

# delete a deployment
az deployment sub  delete  --name b2cdeployment

# deploy the bicep file directly

az deployment sub  create --name b2cdeployment --template-file $BICEP_FILE   --parameters parameters.json --location $LOCATION -o json