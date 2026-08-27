import React, { useState } from 'react';
import { View, StyleSheet, SafeAreaView, StatusBar, TouchableOpacity, Text } from 'react-native';
import { HomeScreen } from './src/screens/HomeScreen';
import { ReportScreen } from './src/screens/ReportScreen';
import { WorkerJobScreen } from './src/screens/WorkerJobScreen';

export default function App() {
  const [currentScreen, setCurrentScreen] = useState<'Home' | 'Report' | 'Worker'>('Home');

  const navigation = {
    navigate: (screen: 'Home' | 'Report' | 'Worker') => setCurrentScreen(screen),
    goBack: () => setCurrentScreen('Home'),
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar barStyle="light-content" backgroundColor="#0a0e17" />
      <View style={styles.container}>
        {currentScreen === 'Home' && <HomeScreen navigation={navigation} />}
        {currentScreen === 'Report' && <ReportScreen navigation={navigation} />}
        {currentScreen === 'Worker' && <WorkerJobScreen navigation={navigation} route={{}} />}

        {/* Bottom Tab Bar */}
        <View style={styles.bottomBar}>
          <TouchableOpacity 
            style={styles.tabItem} 
            onPress={() => setCurrentScreen('Home')}
          >
            <Text style={[styles.tabText, currentScreen === 'Home' && styles.activeTab]}>
              🏠 Citizen Feed
            </Text>
          </TouchableOpacity>

          <TouchableOpacity 
            style={styles.tabItem} 
            onPress={() => setCurrentScreen('Report')}
          >
            <Text style={[styles.tabText, currentScreen === 'Report' && styles.activeTab]}>
              📸 Report
            </Text>
          </TouchableOpacity>

          <TouchableOpacity 
            style={styles.tabItem} 
            onPress={() => setCurrentScreen('Worker')}
          >
            <Text style={[styles.tabText, currentScreen === 'Worker' && styles.activeTab]}>
              🔧 Worker Console
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#0a0e17',
  },
  container: {
    flex: 1,
    backgroundColor: '#0a0e17',
  },
  bottomBar: {
    flexDirection: 'row',
    backgroundColor: '#111827',
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.08)',
    paddingVertical: 12,
    paddingHorizontal: 8,
  },
  tabItem: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  tabText: {
    color: '#94a3b8',
    fontSize: 12,
    fontWeight: '600',
  },
  activeTab: {
    color: '#38bdf8',
    fontWeight: '800',
  },
});
