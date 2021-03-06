﻿using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Extensions
{
    public static class DynamoDBExtensions
    {
        public static object ConvertTo(this Dictionary<string, AttributeValue> item, Type type)
        {
            var instance = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
            {
                if (item.ContainsKey(prop.Name))
                {
                    var value = GetValueRecursive(item[prop.Name], prop.PropertyType);

                    prop.SetValue(instance, value);
                }
            }

            return instance;
        }

        public static object ConvertTo(this KeyValuePair<string, AttributeValue> item, Type type)
        {
            var value = GetValueRecursive(item.Value, type);

            return value;
        }

        private static object GetValueRecursive(AttributeValue attr, Type type)
        {
            if (attr.NULL)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(attr.N))
            {
                if (type.IsEnum)
                {
                    return Enum.Parse(type, attr.N);
                }

                var realType = Nullable.GetUnderlyingType(type) != null ?
                    type.UnderlyingSystemType.GetGenericArguments()[0] :
                    type.UnderlyingSystemType;

                if (realType == typeof(decimal))
                {
                    return decimal.Parse(attr.N);
                }

                if (realType == typeof(long))
                {
                    return long.Parse(attr.N);
                }

                if (realType == typeof(double))
                {
                    return double.Parse(attr.N);
                }

                return int.Parse(attr.N);
            }

            if (attr.IsBOOLSet)
            {
                return attr.BOOL;
            }

            if (!string.IsNullOrWhiteSpace(attr.S))
            {
                if (type.IsEnum)
                {
                    return Enum.Parse(type, attr.S);
                }

                if (TimeSpan.TryParse(attr.S, out TimeSpan ts))
                {
                    return ts;
                }

                if (DateTime.TryParse(attr.S, out DateTime dt))
                {
                    return dt;
                }

                return attr.S;
            }

            if (attr.IsMSet)
            {
                return ConvertTo(attr.M, type);
            }

            if (attr.IsLSet)
            {
                var listType = type.GenericTypeArguments[0];
                var list = attr.L.Select(i => GetValueRecursive(i, listType)).ToList();
                var genericList = list.ToGenericEnumerable(listType);

                return genericList;
            }

            throw new InvalidOperationException("Could not map DynamoDB value.");
        }
    }
}
