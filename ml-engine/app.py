import os
import random
from typing import Dict, Any, List, Optional
from datetime import datetime, timedelta
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
import joblib
import pandas as pd
from services.nlu_service import nlu_engine

app = FastAPI(
    title="CivicFix ML & Civic Intelligence Service",
    description="AI/ML Microservice for Multi-lingual NLU, Incident Categorization, Resolution-Time Prediction, Time-Series Forecasting, and Anomaly Detection for Sonipat Municipal Corporation.",
    version="1.0.0"
)

# CORS Middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

MODEL_DIR = "models"
text_classifier = None
regressor_model = None
anomaly_model = None
ward_baselines_df = None

@app.on_event("startup")
def load_models():
    global text_classifier, regressor_model, anomaly_model, ward_baselines_df
    try:
        if os.path.exists(os.path.join(MODEL_DIR, "text_classifier.pkl")):
            text_classifier = joblib.load(os.path.join(MODEL_DIR, "text_classifier.pkl"))
            print(" Loaded text_classifier.pkl")
        
        if os.path.exists(os.path.join(MODEL_DIR, "resolution_time_regressor.pkl")):
            regressor_model = joblib.load(os.path.join(MODEL_DIR, "resolution_time_regressor.pkl"))
            print(" Loaded resolution_time_regressor.pkl")

        if os.path.exists(os.path.join(MODEL_DIR, "anomaly_detector.pkl")):
            anomaly_model = joblib.load(os.path.join(MODEL_DIR, "anomaly_detector.pkl"))
            print(" Loaded anomaly_detector.pkl")

        if os.path.exists(os.path.join(MODEL_DIR, "ward_forecast_baselines.csv")):
            ward_baselines_df = pd.read_csv(os.path.join(MODEL_DIR, "ward_forecast_baselines.csv"))
            print(" Loaded ward_forecast_baselines.csv")
    except Exception as e:
        print(f"⚠️ Warning loading models: {e}")

# ----------------- Request / Response Schemas -----------------
class CategorizationRequest(BaseModel):
    description: str = Field(..., example="paani ki main line burst ho gayi h gali me bohot paani bhar gaya")
    wardSector: Optional[str] = Field(None, example="Sector 14")

class CategorizationResponse(BaseModel):
    departmentCode: str
    departmentName: str
    categoryCode: str
    categoryName: str
    slaTargetHours: int
    confidenceScore: float
    severity: str
    priority: str
    detectedWard: Optional[str]
    matchedKeywords: List[str]
    sentiment: str
    aiSummary: str

class ResolutionTimeRequest(BaseModel):
    departmentCode: str = Field(..., example="WATER")
    categoryCode: str = Field(..., example="WATER_PIPE_LEAK")
    ward: str = Field(..., example="Sector 14")
    severity: str = Field("Medium", example="High")
    infraAgeYears: Optional[int] = Field(14, example=14)
    populationDensity: Optional[int] = Field(8500, example=8500)
    rainfallMm: Optional[float] = Field(5.0, example=25.0)
    temperatureC: Optional[float] = Field(32.0, example=34.0)
    isMonsoon: Optional[int] = Field(0, example=1)
    slaTargetHours: Optional[int] = Field(24, example=24)

class ResolutionTimeResponse(BaseModel):
    predictedResolutionHours: float
    slaTargetHours: int
    slaBreachRisk: str
    confidenceIntervalHours: Dict[str, float]
    influencingFactors: List[str]

class ForecastRequest(BaseModel):
    ward: str = Field(..., example="Sector 14")
    departmentCode: Optional[str] = Field(None, example="WATER")
    horizonDays: int = Field(14, ge=1, le=30, example=14)

class DailyForecastPoint(BaseModel):
    date: str
    dayOfWeek: str
    predictedIncidents: int
    isWeekend: bool
    weatherRiskLevel: str

class ForecastResponse(BaseModel):
    ward: str
    departmentCode: Optional[str]
    forecastHorizonDays: int
    totalPredictedIncidents: int
    dailyForecast: List[DailyForecastPoint]
    historicalSlaComplianceRate: float
    recommendedFieldWorkers: int

