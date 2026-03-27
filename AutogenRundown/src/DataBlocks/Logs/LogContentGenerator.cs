using AutogenRundown.DataBlocks.Terminals;

namespace AutogenRundown.DataBlocks.Logs;

/// <summary>
/// Generates procedural log content for terminal puzzle challenges.
/// All generation is seeded via Generator.* for deterministic results.
/// </summary>
public static class LogContentGenerator
{
    #region Name Data

    private static readonly string[] FirstNames =
    {
        "Anders", "Angela", "Boris", "Chen", "Dauda", "Edgar", "Ellis",
        "Frank", "Hamid", "Igor", "James", "Karim", "Lin", "Marcus",
        "Nadia", "Oscar", "Pavel", "Rachel", "Stefan", "Tanya",
        "Viktor", "Yuki", "Zara", "Anton", "Clara", "Dmitri",
        "Eva", "Gunnar", "Hana", "Ivan", "Klaus", "Lena", "Mikhail",
        "Nina", "Oleg", "Rosa", "Sven", "Ursula", "Wendy", "Xander"
    };

    private static readonly string[] LastNames =
    {
        "Johanson", "Klein", "Stanovich", "Piros", "Clinton",
        "Soweta", "Carnegie", "Bishop", "Richtofer", "Stokes",
        "Chapman", "Lefavre", "Rostok", "Durant", "Davies",
        "Schaeffer", "Molina", "Nesbitt", "Lockwood", "Connor",
        "Brennan", "Eriksson", "Gruber", "Holt", "Ivanov",
        "Krueger", "Lang", "Mendez", "Novak", "Olsen",
        "Petrov", "Quinn", "Reyes", "Schultz", "Torres",
        "Vogt", "Walsh", "Yamada", "Zhao", "Berg"
    };

    private static readonly string[] Departments =
    {
        "Containment", "Engineering", "Security", "Research",
        "Maintenance", "Logistics", "Medical", "Operations",
        "Analytics", "Extraction", "Monitoring", "Transport"
    };

    private static readonly string[] Roles =
    {
        "Supervisor", "Technician", "Analyst", "Operator",
        "Lead", "Specialist", "Engineer", "Coordinator"
    };

    private static readonly string[] Sectors =
    {
        "SECTOR-1A", "SECTOR-2B", "SECTOR-3C", "SECTOR-4D",
        "SECTOR-5E", "SECTOR-6F", "SECTOR-7G", "SECTOR-8H",
        "SECTOR-9J", "SECTOR-10K", "SUBLEVEL-A", "SUBLEVEL-B",
        "SUBLEVEL-C", "WING-NORTH", "WING-SOUTH", "WING-EAST"
    };

    private static readonly string[] ShiftDates =
    {
        "2054-01-12", "2054-01-15", "2054-02-03", "2054-02-18",
        "2054-03-07", "2054-03-21", "2054-04-02", "2054-04-14",
        "2054-05-09", "2054-05-23", "2054-06-11", "2054-06-28"
    };

    private static readonly string[] IncidentTypes =
    {
        "Containment Breach", "Power Grid Failure", "Ventilation Malfunction",
        "Unauthorized Access", "Biological Contamination", "Structural Damage",
        "Communication Blackout", "Pressure Seal Failure", "Coolant System Leak",
        "Security Lockout", "Equipment Malfunction", "Atmospheric Anomaly"
    };

    private static readonly string[] IncidentSeverities =
    {
        "CRITICAL", "HIGH", "MODERATE", "ELEVATED"
    };

    #endregion

    #region Personnel Dossier Generation

