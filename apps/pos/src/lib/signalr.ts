import * as signalR from '@microsoft/signalr'

const HUB_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'}/hubs/notifications`

export function createHubConnection(token?: string) {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, token ? { accessTokenFactory: () => token } : {})
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()
}
