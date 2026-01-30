using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace calculatorForQA
{
    public partial class MainWindow : Window
    {
        private string _currentInput = "0";
        private double? _previousValue;
        private string? _pendingOperator;
        private bool _isNewInput = true;

        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        public MainWindow()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            DisplayText.Text = _currentInput;
        }

        private void OnNumberClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Content?.ToString() is not string value)
                return;

            if (_isNewInput)
            {
                _currentInput = value == "." ? "0." : value;
                _isNewInput = false;
            }
            else if (value == ".")
            {
                if (!_currentInput.Contains("."))
                    _currentInput += ".";
            }
            else
            {
                _currentInput = _currentInput == "0" ? value : _currentInput + value;
            }

            UpdateDisplay();
        }

        private void OnOperatorClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Content?.ToString() is not string op)
                return;

            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            if (_previousValue.HasValue && _pendingOperator != null && !_isNewInput)
            {
                _previousValue = Calculate(_previousValue.Value, current, _pendingOperator);
                _currentInput = FormatNumber(_previousValue.Value);
                UpdateDisplay();
            }
            else
            {
                _previousValue = current;
            }

            _pendingOperator = op;
            _isNewInput = true;
        }

        private void OnEqualsClick(object? sender, RoutedEventArgs e)
        {
            if (!_previousValue.HasValue || _pendingOperator == null)
                return;

            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            var result = Calculate(_previousValue.Value, current, _pendingOperator);
            _currentInput = FormatNumber(result);
            _previousValue = null;
            _pendingOperator = null;
            _isNewInput = true;

            UpdateDisplay();
        }

        private void OnClearClick(object? sender, RoutedEventArgs e)
        {
            _currentInput = "0";
            _previousValue = null;
            _pendingOperator = null;
            _isNewInput = true;
            UpdateDisplay();
        }

        private void OnToggleSignClick(object? sender, RoutedEventArgs e)
        {
            if (_currentInput == "0")
                return;

            _currentInput = _currentInput.StartsWith("-") 
                ? _currentInput[1..] 
                : "-" + _currentInput;

            UpdateDisplay();
        }

        private void OnPercentClick(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, _culture, out var current))
                return;

            current /= 100.0;
            _currentInput = FormatNumber(current);
            _isNewInput = true;
            UpdateDisplay();
        }

        private double Calculate(double left, double right, string op)
        {
            return op switch
            {
                "+" => left + right,
                "-" => left - right,
                "×" => left * right,
                "÷" => right == 0 ? double.NaN : left / right,
                _ => right
            };
        }

        private string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "Error";

            var text = value.ToString("G15", _culture);
            return text.Contains(".") ? text.TrimEnd('0').TrimEnd('.') : text;
        }
    }
}