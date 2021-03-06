﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Common
{
    using Aqua.TypeSystem;
    using Newtonsoft.Json;
    using Remote.Linq;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;

    public static class JsonFormatter
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings().ConfigureRemoteLinq();

        public static void Write(this Stream stream, object obj) => stream.WriteAsync(obj).Wait();

        public static T Read<T>(this Stream stream) => stream.ReadAsync<T>().Result;

        public static async Task WriteAsync(this Stream stream, object obj)
        {
            var typeInfo = new TypeInfo(obj.GetType(), false, false);

            await WriteInternalAsync(stream, typeInfo).ConfigureAwait(false);

            await WriteInternalAsync(stream, obj).ConfigureAwait(false);
        }

        private static async Task WriteInternalAsync(this Stream stream, object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented, _jsonSerializerSettings);

            byte[] data;
            using (var dataStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(dataStream, json);

                dataStream.Position = 0;
                data = dataStream.ToArray();
            }

            long size = data.LongLength;
            byte[] sizeData = BitConverter.GetBytes(size);

            await stream.WriteAsync(sizeData, 0, sizeData.Length).ConfigureAwait(false);
            await stream.WriteAsync(new[] { obj is Exception ? (byte)1 : (byte)0 }, 0, 1).ConfigureAwait(false);
            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
        }

        public static async Task<T> ReadAsync<T>(this Stream stream)
        {
            var typeInfo = await ReadInternalAsync<TypeInfo>(stream).ConfigureAwait(false);
            var type = typeInfo.Type;

            T obj = await ReadInternalAsync<T>(stream, type).ConfigureAwait(false);
            return obj;
        }

        public static async Task<T> ReadInternalAsync<T>(this Stream stream, Type type = null)
        {
            byte[] bytes = new byte[256];

            await stream.ReadAsync(bytes, 0, 8).ConfigureAwait(false);
            long size = BitConverter.ToInt64(bytes, 0);

            byte[] exceptionFlag = new byte[1];
            int i = await stream.ReadAsync(exceptionFlag, 0, 1).ConfigureAwait(false);
            if (i != 1)
            {
                throw new IOException("Unable to read expected error indication flag.");
            }

            object obj;
            using (var dataStream = new MemoryStream())
            {
                int count = 0;
                do
                {
                    int length = size - count < bytes.Length
                        ? (int)(size - count)
                        : bytes.Length;

                    i = await stream.ReadAsync(bytes, 0, length).ConfigureAwait(false);
                    count += i;

                    dataStream.Write(bytes, 0, i);
                }
                while (count < size);

                dataStream.Position = 0;

                var formatter = new BinaryFormatter();
                string json = (string)formatter.Deserialize(dataStream);

                obj = JsonConvert.DeserializeObject(json, type ?? typeof(T), _jsonSerializerSettings);
            }

            if (exceptionFlag[0] != 0)
            {
                var exception = (Exception)obj;
                throw exception;
            }

            return (T)obj;
        }
    }
}
