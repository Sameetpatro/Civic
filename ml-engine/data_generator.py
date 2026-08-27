import os
import random
import csv
from datetime import datetime, timedelta
import numpy as np
import pandas as pd

# Sonipat Wards and Landmarks
SONIPAT_WARDS = [
    {"ward": "Sector 14", "lat": 28.9931, "lng": 77.0151, "infra_age_years": 14, "population_density": 8500},
    {"ward": "Sector 15", "lat": 28.9880, "lng": 77.0350, "infra_age_years": 18, "population_density": 7800},
    {"ward": "Model Town", "lat": 28.9985, "lng": 77.0225, "infra_age_years": 28, "population_density": 14200},
    {"ward": "Murthal Road", "lat": 29.0125, "lng": 77.0385, "infra_age_years": 10, "population_density": 6500},
    {"ward": "Kakroi Road", "lat": 28.9850, "lng": 77.0050, "infra_age_years": 22, "population_density": 11000},
    {"ward": "Subhash Chowk", "lat": 28.9910, "lng": 77.0280, "infra_age_years": 35, "population_density": 19500},
    {"ward": "Gohana Road", "lat": 29.0040, "lng": 76.9950, "infra_age_years": 16, "population_density": 9200},
    {"ward": "Rathdhana Road", "lat": 28.9720, "lng": 77.0420, "infra_age_years": 12, "population_density": 6800},
    {"ward": "Atlas Road", "lat": 28.9950, "lng": 77.0190, "infra_age_years": 30, "population_density": 16000},
    {"ward": "Bahalgarh Road", "lat": 28.9550, "lng": 77.0680, "infra_age_years": 8, "population_density": 5400},
    {"ward": "Sector 23", "lat": 28.9680, "lng": 77.0120, "infra_age_years": 6, "population_density": 4800},
    {"ward": "Old City Ward 4", "lat": 28.9900, "lng": 77.0260, "infra_age_years": 40, "population_density": 22000}
]

DEPARTMENTS_CATEGORIES = {
    "WATER": {
        "name": "Water Supply & Sewerage",
        "categories": [
            ("WATER_PIPE_LEAK", "Pipe Leakage / Burst", 24, [
                "Main water supply pipe burst overflowing onto street",
                "paani ki pipeline phat gayi h road par bohot paani beh raha h",
                "Severe pipeline leakage outside house water wasted",
                "gali me drinking water line leak ho gayi hai please fix"
            ]),
            ("WATER_CONTAMINATION", "Contaminated / Dirty Water", 12, [
                "Tap water is muddy and foul smelling unusable",
                "paani me mitti aur badbu aa rahi hai peene layak nahi h",
                "Black colored contaminated water supply in whole lane",
                "sewage mixing in drinking water pipeline urgent alert"
            ]),
            ("WATER_LOW_PRESSURE", "Low Water Pressure", 48, [
                "No water pressure on first floor since 3 days",
                "paani ka pressure bohot kam hai motor bhi nahi utha rahi",
                "Extremely low drinking water pressure in morning supply"
            ])
        ]
    },
    "ROADS": {
        "name": "Roads & Public Works (PWD)",
        "categories": [
            ("ROAD_POTHOLE", "Potholes & Surface Damage", 48, [
                "Deep pothole on main road causing bike accidents",
                "sadak par bohot bada gaddha ho gaya h girne ka darr h",
                "Multiple dangerous potholes after heavy rain near market",
                "road surface broken completely unpaved patches"
            ]),
            ("ROAD_CAVE_IN", "Road Cave-in / Sinkhole", 12, [
                "Road collapsed suddenly creating deep sinkhole",
                "sadak dhas gayi h gadi phasne ka khatra h",
                "Severe road cave-in over sewer line danger to vehicles"
            ])
        ]
    },
    "DRAINAGE": {
        "name": "Drainage & Stormwater",
        "categories": [
            ("DRAIN_BLOCKED", "Blocked Drain / Overflow", 12, [
                "Storm drain blocked with solid plastic waste overflowing",
                "naali block ho gayi h ganda paani gharo me ghus raha h",
                "Severe waterlogging at road crossing drain choked",
                "monsoon rain water accumulating blocked drainage channel"
            ])
        ]
    },
    "GARBAGE": {
        "name": "Solid Waste & Sanitation",
        "categories": [
            ("GARBAGE_DUMP", "Overflowing Garbage Dump", 8, [
                "Huge pile of uncollected garbage outside park rotting",
                "kachra petti overflow ho rahi h bohot badbu aa rahi h",
                "Garbage not picked up for 4 days stray dogs spreading it",
                "Illegal dumping of commercial waste on open plot"
            ]),
            ("DEAD_ANIMAL", "Dead Animal Removal", 4, [
                "Dead animal lying on road causing health hazard",
                "sadak par mara hua janwar pada h turant uthwaye",
                "Carcass lying near primary school urgent removal"
            ])
        ]
    },
    "STREETLIGHT": {
        "name": "Streetlighting & Illumination",
        "categories": [
            ("STREETLIGHT_DARK", "Dark Street / Broken Light", 24, [
                "Streetlight not functioning whole lane pitch dark at night",
                "gali ki light kharab h raat ko andhera rehta h safety issue",
                "3 consecutive street light poles blinking and dead",
                "broken bulb and hanging wire on pole"
            ])
        ]
    },
    "ELECTRICITY": {
        "name": "Electricity & Power Distribution",
        "categories": [
            ("ELECTRICITY_WIRE", "Exposed Live Wire / Sparking", 4, [
                "Live electrical wire snapped hanging on walkway dangerous",
                "bijli ki taar khuli padi h current aane ka darr h",
                "Transformer sparking heavily sparks falling on parked cars",
                "high voltage wire touching tree branches"
            ])
        ]
    },
    "TREES": {
        "name": "Horticulture & Urban Forestry",
        "categories": [
            ("TREE_FALLEN", "Fallen Tree / Broken Branch", 12, [
                "Large eucalyptus tree fell during storm blocking road",
                "ped gir gaya h rasta band ho gaya h",
                "Dangerous heavy branch hanging over power cables"
            ])
        ]
    },
    "ANIMAL": {
        "name": "Animal Welfare & Control",
        "categories": [
            ("ANIMAL_STRAY_CATTLE", "Stray Cattle Traffic Hazard", 8, [
                "Stray cattle sitting in middle of highway causing traffic jam",
                "awarah pashu sadak par ghum rahe h accident ka darr",
                "Aggressive stray bulls fighting in busy bazaar"
            ])
        ]
    }
}

