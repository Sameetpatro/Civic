import React from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMapEvents } from 'react-leaflet';
import L from 'leaflet';
import type { IssueSummary } from '../types';

// Sonipat coordinates
const SONIPAT_CENTER: [number, number] = [28.9931, 77.0151];

// Category color mapping
const getCategoryColor = (categoryGroup: string) => {
  switch (categoryGroup?.toUpperCase()) {
    case 'WATER': return '#38bdf8';
    case 'ROADS': return '#f59e0b';
    case 'GARBAGE': return '#10b981';
    case 'DRAINAGE': return '#06b6d4';
    case 'ELECTRICITY':
    case 'STREETLIGHT': return '#eab308';
    case 'TREES': return '#22c55e';
    case 'ANIMAL': return '#f97316';
    case 'CONSTRUCTION': return '#ec4899';
    default: return '#818cf8';
  }
};

// Create dynamic SVG icon with pulse for critical/high severity
const createCustomIcon = (categoryGroup: string, severity: string) => {
  const color = getCategoryColor(categoryGroup);
  const isCritical = severity === 'Critical' || severity === 'High';

  const html = `
    <div style="position: relative; width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;">
      ${isCritical ? `<div style="position: absolute; width: 38px; height: 38px; border-radius: 50%; background: ${color}; opacity: 0.4; animation: ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite;"></div>` : ''}
      <div style="width: 24px; height: 24px; border-radius: 50%; background: #111827; border: 2px solid ${color}; box-shadow: 0 0 10px ${color}; display: flex; align-items: center; justify-content: center;">
        <div style="width: 10px; height: 10px; border-radius: 50%; background: ${color};"></div>
      </div>
    </div>
  `;

  return L.divIcon({
    html,
    className: 'custom-map-marker',
    iconSize: [32, 32],
    iconAnchor: [16, 16],
    popupAnchor: [0, -16],
  });
};

interface LocationPickerProps {
  onLocationSelect: (lat: number, lng: number) => void;
  selectedPos?: [number, number];
}

const LocationPickerHandler: React.FC<LocationPickerProps> = ({ onLocationSelect, selectedPos }) => {
  useMapEvents({
    click(e) {
      onLocationSelect(e.latlng.lat, e.latlng.lng);
    },
  });

  if (!selectedPos) return null;

  const pinIcon = L.divIcon({
    html: `
      <div style="position: relative; width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;">
        <div style="width: 28px; height: 28px; border-radius: 50%; background: #ef4444; border: 3px solid #fff; box-shadow: 0 0 15px #ef4444; display: flex; align-items: center; justify-content: center; color: #fff; font-weight: bold; font-size: 14px;">📍</div>
      </div>
    `,
    className: 'picker-pin',
    iconSize: [36, 36],
    iconAnchor: [18, 18],
  });

  return <Marker position={selectedPos} icon={pinIcon} />;
};

interface IncidentMapProps {
  issues?: IssueSummary[];
  onSelectIssue?: (issueId: string) => void;
  isPickerMode?: boolean;
  selectedLocation?: [number, number];
  onLocationSelect?: (lat: number, lng: number) => void;
  height?: string;
}

export const IncidentMap: React.FC<IncidentMapProps> = ({
  issues = [],
  onSelectIssue,
  isPickerMode = false,
  selectedLocation,
  onLocationSelect,
  height = '480px',
}) => {
  return (
    <div style={{ height, width: '100%', borderRadius: '12px', overflow: 'hidden', border: '1px solid rgba(255, 255, 255, 0.1)' }}>
      <MapContainer
        center={selectedLocation || SONIPAT_CENTER}
        zoom={13}
        scrollWheelZoom={true}
        style={{ height: '100%', width: '100%' }}
      >
        {/* CartoDB Dark Matter tiles for modern sleek aesthetic */}
        <TileLayer
          attribution='&copy; <a href="https://carto.com/">CARTO</a> & OpenStreetMap'
          url="https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png"
        />

        {isPickerMode && onLocationSelect && (
          <LocationPickerHandler
            onLocationSelect={onLocationSelect}
            selectedPos={selectedLocation}
          />
        )}

        {!isPickerMode && issues.map((issue) => (
          <Marker
            key={issue.id}
            position={[issue.latitude, issue.longitude]}
            icon={createCustomIcon(issue.primaryCategoryGroup, issue.severity)}
          >
            <Popup>
              <div style={{ padding: '6px', maxWidth: '240px' }}>
                <div style={{ fontSize: '0.75rem', color: '#94a3b8', marginBottom: '2px' }}>
                  {issue.referenceNumber} • {issue.wardSector}
                </div>
                <div style={{ fontWeight: 700, fontSize: '0.95rem', color: '#fff', marginBottom: '6px' }}>
                  {issue.title}
                </div>
                <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap', marginBottom: '8px' }}>
                  <span style={{ 
                    fontSize: '0.7rem', 
                    padding: '2px 6px', 
                    borderRadius: '4px', 
                    background: getCategoryColor(issue.primaryCategoryGroup) + '22',
                    color: getCategoryColor(issue.primaryCategoryGroup),
                    border: `1px solid ${getCategoryColor(issue.primaryCategoryGroup)}44`
                  }}>
                    {issue.categoryName}
                  </span>
                  <span style={{ 
                    fontSize: '0.7rem', 
                    padding: '2px 6px', 
                    borderRadius: '4px', 
                    background: 'rgba(255,255,255,0.1)',
                    color: '#e2e8f0'
                  }}>
                    {issue.statusName}
                  </span>
                </div>
                {onSelectIssue && (
                  <button
                    onClick={() => onSelectIssue(issue.id)}
                    style={{
                      width: '100%',
                      padding: '6px 10px',
                      borderRadius: '6px',
                      background: '#0284c7',
                      color: '#fff',
                      border: 'none',
                      fontSize: '0.8rem',
                      fontWeight: 600,
                      cursor: 'pointer',
                    }}
                  >
                    Inspect Lifecycle →
                  </button>
                )}
              </div>
            </Popup>
          </Marker>
        ))}
      </MapContainer>
    </div>
  );
};
