# Industrial Telemetry Hub

[![CI](https://github.com/Cinkut/IndustrialTelemetryHub/actions/workflows/ci.yml/badge.svg)](https://github.com/Cinkut/IndustrialTelemetryHub/actions/workflows/ci.yml)

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

## Architektura

```
Telemetry.Domain          # encje: TelemetryReading, MachineStatus
Telemetry.Application      # logika, abstrakcje (repo, analizator AI), DTO
Telemetry.Infrastructure   # EF Core, repozytorium, analizatory (heurystyka + LLM)
Telemetry.Api              # REST API + SignalR hub + dashboard + ingestia MQTT
Telemetry.Simulator        # symulator maszyn publikujący telemetrię (MQTT)
Telemetry.Tests            # testy jednostkowe (xUnit)
```

Przepływ: **maszyny → MQTT (broker) → ingestia → PostgreSQL → REST API / SignalR → dashboard**,
z warstwą AI analizującą odczyty na żądanie.

## Status

✅ **Ukończony** — wszystkie zaplanowane funkcjonalności zrealizowane.

- [x] Integracja maszyn przez MQTT (symulator + broker Mosquitto)
- [x] Ingestia strumienia MQTT → PostgreSQL (EF Core, dane time-series)
- [x] REST API + architektura warstwowa (Repository + Service + DI)
- [x] Real-time: SignalR + live dashboard
- [x] Warstwa AI: detekcja anomalii + opis stanu (heurystyka + pluggable LLM)
- [x] Testy jednostkowe (xUnit)
- [x] Konteneryzacja (Docker) + CI (GitHub Actions)

## Endpointy

| Endpoint | Opis |
|---|---|
| `GET /` | live dashboard (kafelki maszyn aktualizowane przez SignalR) |
| `GET /api/machines` | lista maszyn z najnowszym stanem |
| `GET /api/machines/{id}/readings?limit=N` | historia odczytów |
| `GET /api/machines/{id}/analysis` | analiza stanu (anomalie + opis) |

## Uruchomienie

```bash
docker compose up -d --build    # broker MQTT + baza + API + symulator
# Dashboard:  http://localhost:8080
# Swagger:    http://localhost:8080/swagger
```

## Analiza AI (opcjonalny LLM)

Domyślnie analiza działa offline (heurystyka). Aby włączyć prawdziwy model językowy,
wystarczy podać klucz API w konfiguracji — bez zmian w kodzie:

```bash
# np. zmienna środowiskowa / sekcja Ai__ApiKey w docker-compose
Ai__ApiKey=sk-ant-...
```

Endpoint `/analysis` zwraca wtedy `"source": "llm"` zamiast `"heuristic"`.

## Testy

```bash
dotnet test
```
