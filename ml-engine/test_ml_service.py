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

if __name__ == "__main__":
    unittest.main()
