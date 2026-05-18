using Valcongrup.Models;
using Valcongrup.UserControls;

namespace Valcongrup.Forms.Dialogs;

public class DialogUtilizator : Form
{
    private readonly TextBox _nume = AppTheme.TextBox();
    private readonly TextBox _prenume = AppTheme.TextBox();
    private readonly TextBox _email = AppTheme.TextBox();
    private readonly TextBox _telefon = AppTheme.TextBox();
    private readonly TextBox _parola = AppTheme.TextBox();
    private readonly ComboBox _rol = AppTheme.Combo();
    public Utilizator Utilizator { get; } = new();
    public string Parola => _parola.Text;

    public DialogUtilizator()
    {
        Text = "Adauga Membru"; Size = new Size(460, 430); StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.BgDark; Padding = new Padding(18);
        _parola.UseSystemPasswordChar = true;
        UiFactory.Try(() => { _rol.DataSource = new Data.UtilizatoriRepository().GetRoluriTable(); _rol.DisplayMember = "nume"; _rol.ValueMember = "id"; });
        var body = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgDark }; Controls.Add(body);
        body.Controls.AddRange(new Control[] { Buttons(), UiFactory.Field("Rol*", _rol), UiFactory.Field("Telefon", _telefon), UiFactory.Field("Parola*", _parola), UiFactory.Field("Email*", _email), UiFactory.Field("Prenume*", _prenume), UiFactory.Field("Nume*", _nume) });
    }

    private Control Buttons()
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft, BackColor = AppTheme.BgDark };
        var save = AppTheme.PrimaryButton("Salveaza"); var cancel = AppTheme.SecondaryButton("Anuleaza");
        save.Click += (_, _) => { if (string.IsNullOrWhiteSpace(_nume.Text) || string.IsNullOrWhiteSpace(_prenume.Text) || string.IsNullOrWhiteSpace(_email.Text) || string.IsNullOrWhiteSpace(_parola.Text) || _rol.SelectedValue == null) { MessageBox.Show("Completeaza campurile obligatorii."); return; } Utilizator.Nume = _nume.Text.Trim(); Utilizator.Prenume = _prenume.Text.Trim(); Utilizator.Email = _email.Text.Trim(); Utilizator.Telefon = _telefon.Text.Trim(); Utilizator.IdRol = Convert.ToInt32(_rol.SelectedValue); Utilizator.Activ = true; DialogResult = DialogResult.OK; };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        p.Controls.AddRange(new Control[] { save, cancel }); return p;
    }
}
