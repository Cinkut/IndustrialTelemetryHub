namespace Telemetry.Application.Models;

/// <summary>Wynik analizy stanu maszyny (heurystyka lub LLM).</summary>
public record MachineAnalysis(
    string MachineId,
    string Severity,   // Normal | Warning | Critical
    string Summary,    // opis w języku naturalnym
    string Source);    // "heuristic" lub "llm"
