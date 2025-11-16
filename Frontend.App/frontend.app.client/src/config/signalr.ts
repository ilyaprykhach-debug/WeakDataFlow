import * as signalR from '@microsoft/signalr';

const NOTIFICATION_URI = import.meta.env.VITE_NOTIFICATION_URI || 'http://localhost:5003/notificationHub';

export const createSignalRConnection = () => {
  return new signalR.HubConnectionBuilder()
    .withUrl(NOTIFICATION_URI, {
      skipNegotiation: false,
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .build();
};

