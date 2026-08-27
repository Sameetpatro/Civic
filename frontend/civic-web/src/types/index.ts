export type UserRole = 'Citizen' | 'DepartmentOfficer' | 'FieldWorker' | 'Admin';

export type IssueStatus = 
  | 'Reported'
  | 'Classified'
  | 'DepartmentAssigned'
  | 'OfficerReview'
  | 'WorkerAssigned'
  | 'Accepted'
  | 'InProgress'
  | 'Resolved'
  | 'CitizenVerification'
  | 'Closed'
  | 'Rejected'
  | 'Duplicate'
  | 'Escalated'
  | 'Reopened';

export type IssueSeverity = 'Low' | 'Medium' | 'High' | 'Critical';
export type IssuePriority = 'Low' | 'Medium' | 'High' | 'Urgent';
export type PhotoType = 'ReportBefore' | 'WorkInProgress' | 'WorkCompletedAfter';

export interface User {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: UserRole;
  roleName: string;
  departmentId?: string;
  departmentName?: string;
  createdAtUtc: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAtUtc: string;
}

export interface Department {
  id: string;
  name: string;
  code: string;
  description: string;
  contactEmail: string;
  contactPhone: string;
  headOfficerName: string;
  totalActiveIssues: number;
  totalAvailableWorkers: number;
}

export interface Category {
  id: string;
  name: string;
  code: string;
  primaryCategoryGroup: string;
  description: string;
  departmentId: string;
  departmentName: string;
  defaultSlaHours: number;
}

export interface WorkerSummary {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  specialization: string;
  assignedWardOrZone: string;
  activeJobsCount: number;
  maxCapacity: number;
  isAvailable: boolean;
  rating: number;
  totalCompletedJobs: number;
}

export interface IssuePhoto {
  id: string;
  photoUrl: string;
  photoType: PhotoType;
  photoTypeName: string;
  caption?: string;
  uploadedByUserId: string;
  uploadedAtUtc: string;
}

export interface IssueStatusHistory {
  id: string;
  fromStatus: IssueStatus;
  fromStatusName: string;
  toStatus: IssueStatus;
  toStatusName: string;
  changedByUserId: string;
  changedByUserName: string;
  changedAtUtc: string;
  remarks: string;
}

export interface IssueSummary {
  id: string;
  referenceNumber: string;
  title: string;
  categoryName: string;
  primaryCategoryGroup: string;
  departmentName?: string;
  status: IssueStatus;
  statusName: string;
  severity: IssueSeverity;
  severityName: string;
  priority: IssuePriority;
  priorityName: string;
  latitude: number;
  longitude: number;
  wardSector: string;
  reportedAtUtc: string;
  targetSlaUtc?: string;
  thumbnailUrl?: string;
  isSlaBreached: boolean;
}

export interface IssueDetail {
  id: string;
  referenceNumber: string;
  title: string;
  description: string;
  aiSummary?: string;
  categoryId: string;
  categoryName: string;
  primaryCategoryGroup: string;
  departmentId?: string;
  departmentName?: string;
  reportedByUserId: string;
  reportedByUserName: string;
  assignedWorkerId?: string;
  assignedWorkerName?: string;
  status: IssueStatus;
  statusName: string;
  severity: IssueSeverity;
  severityName: string;
  priority: IssuePriority;
  priorityName: string;
  latitude: number;
  longitude: number;
  address: string;
  wardSector: string;
  reportedAtUtc: string;
  updatedAtUtc?: string;
  targetSlaUtc?: string;
  resolvedAtUtc?: string;
  closedAtUtc?: string;
  isSensitive: boolean;
  resolutionNotes?: string;
  citizenFeedback?: string;
  citizenRating?: number;
  masterIssueId?: string;
  photos: IssuePhoto[];
  statusHistory: IssueStatusHistory[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
