import os
import joblib
import numpy as np
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.ensemble import RandomForestRegressor, IsolationForest
from sklearn.preprocessing import OneHotEncoder
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.metrics import mean_squared_error, r2_score, classification_report
from data_generator import generate_dataset

DATA_PATH = "data/sonipat_civic_incidents_50k.csv"
MODEL_DIR = "models"

def train_and_export_models():
    os.makedirs(MODEL_DIR, exist_ok=True)

    if not os.path.exists(DATA_PATH):
        print(f"Dataset {DATA_PATH} not found. Generating fresh dataset...")
        df = generate_dataset(num_records=50000, output_path=DATA_PATH)
    else:
        print(f"Loading dataset from {DATA_PATH}...")
        df = pd.read_csv(DATA_PATH)

    print(f"Loaded {len(df):,} records.")

    # -------------------------------------------------------------
    # 1. Text Classification Model (Complaint Text -> Department)
    # -------------------------------------------------------------
    print("\n📦 [1/4] Training Multi-Lingual Text Classifier...")
    text_pipeline = Pipeline([
        ('tfidf', TfidfVectorizer(max_features=5000, ngram_range=(1, 2))),
        ('clf', LogisticRegression(max_iter=1000, C=1.5))
    ])

    X_text = df['description']
    y_text = df['department_code']
    text_pipeline.fit(X_text, y_text)
    joblib.dump(text_pipeline, os.path.join(MODEL_DIR, "text_classifier.pkl"))
    print(f"✅ Text Classifier saved to {MODEL_DIR}/text_classifier.pkl")

    # -------------------------------------------------------------
    # 2. Resolution Time Predictor (Features -> Hours)
    # -------------------------------------------------------------
    print("\n📦 [2/4] Training Resolution Time Regressor...")
    feature_cols = [
        'department_code', 'category_code', 'ward', 
        'infra_age_years', 'population_density', 'rainfall_mm', 
        'temperature_c', 'is_monsoon', 'severity', 'sla_target_hours'
    ]
    categorical_cols = ['department_code', 'category_code', 'ward', 'severity']
    numerical_cols = ['infra_age_years', 'population_density', 'rainfall_mm', 'temperature_c', 'is_monsoon', 'sla_target_hours']

    preprocessor = ColumnTransformer(transformers=[
        ('cat', OneHotEncoder(handle_unknown='ignore'), categorical_cols),
        ('num', 'passthrough', numerical_cols)
    ])

    regressor_pipeline = Pipeline([
        ('preprocessor', preprocessor),
        ('regressor', RandomForestRegressor(n_estimators=60, max_depth=14, random_state=42, n_jobs=-1))
    ])

    X_reg = df[feature_cols]
    y_reg = df['actual_resolution_hours']

    # Train on 80% split
    split_idx = int(0.8 * len(df))
    X_train, X_test = X_reg.iloc[:split_idx], X_reg.iloc[split_idx:]
    y_train, y_test = y_reg.iloc[:split_idx], y_reg.iloc[split_idx:]

    regressor_pipeline.fit(X_train, y_train)
    y_pred = regressor_pipeline.predict(X_test)
    r2 = r2_score(y_test, y_pred)
    rmse = np.sqrt(mean_squared_error(y_test, y_pred))
    print(f"   Resolution Time Model R²: {r2:.3f}, RMSE: {rmse:.2f} hours")

    joblib.dump(regressor_pipeline, os.path.join(MODEL_DIR, "resolution_time_regressor.pkl"))
    print(f"✅ Resolution Time Regressor saved to {MODEL_DIR}/resolution_time_regressor.pkl")

    # -------------------------------------------------------------
    # 3. Incident Anomaly Detector (Isolation Forest)
    # -------------------------------------------------------------
    print("\n📦 [3/4] Training Incident Anomaly Burst Detector...")
    anomaly_features = ['infra_age_years', 'rainfall_mm', 'temperature_c', 'actual_resolution_hours', 'is_sla_breached']
    iso_forest = IsolationForest(contamination=0.03, random_state=42, n_jobs=-1)
    iso_forest.fit(df[anomaly_features])

    joblib.dump(iso_forest, os.path.join(MODEL_DIR, "anomaly_detector.pkl"))
    print(f"✅ Anomaly Detector saved to {MODEL_DIR}/anomaly_detector.pkl")

    # -------------------------------------------------------------
    # 4. Ward-Level Time-Series Forecasting Baseline Aggregates
    # -------------------------------------------------------------
    print("\n📦 [4/4] Generating Ward Time-Series Baselines & Forecast Index...")
    df['timestamp'] = pd.to_datetime(df['timestamp'])
    df['date'] = df['timestamp'].dt.date

    # Daily incident count by ward and department
    daily_stats = df.groupby(['date', 'ward', 'department_code']).size().reset_index(name='incident_count')
    ward_averages = df.groupby(['ward', 'department_code', 'is_monsoon']).size().reset_index(name='total_incidents')
    
    # Calculate daily incident velocity per ward
    ward_velocity = df.groupby(['ward', 'department_code']).agg(
        avg_daily_incidents=('incident_id', lambda x: round(len(x) / (total_days_est := 1300), 2)),
        avg_resolution_hours=('actual_resolution_hours', 'mean'),
        sla_compliance_rate=('is_sla_breached', lambda x: round((1 - x.mean()) * 100, 1))
    ).reset_index()

    ward_velocity.to_csv(os.path.join(MODEL_DIR, "ward_forecast_baselines.csv"), index=False)
    print(f"✅ Ward Forecast Baselines saved to {MODEL_DIR}/ward_forecast_baselines.csv")

    print("\n🎯 ALL 4 CIVICFIX MACHINE LEARNING MODELS COMPILED & EXPORTED SUCCESSFULLY!\n")

if __name__ == "__main__":
    train_and_export_models()
