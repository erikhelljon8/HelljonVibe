import React from 'react';
import { Spinner } from 'reactstrap';

interface LoadingProps {
  message?: string;
}

const Loading: React.FC<LoadingProps> = ({ message = 'Loading...' }) => {
  return (
    <div className="d-flex flex-column align-items-center justify-content-center" style={{ minHeight: '200px' }}>
      <Spinner color="primary" type="grow" className="mb-3" style={{ width: '3rem', height: '3rem' }} />
      <p className="text-muted">{message}</p>
    </div>
  );
};

export default Loading;
