using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace AccountPlugin
{
    public class AccountPlugin : IPlugin
    {
        //Create a note of old address whenever address is changed, if prior address has value.
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService orgService = factory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters != null && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "account") { return; }
                try
                {
                    tracingService.Trace("AccountPlugin: Creating Pre Image and Post Image Account entity.");
                   Entity accountPreImage = (Entity)context.PreEntityImages["Image"];
                    Entity accountPostImage = (Entity)context.PostEntityImages["Image"];
                    if (accountPreImage.Contains("address1_composite") && accountPreImage.Contains("address1_composite"))
                    {
                        tracingService.Trace("Compare Before and After address.");
                        if (accountPreImage.GetAttributeValue<String>("address1_composite")!=accountPostImage.GetAttributeValue<String>("address1_composite")
                                &&accountPreImage.GetAttributeValue<String>("address1_composite") != null)
                        {
                            Entity annotation = new Entity("annotation");
                            annotation["objectid"] = new EntityReference(accountPostImage.LogicalName, accountPostImage.Id);
                            annotation["subject"] = "Previous Address:";
                            annotation["notetext"] = accountPreImage["address1_composite"];
                            annotation["objecttypecode"] = accountPostImage.LogicalName;
                            tracingService.Trace("AccountPlugin: Updating Note.");
                            orgService.Create(annotation);
                        }
                    }


                }
                catch (Exception ex)
                {
                    tracingService.Trace("AccountPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

    }
}
