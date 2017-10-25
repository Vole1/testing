using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Challenge
{
	[TestFixture]
	public class WordsStatistics_Tests
	{
		public static string Authors = "Каширин Михеев"; // "Egorov Shagalina"

		public virtual IWordsStatistics CreateStatistics()
		{
			// меняется на разные реализации при запуске exe
			return new WordsStatistics();
		}

		private IWordsStatistics statistics;

		[SetUp]
		public void SetUp()
		{
			statistics = CreateStatistics();
		}

        [Test]
		public void GetStatistics_ContainsManyItems_AfterAdditionOfDifferentWords()
		{
			statistics.AddWord("abc");
			statistics.AddWord("def");
			statistics.GetStatistics().Should().HaveCount(2);
            statistics.AddWord("gfd");
		    statistics.GetStatistics().Should().HaveCount(3);
        }

	    [Test, Timeout(30)]
	    public void AddWord_AddsManyWords()
	    {
	        for (var i = 0; i < 2000; i++)
	        {
	            statistics.AddWord(i + "fhd");
	        }
	        statistics.GetStatistics().Should().HaveCount(2000);
	    }

	    [Test, Timeout(65)]
	    public void AddWord_AddsTheSameWord()
	    {
	        for (var i = 0; i < 100000; i++)
	        {
	            statistics.AddWord(i%10 + "fhd");
	        }
	        statistics.GetStatistics().Should().Equal(Tuple.Create(10000, "0fhd"), Tuple.Create(10000, "1fhd"), Tuple.Create(10000, "2fhd"),
	            Tuple.Create(10000, "3fhd"), Tuple.Create(10000, "4fhd"), Tuple.Create(10000, "5fhd"),
                Tuple.Create(10000, "6fhd"), Tuple.Create(10000, "7fhd"),
                Tuple.Create(10000, "8fhd"), Tuple.Create(10000, "9fhd"));
	    }

        [Test]
	    public void AddWord_AddsNullStr()
	    {
	        Action action = () => statistics.AddWord(null);
	        action.ShouldThrow<ArgumentNullException>();
	    }

        [Test]
	    public void AddWord_AddsSpaceStr()
	    {
	        statistics.AddWord("   ");
	        statistics.GetStatistics().Should().BeEmpty();
        }

	    [Test]
	    public void AddWord_AddsWordLengthBetween5And10()
	    {
	        statistics.AddWord("aaaaaa");
	        statistics.GetStatistics().Should().Equal(Tuple.Create(1, "aaaaaa"));
	    }

	    [Test] 
	    public void AddWord_AddMess()
	    {
	        statistics.AddWord("qw");
	        statistics.AddWord("QW");
            statistics.GetStatistics().Should().Equal(Tuple.Create(2, "qw"));
	    }

        [Test]
        public void AddWord_AddsWordLength9()
        {
            statistics.AddWord("          a");
            statistics.GetStatistics().Should().Equal(Tuple.Create(1, "          "));
        }


        [Test]
        public void AddWord_KeyOrder()
        {
            statistics.AddWord("c");
            statistics.AddWord("b");
            statistics.AddWord("a");
            statistics.GetStatistics().Should().Equal(Tuple.Create(1, "a"), Tuple.Create(1, "b"), Tuple.Create(1, "c"));
        }

        [Test]
        public void AddWord_ValueOrder()
        {
            statistics.AddWord("a");
            statistics.AddWord("aa");
            statistics.AddWord("aaa");
            statistics.AddWord("aaa");
            statistics.AddWord("aaa");
            statistics.GetStatistics().Should().Equal(Tuple.Create(3, "aaa"), Tuple.Create(1, "a"), Tuple.Create(1, "aa"));
        }


        // Документация по FluentAssertions с примерами : https://github.com/fluentassertions/fluentassertions/wiki
    }
}