def generate_dataset(num_records=50000, output_path="data/sonipat_civic_incidents_50k.csv"):
    print(f"🚀 Generating {num_records:,} high-fidelity synthetic Sonipat civic records...")
    
    start_date = datetime(2023, 1, 1)
    end_date = datetime(2026, 8, 25)
    total_days = (end_date - start_date).days

    records = []
    dept_keys = list(DEPARTMENTS_CATEGORIES.keys())

    # Pre-generate dates with seasonal weights
    for i in range(num_records):
        # Random timestamp
        random_day_offset = random.randint(0, total_days)
        incident_date = start_date + timedelta(
            days=random_day_offset,
            hours=random.randint(6, 22),
            minutes=random.randint(0, 59)
        )
        month = incident_date.month

        # Seasonal Weather Simulation for Sonipat, Haryana
        # Monsoon: July (7), Aug (8), Sept (9)
        # Summer: May (5), June (6)
        # Winter: Dec (12), Jan (1)
        if month in [7, 8, 9]: # Monsoon
            rainfall_mm = max(0.0, np.random.exponential(scale=28.0))
            temperature_c = random.uniform(28.0, 36.0)
            is_monsoon = 1
        elif month in [5, 6]: # Peak Summer
            rainfall_mm = max(0.0, np.random.exponential(scale=2.0))
            temperature_c = random.uniform(38.0, 47.0)
            is_monsoon = 0
        elif month in [12, 1]: # Winter
            rainfall_mm = max(0.0, np.random.exponential(scale=1.5))
            temperature_c = random.uniform(6.0, 18.0)
            is_monsoon = 0
        else:
            rainfall_mm = max(0.0, np.random.exponential(scale=4.0))
            temperature_c = random.uniform(20.0, 32.0)
            is_monsoon = 0

        # Department selection skewed by weather
        if rainfall_mm > 25.0: # Heavy rain boosts drainage, potholes, fallen trees
            dept_weights = [0.25, 0.30, 0.30, 0.05, 0.03, 0.04, 0.02, 0.01]
        elif temperature_c > 42.0: # Extreme heat boosts water issues & transformer sparks
            dept_weights = [0.40, 0.10, 0.05, 0.15, 0.05, 0.20, 0.02, 0.03]
        else: # Normal baseline
            dept_weights = [0.22, 0.20, 0.15, 0.18, 0.10, 0.07, 0.04, 0.04]

        dept_code = random.choices(dept_keys, weights=dept_weights, k=1)[0]
        dept_info = DEPARTMENTS_CATEGORIES[dept_code]
        category_choice = random.choice(dept_info["categories"])
        cat_code, cat_name, base_sla, text_samples = category_choice

        # Ward selection
        ward_info = random.choice(SONIPAT_WARDS)
        ward_name = ward_info["ward"]
        base_lat = ward_info["lat"]
        base_lng = ward_info["lng"]
        infra_age = ward_info["infra_age_years"]

        # Jitter coordinates slightly (+- 300m)
        lat = base_lat + random.uniform(-0.003, 0.003)
        lng = base_lng + random.uniform(-0.003, 0.003)

        # Text complaint description
        description = random.choice(text_samples)

        # Severity & Priority
        if dept_code in ["ELECTRICITY", "ROAD_CAVE_IN"] or "burst" in description.lower() or "collapsed" in description.lower():
            severity = random.choices(["Critical", "High", "Medium"], weights=[0.6, 0.3, 0.1])[0]
            priority = "Urgent" if severity == "Critical" else "High"
        elif rainfall_mm > 30.0:
            severity = random.choices(["High", "Medium", "Low"], weights=[0.5, 0.4, 0.1])[0]
            priority = "High"
        else:
            severity = random.choices(["High", "Medium", "Low"], weights=[0.15, 0.65, 0.20])[0]
            priority = "Medium" if severity == "Medium" else ("High" if severity == "High" else "Low")

        # Resolution Time Calculation (ground truth with physics & capacity features)
        # Higher infra age, rain, and severity increase resolution duration
        resolution_multiplier = 1.0
        if severity == "Critical": resolution_multiplier *= 0.6 # Dispatched faster
        elif severity == "Low": resolution_multiplier *= 1.4 # Backlogged
        
        if rainfall_mm > 15: resolution_multiplier *= 1.5 # Weather delay
        if infra_age > 25: resolution_multiplier *= 1.3 # Harder repairs

        base_res_hours = base_sla * random.uniform(0.4, 1.3) * resolution_multiplier
        resolution_hours = round(max(1.5, base_res_hours), 1)

        # SLA Breach
        is_sla_breached = 1 if resolution_hours > base_sla else 0

        # Citizen Satisfaction Rating (1 to 5)
        if is_sla_breached:
            rating = random.choices([1, 2, 3, 4], weights=[0.45, 0.35, 0.15, 0.05])[0]
        else:
            rating = random.choices([3, 4, 5], weights=[0.10, 0.40, 0.50])[0]

        # Anomaly Flag (1 = sudden surge, unusual incident spike, 0 = normal)
        is_anomaly = 1 if (rainfall_mm > 60.0 or (dept_code == "WATER" and infra_age > 30 and random.random() < 0.08)) else 0

        ref_id = f"CVX-{incident_date.strftime('%Y%m%d')}-{random.randint(1000, 9999)}"

        records.append({
            "incident_id": ref_id,
            "timestamp": incident_date.strftime("%Y-%m-%d %H:%M:%S"),
            "year": incident_date.year,
            "month": incident_date.month,
            "day_of_week": incident_date.weekday(),
            "hour": incident_date.hour,
            "department_code": dept_code,
            "department_name": dept_info["name"],
            "category_code": cat_code,
            "category_name": cat_name,
            "description": description,
            "ward": ward_name,
            "latitude": round(lat, 6),
            "longitude": round(lng, 6),
            "infra_age_years": infra_age,
            "population_density": ward_info["population_density"],
            "temperature_c": round(temperature_c, 1),
            "rainfall_mm": round(rainfall_mm, 1),
            "is_monsoon": is_monsoon,
            "severity": severity,
            "priority": priority,
            "sla_target_hours": base_sla,
            "actual_resolution_hours": resolution_hours,
            "is_sla_breached": is_sla_breached,
            "citizen_rating": rating,
            "is_anomaly": is_anomaly
        })

    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    df = pd.DataFrame(records)
    df.to_csv(output_path, index=False)
    print(f"✅ Successfully exported {len(df):,} records to {output_path} (File size: {os.path.getsize(output_path) / 1024 / 1024:.2f} MB)")
    return df

if __name__ == "__main__":
    generate_dataset(num_records=50000)
