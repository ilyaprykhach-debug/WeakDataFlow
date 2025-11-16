import { useState } from 'react';
import { ApolloProvider } from '@apollo/client';
import { apolloClient } from './config/apollo';
import { useSignalR } from './hooks/useSignalR';
import { LatestValues } from './components/LatestValues';
import { Charts } from './components/Charts';
import { Aggregations } from './components/Aggregations';
import { SearchFilter } from './components/SearchFilter';
import { NotificationToast } from './components/NotificationToast';
import type { NotificationEvent } from './types/graphql';
import './App.css';

function AppContent() {
  const [notifications, setNotifications] = useState<NotificationEvent[]>([]);
  const [activeTab, setActiveTab] = useState<'latest' | 'charts' | 'aggregations' | 'search'>('latest');

  const handleNotification = (notification: NotificationEvent) => {
    setNotifications((prev) => [notification, ...prev].slice(0, 5));
  };

  const { isConnected } = useSignalR(handleNotification);

  const removeNotification = (index: number) => {
    setNotifications((prev) => prev.filter((_, i) => i !== index));
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>Sensor Data Dashboard</h1>
        <div className="header-info">
          <div className={`connection-status ${isConnected ? 'connected' : 'disconnected'}`}>
            <span className="status-dot"></span>
            {isConnected ? 'Connected' : 'Disconnected'}
          </div>
        </div>
      </header>

      <nav className="app-nav">
        <button
          className={activeTab === 'latest' ? 'active' : ''}
          onClick={() => setActiveTab('latest')}
        >
          Latest Values
        </button>
        <button
          className={activeTab === 'charts' ? 'active' : ''}
          onClick={() => setActiveTab('charts')}
        >
          Charts
        </button>
        <button
          className={activeTab === 'aggregations' ? 'active' : ''}
          onClick={() => setActiveTab('aggregations')}
        >
          Aggregations
        </button>
        <button
          className={activeTab === 'search' ? 'active' : ''}
          onClick={() => setActiveTab('search')}
        >
          Search & Filter
        </button>
      </nav>

      <main className="app-main">
        <div className="notifications-container">
          {notifications.map((notification, index) => (
            <NotificationToast
              key={index}
              notification={notification}
              onClose={() => removeNotification(index)}
            />
          ))}
        </div>

        {activeTab === 'latest' && <LatestValues />}
        {activeTab === 'charts' && <Charts />}
        {activeTab === 'aggregations' && <Aggregations />}
        {activeTab === 'search' && <SearchFilter />}
      </main>

      <footer className="app-footer">
        <p>© Developed by Ilya Prykhach</p>
      </footer>
    </div>
  );
}

function App() {
  return (
    <ApolloProvider client={apolloClient}>
      <AppContent />
    </ApolloProvider>
  );
}

export default App;
