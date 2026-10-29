using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.VisualBasic;
using Npgsql;
using System.Runtime.InteropServices;

namespace BdManga
{
    public partial class Base : Form
    {

        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=Manga";

        private List<Manga> allManga;
        private List<Manga> favoriteManga;
        private User user;
        public Base(User user)
        {

            InitializeComponent();
            this.user = user;
            labelname.Content = user.Login;
            InitializeCatalog();
            LoadReadingHistoryFromDatabase();
            InitializeListView();
            LoadFavoriteMangaFromDatabase();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            cuiTabControl1.SelectedTab = cuiTabControl1.TabPages[2];

            this.ControlBox = true;
            this.MinimizeBox = true;
            this.MaximizeBox = true;
        }

        private void InitializeCatalog()
        {
            allManga = new List<Manga>();
            favoriteManga = new List<Manga>();

            treeViewGenres.Nodes.Add("Все жанры");
            treeViewGenres.Nodes.Add("Сёнэн");
            treeViewGenres.Nodes.Add("Фэнтези");

            AddMangaToList("Поднятие уровня в одиночку", "Na Honjaman Lebel-eob", "2018", "Боевик, Приключение, Сёнэн", "10 лет назад раскрылись врата в другой мир...", 9.8, "7580--i-alone-level-up");
            AddMangaToList("Ветролом", "windebeuleikeo", "2013", "Драма, Комедия, Повседневность", "Ветролом — драма о юных уличных гонщиках...", 9.8, "1773--wind-breaker");

            DisplayManga(allManga);

            treeViewGenres.AfterSelect += TreeViewGenres_AfterSelect;
            Searchtextbox.TextChanged += Searchtextbox_TextChanged;
            Searchtextbox.Enter += Searchtextbox_Enter;

            cuiTabControl1.SelectedIndexChanged += (s, e) =>
            {
                if (cuiTabControl1.SelectedTab == tabPage1)
                {
                    flowLayoutPanel2.Controls.Clear();
                    foreach (var manga in favoriteManga)
                    {
                        flowLayoutPanel2.Controls.Add(CreateMangaCard(manga));
                    }
                }
                else if (cuiTabControl1.SelectedTab == tabPage2)
                {
                    DisplayManga(allManga);
                }
                else if (cuiTabControl1.SelectedTab == tabPage3)
                {
                    LoadReadingHistoryFromDatabase();
                }
            };
        }


        private void AddMangaToList(string title, string author, string year, string genre, string description, double rating, string resourceName)
        {
            Manga manga = new Manga
            {
                Title = title,
                Author = author,
                Year = year,
                Genre = genre,
                Description = description,
                Rating = rating,
                ResourceName = resourceName,
                CurrentChapter = ""
            };
            allManga.Add(manga);

            var listViewItem = new ListViewItem(manga.Title);
            listViewItem.SubItems.Add(manga.CurrentChapter ?? "Не прочитано");
            listViewItem.Tag = manga;

            InsertMangaIntoDatabase(manga);
        }
        private bool MangaExists(string title)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM manga WHERE title = @Title", conn))
                {
                    cmd.Parameters.AddWithValue("Title", title);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }


