using Sklad1.Data;
using Sklad1.Helpers;
using Sklad1.Models;
using Sklad1.Properties;

namespace Sklad1.Forms
{
    public partial class FormShipment : Form
    {
        private readonly List<ShipmentItemTemp> _items = new List<ShipmentItemTemp>();
        private ContractorCheckResult? _contractorCheck;
        private WeatherCheckResult? _weatherCheck;

        private TextBox txtContractorInn = null!;
        private Button btnCheckInn = null!;
        private Label lblContractorStatus = null!;
        private Label lblContractorOrganization = null!;
        private Label lblContractorCheckedAt = null!;

        private TextBox txtDeliveryCity = null!;
        private DateTimePicker dtpDeliveryDate = null!;
        private Button btnGetWeather = null!;
        private Label lblWeatherResult = null!;

        public FormShipment()
        {
            InitializeComponent();
            InitializeContractorAndWeatherBlocks();
            AppCurrencyManager.CurrencyChanged += OnCurrencyChanged;
            LoadProducts();

            btnAdd.Click += BtnAdd_Click;
            btnShip.Click += BtnShip_Click;
            btnCancel.Click += btnCancel_Click;
            cmbProduct.SelectedIndexChanged += cmbProduct_SelectedIndexChanged;
        }

        private void InitializeContractorAndWeatherBlocks()
        {
            // Макет приведён к рисунку 2 из ТЗ: слева список товаров, справа блоки
            // «ИНН контрагента», «Геолокация и погода», «Отгрузка».
            Text = "Отгрузка";
            BackColor = SystemColors.ActiveCaption;
            MinimumSize = new Size(820, 620);
            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(820, 620);

            panel2.Height = 44;
            panel2.BackColor = Color.MidnightBlue;
            panel2.Dock = DockStyle.Bottom;

            lblProductList.Text = "Список товаров";
            lblProductList.Font = new Font("Segoe UI Semibold", 13.5F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblProductList.TextAlign = ContentAlignment.MiddleCenter;
            dgvItems.BackgroundColor = Color.White;
            dgvItems.BorderStyle = BorderStyle.FixedSingle;
            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 240, 248);
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);

            panel1.Width = 340;
            panel1.BackColor = Color.White;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panel1.AutoScroll = false;
            panel1.Controls.Clear();

