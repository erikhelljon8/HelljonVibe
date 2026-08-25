import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Card, CardBody, CardHeader, Form, FormGroup, Label, Input, Button, Alert } from 'reactstrap';
import { AuthService } from '../services/authService';
import Loading from '../components/Loading';

const LoginPage: React.FC = () => {
  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as any)?.from?.pathname || '/dashboard';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await AuthService.login(usernameOrEmail, password);
      navigate(from, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="d-flex align-items-center justify-content-center min-vh-100 bg-light">
      <div className="w-100" style={{ maxWidth: '400px' }}>
        <Card className="shadow">
          <CardHeader className="text-center">
            <h3>
              <i className="bi bi-shield-check me-2"></i>
              HelljonVibe Identity
            </h3>
            <p className="text-muted mb-0">Admin Portal</p>
          </CardHeader>
          <CardBody>
            {error && <Alert color="danger">{error}</Alert>}
            
            <Form onSubmit={handleSubmit}>
              <FormGroup>
                <Label for="usernameOrEmail">Username or Email</Label>
                <Input
                  type="text"
                  id="usernameOrEmail"
                  value={usernameOrEmail}
                  onChange={(e) => setUsernameOrEmail(e.target.value)}
                  placeholder="Enter username or email"
                  required
                  disabled={loading}
                />
              </FormGroup>

              <FormGroup className="mt-3">
                <Label for="password">Password</Label>
                <Input
                  type="password"
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter password"
                  required
                  disabled={loading}
                />
              </FormGroup>

              <div className="d-grid gap-2 mt-4">
                {loading ? (
                  <Loading message="Logging in..." />
                ) : (
                  <Button color="primary" type="submit" size="lg">
                    <i className="bi bi-box-arrow-in-right me-2"></i>
                    Login
                  </Button>
                )}
              </div>
            </Form>

            <div className="mt-3 text-center">
              <small className="text-muted">
                <a href="/forgot-password">Forgot password?</a>
              </small>
            </div>
          </CardBody>
        </Card>
      </div>
    </div>
  );
};

export default LoginPage;
