using Valcongrup.Data;
using Valcongrup.Forms.Dialogs;

namespace Valcongrup.UserControls;

public class UcProiecte : UserControl
{
    private readonly DataGridView _grid = ModernUi.Grid();
    private readonly ProiecteRepository _repo = new();

    public UcProiecte()
    {
        BackColor = AppTheme.Shell;
        Padding = new Padding(48, 0, 32, 32);

        var add = AppTheme.AccentButton("+ Adauga Proiect", 210, 46);
        add.Visible = Session.IsManagerOrAdmin();
        add.Click += (_, _) => Add();

        Controls.Add(BuildTable());
        Controls.Add(ModernUi.PageHeader(
            "Gestionare Proiecte",
            "Administreaza si monitorizeaza proiectele de constructii.",
            ModernUi.GhostButton("Status", 120),
            ModernUi.GhostButton("Manager", 140),
            add));

        _grid.CellFormatting += GridFormatting;
        _grid.CellPainting += (_, e) => ModernUi.PaintStatusBadge(_grid, e);
        LoadData();
    }

    private Control BuildTable()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 18,
            BackColor = AppTheme.Card,
            Padding = new Padding(0),
            Margin = new Padding(0, 20, 0, 0)
        };

        _grid.Location = new Point(0, 0);
        _grid.Size = card.Size;
        _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        card.Controls.Add(_grid);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 74, BackColor = AppTheme.Card };
        var count = ModernUi.Text("Afisand proiectele active", 10f, AppTheme.MutedBlue);
        count.Location = new Point(36, 26);
        var prev = ModernUi.GhostButton("Anterior", 106);
        prev.Height = 40;
        var next = ModernUi.GhostButton("Urmator", 106);
        next.Height = 40;
        prev.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        next.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        footer.Controls.AddRange(new Control[] { count, prev, next });
        footer.Resize += (_, _) =>
        {
            next.Location = new Point(footer.Width - 140, 17);
            prev.Location = new Point(footer.Width - 260, 17);
        };
        card.Controls.Add(footer);
        return card;
    }

    private void LoadData()
    {
        UiFactory.Try(() => _grid.DataSource = _repo.LoadProjectsData());
    }

    private void GridFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.ColumnIndex < 0)
            return;

        var header = _grid.Columns[e.ColumnIndex].HeaderText;
        if (header == "Buget Total")
            e.CellStyle!.ForeColor = AppTheme.Accent;
        if (header == "Procent Finalizat")
            e.CellStyle!.ForeColor = AppTheme.TextOnLight;
        if (header == "Status")
            e.CellStyle!.ForeColor = AppTheme.TextOnLight;
        if (header == "Nume Proiect")
            e.CellStyle!.Font = AppTheme.Font(10f, FontStyle.Bold);
    }

    private void Add()
    {
        if (!Session.IsManagerOrAdmin())
        {
            MessageBox.Show("Doar rolurile Manager si Admin pot adauga proiecte.", "VALCONGRUP", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new DialogProiect();
        if (dlg.ShowDialog() != DialogResult.OK)
            return;

        UiFactory.Try(() =>
        {
            var id = _repo.Insert(dlg.Proiect);
            new JurnalRepository().Log(Session.CurrentUser!.Id, id, $"A adaugat proiectul '{dlg.Proiect.Nume}'", "Proiect", id);
            LoadData();
        });
    }
}
