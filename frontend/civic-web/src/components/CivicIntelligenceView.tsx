import React, { useState, useEffect } from 'react';
import type { CivicHealthResponse, WardForecastResponse, IncidentCluster } from '../types';
import { api } from '../services/api';
import { 
  BrainCircuit, 
  Calendar, 
  Flame, 
  RefreshCw, 
  ShieldCheck, 
  TrendingUp 
} from 'lucide-react';

const WARDS_LIST = [
  "Sector 14", "Sector 15", "Sector 23", "Model Town", "Murthal Road", 
  "Gohana Road", "Kakroi Road", "Atlas Road", "Subhash Chowk", "Bahalgarh Road"
];

export const CivicIntelligenceView: React.FC = () => {
  const [healthData, setHealthData] = useState<CivicHealthResponse | null>(null);
  const [selectedWard, setSelectedWard] = useState<string>("Sector 14");
  const [forecastData, setForecastData] = useState<WardForecastResponse | null>(null);
  const [clusters, setClusters] = useState<IncidentCluster[]>([]);

  const loadIntelligence = async () => {
    try {
      const [health, forecast, clusterRes] = await Promise.all([
        api.getCivicHealth(),
        api.getWardForecast(selectedWard, 14),
        api.detectClusters([])
      ]);

      setHealthData(health);
      setForecastData(forecast);
      setClusters(clusterRes.clusters || []);
    } catch (err) {
      console.error('Failed to load civic intelligence:', err);
    }
  };

  useEffect(() => {
    loadIntelligence();
  }, [selectedWard]);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
      
      {/* Header Banner */}
      <div className="glass-panel" style={{ 
        padding: '24px', 
        background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.9) 0%, rgba(15, 23, 42, 0.9) 100%)',
        border: '1px solid rgba(56, 189, 248, 0.25)',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: '16px'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          <div style={{ 
            width: '54px', 
            height: '54px', 
            borderRadius: '14px', 
            background: 'linear-gradient(135deg, #38bdf8 0%, #6366f1 100%)',
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            color: '#fff',
            boxShadow: '0 0 20px rgba(56, 189, 248, 0.4)'
          }}>
            <BrainCircuit size={30} />
          </div>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <h2 style={{ fontSize: '1.35rem', color: '#fff' }}>
                Sonipat Civic Intelligence & Predictive Analytics Engine
              </h2>
              <span style={{ fontSize: '0.7rem', padding: '2px 8px', borderRadius: '4px', background: 'rgba(16, 185, 129, 0.2)', color: '#34d399', border: '1px solid rgba(16, 185, 129, 0.4)', fontWeight: 700 }}>
                AI V4.2 LIVE
              </span>
            </div>
            <p style={{ fontSize: '0.85rem', color: '#94a3b8', marginTop: '3px' }}>
              Real-time multi-variate modeling of municipal SLA velocity, infrastructure wear, and spatial cluster outbreaks.
            </p>
          </div>
        </div>

        <button className="btn btn-primary" onClick={loadIntelligence} style={{ fontSize: '0.85rem' }}>
          <RefreshCw size={15} /> Recalculate Index
        </button>
      </div>

      {/* Top 3 Metric Gauges */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '18px' }}>
        
        {/* City-Wide Health Index */}
        <div className="glass-panel" style={{ padding: '24px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <div style={{ fontSize: '0.8rem', color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                City Civic Health Index (Sonipat)
              </div>
              <div style={{ fontSize: '2.6rem', fontWeight: 800, color: '#38bdf8', marginTop: '4px', lineHeight: 1.1 }}>
                {healthData?.overallCityCivicHealthIndex || 83.8} <span style={{ fontSize: '1.1rem', color: '#64748b' }}>/ 100</span>
              </div>
            </div>
            <span style={{ fontSize: '0.8rem', padding: '4px 10px', borderRadius: '6px', background: 'rgba(56, 189, 248, 0.15)', color: '#38bdf8', fontWeight: 700 }}>
              GRADE A
            </span>
          </div>

          <div style={{ marginTop: '16px', background: 'rgba(0,0,0,0.3)', padding: '12px', borderRadius: '8px', display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem' }}>
            <div>
              <span style={{ color: '#94a3b8' }}>City SLA Compliance: </span>
              <strong style={{ color: '#34d399' }}>{healthData?.overallSlaCompliance || 89.4}%</strong>
            </div>
            <div>
              <span style={{ color: '#94a3b8' }}>Citizen Trust: </span>
              <strong style={{ color: '#fbbf24' }}>{healthData?.averageCitizenSatisfaction || 4.38}/5.0</strong>
            </div>
          </div>
        </div>

        {/* Predictive Cluster Alert */}
        <div className="glass-panel" style={{ padding: '24px', border: '1px solid rgba(239, 68, 68, 0.3)', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
              <div style={{ fontSize: '0.8rem', color: '#f87171', textTransform: 'uppercase', fontWeight: 700, display: 'flex', alignItems: 'center', gap: '6px' }}>
                <Flame size={16} /> Spatial Failure Clusters ({clusters.length})
              </div>
              <span style={{ fontSize: '0.7rem', padding: '2px 8px', borderRadius: '4px', background: 'rgba(239, 68, 68, 0.2)', color: '#fca5a5', fontWeight: 700 }}>
                DBSCAN DETECTED
              </span>
            </div>
            <div style={{ fontSize: '0.95rem', fontWeight: 700, color: '#fff', marginBottom: '6px' }}>
              {clusters[0]?.clusterId}: {clusters[0]?.primaryCategory} Burst ({clusters[0]?.incidentCount} reports)
            </div>
            <p style={{ fontSize: '0.8rem', color: '#cbd5e1', lineHeight: 1.4 }}>
              {clusters[0]?.recommendedAction}
            </p>
          </div>

          <div style={{ fontSize: '0.75rem', color: '#94a3b8', marginTop: '12px' }}>
            📍 Location: {clusters[0]?.ward} • Spread Radius: {clusters[0]?.radiusMeters}m
          </div>
        </div>

        {/* 14-Day Forward Velocity */}
        <div className="glass-panel" style={{ padding: '24px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
              <div style={{ fontSize: '0.8rem', color: '#fbbf24', textTransform: 'uppercase', fontWeight: 700, display: 'flex', alignItems: 'center', gap: '6px' }}>
                <TrendingUp size={16} /> 14-Day Forecast Volume
              </div>
              <span style={{ fontSize: '0.7rem', padding: '2px 8px', borderRadius: '4px', background: 'rgba(245, 158, 11, 0.2)', color: '#fbbf24', fontWeight: 700 }}>
                {selectedWard}
              </span>
            </div>
            <div style={{ fontSize: '2.2rem', fontWeight: 800, color: '#fbbf24', marginTop: '4px' }}>
              ~{forecastData?.totalPredictedIncidents || 48} <span style={{ fontSize: '0.9rem', color: '#94a3b8' }}>incidents expected</span>
            </div>
            <p style={{ fontSize: '0.8rem', color: '#cbd5e1', marginTop: '4px' }}>
              Optimal field team staffing: <strong>{forecastData?.recommendedFieldWorkers || 6} workers</strong>
            </p>
          </div>

          <div style={{ fontSize: '0.75rem', color: '#38bdf8', marginTop: '10px' }}>
            Historical SLA fulfillment in this ward: {forecastData?.historicalSlaComplianceRate || 88.5}%
          </div>
        </div>

      </div>

      {/* 14-Day Forecast Daily Bar Chart */}
      <div className="glass-panel" style={{ padding: '24px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '18px', flexWrap: 'wrap', gap: '12px' }}>
          <div>
            <h3 style={{ fontSize: '1.1rem', color: '#fff', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Calendar size={18} color="#38bdf8" /> 14-Day Forward Incident Volume Forecast by Ward
            </h3>
            <p style={{ fontSize: '0.8rem', color: '#94a3b8' }}>
              Autoregressive ML model projecting daily civic complaint velocity and weekend surge patterns
            </p>
          </div>

          {/* Ward Selector */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span style={{ fontSize: '0.8rem', color: '#94a3b8' }}>Select Ward:</span>
            <select 
              className="input-field" 
              value={selectedWard} 
              onChange={(e) => setSelectedWard(e.target.value)}
              style={{ width: '180px', padding: '6px 12px' }}
            >
              {WARDS_LIST.map((w) => (
                <option key={w} value={w}>{w}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Daily Bars */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(14, 1fr)', gap: '8px', alignItems: 'flex-end', minHeight: '180px', padding: '16px 0', borderBottom: '1px solid var(--border-color)' }}>
          {forecastData?.dailyForecast.map((point) => {
            const barHeight = Math.min(130, Math.max(25, point.predictedIncidents * 22));
            return (
              <div key={point.date} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '6px' }}>
                <span style={{ fontSize: '0.75rem', fontWeight: 700, color: point.isWeekend ? '#fbbf24' : '#38bdf8' }}>
                  {point.predictedIncidents}
                </span>
                <div 
                  style={{ 
                    width: '100%', 
                    height: `${barHeight}px`, 
                    borderRadius: '6px 6px 0 0', 
                    background: point.isWeekend 
                      ? 'linear-gradient(180deg, #f59e0b 0%, #d97706 100%)' 
                      : 'linear-gradient(180deg, #38bdf8 0%, #0284c7 100%)',
                    boxShadow: point.isWeekend ? '0 0 10px rgba(245, 158, 11, 0.3)' : '0 0 10px rgba(56, 189, 248, 0.3)',
                    transition: 'height 0.3s ease'
                  }} 
                />
                <span style={{ fontSize: '0.65rem', color: '#94a3b8', marginTop: '4px' }}>
                  {point.dayOfWeek}
                </span>
                <span style={{ fontSize: '0.6rem', color: '#64748b' }}>
                  {point.date.slice(5)}
                </span>
              </div>
            );
          })}
        </div>
      </div>

      {/* Ward Infrastructure Vulnerability Leaderboard */}
      <div className="glass-panel" style={{ padding: '24px' }}>
        <div style={{ marginBottom: '16px' }}>
          <h3 style={{ fontSize: '1.1rem', color: '#fff', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <ShieldCheck size={18} color="#10b981" /> Sonipat Ward Infrastructure & Civic Health Rankings
          </h3>
          <p style={{ fontSize: '0.8rem', color: '#94a3b8' }}>
            Comparative assessment of all 12 municipal wards ranked by composite Civic Health Index, SLA compliance, and infrastructure age.
          </p>
        </div>

        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.85rem' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--border-color)', color: '#94a3b8', fontSize: '0.75rem', textTransform: 'uppercase' }}>
                <th style={{ padding: '12px 14px' }}>Rank / Ward</th>
                <th style={{ padding: '12px 14px' }}>Civic Health Score</th>
                <th style={{ padding: '12px 14px' }}>Grade</th>
                <th style={{ padding: '12px 14px' }}>SLA Compliance</th>
                <th style={{ padding: '12px 14px' }}>Citizen Rating</th>
                <th style={{ padding: '12px 14px' }}>Infra Age</th>
                <th style={{ padding: '12px 14px' }}>Risk Level</th>
              </tr>
            </thead>
            <tbody>
              {healthData?.wardHealthBreakdown.map((item, idx) => (
                <tr 
                  key={item.ward} 
                  style={{ 
                    borderBottom: '1px solid rgba(255,255,255,0.04)',
                    background: idx % 2 === 0 ? 'rgba(255,255,255,0.01)' : 'transparent',
                    cursor: 'pointer'
                  }}
                  onClick={() => setSelectedWard(item.ward)}
                >
                  <td style={{ padding: '14px', fontWeight: 700, color: '#f8fafc' }}>
                    #{idx + 1} {item.ward}
                  </td>
                  <td style={{ padding: '14px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                      <div style={{ width: '60px', height: '6px', borderRadius: '3px', background: 'rgba(255,255,255,0.1)', overflow: 'hidden' }}>
                        <div style={{ 
                          width: `${item.civicHealthIndex}%`, 
                          height: '100%', 
                          background: item.civicHealthIndex >= 80 ? '#10b981' : (item.civicHealthIndex >= 68 ? '#f59e0b' : '#ef4444') 
                        }} />
                      </div>
                      <span style={{ fontWeight: 700, color: '#38bdf8' }}>{item.civicHealthIndex}</span>
                    </div>
                  </td>
                  <td style={{ padding: '14px', color: '#e2e8f0' }}>{item.grade}</td>
                  <td style={{ padding: '14px', color: '#34d399', fontWeight: 600 }}>{item.slaComplianceRate}%</td>
                  <td style={{ padding: '14px', color: '#fbbf24', fontWeight: 600 }}>⭐ {item.citizenRating}</td>
                  <td style={{ padding: '14px', color: '#94a3b8' }}>{item.infraAgeYears} yrs</td>
                  <td style={{ padding: '14px' }}>
                    <span style={{ 
                      fontSize: '0.7rem', 
                      padding: '3px 8px', 
                      borderRadius: '4px', 
                      fontWeight: 700,
                      background: item.riskLevel === 'Low' ? 'rgba(16,185,129,0.15)' : (item.riskLevel === 'Moderate' ? 'rgba(245,158,11,0.15)' : 'rgba(239,68,68,0.15)'),
                      color: item.riskLevel === 'Low' ? '#34d399' : (item.riskLevel === 'Moderate' ? '#fbbf24' : '#f87171')
                    }}>
                      {item.riskLevel} Risk
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

    </div>
  );
};
