---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
  services: Monitor
  platforms: dotnet
---

# Configuring metric alerts to be triggered on potential performance downgrade. #

 This sample shows examples of configuring Metric Alerts for WebApp instance performance monitoring through app service plan.
  - Create a App Service plan
  - Setup an action group to trigger a notification to the heavy performance alerts
  - Create auto-mitigated metric alerts for the App Service plan when
    - average CPUPercentage on any of Web App instance (where Instance = ) over the last 5 minutes is above 80%


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/monitor-dotnet-metric-alerts-on-critical-performance.git

    cd monitor-dotnet-metric-alerts-on-critical-performance

    dotnet build

    bin\Debug\net452\WebAppPerformanceMonitoringAlerts.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.