            var gbInn = new GroupBox
            {
                Text = "ИНН контрагента",
                Name = "gbShipmentInn",
                Size = new Size(320, 126),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204),
                BackColor = Color.White
            };
            var lblInn = new Label
            {
                Text = "ИНН",
                AutoSize = true,
                Location = new Point(12, 27),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            txtContractorInn = new TextBox
            {
                BackColor = SystemColors.ActiveCaption,
                Location = new Point(12, 50),
                Size = new Size(145, 28),
                MaxLength = 12,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            btnCheckInn = CreateStyledButton("Проверить по API", new Point(168, 48), new Size(140, 32));
            lblContractorStatus = new Label
            {
                Text = ContractorCheckService.NotPerformedText,
                AutoSize = false,
                Location = new Point(12, 82),
                Size = new Size(296, 20),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204)
            };
            lblContractorOrganization = new Label
            {
                Text = "Организация: —",
                AutoSize = false,
                Location = new Point(12, 103),
                Size = new Size(142, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            lblContractorCheckedAt = new Label
            {
                Text = "Дата проверки: —",
                AutoSize = false,
                Location = new Point(160, 103),
                Size = new Size(148, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            gbInn.Controls.AddRange(new Control[] { lblInn, txtContractorInn, btnCheckInn, lblContractorStatus, lblContractorOrganization, lblContractorCheckedAt });

            var gbWeather = new GroupBox
            {
                Text = "Геолокация и погода",
                Name = "gbShipmentWeather",
                Size = new Size(320, 145),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204),
                BackColor = Color.White
            };
            var lblCity = new Label
            {
                Text = "Город доставки",
                AutoSize = true,
                Location = new Point(12, 26),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            txtDeliveryCity = new TextBox
            {
                BackColor = SystemColors.ActiveCaption,
                Location = new Point(12, 48),
                Size = new Size(145, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            var lblDate = new Label
            {
                Text = "Дата доставки",
                AutoSize = true,
                Location = new Point(168, 26),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            dtpDeliveryDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Location = new Point(168, 48),
                Size = new Size(140, 28),
                MinDate = DateTime.Today,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            btnGetWeather = CreateStyledButton("Получить прогноз", new Point(168, 88), new Size(140, 32));
            lblWeatherResult = new Label
            {
                Text = "Прогноз не получен",
                AutoSize = false,
                Location = new Point(12, 86),
                Size = new Size(148, 50),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8.2F, FontStyle.Regular, GraphicsUnit.Point, 204)
            };
            gbWeather.Controls.AddRange(new Control[] { lblCity, txtDeliveryCity, lblDate, dtpDeliveryDate, btnGetWeather, lblWeatherResult });

            var gbShipment = new GroupBox
            {
                Text = "Отгрузка",
                Name = "gbShipmentAction",
                Size = new Size(320, 205),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204),
                BackColor = Color.White
            };
            lblProduct.Text = "Выберите товар из списка";
            lblProduct.Location = new Point(14, 30);
            lblProduct.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            cmbProduct.Location = new Point(14, 56);
            cmbProduct.Size = new Size(296, 28);
            lblQuantity.Text = "Введите количество";
            lblQuantity.Location = new Point(14, 97);
            lblQuantity.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            cmbQuantity.Location = new Point(14, 123);
            cmbQuantity.Size = new Size(296, 28);
            lblClient.Text = "Куда отгрузить";
            lblClient.Location = new Point(14, 158);
            lblClient.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            txtClient.Location = new Point(14, 180);
            txtClient.Size = new Size(178, 28);
            btnAdd = CreateStyledButton("Добавить", new Point(205, 177), new Size(103, 32));
            gbShipment.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblQuantity, cmbQuantity, lblClient, txtClient, btnAdd });

            btnCancel = CreateStyledButton("Отмена", new Point(36, 0), new Size(128, 44));
            btnShip = CreateStyledButton("Совершить\nотгрузку", new Point(178, 0), new Size(130, 44));
            panel1.Controls.AddRange(new Control[] { gbInn, gbWeather, gbShipment, btnCancel, btnShip });

            void LayoutShipmentForm()
            {
                var rightWidth = 340;
                var rightLeft = ClientSize.Width - rightWidth - 28;
                panel1.Location = new Point(rightLeft, 66);
                panel1.Size = new Size(rightWidth, Math.Max(488, ClientSize.Height - 110));

                lblProductList.Location = new Point(0, 22);
                lblProductList.Size = new Size(Math.Max(360, panel1.Left - 20), 42);
                dgvItems.Location = new Point(30, 75);
                dgvItems.Size = new Size(Math.Max(360, panel1.Left - 55), Math.Max(450, ClientSize.Height - 128));

                var margin = 10;
                gbInn.Location = new Point(margin, 16);
                gbWeather.Location = new Point(margin, gbInn.Bottom + 12);
                gbShipment.Location = new Point(margin, gbWeather.Bottom + 12);
                btnCancel.Location = new Point(28, gbShipment.Bottom + 12);
                btnShip.Location = new Point(180, gbShipment.Bottom + 12);
            }

            LayoutShipmentForm();
            Resize += (_, _) => LayoutShipmentForm();

            txtContractorInn.TextChanged += (_, _) => ResetContractorCheck();
            btnCheckInn.Click += async (_, _) => await CheckContractorInnAsync();
            txtDeliveryCity.TextChanged += (_, _) => ResetWeatherCheck();
            dtpDeliveryDate.ValueChanged += (_, _) => ResetWeatherCheck();
            btnGetWeather.Click += async (_, _) => await GetWeatherAsync();
        }

        private Button CreateStyledButton(string text, Point location, Size size)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = SystemColors.ActiveCaption,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(100, 130, 165);
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private void OnCurrencyChanged()
        {
            LoadProducts();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoadProducts()
        {
            using (var bd = new Context())
            {
                var products = bd.Products.Where(p => p.Quantity > 0).Select(p => new ProductItem
                {
                    Article = p.Article,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    PurchasePrice = p.PurchasePrice
                })
                    .ToList();

                cmbProduct.DisplayMember = nameof(ProductItem.Name);
                cmbProduct.ValueMember = nameof(ProductItem.Article);
                cmbProduct.DataSource = products;
            }
        }

        private void UpdateQuantityDropdown()
        {
            cmbQuantity.Items.Clear();
            if (cmbProduct.SelectedItem == null) return;

            var selectedProduct = (ProductItem)cmbProduct.SelectedItem;

            using (var bd = new Context())
            {
                var product = bd.Products.FirstOrDefault(p => p.Article == selectedProduct.Article);
                if (product != null)
                {
                    var availableQuantity = bd.ProductBatches
                        .Where(b => b.ProductId == product.Id && b.Status == "active" && b.Quantity > 0 && b.ExpiryDate >= DateTime.UtcNow.Date)
                        .Sum(b => b.Quantity);

                    selectedProduct.Quantity = availableQuantity;
                }
            }

            int maxQuantity = selectedProduct.Quantity;
            if (maxQuantity == 0)
            {
                MessageBox.Show(Resources.InvalidShipment);
                btnShip.Enabled = false;
                return;
            }
            else
            {
                btnShip.Enabled = true;
            }
            int maxItemsToShow = Math.Min(maxQuantity, 20);
            for (int i = 1; i <= maxItemsToShow; i++)
            {
                cmbQuantity.Items.Add(i);
            }

            if (maxQuantity > 20)
            {
                cmbQuantity.Items.Add($"Другое до {maxQuantity}");
            }
        }

        private void cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateQuantityDropdown();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateClient())
                return;

            if (!ValidateSelection())
                return;

            if (!int.TryParse(cmbQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show(Resources.InvalidQuantity);
                return;
            }

            var selected = (ProductItem)cmbProduct.SelectedItem;

            if (quantity > selected.Quantity)
            {
                MessageBox.Show(Resources.InsufficientStock);
                return;
            }

            AddOrUpdateItem(selected, quantity);
            UpdateGrid();
            cmbQuantity.Text = "";
        }

        private bool ValidateSelection()
        {
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show(Resources.SelectProduct);
                return false;
            }
            return true;
        }

        private void AddOrUpdateItem(ProductItem selected, int quantity)
        {
            var currentClient = txtClient.Text.Trim();

            var existing = _items.FirstOrDefault(i => i.Article == selected.Article && i.Client == currentClient);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new ShipmentItemTemp
                {
                    Article = selected.Article,
                    Name = selected.Name,
                    Quantity = quantity,
                    Price = selected.PurchasePrice,
                    Client = currentClient
                });
            }
        }

