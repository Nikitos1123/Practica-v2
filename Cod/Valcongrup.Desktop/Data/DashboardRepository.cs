using System.Data;
using MySql.Data.MySqlClient;

namespace Valcongrup.Data;

public class DashboardRepository
{
    public DashboardKpis GetKpis()
    {
        using var conn = DbConnection.GetConnection();
        using var cmd = new MySqlCommand(@"
SELECT COUNT(*) FROM proiecte WHERE status = 'Activ';
SELECT COUNT(*) FROM sarcini WHERE status IN ('În Lucru','In Lucru','In_Progres','În Progres');
SELECT COUNT(*) FROM utilizatori WHERE activ = 1;
SELECT COALESCE(SUM(buget_total),0) FROM proiecte;", conn);
        using var r = cmd.ExecuteReader();
        var kpis = new DashboardKpis();
        if (r.Read()) kpis.ActiveProjectsCount = r.GetInt32(0);
        if (r.NextResult() && r.Read()) kpis.PendingTasksCount = r.GetInt32(0);
        if (r.NextResult() && r.Read()) kpis.TeamMembersCount = r.GetInt32(0);
        if (r.NextResult() && r.Read()) kpis.TotalUtilizedBudget = r.GetDecimal(0);
        return kpis;
    }

    public int CountProiecteActive() => GetKpis().ActiveProjectsCount;
    public int CountSarciniInProgres() => GetKpis().PendingTasksCount;
    public decimal BugetUtilizat() => GetKpis().TotalUtilizedBudget;
    public int CountMembri() => GetKpis().TeamMembersCount;

    public DataTable ActiveProjects() => RepositoryHelpers.Fill(@"SELECT p.nume AS Proiect, COALESCE(c.nume,'') AS Client, COALESCE(CONCAT(u.prenume,' ',u.nume),'') AS Manager, p.progres AS Progres, p.status AS Status, p.data_termen AS Termen FROM proiecte p LEFT JOIN clienti c ON p.id_client=c.id LEFT JOIN utilizatori u ON p.id_manager=u.id WHERE p.status IN ('Activ','Planificat') ORDER BY p.data_termen");
}

public sealed class DashboardKpis
{
    public int ActiveProjectsCount { get; set; }
    public int PendingTasksCount { get; set; }
    public int TeamMembersCount { get; set; }
    public decimal TotalUtilizedBudget { get; set; }
    public string TotalUtilizedBudgetMdl => $"{TotalUtilizedBudget:N0} MDL";
}