    /// <summary>
    /// Generates a set of personnel for cross-reference challenges.
    /// Returns the correct employee and a list of red herrings.
    /// </summary>
    public static (PersonnelRecord correct, List<PersonnelRecord> allRecords) GeneratePersonnel(
        int totalCount,
        int redHerringCount)
    {
        var usedNames = new HashSet<string>();
        var records = new List<PersonnelRecord>();

        for (var i = 0; i < totalCount; i++)
        {
            string firstName, lastName, fullName;
            do
            {
                firstName = Generator.Pick(FirstNames)!;
                lastName = Generator.Pick(LastNames)!;
                fullName = $"{firstName} {lastName}";
            } while (usedNames.Contains(fullName));

            usedNames.Add(fullName);

            records.Add(new PersonnelRecord
            {
                FirstName = firstName,
                LastName = lastName,
                EmployeeId = $"EMP-{Generator.Between(1000, 9999)}",
                Department = Generator.Pick(Departments)!,
                Role = Generator.Pick(Roles)!,
                AssignedSector = Generator.Pick(Sectors)!,
                ShiftDate = Generator.Pick(ShiftDates)!,
            });
        }

        // Pick the correct answer
        var correctIndex = Generator.Between(0, records.Count - 1);
        var correct = records[correctIndex];

        return (correct, records);
    }

    /// <summary>
    /// Generates a duty roster log containing names and employee IDs.
    /// </summary>
    public static LogFile GenerateDutyRoster(List<PersonnelRecord> records)
    {
        var lines = new List<string>
        {
            "DUTY ROSTER - ACTIVE PERSONNEL",
            "Classification: INTERNAL",
            "---",
            "",
            "NAME                  ID",
            "---                   ---"
        };

        // Shuffle records for display
        var shuffled = records.OrderBy(_ => Generator.Between(0, 1000)).ToList();

        foreach (var r in shuffled)
        {
            var name = $"{r.LastName}, {r.FirstName}";
            lines.Add($"{name,-22}{r.EmployeeId}");
        }

        return new LogFile
        {
            FileName = "duty_roster.log",
            FileContent = new Text(string.Join("\n", lines)),
        };
    }

    /// <summary>
    /// Generates a shift assignment log containing names, dates, and sectors.
    /// </summary>
    public static LogFile GenerateShiftAssignments(List<PersonnelRecord> records)
    {
        var lines = new List<string>
        {
            "SHIFT ASSIGNMENTS",
            "Classification: INTERNAL",
            "---",
            "",
        };

        var shuffled = records.OrderBy(_ => Generator.Between(0, 1000)).ToList();

        foreach (var r in shuffled)
        {
            lines.Add($"{r.LastName}, {r.FirstName}");
            lines.Add($"  Shift: {r.ShiftDate}");
            lines.Add($"  Sector: {r.AssignedSector}");
            lines.Add($"  Dept: {r.Department}");
            lines.Add("");
        }

        return new LogFile
        {
            FileName = "shift_assignments.log",
            FileContent = new Text(string.Join("\n", lines)),
        };
    }

    /// <summary>
    /// Generates an access log mentioning personnel names with their roles.
    /// </summary>
    public static LogFile GenerateAccessLog(List<PersonnelRecord> records)
    {
        var lines = new List<string>
        {
            "ACCESS LOG - SECURITY CHECKPOINT",
            "Classification: RESTRICTED",
            "---",
            ""
        };

        var shuffled = records.OrderBy(_ => Generator.Between(0, 1000)).ToList();

        foreach (var r in shuffled)
        {
            var time = $"{Generator.Between(0, 23):D2}:{Generator.Between(0, 59):D2}";
            lines.Add($"[{r.ShiftDate} {time}]");
            lines.Add($"  {r.FirstName} {r.LastName}");
            lines.Add($"  Role: {r.Department} {r.Role}");
            lines.Add($"  Access: {r.AssignedSector}");
            lines.Add("");
        }

        return new LogFile
        {
            FileName = "access_log.log",
            FileContent = new Text(string.Join("\n", lines)),
        };
    }

