import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { 
  Collapse, 
  Navbar, 
  NavbarBrand, 
  NavbarToggler, 
  Nav, 
  NavItem, 
  NavLink, 
  Button,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem
} from 'reactstrap';
import { AuthService } from '../services/authService';

const NavMenu: React.FC = () => {
  const [collapsed, setCollapsed] = useState(true);
  const [user, setUser] = useState<any>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const currentUser = AuthService.getCurrentUser();
    setUser(currentUser);
  }, []);

  const toggleNavbar = () => {
    setCollapsed(!collapsed);
  };

  const handleLogout = () => {
    AuthService.logout();
    navigate('/login');
  };

  const isAuthenticated = user !== null;

  return (
    <header>
      <Navbar className="navbar navbar-expand-sm navbar-toggleable-sm mb-3" expand="md">
        <NavbarBrand tag={Link} to="/">
          <i className="bi bi-shield-check me-2"></i>
          HelljonVibe Identity Admin
        </NavbarBrand>
        <NavbarToggler onClick={toggleNavbar} className="me-2" />
        <Collapse isOpen={!collapsed} navbar>
          <Nav navbar className="me-auto">
            {isAuthenticated && (
              <>
                <NavItem>
                  <NavLink tag={Link} to="/dashboard" className="text-white">
                    <i className="bi bi-speedometer2 me-1"></i> Dashboard
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} to="/users" className="text-white">
                    <i className="bi bi-people me-1"></i> Users
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} to="/roles" className="text-white">
                    <i className="bi bi-person-check me-1"></i> Roles
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} to="/audit-logs" className="text-white">
                    <i className="bi bi-file-earmark-text me-1"></i> Audit Logs
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} to="/gdpr" className="text-white">
                    <i className="bi bi-shield-fill-check me-1"></i> GDPR
                  </NavLink>
                </NavItem>
              </>
            )}
          </Nav>
          <Nav navbar>
            {isAuthenticated ? (
              <UncontrolledDropdown nav inNavbar>
                <DropdownToggle nav caret className="user-info">
                  <i className="bi bi-person-circle me-1"></i>
                  {user?.username}
                </DropdownToggle>
                <DropdownMenu end>
                  <DropdownItem tag={Link} to="/profile">
                    <i className="bi bi-person me-2"></i> Profile
                  </DropdownItem>
                  <DropdownItem divider />
                  <DropdownItem onClick={handleLogout}>
                    <i className="bi bi-box-arrow-right me-2"></i> Logout
                  </DropdownItem>
                </DropdownMenu>
              </UncontrolledDropdown>
            ) : (
              <Button color="login" tag={Link} to="/login">
                <i className="bi bi-box-arrow-in-right me-1"></i> Login
              </Button>
            )}
          </Nav>
        </Collapse>
      </Navbar>
    </header>
  );
};

export default NavMenu;
