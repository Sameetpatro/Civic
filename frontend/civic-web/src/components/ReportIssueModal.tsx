import React, { useState, useEffect } from 'react';
import type { Category } from '../types';
import { api } from '../services/api';
import { IncidentMap } from './IncidentMap';
import { X, MapPin, Camera, AlertCircle } from 'lucide-react';

interface ReportIssueModalProps {
  onClose: () => void;
  onIssueCreated: () => void;
}

const SONIPAT_LOCATIONS = [
  { name: 'Sector 14 Commercial Complex', lat: 28.9931, lng: 77.0151, ward: 'Sector 14' },
  { name: 'Model Town Market Main Road', lat: 28.9985, lng: 77.0225, ward: 'Model Town' },
  { name: 'Murthal Road Flyover Junction', lat: 29.0125, lng: 77.0385, ward: 'Murthal Road' },
  { name: 'Kakroi Road Residential Block', lat: 28.9850, lng: 77.0050, ward: 'Kakroi Road' },
  { name: 'Subhash Chowk Old City', lat: 28.9910, lng: 77.0280, ward: 'Ward 8' },
  { name: 'Sector 15 Community Park', lat: 28.9880, lng: 77.0350, ward: 'Sector 15' },
];

export const ReportIssueModal: React.FC<ReportIssueModalProps> = ({
  onClose,
  onIssueCreated,
}) => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>('');
  const [title, setTitle] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [address, setAddress] = useState<string>('Sector 14 Commercial Complex, Sonipat');
  const [wardSector, setWardSector] = useState<string>('Sector 14');
  const [coords, setCoords] = useState<[number, number]>([28.9931, 77.0151]);
  const [photoUrl, setPhotoUrl] = useState<string>('https://images.unsplash.com/photo-1541888946425-d0fbb180c5f5?w=600&auto=format&fit=crop&q=80');
  const [isSensitive, setIsSensitive] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadCategories = async () => {
      try {
        const data = await api.getCategories();
        setCategories(data);
        if (data.length > 0) {
          setSelectedCategoryId(data[0].id);
        }
      } catch (err: any) {
        console.error('Failed to load categories:', err);
      }
    };
    loadCategories();
  }, []);

  const handleLocationSelect = (lat: number, lng: number) => {
    setCoords([lat, lng]);
  };

  const handlePresetLocation = (preset: typeof SONIPAT_LOCATIONS[0]) => {
    setCoords([preset.lat, preset.lng]);
    setAddress(preset.name);
    setWardSector(preset.ward);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title || !description || !selectedCategoryId) {
      setError('Please fill in all required fields.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await api.createIssue({
        title,
        description,
        categoryId: selectedCategoryId,
        latitude: coords[0],
        longitude: coords[1],
        address,
        wardSector,
        isSensitive,
        photoUrls: photoUrl ? [photoUrl] : [],
      });

      onIssueCreated();
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to submit issue.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '800px' }}>
        {/* Header */}
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center', 
          padding: '20px 28px',
          borderBottom: '1px solid var(--border-color)',
          background: 'rgba(255, 255, 255, 0.02)'
        }}>
          <div>
            <h2 style={{ fontSize: '1.3rem', color: '#fff' }}>📢 Report Civic Incident / Public Grievance</h2>
            <p style={{ fontSize: '0.8rem', color: '#94a3b8' }}>
              Sonipat Municipal Grievance & Smart Redressal Network
            </p>
          </div>
          <button 
            onClick={onClose}
            style={{ background: 'transparent', border: 'none', color: '#94a3b8', cursor: 'pointer' }}
          >
            <X size={24} />
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} style={{ padding: '28px', display: 'flex', flexDirection: 'column', gap: '20px' }}>
          
          {error && (
            <div style={{ background: 'rgba(239, 68, 68, 0.15)', border: '1px solid #ef4444', borderRadius: '8px', padding: '12px', color: '#fca5a5', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <AlertCircle size={18} /> {error}
            </div>
          )}

          {/* Category Selector */}
          <div>
            <label style={{ fontSize: '0.85rem', fontWeight: 600, color: '#e2e8f0', display: 'block', marginBottom: '6px' }}>
              Select Incident Category *
            </label>
            <select 
              className="input-field" 
              value={selectedCategoryId} 
              onChange={(e) => setSelectedCategoryId(e.target.value)}
              required
            >
              {categories.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.primaryCategoryGroup} — {cat.name} (SLA: {cat.defaultSlaHours}h, Routed to: {cat.departmentName})
                </option>
              ))}
            </select>
          </div>

          {/* Title */}
          <div>
            <label style={{ fontSize: '0.85rem', fontWeight: 600, color: '#e2e8f0', display: 'block', marginBottom: '6px' }}>
              Issue Summary / Title *
            </label>
            <input 
              type="text" 
              className="input-field" 
              placeholder="e.g. Broken Water Pipe overflowing into main road" 
              value={title} 
              onChange={(e) => setTitle(e.target.value)}
              required 
              minLength={5}
            />
          </div>

          {/* Description */}
          <div>
            <label style={{ fontSize: '0.85rem', fontWeight: 600, color: '#e2e8f0', display: 'block', marginBottom: '6px' }}>
              Detailed Description *
            </label>
            <textarea 
              className="input-field" 
              placeholder="Describe the severity, exact location landmarks, and hazard risks..." 
              rows={3}
              value={description} 
              onChange={(e) => setDescription(e.target.value)}
              required 
              minLength={10}
            />
          </div>

          {/* Location Picker Map */}
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
              <label style={{ fontSize: '0.85rem', fontWeight: 600, color: '#e2e8f0', display: 'flex', alignItems: 'center', gap: '6px' }}>
                <MapPin size={16} color="#38bdf8" /> Pin Incident GPS Location on Sonipat Map *
              </label>
              <span style={{ fontSize: '0.75rem', color: '#94a3b8' }}>
                GPS: {coords[0].toFixed(5)}, {coords[1].toFixed(5)}
              </span>
            </div>

            {/* Quick Sonipat Preset Badges */}
            <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', marginBottom: '10px' }}>
              {SONIPAT_LOCATIONS.map((preset) => (
                <button
                  key={preset.name}
                  type="button"
                  onClick={() => handlePresetLocation(preset)}
                  style={{
                    fontSize: '0.75rem',
                    padding: '4px 10px',
                    borderRadius: '6px',
                    background: wardSector === preset.ward ? 'rgba(56, 189, 248, 0.2)' : 'rgba(255, 255, 255, 0.05)',
                    color: wardSector === preset.ward ? '#38bdf8' : '#94a3b8',
                    border: wardSector === preset.ward ? '1px solid #38bdf8' : '1px solid rgba(255,255,255,0.1)',
                    cursor: 'pointer'
                  }}
                >
                  📍 {preset.name}
                </button>
              ))}
            </div>

            <IncidentMap 
              isPickerMode={true}
              selectedLocation={coords}
              onLocationSelect={handleLocationSelect}
              height="260px"
            />
          </div>

          {/* Address & Ward Row */}
          <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '12px' }}>
            <div>
              <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: '4px' }}>Address / Landmark</label>
              <input 
                type="text" 
                className="input-field" 
                value={address} 
                onChange={(e) => setAddress(e.target.value)}
                required
              />
            </div>
            <div>
              <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: '4px' }}>Ward / Sector</label>
              <input 
                type="text" 
                className="input-field" 
                value={wardSector} 
                onChange={(e) => setWardSector(e.target.value)}
                required
              />
            </div>
          </div>

          {/* Photo Evidence URL */}
          <div>
            <label style={{ fontSize: '0.85rem', fontWeight: 600, color: '#e2e8f0', display: 'flex', alignItems: 'center', gap: '6px', marginBottom: '6px' }}>
              <Camera size={16} color="#10b981" /> Photo Evidence URL
            </label>
            <input 
              type="url" 
              className="input-field" 
              placeholder="https://..." 
              value={photoUrl} 
              onChange={(e) => setPhotoUrl(e.target.value)}
            />
          </div>

          {/* Sensitive Checkbox */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <input 
              type="checkbox" 
              id="sensitiveToggle"
              checked={isSensitive} 
              onChange={(e) => setIsSensitive(e.target.checked)}
              style={{ width: '16px', height: '16px', accentColor: '#ef4444' }}
            />
            <label htmlFor="sensitiveToggle" style={{ fontSize: '0.85rem', color: '#cbd5e1', cursor: 'pointer' }}>
              Mark as sensitive / confidential incident (restricts citizen visibility)
            </label>
          </div>

          {/* Submit Button */}
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', marginTop: '10px' }}>
            <button type="button" className="btn btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading} style={{ minWidth: '160px' }}>
              {loading ? 'Submitting...' : '🚀 Submit Report'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
