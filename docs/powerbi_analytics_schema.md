# 📊 CivicFix Power BI & Executive Analytics Integration

This document defines the schema, data model, and DAX measures for integrating **CivicFix** with **Microsoft Power BI**, **Tableau**, and municipal data warehouses.

---

## 1. Streaming Dataset Endpoints

| Endpoint | Method | Format | Description |
|---|---|---|---|
| `/api/analytics/kpis` | `GET` | JSON | Live platform KPIs (Total, In Progress, SLA %, Citizen Rating) |
| `/api/analytics/export/csv` | `GET` | CSV | High-performance full incident dump for DirectQuery or Scheduled Refresh |
| `/api/analytics/ward-performance` | `GET` | JSON | Ward-by-ward aggregated performance and resolution rate |

---

## 2. Power BI Tabular Schema

```text
Table: Incidents
├── ReferenceNumber (String, PK)
├── Title (String)
├── Department (String, FK -> Dim_Department)
├── Category (String, FK -> Dim_Category)
├── Status (String)
├── Severity (String)
├── Priority (String)
├── WardSector (String, FK -> Dim_Ward)
├── Latitude (Decimal)
├── Longitude (Decimal)
├── ReportedAtUtc (DateTime)
├── TargetSlaUtc (DateTime)
├── ResolvedAtUtc (DateTime, Nullable)
├── CitizenRating (Integer, 1-5)
└── IsSensitive (Boolean)
```

---

## 3. Key DAX Measures for Municipal Dashboards

### SLA Compliance Rate
```dax
SLA Compliance % = 
VAR TotalIncidents = COUNTROWS(Incidents)
VAR SlaBreaches = CALCULATE(COUNTROWS(Incidents), Incidents[ResolvedAtUtc] > Incidents[TargetSlaUtc] || (ISBLANK(Incidents[ResolvedAtUtc]) && Incidents[TargetSlaUtc] < NOW()))
RETURN
DIVIDE(TotalIncidents - SlaBreaches, TotalIncidents, 1.0) * 100
```

### Average Resolution Time (Hours)
```dax
Avg Resolution Hours = 
AVERAGEX(
    FILTER(Incidents, NOT(ISBLANK(Incidents[ResolvedAtUtc]))),
    DATEDIFF(Incidents[ReportedAtUtc], Incidents[ResolvedAtUtc], HOUR)
)
```

### Citizen Net Promoter / Trust Score
```dax
Citizen Satisfaction Rating = 
AVERAGE(Incidents[CitizenRating])
```
