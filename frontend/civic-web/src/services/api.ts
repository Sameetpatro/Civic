import type { 
  AuthResponse, 
  Category, 
  Department, 
  IssueDetail, 
  IssueSummary, 
  PagedResult, 
  User, 
  WorkerSummary,
  IssueStatus,
  IssueSeverity
} from '../types';

const API_BASE = 'http://localhost:5000/api';

class ApiService {
  private getToken(): string | null {
    return localStorage.getItem('civicfix_token');
  }

  private setToken(token: string) {
    localStorage.setItem('civicfix_token', token);
  }

  public clearToken() {
    localStorage.removeItem('civicfix_token');
    localStorage.removeItem('civicfix_user');
  }

  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const token = this.getToken();
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string> || {}),
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        headers,
      });

      if (!response.ok) {
        let errorMessage = `HTTP Error ${response.status}: ${response.statusText}`;
        try {
          const problemDetails = await response.json();
          if (problemDetails.detail) {
            errorMessage = problemDetails.detail;
          } else if (problemDetails.title) {
            errorMessage = problemDetails.title;
          } else if (problemDetails.errors) {
            errorMessage = Object.values(problemDetails.errors).flat().join(', ');
          }
        } catch {
          // If response body is not json
        }
        throw new Error(errorMessage);
      }

      if (response.status === 204) {
        return {} as T;
      }

      return await response.json();
    } catch (err: any) {
      console.error(`API Call failed [${endpoint}]:`, err);
      throw err;
    }
  }

  // --- Auth APIs ---
  async login(email: string, password: string): Promise<AuthResponse> {
    const result = await this.request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    this.setToken(result.token);
    localStorage.setItem('civicfix_user', JSON.stringify(result.user));
    return result;
  }

  async register(data: {
    fullName: string;
    email: string;
    password: string;
    phoneNumber: string;
    role?: number;
    departmentId?: string;
  }): Promise<AuthResponse> {
    const result = await this.request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    this.setToken(result.token);
    localStorage.setItem('civicfix_user', JSON.stringify(result.user));
    return result;
  }

  async getMe(): Promise<User> {
    return this.request<User>('/auth/me');
  }

  // --- Department & Category APIs ---
  async getDepartments(): Promise<Department[]> {
    return this.request<Department[]>('/departments');
  }

  async getCategories(departmentId?: string): Promise<Category[]> {
    const query = departmentId ? `?departmentId=${departmentId}` : '';
    return this.request<Category[]>(`/departments/categories${query}`);
  }

  async getDepartmentWorkers(departmentId: string, wardSector?: string): Promise<WorkerSummary[]> {
    const query = wardSector ? `?wardSector=${encodeURIComponent(wardSector)}` : '';
    return this.request<WorkerSummary[]>(`/departments/${departmentId}/workers${query}`);
  }

  // --- Issue APIs ---
  async getIssues(filter: {
    departmentId?: string;
    status?: IssueStatus;
    severity?: IssueSeverity;
    wardSector?: string;
    categoryId?: string;
    reportedByUserId?: string;
    assignedWorkerId?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Promise<PagedResult<IssueSummary>> {
    const params = new URLSearchParams();
    if (filter.departmentId) params.append('departmentId', filter.departmentId);
    if (filter.status) params.append('status', filter.status);
    if (filter.severity) params.append('severity', filter.severity);
    if (filter.wardSector) params.append('wardSector', filter.wardSector);
    if (filter.categoryId) params.append('categoryId', filter.categoryId);
    if (filter.reportedByUserId) params.append('reportedByUserId', filter.reportedByUserId);
    if (filter.assignedWorkerId) params.append('assignedWorkerId', filter.assignedWorkerId);
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());

    const qs = params.toString() ? `?${params.toString()}` : '';
    return this.request<PagedResult<IssueSummary>>(`/issues${qs}`);
  }

  async getIssueById(id: string): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${id}`);
  }

  async getNearbyIssues(latitude: number, longitude: number, radiusKm: number = 2.0): Promise<IssueSummary[]> {
    return this.request<IssueSummary[]>(`/issues/nearby?latitude=${latitude}&longitude=${longitude}&radiusKm=${radiusKm}`);
  }

  async createIssue(data: {
    title: string;
    description: string;
    categoryId: string;
    latitude: number;
    longitude: number;
    address: string;
    wardSector: string;
    isSensitive?: boolean;
    photoUrls?: string[];
  }): Promise<IssueDetail> {
    return this.request<IssueDetail>('/issues', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async assignDepartment(issueId: string, departmentId: string, remarks: string): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/assign-department`, {
      method: 'POST',
      body: JSON.stringify({ departmentId, remarks }),
    });
  }

  async assignWorker(issueId: string, workerId: string, remarks: string): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/assign-worker`, {
      method: 'POST',
      body: JSON.stringify({ workerId, remarks }),
    });
  }

  async acceptJob(issueId: string): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/accept`, {
      method: 'POST',
    });
  }

  async startWork(issueId: string): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/start`, {
      method: 'POST',
    });
  }

  async resolveIssue(issueId: string, resolutionNotes: string, afterPhotoUrls?: string[]): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/resolve`, {
      method: 'POST',
      body: JSON.stringify({ resolutionNotes, afterPhotoUrls }),
    });
  }

  async verifyResolution(issueId: string, isSatisfied: boolean, feedback?: string, rating: number = 5): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/verify`, {
      method: 'POST',
      body: JSON.stringify({ isSatisfied, feedback, rating }),
    });
  }

  async changeStatus(issueId: string, newStatus: IssueStatus, remarks: string): Promise<IssueDetail> {
    return this.request<IssueDetail>(`/issues/${issueId}/status`, {
      method: 'POST',
      body: JSON.stringify({ newStatus, remarks }),
    });
  }
}

export const api = new ApiService();
