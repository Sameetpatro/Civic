# 🏛️ CivicFix — AI-Powered Civic Intelligence & Public Welfare Platform

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.0-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.3-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-0.110-009688?logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![Leaflet](https://img.shields.io/badge/Leaflet-Geospatial-199900?logo=leaflet&logoColor=white)](https://leafletjs.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**CivicFix** is a regional public-welfare platform and civic intelligence engine that directly bridges citizens, municipal department officers, and field workers. Tailored for **Sonipat, Haryana, India** with an expandable modular architecture, CivicFix turns fragmented citizen complaints into structured, auditable, and predictive public infrastructure management.

---

## 🌟 Key Capabilities

### 1. 🔄 Closed-Loop Citizen Redressal State Machine
Every municipal incident traverses an auditable, timestamped lifecycle:
$$\text{Reported} \longrightarrow \text{Auto-Department Routed} \longrightarrow \text{Officer Review} \longrightarrow \text{Worker Dispatched} \longrightarrow \text{In Progress} \longrightarrow \text{Resolved (Photo Evidence)} \longrightarrow \text{Citizen 5-Star Verification} \longrightarrow \text{Closed}$$

* **Immutable Audit Trail**: Every status transition, remark, and reassignment is recorded in `IssueStatusHistory`.
* **Citizen Power**: Tickets can only be marked `Closed` after the reporting citizen verifies resolution or provides feedback.

### 2. 🗺️ Geospatial Command Center (Sonipat Focus)
* Interactive OpenStreetMap & Leaflet command center centered on **Sonipat, Haryana (`28.9931° N, 77.0151° E`)**.
* Live color-coded marker pins for 11 municipal departments (Water, Roads, Sanitation, Drainage, Streetlights, etc.) with pulsing rings on critical hazard zones.
* Fast geospatial Haversine radius search to identify nearby duplicate complaints.

### 3. 🤖 AI Multi-Lingual NLU & Civic Intelligence
* **Multi-Lingual Complaint Understanding**: Analyzes complaints in English, Hindi, and Hinglish transliteration (*e.g.*, *"paani ki line tuti hai sector 14 me"* $\to$ Category: `Water Supply`, Priority: `High`).
* **SLA Calculation**: Computes target resolution deadlines dynamically based on category severity policies.
* **Predictive ML Models**: Forecasts ward-level incident volume, identifies infrastructure failure anomalies, and estimates resolution times.

### 4. 👷 Field Workforce Capacity & Dispatch Engine
* Prevents worker burnout by tracking live active jobs against maximum capacities.
* Officer dispatch console displays candidate workers ranked by ward zone proximity and current workload.

---

## 🏗️ Architecture Overview

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                   CLIENT APPLICATIONS                                  │
├─────────────────────────────────────────┬──────────────────────────────────────────────┤
│  React 19 + TypeScript Web Portal        │  React Native / Expo Mobile App              │
│  • Operations Command Center & KPI cards │  • Citizen Camera & GPS incident reporting   │
│  • Geospatial Leaflet Map (Sonipat)     │  • Field Worker accept / on-site execution   │
│  • 1-Click Role Persona Switcher        │  • Photographic proof & resolution upload    │
└────────────────────────────────────┬────┴──────────────────────────────────────────────┘
                                     │ HTTP / REST + Bearer JWT
                                     ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        ASP.NET CORE 10 CLEAN ARCHITECTURE API                          │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  src/CivicFix.Api            • REST Controllers, JWT Auth Handler, Swagger, CORS       │
│  src/CivicFix.Application    • Use Cases, State Machine, DTOs, FluentValidation        │
│  src/CivicFix.Domain         • Entities (Issue, Department, WorkerProfile, Audit)      │
│  src/CivicFix.Infrastructure • EF Core DbContext, Fluent Mappings, DB Initializer      │
└──────────────────┬───────────────────────────────────────────────────┬─────────────────┘
                   │                                                   │
                   ▼                                                   ▼
┌──────────────────────────────────────┐            ┌────────────────────────────────────┐
│      RELATIONAL DATA ACCESS          │            │       PYTHON ML ENGINE             │
│  • Microsoft SQL Server / SQLite     │            │  • Multi-Lingual NLU Service       │
│  • Dual-provider support (EF Core)   │            │  • Time-Series Volume Forecaster   │
│  • Spatial indexes & audit logging   │            │  • Anomaly Burst Detector          │
└──────────────────────────────────────┘            └────────────────────────────────────┘
```

---

## 🏛️ Pre-Seeded Sonipat Municipal Departments

The system seeds **11 Sonipat Municipal Departments** and **20+ standardized categories**:

| Code | Department Name | Key Categories | Default SLA |
|---|---|---|---|
| `WATER` | Water Supply & Sewerage | Pipe Burst, Contaminated Water, Low Pressure | 24 Hours |
| `ROADS` | Roads & Public Works (PWD) | Potholes, Road Cave-in, Broken Divider | 48 Hours |
| `DRAINAGE` | Drainage & Stormwater | Blocked Drain, Monsoon Waterlogging | 12 Hours |
| `GARBAGE` | Solid Waste & Sanitation | Overflowing Garbage Dump, Dead Animal, Unswept Street | 8 Hours |
| `STREETLIGHT` | Streetlighting & Illumination | Dark Street, Broken Pole, Flickering Lamp | 24 Hours |
| `ELECTRICITY` | Electricity & Power Distribution | Exposed Live Wire, Transformer Sparking | 4 Hours (Urgent) |
| `TREES` | Horticulture & Urban Forestry | Fallen Tree Blocking Road, Overhanging Branches | 12 Hours |
| `PUBLIC_PROP` | Public Infrastructure & Assets | Broken Park Bench, Damaged Bus Stop Shelter | 72 Hours |
| `ANIMAL` | Animal Welfare & Control | Stray Cattle Hazard, Rabid Animal Alert | 8 Hours |
| `CONSTRUCTION`| Construction Safety & Encroachment | Illegal Footpath Encroachment, Debris Dumping | 48 Hours |
| `OTHER` | General Grievance Cell | Uncategorized Civic Problems | 48 Hours |

---

## 👥 Demo Personas (Instant 1-Click Login)

The web portal includes an instant **Role Persona Switcher** in the top navigation bar with pre-configured accounts:

| Role | Persona Name | Email | Password |
|---|---|---|---|
| 👑 **Admin** | Sonipat Municipal Admin | `admin@sonipat.civicfix.gov.in` | `Password123!` |
| 💧 **Officer** | Er. Rajesh Malik (Water Dept) | `water.officer@sonipat.civicfix.gov.in` | `Password123!` |
| 🚧 **Officer** | Er. Sunil Hooda (Roads PWD) | `roads.officer@sonipat.civicfix.gov.in` | `Password123!` |
| 🧹 **Officer** | Dr. Manju Sharma (Sanitation) | `sanitation.officer@sonipat.civicfix.gov.in` | `Password123!` |
| 🔧 **Worker** | Ramesh Kumar (Water Specialist) | `ramesh.kumar@worker.civicfix.gov.in` | `Password123!` |
| 🙋‍♂️ **Citizen** | Vikram Singh (Sector 14 Resident)| `vikram.singh@gmail.com` | `Password123!` |

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET SDK 10.0+](https://dotnet.microsoft.com/)
- [Node.js 20+ & npm](https://nodejs.org/)
- [Python 3.10+](https://www.python.org/)

---

### 1. Run the .NET 10 Backend API

```bash
# Clone the repository
git clone https://github.com/Sameetpatro/Civic.git
cd Civic

# Run backend API (automatically seeds database on first start)
dotnet run --project src/CivicFix.Api/CivicFix.Api.csproj
```
- 🌐 **Swagger Interactive API Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

### 2. Run the React Web Command Portal

```bash
cd frontend/civic-web
npm install
npm run dev
```
- 🌐 **Web Operations Command Portal**: [http://localhost:5173](http://localhost:5173)

---

### Option A: Run Everything via Docker Compose
```bash
docker compose up --build
```
- 🌐 **Web Portal**: [http://localhost:5173](http://localhost:5173)
- 🌐 **Backend API Swagger**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- 🌐 **ML Microservice Docs**: [http://localhost:8000/docs](http://localhost:8000/docs)

---

### Option B: Run Services Locally

#### 1. Backend API (.NET 10)
```bash
dotnet run --project src/CivicFix.Api/CivicFix.Api.csproj
```
- 🌐 **Swagger Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

#### 2. React Web Command Portal
```bash
cd frontend/civic-web
npm install
npm run dev
```
- 🌐 **Web Operations Command Portal**: [http://localhost:5173](http://localhost:5173)

#### 3. Mobile App (React Native / Expo)
```bash
cd mobile/civic-app
npm install
npx expo start
```

#### 4. Python ML & Civic Intelligence Engine
```bash
cd ml-engine
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python train_models.py
python app.py
```
- 🌐 **ML Microservice API**: [http://localhost:8000/docs](http://localhost:8000/docs)

---

## 🧪 Automated Testing

### Backend Unit & Integration Tests (.NET 10)
```bash
dotnet test
```
```text
Passed!  - Failed: 0, Passed: 15, Skipped: 0, Total: 15, Duration: 1 s - CivicFix.Tests.dll (net10.0)
```

### Python ML Engine Unit Tests
```bash
PYTHONPATH=ml-engine python -m unittest ml-engine/test_ml_service.py
```
```text
Ran 8 tests in 0.84s - OK
```

### React Web Production Bundle
```bash
cd frontend/civic-web && npm run build
```
```text
✓ built in 121ms
```

---

## 📄 License
This project is licensed under the [MIT License](LICENSE).
