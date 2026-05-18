using Valcongrup.Data;
using Valcongrup.Forms.Dialogs;
using Valcongrup.Models;

namespace Valcongrup.UserControls;

public class UcEchipa : UserControl
{
    private readonly DataGridView _grid = ModernUi.Grid();
    private readonly UtilizatoriRepository _repo = new();
    private readonly Button _specialization;

    public UcEchipa()
    {
        BackColor = AppTheme.Shell;
        Padding = new Padding(48, 0, 32, 32);

        _specialization = AppTheme.AccentButton("Specializare", 170, 46);
        _specialization.Click += (_, _) => ShowSpecializationFilter();

        var add = AppTheme.AccentButton("+  Invită membru", 200, 46);
        add.Click += (_, _) => Add();

        Controls.Add(BuildTable());
        Controls.Add(ModernUi.PageHeader("Echipă", "Gestionează membrii echipei și alocările lor pe proiecte.", _specialization, add));
        ConfigureGrid();
        LoadData();
    }

    private Control BuildTable()
    {
        var card = new RoundedPanel { Dock = DockStyle.Fill, Radius = 18, BackColor = AppTheme.Card, Padding = new Padding(0), Margin = new Padding(0, 20, 0, 0) };
        card.Controls.Add(_grid);
        return card;
    }

    private void ConfigureGrid()
    {
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Membru", DataPropertyName = "FullName", FillWeight = 28, MinimumWidth = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rol", DataPropertyName = "NumeRol", FillWeight = 18, MinimumWidth = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", FillWeight = 30, MinimumWidth = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Telefon", DataPropertyName = "Telefon", FillWeight = 14, MinimumWidth = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 10, MinimumWidth = 100 });
        _grid.CellPainting += (_, e) => ModernUi.PaintStatusBadge(_grid, e);
        foreach (DataGridViewColumn col in _grid.Columns)
            col.SortMode = DataGridViewColumnSortMode.NotSortable;
    }

    private void LoadData()
    {
        UiFactory.Try(() =>
        {
            _grid.DataSource = _repo.GetAll()
                .Where(u => u.Activ)
                .Select(u => new TeamRow(u))
                .ToList();
        });
    }

    private void Add()
    {
        using var dlg = new DialogUtilizator();
        if (dlg.ShowDialog() == DialogResult.OK)
            UiFactory.Try(() =>
            {
                var id = _repo.Insert(dlg.Utilizator, Helpers.PasswordHelper.Hash(dlg.Parola));
                new JurnalRepository().Log(Session.CurrentUser!.Id, null, $"A adaugat utilizatorul '{dlg.Utilizator.Email}'", "Utilizator", id);
                LoadData();
            });
    }

    private void ShowSpecializationFilter()
    {
        var roles = _repo.GetAll()
            .Select(u => u.NumeRol)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r)
            .ToList();

        if (roles.Count == 0)
        {
            MessageBox.Show("Nu există specializări definite în acest moment.", "Specializare", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new Form
        {
            Text = "Filtrează specializare",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ClientSize = new Size(420, 160)
        };

        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, Font = AppTheme.Font(10f), Height = 36, Margin = new Padding(16) };
        combo.Items.Add("Toate specializările");
        combo.Items.AddRange(roles.Cast<object>().ToArray());
        combo.SelectedIndex = 0;

        var label = new Label
        {
            Text = "Alege specializarea:",
            Dock = DockStyle.Top,
            Font = AppTheme.Font(10f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            Padding = new Padding(16, 16, 16, 4)
        };

        var apply = AppTheme.AccentButton("Aplică", 120, 42);
        apply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        apply.Location = new Point(dialog.ClientSize.Width - apply.Width - 16, dialog.ClientSize.Height - apply.Height - 16);
        apply.Click += (_, _) => dialog.DialogResult = DialogResult.OK;

        dialog.Controls.Add(apply);
        dialog.Controls.Add(combo);
        dialog.Controls.Add(label);

        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

        var selected = combo.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selected) || selected == "Toate specializările")
            LoadData();
        else
            UiFactory.Try(() =>
            {
                _grid.DataSource = _repo.GetAll()
                    .Where(u => string.Equals(u.NumeRol, selected, StringComparison.OrdinalIgnoreCase))
                    .Select(u => new TeamRow(u))
                    .ToList();
            });
    }

    private sealed class TeamRow
    {
        public TeamRow(Utilizator user)
        {
            FullName = $"{user.Prenume} {user.Nume}".Trim();
            NumeRol = user.NumeRol;
            Email = user.Email;
            Telefon = string.IsNullOrWhiteSpace(user.Telefon) ? "-" : user.Telefon;
            StatusText = user.Activ ? "Activ" : "Suspendat";
        }

        public string FullName { get; }
        public string NumeRol { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string StatusText { get; }
    }
}
