import React, { useState, useEffect } from 'react';
import { useAuth, DEMO_PERSONAS } from './context/AuthContext';
import { api } from './services/api';
import type { 
  IssueSummary, 
  Department, 
  IssueStatus, 
  IssueSeverity 
} from './types';
import { IncidentMap } from './components/IncidentMap';
import { IssueDetailModal } from './components/IssueDetailModal';
import { ReportIssueModal } from './components/ReportIssueModal';
import { 
  Activity, 
  AlertTriangle, 
  Building2, 
  CheckCircle2, 
  Clock, 
  Compass, 
  Layers, 
  MapPin, 
  Plus, 
  RefreshCw, 
  Search 
} from 'lucide-react';

export const App: React.FC = () => {
  const { user, switchPersona } = useAuth();
  
  // Navigation
  const [activeTab, setActiveTab] = useState<'dashboard' | 'issues' | 'departments'>('dashboard');

  // Data State
  const [issues, setIssues] = useState<IssueSummary[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);

  // Filters
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedDeptId, setSelectedDeptId] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [selectedSeverity, setSelectedSeverity] = useState<string>('');

  // Modals
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null);
  const [showReportModal, setShowReportModal] = useState<boolean>(false);
  const [showPersonaMenu, setShowPersonaMenu] = useState<boolean>(false);

  const loadData = async () => {
    try {
      const [issuesRes, deptsRes] = await Promise.all([
        api.getIssues({
          searchTerm: searchTerm || undefined,
          departmentId: selectedDeptId || undefined,
          status: selectedStatus as IssueStatus || undefined,
          severity: selectedSeverity as IssueSeverity || undefined,
          pageSize: 50,
        }),
        api.getDepartments(),
      ]);

      setIssues(issuesRes.items || []);
      setDepartments(deptsRes || []);
    } catch (err) {
      console.error('Failed to load dashboard data:', err);
    }
  };

  useEffect(() => {
    loadData();
  }, [selectedDeptId, selectedStatus, selectedSeverity]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loadData();
  };

  // KPI Computations
  const totalReported = issues.length;
  const inProgressCount = issues.filter(i => i.status === 'InProgress' || i.status === 'Accepted' || i.status === 'WorkerAssigned').length;
  const resolvedCount = issues.filter(i => i.status === 'Resolved' || i.status === 'Closed').length;
  const criticalCount = issues.filter(i => i.severity === 'Critical' || i.severity === 'High').length;
  const slaBreachedCount = issues.filter(i => i.isSlaBreached).length;

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      
      {/* Top Navigation Bar */}
      <header style={{ 
        background: 'var(--bg-header)', 
        backdropFilter: 'blur(16px)', 
        borderBottom: '1px solid var(--border-color)', 
        position: 'sticky', 
        top: 0, 
        zIndex: 100 
      }}>
        <div style={{ maxWidth: '1400px', margin: '0 auto', padding: '12px 24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          
          {/* Logo & Region Badge */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '14px' }}>
            <div style={{ 
              width: '38px', 
              height: '38px', 
              borderRadius: '10px', 
              background: 'linear-gradient(135deg, #0284c7 0%, #6366f1 100%)', 
              display: 'flex', 
              alignItems: 'center', 
              justifyContent: 'center', 
              fontWeight: 800, 
              color: '#fff', 
              fontSize: '1.2rem',
              boxShadow: '0 0 15px rgba(56, 189, 248, 0.3)'
            }}>
              🏛️
            </div>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <h1 style={{ fontSize: '1.15rem', fontWeight: 800, letterSpacing: '-0.02em', background: 'linear-gradient(to right, #ffffff, #94a3b8)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent' }}>
                  CIVICFIX
                </h1>
                <span style={{ fontSize: '0.65rem', padding: '2px 6px', borderRadius: '4px', background: 'rgba(56, 189, 248, 0.15)', color: '#38bdf8', border: '1px solid rgba(56, 189, 248, 0.3)', fontWeight: 700 }}>
                  SONIPAT SMART CITY
                </span>
              </div>
              <p style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                Municipal Welfare & Civic Intelligence Platform
              </p>
            </div>
          </div>

          {/* Navigation Links */}
          <nav style={{ display: 'flex', gap: '6px' }}>
            <button 
              className={`btn ${activeTab === 'dashboard' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => setActiveTab('dashboard')}
              style={{ fontSize: '0.8rem', padding: '6px 14px' }}
            >
              <Compass size={15} /> Operations Map
            </button>
            <button 
              className={`btn ${activeTab === 'issues' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => setActiveTab('issues')}
              style={{ fontSize: '0.8rem', padding: '6px 14px' }}
            >
              <Layers size={15} /> Issue Explorer ({issues.length})
            </button>
            <button 
              className={`btn ${activeTab === 'departments' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => setActiveTab('departments')}
              style={{ fontSize: '0.8rem', padding: '6px 14px' }}
            >
              <Building2 size={15} /> Departments ({departments.length})
            </button>
          </nav>

          {/* Right Actions & Quick Persona Switcher */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <button 
              className="btn btn-primary"
              onClick={() => setShowReportModal(true)}
              style={{ background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)', boxShadow: '0 2px 10px rgba(16, 185, 129, 0.3)' }}
            >
              <Plus size={16} /> Report Issue
            </button>

            {/* Persona Switcher Dropdown */}
            <div style={{ position: 'relative' }}>
              <button 
                className="btn btn-secondary"
                onClick={() => setShowPersonaMenu(!showPersonaMenu)}
                style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '6px 12px' }}
              >
                <div style={{ width: '8px', height: '8px', borderRadius: '50%', background: '#10b981' }}></div>
                <div style={{ textAlign: 'left', lineHeight: 1.1 }}>
                  <div style={{ fontSize: '0.8rem', fontWeight: 700, color: '#f8fafc' }}>
                    {user?.fullName || 'Demo User'}
                  </div>
                  <div style={{ fontSize: '0.65rem', color: '#38bdf8' }}>
                    {user?.roleName || user?.role}
                  </div>
                </div>
              </button>

              {showPersonaMenu && (
                <div style={{ 
                  position: 'absolute', 
                  right: 0, 
                  top: '110%', 
                  width: '280px', 
                  background: '#111827', 
                  border: '1px solid var(--border-color-hover)', 
                  borderRadius: '12px', 
                  boxShadow: 'var(--shadow-lg)', 
                  padding: '10px',
                  zIndex: 200 
                }}>
                  <div style={{ fontSize: '0.75rem', fontWeight: 700, color: '#94a3b8', padding: '6px 10px', borderBottom: '1px solid var(--border-color)', marginBottom: '6px' }}>
                    ⚡ Switch Sonipat Role Persona:
                  </div>
                  {DEMO_PERSONAS.map((p) => (
                    <button
                      key={p.key}
                      onClick={() => {
                        switchPersona(p.key);
                        setShowPersonaMenu(false);
                      }}
                      style={{
                        width: '100%',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '10px',
                        padding: '8px 10px',
                        background: user?.email === p.email ? 'rgba(56, 189, 248, 0.15)' : 'transparent',
                        border: 'none',
                        borderRadius: '6px',
                        color: user?.email === p.email ? '#38bdf8' : '#e2e8f0',
                        cursor: 'pointer',
                        textAlign: 'left',
                      }}
                    >
                      <div style={{ width: '24px', height: '24px', borderRadius: '50%', background: '#1e293b', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '0.75rem' }}>
                        {p.role === 'Admin' ? '👑' : p.role === 'DepartmentOfficer' ? '👮' : p.role === 'FieldWorker' ? '🔧' : '🙋'}
                      </div>
                      <div style={{ flex: 1 }}>
                        <div style={{ fontSize: '0.8rem', fontWeight: 600 }}>{p.name}</div>
                        <div style={{ fontSize: '0.65rem', color: '#94a3b8' }}>{p.roleLabel}</div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>

          </div>

        </div>
      </header>

      {/* Main Container */}
      <main style={{ maxWidth: '1400px', margin: '0 auto', padding: '24px', flex: 1, width: '100%' }}>
        
        {/* KPI Metrics Row */}
        <section style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '16px', marginBottom: '24px' }}>
          
          <div className="glass-panel" style={{ padding: '18px', display: 'flex', alignItems: 'center', gap: '14px' }}>
            <div style={{ width: '48px', height: '48px', borderRadius: '12px', background: 'rgba(56, 189, 248, 0.12)', border: '1px solid rgba(56, 189, 248, 0.25)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#38bdf8' }}>
              <Layers size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Total Incidents</div>
              <div style={{ fontSize: '1.6rem', fontWeight: 800, color: '#fff' }}>{totalReported}</div>
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '18px', display: 'flex', alignItems: 'center', gap: '14px' }}>
            <div style={{ width: '48px', height: '48px', borderRadius: '12px', background: 'rgba(245, 158, 11, 0.12)', border: '1px solid rgba(245, 158, 11, 0.25)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#fbbf24' }}>
              <Clock size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Active Dispatches</div>
              <div style={{ fontSize: '1.6rem', fontWeight: 800, color: '#fbbf24' }}>{inProgressCount}</div>
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '18px', display: 'flex', alignItems: 'center', gap: '14px' }}>
            <div style={{ width: '48px', height: '48px', borderRadius: '12px', background: 'rgba(16, 185, 129, 0.12)', border: '1px solid rgba(16, 185, 129, 0.25)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#34d399' }}>
              <CheckCircle2 size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Resolved & Closed</div>
              <div style={{ fontSize: '1.6rem', fontWeight: 800, color: '#34d399' }}>{resolvedCount}</div>
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '18px', display: 'flex', alignItems: 'center', gap: '14px' }}>
            <div style={{ width: '48px', height: '48px', borderRadius: '12px', background: 'rgba(239, 68, 68, 0.12)', border: '1px solid rgba(239, 68, 68, 0.25)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#f87171' }}>
              <AlertTriangle size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>High / Critical Hazards</div>
              <div style={{ fontSize: '1.6rem', fontWeight: 800, color: '#f87171' }}>{criticalCount}</div>
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '18px', display: 'flex', alignItems: 'center', gap: '14px' }}>
            <div style={{ width: '48px', height: '48px', borderRadius: '12px', background: 'rgba(220, 38, 38, 0.15)', border: '1px solid rgba(220, 38, 38, 0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#ef4444' }}>
              <Clock size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>SLA Overdue Breaches</div>
              <div style={{ fontSize: '1.6rem', fontWeight: 800, color: '#ef4444' }}>{slaBreachedCount}</div>
            </div>
          </div>

        </section>

        {/* Tab 1: Operations Dashboard & Map */}
        {activeTab === 'dashboard' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
            
            {/* Map Header & Controls */}
            <div className="glass-panel" style={{ padding: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                <div>
                  <h3 style={{ fontSize: '1.1rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <MapPin size={20} color="#38bdf8" /> Sonipat Geospatial Incident Command Center
                  </h3>
                  <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                    Real-time GPS mapping of municipal reports across Sector 14, Model Town, Murthal Road & Wards
                  </p>
                </div>
                <button className="btn btn-secondary" onClick={loadData} style={{ fontSize: '0.8rem' }}>
                  <RefreshCw size={14} /> Refresh Stream
                </button>
              </div>

              {/* Leaflet Map */}
              <IncidentMap 
                issues={issues} 
                onSelectIssue={(id) => setSelectedIssueId(id)}
                height="500px" 
              />
            </div>

            {/* Quick Activity Stream */}
            <div className="glass-panel" style={{ padding: '20px' }}>
              <h3 style={{ fontSize: '1.05rem', marginBottom: '14px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Activity size={18} color="#10b981" /> Live Civic Incident Feed
              </h3>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '14px' }}>
                {issues.slice(0, 6).map((issue) => (
                  <div 
                    key={issue.id} 
                    className="glass-panel glass-panel-interactive"
                    onClick={() => setSelectedIssueId(issue.id)}
                    style={{ padding: '16px' }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
                      <span style={{ fontSize: '0.75rem', fontWeight: 700, color: '#38bdf8' }}>{issue.referenceNumber}</span>
                      <span className={`badge badge-${issue.status.toLowerCase().replace('_', '-')}`}>
                        {issue.statusName}
                      </span>
                    </div>
                    <h4 style={{ fontSize: '0.95rem', marginBottom: '6px', color: '#f8fafc' }}>{issue.title}</h4>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', justifyContent: 'space-between' }}>
                      <span>📍 {issue.wardSector}</span>
                      <span>🏛️ {issue.categoryName}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

          </div>
        )}

        {/* Tab 2: Issue Explorer */}
        {activeTab === 'issues' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
            
            {/* Filter Toolbar */}
            <div className="glass-panel" style={{ padding: '18px' }}>
              <form onSubmit={handleSearchSubmit} style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'center' }}>
                <div style={{ flex: 2, minWidth: '220px', position: 'relative' }}>
                  <input 
                    type="text" 
                    className="input-field" 
                    placeholder="Search by title, description or reference (CVX-...)"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    style={{ paddingLeft: '34px' }}
                  />
                  <Search size={16} color="#64748b" style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)' }} />
                </div>

                <select 
                  className="input-field" 
                  value={selectedDeptId} 
                  onChange={(e) => setSelectedDeptId(e.target.value)}
                  style={{ flex: 1, minWidth: '160px' }}
                >
                  <option value="">All Departments</option>
                  {departments.map((d) => (
                    <option key={d.id} value={d.id}>{d.name}</option>
                  ))}
                </select>

                <select 
                  className="input-field" 
                  value={selectedStatus} 
                  onChange={(e) => setSelectedStatus(e.target.value)}
                  style={{ flex: 1, minWidth: '140px' }}
                >
                  <option value="">All Statuses</option>
                  <option value="DepartmentAssigned">Department Assigned</option>
                  <option value="WorkerAssigned">Worker Assigned</option>
                  <option value="Accepted">Accepted</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Resolved">Resolved</option>
                  <option value="Closed">Closed</option>
                  <option value="Reopened">Reopened</option>
                </select>

                <select 
                  className="input-field" 
                  value={selectedSeverity} 
                  onChange={(e) => setSelectedSeverity(e.target.value)}
                  style={{ flex: 1, minWidth: '140px' }}
                >
                  <option value="">All Severities</option>
                  <option value="Critical">Critical</option>
                  <option value="High">High</option>
                  <option value="Medium">Medium</option>
                  <option value="Low">Low</option>
                </select>

                <button type="submit" className="btn btn-primary">
                  Filter
                </button>
              </form>
            </div>

            {/* Issue Cards Grid */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(340px, 1fr))', gap: '16px' }}>
              {issues.map((issue) => (
                <div 
                  key={issue.id} 
                  className="glass-panel glass-panel-interactive"
                  onClick={() => setSelectedIssueId(issue.id)}
                  style={{ padding: '20px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}
                >
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                      <span style={{ fontSize: '0.75rem', fontWeight: 700, color: '#38bdf8' }}>{issue.referenceNumber}</span>
                      <span className={`badge badge-severity-${issue.severity.toLowerCase()}`}>
                        {issue.severityName}
                      </span>
                    </div>

                    <h3 style={{ fontSize: '1.05rem', marginBottom: '8px', color: '#fff', lineHeight: 1.3 }}>
                      {issue.title}
                    </h3>

                    <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', marginBottom: '14px' }}>
                      <span className={`badge badge-${issue.status.toLowerCase().replace('_', '-')}`}>
                        {issue.statusName}
                      </span>
                      <span style={{ fontSize: '0.75rem', padding: '3px 8px', borderRadius: '4px', background: 'rgba(255,255,255,0.06)', color: '#cbd5e1' }}>
                        {issue.categoryName}
                      </span>
                    </div>
                  </div>

                  <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '12px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                    <span>📍 {issue.wardSector}</span>
                    <span>🕒 {new Date(issue.reportedAtUtc).toLocaleDateString()}</span>
                  </div>
                </div>
              ))}
            </div>

          </div>
        )}

        {/* Tab 3: Department Registry */}
        {activeTab === 'departments' && (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '18px' }}>
            {departments.map((dept) => (
              <div key={dept.id} className="glass-panel" style={{ padding: '22px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '10px' }}>
                  <div style={{ width: '40px', height: '40px', borderRadius: '10px', background: 'rgba(56, 189, 248, 0.12)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#38bdf8', fontSize: '1.2rem' }}>
                    🏢
                  </div>
                  <span style={{ fontSize: '0.75rem', padding: '3px 8px', borderRadius: '4px', background: 'rgba(255,255,255,0.08)', color: '#38bdf8', fontWeight: 700 }}>
                    {dept.code}
                  </span>
                </div>

                <h3 style={{ fontSize: '1.1rem', marginBottom: '6px', color: '#fff' }}>{dept.name}</h3>
                <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '14px', lineHeight: 1.4 }}>
                  {dept.description}
                </p>

                <div style={{ background: 'rgba(0,0,0,0.25)', padding: '12px', borderRadius: '8px', fontSize: '0.8rem', color: '#cbd5e1', display: 'flex', flexDirection: 'column', gap: '4px' }}>
                  <div>👮 <strong>Head Officer:</strong> {dept.headOfficerName}</div>
                  <div>✉️ <strong>Email:</strong> {dept.contactEmail}</div>
                  <div>📞 <strong>Hotline:</strong> {dept.contactPhone}</div>
                </div>
              </div>
            ))}
          </div>
        )}

      </main>

      {/* Issue Detail Modal */}
      {selectedIssueId && (
        <IssueDetailModal
          issueId={selectedIssueId}
          onClose={() => setSelectedIssueId(null)}
          onIssueUpdated={loadData}
        />
      )}

      {/* Citizen Report Modal */}
      {showReportModal && (
        <ReportIssueModal
          onClose={() => setShowReportModal(false)}
          onIssueCreated={loadData}
        />
      )}

    </div>
  );
};
export default App;
