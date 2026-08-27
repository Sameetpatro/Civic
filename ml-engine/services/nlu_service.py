import re
from typing import Dict, Any, List

# Multi-lingual keyword maps for Sonipat civic terminology (English, Hindi, Hinglish)
DEPARTMENT_RULES = {
    "WATER": {
        "keywords": [
            "paani", "pani", "water", "pipe", "pipeline", "leak", "leakage", "burst", 
            "pressure", "supply", "tap", "nal", "sewer", "sewerage", "dirty water", 
            "ganda paani", "muddy", "contamination", "sewage mixing"
        ],
        "department_code": "WATER",
        "department_name": "Water Supply & Sewerage",
        "default_category": "WATER_PIPE_LEAK",
        "default_category_name": "Pipe Leakage / Burst",
        "sla_hours": 24
    },
    "ROADS": {
        "keywords": [
            "sadak", "road", "pothole", "gaddha", "gadha", "sinkhole", "dhas", "cave", 
            "divider", "footpath", "tar", "unpaved", "accident", "bike slip", "pavement"
        ],
        "department_code": "ROADS",
        "department_name": "Roads & Public Works (PWD)",
        "default_category": "ROAD_POTHOLE",
        "default_category_name": "Potholes & Surface Damage",
        "sla_hours": 48
    },
    "DRAINAGE": {
        "keywords": [
            "naali", "nali", "drain", "drainage", "stormwater", "overflow", "waterlogging", 
            "choked", "block", "blocked", "rain water", "barish ka paani"
        ],
        "department_code": "DRAINAGE",
        "department_name": "Drainage & Stormwater",
        "default_category": "DRAIN_BLOCKED",
        "default_category_name": "Blocked Drain / Overflow",
        "sla_hours": 12
    },
    "GARBAGE": {
        "keywords": [
            "kachra", "kachda", "garbage", "trash", "waste", "dump", "dustbin", "pile", 
            "smell", "badbu", "dead animal", "mara hua", "carcass", "rotten", "safai", "sweeper"
        ],
        "department_code": "GARBAGE",
        "department_name": "Solid Waste & Sanitation",
        "default_category": "GARBAGE_DUMP",
        "default_category_name": "Overflowing Garbage Dump",
        "sla_hours": 8
    },
    "STREETLIGHT": {
        "keywords": [
            "light", "streetlight", "street light", "andhera", "dark", "pole", "khamba", 
            "blinking", "bulb", "flickering", "darkness", "batti"
        ],
        "department_code": "STREETLIGHT",
        "department_name": "Streetlighting & Illumination",
        "default_category": "STREETLIGHT_DARK",
        "default_category_name": "Dark Street / Broken Light",
        "sla_hours": 24
    },
    "ELECTRICITY": {
        "keywords": [
            "bijli", "electricity", "wire", "taar", "spark", "sparking", "transformer", 
            "current", "shock", "live wire", "open wire", "high voltage", "power cut", "short circuit"
        ],
        "department_code": "ELECTRICITY",
        "department_name": "Electricity & Power Distribution",
        "default_category": "ELECTRICITY_WIRE",
        "default_category_name": "Exposed Live Wire / Sparking",
        "sla_hours": 4
    },
    "TREES": {
        "keywords": [
            "tree", "ped", "branch", "daali", "fallen", "gir gaya", "horticulture", "leaves", "wood"
        ],
        "department_code": "TREES",
        "department_name": "Horticulture & Urban Forestry",
        "default_category": "TREE_FALLEN",
        "default_category_name": "Fallen Tree / Broken Branch",
        "sla_hours": 12
    },
    "ANIMAL": {
        "keywords": [
            "animal", "stray", "cattle", "cow", "bull", "pashu", "gay", "saand", "dog", "kutta", "dog bite"
        ],
        "department_code": "ANIMAL",
        "department_name": "Animal Welfare & Control",
        "default_category": "ANIMAL_STRAY_CATTLE",
        "default_category_name": "Stray Cattle Traffic Hazard",
        "sla_hours": 8
    }
}

CRITICAL_KEYWORDS = [
    "burst", "sparking", "current", "shock", "sinkhole", "collapsed", "urgent", "danger", 
    "khatra", "emergency", "fire", "smoke", "live wire", "sewage mixing", "accident"
]

HIGH_KEYWORDS = [
    "overflowing", "bad smell", "badbu", "pitch dark", "andhera", "choked", "blocked", "heavy leak"
]

class CivicNluEngine:
    def __init__(self):
        pass

    def analyze_complaint(self, text: str) -> Dict[str, Any]:
        """Analyzes multi-lingual complaint text and returns department, category, severity, and entities."""
        lower_text = text.lower()
        matched_scores: Dict[str, int] = {}
        matched_keywords: Dict[str, List[str]] = {}

        for dept_code, rule in DEPARTMENT_RULES.items():
            count = 0
            found = []
            for kw in rule["keywords"]:
                pattern = r'\b' + re.escape(kw) + r'\b'
                if re.search(pattern, lower_text):
                    count += 1
                    found.append(kw)
            if count > 0:
                matched_scores[dept_code] = count
                matched_keywords[dept_code] = found

        if matched_scores:
            best_dept = max(matched_scores, key=matched_scores.get)
            rule = DEPARTMENT_RULES[best_dept]
            confidence = min(0.98, 0.65 + (matched_scores[best_dept] * 0.1))
        else:
            best_dept = "OTHER"
            rule = {
                "department_code": "OTHER",
                "department_name": "General Grievance Cell",
                "default_category": "OTHER_CIVIC",
                "default_category_name": "General Civic Grievance",
                "sla_hours": 48
            }
            confidence = 0.50

        # Determine Severity & Priority
        severity = "Medium"
        priority = "Medium"
        is_critical = any(kw in lower_text for kw in CRITICAL_KEYWORDS)
        is_high = any(kw in lower_text for kw in HIGH_KEYWORDS)

        if is_critical or best_dept == "ELECTRICITY":
            severity = "Critical"
            priority = "Urgent"
        elif is_high or best_dept in ["DRAINAGE", "GARBAGE"]:
            severity = "High"
            priority = "High"

        # Extract Ward if mentioned
        detected_ward = None
        for ward in ["Sector 14", "Sector 15", "Model Town", "Murthal Road", "Kakroi Road", "Subhash Chowk", "Gohana Road", "Atlas Road"]:
            if ward.lower() in lower_text:
                detected_ward = ward
                break

        return {
            "department_code": rule["department_code"],
            "department_name": rule["department_name"],
            "category_code": rule["default_category"],
            "category_name": rule["default_category_name"],
            "sla_target_hours": rule["sla_hours"],
            "confidence_score": round(confidence, 2),
            "severity": severity,
            "priority": priority,
            "detected_ward": detected_ward,
            "matched_keywords": matched_keywords.get(best_dept, []),
            "sentiment": "Negative" if is_critical else "Neutral-Negative",
            "ai_summary": f"Categorized under {rule['department_name']} ({severity} severity) based on keywords: {', '.join(matched_keywords.get(best_dept, [])) or 'general pattern'}."
        }

nlu_engine = CivicNluEngine()
