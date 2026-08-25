import { AuthService } from './authService';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api';

class ApiService {
  private static async request<T>(
    method: string,
    endpoint: string,
    data?: any,
    params?: Record<string, string | number | boolean>
  ): Promise<T> {
    let url = `${API_BASE_URL}${endpoint}`;
    
    // Add query parameters
    if (params) {
      const queryString = Object.entries(params)
        .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
        .join('&');
      url += `?${queryString}`;
    }

    const token = AuthService.getAccessToken();
    
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const options: RequestInit = {
      method,
      headers,
      body: data ? JSON.stringify(data) : undefined,
    };

    let response = await fetch(url, options);

    // Handle 401 Unauthorized - try to refresh token
    if (response.status === 401) {
      try {
        await AuthService.refreshToken();
        // Retry with new token
        headers['Authorization'] = `Bearer ${AuthService.getAccessToken()}`;
        response = await fetch(url, { ...options, headers });
      } catch {
        // Refresh failed, redirect to login
        AuthService.logout();
        window.location.href = '/login';
        throw new Error('Session expired');
      }
    }

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Request failed' }));
      throw new Error(error.message || `HTTP error! status: ${response.status}`);
    }

    return response.json() as Promise<T>;
  }

  static async get<T>(endpoint: string, params?: Record<string, string | number | boolean>): Promise<T> {
    return this.request<T>('GET', endpoint, undefined, params);
  }

  static async post<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>('POST', endpoint, data);
  }

  static async put<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>('PUT', endpoint, data);
  }

  static async patch<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>('PATCH', endpoint, data);
  }

  static async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>('DELETE', endpoint);
  }

  // User API methods
  static async getUsers(
    pageNumber: number = 1,
    pageSize: number = 10,
    isApproved?: boolean,
    isActive?: boolean
  ): Promise<{ data: any[]; totalCount: number; pageNumber: number; pageSize: number; totalPages: number }> {
    return this.get<any>('/users', {
      pageNumber,
      pageSize,
      isApproved,
      isActive
    });
  }

  static async getUser(id: string): Promise<any> {
    return this.get<any>(`/users/${id}`);
  }

  static async createUser(userData: any): Promise<any> {
    return this.post<any>('/users', userData);
  }

  static async updateUser(id: string, userData: any): Promise<any> {
    return this.put<any>(`/users/${id}`, userData);
  }

  static async deleteUser(id: string): Promise<any> {
    return this.delete<any>(`/users/${id}`);
  }

  static async approveUser(id: string): Promise<any> {
    return this.post<any>(`/users/${id}/approve`);
  }

  static async toggleUserActivation(id: string): Promise<any> {
    return this.post<any>(`/users/${id}/toggle-activation`);
  }

  static async resetUserPassword(id: string, newPassword: string): Promise<any> {
    return this.post<any>(`/users/${id}/reset-password`, newPassword);
  }

  // Role API methods
  static async getRoles(): Promise<any[]> {
    return this.get<any[]>('/roles');
  }

  // Audit Log API methods
  static async getAuditLogs(
    pageNumber: number = 1,
    pageSize: number = 10,
    userId?: string,
    action?: string,
    entityType?: string
  ): Promise<{ data: any[]; totalCount: number; pageNumber: number; pageSize: number; totalPages: number }> {
    return this.get<any>('/audit-logs', {
      pageNumber,
      pageSize,
      userId,
      action,
      entityType
    });
  }

  // GDPR API methods
  static async getGdprStatus(userId: string): Promise<any> {
    return this.get<any>(`/gdpr/${userId}/status`);
  }

  static async requestDataExport(userId: string): Promise<any> {
    return this.post<any>(`/gdpr/${userId}/export`);
  }

  static async requestAccountDeletion(userId: string, reason: string): Promise<any> {
    return this.post<any>(`/gdpr/${userId}/delete`, { reason });
  }

  // Profile API methods
  static async updateProfile(profileData: any): Promise<any> {
    return this.put<any>('/users/me/profile', profileData);
  }

  static async changePassword(currentPassword: string, newPassword: string): Promise<any> {
    return this.post<any>('/auth/change-password', { currentPassword, newPassword });
  }

  // Dashboard statistics
  static async getDashboardStats(): Promise<any> {
    return this.get<any>('/dashboard/stats');
  }
}

export { ApiService };
