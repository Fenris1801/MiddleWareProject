using System;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel;

[AttributeUsage(AttributeTargets.Class)]
public class CorsBehaviorAttribute : Attribute, IServiceBehavior
{
    public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
        System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

    public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
        foreach (ChannelDispatcher chDisp in serviceHostBase.ChannelDispatchers)
        {
            foreach (EndpointDispatcher epDisp in chDisp.Endpoints)
            {
                epDisp.DispatchRuntime.MessageInspectors.Add(new CorsMessageInspector());
            }
        }
    }

    public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
}

public class CorsMessageInspector : IDispatchMessageInspector
{
    public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext) => null;

    public void BeforeSendReply(ref Message reply, object correlationState)
    {
        var http = reply.Properties.ContainsKey(HttpResponseMessageProperty.Name)
            ? (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name]
            : null;

        if (http == null)
        {
            http = new HttpResponseMessageProperty();
            reply.Properties.Add(HttpResponseMessageProperty.Name, http);
        }

        http.Headers.Set("Access-Control-Allow-Origin", "*");
        http.Headers.Set("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        http.Headers.Set("Access-Control-Allow-Headers", "Content-Type, Accept");
    }
}