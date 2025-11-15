import { useEffect } from 'react';
import type { NotificationEvent } from '../types/graphql';
import { format } from 'date-fns';
import './NotificationToast.css';

interface NotificationToastProps {
  notification: NotificationEvent;
  onClose: () => void;
}

export const NotificationToast = ({ notification, onClose }: NotificationToastProps) => {
  useEffect(() => {
    const timer = setTimeout(() => {
      onClose();
    }, 5000);

    return () => clearTimeout(timer);
  }, [onClose]);

  return (
    <div className="notification-toast" onClick={onClose}>
      <div className="notification-header">
        <span className="notification-service">{notification.serviceName}</span>
        <span className="notification-time">
          {format(new Date(notification.timestamp), 'HH:mm:ss')}
        </span>
      </div>
      <div className="notification-event">{notification.eventType}</div>
      {notification.message && (
        <div className="notification-message">{notification.message}</div>
      )}
    </div>
  );
};

