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

		[TestCase(-1, 2, true)]
		[TestCase(-1, 2, false)]
		public void TestNumberValidatorInitializationThrowsExceptions(int precision, int scale, bool onlyPositive)
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
		}

		[TestCase(17, 2, false, "0.0")]
		[TestCase(17, 2, false, "0")]
		[TestCase(12, 2, true, "+1.23")]
		[TestCase(7, 3, false, "-000.000")]
		public void TestCheckingValidPresicion(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			Assert.IsTrue(new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck));
		}

		[TestCase(1, 0, true, "")]
		[TestCase(3, 2, true, "a.sd")]
		[TestCase(4, 2, false, "-v.nt")]
		[TestCase(3, 2, true, "3.k")]
		[TestCase(4, 2, true, "2a.85")]
		[TestCase(5, 2, false, "-2a.85")]
		public void TestCheckingInvalidNumbers(int precision, int scale, bool onlyPositive, string valueToCheck)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(valueToCheck).Should().BeFalse();
		}


		[TestCase(3, 2, true, "00.00")]
		[TestCase(3, 2, false, "-0.00")]
		[TestCase(3, 2, true, "+0.00")]
		[TestCase(3, 2, true, "+1.23")]
		[TestCase(10, 0, true, "00000000000")]
		[TestCase(10, 0, false, "-0000000000")]
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

		[TestCase(5, 2, false, "-00.00")]
		[TestCase(3, 1, true, "+0.0")]
		[TestCase(3, 0, true, "+12")]
		[TestCase(2, 0, false, "-0")]
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