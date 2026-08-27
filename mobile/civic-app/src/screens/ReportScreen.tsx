import React, { useState } from 'react';
import { 
  View, 
  Text, 
  TextInput, 
  TouchableOpacity, 
  StyleSheet, 
  ScrollView, 
  Alert 
} from 'react-native';

export const ReportScreen: React.FC<{ navigation: any }> = ({ navigation }) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [address, setAddress] = useState('Sector 14, Main Commercial Complex, Sonipat');
  const [ward, setWard] = useState('Sector 14');
  const [category, setCategory] = useState('Water Supply & Sewerage');
  const [coords, setCoords] = useState({ lat: 28.9931, lng: 77.0151 });
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = () => {
    if (!title || !description) {
      Alert.alert('Validation Error', 'Please fill in both title and description.');
      return;
    }

    setSubmitting(true);
    setTimeout(() => {
      setSubmitting(false);
      Alert.alert(
        'Incident Reported',
        'Your incident has been logged and routed to Sonipat Municipal Department.',
        [{ text: 'OK', onPress: () => navigation.goBack() }]
      );
    }, 1000);
  };

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>📢 Report Civic Issue</Text>
      <Text style={styles.subtitle}>Direct Redressal • Sonipat Municipal Corporation</Text>

      <View style={styles.formGroup}>
        <Text style={styles.label}>Category</Text>
        <TextInput 
          style={styles.input} 
          value={category} 
          onChangeText={setCategory} 
        />
      </View>

      <View style={styles.formGroup}>
        <Text style={styles.label}>Incident Title</Text>
        <TextInput 
          style={styles.input} 
          placeholder="e.g. Broken Streetlight causing darkness" 
          placeholderTextColor="#64748b"
          value={title} 
          onChangeText={setTitle} 
        />
      </View>

      <View style={styles.formGroup}>
        <Text style={styles.label}>Description</Text>
        <TextInput 
          style={[styles.input, { height: 90, textAlignVertical: 'top' }]} 
          placeholder="Detail the issue and exact location markers..." 
          placeholderTextColor="#64748b"
          multiline 
          numberOfLines={4}
          value={description} 
          onChangeText={setDescription} 
        />
      </View>

      <View style={styles.formGroup}>
        <Text style={styles.label}>📍 GPS Location Detected</Text>
        <View style={styles.locationBox}>
          <Text style={styles.locationText}>
            Lat: {coords.lat.toFixed(4)}, Lng: {coords.lng.toFixed(4)} ({ward})
          </Text>
        </View>
      </View>

      <View style={styles.formGroup}>
        <Text style={styles.label}>Address / Landmark</Text>
        <TextInput 
          style={styles.input} 
          value={address} 
          onChangeText={setAddress} 
        />
      </View>

      <TouchableOpacity 
        style={styles.cameraButton}
        onPress={() => Alert.alert('Camera Capture', 'Incident photo captured and attached successfully.')}
      >
        <Text style={styles.cameraButtonText}>📸 Capture Photo Evidence</Text>
      </TouchableOpacity>

      <TouchableOpacity 
        style={styles.submitButton}
        onPress={handleSubmit}
        disabled={submitting}
      >
        <Text style={styles.submitButtonText}>
          {submitting ? 'Submitting...' : '🚀 Submit Report'}
        </Text>
      </TouchableOpacity>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#0a0e17',
    padding: 16,
  },
  title: {
    fontSize: 22,
    fontWeight: '800',
    color: '#ffffff',
    marginTop: 20,
  },
  subtitle: {
    fontSize: 12,
    color: '#94a3b8',
    marginBottom: 20,
  },
  formGroup: {
    marginBottom: 16,
  },
  label: {
    fontSize: 13,
    fontWeight: '600',
    color: '#e2e8f0',
    marginBottom: 6,
  },
  input: {
    backgroundColor: '#111827',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 8,
    padding: 12,
    color: '#ffffff',
    fontSize: 14,
  },
  locationBox: {
    backgroundColor: '#172033',
    padding: 12,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#38bdf8',
  },
  locationText: {
    color: '#38bdf8',
    fontWeight: '600',
    fontSize: 13,
  },
  cameraButton: {
    backgroundColor: 'rgba(16, 185, 129, 0.15)',
    borderWidth: 1,
    borderColor: '#10b981',
    padding: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginBottom: 20,
  },
  cameraButtonText: {
    color: '#34d399',
    fontWeight: '700',
    fontSize: 14,
  },
  submitButton: {
    backgroundColor: '#0284c7',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
    marginBottom: 40,
  },
  submitButtonText: {
    color: '#ffffff',
    fontWeight: '700',
    fontSize: 16,
  },
});
