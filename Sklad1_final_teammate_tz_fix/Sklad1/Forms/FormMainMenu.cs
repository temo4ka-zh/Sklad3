using Sklad1.Data;
using Sklad1.Helpers;
using Sklad1.Models;
using Sklad1.Properties;

namespace Sklad1.Forms
{
    /// <summary>
    /// Главное меню приложения
    /// </summary>
    public partial class FormMainMenu : Form
    {
        private ToolStripMenuItem? tsmiWarehouseMap;
        private Panel? pnlMap;

        public FormMainMenu()
        {
            InitializeComponent();

            // Новая плитка карты склада не должна мешать входу в уже существующую систему.
            // Если создание дополнительной навигации завершится ошибкой, главное меню всё равно откроется.
            try
            {
                CreateWarehouseMapNavigation();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Ошибка создания навигации карты склада");
            }

            NormalizeMainMenuLayout();
            ConfigureTileNavigation();
            SetPermissions();
            this.Resize += (_, _) => CenterMainMenuLayout();
            this.FormClosing += FormMainMenu_FormClosing; 
        }

        private void SetPermissions()
        {
            if (CurrentUser.Role != UserRole.Admin)
            {
                tsmiReports.Visible = false;
                tsmiSettings.Visible = false;
                if (tsmiWarehouseMap != null) tsmiWarehouseMap.Visible = false;
                if (pnlMap != null) pnlMap.Visible = false;
                if (pnlSettingsTile != null) pnlSettingsTile.Visible = false;
            }
        }

        private void ConfigureTileNavigation()
        {
            MakeTileClickable(pnlWh, tsmiSklad_Click);
            MakeTileClickable(pnlDel, tsmiSupply_Click);
            MakeTileClickable(pnlExp, tsmiExpiry_Click);
            MakeTileClickable(pnlRep, tsmiReports_Click);
        }

        private void NormalizeMainMenuLayout()
        {
            // Макет главного меню приведён к рисунку 1 из ТЗ: шесть плиток 3×2.
            Text = "Складской учёт - Главное меню";
            BackColor = SystemColors.Control;
            MinimumSize = new Size(980, 700);
            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(Math.Max(ClientSize.Width, 1040), Math.Max(ClientSize.Height, 700));

            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.AutoSize = false;
            tableLayoutPanel1.Anchor = AnchorStyles.None;
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.BackColor = SystemColors.Control;

            RebuildImageTile(pnlWh, pictureBox1.BackgroundImage, "Склад", "Товары и категории");
            RebuildImageTile(pnlDel, pictureBox2.BackgroundImage, "Поставки", "Приход товаров");
            RebuildImageTile(pnlExp, pictureBox3.BackgroundImage, "Сроки годности", "Контроль партий");
            RebuildImageTile(pnlRep, pictureBox4.BackgroundImage, "Отчёты", "Аналитика и экспорт");

            tableLayoutPanel1.SetCellPosition(pnlWh, new TableLayoutPanelCellPosition(0, 0));
            tableLayoutPanel1.SetCellPosition(pnlDel, new TableLayoutPanelCellPosition(1, 0));
            tableLayoutPanel1.SetCellPosition(pnlExp, new TableLayoutPanelCellPosition(2, 0));
            tableLayoutPanel1.SetCellPosition(pnlRep, new TableLayoutPanelCellPosition(0, 1));
            tableLayoutPanel1.ResumeLayout();
            CenterMainMenuLayout();
        }

        private void CenterMainMenuLayout()
        {
            var desiredWidth = Math.Min(980, Math.Max(900, ClientSize.Width - 120));
            var desiredHeight = Math.Min(520, Math.Max(460, ClientSize.Height - menuStripMain.Height - 135));
            tableLayoutPanel1.Size = new Size(desiredWidth, desiredHeight);
            tableLayoutPanel1.Location = new Point((ClientSize.Width - desiredWidth) / 2, Math.Max(105, menuStripMain.Height + 70));

            lblTitle.Left = Math.Max(0, (ClientSize.Width - lblTitle.Width) / 2);
            lblTitle.Top = menuStripMain.Height + 10;
            lblSubtitle.Left = Math.Max(0, (ClientSize.Width - lblSubtitle.Width) / 2);
            lblSubtitle.Top = lblTitle.Bottom + 2;
        }

