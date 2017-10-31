using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	[TestFixture]
	public class NumberValidatorTests
	{
		[TestCase(1, 0, true)]
		public void TestValidInitializationOfNumberValidator(int precision, int scale, bool onlyPositive)
		{
			Assert.DoesNotThrow(() => new NumberValidator(precision, scale, onlyPositive));
		}

		[TestCase(-1, 2, true, TestName = "Negative precision with false Positive Flag")]
		[TestCase(-1, 2, false, TestName = "Negative precision with true positive flag")]
		[TestCase(0, 2, false, TestName = "Zero precision")]
		[TestCase(1, -2, false, TestName = "Negative scale with true positive flag")]
		[TestCase(2, 4, false, TestName = "precision < than scale")]
		public void TestNumberValidatorInitializationThrowsExceptions(int precision, int scale, bool onlyPositive)
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
		}

		[TestCase(2, 0, false, "-0", TestName = "Negetive Zero")]
		public void TestCheckingValidNumbers(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeTrue();
		}


		[TestCase(1, 0, true, "", TestName = "Empty number")]
		[TestCase(1, 0, true, "-", TestName = "Empty negative number")]
		[TestCase(3, 2, true, "a.sd", TestName = "Chars instead digits")]
		[TestCase(4, 2, false, "-v.nt", TestName = "Chars intsad digits with minus")]
		[TestCase(3, 2, true, "3.k", TestName = "Chars in decimal part")]
		[TestCase(4, 2, true, "2a.85", TestName = "Chars in int part")]
		[TestCase(5, 2, false, "-2a.85", TestName = "Chars in int part with minus")]
		public void TestCheckingInvalidNumbers(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeFalse();
		}

		[TestCase(17, 2, false, "0.0")]
		[TestCase(17, 2, false, "0")]
		[TestCase(12, 2, true, "+1.23")]
		[TestCase(7, 3, false, "-000.000")]
		public void TestCheckingValidPresicion(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			Assert.IsTrue(new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck));
		}

		[TestCase(3, 2, true, "00.00")]
		[TestCase(3, 2, true, null)]
		[TestCase(3, 2, false, null)]
		[TestCase(3, 2, false, ".05")]
		[TestCase(4, 3, true, ".-05")]
		[TestCase(4, 1, true, "-05.")]
		[TestCase(3, 1, false, "05.")]
		[TestCase(5, 3, false, "0.5.4")]
		[TestCase(6, 1, true, "-0 7.1")]
		[TestCase(3, 2, false, "-0.00")]
		[TestCase(3, 2, true, "+0.00")]
		[TestCase(3, 2, true, "+1.23")]
		[TestCase(4, 0, true, "00000")]
		[TestCase(4, 0, false, "-0000")]
		[TestCase(3, 2, false, "-1.23")]
		public void TestCheckingInvalidPrecision(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeFalse();
		}

		[TestCase(17, 2, true, "0.0")]
		[TestCase(17, 2, false, "-0")]
		[TestCase(4, 2, true, "+1.23")]
		[TestCase(2, 0, false, "-1")]
		public void TestCheckingValidScale(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeTrue();
		}

		[TestCase(4, 1, true, "+0.00")]
		[TestCase(5, 2, false, "-0.999")]
		[TestCase(4, 0, false, "-00.00")]
		public void TestCheckingInvalidScale(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeFalse();
		}

		[TestCase(5, 2, false, "-00.00", TestName = "false flag with negative number")]
		[TestCase(3, 1, true, "+0.0", TestName = "true flag with positive number")]
		[TestCase(3, 0, true, "+12", TestName = "true flag With positive number")]
		[TestCase(2, 0, false, "-0", TestName = "false flag With negative number")]
		public void TestCheckingValidPositiveFlag(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeTrue();
		}

		[TestCase(5, 2, true, "-00.00")]
		[TestCase(3, 1, true, "-0.0")]
		[TestCase(6, 3, true, "-12.345")]
		public void TestCheckingInvalidPositiveFlag(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeFalse();
		}

	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}