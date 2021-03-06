﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LightBDD.Core.Formatting.Values;

namespace LightBDD.Framework.Expectations.Implementation
{
    [DebuggerStepThrough]
    internal class AnyItemExpectation<TValue> : Expectation<IEnumerable<TValue>>
    {
        private readonly IExpectation<TValue> _itemExpectation;

        public AnyItemExpectation(IExpectation<TValue> itemExpectation)
        {
            _itemExpectation = itemExpectation;
        }

        public override ExpectationResult Verify(IEnumerable<TValue> collection, IValueFormattingService formattingService)
        {
            List<string> errors = new List<string>();
            int i = 0;
            foreach (var item in collection ?? Enumerable.Empty<TValue>())
            {
                var result = _itemExpectation.Verify(item, formattingService);
                if (result)
                    return ExpectationResult.Success;
                errors.Add($"[{i++}]: {result.Message}");
            }

            return FormatFailure(formattingService, $"got: '{formattingService.FormatValue(collection)}'", errors);
        }

        public override string Format(IValueFormattingService formattingService)
        {
            return "any item " + _itemExpectation.Format(formattingService);
        }
    }
}