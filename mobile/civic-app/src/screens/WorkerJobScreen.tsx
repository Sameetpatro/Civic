import React, { useState } from 'react';
import { 
  View, 
  Text, 
  TouchableOpacity, 
  StyleSheet, 
  ScrollView, 
  TextInput, 
  Alert 
} from 'react-native';

export const WorkerJobScreen: React.FC<{ navigation: any; route: any }> = ({ navigation, route }) => {
  const [status, setStatus] = useState<'WorkerAssigned' | 'Accepted' | 'InProgress' | 'Resolved'>('WorkerAssigned');
  const [resolutionNotes, setResolutionNotes] = useState('');

  const handleAccept = () => {
    setStatus('Accepted');
    Alert.alert('Job Acknowledged', 'Dispatch confirmed. You are now designated to resolve this incident.');
  };

  const handleStart = () => {
    setStatus('InProgress');
    Alert.alert('Work Commenced', 'Timer started. GPS timestamp recorded on site.');
  };

  const handleResolve = () => {
    if (!resolutionNotes) {
      Alert.alert('Notes Required', 'Please input details of the repair work performed.');
      return;
    }
    setStatus('Resolved');
    Alert.alert('Resolution Submitted', 'Work marked resolved. Citizen verification requested.', [
      { text: 'OK', onPress: () => navigation.goBack() }
    ]);
  };

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>🔧 Field Worker Execution</Text>
      <Text style={styles.ref}>CVX-20260828-4821 • Sector 14 Sonipat</Text>

      <View style={styles.card}>
        <Text style={styles.jobTitle}>Water Pipe Leakage at Community Center</Text>
        <Text style={styles.desc}>Urgent repair requested. Water pressure dropping in surrounding blocks.</Text>
        <Text style={styles.loc}>📍 Sector 14, Main Road (Landmark: Near Gate 2)</Text>
      </View>

      <View style={styles.statusBox}>
        <Text style={styles.statusLabel}>Current Operational Status:</Text>
        <Text style={styles.statusValue}>{status}</Text>
      </View>

      {status === 'WorkerAssigned' && (
        <TouchableOpacity style={styles.btnPrimary} onPress={handleAccept}>
          <Text style={styles.btnText}>✅ Accept Dispatch & Acknowledge</Text>
        </TouchableOpacity>
      )}

      {status === 'Accepted' && (
        <TouchableOpacity style={styles.btnPrimary} onPress={handleStart}>
          <Text style={styles.btnText}>📍 Arrived on Site & Start Work</Text>
        </TouchableOpacity>
      )}

      {status === 'InProgress' && (
        <View style={{ marginTop: 10 }}>
          <Text style={styles.label}>Resolution / Repair Notes:</Text>
          <TextInput 
            style={styles.textArea} 
            placeholder="Replaced damaged 4-inch connector joint and tested pressure..." 
            placeholderTextColor="#64748b"
            multiline 
            numberOfLines={4}
            value={resolutionNotes} 
            onChangeText={setResolutionNotes} 
          />
          <TouchableOpacity 
            style={styles.cameraBtn}
            onPress={() => Alert.alert('Proof Captured', 'Completed work photo attached.')}
          >
            <Text style={styles.cameraBtnText}>📸 Capture After-Repair Photo</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.btnSuccess} onPress={handleResolve}>
            <Text style={styles.btnText}>🚀 Complete & Submit Resolution</Text>
          </TouchableOpacity>
        </View>
      )}
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#0a0e17', padding: 16 },
  title: { fontSize: 22, fontWeight: '800', color: '#fff', marginTop: 20 },
  ref: { fontSize: 13, color: '#38bdf8', marginBottom: 20 },
  card: { backgroundColor: '#111827', borderRadius: 12, padding: 16, marginBottom: 16 },
  jobTitle: { fontSize: 16, fontWeight: '700', color: '#fff', marginBottom: 6 },
  desc: { fontSize: 13, color: '#cbd5e1', lineHeight: 18, marginBottom: 8 },
  loc: { fontSize: 12, color: '#94a3b8' },
  statusBox: { backgroundColor: '#172033', padding: 14, borderRadius: 10, marginBottom: 20 },
  statusLabel: { fontSize: 12, color: '#94a3b8' },
  statusValue: { fontSize: 18, fontWeight: '800', color: '#fbbf24', marginTop: 4 },
  btnPrimary: { backgroundColor: '#0284c7', padding: 16, borderRadius: 12, alignItems: 'center', marginBottom: 20 },
  btnSuccess: { backgroundColor: '#059669', padding: 16, borderRadius: 12, alignItems: 'center', marginBottom: 30 },
  btnText: { color: '#fff', fontWeight: '700', fontSize: 15 },
  label: { color: '#e2e8f0', fontSize: 13, fontWeight: '600', marginBottom: 6 },
  textArea: { backgroundColor: '#111827', borderRadius: 8, borderWidth: 1, borderColor: '#334155', color: '#fff', padding: 12, height: 90, textAlignVertical: 'top', marginBottom: 14 },
  cameraBtn: { backgroundColor: 'rgba(56, 189, 248, 0.15)', borderWidth: 1, borderColor: '#38bdf8', padding: 14, borderRadius: 10, alignItems: 'center', marginBottom: 14 },
  cameraBtnText: { color: '#38bdf8', fontWeight: '700', fontSize: 14 },
});
