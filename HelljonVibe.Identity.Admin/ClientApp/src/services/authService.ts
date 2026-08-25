const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api';

interface User {
  id: string;
  username: string;
  email: string;
  roles: string[];
  isApproved: boolean;
  isActive: boolean;
}

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  userId: string;
  username: string;
  email: string;
  roles: string[];
  isApproved: boolean;
  requiresEmailConfirmation: boolean;
  requiresApproval: boolean;
}

interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

class AuthService {
  private static readonly TOKEN_KEY = 'helljonvibe_access_token';
  private static readonly REFRESH_TOKEN_KEY = 'helljonvibe_refresh_token';
  private static readonly USER_KEY = 'helljonvibe_user';

  static async login(usernameOrEmail: string, password: string): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ usernameOrEmail, password } as LoginRequest),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Login failed');
    }

    const data = await response.json();
    
    // Store tokens and user info
    localStorage.setItem(this.TOKEN_KEY, data.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, data.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify({
      id: data.userId,
      username: data.username,
      email: data.email,
      roles: data.roles,
      isApproved: data.isApproved,
      isActive: data.isActive
    }));

    return data;
  }

  static async register(username: string, email: string, password: string): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ username, email, password } as RegisterRequest),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Registration failed');
    }

    const data = await response.json();
    
    // Store tokens and user info
    localStorage.setItem(this.TOKEN_KEY, data.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, data.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify({
      id: data.userId,
      username: data.username,
      email: data.email,
      roles: data.roles,
      isApproved: data.isApproved,
      isActive: data.isActive
    }));

    return data;
  }

  static async refreshToken(): Promise<string> {
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await fetch(`${API_BASE_URL}/auth/refresh-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      this.logout();
      throw new Error('Token refresh failed');
    }

    const data = await response.json();
    
    // Update tokens
    localStorage.setItem(this.TOKEN_KEY, data.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, data.refreshToken);
    
    return data.accessToken;
  }

  static logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  static getCurrentUser(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (!userJson) {
      return null;
    }
    return JSON.parse(userJson) as User;
  }

  static getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  static isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  static hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.includes(role) || false;
  }

  static hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.some(role => roles.includes(role)) || false;
  }

  static isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  static async getUserInfo(): Promise<User> {
    const token = this.getAccessToken();
    
    if (!token) {
      throw new Error('Not authenticated');
    }

    const response = await fetch(`${API_BASE_URL}/auth/me`, {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to get user info');
    }

    const data = await response.json();
    
    // Update user info in storage
    localStorage.setItem(this.USER_KEY, JSON.stringify({
      id: data.id,
      username: data.username,
      email: data.email,
      roles: data.roles,
      isApproved: data.isApproved,
      isActive: data.isActive
    }));

    return data as User;
  }
}

export { AuthService, User, AuthResponse };
