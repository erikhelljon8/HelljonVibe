import React, { useState, useEffect } from 'react';
import { Alert as BootstrapAlert } from 'reactstrap';

interface AlertProps {
  color: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'light' | 'dark';
  message: string;
  autoClose?: boolean;
  duration?: number;
  onDismiss?: () => void;
}

const Alert: React.FC<AlertProps> = ({ 
  color, 
  message, 
  autoClose = false, 
  duration = 5000,
  onDismiss 
}) => {
  const [visible, setVisible] = useState(true);

  useEffect(() => {
    if (autoClose && visible) {
      const timer = setTimeout(() => {
        setVisible(false);
        onDismiss?.();
      }, duration);
      return () => clearTimeout(timer);
    }
  }, [autoClose, duration, visible, onDismiss]);

  const handleDismiss = () => {
    setVisible(false);
    onDismiss?.();
  };

  if (!visible) {
    return null;
  }

  return (
    <BootstrapAlert color={color} toggle={handleDismiss} fade={autoClose}>
      {message}
    </BootstrapAlert>
  );
};

export default Alert;
