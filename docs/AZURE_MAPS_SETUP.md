# Azure Maps Setup Guide

## Overview
This guide explains how to configure Azure Maps for the GreenSync application to display waste report locations on an interactive map.

## The Issue
When running GreenSync-app without proper Azure Maps configuration, you'll see an error message:
- **"Issue initializing map"** or
- **"Map configuration is missing. Please add your Azure Maps subscription key to appsettings.json"**

This happens because the application needs valid Azure Maps credentials to render the map component.

## Solution

### Option 1: Use Azure Maps (Recommended for Production)

#### Step 1: Create an Azure Maps Account
1. Go to the [Azure Portal](https://portal.azure.com)
2. Click **"Create a resource"**
3. Search for **"Azure Maps"**
4. Click **"Create"** and fill in the required details:
   - **Subscription**: Select your Azure subscription
   - **Resource Group**: Create new or use existing
   - **Name**: Choose a unique name for your Azure Maps account
   - **Pricing Tier**: Select **"Gen2"** (recommended) or **"Gen1 S0"** (free tier with limitations)
5. Click **"Review + Create"** then **"Create"**

#### Step 2: Get Your Subscription Key
1. Once deployed, go to your Azure Maps account
2. In the left menu, click **"Authentication"** under **"Settings"**
3. Copy the **"Primary Key"** or **"Secondary Key"**

#### Step 3: Configure the Application
1. Open `GreenSync-app/appsettings.json`
2. Replace `YOUR_AZURE_MAPS_SUBSCRIPTION_KEY_HERE` with your actual subscription key:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "your-actual-subscription-key-here"
  }
}
```

3. For development, you can also add it to `appsettings.Development.json`:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "your-actual-subscription-key-here"
  }
}
```

#### Step 4: Secure Your Key (Important!)
- **Never commit your subscription key to source control**
- Add `appsettings.Development.json` to your `.gitignore`
- For production, use Azure Key Vault or environment variables:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "${AZURE_MAPS_SUBSCRIPTION_KEY}"
  }
}
```

### Option 2: Use Environment Variables

You can also set the subscription key as an environment variable:

**Windows (PowerShell):**
```powershell
$env:AzureMaps__SubscriptionKey="your-subscription-key-here"
```

**Windows (Command Prompt):**
```cmd
set AzureMaps__SubscriptionKey=your-subscription-key-here
```

**Linux/Mac:**
```bash
export AzureMaps__SubscriptionKey="your-subscription-key-here"
```

### Option 3: Development Without Azure Maps

If you don't have an Azure Maps subscription, the application will still run but show a user-friendly error message on the map component. All other features will work normally.

The map will display:
- A warning icon
- Message: "Map configuration is missing. Please add your Azure Maps subscription key to appsettings.json"
- Contact support message

## Troubleshooting

### Map Still Shows Error After Configuration
1. **Verify the subscription key is correct**
   - Check for extra spaces or missing characters
   - Ensure you copied the entire key

2. **Restart the application**
   - Stop the application (Ctrl+C in terminal)
   - Run `dotnet run` again

3. **Check the browser console**
   - Press F12 to open Developer Tools
   - Look for errors in the Console tab
   - Common issues:
     - CORS errors: Check Azure Maps account CORS settings
     - 401 Unauthorized: Invalid subscription key
     - 403 Forbidden: Key doesn't have required permissions

4. **Verify Azure Maps account is active**
   - Log into Azure Portal
   - Check that the Azure Maps account is not disabled
   - Verify you're using the correct subscription key

### Map Loads But No Markers Appear
This is normal if you don't have any waste reports yet. The map will show a message:
- "No waste reports to display"
- "Submit your first report to see it on the map!"

Create a waste report with valid latitude/longitude coordinates to see markers on the map.

## Features

Once configured, the Azure Maps integration provides:
- **Interactive Map Display**: Pan, zoom, and rotate the map
- **Waste Report Markers**: Visual indicators for each reported location
- **Popup Details**: Click markers to see report details
- **Map Controls**: 
  - Zoom in/out
  - Compass navigation
  - Pitch control (3D tilt)
  - Map style selector
- **Auto-fitting**: Map automatically adjusts to show all report markers
- **Responsive Design**: Works on desktop and mobile devices

## Cost Considerations

### Gen2 Pricing (Recommended)
- Pay-as-you-go model
- First 1,000 transactions free per month
- Typical usage: ~$0.005 per 1,000 map views for small applications

### Gen1 S0 (Free Tier)
- Limited to 250,000 transactions per month
- Good for development and small applications
- No credit card required for free tier

For detailed pricing, visit: [Azure Maps Pricing](https://azure.microsoft.com/en-us/pricing/details/azure-maps/)

## Security Best Practices

1. **Use Azure Key Vault** for production deployments
2. **Rotate keys regularly** (every 90 days recommended)
3. **Monitor usage** in Azure Portal to detect unauthorized access
4. **Implement rate limiting** in your application
5. **Use CORS restrictions** in Azure Maps settings to limit which domains can use your key
6. **Never expose keys in client-side code** (except as needed for map rendering with proper CORS)

## Additional Resources

- [Azure Maps Documentation](https://docs.microsoft.com/en-us/azure/azure-maps/)
- [Azure Maps Web SDK](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-use-map-control)
- [Azure Maps Samples](https://samples.azuremaps.com/)

## Support

If you continue to experience issues:
1. Check the application logs for detailed error messages
2. Verify all files were updated correctly during the fix
3. Review this documentation for missing steps
4. Contact the development team for assistance
