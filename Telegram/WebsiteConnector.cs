
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram;

public class WebsiteConnector
{
    private readonly string _url;

    public WebsiteConnector(string url)
    {
        _url = url;
    }

    public async Task ValidateNumberOnWebsite(string code,string connectionId)
    {
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        var connection = new HubConnectionBuilder().WithUrl(_url,options =>
        {
            options.HttpMessageHandlerFactory = _ => httpClientHandler;
        }).WithAutomaticReconnect().Build();
        await connection.StartAsync();
        await connection.InvokeAsync("ValidateNumber", code, connectionId);
        await connection.StopAsync();
    }
}
