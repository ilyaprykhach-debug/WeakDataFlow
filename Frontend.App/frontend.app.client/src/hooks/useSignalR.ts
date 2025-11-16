import { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { createSignalRConnection } from '../config/signalr';
import type { NotificationEvent } from '../types/graphql';

export const useSignalR = (onNotification?: (notification: NotificationEvent) => void) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const newConnection = createSignalRConnection();
    connectionRef.current = newConnection;
    setConnection(newConnection);

    newConnection
      .start()
      .then(() => {
        console.log('SignalR Connected');
        setIsConnected(true);
      })
      .catch((err) => {
        console.error('SignalR Connection Error: ', err);
        setIsConnected(false);
      });

    newConnection.onclose(() => {
      console.log('SignalR Disconnected');
      setIsConnected(false);
    });

    newConnection.onreconnecting(() => {
      console.log('SignalR Reconnecting...');
      setIsConnected(false);
    });

    newConnection.onreconnected(() => {
      console.log('SignalR Reconnected');
      setIsConnected(true);
    });

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, []);

  useEffect(() => {
    if (connection && onNotification) {
      connection.on('NotificationReceived', (notification: NotificationEvent) => {
        onNotification(notification);
      });

      return () => {
        connection.off('NotificationReceived');
      };
    }
  }, [connection, onNotification]);

  return { connection, isConnected };
};

