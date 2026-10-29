using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace Hotel
{
    public partial class MainForm : Form
    {
        private const string VirtualHost = "app.local";

        private string ConnectionString =>
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + Application.StartupPath + @"\Database1.mdf;Integrated Security=True";

        public MainForm()
        {
            InitializeComponent();
            Load += MainForm_Load;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();

            string wwwroot = Path.Combine(Application.StartupPath, "wwwroot");
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                VirtualHost, wwwroot, Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            Navigate("main");
        }

        private void Navigate(string page)
        {
            webView.CoreWebView2.Navigate($"https://{VirtualHost}/{page}.html");
        }

        private async void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            JObject message;
            try
            {
                message = JObject.Parse(e.TryGetWebMessageAsString());
            }
            catch
            {
                return;
            }

            string type = (string)message["type"];
            JObject payload = message["payload"] as JObject ?? new JObject();

            try
            {
                switch (type)
                {
                    case "navigate":
                        Navigate((string)payload["page"]);
                        break;
                    case "ready":
                        await OnPageReady();
                        break;
                    case "addGuest":
                        await AddGuest(payload);
                        break;
                    case "searchGuests":
                        await SearchGuests((string)payload["query"]);
                        break;
                    case "addBooking":
                        await AddBooking(payload);
                        break;
                    case "calcPrice":
                        await CalcPrice(payload);
                        break;
                    case "searchGuestsForBooking":
                        await SearchGuestsForBooking((string)payload["query"]);
                        break;
                    case "exportBookings":
                        ExportBookings();
                        break;
                    case "exportCurrentBooking":
                        ExportCurrentBooking(payload);
                        break;
                    case "serviceSelected":
                        await ServiceSelected((int)payload["serviceId"]);
                        break;
                    case "addOrder":
                        await AddOrder(payload);
                        break;
                    case "exportOrders":
                        ExportOrders();
                        break;
                    case "exportCurrentOrder":
                        ExportCurrentOrder(payload);
                        break;
                    case "markOrderDone":
                        await MarkOrderDone((int)payload["orderId"]);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task RunScript(string script)
        {
            await webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        private string CurrentPage()
        {
            var uri = webView.Source;
            if (uri == null) return "";
            return Path.GetFileNameWithoutExtension(uri.AbsolutePath);
        }

        private async System.Threading.Tasks.Task OnPageReady()
        {
            switch (CurrentPage())
            {
                case "addguest":
                    await PushGuests();
                    break;
                case "booking":
                    await PushBookingOptions();
                    await PushBookings();
                    await PushGuestSearchResults("");
                    break;
                case "service":
                    await PushServiceOptions();
                    await PushOrders();
                    break;
            }
        }

        // ───────────────────────── Гости ─────────────────────────

        private int NewGuestIndex()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Гости", connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar()) + 1;
                }
            }
        }

        private List<object> FetchGuests(string searchQuery)
        {
            var result = new List<object>();
            string query = "SELECT * FROM Гости";
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = "SELECT * FROM Гости WHERE ФИО LIKE @p OR номер_телефона LIKE @p";
            }

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    command.Parameters.AddWithValue("@p", "%" + searchQuery + "%");
                }
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new
                        {
                            fio = reader[1].ToString(),
                            passport = reader[2].ToString(),
                            phone = reader[3].ToString(),
                            regDate = Convert.ToDateTime(reader[4]).ToString("yyyy-MM-dd")
                        });
                    }
                }
            }
            return result;
        }

        private async System.Threading.Tasks.Task PushGuests()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(FetchGuests(null));
            await RunScript($"renderGuests({json})");
        }

        private async System.Threading.Tasks.Task AddGuest(JObject payload)
        {
            string fio = (string)payload["fio"];
            string passport = (string)payload["passport"];
            string phone = (string)payload["phone"];
            DateTime regDate = (DateTime)payload["regDate"];

            if (string.IsNullOrWhiteSpace(fio) || string.IsNullOrWhiteSpace(passport) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Введите данные!");
                return;
            }

            string query = "INSERT INTO Гости (гость_ID, ФИО, номер_и_серия_паспорта, номер_телефона, дата_регистрации) " +
                "VALUES (@id, @fio, @passport, @phone, @regDate);";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", NewGuestIndex());
                command.Parameters.AddWithValue("@fio", fio);
                command.Parameters.AddWithValue("@passport", passport);
                command.Parameters.AddWithValue("@phone", phone);
                command.Parameters.AddWithValue("@regDate", regDate);

                connection.Open();
                try
                {
                    if (command.ExecuteNonQuery() == 0)
                        MessageBox.Show("Неверно введённые данные!");
                    else
                        MessageBox.Show("Гость зарегистрирован!");
                }
                catch
                {
                    MessageBox.Show("Неверно введённые данные!");
                }
            }

            await RunScript("clearGuestForm()");
            await PushGuests();
        }

        private async System.Threading.Tasks.Task SearchGuests(string query)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(FetchGuests(query));
            await RunScript($"renderGuests({json})");
        }

        // ───────────────────────── Бронирование ─────────────────────────

        private int NewBookingIndex()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Бронирование", connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar()) + 1;
                }
            }
        }

        private async System.Threading.Tasks.Task PushBookingOptions()
        {
            var guests = new List<object>();
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand("SELECT * FROM Гости", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        guests.Add(new { id = reader.GetInt32(0), label = reader.GetInt32(0) + " " + reader[1] });
                    }
                }
            }

            var rooms = new List<object>();
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(
                "SELECT н.номер_ID, тн.Название_Типа FROM Номера н JOIN Типы_номеров тн ON тн.Тип_номера_ID = н.Тип_номера", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int roomId = reader.GetInt32(0);
                        rooms.Add(new { id = roomId, label = roomId + " номер, " + reader[1] });
                    }
                }
            }

            await RunScript($"renderGuestOptions({Newtonsoft.Json.JsonConvert.SerializeObject(guests)})");
            await RunScript($"renderRoomOptions({Newtonsoft.Json.JsonConvert.SerializeObject(rooms)})");
        }

        private async System.Threading.Tasks.Task PushBookings()
        {
            var rows = new List<object>();
            string query = "SELECT г.ФИО, б.номер, б.Дата_заезда, б.Дата_выезда, б.Дата_бронирования, б.Статус_Бронирования, б.Цена " +
                "FROM Бронирование б JOIN Гости г ON г.гость_ID = б.Гость";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new
                        {
                            guestName = reader[0].ToString(),
                            roomId = reader[1].ToString(),
                            dateIn = Convert.ToDateTime(reader[2]).ToString("yyyy-MM-dd"),
                            dateOut = Convert.ToDateTime(reader[3]).ToString("yyyy-MM-dd"),
                            bookedAt = Convert.ToDateTime(reader[4]).ToString("yyyy-MM-dd"),
                            status = reader[5].ToString(),
                            price = reader[6].ToString()
                        });
                    }
                }
            }

            await RunScript($"renderBookings({Newtonsoft.Json.JsonConvert.SerializeObject(rows)})");
        }

        private async System.Threading.Tasks.Task PushGuestSearchResults(string searchQuery)
        {
            var rows = new List<object>();
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(
                "SELECT гость_ID, ФИО, номер_телефона FROM Гости WHERE ФИО LIKE @p OR номер_телефона LIKE @p", connection))
            {
                command.Parameters.AddWithValue("@p", "%" + (searchQuery ?? "") + "%");
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new { id = reader.GetInt32(0), fio = reader[1].ToString(), phone = reader[2].ToString() });
                    }
                }
            }

            await RunScript($"renderGuestSearchResults({Newtonsoft.Json.JsonConvert.SerializeObject(rows)})");
        }

        private string GetAvailableRooms(DateTime dateStart, DateTime dateEnd)
        {
            string availableRooms = "";
            string query = @"
                SELECT номер_ID FROM Номера
                WHERE номер_ID NOT IN (
                    SELECT номер FROM Бронирование
                    WHERE (Дата_заезда <= @DateEnd AND Дата_выезда >= @DateStart)
                )";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DateStart", dateStart);
                command.Parameters.AddWithValue("@DateEnd", dateEnd);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        availableRooms += $"• {reader.GetInt32(0)}\n";
                    }
                }
            }

            return string.IsNullOrEmpty(availableRooms) ? "Нет свободных номеров на выбранные даты" : availableRooms;
        }

        private bool IsRoomAvailable(int roomId, DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT COUNT(*) FROM Бронирование
                WHERE номер = @Номер
                AND Статус_Бронирования NOT IN ('Отменено', 'Завершено')
                AND (
                    (Дата_заезда < @ДатаВыезда AND Дата_выезда > @ДатаЗаезда)
                    OR (@ДатаЗаезда >= Дата_заезда AND @ДатаЗаезда < Дата_выезда)
                    OR (@ДатаВыезда > Дата_заезда AND @ДатаВыезда <= Дата_выезда)
                )";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Номер", roomId);
                command.Parameters.AddWithValue("@ДатаЗаезда", startDate.Date);
                command.Parameters.AddWithValue("@ДатаВыезда", endDate.Date);
                connection.Open();
                return (int)command.ExecuteScalar() == 0;
            }
        }

        private async System.Threading.Tasks.Task AddBooking(JObject payload)
        {
            int? guestId = (int?)payload["guestId"];
            int? roomId = (int?)payload["roomId"];
            string status = (string)payload["status"];
            string priceText = (string)payload["price"];

            if (guestId == null || roomId == null || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(priceText))
            {
                MessageBox.Show("Введите данные!");
                return;
            }

            DateTime dateStart = (DateTime)payload["dateIn"];
            DateTime dateEnd = (DateTime)payload["dateOut"];

            if (dateStart > dateEnd)
            {
                MessageBox.Show("Дата заселения должна быть до даты выселения");
                return;
            }

            if (!IsRoomAvailable(roomId.Value, dateStart, dateEnd))
            {
                MessageBox.Show($"Номер занят на выбранные даты! Выберите другие даты или другой номер.\n\nСвободные номера на выбранные даты:\n{GetAvailableRooms(dateStart, dateEnd)}");
                return;
            }

            string query = @"INSERT INTO Бронирование (Гость, номер, Дата_заезда, Дата_выезда, Дата_бронирования, Статус_Бронирования, Цена)
                VALUES (@Гость, @Номер, @Заезд, @Выезд, @Бронь, @Статус, @Цена)";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Гость", guestId.Value);
                command.Parameters.AddWithValue("@Номер", roomId.Value);
                command.Parameters.AddWithValue("@Заезд", dateStart);
                command.Parameters.AddWithValue("@Выезд", dateEnd);
                command.Parameters.AddWithValue("@Бронь", DateTime.Now);
                command.Parameters.AddWithValue("@Статус", status);
                command.Parameters.AddWithValue("@Цена", Convert.ToDecimal(priceText));

                connection.Open();
                if (command.ExecuteNonQuery() > 0)
                    MessageBox.Show("Забронировано!");
                else
                    MessageBox.Show("Неверно введённые данные!");
            }

            await PushBookings();
        }

        private async System.Threading.Tasks.Task CalcPrice(JObject payload)
        {
            int? roomId = (int?)payload["roomId"];
            string dateInText = (string)payload["dateIn"];
            string dateOutText = (string)payload["dateOut"];

            if (roomId == null || string.IsNullOrWhiteSpace(dateInText) || string.IsNullOrWhiteSpace(dateOutText))
            {
                await RunScript("setPrice(0)");
                return;
            }

            DateTime dateStart = DateTime.Parse(dateInText);
            DateTime dateEnd = DateTime.Parse(dateOutText);

            if (dateEnd <= dateStart)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда!");
                await RunScript("setPrice(0)");
                return;
            }

            int nightsCount = (dateEnd.Date - dateStart.Date).Days;
            if (nightsCount == 0) nightsCount = 1;

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(
                "SELECT Цена FROM Типы_номеров JOIN Номера ON Типы_номеров.Тип_номера_ID = Номера.Тип_номера WHERE номер_ID = @roomId", connection))
            {
                command.Parameters.AddWithValue("@roomId", roomId.Value);
                connection.Open();
                int pricePerDay = Convert.ToInt32(command.ExecuteScalar());
                await RunScript($"setPrice({pricePerDay * nightsCount})");
            }
        }

        private async System.Threading.Tasks.Task SearchGuestsForBooking(string query)
        {
            await PushGuestSearchResults(query);
        }

        private void ExportBookings()
        {
            string query = @"
                SELECT б.Бронь_ID AS ID, г.ФИО AS Гость, н.номер_ID AS Номер, б.Дата_заезда AS Заезд,
                    б.Дата_выезда AS Выезд, б.Дата_бронирования AS Дата_бронирования, б.Статус_Бронирования AS Статус, б.Цена AS Цена
                FROM Бронирование б
                JOIN Гости г ON б.Гость = г.гость_ID
                JOIN Номера н ON б.номер = н.номер_ID";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var excelApp = new Excel.Application();
                    var workbook = excelApp.Workbooks.Add();
                    Excel.Worksheet worksheet = workbook.ActiveSheet;

                    worksheet.Cells[1, 1] = "Гость";
                    worksheet.Cells[1, 2] = "Номер";
                    worksheet.Cells[1, 3] = "Дата заезда";
                    worksheet.Cells[1, 4] = "Дата выезда";
                    worksheet.Cells[1, 5] = "Дата бронирования";
                    worksheet.Cells[1, 6] = "Статус";
                    worksheet.Cells[1, 7] = "Цена";

                    int row = 2;
                    while (reader.Read())
                    {
                        worksheet.Cells[row, 1] = reader["Гость"].ToString();
                        worksheet.Cells[row, 2] = reader["Номер"].ToString();
                        worksheet.Cells[row, 3] = Convert.ToDateTime(reader["Заезд"]).ToShortDateString();
                        worksheet.Cells[row, 4] = Convert.ToDateTime(reader["Выезд"]).ToShortDateString();
                        worksheet.Cells[row, 5] = Convert.ToDateTime(reader["Дата_бронирования"]).ToShortDateString();
                        worksheet.Cells[row, 6] = reader["Статус"].ToString();
                        worksheet.Cells[row, 7] = reader["Цена"].ToString();
                        row++;
                    }

                    int lastRow = row - 1;
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[lastRow, 7]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    dataRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                    Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 7]];
                    headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkSeaGreen);

                    worksheet.Columns.AutoFit();
                    excelApp.Visible = true;
                }
            }
        }

        private void ExportCurrentBooking(JObject payload)
        {
            int? guestId = (int?)payload["guestId"];
            int? roomId = (int?)payload["roomId"];

            if (guestId == null || roomId == null || string.IsNullOrWhiteSpace((string)payload["status"]) ||
                string.IsNullOrWhiteSpace((string)payload["price"]))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            string guestName = "";
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand("SELECT ФИО FROM Гости WHERE гость_ID = @id", connection))
            {
                command.Parameters.AddWithValue("@id", guestId.Value);
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) guestName = result.ToString();
            }

            var excelApp = new Excel.Application();
            var workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = workbook.ActiveSheet;

            string[] headers = { "Гость", "Номер", "Дата заезда", "Дата выезда", "Дата бронирования", "Статус", "Цена" };
            for (int i = 0; i < headers.Length; i++) worksheet.Cells[1, i + 1] = headers[i];

            worksheet.Cells[2, 1] = guestName;
            worksheet.Cells[2, 2] = roomId.Value;
            worksheet.Cells[2, 3] = ((string)payload["dateIn"]);
            worksheet.Cells[2, 4] = ((string)payload["dateOut"]);
            worksheet.Cells[2, 5] = DateTime.Now.ToShortDateString();
            worksheet.Cells[2, 6] = (string)payload["status"];
            worksheet.Cells[2, 7] = (string)payload["price"];

            Excel.Range dataRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, 7]];
            dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 7]].Interior.Color =
                System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkSeaGreen);

            worksheet.Columns.AutoFit();
            excelApp.Visible = true;
        }

        // ───────────────────────── Услуги ─────────────────────────

        private int NewOrderIndex()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Заказ_услуги", connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar()) + 1;
                }
            }
        }

        private async System.Threading.Tasks.Task PushServiceOptions()
        {
            var bookings = new List<object>();
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand("SELECT Бронь_ID, Дата_бронирования, Цена FROM Бронирование", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string date = Convert.ToDateTime(reader[1]).ToString("yyyy-MM-dd");
                        bookings.Add(new { id, label = id + " Бронь от " + date + " Цена: " + reader[2] });
                    }
                }
            }

            var services = new List<object>();
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand("SELECT * FROM Услуги", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        services.Add(new { id = reader.GetInt32(0), label = reader[0] + " " + reader[1] });
                    }
                }
            }

            await RunScript($"renderBookingOptions({Newtonsoft.Json.JsonConvert.SerializeObject(bookings)})");
            await RunScript($"renderServiceOptions({Newtonsoft.Json.JsonConvert.SerializeObject(services)})");
        }

        private async System.Threading.Tasks.Task PushOrders()
        {
            var rows = new List<object>();
            string query = "SELECT зу.Заказ_id, зу.Бронь, г.ФИО, у.Название_услуги, зу.Дата_Заказа, зу.цена, зу.Статус_заказа FROM Заказ_услуги зу " +
                "JOIN Услуги у ON у.Услуга_ID = зу.Услуга " +
                "JOIN Бронирование б ON зу.Бронь = б.Бронь_ID " +
                "JOIN Гости г ON б.Гость = г.гость_ID";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new
                        {
                            orderId = reader.GetInt32(0),
                            bookingId = reader[1].ToString(),
                            guest = reader[2].ToString(),
                            service = reader[3].ToString(),
                            orderDate = Convert.ToDateTime(reader[4]).ToString("yyyy-MM-dd"),
                            price = reader[5].ToString(),
                            status = reader[6].ToString()
                        });
                    }
                }
            }

            await RunScript($"renderOrders({Newtonsoft.Json.JsonConvert.SerializeObject(rows)})");
        }

        private async System.Threading.Tasks.Task ServiceSelected(int serviceId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand("SELECT Цена FROM Услуги WHERE Услуга_ID = @id", connection))
            {
                command.Parameters.AddWithValue("@id", serviceId);
                connection.Open();
                object result = command.ExecuteScalar();
                int price = result != null ? Convert.ToInt32(result) : 0;
                await RunScript($"setPrice({price})");
            }
        }

        private async System.Threading.Tasks.Task AddOrder(JObject payload)
        {
            string bookingIdText = (string)payload["bookingId"];
            string serviceIdText = (string)payload["serviceId"];
            string statusText = (string)payload["status"];
            string priceText = (string)payload["price"];

            if (string.IsNullOrWhiteSpace(bookingIdText) || string.IsNullOrWhiteSpace(serviceIdText) ||
                string.IsNullOrWhiteSpace(statusText) || string.IsNullOrWhiteSpace(priceText))
            {
                MessageBox.Show("Введите данные!");
                return;
            }

            string query = "INSERT INTO Заказ_услуги (Заказ_Id, Бронь, Услуга, Дата_Заказа, цена, Статус_заказа) " +
                "VALUES (@id, @booking, @service, @date, @price, @status);";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", NewOrderIndex());
                command.Parameters.AddWithValue("@booking", Convert.ToInt32(bookingIdText));
                command.Parameters.AddWithValue("@service", Convert.ToInt32(serviceIdText));
                command.Parameters.AddWithValue("@date", (DateTime)payload["orderDate"]);
                command.Parameters.AddWithValue("@price", priceText);
                command.Parameters.AddWithValue("@status", statusText);

                connection.Open();
                try
                {
                    if (command.ExecuteNonQuery() == 0)
                        MessageBox.Show("Неверно введённые данные!");
                    else
                        MessageBox.Show("Заказ зарегистрирован!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} | Неверно введённые данные!");
                }
            }

            await PushOrders();
        }

        private void ExportOrders()
        {
            string query = @"
                SELECT б.Бронь_ID AS Бронь, г.ФИО AS Гость, у.Название_услуги AS Услуга, зу.цена AS Цена,
                    зу.Дата_Заказа AS Дата, зу.Статус_заказа AS Статус
                FROM Заказ_услуги зу
                JOIN Бронирование б ON зу.Бронь = б.Бронь_ID
                JOIN Гости г ON б.Гость = г.гость_ID
                JOIN Услуги у ON зу.Услуга = у.Услуга_ID";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var excelApp = new Excel.Application();
                    var workbook = excelApp.Workbooks.Add();
                    Excel.Worksheet worksheet = workbook.ActiveSheet;

                    int col = 1;
                    worksheet.Cells[1, col++] = "Бронь";
                    worksheet.Cells[1, col++] = "Гость";
                    worksheet.Cells[1, col++] = "Услуга";
                    worksheet.Cells[1, col++] = "Цена";
                    worksheet.Cells[1, col++] = "Дата заказа";
                    worksheet.Cells[1, col++] = "Статус";

                    int row = 2;
                    while (reader.Read())
                    {
                        col = 1;
                        worksheet.Cells[row, col++] = reader["Бронь"].ToString();
                        worksheet.Cells[row, col++] = reader["Гость"].ToString();
                        worksheet.Cells[row, col++] = reader["Услуга"].ToString();
                        worksheet.Cells[row, col++] = reader["Цена"].ToString();
                        worksheet.Cells[row, col++] = Convert.ToDateTime(reader["Дата"]).ToShortDateString();
                        worksheet.Cells[row, col++] = reader["Статус"].ToString();
                        row++;
                    }

                    int lastRow = row - 1;
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[lastRow, 6]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    dataRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                    Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 6]];
                    headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkSeaGreen);

                    worksheet.Columns.AutoFit();
                    excelApp.Visible = true;
                }
            }
        }

        private void ExportCurrentOrder(JObject payload)
        {
            string bookingId = (string)payload["bookingId"];
            string bookingLabel = (string)payload["bookingLabel"];
            string serviceLabel = (string)payload["serviceLabel"];
            string status = (string)payload["status"];
            string price = (string)payload["price"];

            if (string.IsNullOrWhiteSpace(bookingId) || string.IsNullOrWhiteSpace(serviceLabel) ||
                string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(price))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            string guestName = "";
            int bookingIdNum = Convert.ToInt32(bookingId);
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(
                "SELECT г.ФИО FROM Бронирование б JOIN Гости г ON б.Гость = г.гость_ID WHERE б.Бронь_ID = @id", connection))
            {
                command.Parameters.AddWithValue("@id", bookingIdNum);
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) guestName = result.ToString();
            }

            var excelApp = new Excel.Application();
            var workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = workbook.ActiveSheet;

            string[] headers = { "Бронь", "Гость", "Услуга", "Дата заказа", "Цена", "Статус услуги" };
            for (int i = 0; i < headers.Length; i++) worksheet.Cells[1, i + 1] = headers[i];

            worksheet.Cells[2, 1] = bookingId;
            worksheet.Cells[2, 2] = guestName;
            worksheet.Cells[2, 3] = serviceLabel;
            worksheet.Cells[2, 4] = (string)payload["orderDate"];
            worksheet.Cells[2, 5] = price;
            worksheet.Cells[2, 6] = status;

            Excel.Range dataRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, 6]];
            dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 6]].Interior.Color =
                System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkSeaGreen);

            worksheet.Columns.AutoFit();
            excelApp.Visible = true;
        }

        private async System.Threading.Tasks.Task MarkOrderDone(int orderId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand("UPDATE Заказ_услуги SET Статус_заказа = N'Выполнена' WHERE Заказ_id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", orderId);
                connection.Open();
                command.ExecuteNonQuery();
            }

            await PushOrders();
        }
    }
}
