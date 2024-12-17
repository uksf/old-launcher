using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace UKSF.Launcher.Network {
    public class HubWrapper {
        public static async Task<HubConnection> Connecthub(string path) {
            HubConnection hubConnection = new HubConnectionBuilder().WithUrl($"{Global.URL}/hub/{path}", options => options.AccessTokenProvider = () => Task.FromResult(ApiWrapper.Token)).Build();

            hubConnection.Closed += async exception => {
                await Task.Delay(100);
                await hubConnection.StartAsync();
            };
            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}