    /// <summary>
    /// Generates the password hint text for a cross-reference dossier.
    /// </summary>
    public static string GenerateDossierHint(PersonnelRecord correct)
    {
        return $"Password Required.\n\nEnter the EMPLOYEE ID of the\n{correct.Department} {correct.Role}\nassigned to {correct.AssignedSector}\non shift {correct.ShiftDate}.";
    }

    #endregion

    #region Incident Report Generation

    /// <summary>
    /// Generates a set of incidents for forensic reconstruction.
    /// Returns the correct incident and a list of all incidents including red herrings.
    /// </summary>
    public static (IncidentRecord correct, List<IncidentRecord> allIncidents) GenerateIncidents(
        int totalCount)
    {
        var incidents = new List<IncidentRecord>();

        for (var i = 0; i < totalCount; i++)
        {
            incidents.Add(new IncidentRecord
            {
                Sector = Generator.Pick(Sectors)!,
                Timestamp = $"{Generator.Pick(ShiftDates)!} {Generator.Between(0, 23):D2}:{Generator.Between(0, 59):D2}",
                Subject = $"{Generator.Pick(FirstNames)!} {Generator.Pick(LastNames)!}",
                Type = Generator.Pick(IncidentTypes)!,
                Severity = Generator.Pick(IncidentSeverities)!,
                CaseNumber = $"INC-{Generator.Between(10000, 99999)}",
            });
        }

        var correctIndex = Generator.Between(0, incidents.Count - 1);
        return (incidents[correctIndex], incidents);
    }

    /// <summary>
    /// Generates a partial incident report log file covering a subset of incidents.
    /// </summary>
    public static LogFile GenerateIncidentReport(
        List<IncidentRecord> incidents,
        int startIndex,
        int count,
        int fileIndex,
        bool corrupted = false)
    {
        var lines = new List<string>
        {
            $"INCIDENT REPORT - BATCH {fileIndex + 1}",
            "Classification: RESTRICTED",
            "---",
            ""
        };

        var subset = incidents
            .Skip(startIndex)
            .Take(count)
            .OrderBy(_ => Generator.Between(0, 1000))
            .ToList();

        foreach (var inc in subset)
        {
            lines.Add($"Case: {inc.CaseNumber}");
            lines.Add($"  Severity: {inc.Severity}");

            if (corrupted && Generator.Flip(0.3))
            {
                lines.Add($"  Sector: [DATA CORRUPTED]");
            }
            else
            {
                lines.Add($"  Sector: {inc.Sector}");
            }

            if (corrupted && Generator.Flip(0.3))
            {
                lines.Add($"  Time: {inc.Timestamp.Substring(0, 10)} ??:??");
            }
            else
            {
                lines.Add($"  Time: {inc.Timestamp}");
            }

            lines.Add($"  Type: {inc.Type}");

            if (corrupted && Generator.Flip(0.2))
            {
                lines.Add($"  Subject: [REDACTED]");
            }
            else
            {
                lines.Add($"  Subject: {inc.Subject}");
            }

            lines.Add("");
        }

        return new LogFile
        {
            FileName = $"incident_report_{fileIndex + 1:D2}.log",
            FileContent = new Text(string.Join("\n", lines)),
        };
    }

    /// <summary>
    /// Generates the password hint for forensic reconstruction.
    /// </summary>
    public static string GenerateForensicHint(IncidentRecord correct)
    {
        return $"Password Required.\n\nReconstruct the incident sequence.\nEnter the CASE NUMBER for the\n{correct.Severity} {correct.Type}\nin {correct.Sector}.";
    }

    #endregion
}

public record PersonnelRecord
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string EmployeeId { get; set; } = "";
    public string Department { get; set; } = "";
    public string Role { get; set; } = "";
    public string AssignedSector { get; set; } = "";
    public string ShiftDate { get; set; } = "";
}

public record IncidentRecord
{
    public string Sector { get; set; } = "";
    public string Timestamp { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Type { get; set; } = "";
    public string Severity { get; set; } = "";
    public string CaseNumber { get; set; } = "";
}
