using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemProgramming_Module5_Part2
{
    public partial class MainForm : Form
    {
        private TabControl tabControl;
        private TabPage tabPageNumbers;
        private TabPage tabPageResumes;

        private TextBox textBoxNumbersFilePath;
        private Button buttonBrowseNumbersFile;
        private Button buttonAnalyzeNumbers;
        private TextBox textBoxNumbersResult;

        private TextBox textBoxResumesFolder;
        private Button buttonBrowseResumesFolder;
        private CheckedListBox checkedListBoxResumeOptions;
        private Button buttonAnalyzeResumes;
        private TextBox textBoxResumesResult;

        public MainForm()
        {
            InitializeComponent();
            SetupTabs();
            this.Text = "Домашнє завдання. Системне програмування. Модуль 5. Частина 2";
            this.Width = 800;
            this.Height = 600;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl() { Dock = DockStyle.Fill };
            this.tabPageNumbers = new TabPage("Аналіз чисел (PLINQ)");
            this.tabPageResumes = new TabPage("Аналіз резюме");
            this.tabControl.TabPages.Add(tabPageNumbers);
            this.tabControl.TabPages.Add(tabPageResumes);
            this.Controls.Add(tabControl);
        }

        private void SetupTabs()
        {
            SetupNumbersTab();
            SetupResumesTab();
        }

        private void SetupNumbersTab()
        {
            Label labelFile = new Label() { Text = "Файл з числами:", Left = 12, Top = 15, Width = 120 };
            textBoxNumbersFilePath = new TextBox() { Left = 12, Top = 40, Width = 500 };
            buttonBrowseNumbersFile = new Button() { Text = "Огляд...", Left = 520, Top = 38, Width = 80 };
            buttonBrowseNumbersFile.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                        textBoxNumbersFilePath.Text = ofd.FileName;
                }
            };

            buttonAnalyzeNumbers = new Button() { Text = "Проаналізувати", Left = 12, Top = 75, Width = 150 };
            buttonAnalyzeNumbers.Click += async (s, e) => await ButtonAnalyzeNumbers_Click();

            textBoxNumbersResult = new TextBox()
            {
                Left = 12, Top = 115, Width = 700, Height = 400,
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical
            };

            tabPageNumbers.Controls.Add(labelFile);
            tabPageNumbers.Controls.Add(textBoxNumbersFilePath);
            tabPageNumbers.Controls.Add(buttonBrowseNumbersFile);
            tabPageNumbers.Controls.Add(buttonAnalyzeNumbers);
            tabPageNumbers.Controls.Add(textBoxNumbersResult);
        }

        private async Task ButtonAnalyzeNumbers_Click()
        {
            string filePath = textBoxNumbersFilePath.Text;
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            buttonAnalyzeNumbers.Enabled = false;
            textBoxNumbersResult.Clear();
            textBoxNumbersResult.Text = "Виконується аналіз...\r\n";

            List<int> numbers = new List<int>();
            try
            {
                string content = await Task.Run(() => File.ReadAllText(filePath));
                numbers = content.Split(new[] { ' ', '\r', '\n', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => int.TryParse(s, out int n) ? n : (int?)null)
                                 .Where(n => n.HasValue)
                                 .Select(n => n.Value)
                                 .ToList();

                if (numbers.Count == 0)
                {
                    textBoxNumbersResult.Text = "Файл не містить цілих чисел.";
                    buttonAnalyzeNumbers.Enabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                textBoxNumbersResult.Text = $"Помилка читання файлу: {ex.Message}";
                buttonAnalyzeNumbers.Enabled = true;
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Загальна кількість чисел: {numbers.Count}");
            sb.AppendLine();

            int uniqueCount = await Task.Run(() => numbers.AsParallel().Distinct().Count());
            sb.AppendLine($"Завдання 1: Кількість унікальних значень = {uniqueCount}");

            int maxIncreasingLength = await Task.Run(() =>
            {
                if (numbers.Count == 0) return 0;
                int maxLen = 1;
                int currentLen = 1;
                for (int i = 1; i < numbers.Count; i++)
                {
                    if (numbers[i] > numbers[i - 1])
                    {
                        currentLen++;
                        if (currentLen > maxLen) maxLen = currentLen;
                    }
                    else
                    {
                        currentLen = 1;
                    }
                }
                return maxLen;
            });
            sb.AppendLine($"Завдання 2: Максимальна довжина зростаючої послідовності = {maxIncreasingLength}");

            int maxPositiveLength = await Task.Run(() =>
            {
                int maxLen = 0;
                int currentLen = 0;
                foreach (int num in numbers)
                {
                    if (num > 0)
                    {
                        currentLen++;
                        if (currentLen > maxLen) maxLen = currentLen;
                    }
                    else
                    {
                        currentLen = 0;
                    }
                }
                return maxLen;
            });
            sb.AppendLine($"Завдання 3: Максимальна довжина послідовності додатних чисел = {maxPositiveLength}");

            textBoxNumbersResult.Text = sb.ToString();
            buttonAnalyzeNumbers.Enabled = true;
        }

        private void SetupResumesTab()
        {
            Label labelFolder = new Label() { Text = "Папка з резюме:", Left = 12, Top = 15, Width = 120 };
            textBoxResumesFolder = new TextBox() { Left = 12, Top = 40, Width = 500 };
            buttonBrowseResumesFolder = new Button() { Text = "Огляд...", Left = 520, Top = 38, Width = 80 };
            buttonBrowseResumesFolder.Click += (s, e) =>
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                        textBoxResumesFolder.Text = fbd.SelectedPath;
                }
            };

            GroupBox groupBoxReports = new GroupBox()
            {
                Text = "Звіти", Left = 12, Top = 75, Width = 350, Height = 170
            };
            checkedListBoxResumeOptions = new CheckedListBox()
            {
                Left = 10, Top = 20, Width = 330, Height = 140, CheckOnClick = true
            };
            checkedListBoxResumeOptions.Items.AddRange(new object[] {
                "Найдосвідченіший кандидат",
                "Найбільш недосвідчений кандидат",
                "Кандидати з одного міста",
                "Кандидат із найнижчою вимогою до зарплати",
                "Кандидат із найвищими вимогами до зарплати"
            });
            for (int i = 0; i < checkedListBoxResumeOptions.Items.Count; i++)
                checkedListBoxResumeOptions.SetItemChecked(i, true);
            groupBoxReports.Controls.Add(checkedListBoxResumeOptions);

            buttonAnalyzeResumes = new Button() { Text = "Проаналізувати резюме", Left = 12, Top = 260, Width = 200 };
            buttonAnalyzeResumes.Click += async (s, e) => await ButtonAnalyzeResumes_Click();

            textBoxResumesResult = new TextBox()
            {
                Left = 12, Top = 300, Width = 700, Height = 220,
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical
            };

            tabPageResumes.Controls.Add(labelFolder);
            tabPageResumes.Controls.Add(textBoxResumesFolder);
            tabPageResumes.Controls.Add(buttonBrowseResumesFolder);
            tabPageResumes.Controls.Add(groupBoxReports);
            tabPageResumes.Controls.Add(buttonAnalyzeResumes);
            tabPageResumes.Controls.Add(textBoxResumesResult);
        }

        private async Task ButtonAnalyzeResumes_Click()
        {
            string folderPath = textBoxResumesFolder.Text;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Папка не існує.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedReports = new List<string>();
            foreach (var item in checkedListBoxResumeOptions.CheckedItems)
                selectedReports.Add(item.ToString());

            if (selectedReports.Count == 0)
            {
                MessageBox.Show("Виберіть хоча б один тип звіту.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonAnalyzeResumes.Enabled = false;
            textBoxResumesResult.Clear();
            textBoxResumesResult.Text = "Завантаження та аналіз резюме...\r\n";

            List<Resume> resumes = new List<Resume>();
            try
            {
                string[] files = Directory.GetFiles(folderPath, "*.txt");
                if (files.Length == 0)
                {
                    textBoxResumesResult.Text = "У папці немає файлів резюме (.txt).";
                    buttonAnalyzeResumes.Enabled = true;
                    return;
                }

                var loadTasks = files.Select(f => Task.Run(() => ParseResume(f)));
                Resume[] results = await Task.WhenAll(loadTasks);
                resumes = results.Where(r => r != null).ToList();

                if (resumes.Count == 0)
                {
                    textBoxResumesResult.Text = "Не вдалося завантажити жодного коректного резюме.";
                    buttonAnalyzeResumes.Enabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                textBoxResumesResult.Text = $"Помилка завантаження: {ex.Message}";
                buttonAnalyzeResumes.Enabled = true;
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Завантажено резюме: {resumes.Count}");
            sb.AppendLine();

            var reportTasks = new List<Task<string>>();

            if (selectedReports.Contains("Найдосвідченіший кандидат"))
            {
                reportTasks.Add(Task.Run(() =>
                {
                    var best = resumes.OrderByDescending(r => r.ExperienceYears).First();
                    return $"Найдосвідченіший кандидат: {best.Name}, досвід: {best.ExperienceYears} років";
                }));
            }
            if (selectedReports.Contains("Найбільш недосвідчений кандидат"))
            {
                reportTasks.Add(Task.Run(() =>
                {
                    var worst = resumes.OrderBy(r => r.ExperienceYears).First();
                    return $"Найбільш недосвідчений кандидат: {worst.Name}, досвід: {worst.ExperienceYears} років";
                }));
            }
            if (selectedReports.Contains("Кандидати з одного міста"))
            {
                reportTasks.Add(Task.Run(() =>
                {
                    var cityGroups = resumes.GroupBy(r => r.City)
                                            .Where(g => g.Count() > 1)
                                            .OrderByDescending(g => g.Count());
                    if (!cityGroups.Any()) return "Немає кандидатів з одного міста.";
                    StringBuilder citySb = new StringBuilder("Кандидати з одного міста:\r\n");
                    foreach (var group in cityGroups)
                    {
                        citySb.AppendLine($"  Місто: {group.Key} ({group.Count()} кандидатів)");
                        foreach (var r in group)
                            citySb.AppendLine($"    - {r.Name}");
                    }
                    return citySb.ToString().TrimEnd();
                }));
            }
            if (selectedReports.Contains("Кандидат із найнижчою вимогою до зарплати"))
            {
                reportTasks.Add(Task.Run(() =>
                {
                    var lowest = resumes.OrderBy(r => r.SalaryRequired).First();
                    return $"Кандидат із найнижчою вимогою: {lowest.Name}, зарплата: {lowest.SalaryRequired}";
                }));
            }
            if (selectedReports.Contains("Кандидат із найвищими вимогами до зарплати"))
            {
                reportTasks.Add(Task.Run(() =>
                {
                    var highest = resumes.OrderByDescending(r => r.SalaryRequired).First();
                    return $"Кандидат із найвищою вимогою: {highest.Name}, зарплата: {highest.SalaryRequired}";
                }));
            }

            string[] reportResults = await Task.WhenAll(reportTasks);
            foreach (string report in reportResults)
            {
                sb.AppendLine(report);
                sb.AppendLine();
            }

            textBoxResumesResult.Text = sb.ToString();
            buttonAnalyzeResumes.Enabled = true;
        }

        private Resume ParseResume(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length < 4) return null;

                string name = lines[0].Trim();
                if (!int.TryParse(lines[1].Trim(), out int experience)) return null;
                string city = lines[2].Trim();
                if (!decimal.TryParse(lines[3].Trim(), out decimal salary)) return null;

                return new Resume
                {
                    FilePath = filePath,
                    Name = name,
                    ExperienceYears = experience,
                    City = city,
                    SalaryRequired = salary
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class Resume
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public int ExperienceYears { get; set; }
        public string City { get; set; }
        public decimal SalaryRequired { get; set; }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}