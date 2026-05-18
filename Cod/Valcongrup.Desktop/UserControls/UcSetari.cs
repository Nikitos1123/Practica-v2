using Valcongrup.Data;
using Valcongrup.Helpers;
using System.Text.RegularExpressions;

namespace Valcongrup.UserControls;

public class UcSetari : UserControl
{
    private readonly TextBox _nume = AppTheme.TextBox();
    private readonly TextBox _prenume = AppTheme.TextBox();
    private readonly TextBox _telefon = AppTheme.TextBox();
    private readonly TextBox _veche = AppTheme.TextBox();
    private readonly TextBox _noua = AppTheme.TextBox();
    private readonly TextBox _confirma = AppTheme.TextBox();
    private readonly Label _msg = UiFactory.Label("", 9f, AppTheme.Success);
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s().-]{7,20}$", RegexOptions.Compiled);

    public UcSetari()
    {
        BackColor = AppTheme.Shell; Padding = new Padding(48, 0, 32, 32);
        _veche.UseSystemPasswordChar = _noua.UseSystemPasswordChar = _confirma.UseSystemPasswordChar = true;
        var u = Session.CurrentUser!;
        _nume.Text = u.Nume;
        _prenume.Text = u.Prenume;
        _telefon.Text = u.Telefon;
        var body = new RoundedPanel { Height = 560, Width = 540, BackColor = AppTheme.Card, Padding = new Padding(28), Radius = 12 };
        body.Anchor = AnchorStyles.Top;
        var save = AppTheme.AccentButton("Salvează", 480, 46); save.Dock = DockStyle.Top; save.Click += (_, _) => SaveProfile();
        var pass = AppTheme.AccentButton("Schimbă Parola", 480, 46); pass.Dock = DockStyle.Top; pass.Click += (_, _) => ChangePassword();
        body.Controls.AddRange(new Control[] { _msg, pass, UiFactory.Field("Confirmă Parola", _confirma), UiFactory.Field("Parola Nouă", _noua), UiFactory.Field("Parola Veche", _veche), save, UiFactory.Field("Telefon", _telefon), UiFactory.Field("Prenume", _prenume), UiFactory.Field("Nume", _nume) });
        body.Location = new Point(260, 170);
        Resize += (_, _) => body.Location = new Point(Math.Max(48, (ClientSize.Width - body.Width) / 2), 178);
        Controls.Add(body); Controls.Add(ModernUi.PageHeader("Setări", "Cont: nikitamoraru606@gmail.com | Rol: Manager"));
    }

    private void SaveProfile()
    {
        if (string.IsNullOrWhiteSpace(_nume.Text) || string.IsNullOrWhiteSpace(_prenume.Text))
        {
            ShowMsg("Numele si prenumele sunt obligatorii.", true);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_telefon.Text) && !PhoneRegex.IsMatch(_telefon.Text.Trim()))
        {
            ShowMsg("Telefon invalid. Foloseste cifre si optional +, spatii sau cratime.", true);
            return;
        }

        UiFactory.Try(() =>
        {
            var u = Session.CurrentUser!;
            u.Nume = _nume.Text.Trim(); u.Prenume = _prenume.Text.Trim(); u.Telefon = _telefon.Text.Trim();
            new UtilizatoriRepository().Update(u);
            new JurnalRepository().Log(u.Id, null, "A actualizat informatiile personale", "Utilizator", u.Id);
            ShowMsg("Profil salvat.", false);
        });
    }

    private void ChangePassword()
    {
        if (string.IsNullOrWhiteSpace(_veche.Text) || string.IsNullOrWhiteSpace(_noua.Text))
        {
            ShowMsg("Completeaza parola veche si parola noua.", true);
            return;
        }
        if (_noua.Text != _confirma.Text) { ShowMsg("Parolele nu coincid!", true); return; }
        if (_noua.Text.Length < 8) { ShowMsg("Parola noua trebuie sa aiba minim 8 caractere.", true); return; }
        UiFactory.Try(() =>
        {
            var ok = new UtilizatoriRepository().UpdateParola(Session.CurrentUser!.Id, PasswordHelper.Hash(_veche.Text), PasswordHelper.Hash(_noua.Text));
            ShowMsg(ok ? "Parola schimbată cu succes!" : "Parola veche incorectă!", !ok);
        });
    }

    private void ShowMsg(string text, bool error) { _msg.Text = text; _msg.ForeColor = error ? AppTheme.Danger : AppTheme.Success; }
}