        private void RebuildImageTile(Panel panel, Image? image, string title, string subtitle)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();
            panel.AutoSize = false;
            panel.BackColor = SystemColors.GradientInactiveCaption;
            panel.Margin = new Padding(8);
            panel.Cursor = Cursors.Hand;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = SystemColors.GradientInactiveCaption,
                Cursor = Cursors.Hand,
                Padding = new Padding(8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            var picture = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackgroundImage = image,
                BackgroundImageLayout = ImageLayout.Zoom,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Margin = new Padding(35, 10, 35, 0)
            };
            var lblHeader = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, 204),
                Cursor = Cursors.Hand
            };
            var lblText = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204),
                Cursor = Cursors.Hand
            };
            layout.Controls.Add(picture, 0, 0);
            layout.Controls.Add(lblHeader, 0, 1);
            layout.Controls.Add(lblText, 0, 2);
            panel.Controls.Add(layout);
            panel.ResumeLayout();
        }

        private void MakeTileClickable(Control tile, EventHandler handler)
        {
            tile.Cursor = Cursors.Hand;
            tile.Click += handler;
            foreach (Control child in tile.Controls)
            {
                MakeTileClickable(child, handler);
            }
        }

        private void CreateWarehouseMapNavigation()
        {
            tsmiWarehouseMap = new ToolStripMenuItem
            {
                Text = "Карта склада",
                BackColor = Color.MidnightBlue,
                ForeColor = SystemColors.ButtonFace,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204)
            };
            tsmiWarehouseMap.Click += OpenWarehouseMap;
            menuStripMain.Items.Insert(Math.Max(0, menuStripMain.Items.Count - 2), tsmiWarehouseMap);

            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.RowCount = 2;

            pnlMap = CreateMenuTile("Карта склада", "Тепловая карта", "▦", Color.RoyalBlue);
            MakeTileClickable(pnlMap, OpenWarehouseMap);
            tableLayoutPanel1.Controls.Add(pnlMap, 1, 1);

            if (pnlSettingsTile == null)
            {
                pnlSettingsTile = CreateMenuTile("Настройки", "Параметры системы", "⚙", Color.DimGray);
                MakeTileClickable(pnlSettingsTile, tsmiSettings_Click);
                tableLayoutPanel1.Controls.Add(pnlSettingsTile, 2, 1);
            }
        }

        private Panel? pnlSettingsTile;

        private Panel CreateMenuTile(string title, string subtitle, string icon, Color iconColor)
        {
            var panel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = SystemColors.GradientInactiveCaption,
                Margin = new Padding(3),
                Cursor = Cursors.Hand
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.GradientInactiveCaption,
                ColumnCount = 1,
                RowCount = 3,
                Cursor = Cursors.Hand
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            var lblIcon = new Label
            {
                Text = icon,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 42F, FontStyle.Bold, GraphicsUnit.Point, 204),
                ForeColor = iconColor,
                Cursor = Cursors.Hand
            };
            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, 204),
                Cursor = Cursors.Hand
            };
            var lblSubtitle = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Cursor = Cursors.Hand
            };

            layout.Controls.Add(lblIcon, 0, 0);
            layout.Controls.Add(lblTitle, 0, 1);
            layout.Controls.Add(lblSubtitle, 0, 2);
            panel.Controls.Add(layout);
            return panel;
        }

        private void OpenWarehouseMap(object? sender, EventArgs e)
        {
            if (CurrentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Нет доступа к карте склада");
                return;
            }

            var form = new FormWarehouseMap();
            form.ShowDialog();
        }

        private void tsmiSklad_Click(object sender, EventArgs e)
        {
            var form = new FormMain();
            form.ShowDialog();
        }

        private void tsmiSupply_Click(object sender, EventArgs e)
        {
            var form = new FormSupply();
            form.ShowDialog();
        }

        private void tsmiSupplyImport_Click(object sender, EventArgs e)
        {
            var form = new FormSupplyImport();
            form.ShowDialog();
        }

        private void tsmiExpiry_Click(object sender, EventArgs e)
        {
            var form = new FormExpiryDates();
            form.ShowDialog();
        }

        private void tsmiReports_Click(object sender, EventArgs e)
        {
            var form = new FormAnalyticReport();
            form.ShowDialog();
        }

        private void tsmiSettings_Click(object sender, EventArgs e)
        {
            var form = new FormCurrencySettings();
            form.ShowDialog();
        }

        private void tsmiSupplies_Click(object sender, EventArgs e)
        {
            //чтоб открыть подпункты
        }

        private void tsmiLogOut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.LogOut, Resources.LogOutText, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                FormLogin loginForm = new FormLogin();
                loginForm.Show();

                this.Close();
            }
        }
        private void FormMainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(Resources.LogOut, Resources.LogOutText, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                FormLogin loginForm = new FormLogin();
                loginForm.Show();
            }
            else
            {
                e.Cancel = true;  
            }
        }
    }
}