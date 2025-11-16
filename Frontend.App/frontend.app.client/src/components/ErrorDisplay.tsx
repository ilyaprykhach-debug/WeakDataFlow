import './ErrorDisplay.css';

interface ErrorDisplayProps {
  error: Error | string;
  onRetry?: () => void;
}

export const ErrorDisplay = ({ error, onRetry }: ErrorDisplayProps) => {
  const errorMessage = typeof error === 'string' ? error : error.message;

  return (
    <div className="error-display">
      <div className="error-icon">⚠️</div>
      <h3>Error</h3>
      <p>{errorMessage}</p>
      {onRetry && (
        <button onClick={onRetry} className="retry-button">
          Retry
        </button>
      )}
    </div>
  );
};