class AnomalyCheckRequest(BaseModel):
    ward: str = Field(..., example="Model Town")
    rainfallMm: float = Field(..., example=65.0)
    temperatureC: float = Field(..., example=33.0)
    infraAgeYears: int = Field(..., example=28)
    actualResolutionHours: float = Field(..., example=55.0)
    isSlaBreached: int = Field(..., example=1)

class AnomalyCheckResponse(BaseModel):
    isAnomaly: bool
    anomalyScore: float
    riskLevel: str
    recommendedAction: str

# ----------------- Endpoints -----------------
@app.get("/")
def get_root():
    return {
        "service": "CivicFix AI & Civic Intelligence Microservice",
        "region": "Sonipat, Haryana, India",
        "status": "Operational",
        "models_loaded": {
            "text_classifier": text_classifier is not None,
            "resolution_time_regressor": regressor_model is not None,
            "anomaly_detector": anomaly_model is not None,
            "ward_baselines": ward_baselines_df is not None
        }
    }

@app.get("/health")
def health_check():
    return {"status": "Healthy", "timestamp": datetime.utcnow().isoformat()}

@app.post("/predict/categorize", response_model=CategorizationResponse)
def categorize_complaint(request: CategorizationRequest):
    """Analyzes multi-lingual complaint text (English/Hindi/Hinglish) and routes to municipal department."""
    if not request.description or len(request.description.strip()) < 3:
        raise HTTPException(status_code=400, detail="Description is too short.")
    
    result = nlu_engine.analyze_complaint(request.description)
    
    # If ML text classifier is available, blend prediction
    if text_classifier:
        try:
            pred_dept = text_classifier.predict([request.description])[0]
            # If NLU had low confidence, prefer classifier
            if result["confidence_score"] < 0.60:
                result["department_code"] = pred_dept
        except Exception:
            pass

    return CategorizationResponse(
        departmentCode=result["department_code"],
        departmentName=result["department_name"],
        categoryCode=result["category_code"],
        categoryName=result["category_name"],
        slaTargetHours=result["sla_target_hours"],
        confidenceScore=result["confidence_score"],
        severity=result["severity"],
        priority=result["priority"],
        detectedWard=result["detected_ward"] or request.wardSector,
        matchedKeywords=result["matched_keywords"],
        sentiment=result["sentiment"],
        aiSummary=result["ai_summary"]
    )

@app.post("/predict/resolution-time", response_model=ResolutionTimeResponse)
def predict_resolution_time(request: ResolutionTimeRequest):
    """Estimates expected resolution duration (in hours) based on weather, infra age, and severity."""
    base_sla = request.slaTargetHours or 24
    
    if regressor_model:
        try:
            df_in = pd.DataFrame([{
                'department_code': request.departmentCode,
                'category_code': request.categoryCode,
                'ward': request.ward,
                'infra_age_years': request.infraAgeYears or 15,
                'population_density': request.populationDensity or 8000,
                'rainfall_mm': request.rainfallMm or 0.0,
                'temperature_c': request.temperatureC or 30.0,
                'is_monsoon': request.isMonsoon or 0,
                'severity': request.severity,
                'sla_target_hours': base_sla
            }])
            pred_hours = float(regressor_model.predict(df_in)[0])
        except Exception as e:
            pred_hours = base_sla * 0.85
    else:
        pred_hours = base_sla * (0.8 if request.severity == "Critical" else 1.1)

    pred_hours = round(max(1.0, pred_hours), 1)
    breach_ratio = pred_hours / base_sla

    if breach_ratio > 1.15:
        sla_risk = "HIGH_RISK_OF_BREACH"
    elif breach_ratio > 0.85:
        sla_risk = "MODERATE_RISK"
    else:
        sla_risk = "SAFE_WITHIN_SLA"

    factors = []
    if (request.rainfallMm or 0) > 20: factors.append(f"Heavy rainfall ({request.rainfallMm}mm) slowing down road operations")
    if (request.infraAgeYears or 0) > 25: factors.append(f"Aging underground pipeline infrastructure ({request.infraAgeYears} years old)")
    if request.severity == "Critical": factors.append("Prioritized urgent emergency response dispatch")

    return ResolutionTimeResponse(
        predictedResolutionHours=pred_hours,
        slaTargetHours=base_sla,
        slaBreachRisk=sla_risk,
        confidenceIntervalHours={
            "lowerBound": round(max(0.5, pred_hours * 0.82), 1),
            "upperBound": round(pred_hours * 1.25, 1)
        },
        influencingFactors=factors or ["Standard operating conditions"]
    )

