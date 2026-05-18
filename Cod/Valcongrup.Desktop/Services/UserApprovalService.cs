using System.Data;
using MySql.Data.MySqlClient;
using Valcongrup.Data;

namespace Valcongrup.Services;

public class UserApprovalService
{
    public DataTable GetPendingUsers()
    {
        using var conn = DbConnection.GetConnection();
        using var cmd = new MySqlCommand(@"SELECT u.id AS user_id, CONCAT(u.nume, ' ', u.prenume) AS Nume, u.email AS Email, COALESCE(u.telefon,'') AS Telefon, COALESCE(r.nume,'') AS Rol
FROM utilizatori u
LEFT JOIN roluri r ON r.id = u.id_rol
WHERE u.is_approved = 0
ORDER BY u.creat_la DESC, u.nume, u.prenume", conn);
        using var adapter = new MySqlDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public bool ApproveUser(int userId)
    {
        using var conn = DbConnection.GetConnection();
        using var cmd = new MySqlCommand("UPDATE utilizatori SET is_approved = 1, activ = 1 WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", userId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool RejectUser(int userId)
    {
        using var conn = DbConnection.GetConnection();
        using var cmd = new MySqlCommand("UPDATE utilizatori SET activ = 0, is_approved = 0 WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", userId);
        return cmd.ExecuteNonQuery() > 0;
    }
}
