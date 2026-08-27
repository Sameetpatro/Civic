import React, { useState, useEffect } from 'react';
import type { 
  IssueDetail, 
  WorkerSummary 
} from '../types';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { 
  X, 
  Clock, 
  MapPin, 
  CheckCircle2, 
  User, 
  Building2, 
  Camera, 
  Layers 
} from 'lucide-react';

interface IssueDetailModalProps {
  issueId: string;
  onClose: () => void;
  onIssueUpdated: () => void;
}

export const IssueDetailModal: React.FC<IssueDetailModalProps> = ({
  issueId,
  onClose,
  onIssueUpdated,
}) => {
  const { user } = useAuth();
  const [issue, setIssue] = useState<IssueDetail | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Officer Assignment State
  const [availableWorkers, setAvailableWorkers] = useState<WorkerSummary[]>([]);
  const [selectedWorkerId, setSelectedWorkerId] = useState<string>('');
  const [assignRemarks, setAssignRemarks] = useState<string>('');
  const [isAssigning, setIsAssigning] = useState<boolean>(false);

  // Worker Resolution State
  const [showResolveForm, setShowResolveForm] = useState<boolean>(false);
  const [resolutionNotes, setResolutionNotes] = useState<string>('');
  const [afterPhotoUrl, setAfterPhotoUrl] = useState<string>('');
  const [isResolving, setIsResolving] = useState<boolean>(false);

  // Citizen Verification State
  const [citizenRating, setCitizenRating] = useState<number>(5);
  const [citizenFeedback, setCitizenFeedback] = useState<string>('');
  const [isVerifying, setIsVerifying] = useState<boolean>(false);

  const fetchDetail = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.getIssueById(issueId);
      setIssue(data);

      // If department officer / admin, load available workers for this department
      if (data.departmentId && (user?.role === 'DepartmentOfficer' || user?.role === 'Admin')) {
        const workers = await api.getDepartmentWorkers(data.departmentId, data.wardSector);
        setAvailableWorkers(workers);
        if (workers.length > 0) {
          setSelectedWorkerId(workers[0].id);
        }
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load issue details.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDetail();
  }, [issueId]);

  // Actions
  const handleAssignWorker = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedWorkerId) return;

    setIsAssigning(true);
    try {
      await api.assignWorker(issueId, selectedWorkerId, assignRemarks || 'Assigned for immediate resolution.');
      await fetchDetail();
      onIssueUpdated();
    } catch (err: any) {
      alert(`Assignment failed: ${err.message}`);
    } finally {
      setIsAssigning(false);
    }
  };

  const handleAcceptJob = async () => {
    try {
      await api.acceptJob(issueId);
      await fetchDetail();
      onIssueUpdated();
    } catch (err: any) {
      alert(`Error: ${err.message}`);
    }
  };

  const handleStartWork = async () => {
    try {
      await api.startWork(issueId);
      await fetchDetail();
      onIssueUpdated();
    } catch (err: any) {
      alert(`Error: ${err.message}`);
    }
  };

  const handleResolve = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resolutionNotes) return;

    setIsResolving(true);
    try {
      const photos = afterPhotoUrl ? [afterPhotoUrl] : [];
      await api.resolveIssue(issueId, resolutionNotes, photos);
      setShowResolveForm(false);
      await fetchDetail();
      onIssueUpdated();
    } catch (err: any) {
      alert(`Resolution failed: ${err.message}`);
    } finally {
      setIsResolving(false);
    }
  };

  const handleVerify = async (isSatisfied: boolean) => {
    setIsVerifying(true);
    try {
      await api.verifyResolution(issueId, isSatisfied, citizenFeedback, citizenRating);
      await fetchDetail();
      onIssueUpdated();
    } catch (err: any) {
      alert(`Verification failed: ${err.message}`);
    } finally {
      setIsVerifying(false);
    }
  };

  if (loading) {
    return (
      <div className="modal-overlay">
        <div className="modal-content" style={{ padding: '30px', textAlign: 'center' }}>
          <div style={{ color: '#38bdf8' }}>Loading incident lifecycle details...</div>
        </div>
      </div>
    );
  }

  if (error || !issue) {
    return (
      <div className="modal-overlay">
        <div className="modal-content" style={{ padding: '30px' }}>
          <div style={{ color: '#ef4444', marginBottom: '15px' }}>{error || 'Issue not found.'}</div>
          <button className="btn btn-secondary" onClick={onClose}>Close</button>
        </div>
      </div>
    );
  }

  const isReporter = user?.id === issue.reportedByUserId;
  const isAssignedWorker = user?.id === issue.assignedWorkerId;
  const isOfficerOrAdmin = user?.role === 'DepartmentOfficer' || user?.role === 'Admin';

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '850px' }}>
        {/* Header */}
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'flex-start',
          padding: '24px 28px',
          borderBottom: '1px solid var(--border-color)',
          background: 'rgba(255, 255, 255, 0.02)'
        }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '6px' }}>
              <span style={{ fontSize: '0.8rem', fontWeight: 700, color: '#38bdf8', letterSpacing: '0.05em' }}>
                {issue.referenceNumber}
              </span>
              <span className={`badge badge-${issue.status.toLowerCase().replace('_', '-')}`}>
                {issue.statusName}
              </span>
              <span className={`badge badge-severity-${issue.severity.toLowerCase()}`}>
                {issue.severityName} Priority
              </span>
              {issue.isSensitive && (
                <span style={{ background: '#dc2626', color: '#fff', padding: '2px 8px', borderRadius: '4px', fontSize: '0.7rem', fontWeight: 700 }}>
                  RESTRICTED SENSITIVE
                </span>
              )}
            </div>
            <h2 style={{ fontSize: '1.4rem' }}>{issue.title}</h2>
          </div>
          <button 
            onClick={onClose}
            style={{ background: 'transparent', border: 'none', color: '#94a3b8', cursor: 'pointer', padding: '4px' }}
          >
            <X size={24} />
          </button>
        </div>

        {/* Modal Body */}
        <div style={{ padding: '28px', display: 'flex', flexDirection: 'column', gap: '24px' }}>
          
          {/* Metadata Grid */}
          <div style={{ 
            display: 'grid', 
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', 
            gap: '16px',
            background: 'rgba(0, 0, 0, 0.25)',
            padding: '16px',
            borderRadius: '10px',
            border: '1px solid var(--border-color)'
          }}>
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                <Building2 size={13} /> Department
              </div>
              <div style={{ fontWeight: 600, color: '#e2e8f0', marginTop: '2px' }}>
                {issue.departmentName || 'Not Assigned'}
              </div>
            </div>

            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                <Layers size={13} /> Category
              </div>
              <div style={{ fontWeight: 600, color: '#e2e8f0', marginTop: '2px' }}>
                {issue.categoryName}
              </div>
            </div>

            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                <MapPin size={13} /> Location / Ward
              </div>
              <div style={{ fontWeight: 600, color: '#e2e8f0', marginTop: '2px' }}>
                {issue.wardSector} ({issue.latitude.toFixed(4)}, {issue.longitude.toFixed(4)})
              </div>
            </div>

            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                <User size={13} /> Assigned Worker
              </div>
              <div style={{ fontWeight: 600, color: issue.assignedWorkerName ? '#38bdf8' : '#94a3b8', marginTop: '2px' }}>
                {issue.assignedWorkerName || 'Unassigned'}
              </div>
            </div>
          </div>

          {/* Description & Address */}
          <div>
            <h4 style={{ fontSize: '0.95rem', color: '#94a3b8', marginBottom: '6px' }}>Incident Description</h4>
            <p style={{ color: '#f1f5f9', lineHeight: 1.6, background: 'rgba(255,255,255,0.03)', padding: '14px', borderRadius: '8px', border: '1px solid var(--border-color)' }}>
              {issue.description}
            </p>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-dim)', marginTop: '6px' }}>
              📍 <strong>Reported Address:</strong> {issue.address}
            </div>
          </div>

          {/* Photos Proof Gallery */}
          {issue.photos.length > 0 && (
            <div>
              <h4 style={{ fontSize: '0.95rem', color: '#94a3b8', marginBottom: '10px', display: 'flex', alignItems: 'center', gap: '6px' }}>
                <Camera size={16} /> Photographic Evidence ({issue.photos.length})
              </h4>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '12px' }}>
                {issue.photos.map((photo) => (
                  <div key={photo.id} style={{ borderRadius: '8px', overflow: 'hidden', border: '1px solid var(--border-color)' }}>
                    <img 
                      src={photo.photoUrl} 
                      alt="Incident Proof" 
                      style={{ width: '100%', height: '130px', objectFit: 'cover' }}
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1541888946425-d0fbb180c5f5?w=500&auto=format&fit=crop&q=60';
                      }}
                    />
                    <div style={{ padding: '6px 10px', background: '#111827', fontSize: '0.75rem', color: '#94a3b8' }}>
                      {photo.photoTypeName} • {new Date(photo.uploadedAtUtc).toLocaleDateString()}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Citizen Feedback Banner if resolved/closed */}
          {issue.citizenFeedback && (
            <div style={{ 
              background: 'rgba(16, 185, 129, 0.1)', 
              border: '1px solid rgba(16, 185, 129, 0.3)', 
              borderRadius: '8px', 
              padding: '16px' 
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: '#34d399', fontWeight: 700, marginBottom: '4px' }}>
                <CheckCircle2 size={18} /> Citizen Verification & Rating: {issue.citizenRating}/5 Stars
              </div>
              <div style={{ color: '#e2e8f0', fontSize: '0.9rem' }}>
                "{issue.citizenFeedback}"
              </div>
            </div>
          )}

          {/* Resolution Notes */}
          {issue.resolutionNotes && (
            <div style={{ 
              background: 'rgba(56, 189, 248, 0.1)', 
              border: '1px solid rgba(56, 189, 248, 0.3)', 
              borderRadius: '8px', 
              padding: '16px' 
            }}>
              <div style={{ color: '#38bdf8', fontWeight: 700, marginBottom: '4px' }}>
                🛠️ Field Worker Resolution Report:
              </div>
              <div style={{ color: '#e2e8f0', fontSize: '0.9rem' }}>
                {issue.resolutionNotes}
              </div>
            </div>
          )}

          {/* Role-Based Action Panels */}

          {/* 1. Officer Worker Assignment Panel */}
          {isOfficerOrAdmin && (issue.status === 'DepartmentAssigned' || issue.status === 'Reported' || issue.status === 'WorkerAssigned') && (
            <div style={{ background: 'rgba(99, 102, 241, 0.08)', border: '1px solid rgba(99, 102, 241, 0.25)', borderRadius: '10px', padding: '18px' }}>
              <h4 style={{ color: '#818cf8', fontSize: '0.95rem', marginBottom: '10px' }}>
                👮 Department Dispatch: Assign Field Worker
              </h4>
              <form onSubmit={handleAssignWorker} style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
                  <select 
                    className="input-field" 
                    value={selectedWorkerId} 
                    onChange={(e) => setSelectedWorkerId(e.target.value)}
                    style={{ flex: 1, minWidth: '240px' }}
                  >
                    {availableWorkers.map((w) => (
                      <option key={w.id} value={w.id}>
                        {w.fullName} — {w.specialization} ({w.activeJobsCount}/{w.maxCapacity} active jobs) • {w.assignedWardOrZone}
                      </option>
                    ))}
                  </select>
                  <input 
                    type="text" 
                    className="input-field" 
                    placeholder="Assignment notes / urgency..."
                    value={assignRemarks}
                    onChange={(e) => setAssignRemarks(e.target.value)}
                    style={{ flex: 2, minWidth: '200px' }}
                  />
                  <button type="submit" className="btn btn-primary" disabled={isAssigning}>
                    {isAssigning ? 'Assigning...' : 'Dispatch Worker'}
                  </button>
                </div>
              </form>
            </div>
          )}

          {/* 2. Worker Action Controls */}
          {isAssignedWorker && (
            <div style={{ background: 'rgba(245, 158, 11, 0.08)', border: '1px solid rgba(245, 158, 11, 0.25)', borderRadius: '10px', padding: '18px' }}>
              <h4 style={{ color: '#fbbf24', fontSize: '0.95rem', marginBottom: '10px' }}>
                🔧 Field Worker Action Console
              </h4>
              
              {issue.status === 'WorkerAssigned' && (
                <button className="btn btn-primary" onClick={handleAcceptJob}>
                  Accept Job & Acknowledge Dispatch
                </button>
              )}

              {issue.status === 'Accepted' && (
                <button className="btn btn-primary" onClick={handleStartWork}>
                  Arrived on Site & Start Work
                </button>
              )}

              {issue.status === 'InProgress' && !showResolveForm && (
                <button className="btn btn-success" onClick={() => setShowResolveForm(true)}>
                  Complete Work & Mark Resolved
                </button>
              )}

              {showResolveForm && (
                <form onSubmit={handleResolve} style={{ display: 'flex', flexDirection: 'column', gap: '12px', marginTop: '10px' }}>
                  <textarea 
                    className="input-field" 
                    placeholder="Detail the repairs performed, materials used, and final status..." 
                    rows={3}
                    value={resolutionNotes}
                    onChange={(e) => setResolutionNotes(e.target.value)}
                    required
                  />
                  <input 
                    type="url" 
                    className="input-field" 
                    placeholder="Completed work evidence photo URL (optional)..."
                    value={afterPhotoUrl}
                    onChange={(e) => setAfterPhotoUrl(e.target.value)}
                  />
                  <div style={{ display: 'flex', gap: '10px' }}>
                    <button type="submit" className="btn btn-success" disabled={isResolving}>
                      {isResolving ? 'Submitting...' : 'Submit Resolution'}
                    </button>
                    <button type="button" className="btn btn-secondary" onClick={() => setShowResolveForm(false)}>
                      Cancel
                    </button>
                  </div>
                </form>
              )}
            </div>
          )}

          {/* 3. Citizen Verification Controls */}
          {isReporter && issue.status === 'Resolved' && (
            <div style={{ background: 'rgba(16, 185, 129, 0.08)', border: '1px solid rgba(16, 185, 129, 0.25)', borderRadius: '10px', padding: '18px' }}>
              <h4 style={{ color: '#34d399', fontSize: '0.95rem', marginBottom: '10px' }}>
                🙋 Citizen Redressal Verification
              </h4>
              <p style={{ fontSize: '0.85rem', color: '#cbd5e1', marginBottom: '12px' }}>
                The field team has marked this incident as resolved. Please verify if the problem has been fixed to your satisfaction.
              </p>

              <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                <div>
                  <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: '4px' }}>Rating (1-5 Stars)</label>
                  <select 
                    className="input-field" 
                    value={citizenRating} 
                    onChange={(e) => setCitizenRating(Number(e.target.value))}
                    style={{ width: '120px' }}
                  >
                    <option value={5}>⭐⭐⭐⭐⭐ (5 - Excellent)</option>
                    <option value={4}>⭐⭐⭐⭐ (4 - Good)</option>
                    <option value={3}>⭐⭐⭐ (3 - Acceptable)</option>
                    <option value={2}>⭐⭐ (2 - Poor)</option>
                    <option value={1}>⭐ (1 - Unresolved)</option>
                  </select>
                </div>
                <input 
                  type="text" 
                  className="input-field" 
                  placeholder="Feedback notes on the repair work..."
                  value={citizenFeedback}
                  onChange={(e) => setCitizenFeedback(e.target.value)}
                />
                <div style={{ display: 'flex', gap: '10px' }}>
                  <button className="btn btn-success" onClick={() => handleVerify(true)} disabled={isVerifying}>
                    ✅ Approve & Close Ticket
                  </button>
                  <button className="btn btn-danger" onClick={() => handleVerify(false)} disabled={isVerifying}>
                    ❌ Reject & Reopen Issue
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Immutable State Machine Audit Timeline */}
          <div>
            <h4 style={{ fontSize: '0.95rem', color: '#94a3b8', marginBottom: '12px', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Clock size={16} /> Audit Trail & Lifecycle History ({issue.statusHistory.length})
            </h4>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
              {issue.statusHistory.map((history) => (
                <div 
                  key={history.id} 
                  style={{ 
                    display: 'flex', 
                    gap: '14px', 
                    padding: '12px 16px', 
                    background: 'rgba(255,255,255,0.02)', 
                    borderRadius: '8px', 
                    border: '1px solid var(--border-color)',
                    borderLeft: '4px solid #38bdf8'
                  }}
                >
                  <div style={{ minWidth: '130px', fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                    {new Date(history.changedAtUtc).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '2px' }}>
                      <span style={{ fontWeight: 700, fontSize: '0.85rem', color: '#f8fafc' }}>
                        {history.fromStatusName} → {history.toStatusName}
                      </span>
                      <span style={{ fontSize: '0.75rem', color: '#94a3b8' }}>
                        by <strong>{history.changedByUserName}</strong>
                      </span>
                    </div>
                    <div style={{ fontSize: '0.8rem', color: '#cbd5e1' }}>
                      {history.remarks}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

        </div>
      </div>
    </div>
  );
};