        private void UpdateGrid()
        {
            dgvItems.DataSource = null;
            dgvItems.DataSource = _items;

            dgvItems.Columns[nameof(ShipmentItemTemp.Article)].HeaderText = Resources.Article;
            dgvItems.Columns[nameof(ShipmentItemTemp.Name)].HeaderText = Resources.Name;
            dgvItems.Columns[nameof(ShipmentItemTemp.Quantity)].HeaderText = Resources.Quantity;
            dgvItems.Columns[nameof(ShipmentItemTemp.Price)].HeaderText = Resources.Price;
            dgvItems.Columns[nameof(ShipmentItemTemp.Client)].HeaderText = Resources.Client;

            btnShip.Enabled = _items.Count > 0;
        }

        private async void BtnShip_Click(object sender, EventArgs e)
        {
            if (!ValidateItems())
                return;

            if (!EnsureContractorCheckAllowsSaving("Shipment", out var blockMessage))
            {
                MessageBox.Show(blockMessage);
                await ContractorCheckService.LogBlockedOperationAsync("Shipment", txtContractorInn.Text.Trim(), blockMessage);
                return;
            }

            foreach (var group in _items.GroupBy(i => i.Client))
            {
                var itemsForShipment = group.Select(i => (i.Article, i.Quantity)).ToList();

                var result = ShipmentService.ProcessShipmentWithResult(group.Key, itemsForShipment);
                if (!result.Success || result.ShipmentId == null)
                {
                    MessageBox.Show(Resources.ShipmentError);
                    return;
                }

                await ContractorCheckService.LinkToShipmentAsync(_contractorCheck?.CheckId, result.ShipmentId.Value);
                await WeatherService.LinkToShipmentAsync(_weatherCheck?.CheckId, result.ShipmentId.Value);
            }

            MessageBox.Show(Resources.ShipmentSuccess);
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateClient()
        {
            if (string.IsNullOrWhiteSpace(txtClient.Text))
            {
                MessageBox.Show(Resources.ShipmentNoClient);
                txtClient.Focus();
                return false;
            }

            var client = txtClient.Text.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(client, @"^[а-яА-ЯёЁa-zA-Z0-9\s\-\.]+$"))
            {
                MessageBox.Show(Resources.InvalidClientName);
                txtClient.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateItems()
        {
            if (_items.Count == 0)
            {
                MessageBox.Show(Resources.ShipmentNoItems);
                return false;
            }
            return true;
        }

        private async Task CheckContractorInnAsync()
        {
            var inn = txtContractorInn.Text.Trim();
            if (!ContractorCheckService.IsValidInn(inn))
            {
                MessageBox.Show(ContractorCheckService.InvalidInnMessage);
                return;
            }

            btnCheckInn.Enabled = false;
            try
            {
                var result = await ContractorCheckService.CheckAndSaveAsync(inn, "Shipment", CurrentUser.Id);
                _contractorCheck = result;
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message);
                    if (result.ErrorKind == "network")
                    {
                        btnCheckInn.Enabled = false;
                    }
                    else
                    {
                        btnCheckInn.Enabled = true;
                    }
                }
                else
                {
                    btnCheckInn.Enabled = true;
                }

                ShowContractorCheckResult(result);
            }
            finally
            {
                if (_contractorCheck?.ErrorKind != "network")
                    btnCheckInn.Enabled = true;
            }
        }

