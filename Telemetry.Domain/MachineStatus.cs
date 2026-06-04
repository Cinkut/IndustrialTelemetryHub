namespace Telemetry.Domain;

/// <summary>Stan pracy maszyny raportowany w telemetrii.</summary>
public enum MachineStatus
{
    Idle = 0,
    Running = 1,
    Warning = 2,
    Fault = 3
}
