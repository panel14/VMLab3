namespace VMLab3 {
    public class Programm
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Добро пожаловать в приложение. Выберите интеграл, который хотите вычислить методом трапеций:\n" +
                "(1): y = 2x + 3/√x\n" +
                "(2): y = 1/12x\u2074 + 1/3x - 1/60\n" +
                "(3): y = sin(x)/x\n" +
                "(4): y = sin(1/x)");
            int choise = Int16.Parse(Console.ReadLine()) - 1;

            Console.WriteLine("Укажите интервал интегрирования:");
            double[] section = Console.ReadLine().Split(' ').Select(Double.Parse).ToArray();

            Console.WriteLine("На заданном промежутке могут быть встречены точки разрыва функции.\n" +
                "Если это точки разрыва 1-ого рода, они могут быть устранены.\n" +
                "Выберите метод устранения точек разрыва:\n" +
                "(1): Замена средним от ближайших точек\n" +
                "(2): Расчёт левой и правой части интеграла относительно точки разрыва.");
            int fixMethod = Int16.Parse(Console.ReadLine()) - 1;

            Solver.Solve(choise, section, fixMethod);

            double?[] results = Solver.GetResults();
            double?[] errors = Solver.GetErrors();
            int[] iters = Solver.GetIters();

            Console.WriteLine("Резульаты работы метода трапеций:");

            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] != null && errors[i] != null)
                {
                    Console.WriteLine("Значение интеграла для {0} итераций: {1}. Погрешность: {2}", iters[i], results[i], errors[i]);
                    List<double> gaps = Solver.GetFGap();
                    if (gaps.Count != 0)
                    {
                        Console.WriteLine("Наденные точки разрыва первого рода:");
                        gaps.ForEach(delegate (double value)
                        {
                            Console.WriteLine(value);
                        });
                    }
                }

                else
                {
                    double secGap = Solver.GetSecGap();
                    Console.WriteLine("Была найдена точка разрыва 2-ого рода X = {0}, решить интеграл невозможно", secGap);
                    return;
                }
            }
        }
    }
}
