// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.AppService;

namespace WebAppPerformanceMonitoringAlerts
{
    public class Program
    {
        /**
         * This sample shows examples of configuring Metric Alerts for WebApp instance performance monitoring through app service plan.
         *  - Create a App Service plan
         *  - Setup an action group to trigger a notification to the heavy performance alerts
         *  - Create auto-mitigated metric alerts for the App Service plan when
         *    - average CPUPercentage on any of Web App instance (where Instance = *) over the last 5 minutes is above 80%
         */
        private static ResourceIdentifier? _resourceGroupId = null;
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                // ============================================================

                // Create an App Service plan
              
                // Create a resourceGroup
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();
                var rgName = Utilities.CreateRandomName("rgMonitor");
                Utilities.Log($"creating a resource group with name : {rgName}...");
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS2));
                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created a resource group with name: " + resourceGroup.Data.Name);

                //Create a App Service Plan
                Utilities.Log("Creating app service plan");
                var appServicePlanCollection = resourceGroup.GetAppServicePlans();
                var appServicePlanName = Utilities.CreateRandomName("HighlyAvailableWebApps");
                var appServicePlanData = new AppServicePlanData(AzureLocation.EastUS2)
                {
                    Sku = new AppServiceSkuDescription
                    {
                        Name = "P1",
                        Tier = "Premium",
                        Capacity = 1
                    },
                    IsReserved = false,
                    Kind = "app"
                };
                var appServicePlan = (await appServicePlanCollection.CreateOrUpdateAsync(WaitUntil.Completed, appServicePlanName, appServicePlanData)).Value;
                Utilities.Log("Created app service plan with name:" + appServicePlan.Data.Name);

                // ============================================================
               
                // Create an action group to send notifications in case activity log alert condition will be triggered
                Utilities.Log("Creating actionGroup...");
                var actionGroupName = Utilities.CreateRandomName("criticalPerformanceActionGroup");
                var actionGroupCollection = resourceGroup.GetActionGroups();
                Uri uri = new Uri("https://www.weeneedmorepower.performancemonitoring.com");
                var actionGroupData = new ActionGroupData(AzureLocation.NorthCentralUS)
                {
                    GroupShortName = "AG",
                    IsEnabled = true,
                    AzureAppPushReceivers =
                    {
                        new MonitorAzureAppPushReceiver("MAAPRtierOne","ops_on_duty@performancemonitoring.com")
                    },
                    EmailReceivers =
                    {
                        new MonitorEmailReceiver("MERtierOne","ops_on_duty@performancemonitoring.com"),
                        new MonitorEmailReceiver("MERtierTwo","ceo@performancemonitoring.com")
                    },
                    SmsReceivers =
                    {
                        new MonitorSmsReceiver("MSRtierOne","1","4255655665")
                    },
                    VoiceReceivers =
                    {
                        new MonitorVoiceReceiver("MVRtierOne","1","2062066050")
                    },
                    WebhookReceivers =
                    {
                        new MonitorWebhookReceiver("MWRtierOne",uri)
                    }
                };
                var actionGroup = (await actionGroupCollection.CreateOrUpdateAsync(WaitUntil.Completed, actionGroupName, actionGroupData)).Value;
                Utilities.Log("Created actionGroup with name:" + actionGroup.Data.Name);

                // ============================================================

                // Set a trigger to fire each time

                //Create MetricAlerts
                Utilities.Log("Creating MetricAlerts...");
                var metricAlertsCollection = resourceGroup.GetMetricAlerts();
                var metricAlertName = Utilities.CreateRandomName("metricAlert");
                var scopes = new List<string>()
                {
                    appServicePlan.Id,
                };
                var criteria = new MetricAlertSingleResourceMultipleMetricCriteria()
                {
                   AllOf =
                    {
                        new MetricCriteria("Metric1", "CPUPercentage", MetricCriteriaTimeAggregationType.Total, MetricCriteriaOperator.GreaterThan, 80)
                        {
                            Dimensions =
                            {
                                new Azure.ResourceManager.Monitor.Models.MetricDimension("Instance","Include",new List<string> {"*"})
                            }
                        }
                    }
                };
                var metricAlertData = new MetricAlertData("global",3,true,scopes,TimeSpan.FromMinutes(1),TimeSpan.FromMinutes(5),criteria)
                {
                    Actions =
                    {
                        new MetricAlertAction()
                        {
                            ActionGroupId = actionGroup.Id,
                            
                        }
                    },
                    Severity = 3,
                    Description = "This alert rule is for U5 - Single resource-multiple criteria - with dimensions - with star",
                };
                var metricAlerts = (await metricAlertsCollection.CreateOrUpdateAsync(WaitUntil.Completed, metricAlertName, metricAlertData)).Value;
                Utilities.Log("Created MetricAlerts with Name : " + metricAlerts.Data.Name);
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }
        public static async Task Main(string[] args)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);
                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}
