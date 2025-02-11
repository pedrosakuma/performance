﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CSharpBenchmark.JsonSerializer
{
    [GenericTypeArguments(typeof(LoginViewModel))]
    [GenericTypeArguments(typeof(Location))]
    [GenericTypeArguments(typeof(IndexViewModel))]
    [GenericTypeArguments(typeof(MyEventsListerViewModel))]
    [GenericTypeArguments(typeof(CollectionsOfPrimitives))]
    [MemoryDiagnoser]
    [BenchmarkCategory(Categories.JsonSerializer)]
    public class Json_ToStream<T>
    {
        private T value;

        private MemoryStream memoryStream;
        private StreamWriter streamWriter;

        private DataContractJsonSerializer dataContractJsonSerializer;
        private Newtonsoft.Json.JsonSerializer newtonSoftJsonSerializer;

        [GlobalSetup]
        public void Setup()
        {
            value = DataGenerator.Generate<T>();

            // the stream is pre-allocated, we don't want the benchmarks to include stream allocaton cost
            memoryStream = new MemoryStream(capacity: short.MaxValue);
            streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

            dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
            newtonSoftJsonSerializer = new Newtonsoft.Json.JsonSerializer();
        }

        [BenchmarkCategory(Categories.ThirdParty)]
        [Benchmark(Description = "Jil")]
        public void Jil_()
        {
            memoryStream.Position = 0;
            Jil.JSON.Serialize<T>(value, streamWriter, Jil.Options.ISO8601);
        }

        [BenchmarkCategory(Categories.Libraries, Categories.ThirdParty)]
        [Benchmark(Description = "JSON.NET")]
        public void JsonNet_()
        {
            memoryStream.Position = 0;
            newtonSoftJsonSerializer.Serialize(streamWriter, value);
        }

        [BenchmarkCategory(Categories.ThirdParty)]
        [Benchmark(Description = "Utf8Json")]
        public void Utf8Json_()
        {
            memoryStream.Position = 0;
            Utf8Json.JsonSerializer.Serialize(memoryStream, value);
        }

        [BenchmarkCategory(Categories.Libraries)]
        [Benchmark(Description = "DataContractJsonSerializer")]
        public void DataContractJsonSerializer_()
        {
            memoryStream.Position = 0;
            dataContractJsonSerializer.WriteObject(memoryStream, value);
        }

        [BenchmarkCategory(Categories.Libraries)]
        [Benchmark(Description = "SystemTextJson")]
        public void SystemTextJson_()
        {
            memoryStream.Position = 0;
            using (var writer = new System.Text.Json.Utf8JsonWriter(memoryStream))
                System.Text.Json.JsonSerializer.Serialize(writer, value);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            streamWriter.Dispose();
            memoryStream.Dispose();
        }
    }
}