        private void InsertMangaIntoDatabase(Manga manga)
        {
            try
            {
                if (MangaExists(manga.Title))
                {
                    return;
                }

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("INSERT INTO manga (title, author, year, genre, description, rating, resourcename, currentchapter) VALUES (@Title, @Author, @Year, @Genre, @Description, @Rating, @ResourceName, @CurrentChapter)", conn))
                    {
                        cmd.Parameters.AddWithValue("Title", manga.Title);
                        cmd.Parameters.AddWithValue("Author", manga.Author);
                        cmd.Parameters.AddWithValue("Year", manga.Year);
                        cmd.Parameters.AddWithValue("Genre", manga.Genre);
                        cmd.Parameters.AddWithValue("Description", manga.Description);
                        cmd.Parameters.AddWithValue("Rating", manga.Rating);
                        cmd.Parameters.AddWithValue("ResourceName", manga.ResourceName);
                        cmd.Parameters.AddWithValue("CurrentChapter", manga.CurrentChapter);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}");
            }
        }
        private void AddMangaToReadingHistory(Manga manga)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM readinghistory WHERE userlogin = @UserLogin AND mangatitle = @MangaTitle", conn))
                {
                    checkCmd.Parameters.AddWithValue("UserLogin", user.Login);
                    checkCmd.Parameters.AddWithValue("MangaTitle", manga.Title);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        return; 
                    }
                }

                using (var cmd = new NpgsqlCommand("INSERT INTO readinghistory (userlogin, mangatitle, currentchapter) VALUES (@UserLogin, @MangaTitle, @CurrentChapter)", conn))
                {
                    cmd.Parameters.AddWithValue("UserLogin", user.Login);
                    cmd.Parameters.AddWithValue("MangaTitle", manga.Title);
                    cmd.Parameters.AddWithValue("CurrentChapter", manga.CurrentChapter ?? "Не прочитано");
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void DisplayManga(List<Manga> mangaList)
        {
            flowLayoutPanelManga.Controls.Clear();
            foreach (var manga in mangaList)
            {
                flowLayoutPanelManga.Controls.Add(CreateMangaCard(manga));
            }
        }

        private Control CreateMangaCard(Manga manga)
        {
            Panel card = new Panel
            {
                Size = new Size(950, 150),
                BackColor = Color.FromArgb(40, 40, 40),
                Margin = new Padding(10)
            };

            PictureBox cover = new PictureBox
            {
                Size = new Size(100, 140),
                Location = new Point(10, 5),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            try
            {
                if (!string.IsNullOrEmpty(manga.ResourceName))
                {
                    byte[] imageBytes = (byte[])Properties.Resources.ResourceManager.GetObject(manga.ResourceName);
                    if (imageBytes != null)
                    {
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            cover.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        cover.BackColor = Color.Gray;
                    }
                }
                else
                {
                    cover.BackColor = Color.Gray;
                }
            }
            catch (Exception ex)
            {
                cover.BackColor = Color.Gray;
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
            }

            Label ratingLabel = new Label
            {
                Text = manga.Rating.ToString(),
                Location = new Point(10, 5),
                Size = new Size(30, 20),
                ForeColor = Color.Yellow,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Label titleLabel = new Label
            {
                Text = manga.Title,
                Location = new Point(120, 5),
                Size = new Size(400, 20),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            Label authorLabel = new Label
            {
                Text = $"{manga.Author}, {manga.Year}",
                Location = new Point(120, 30),
                Size = new Size(400, 20),
                ForeColor = Color.Gray,
                Font = new Font("Arial", 10)
            };

            Label descLabel = new Label
            {
                Text = manga.Description,
                Location = new Point(120, 55),
                Size = new Size(400, 40),
                ForeColor = Color.White,
                Font = new Font("Arial", 9),
                AutoEllipsis = true
            };

            Label genreLabel = new Label
            {
                Text = manga.Genre,
                Location = new Point(120, 100),
                Size = new Size(500, 20),
                ForeColor = Color.Gray,
                Font = new Font("Arial", 9)
            };

            Button viewButton = new Button
            {
                Text = "Открыть",
                Location = new Point(600, 5),
                Size = new Size(120, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10)
            };
            viewButton.Click += (s, e) =>
            {
                string url = $"https://mangalib.org/ru/manga/{manga.ResourceName}?ui=5087074";
                TabPage newTab = new TabPage(manga.Title);
                WebView2 webView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    Source = new Uri(url)
                };

                newTab.Controls.Add(webView);
                cuiTabControl1.TabPages.Add(newTab);
                cuiTabControl1.SelectedTab = newTab;

                AddMangaToReadingHistory(manga);

            };


            Button starButton = new Button
            {
                Text = favoriteManga.Contains(manga) ? "★" : "☆",
                Location = new Point(625, 60),
                Size = new Size(30, 30),
                BackColor = Color.Transparent,
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 14)
            };

            starButton.Click += (s, e) =>
            {
                if (favoriteManga.Contains(manga))
                {
                    favoriteManga.Remove(manga);
                    RemoveFavoriteMangaFromDatabase(manga);
                    starButton.Text = "☆";
                }
                else
                {
                    favoriteManga.Add(manga);
                    SaveFavoriteMangaToDatabase(manga);
                    starButton.Text = "★";
                }
            };


            card.Controls.Add(cover);
            card.Controls.Add(ratingLabel);
            card.Controls.Add(titleLabel);
            card.Controls.Add(authorLabel);
            card.Controls.Add(descLabel);
            card.Controls.Add(genreLabel);
            card.Controls.Add(viewButton);
            card.Controls.Add(starButton);
            return card;
        }

        private void TreeViewGenres_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedGenre = e.Node.Text;
            flowLayoutPanelManga.Controls.Clear();

            if (selectedGenre == "Все жанры")
            {
                DisplayManga(allManga);
            }
            else
            {
                var filteredManga = allManga.FindAll(m => m.Genre.Contains(selectedGenre));
                DisplayManga(filteredManga);
            }
        }

        private void Searchtextbox_TextChanged(object sender, EventArgs e)
        {
            string searchText = Searchtextbox.Text.ToLower();

            flowLayoutPanelManga.Controls.Clear();

            var filteredManga = allManga.FindAll(m =>
                m.Title.ToLower().Contains(searchText) ||
                m.Genre.ToLower().Contains(searchText) ||
                m.Author.ToLower().Contains(searchText));

            foreach (var manga in filteredManga)
            {
                flowLayoutPanelManga.Controls.Add(CreateMangaCard(manga));
            }
        }

        private void Searchtextbox_Enter(object sender, EventArgs e)
        {
            Searchtextbox.Text = "";
        }

        private void SetChapterButton_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = listView2.SelectedItems[0];
            Manga manga = (Manga)selectedItem.Tag;

            if (manga == null) return;

            string input = Interaction.InputBox($"Введите номер главы для \"{manga.Title}\":",
                                                "Установка главы",
                                                manga.CurrentChapter);

            if (!string.IsNullOrWhiteSpace(input))
            {
                manga.CurrentChapter = input;
                selectedItem.SubItems[1].Text = input;

                SaveChapterToDatabase(manga);
            }
        }


        private void InitializeListView()
        {
            listView2.View = View.Details;
            listView2.FullRowSelect = true;
            listView2.GridLines = true;

            listView2.Columns.Add("Название манги", 200);
            listView2.Columns.Add("Текущая глава", 100);
        }

        private void LoadReadingHistoryFromDatabase()
        {
            listView2.Items.Clear();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT mangatitle, currentchapter FROM \"readinghistory\" WHERE userlogin = @UserLogin", conn))
                {
                    cmd.Parameters.AddWithValue("@UserLogin", user.Login);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var manga = new Manga
                            {
                                Title = reader.GetString(0),
                                CurrentChapter = reader.IsDBNull(1) ? null : reader.GetString(1)
                            };

                            var listViewItem = new ListViewItem(manga.Title);
                            listViewItem.SubItems.Add(manga.CurrentChapter ?? "Не прочитано");
                            listViewItem.Tag = manga;
                            listView2.Items.Add(listViewItem);
                        }
                    }
                }
            }
        }







        private void ListView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var selectedItem = listView2.SelectedItems[0];
                if (selectedItem.Tag is Manga manga)
                {
                    string chapter = Microsoft.VisualBasic.Interaction.InputBox("Введите главу для манги:",
                                                                                 "Установка главы",
                                                                                 manga.CurrentChapter ?? "",
                                                                                 -1, -1);

                    if (!string.IsNullOrEmpty(chapter))
                    {
                        manga.CurrentChapter = chapter;
                        selectedItem.SubItems[1].Text = chapter;
                        MessageBox.Show($"Глава для '{manga.Title}' установлена на {chapter}");
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: объект манги не найден.");
                }
            }
        }
        private void SaveFavoriteMangaToDatabase(Manga manga)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO favorite_manga (userlogin, mangatitle) VALUES (@UserLogin, @MangaTitle)", conn))
                {
                    cmd.Parameters.AddWithValue("UserLogin", user.Login);
                    cmd.Parameters.AddWithValue("MangaTitle", manga.Title);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void RemoveFavoriteMangaFromDatabase(Manga manga)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM favorite_manga WHERE userlogin = @UserLogin AND mangatitle = @MangaTitle", conn))
                {
                    cmd.Parameters.AddWithValue("UserLogin", user.Login);
                    cmd.Parameters.AddWithValue("MangaTitle", manga.Title);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadFavoriteMangaFromDatabase()
        {
            favoriteManga.Clear();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT mangatitle FROM favorite_manga WHERE userlogin = @UserLogin", conn))
                {
                    cmd.Parameters.AddWithValue("@UserLogin", user.Login);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader.GetString(0);
                            var manga = allManga.Find(m => m.Title == title);
                            if (manga != null)
                            {
                                favoriteManga.Add(manga);
                            }
                        }
                    }
                }
            }
        }

        private void SaveChapterToDatabase(Manga manga)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE ReadingHistory SET CurrentChapter = @Chapter WHERE UserLogin = @UserLogin AND MangaTitle = @MangaTitle", conn))
                {
                    cmd.Parameters.AddWithValue("Chapter", manga.CurrentChapter);
                    cmd.Parameters.AddWithValue("UserLogin", user.Login);
                    cmd.Parameters.AddWithValue("MangaTitle", manga.Title);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void cuiButtonexit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }



    public class Manga
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public string ResourceName { get; set; }
        public string CurrentChapter { get; set; }
    }
}