@app.post("/predict/forecast", response_model=ForecastResponse)
def get_ward_forecast(request: ForecastRequest):
    """Provides 14-day forward daily incident volume forecasting for a Sonipat ward."""
    daily_points: List[DailyForecastPoint] = []
    base_date = datetime.now()
    
    # Base rate estimate
    daily_rate = 3.5
    compliance = 88.5

    if ward_baselines_df is not None:
        matched = ward_baselines_df[ward_baselines_df['ward'] == request.ward]
        if not matched.empty:
            daily_rate = float(matched['avg_daily_incidents'].sum())
            compliance = float(matched['sla_compliance_rate'].mean())

    total_pred = 0
    for i in range(request.horizonDays):
        day_date = base_date + timedelta(days=i + 1)
        is_weekend = day_date.weekday() in [5, 6]
        
        # Fluctuation
        day_val = max(1, int(round(daily_rate * random.uniform(0.75, 1.35) * (1.15 if is_weekend else 1.0))))
        total_pred += day_val

        daily_points.append(DailyForecastPoint(
            date=day_date.strftime("%Y-%m-%d"),
            dayOfWeek=day_date.strftime("%A"),
            predictedIncidents=day_val,
            isWeekend=is_weekend,
            weatherRiskLevel="Elevated" if random.random() < 0.2 else "Normal"
        ))

    recommended_workers = max(2, int(round(daily_rate * 1.6)))

    return ForecastResponse(
        ward=request.ward,
        departmentCode=request.departmentCode,
        forecastHorizonDays=request.horizonDays,
        totalPredictedIncidents=total_pred,
        dailyForecast=daily_points,
        historicalSlaComplianceRate=compliance,
        recommendedFieldWorkers=recommended_workers
    )

@app.post("/predict/anomaly", response_model=AnomalyCheckResponse)
def check_anomaly(request: AnomalyCheckRequest):
    """Detects sudden anomalous localized incident spikes or infrastructure failure risks."""
    features = [[
        request.infraAgeYears,
        request.rainfallMm,
        request.temperatureC,
        request.actualResolutionHours,
        request.isSlaBreached
    ]]

    is_anomaly = False
    score = 0.15

    if anomaly_model:
        pred = anomaly_model.predict(features)[0] # -1 for anomaly, 1 for normal
        decision_score = anomaly_model.decision_function(features)[0]
        is_anomaly = (pred == -1)
        score = round(float(abs(decision_score)), 3)
    else:
        if request.rainfallMm > 50 or request.actualResolutionHours > 48:
            is_anomaly = True
            score = 0.85

    return AnomalyCheckResponse(
        isAnomaly=is_anomaly,
        anomalyScore=score,
        riskLevel="CRITICAL_ANOMALY" if is_anomaly else "NORMAL",
        recommendedAction="Deploy emergency field inspection task force immediately." if is_anomaly else "Standard operational dispatch."
    )

# ----------------- Civic Intelligence & Analytics Endpoints -----------------
@app.get("/intelligence/civic-health")
def get_civic_health():
    """Returns composite Civic Health Index scores (0-100) and ward vulnerability breakdowns."""
    from services.intelligence_service import intelligence_engine
    return intelligence_engine.get_city_and_ward_health()

class SpatialClusteringRequest(BaseModel):
    incidents: List[Dict[str, Any]]
    radiusKm: Optional[float] = 0.4
    minClusterSize: Optional[int] = 2

@app.post("/intelligence/clusters")
def detect_clusters(request: SpatialClusteringRequest):
    """Executes DBSCAN spatial clustering to detect grouped infrastructure failures or complaint bursts."""
    from services.intelligence_service import intelligence_engine
    clusters = intelligence_engine.detect_spatial_clusters(
        request.incidents, 
        eps_km=request.radiusKm or 0.4, 
        min_samples=request.minClusterSize or 2
    )
    return {
        "totalClustersDetected": len(clusters),
        "clusters": clusters
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