        private async Task GetWeatherAsync()
        {
            var city = txtDeliveryCity.Text.Trim();
            if (city.Length < 2)
            {
                MessageBox.Show(WeatherService.EnterCityMessage);
                return;
            }

            if (dtpDeliveryDate.Value.Date < DateTime.Today)
            {
                MessageBox.Show(WeatherService.DateInPastMessage);
                return;
            }

            btnGetWeather.Enabled = false;
            try
            {
                var result = await WeatherService.GetForecastAndSaveAsync(city, dtpDeliveryDate.Value.Date, CurrentUser.Id);
                _weatherCheck = result;
                ShowWeatherResult(result);
                if (!result.IsSuccess)
                    MessageBox.Show(result.ErrorMessage ?? WeatherService.ForecastUnavailableMessage);
            }
            catch (InvalidOperationException ex)
            {
                _weatherCheck = WeatherCheckResult.Error(ex.Message);
                ShowWeatherResult(_weatherCheck);
                MessageBox.Show(ex.Message);
            }
            catch
            {
                _weatherCheck = WeatherCheckResult.Error(WeatherService.ForecastUnavailableMessage);
                ShowWeatherResult(_weatherCheck);
                MessageBox.Show(WeatherService.ForecastUnavailableMessage);
            }
            finally
            {
                btnGetWeather.Enabled = true;
            }
        }

        private void ResetContractorCheck()
        {
            _contractorCheck = null;
            btnCheckInn.Enabled = true;
            lblContractorStatus.Text = ContractorCheckService.NotPerformedText;
            lblContractorStatus.ForeColor = Color.DimGray;
            lblContractorOrganization.Text = "Организация: —";
            lblContractorCheckedAt.Text = "Дата проверки: —";
        }

        private void ResetWeatherCheck()
        {
            _weatherCheck = null;
            lblWeatherResult.Text = "Прогноз не получен";
            lblWeatherResult.ForeColor = Color.DimGray;
        }

        private void ShowContractorCheckResult(ContractorCheckResult result)
        {
            if (!result.IsSuccess)
            {
                lblContractorStatus.Text = result.Message;
                lblContractorStatus.ForeColor = Color.DarkRed;
                lblContractorOrganization.Text = "Организация: —";
                lblContractorCheckedAt.Text = "Дата проверки: —";
                return;
            }

            lblContractorStatus.Text = result.ResultStatus == ContractorCheckService.StatusReliable
                ? ContractorCheckService.ReliableText
                : ContractorCheckService.BlacklistedText;
            lblContractorStatus.ForeColor = result.ResultStatus == ContractorCheckService.StatusReliable
                ? Color.DarkGreen
                : Color.DarkRed;
            lblContractorOrganization.Text = $"Организация: {result.OrganizationName}";
            lblContractorCheckedAt.Text = $"Дата проверки: {result.CheckedAt.ToLocalTime():dd.MM.yyyy HH:mm}";
        }

        private void ShowWeatherResult(WeatherCheckResult result)
        {
            if (!result.IsSuccess)
            {
                lblWeatherResult.Text = result.ErrorMessage ?? "Прогноз не получен";
                lblWeatherResult.ForeColor = Color.DarkRed;
                return;
            }

            lblWeatherResult.Text = result.RiskLevel == WeatherService.NormalRisk
                ? result.Recommendation
                : $"Внимание! {result.Recommendation}";
            lblWeatherResult.ForeColor = result.RiskLevel == WeatherService.NormalRisk ? Color.DarkGreen : Color.DarkOrange;
            if (result.RiskLevel == WeatherService.CriticalRisk)
                lblWeatherResult.ForeColor = Color.DarkRed;
        }

        private bool EnsureContractorCheckAllowsSaving(string documentType, out string message)
        {
            message = string.Empty;
            var currentInn = txtContractorInn.Text.Trim();

            if (_contractorCheck == null || string.IsNullOrWhiteSpace(currentInn) || _contractorCheck.Inn != currentInn)
            {
                message = ContractorCheckService.NeedCheckMessage;
                return false;
            }

            if (!_contractorCheck.IsSuccess)
            {
                message = ContractorCheckService.CheckErrorMessage;
                return false;
            }

            if (!_contractorCheck.IsReliable || _contractorCheck.ResultStatus == ContractorCheckService.StatusBlacklisted)
            {
                message = ContractorCheckService.BlockedBlackListMessage;
                return false;
            }

            return true;
        }
    }
}
