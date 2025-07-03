// Gra nonogram polega na tym by zamalowaæ odpowiednie kwadraty wed³ug pewnych wytycznych.
// Nale¿y zamalowaæ tyle ci¹gów kwadratów o takiej d³ugoœci jaka jest zapisana przy danej kolumnie/wierszu
// Ka¿dy ci¹g powinien byæ oddzielony conajmniej jednym kwadratem, ci¹gi nie mog¹ siê stykaæ.
// Np 5,4 - 5 kwadratów zamalowanych, jeden kwadrat przerwy, 4 kwadraty zamalowane.
// Program sam generuje nonogram losowo, pokazuje rozwi¹zanie, umo¿liwia zapisanie nonogramu w formie grafiki, któr¹ mo¿na wydrukowaæ.
// Mo¿e byæ wykorzystywane, by wspomóc logiczne myœlenie dziecka i oderwaæ go od ekranu.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NonogramAvalonia
{
    public partial class MainWindow : Window
    {
        private int[,] _solutionGrid = new int[10, 10];
        private Border[,] _cellRects = new Border[10, 10];

        public MainWindow()
        {
            InitializeComponent();
            GenerateButton.Click += (s, e) => GenerateGrid();
            ShowSolutionButton.Click += (s, e) => ShowSolution();
            ExportImageButton.Click += (s, e) => ExportAsPng();
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            var rnd = new Random();
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    _solutionGrid[i, j] = rnd.Next(2);

            for (int i = 0; i < 11; i++)
                MainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (int j = 0; j < 11; j++)
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            AddHints();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var cell = new Border
                    {
                        Width = 40,
                        Height = 40,
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetRow(cell, i + 1);
                    Grid.SetColumn(cell, j + 1);
                    MainGrid.Children.Add(cell);
                    _cellRects[i, j] = cell;
                }
            }
        }

        private void ShowSolution()
        {
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    _cellRects[i, j].Background = _solutionGrid[i, j] == 1
                        ? Brushes.Black
                        : Brushes.White;
        }

        private void AddHints()
        {
            for (int i = 0; i < 10; i++)
            {
                var hints = GetHintsForRow(i);
                var text = new TextBlock
                {
                    Text = string.Join(" ", hints),
                    FontSize = 14,
                    Margin = new Thickness(5, 0, 50, 0), // lewy, górny, prawy, dolny
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                Grid.SetRow(text, i + 1);
                Grid.SetColumn(text, 0);
                MainGrid.Children.Add(text);
            }

            for (int j = 0; j < 10; j++)
            {
                var hints = GetHintsForColumn(j);
                var text = new TextBlock
                {
                    Text = string.Join("\n", hints),
                    FontSize = 14,
                    Margin = new Thickness(5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                Grid.SetRow(text, 0);
                Grid.SetColumn(text, j + 1);
                MainGrid.Children.Add(text);
            }
        }

        private List<int> GetHintsForRow(int row)
        {
            var hints = new List<int>();
            int count = 0;
            for (int j = 0; j < 10; j++)
            {
                if (_solutionGrid[row, j] == 1) count++;
                else if (count > 0) { hints.Add(count); count = 0; }
            }
            if (count > 0) hints.Add(count);
            return hints.Count == 0 ? new List<int> { 0 } : hints;
        }

        private List<int> GetHintsForColumn(int col)
        {
            var hints = new List<int>();
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (_solutionGrid[i, col] == 1) count++;
                else if (count > 0) { hints.Add(count); count = 0; }
            }
            if (count > 0) hints.Add(count);
            return hints.Count == 0 ? new List<int> { 0 } : hints;
        }

        private void ExportAsPng()
        {
            int cellSize = 40;
            int padding = 120; 
            int gridSize = 10;
            int width = padding + gridSize * cellSize + 20;
            int height = padding + gridSize * cellSize + 20;


            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            var blackPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true
            };

            var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 16,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            for (int i = 0; i <= gridSize; i++)
            {
                canvas.DrawLine(padding, padding + i * cellSize, padding + gridSize * cellSize, padding + i * cellSize, blackPaint);
                canvas.DrawLine(padding + i * cellSize, padding, padding + i * cellSize, padding + gridSize * cellSize, blackPaint);
            }

            // Podpowiedzi do wierszy
            for (int i = 0; i < gridSize; i++)
            {
                var hints = GetHintsForRow(i);
                string text = string.Join(" ", hints);
                canvas.DrawText(text, padding - 20, padding + i * cellSize + cellSize / 2 + 5, textPaint);
            }


            for (int j = 0; j < gridSize; j++)
            {
                var hints = GetHintsForColumn(j);
                for (int k = 0; k < hints.Count; k++)
                {
                    string text = hints[k].ToString();
                    canvas.DrawText(text, padding + j * cellSize + cellSize / 2, padding - (hints.Count - k) * 15, textPaint);
                }
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "nonogram.png");
            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);

            _ = MessageBox($"Zapisano na pulpicie: nonogram.png");
        }

        private async Task MessageBox(string msg)
        {
            var dialog = new Window
            {
                Width = 300,
                Height = 100,
                Content = new TextBlock
                {
                    Text = msg,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                }
            };

            await dialog.ShowDialog(this);
        }
    }
}
