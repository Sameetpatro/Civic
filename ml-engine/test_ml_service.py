import unittest
from fastapi.testclient import TestClient
from app import app

class TestCivicMlService(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.client = TestClient(app)

    def test_health_endpoint(self):
        response = self.client.get("/health")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["status"], "Healthy")

    def test_nlu_categorize_hindi_hinglish_water(self):
        # Hinglish: "paani ki line phat gayi h road par bohot paani bhar gaya"
        payload = {
            "description": "paani ki pipeline phat gayi h gali me bohot paani beh raha h",
            "wardSector": "Sector 14"
        }
        response = self.client.post("/predict/categorize", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["departmentCode"], "WATER")
        self.assertEqual(data["categoryCode"], "WATER_PIPE_LEAK")
        self.assertGreaterEqual(data["confidenceScore"], 0.6)

    def test_nlu_categorize_critical_electricity_wire(self):
        payload = {
            "description": "live electrical wire snapped sparking heavily near park gate",
            "wardSector": "Model Town"
        }
        response = self.client.post("/predict/categorize", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["departmentCode"], "ELECTRICITY")
        self.assertEqual(data["severity"], "Critical")
        self.assertEqual(data["priority"], "Urgent")

    def test_resolution_time_prediction(self):
        payload = {
            "departmentCode": "ROADS",
            "categoryCode": "ROAD_POTHOLE",
            "ward": "Murthal Road",
            "severity": "Medium",
            "infraAgeYears": 10,
            "rainfallMm": 35.0,
            "temperatureC": 30.0,
            "isMonsoon": 1,
            "slaTargetHours": 48
        }
        response = self.client.post("/predict/resolution-time", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn("predictedResolutionHours", data)
        self.assertGreater(data["predictedResolutionHours"], 0)
        self.assertIn("confidenceIntervalHours", data)

    def test_ward_forecast(self):
        payload = {
            "ward": "Sector 14",
            "departmentCode": "WATER",
            "horizonDays": 14
        }
        response = self.client.post("/predict/forecast", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["ward"], "Sector 14")
        self.assertEqual(len(data["dailyForecast"]), 14)
        self.assertGreater(data["totalPredictedIncidents"], 0)

    def test_anomaly_detector_trigger(self):
        payload = {
            "ward": "Model Town",
            "rainfallMm": 85.0,
            "temperatureC": 34.0,
            "infraAgeYears": 35,
            "actualResolutionHours": 60.0,
            "isSlaBreached": 1
        }
        response = self.client.post("/predict/anomaly", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn("isAnomaly", data)
        self.assertIn("anomalyScore", data)

    def test_civic_health_index(self):
        response = self.client.get("/intelligence/civic-health")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn("overallCityCivicHealthIndex", data)
        self.assertGreater(data["overallCityCivicHealthIndex"], 0)
        self.assertIn("wardHealthBreakdown", data)

    def test_spatial_clustering_detection(self):
        # 3 nearby points in Sector 14
        payload = {
            "incidents": [
                {"referenceNumber": "CVX-001", "latitude": 28.9931, "longitude": 77.0151, "categoryName": "Water Leak", "wardSector": "Sector 14"},
                {"referenceNumber": "CVX-002", "latitude": 28.9933, "longitude": 77.0152, "categoryName": "Water Leak", "wardSector": "Sector 14"},
                {"referenceNumber": "CVX-003", "latitude": 28.9932, "longitude": 77.0150, "categoryName": "Water Leak", "wardSector": "Sector 14"},
                {"referenceNumber": "CVX-004", "latitude": 29.0500, "longitude": 77.1000, "categoryName": "Pothole", "wardSector": "Outer"}
            ],
            "radiusKm": 0.5,
            "minClusterSize": 2
        }
        response = self.client.post("/intelligence/clusters", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertGreaterEqual(data["totalClustersDetected"], 1)

if __name__ == "__main__":
    unittest.main()
