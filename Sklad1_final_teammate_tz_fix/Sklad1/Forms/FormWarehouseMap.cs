using Microsoft.EntityFrameworkCore;
using Sklad1.Data;
using Sklad1.Helpers;
using Sklad1.Models;

namespace Sklad1.Forms
{
    /// <summary>
    /// Тепловая карта склада: цвет ячейки зависит от срока годности и количества товара.
    /// </summary>
    public class FormWarehouseMap : Form
    {
        private readonly Dictionary<string, Button> _cellButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, List<BatchInfo>> _cellData = new Dictionary<string, List<BatchInfo>>();
        private readonly HashSet<string> _highlightedCells = new HashSet<string>();

        private FlowLayoutPanel pnlGrid = null!;
        private TextBox txtSearch = null!;
        private ListBox lstSearchResults = null!;
        private Label lblSearchMessage = null!;
        private TextBox txtInfo = null!;
        private TextBox txtSummary = null!;
        private FlowLayoutPanel pnlLegend = null!;
        private Button btnRefresh = null!;

        public FormWarehouseMap()
        {
            if (CurrentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Нет доступа к карте склада");
                BeginInvoke(new Action(Close));
                return;
            }

            InitializeComponentRuntime();
            LoadMapData();
        }

        private void InitializeComponentRuntime()
        {
            Text = "Карта склада";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(940, 620);
            BackColor = Color.FromArgb(205, 226, 239);
            MinimumSize = new Size(900, 600);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);

            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.MidnightBlue
            };
            var title = new Label
            {
                Text = "Карта склада",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204),
                AutoSize = true,
                Location = new Point(8, 6)
            };
            btnRefresh = new Button
            {
                Text = "Обновить",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = SystemColors.ActiveCaption,
                Location = new Point(760, 6),
                Size = new Size(120, 28)
            };
            btnRefresh.Click += (_, _) => LoadMapData();
            top.Controls.Add(title);
            top.Controls.Add(btnRefresh);
            Controls.Add(top);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(205, 226, 239)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
            Controls.Add(root);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White,
                Padding = new Padding(6),
                Margin = new Padding(0, 0, 8, 0)
            };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            root.Controls.Add(left, 0, 0);

            var searchBox = new GroupBox
            {
                Text = "Поиск товара",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204)
            };
            var lblSearch = new Label
            {
                Text = "Название товара",
                AutoSize = true,
                Location = new Point(8, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            txtSearch = new TextBox { Location = new Point(8, 48), Width = 150 };
            lblSearchMessage = new Label
            {
                Text = string.Empty,
                Location = new Point(8, 77),
                Size = new Size(150, 28),
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            lstSearchResults = new ListBox
            {
                Location = new Point(8, 76),
                Size = new Size(150, 28),
                DisplayMember = nameof(SearchProductItem.Name)
            };
            txtSearch.TextChanged += (_, _) => SearchProducts();
            lstSearchResults.SelectedIndexChanged += (_, _) => HighlightSelectedProduct();
            searchBox.Controls.Add(lblSearch);
            searchBox.Controls.Add(txtSearch);
            searchBox.Controls.Add(lblSearchMessage);
            searchBox.Controls.Add(lstSearchResults);
            left.Controls.Add(searchBox, 0, 0);

            var infoBox = new GroupBox
            {
                Text = "Информация о товаре",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204)
            };
            txtInfo = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Text = "Выберите ячейку на карте",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            infoBox.Controls.Add(txtInfo);
            left.Controls.Add(infoBox, 0, 1);

            var lblSummary = new Label
            {
                Text = "Сводка по складу",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 204)
            };
            left.Controls.Add(lblSummary, 0, 2);

            txtSummary = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            left.Controls.Add(txtSummary, 0, 3);

            var center = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(205, 226, 239),
                Padding = new Padding(8, 6, 8, 6)
            };
            root.Controls.Add(center, 1, 0);

            var mapTitle = new Label
            {
                Text = "Карта склада",
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 204),
                ForeColor = Color.MidnightBlue
            };
            center.Controls.Add(mapTitle);

            pnlGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(205, 226, 239),
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0, 4, 0, 0)
            };
            center.Controls.Add(pnlGrid);

            pnlLegend = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(6),
                WrapContents = true,
                Margin = new Padding(0, 8, 0, 0)
            };
            root.SetColumnSpan(pnlLegend, 2);
            root.Controls.Add(pnlLegend, 0, 1);

            BuildEmptyGrid();
            BuildLegend();
        }

        private void BuildEmptyGrid()
        {
            pnlGrid.Controls.Clear();
            _cellButtons.Clear();

            var headerRow = new FlowLayoutPanel
            {
                Width = 520,
                Height = 28,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            headerRow.Controls.Add(new Label { Text = string.Empty, Width = 34, Height = 24, TextAlign = ContentAlignment.MiddleCenter });
            foreach (var col in WarehouseMapRules.Columns)
            {
                headerRow.Controls.Add(new Label { Text = col.ToString(), Width = 76, Height = 24, TextAlign = ContentAlignment.MiddleCenter });
            }
            pnlGrid.Controls.Add(headerRow);

            foreach (var row in WarehouseMapRules.Rows)
            {
                var rowPanel = new FlowLayoutPanel
                {
                    Width = 520,
                    Height = 44,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false
                };
                rowPanel.Controls.Add(new Label { Text = row, Width = 34, Height = 38, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10F, FontStyle.Bold) });

                foreach (var col in WarehouseMapRules.Columns)
                {
                    var code = $"{row}{col}";
                    var button = new Button
                    {
                        Text = code,
                        Tag = code,
                        Width = 76,
                        Height = 38,
                        Margin = new Padding(2),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.White
                    };
                    button.FlatAppearance.BorderColor = Color.Gray;
                    button.FlatAppearance.BorderSize = 1;
                    button.Click += (_, _) => ShowCellInfo(code);
                    _cellButtons[code] = button;
                    rowPanel.Controls.Add(button);
                }
                pnlGrid.Controls.Add(rowPanel);
            }
        }

        private void LoadMapData()
        {
            try
            {
                _cellData.Clear();
                DatabaseSchemaInitializer.Initialize();
                using var db = new Context();
                var batches = db.ProductBatches
                    .Include(b => b.Product)
                    .Where(b => b.Status == "active" && b.Quantity > 0)
                    .ToList();

                var changed = false;
                foreach (var batch in batches)
                {
                    var normalized = WarehouseMapRules.NormalizeCellCode(batch.CellCode, batch.ProductId);
                    if (batch.CellCode != normalized)
                    {
                        batch.CellCode = normalized;
                        changed = true;
                    }
                }
                if (changed)
                    db.SaveChanges();

                foreach (var batch in batches)
                {
                    var code = WarehouseMapRules.NormalizeCellCode(batch.CellCode, batch.ProductId);
                    if (!_cellData.ContainsKey(code))
                        _cellData[code] = new List<BatchInfo>();

                    _cellData[code].Add(new BatchInfo
                    {
                        BatchId = batch.Id,
                        ProductId = batch.ProductId,
                        ProductName = batch.Product?.Name ?? "Без названия",
                        Quantity = batch.Quantity,
                        Unit = batch.Product?.Unit ?? "шт",
                        ExpiryDate = batch.ExpiryDate.Date,
                        CellCode = code
                    });
                }

                ApplyCellVisuals();
                UpdateSummary();
            }
            catch
            {
                MessageBox.Show("Ошибка базы данных. Повторите попытку");
            }
        }

        private void ApplyCellVisuals()
        {
            foreach (var pair in _cellButtons)
            {
                var code = pair.Key;
                var button = pair.Value;
                var batches = _cellData.TryGetValue(code, out var infos)
                    ? infos.Select(i => new ProductBatch { Quantity = i.Quantity, ExpiryDate = i.ExpiryDate, Status = "active" })
                    : Enumerable.Empty<ProductBatch>();
                var state = WarehouseMapRules.CalculateCellState(batches, DateTime.Today);

                button.BackColor = state.BackColor;
                button.FlatAppearance.BorderColor = _highlightedCells.Contains(code)
                    ? Color.Black
                    : state.HasBlueBorder ? Color.RoyalBlue : Color.Gray;
                button.FlatAppearance.BorderSize = _highlightedCells.Contains(code)
                    ? 4
                    : state.HasBlueBorder ? 3 : 1;
            }
        }

        private void SearchProducts()
        {
            var query = txtSearch.Text.Trim();
            lblSearchMessage.Text = string.Empty;
            lstSearchResults.DataSource = null;
            _highlightedCells.Clear();
            ApplyCellVisuals();

            if (query.Length == 0)
            {
                txtInfo.Text = "Выберите ячейку на карте";
                return;
            }

            if (query.Length == 1)
            {
                lblSearchMessage.Text = "Введите минимум 2 символа для поиска";
                return;
            }

            try
            {
                using var db = new Context();
                var lower = query.ToLowerInvariant();
                var catalogProducts = db.Products
                    .Where(p => p.Name.ToLower().Contains(lower))
                    .Select(p => new SearchProductItem { ProductId = p.Id, Name = p.Name })
                    .ToList();

                var stockProductIds = _cellData.Values
                    .SelectMany(v => v)
                    .Where(b => b.ProductName.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                    .Select(b => b.ProductId)
                    .Distinct()
                    .ToHashSet();

                var result = catalogProducts
                    .Where(p => stockProductIds.Contains(p.ProductId))
                    .OrderBy(p => p.Name)
                    .ToList();

                if (result.Count == 0)
                {
                    lblSearchMessage.Text = catalogProducts.Count > 0
                        ? "Товар найден в справочнике, но отсутствует на складе"
                        : "Товар не найден на складе";
                    return;
                }

                lstSearchResults.DataSource = result;
            }
            catch
            {
                lblSearchMessage.Text = "Ошибка базы данных. Повторите попытку";
            }
        }

        private void HighlightSelectedProduct()
        {
            if (lstSearchResults.SelectedItem is not SearchProductItem selected)
                return;

            _highlightedCells.Clear();
            var cells = _cellData
                .Where(pair => pair.Value.Any(b => b.ProductId == selected.ProductId))
                .Select(pair => pair.Key)
                .ToList();

            foreach (var cell in cells)
                _highlightedCells.Add(cell);

            ApplyCellVisuals();

            if (cells.Count > 0)
                ShowCellInfo(cells[0]);
        }

        private void ShowCellInfo(string cellCode)
        {
            if (!_cellData.TryGetValue(cellCode, out var batches) || batches.Count == 0)
            {
                txtInfo.Text = $"Ячейка: {cellCode}. Состояние: Пусто";
                return;
            }

            var groupedBatches = batches
                .GroupBy(b => new { b.ProductId, b.ProductName, b.Unit, Expiry = b.ExpiryDate.Date })
                .Select(g => new
                {
                    g.Key.ProductName,
                    g.Key.Unit,
                    ExpiryDate = g.Key.Expiry,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(b => b.ExpiryDate)
                .ThenBy(b => b.ProductName)
                .ToList();

            var lines = new List<string> { $"Ячейка: {cellCode}", string.Empty };
            foreach (var batch in groupedBatches)
            {
                var daysLeft = (batch.ExpiryDate.Date - DateTime.Today).Days;
                lines.Add($"Товар: {batch.ProductName}");
                lines.Add($"Количество: {batch.Quantity} {batch.Unit}");
                lines.Add($"Срок годности: {batch.ExpiryDate:dd.MM.yyyy}");
                lines.Add($"Осталось дней: {daysLeft}");
                lines.Add(string.Empty);
            }
            txtInfo.Text = string.Join(Environment.NewLine, lines);
        }

        private void UpdateSummary()
        {
            var all = _cellData.Values.SelectMany(x => x).ToList();
            if (all.Count == 0)
            {
                txtSummary.Text = "Всего товаров: 0";
                return;
            }

            var today = DateTime.Today;
            var total = all.Sum(b => b.Quantity);
            var normal = all.Where(b => (b.ExpiryDate - today).Days > 30).Sum(b => b.Quantity);
            var avgDays = all.Average(b => (b.ExpiryDate - today).Days);
            var expiring = all.Where(b => (b.ExpiryDate - today).Days <= 30).Sum(b => b.Quantity);
            var lowQuantity = all.Where(b => b.Quantity < WarehouseMapRules.LowQuantityThreshold).Sum(b => b.Quantity);
            var critical = all.Where(b => (b.ExpiryDate - today).Days < 15 && b.Quantity < WarehouseMapRules.LowQuantityThreshold).Sum(b => b.Quantity);

            txtSummary.Text = string.Join(Environment.NewLine, new[]
            {
                $"Всего товаров: {total}",
                $"Норма (>30 дн.): {normal}",
                $"Средний срок: {avgDays:0} дн.",
                $"Истекает (<=30 дн.): {expiring}",
                $"Меньше 10 шт.: {lowQuantity}",
                $"Критические товары: {critical}"
            });
        }

        private void BuildLegend()
        {
            pnlLegend.Controls.Clear();
            AddLegendItem(Color.LightGreen, "Зелёный — срок больше 30 дней");
            AddLegendItem(Color.Khaki, "Жёлтый — срок 15–30 дней включительно");
            AddLegendItem(Color.Orange, "Оранжевый — срок меньше 15 дней, количество от 10");
            AddLegendItem(Color.IndianRed, "Красный — срок меньше 15 дней и количество меньше 10");
            AddLegendItem(Color.White, "Синяя рамка — количество меньше 10 и срок от 15 дней", Color.RoyalBlue, 3);
            AddLegendItem(Color.White, "Толстая чёрная рамка — найденный товар", Color.Black, 4);
        }

        private void AddLegendItem(Color backColor, string text, Color? borderColor = null, int borderSize = 1)
        {
            var panel = new Panel { Width = 34, Height = 24, BackColor = backColor, Margin = new Padding(8, 8, 3, 3) };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(borderColor ?? Color.Gray, borderSize);
                e.Graphics.DrawRectangle(pen, 1, 1, panel.Width - 3, panel.Height - 3);
            };
            var label = new Label { Text = text, AutoSize = true, Height = 32, Margin = new Padding(3, 8, 20, 3) };
            pnlLegend.Controls.Add(panel);
            pnlLegend.Controls.Add(label);
        }

        private sealed class BatchInfo
        {
            public Guid BatchId { get; set; }
            public Guid ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Unit { get; set; } = string.Empty;
            public DateTime ExpiryDate { get; set; }
            public string CellCode { get; set; } = string.Empty;
        }

        private sealed class SearchProductItem
        {
            public Guid ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public override string ToString() => Name;
        }
    }
}
