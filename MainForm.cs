namespace RapidClicker;

internal sealed class MainForm : Form
{
    private const int ToggleHotkeyId = 1;
    private const int QuitHotkeyId = 2;

    private readonly NumericUpDown intervalInput = new()
    {
        Minimum = 1,
        Maximum = 10_000,
        Value = 50,
        Increment = 5,
        Width = 100,
        TextAlign = HorizontalAlignment.Center
    };
    private readonly Label speedLabel = new() { AutoSize = true, ForeColor = Color.DimGray };
    private readonly Label statusLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
    private readonly Button toggleButton = new() { AutoSize = true, Padding = new Padding(16, 7, 16, 7) };
    private readonly NotifyIcon trayIcon;
    private CancellationTokenSource? clickCancellation;
    private bool isClicking;
    private bool allowClose;

    public MainForm()
    {
        Text = "Rapid Clicker";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(380, 230);
        Font = new Font("Segoe UI", 9);

        var title = new Label
        {
            Text = "Rapid Clicker",
            AutoSize = true,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(24, 20)
        };

        var description = new Label
        {
            Text = "Clique automático no ponto atual do cursor.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(27, 54)
        };

        var intervalLabel = new Label
        {
            Text = "Intervalo entre cliques (ms)",
            AutoSize = true,
            Location = new Point(27, 95)
        };

        intervalInput.Location = new Point(225, 90);
        intervalInput.ValueChanged += (_, _) => UpdateSpeedLabel();

        speedLabel.Location = new Point(27, 123);
        statusLabel.Location = new Point(27, 154);
        toggleButton.Location = new Point(225, 145);
        toggleButton.Click += (_, _) => ToggleClicking();

        var hotkeyHint = new Label
        {
            Text = "F6 iniciar/pausar  •  F7 encerrar",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(27, 202)
        };

        Controls.AddRange([title, description, intervalLabel, intervalInput, speedLabel, statusLabel, toggleButton, hotkeyHint]);

        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Mostrar", null, (_, _) => ShowFromTray());
        trayMenu.Items.Add("Iniciar/Pausar (F6)", null, (_, _) => ToggleClicking());
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Encerrar", null, (_, _) => ExitApplication());
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Rapid Clicker",
            ContextMenuStrip = trayMenu,
            Visible = true
        };
        trayIcon.DoubleClick += (_, _) => ShowFromTray();

        UpdateSpeedLabel();
        UpdateUi();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        var flags = NativeMethods.ModNone | NativeMethods.ModNoRepeat;
        if (!NativeMethods.RegisterHotKey(Handle, ToggleHotkeyId, flags, NativeMethods.KeyF6) ||
            !NativeMethods.RegisterHotKey(Handle, QuitHotkeyId, flags, NativeMethods.KeyF7))
        {
            MessageBox.Show("Não foi possível registrar F6 e F7. Feche outro programa que esteja usando esses atalhos.",
                "Rapid Clicker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WmHotKey)
        {
            switch (m.WParam.ToInt32())
            {
                case ToggleHotkeyId:
                    ToggleClicking();
                    return;
                case QuitHotkeyId:
                    ExitApplication();
                    return;
            }
        }

        base.WndProc(ref m);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            trayIcon.ShowBalloonTip(1000, "Rapid Clicker", "O aplicativo continua disponível na bandeja do sistema.", ToolTipIcon.Info);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        StopClicking();
        NativeMethods.UnregisterHotKey(Handle, ToggleHotkeyId);
        NativeMethods.UnregisterHotKey(Handle, QuitHotkeyId);
        trayIcon.Visible = false;
        trayIcon.Dispose();
        base.OnFormClosing(e);
    }

    private void ToggleClicking()
    {
        if (isClicking)
            StopClicking();
        else
            StartClicking();
    }

    private void StartClicking()
    {
        if (isClicking)
            return;

        isClicking = true;
        intervalInput.Enabled = false;
        clickCancellation = new CancellationTokenSource();
        _ = ClickLoopAsync((int)intervalInput.Value, clickCancellation.Token);
        UpdateUi();
    }

    private void StopClicking()
    {
        if (!isClicking)
            return;

        isClicking = false;
        clickCancellation?.Cancel();
        clickCancellation?.Dispose();
        clickCancellation = null;
        intervalInput.Enabled = true;
        UpdateUi();
    }

    private static async Task ClickLoopAsync(int intervalMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMilliseconds));
            while (await timer.WaitForNextTickAsync(cancellationToken))
                NativeMethods.LeftClick();
        }
        catch (OperationCanceledException)
        {
            // Expected when paused or closed.
        }
    }

    private void UpdateSpeedLabel()
    {
        var clicksPerSecond = 1_000m / intervalInput.Value;
        speedLabel.Text = $"Velocidade aproximada: {clicksPerSecond:0.##} cliques/segundo";
    }

    private void UpdateUi()
    {
        statusLabel.Text = isClicking ? "● ATIVO" : "● PAUSADO";
        statusLabel.ForeColor = isClicking ? Color.ForestGreen : Color.Firebrick;
        toggleButton.Text = isClicking ? "Pausar (F6)" : "Iniciar (F6)";
        trayIcon.Text = isClicking ? "Rapid Clicker — ativo" : "Rapid Clicker — pausado";
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        allowClose = true;
        Close();
    }

}
