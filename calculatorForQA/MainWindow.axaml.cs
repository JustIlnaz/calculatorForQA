using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace calculatorForQA
{
    public partial class MainWindow : Window
    {
        // Состояние калькулятора
        private string _currentInput = "0";           // Текущее число на экране
        private double? _previousValue;               // Предыдущее число для операции
        private string? _pendingOperator;             // Ожидающая операция (+, -, ×, ÷)
        private bool _isNewInput = true;              // Признак ввода нового числа

        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        // Поддерживаемые операции
        private const string OperatorAdd = "+";
        private const string OperatorSubtract = "-";
        private const string OperatorMultiply = "×";
        private const string OperatorDivide = "÷";
        private const string OperatorDecimal = ".";

        public MainWindow()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            DisplayText.Text = _currentInput;
        }

        // Нажата цифра или точка (0-9, .)
        private void OnNumberClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Content?.ToString() is not string value)
                return;

            // Если начинаем новый ввод - заменяем текущее значение
            if (_isNewInput)
            {
                _currentInput = value == OperatorDecimal ? "0." : value;
                _isNewInput = false;
            }
            // Обработка точки - добавляем только если её нет
            else if (value == OperatorDecimal)
            {
                if (!_currentInput.Contains(OperatorDecimal))
                    _currentInput += OperatorDecimal;
            }
            // Добавляем цифру, заменяя ноль в начале
            else
            {
                _currentInput = _currentInput == "0" ? value : _currentInput + value;
            }

            UpdateDisplay();
        }

        // Нажата операция (+, -, ×, ÷)
        private void OnOperatorClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Content?.ToString() is not string op)
                return;

            // Преобразуем текущий ввод в число
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Если уже есть ожидающая операция - вычисляем результат
            // Это обеспечивает цепочку операций: 2 + 3 + 4 = 9
            if (_previousValue.HasValue && _pendingOperator != null && !_isNewInput)
            {
                _previousValue = Calculate(_previousValue.Value, current, _pendingOperator);
                _currentInput = FormatNumber(_previousValue.Value);
                UpdateDisplay();
            }
            else
            {
                // Иначе просто сохраняем текущее число
                _previousValue = current;
            }

            _pendingOperator = op;
            _isNewInput = true;
        }

        // Нажата кнопка "=" - выполняем финальный расчёт
        private void OnEqualsClick(object? sender, RoutedEventArgs e)
        {
            // Проверяем, что есть предыдущее значение и операция
            if (!_previousValue.HasValue || _pendingOperator == null)
                return;

            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Вычисляем результат и отображаем его
            var result = Calculate(_previousValue.Value, current, _pendingOperator);
            _currentInput = FormatNumber(result);

            // Очищаем операцию для следующего вычисления
            _previousValue = null;
            _pendingOperator = null;
            _isNewInput = true;

            UpdateDisplay();
        }

        // Нажата кнопка "AC" - полная очистка
        private void OnClearClick(object? sender, RoutedEventArgs e)
        {
            _currentInput = "0";
            _previousValue = null;
            _pendingOperator = null;
            _isNewInput = true;
            UpdateDisplay();
        }

        // Нажата кнопка "+/-" - переключение знака числа
        private void OnToggleSignClick(object? sender, RoutedEventArgs e)
        {
            if (_currentInput == "0")
                return;

            _currentInput = _currentInput.StartsWith("-") 
                ? _currentInput[1..]             // Удаляем минус
                : "-" + _currentInput;            // Добавляем минус

            UpdateDisplay();
        }

        // Нажата кнопка "%" - преобразование в проценты
        private void OnPercentClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Делим на 100 для получения процентов
            current /= 100.0;
            _currentInput = FormatNumber(current);
            _isNewInput = true;
            UpdateDisplay();
        }

        // Выполняет арифметическую операцию
        private double Calculate(double left, double right, string op)
        {
            return op switch
            {
                OperatorAdd => left + right,
                OperatorSubtract => left - right,
                OperatorMultiply => left * right,
                OperatorDivide => right == 0 ? double.NaN : left / right,  // Защита от деления на ноль
                _ => right  // По умолчанию возвращаем второе число
            };
        }

        // Нажата кнопка "√x" - квадратный корень
        private void OnSquareRootClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Проверяем, что число не отрицательное (нельзя извлечь корень из отрицательного)
            if (current < 0)
            {
                _currentInput = "Error";
                UpdateDisplay();
                return;
            }

            // Вычисляем квадратный корень
            var result = Math.Sqrt(current);
            _currentInput = FormatNumber(result);
            _isNewInput = true;
            UpdateDisplay();
        }

        // Нажата кнопка "x²" - возведение в квадрат
        private void OnSquareClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Возводим число в квадрат
            var result = current * current;
            _currentInput = FormatNumber(result);
            _isNewInput = true;
            UpdateDisplay();
        }

        // Нажата кнопка "∛x" - кубический корень
        private void OnCubeRootClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Вычисляем кубический корень (работает и для отрицательных чисел)
            var result = Math.Cbrt(current);
            _currentInput = FormatNumber(result);
            _isNewInput = true;
            UpdateDisplay();
        }

        // Нажата кнопка "1/x" - обратное число
        private void OnReciprocalClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Проверяем деление на ноль
            if (current == 0)
            {
                _currentInput = "Error";
                UpdateDisplay();
                return;
            }

            // Вычисляем обратное число (1 / x)
            var result = 1.0 / current;
            _currentInput = FormatNumber(result);
            _isNewInput = true;
            UpdateDisplay();
        }

        // Нажата кнопка "ln" - натуральный логарифм
        private void OnNaturalLogClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            // Проверяем, что число положительное (логарифм только для положительных чисел)
            if (current <= 0)
            {
                _currentInput = "Error";
                UpdateDisplay();
                return;
            }

            // Вычисляем натуральный логарифм (ln = log по основанию e)
            var result = Math.Log(current);
            _currentInput = FormatNumber(result);
            _isNewInput = true;
            UpdateDisplay();
        }

        // Форматирует число для отображения (убирает лишние нули и точку)
        private string FormatNumber(double value)
        {
            // Обработка ошибок (деление на ноль, переполнение и т.д.)
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "Error";

            // Преобразуем в строку с максимальной точностью
            var text = value.ToString("G15", _culture);

            // Удаляем лишние нули после точки (например 1.50 → 1.5) и саму точку (1.0 → 1)
            return text.Contains(".") ? text.TrimEnd('0').TrimEnd('.') : text;
        }
    }
}