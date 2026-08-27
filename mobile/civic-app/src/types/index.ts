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

export interface MobileUser {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  departmentName?: string;
}

export interface MobileIssue {
  id: string;
  referenceNumber: string;
  title: string;
  description: string;
  categoryName: string;
  status: IssueStatus;
  statusName: string;
  latitude: number;
  longitude: number;
  address: string;
  wardSector: string;
  reportedAtUtc: string;
  targetSlaUtc?: string;
  photos: string[];
}
