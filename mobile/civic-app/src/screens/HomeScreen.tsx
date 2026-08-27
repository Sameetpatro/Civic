import React, { useState, useEffect } from 'react';
import { 
  View, 
  Text, 
  StyleSheet, 
  FlatList, 
  TouchableOpacity, 
  RefreshControl 
} from 'react-native';
import { MobileIssue } from '../types';

export const HomeScreen: React.FC<{ navigation: any }> = ({ navigation }) => {
  const [issues, setIssues] = useState<MobileIssue[]>([
    {
      id: '1',
      referenceNumber: 'CVX-20260828-4821',
      title: 'Water Pipe Leakage at Community Center',
      description: 'Major leak causing flooding across Sector 14 market.',
      categoryName: 'Pipe Leakage (Water)',
      status: 'DepartmentAssigned',
      statusName: 'Department Assigned',
      latitude: 28.9931,
      longitude: 77.0151,
      address: 'Community Center, Sector 14, Sonipat',
      wardSector: 'Sector 14',
      reportedAtUtc: new Date().toISOString(),
      photos: ['https://images.unsplash.com/photo-1541888946425-d0fbb180c5f5?w=500']
    },
    {
      id: '2',
      referenceNumber: 'CVX-20260828-8912',
      title: 'Deep Pothole on Murthal Road',
      description: 'Deep road cavity causing severe traffic slowdown.',
      categoryName: 'Road Potholes & Damage',
      status: 'InProgress',
      statusName: 'In Progress',
      latitude: 29.0125,
      longitude: 77.0385,
      address: 'Murthal Road Flyover, Sonipat',
      wardSector: 'Murthal Road',
      reportedAtUtc: new Date().toISOString(),
      photos: ['https://images.unsplash.com/photo-1515162816999-a0c47dc192f7?w=500']
    }
  ]);
  const [refreshing, setRefreshing] = useState(false);

  const onRefresh = () => {
    setRefreshing(true);
    setTimeout(() => setRefreshing(false), 800);
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.headerTitle}>🏛️ CivicFix Sonipat</Text>
        <Text style={styles.headerSubtitle}>Citizen Redressal & Field Operations</Text>
      </View>

      <View style={styles.actionBanner}>
        <TouchableOpacity 
          style={styles.reportButton}
          onPress={() => navigation.navigate('Report')}
        >
          <Text style={styles.reportButtonText}>📸 Report Incident with Camera & GPS</Text>
        </TouchableOpacity>
      </View>

      <Text style={styles.sectionTitle}>Recent Incidents Nearby</Text>

      <FlatList
        data={issues}
        keyExtractor={(item) => item.id}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        renderItem={({ item }) => (
          <TouchableOpacity 
            style={styles.card}
            onPress={() => navigation.navigate('Detail', { issueId: item.id })}
          >
            <View style={styles.cardHeader}>
              <Text style={styles.refNumber}>{item.referenceNumber}</Text>
              <View style={styles.statusBadge}>
                <Text style={styles.statusText}>{item.statusName}</Text>
              </View>
            </View>
            <Text style={styles.cardTitle}>{item.title}</Text>
            <Text style={styles.cardLocation}>📍 {item.wardSector} • {item.address}</Text>
          </TouchableOpacity>
        )}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#0a0e17',
    padding: 16,
  },
  header: {
    marginTop: 30,
    marginBottom: 20,
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: '800',
    color: '#38bdf8',
  },
  headerSubtitle: {
    fontSize: 13,
    color: '#94a3b8',
    marginTop: 4,
  },
  actionBanner: {
    marginBottom: 20,
  },
  reportButton: {
    backgroundColor: '#0284c7',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
    shadowColor: '#0284c7',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
  },
  reportButtonText: {
    color: '#ffffff',
    fontWeight: '700',
    fontSize: 15,
  },
  sectionTitle: {
    fontSize: 17,
    fontWeight: '700',
    color: '#f8fafc',
    marginBottom: 12,
  },
  card: {
    backgroundColor: '#111827',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.08)',
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  refNumber: {
    fontSize: 12,
    fontWeight: '700',
    color: '#38bdf8',
  },
  statusBadge: {
    backgroundColor: 'rgba(56, 189, 248, 0.15)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
  statusText: {
    fontSize: 11,
    fontWeight: '600',
    color: '#38bdf8',
  },
  cardTitle: {
    fontSize: 15,
    fontWeight: '700',
    color: '#ffffff',
    marginBottom: 6,
  },
  cardLocation: {
    fontSize: 12,
    color: '#94a3b8',
  },
});
