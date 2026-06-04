# Industrial Telemetry Hub

System do zbierania i analizy telemetrii z maszyn przemysłowych (Industry IoT).
Maszyny publikują dane (temperatura, obroty, stany) przez **MQTT**; serwis .NET
zbiera je, zapisuje, udostępnia przez REST API oraz live dashboard, a warstwa AI
wykrywa anomalie i opisuje stan maszyn językiem naturalnym.

Projekt portfolio demonstrujący integrację maszyn (Industry IoT / MQTT),
przetwarzanie strumieniowe, komunikację real-time i praktyczne użycie AI.

## Stack technologiczny

- **.NET 10** / C#
- **MQTT** (MQTTnet) — integracja maszyn / Industry IoT
- **ASP.NET Core Web API** (REST + OpenAPI/Swagger)
- **SignalR** — przesyłanie danych w czasie rzeczywistym do dashboardu
- **Entity Framework Core** + **PostgreSQL** (dane time-series)
- **Warstwa AI** — detekcja anomalii + opis stanu maszyny (pluggable LLM)
- **Docker** / docker-compose (broker Mosquitto + baza + serwisy)
- **GitHub Actions** (CI: build + testy)

## Architektura (docelowa)

```
Telemetry.Domain          # encje: Machine, TelemetryReading
Telemetry.Application      # logika, abstrakcje, analiza AI
Telemetry.Infrastructure   # EF Core, klient MQTT, dostęp do danych
Telemetry.Api              # REST API + SignalR hub + dashboard
Telemetry.Simulator        # symulator maszyn publikujący telemetrię (MQTT)
Telemetry.Tests            # testy jednostkowe
```

## Status

🚧 W budowie. Plan rozwoju:

- [x] Etap 0 — repo + solucja
- [ ] Etap 1 — model domenowy + broker MQTT + symulator maszyn
- [ ] Etap 2 — worker: zbieranie telemetrii z MQTT → zapis do bazy
- [ ] Etap 3 — REST API + architektura warstwowa
- [ ] Etap 4 — real-time (SignalR + live dashboard)
- [ ] Etap 5 — warstwa AI (anomalie + opis stanu)
- [ ] Etap 6 — testy + docker-compose + CI

## Uruchomienie (docelowo)

```bash
docker compose up -d --build    # broker MQTT + baza + API + symulator
# Dashboard / Swagger: http://localhost:8080
```
