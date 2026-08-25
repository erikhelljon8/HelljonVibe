import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Container } from 'reactstrap';
import NavMenu from './components/NavMenu';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import UsersPage from './pages/UsersPage';
import RolesPage from './pages/RolesPage';
import AuditLogsPage from './pages/AuditLogsPage';
import GdprPage from './pages/GdprPage';
import ProfilePage from './pages/ProfilePage';
import PrivateRoute from './components/PrivateRoute';

const App: React.FC = () => {
  return (
    <div className="app">
      <NavMenu />
      <Container fluid className="mt-4">
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<PrivateRoute><DashboardPage /></PrivateRoute>} />
          <Route path="/dashboard" element={<PrivateRoute><DashboardPage /></PrivateRoute>} />
          <Route path="/users" element={<PrivateRoute roles={['Admin', 'UserManager']}><UsersPage /></PrivateRoute>} />
          <Route path="/roles" element={<PrivateRoute roles={['Admin']}><RolesPage /></PrivateRoute>} />
          <Route path="/audit-logs" element={<PrivateRoute roles={['Admin', 'Auditor']}><AuditLogsPage /></PrivateRoute>} />
          <Route path="/gdpr" element={<PrivateRoute roles={['Admin']}><GdprPage /></PrivateRoute>} />
          <Route path="/profile" element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Container>
    </div>
  );
};

export default App;
