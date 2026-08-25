import React, { useState, useEffect } from 'react';
import { Card, CardBody, CardHeader, Row, Col, Badge, Button } from 'reactstrap';
import { ApiService } from '../services/apiService';
import { AuthService } from '../services/authService';
import Loading from '../components/Loading';
import Alert from '../components/Alert';

interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  approvedUsers: number;
  pendingApproval: number;
  lockedOutUsers: number;
  totalRoles: number;
  auditLogs: number;
  dataExportRequests: number;
  accountDeletionRequests: number;
}

const DashboardPage: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const user = AuthService.getCurrentUser();

  useEffect(() => {
    const fetchStats = async () => {
      try {
        // In a real implementation, this would call the API
        // For now, we'll use mock data
        const mockStats: DashboardStats = {
          totalUsers: 42,
          activeUsers: 38,
          approvedUsers: 35,
          pendingApproval: 7,
          lockedOutUsers: 2,
          totalRoles: 5,
          auditLogs: 1250,
          dataExportRequests: 12,
          accountDeletionRequests: 3
        };
        setStats(mockStats);
        
        // Uncomment when API is implemented
        // const response = await ApiService.getDashboardStats();
        // setStats(response);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load dashboard statistics');
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  const statCards = [
    { 
      title: 'Total Users', 
      value: stats?.totalUsers || 0, 
      icon: 'bi bi-people',
      color: 'primary',
      badge: 'Users'
    },
    { 
      title: 'Active Users', 
      value: stats?.activeUsers || 0, 
      icon: 'bi bi-person-check',
      color: 'success',
      badge: 'Active'
    },
    { 
      title: 'Pending Approval', 
      value: stats?.pendingApproval || 0, 
      icon: 'bi bi-hourglass-split',
      color: 'warning',
      badge: 'Pending'
    },
    { 
      title: 'Locked Out', 
      value: stats?.lockedOutUsers || 0, 
      icon: 'bi bi-lock-fill',
      color: 'danger',
      badge: 'Locked'
    },
    { 
      title: 'Roles', 
      value: stats?.totalRoles || 0, 
      icon: 'bi bi-person-workspace',
      color: 'info',
      badge: 'Roles'
    },
    { 
      title: 'Audit Logs', 
      value: stats?.auditLogs || 0, 
      icon: 'bi bi-file-earmark-text',
      color: 'secondary',
      badge: 'Logs'
    },
    { 
      title: 'GDPR Requests', 
      value: (stats?.dataExportRequests || 0) + (stats?.accountDeletionRequests || 0), 
      icon: 'bi bi-shield-fill-check',
      color: 'dark',
      badge: 'Requests'
    },
  ];

  return (
    <div className="fade-in">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>
          <i className="bi bi-speedometer2 me-2"></i>
          Dashboard
        </h1>
        <div>
          <Button color="primary" className="me-2">
            <i className="bi bi-arrow-clockwise me-1"></i>
            Refresh
          </Button>
        </div>
      </div>

      {error && <Alert color="danger" message={error} autoClose onDismiss={() => setError(null)} />}

      {loading ? (
        <Loading message="Loading dashboard..." />
      ) : (
        <Row>
          {statCards.map((stat, index) => (
            <Col key={index} sm="6" lg="3" className="mb-4">
              <Card className="h-100 stat-card">
                <CardBody className="text-center">
                  <div className={`text-${stat.color} mb-3`}>
                    <i className={stat.icon} style={{ fontSize: '2.5rem' }}></i>
                  </div>
                  <div className="stat-value">{stat.value}</div>
                  <div className="stat-label text-muted">{stat.title}</div>
                  <Badge color={stat.color} className="mt-2">{stat.badge}</Badge>
                </CardBody>
              </Card>
            </Col>
          ))}

          <Col md="12">
            <Card>
              <CardHeader>
                <h5 className="mb-0">
                  <i className="bi bi-person-circle me-2"></i>
                  Welcome, {user?.username}
                </h5>
              </CardHeader>
              <CardBody>
                <Row>
                  <Col md="6">
                    <h6>Account Information</h6>
                    <ul className="list-unstyled">
                      <li><strong>Email:</strong> {user?.email}</li>
                      <li><strong>Roles:</strong> {user?.roles?.join(', ') || 'None'}</li>
                      <li><strong>Status:</strong> {user?.isActive ? 'Active' : 'Inactive'}</li>
                      <li><strong>Approval:</strong> {user?.isApproved ? 'Approved' : 'Pending'}</li>
                    </ul>
                  </Col>
                  <Col md="6">
                    <h6>Quick Actions</h6>
                    <div className="d-grid gap-2">
                      <Button color="primary" size="sm" tag="a" href="/users">
                        <i className="bi bi-people me-1"></i> Manage Users
                      </Button>
                      <Button color="info" size="sm" tag="a" href="/roles">
                        <i className="bi bi-person-workspace me-1"></i> Manage Roles
                      </Button>
                      <Button color="warning" size="sm" tag="a" href="/audit-logs">
                        <i className="bi bi-file-earmark-text me-1"></i> View Audit Logs
                      </Button>
                      <Button color="dark" size="sm" tag="a" href="/gdpr">
                        <i className="bi bi-shield-fill-check me-1"></i> GDPR Compliance
                      </Button>
                    </div>
                  </Col>
                </Row>
              </CardBody>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  );
};

export default DashboardPage;
