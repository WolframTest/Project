using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prognoz
{
    public partial class Form1 : Form
    {
        //01
        private List<double> X;    // выборка с просмотрами
        private List<double> Y;    // выборка с переходами
        private List<double> X1;    // выборка со средними просмотрами
        private List<double> Y1;    // выборка со средними переходами
        private int N;                // кол-во элементов
        private int N1;               // кол-во элементов в выборке средних
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                X = new List<double>();    // создаем экземпляр класса, выделяем память
                Y = new List<double>();
                X1 = new List<double>();
                Y1 = new List<double>();
                openFileDialog1.FileName = "";
                openFileDialog1.ShowDialog();    // появление окна выбора файлов
                string FileName = openFileDialog1.FileName;    // запоминание имени файлй
                FileStream stream = File.Open(FileName, FileMode.Open, FileAccess.Read);     // файловый поток(не шарю)
                if (stream != null)
                {
                    StreamReader reader = new StreamReader(stream, Encoding.Default);     // ридер файла(не шарю)
                    for (int i = 0; !reader.EndOfStream; i++)      // цикл пока не достигнут конец файла
                    {
                        string str = reader.ReadLine();      // считывание строки с файла
                        X1.Add(double.Parse(str));
                    }
                    stream.Close();    // закрытие потока
                }
                // далее тоже самое для 2-го файла
                openFileDialog1.FileName = "";
                openFileDialog1.ShowDialog();
                FileName = openFileDialog1.FileName;
                stream = File.Open(FileName, FileMode.Open, FileAccess.Read);
                if (stream != null)
                {
                    StreamReader reader = new StreamReader(stream, Encoding.Default);
                    for (int i = 0; !reader.EndOfStream; i++)
                    {
                        string str = reader.ReadLine();
                        Y1.Add(double.Parse(str));
                    }
                    stream.Close();
                }
                N1 = X1.Count;    // сохранение кол-ва

                KorrPole();     // построение корелляционного поля
                srednekv();         // построение многочлена
            }
            catch
            {
                MessageBox.Show("Ошибка. Файл не загружен!");
            }
        }

        private void KorrPole()
        {
            foreach (var s in chart1.Series)
                s.Points.Clear();                 // очистка серий
            double sum = 0;
            for (int i = 0; i < N1; i++)             // синхронная сортировка пузырьком по перой выборке с сохранением соответствий
            {
                for (int j = N1 - 1; j > i; j--)
                {
                    if (X1[j] < X1[j - 1])
                    {
                        double tmp1 = X1[j];
                        X1[j] = X1[j - 1];
                        X1[j - 1] = tmp1;
                        double tmp2 = Y1[j];
                        Y1[j] = Y1[j - 1];
                        Y1[j - 1] = tmp2;
                    }
                }
            }
            for (int i = 0; i < 200; i++)           // вычисление средних в каждой группе
            {
                for (int j = 0; j < N1 / 200; j++)
                {
                    sum += X1[N1 / 200 * i + j];
                }
                sum /= (N1 / 200);
                X.Add(sum);
                sum = 0;
            }
            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < N1 / 200; j++)
                {
                    sum += Y1[N1 / 200 * i + j];
                }
                sum /= (N1 / 200);
                Y.Add(sum);
                sum = 0;
            }
            N = X.Count;
            for (int i = 0; i < N; i++)
            {
                chart1.Series[0].Points.AddXY(X[i], Y[i]);              // нанесение точек график
            }
        }

        public static double[] Gauss(double[,] A, double[] B, int m)         // метод решения системы уравнений методом гаусса
        {
            int n = m - 1;
            double h;
            double[] Res = new double[m];
            for (int i = 0; i <= n - 1; i++)
            {
                for (int j = i + 1; j <= n; j++)
                {
                    A[j, i] = -(A[j, i]) / (A[i, i]);
                    for (int k = i + 1; k <= n; k++)
                    {
                        A[j, k] = A[j, k] + A[j, i] * A[i, k];
                    }
                    B[j] = B[j] + A[j, i] * B[i];
                }
            }
            Res[n] = B[n] / A[n, n];
            for (int i = n - 1; i >= 0; i--)
            {
                h = B[i];
                for (int j = i + 1; j <= n; j++)
                {
                    h = h - Res[j] * A[i, j];
                }
                Res[i] = h / A[i, i];
            }
            return Res;
        }
        public void srednekv()                 // метод построения многочлена
        {
            int m = 10;
            List<double> ListC = new List<double>();
            for (int j = 0; j <= 2 * m; j++)               // вычисление коэффициентов в матрице Грамма
            {
                double sum = 0;
                for (int i = 0; i < N; i++)
                {
                    sum += Math.Pow(X[i], j);
                }
                ListC.Add(sum);
            }

            List<double> ListD = new List<double>();       // вычисление свободных членов
            for (int k = 0; k <= m; k++)
            {
                double sum = 0;
                for (int i = 0; i < N; i++)
                {
                    sum += Math.Pow(X[i], k) * (Y[i]);
                }
                ListD.Add(sum);
            }

            double[,] Det = new double[m + 1, m + 1];         // формирование расширенной матрицы грамма
            double[] Det0 = new double[m + 1];
            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    Det[i, j] = ListC[j + i];
                }
                Det0[i] = ListD[i];
            }
            double[] solution = Gauss(Det, Det0, m + 1);         // нахождение решения системы - коэффициентов
            double res = 0;
            chart1.Series[1].Color = Color.Black;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    res += solution[j] * Math.Pow(X[i], j);
                }
                chart1.Series[1].Points.AddXY(X[i], res);          // построение графика многочлена
                res = 0;
            }
            string str = "";
            foreach(var v in solution)
            {
                str += v.ToString() + ";";        // вывод коэффициентов с разделительным символом ; в текстовое поле для дальнейшего копирования
            }
            str = str.Remove(str.Length - 1, 1);      // удаление символа ; в конце
            textBox1.Text = str;
        }
    }
}
