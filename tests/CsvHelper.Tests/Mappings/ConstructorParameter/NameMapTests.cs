﻿// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper.Tests.Mappings.ConstructorParameter
{
	[TestClass]
    public class NameMapTests
    {
		[TestMethod]
		public void Parameter_WithName_CreatesParameterMaps()
		{
			var map = new DefaultClassMap<Foo>();
			map.Parameter("id").Name("Id");
			map.Parameter("name").Name("Name");

			Assert.AreEqual(2, map.ParameterMaps.Count);
			Assert.AreEqual("Id", map.ParameterMaps[0].Data.Names[0]);
			Assert.AreEqual("Name", map.ParameterMaps[1].Data.Names[0]);
		}

		[TestMethod]
		public void Parameter_WithConstructorFunctionAndName_CreatesParameterMaps()
		{
			var map = new DefaultClassMap<Foo>();
			map.Parameter(() => ConfigurationFunctions.GetConstructor(new GetConstructorArgs { ClassType = typeof(Foo) }), "id").Name("Id");
			map.Parameter(() => ConfigurationFunctions.GetConstructor(new GetConstructorArgs { ClassType = typeof(Foo) }), "name").Name("Name");

			Assert.AreEqual(2, map.ParameterMaps.Count);
			Assert.AreEqual("Id", map.ParameterMaps[0].Data.Names[0]);
			Assert.AreEqual("Name", map.ParameterMaps[1].Data.Names[0]);
		}

		[TestMethod]
		public void Parameter_WithConstructorAndProperty_CreatesParameterMaps()
		{
			var constructor = ConfigurationFunctions.GetConstructor(new GetConstructorArgs { ClassType = typeof(Foo) });
			var parameters = constructor.GetParameters();

			var map = new DefaultClassMap<Foo>();
			map.Parameter(constructor, parameters[0]).Name("Id");
			map.Parameter(constructor, parameters[1]).Name("Name");

			Assert.AreEqual(2, map.ParameterMaps.Count);
			Assert.AreEqual("Id", map.ParameterMaps[0].Data.Names[0]);
			Assert.AreEqual("Name", map.ParameterMaps[1].Data.Names[0]);
		}

		[TestMethod]
		public void GetRecords_WithParameterMap_HasHeader_CreatesRecords()
		{
			var parser = new ParserMock
			{
				{ "Id", "Name" },
				{ "1", "one" },
			};
			using (var csv = new CsvReader(parser))
			{
				csv.Context.RegisterClassMap<FooMap>();
				var records = csv.GetRecords<Foo>().ToList();

				Assert.AreEqual(1, records.Count);
				Assert.AreEqual(1, records[0].Id);
				Assert.AreEqual("one", records[0].Name);
			}
		}

		[TestMethod]
		public void GetRecords_WithParameterMap_NoHeader_CreatesRecords()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false,
			};
			var parser = new ParserMock(config)
			{
				{ "1", "one" },
			};
			using (var csv = new CsvReader(parser))
			{
				csv.Context.RegisterClassMap<FooMap>();

				// Can't read using names when no header is present.
				Assert.ThrowsException<ReaderException>(() => csv.GetRecords<Foo>().ToList());
			}
		}

		[TestMethod]
		public void WriteRecords()
		{
			var records = new List<Foo>
			{
				new Foo(1, "one"),
			};

			using (var writer = new StringWriter())
			using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
			{
				csv.Context.RegisterClassMap<FooMap>();

				csv.WriteRecords(records);

				var expected = new StringBuilder();
				expected.Append("Id,Name\r\n");
				expected.Append("1,one\r\n");

				Assert.AreEqual(expected.ToString(), writer.ToString());
			}
		}

		private class Foo
		{
			public int Id { get; private set; }

			public string Name { get; private set; }

			public Foo(int id, string name)
			{
				Id = id;
				Name = name;
			}
		}

		private class FooMap : ClassMap<Foo>
		{
			public FooMap()
			{
				Map(m => m.Id);
				Map(m => m.Name);
				Parameter("id").Name("Id");
				Parameter("name").Name("Name");
			}
		}
	}
}
